using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platformer4 : MonoBehaviour
{
    [Header("Ground Movement")]
    public float groundMaxXSpeed = 12f;
    public float groundMaxXAcceleration = 50;
    public float groundMaxXDeacceleration = 100;
    float groundXSpeed = 0;

    [Header("Wall Movement")]
    public float wallSlideFriction = 0.8f;
    public float wallMaxYSpeed = 10;
    public float wallMaxAcceleration = 50;
    public float wallMaxDeacceleration = 100;
    public float wallYSpeed = 0;

    [Header("Air Movement")]
    public float airMaxXControlSpeed = 12f; //max speed with wasd
    public float airMaxXSpeed = 36f;        //max speed from being pushed etc.
    public float airMaxXAcceleration = 50;
    public float airMaxXDeacceleration = 100;
    float airXSpeed = 0;
    [Header("Jump")]
    public float jumpSpeed = 10f;
    public float gravity = 40;
    public float aimedFallSpeed = 200f;
    public bool useOverideFall = false;

    [Header("Dash")]
    public float dashVelocity = 25;
    [Range(0, 3)] public int maxDashes = 1;
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
    private bool touchingWall;
    public float jumpForgivenessTime = 0.1f;
    Vector2 lastNormal = Vector2.up;
    bool canJump
    {
        get
        {
            return Mathf.Max(Time.unscaledTime - timeLeftGround, 0) <= jumpForgivenessTime;
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
    public enum sideDir { right = 0, left = 1, up = 2, down = 3 };
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

    void SetAnimator(bool climb)
    {
        if (deltaPos.magnitude > 0.1f)
        {
            direction = deltaPos;
        }
        //Make sure to use blend trees or else flipping sprite rend doesnt work.
        anim.SetFloat("y", direction.y);
        anim.SetFloat("speed", deltaPos.magnitude);
        anim.SetBool("onGround", touchingGround);
        anim.SetBool("climb", climb && touchingWall);

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

        bool stuck = sides[0] && sides[1] && sides[2] && sides[3];
        if (stuck)
        {
            deltaPos = new Vector2(0, 0.5f);
        }
        return deltaPos;
    }
    public void CheckSides()
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
                sides[i] = true;
            }
        }
    }
    public bool PushingAgainstWall(float inputX)
    {
        return inputX > 0.1f && sides[(int)sideDir.right] || inputX < -0.1f && sides[(int)sideDir.left];
    }

    public void MovePlatformer(Vector2 input, bool jump, bool climb, bool jumpCont, bool dash)
    {
        jumpMag = ConvertJumpInfo(jump, jumpCont);

        CheckSides();
        touchingGround = sides[(int)sideDir.down];
        touchingWall = sides[(int)sideDir.right] || sides[(int)sideDir.left];

        deltaPos = Vector2.zero;
        Vector3 pos = transform.position;

        if (touchingGround)
        {
            //reset Dashes to max
            dashes = maxDashes;
        }
        if (touchingGround || touchingWall)
        {
            //Set time left the ground to current, set the normal
            timeLeftGround = Time.unscaledTime;
        }
        if (!climb)
        {
            wallYSpeed = 0;
        }

        //Physics
        if (jump & canJump)
        {
            velocity = Vector2.up * jumpSpeed + Vector2.right * velocity.x;//apply jump at the normal of the ground;
            touchingGround = false;
            touchingWall = false;
            airXSpeed = groundXSpeed;
            wallYSpeed = 0;
        }
        if (touchingGround)
        {
            //Grounded movement
            GroundXMovement(input.x);
        }
        if (touchingWall && !touchingGround && climb)
        {
            //climb
            WallYMovement(input.y);
        }
        else
        {
            Debug.Log("PUSH: " + PushingAgainstWall(input.x));
            if (touchingWall && !touchingGround && PushingAgainstWall(input.x))
            {
                wallYSpeed = 0;
                //Slide
                if (velocity.y < 0.1f)
                {
                    Debug.Log("SLIDE");
                    velocity.y = -wallSlideFriction * gravity;
                }
            }

            AirXMovement(input.x);

            //Jumping && falling
            deltaPos.y = 0;
            LayerMask mask = collideLayers;
            Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
            float constraintDistance = cCollider.radius * transform.localScale.y + skinWidth;
            RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, Vector2.up, constraintDistance, mask);
            if (hit.collider != null)
                velocity.y = Mathf.Min(velocity.y, 0);

            velocity += Vector2.up * Mathf.Lerp(jumpSpeed, -Mathf.Abs(gravity), jumpMagCurve.Evaluate(jumpMag)) * Time.deltaTime;

        }

        //groundXSpeed = velocity.x;
        //airXSpeed = velocity.x;

        //add input
        //groundXSpeed = Calculate1DMovementOld(groundXSpeed, groundMaxXSpeed, 0.1f, groundMaxXAcceleration, groundMaxXDeacceleration, input.x, 0.1f);
        //airXSpeed = Calculate1DMovementOld(airXSpeed, airMaxXControlSpeed, 0.1f, airMaxXAcceleration, airMaxXDeacceleration, input.x, 0.1f);

        //Add Physics here
        airXSpeed = Mathf.Clamp(airXSpeed, -airMaxXSpeed, airMaxXSpeed);
        //velocity.x = (touchingGround ? groundXSpeed : airXSpeed);
        if (touchingGround)
        {
            velocity.x = groundXSpeed;
            velocity.y = 0;
        }
        else if (touchingWall && climb)
        {
            velocity.y = wallYSpeed;
        }
        else
        {
            velocity.x = airXSpeed;
        }

        deltaPos += velocity * Time.deltaTime;

        if (dash && dashes > 0)
        {
            Debug.Log("Dash");
            velocity = input.normalized * dashVelocity;
            dashes--;
        }

        //Constraint
        deltaPos = Constrained(deltaPos);

        //Animation
        SetAnimator(climb);

        //Apply
        deltaPos += snapDeltaPos;
        pos += new Vector3(deltaPos.x, deltaPos.y, 0);
        transform.position = pos;
    }


    float timeLeftGround;
    enum SnapType { none, ground, wall };
    void SnapTo(out Vector2 normal, out Vector2 deltaPos, out bool touchingGround)
    {
        LayerMask mask = collideLayers;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = mask;

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
    bool TouchingWall(out Vector2 delta, Vector2 dir)
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

    float Calculate1DMovement(float currentSpeed, float maxSpeed, float minSpeed, float accel, float deaccel, float input, float threshold)
    {
        if (input < threshold)
        {
            //Treat as zero
            //deaccel the speed
            if (Mathf.Abs(currentSpeed) < Mathf.Abs(minSpeed))
                currentSpeed = 0;
            else
                currentSpeed -= deaccel * Mathf.Sign(currentSpeed) * Time.deltaTime;
        }
        //sign returns 1 for positive and zero, and -1 for negitive
        else if (Mathf.Sign(input) == Mathf.Sign(currentSpeed)) //same direction
        {
            currentSpeed += input * accel * Mathf.Sign(currentSpeed) * Time.deltaTime;
        }
        else //Different directions
        {
            currentSpeed += deaccel * Mathf.Sign(currentSpeed) * Time.deltaTime;
        }

        if (Mathf.Abs(currentSpeed) > Mathf.Abs(maxSpeed))
        {
            currentSpeed = maxSpeed * Mathf.Sign(currentSpeed);
        }
        else if (Mathf.Abs(currentSpeed) < Mathf.Abs(minSpeed))
        {
            currentSpeed = 0;
        }

        return currentSpeed;
    }
    float Calculate1DMovementOld(float currentSpeed, float maxSpeed, float minSpeed, float accel, float deaccel, float input, float threshold)
    {
        if (input > threshold)
        {
            if (currentSpeed < -minSpeed)
                currentSpeed += deaccel * Time.deltaTime;
            else
                currentSpeed += accel * Time.deltaTime;
        }
        else if (input < threshold)
        {
            if (currentSpeed > minSpeed)
                currentSpeed -= deaccel * Time.deltaTime;
            else
                currentSpeed -= accel * Time.deltaTime;
        }
        else
        {
            //if close to zero then zero
            //if there is no input, slow it by the acceleration

            if (Mathf.Abs(currentSpeed) < minSpeed * 2)
                currentSpeed = 0;
            else
                currentSpeed -= deaccel * Time.deltaTime * Mathf.Sign(currentSpeed);
        }

        //clamp the speed.
        currentSpeed = Mathf.Clamp(maxSpeed, -maxSpeed, maxSpeed);
        return currentSpeed;
    }
    void xAxisMovement(Vector2 input)
    {
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
    }

    /// <summary>
    /// Updates groundXSpeed
    /// </summary>
    /// <param name="input"></param>
    void GroundXMovement(float input)
    {
        float threshold = 0.1f; // should be positive

        //add input
        if (input > 0.1f)
        {
            if (groundXSpeed < -threshold)
                groundXSpeed += groundMaxXDeacceleration * Time.deltaTime;
            else
                groundXSpeed += groundMaxXAcceleration * Time.deltaTime;
        }
        else if (input < -0.1f)
        {
            if (groundXSpeed > threshold)
                groundXSpeed -= groundMaxXDeacceleration * Time.deltaTime;
            else
                groundXSpeed -= groundMaxXAcceleration * Time.deltaTime;
        }
        else
        {
            //if close to zero then zero
            //if there is no input, slow it by the acceleration

            if (Mathf.Abs(groundXSpeed) < threshold * 2)
                groundXSpeed = 0;
            else
                groundXSpeed -= groundMaxXDeacceleration * Time.deltaTime * Mathf.Sign(groundXSpeed);
        }
        //clamp the speeds.
        groundXSpeed = Mathf.Clamp(groundXSpeed, -groundMaxXSpeed, groundMaxXSpeed);
    }
    /// <summary>
    /// Updates airXSpeed
    /// </summary>
    /// <param name="input"></param>
    void AirXMovement(float input)
    {
        float threshold = 0.1f; // should be positive

        //add input
        if (input > 0.1f)
        {
            if (airXSpeed < -threshold)
                airXSpeed += airMaxXDeacceleration * Time.deltaTime;
            else
                airXSpeed += airMaxXAcceleration * Time.deltaTime;

        }
        else if (input < -0.1f)
        {
            if (airXSpeed > threshold)
                airXSpeed -= airMaxXDeacceleration * Time.deltaTime;
            else
                airXSpeed -= airMaxXAcceleration * Time.deltaTime;
        }
        else
        {
            //if close to zero then zero
            //if there is no input, slow it by the acceleration
            if (Mathf.Abs(airXSpeed) < threshold * 2)
                airXSpeed = 0f;
            else
                airXSpeed -= airMaxXDeacceleration * Time.deltaTime * Mathf.Sign(airXSpeed);
        }
        //clamp the speeds.
        airXSpeed = Mathf.Clamp(airXSpeed, -airMaxXControlSpeed, airMaxXControlSpeed);
    }
    /// <summary>
    /// Updates wallySpeed
    /// </summary>
    /// <param name="input"></param>
    void WallYMovement(float input)
    {
        float threshold = 0.1f; // should be positive

        //add input
        if (input > 0.1f)
        {
            if (wallYSpeed < -threshold)
                wallYSpeed += wallMaxDeacceleration * Time.deltaTime;
            else
                wallYSpeed += wallMaxAcceleration * Time.deltaTime;
        }
        else if (input < -0.1f)
        {
            if (wallYSpeed > threshold)
                wallYSpeed -= wallMaxDeacceleration * Time.deltaTime;
            else
                wallYSpeed -= wallMaxAcceleration * Time.deltaTime;
        }
        else
        {
            //if close to zero then zero
            //if there is no input, slow it by the acceleration

            if (Mathf.Abs(groundXSpeed) < threshold * 2)
                wallYSpeed = 0;
            else
                wallYSpeed -= wallMaxDeacceleration * Time.deltaTime * Mathf.Sign(wallYSpeed);
        }
        //clamp the speeds.
        wallYSpeed = Mathf.Clamp(wallYSpeed, -wallMaxYSpeed, wallMaxYSpeed);
    }
}