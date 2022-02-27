using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platformer : MonoBehaviour
{
    [Header("Ground Movement")]
    public float groundMaxXSpeed = 12f;
    public float groundMaxXAcceleration = 50;
    public float groundMaxXDeacceleration = 100;
    float groundXSpeed = 0;

    [Header("Air Movement")]
    public float airMaxXControlSpeed = 12f; //max speed with wasd
    public float airMaxXSpeed = 36f;        //max speed from being pushed etc.
    public float airMaxXAcceleration = 50;
    public float airMaxXDeacceleration = 100;
    float airXSpeed = 0;
    [Header("Jump")]
    public float jumpSpeed = 25f;
    public float gravity = 80;
    public float aimedFallSpeed = 200f;
    public bool useOverideFall = false;

    [Header("Dash")]
    public float dashVelocity = 25;
    [Range(0,3)] public int maxDashes = 1;
    public int dashes = 0;

    //Maybe have a cooldown of very short

    //public float speed = 4f;
    Vector2 deltaPos = Vector2.zero;
    Vector2 direction = Vector2.down;

    [Header("Jump Magnitude")]
    public float jumpMagCoeficient = 10;
    public AnimationCurve jumpMagCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    float jumpMag;

    Vector2 velocity;
    Vector2 normal = Vector2.up;

    private bool touchingGround;
    public float jumpForgivenessTime = 0.1f;
    Vector2 lastNormal = Vector2.up;
    bool canJump
    {
        get
        {
            return Mathf.Max(Time.unscaledTime - timeLeftGround,0) <= jumpForgivenessTime;
        }
    }

    [Header("Constraint")]
    public LayerMask collideLayers;
    float skinWidth
    {
        get
        {
            return Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) * 0.1f + 0.01f;
        }
    }
    public bool snappingAllowed = true;
    float snapMaxDistance = 0.075f;
    Vector2 snapDeltaPos;
    private Vector2[] contraintDirections = new Vector2[4] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

    public List<CollisionData> collisions = new List<CollisionData>();


    private Animator anim;
    private CircleCollider2D cCollider;
    private SpriteRenderer rend;


    void Awake()
    {
        anim = GetComponent<Animator>();
        cCollider = GetComponent<CircleCollider2D>();
        rend = GetComponent<SpriteRenderer>();
    }

    void SetAnimator(bool jump)
    {
        if (deltaPos.magnitude > 0.1f)
        {
            direction = deltaPos;
        }
        //Make sure to use blend trees or else flipping sprite rend doesnt work.
        anim.SetFloat("y", direction.y);
        anim.SetFloat("speed", deltaPos.magnitude);
        anim.SetBool("onGround", touchingGround);

        if (direction.x > 0.1)
            rend.flipX = false;
        if (direction.x < -0.1)
            rend.flipX = true;
    }
    bool[] sides = new bool[4] { false, false, false, false };
    public Vector2 Constrained(Vector2 deltaPos)
    {
        LayerMask mask = collideLayers;
        Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
        float constraintDistance = cCollider.radius * transform.localScale.y + skinWidth;
        sides = new bool[4] { false, false, false, false };
        for (int i = 0; i < 4; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, contraintDirections[i], constraintDistance, mask);
            Debug.DrawRay(transform.position + collisionOffset, new Vector3(contraintDirections[i].x, contraintDirections[i].y, 0) * constraintDistance, (hit.collider != null) ? Color.blue : Color.red);
            if (hit.collider != null)
            {
                if (i == 0)//right
                {
                    deltaPos = new Vector2(Mathf.Clamp(deltaPos.x, -1, 0), deltaPos.y);
                    sides[0] = true;
                }
                if (i == 1)//left
                {
                    deltaPos = new Vector2(Mathf.Clamp(deltaPos.x, 0, 1), deltaPos.y);
                    sides[1] = true;
                }
                if (i == 2)//up
                {
                    deltaPos = new Vector2(deltaPos.x, Mathf.Clamp(deltaPos.y, -1, 0));
                    sides[2] = true;
                }
                if (i == 3)//down
                {
                    deltaPos = new Vector2(deltaPos.x, Mathf.Clamp(deltaPos.y, 0, 1));
                    sides[3] = true;
                }
            }
        }
        bool[] allTrue = new bool[4] { true, true, true, true };
        bool stuck = allTrue[0] == sides[0] && allTrue[1] == sides[1] && allTrue[2] == sides[2] && allTrue[3] == sides[3];
        if (stuck)
        {
            deltaPos = new Vector2(0, 0.5f);
        }
        return deltaPos;
    }

    public void MovePlatformer(Vector2 input, bool jump, bool climb, bool jumpCont, bool dash)
    {
        jumpMag = ConvertJumpInfo(jump, jumpCont);

        //initialization
        SnapTo(out normal, out snapDeltaPos, out touchingGround);
        deltaPos = snapDeltaPos = Vector2.zero;
        Vector3 pos = transform.position;

        if (touchingGround)
        {
            timeLeftGround = Time.unscaledTime;
            lastNormal = normal;
        }
        

        //Physics
        if (jump & canJump)
        {
            //Debug.Log(canJump + " t " + (Time.unscaledTime - timeLeftGround));
            velocity = lastNormal * jumpSpeed + velocity.x * Vector2.right;//apply jump at the normal of the ground;
            touchingGround = false;
            airXSpeed = groundXSpeed;
        }

        if (climb)
        {
            
        }
        else if (!touchingGround)
        {
            deltaPos.y = 0;
            LayerMask mask = collideLayers;
            Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
            float constraintDistance = cCollider.radius * transform.localScale.y + skinWidth;
            RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, Vector2.up, constraintDistance, mask);
            if (hit.collider != null)
                velocity.y = Mathf.Min(velocity.y, 0);

            velocity += Vector2.up * Mathf.Lerp(jumpSpeed, -Mathf.Abs(gravity), jumpMagCurve.Evaluate(jumpMag)) * Time.deltaTime;
            if(useOverideFall) velocity += Vector2.up * Mathf.Clamp(input.y, -1, 0) * aimedFallSpeed * Time.deltaTime;
        }
        else
        {
            velocity = new Vector2(velocity.x * Mathf.Lerp(1, 0, normal.x), velocity.y * Mathf.Lerp(1, 0, normal.y));
        }

        groundXSpeed = velocity.x;
        airXSpeed = velocity.x;
        float threshold = 0.1f; // should be positive

        //add input
        if (input.x > 0.1f)
        {
            if (groundXSpeed < -threshold)
                groundXSpeed += groundMaxXDeacceleration * Time.deltaTime;
            else
                groundXSpeed += groundMaxXAcceleration * Time.deltaTime;

            if (airXSpeed < -threshold)
                airXSpeed += airMaxXDeacceleration * Time.deltaTime;
            else
                airXSpeed += airMaxXAcceleration * Time.deltaTime;

        }
        else if (input.x < -0.1f)
        {
            if (groundXSpeed > threshold)
                groundXSpeed -= groundMaxXDeacceleration * Time.deltaTime;
            else
                groundXSpeed -= groundMaxXAcceleration * Time.deltaTime;

            if (airXSpeed > threshold)
                airXSpeed -= airMaxXDeacceleration * Time.deltaTime;
            else
                airXSpeed -= airMaxXAcceleration * Time.deltaTime;
        }
        else
        {
            //if close to zero then zero
            //if there is no input, slow it by the acceleration

            if (Mathf.Abs(groundXSpeed) < threshold * 2)
                groundXSpeed = 0;
            else
                groundXSpeed -= groundMaxXDeacceleration * Time.deltaTime * Mathf.Sign(groundXSpeed);

            if (Mathf.Abs(airXSpeed) < threshold * 2)
                airXSpeed = 0f;
            else
                airXSpeed -= airMaxXDeacceleration * Time.deltaTime * Mathf.Sign(airXSpeed);
        }

        //clamp the speeds.
        groundXSpeed = Mathf.Clamp(groundXSpeed, -groundMaxXSpeed, groundMaxXSpeed);
        airXSpeed = Mathf.Clamp(airXSpeed, -airMaxXControlSpeed, airMaxXControlSpeed);

        //Add Physics here
        float temp = airXSpeed;
        
        airXSpeed = Mathf.Clamp(temp, -airMaxXSpeed, airMaxXSpeed);
        velocity.x = (touchingGround ? groundXSpeed : airXSpeed);

        //Dash overrides
        if(touchingGround)
        {
            //Reset dashes;
            dashes = maxDashes;
        }
        
        if(dash && dashes > 0)
        {
            velocity = input.normalized * dashVelocity;
            dashes -= 1;
        }
        

        Vector2 wallNormalR = Vector2.zero;
        Vector2 wallNormalL = Vector2.zero;

        if(climb && (touchingWall(out wallNormalR, Vector2.right) || touchingWall(out wallNormalL, Vector2.left)) && !touchingGround)
        {
            deltaPos = new Vector2(0, Input.GetAxis("Vertical")) * Time.deltaTime;
            velocity = Vector2.zero;
            Debug.Log("Climbin");
        }
        else
        {
            deltaPos += velocity * Time.deltaTime;
        }

        //Constraint
        deltaPos = Constrained(deltaPos);

        //Animation
        SetAnimator(jump);

        //Apply
        deltaPos += snapDeltaPos;
        pos += new Vector3(deltaPos.x, deltaPos.y, 0);
        transform.position = pos;
    }

    float timeLeftGround;
    float timeSinceLeftGround;

    enum SnapType { none, ground, wall };
    void SnapTo(out Vector2 normal, out Vector2 deltaPos, out bool touchingGround)
    {
        LayerMask mask = collideLayers;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = mask;

        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        Physics2D.CircleCast((Vector2)transform.position, cCollider.radius + skinWidth, Vector2.down, filter, hits, 0);
        collisions.Clear();
        foreach (RaycastHit2D r in hits)
        {
            collisions.Add(new CollisionData(r.normal, r.collider));
        }
        //For now only ground
        Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
        float distance = cCollider.radius + snapMaxDistance;

        //Snap to ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, Vector2.down, distance, mask);
        Debug.DrawRay(transform.position + collisionOffset, Vector3.down * distance, (hit.collider != null) ? Color.blue : Color.red);
        normal = hit.normal;
        RaycastHit2D snapPoint = Physics2D.Raycast(transform.position + collisionOffset, -normal, distance, mask);
        touchingGround = hit.collider != null;
        deltaPos = -snapPoint.point + new Vector2(transform.position.x, transform.position.y);
        return;
    }
    bool touchingWall(out Vector2 delta, Vector2 dir)
    {
        LayerMask mask = collideLayers;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = mask;

        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        Physics2D.CircleCast((Vector2)transform.position, cCollider.radius + skinWidth, dir, filter, hits, 0);
        collisions.Clear();
        foreach (RaycastHit2D r in hits)
        {
            collisions.Add(new CollisionData(r.normal, r.collider));
        }
        //For now only ground
        Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
        float distance = cCollider.radius + snapMaxDistance;

        //Snap to ground
        RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, dir, distance, mask);
        RaycastHit2D snapPoint = Physics2D.Raycast(transform.position + collisionOffset, -hit.normal, distance, mask);
        delta = -snapPoint.point + new Vector2(transform.position.x, transform.position.y);

        return hit.collider != null;
    }
    

    bool jumpContinuous;
    float jumpMagnitude;
    float jumpStart;
    float ConvertJumpInfo(bool jump, bool jumpCont)
    {
        float timeToKeepJumping = 3f;//the larger this number is the smaller the window to keep jumping
        if (jump && canJump)
        {
            jumpStart = Time.unscaledTime;
            jumpContinuous = true;
        }
        if (jumpCont)
        {
            if (jumpContinuous)
                jumpMagnitude = Mathf.Clamp((Time.unscaledTime - jumpStart) * timeToKeepJumping, 0, 1);
            else
            {
                jumpMagnitude = 1;
            }
        }
        else
        {
            jumpContinuous = false;
            jumpMagnitude = 1f;
        }
        return jumpMagnitude;
    }
}