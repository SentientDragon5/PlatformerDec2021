using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platformer3 : MonoBehaviour
{
    [Header("Ground Movement")]
    public float groundMaxXSpeed = 10f;
    float groundXSpeed = 0;

    [Header("Wall Movement")]
    public float wallSlideFriction = 0.08f;
    public float wallMaxYSpeed = 10;
    float wallYSpeed = 0;

    [Header("Air Movement")]
    public float airMaxXControlSpeed = 10f; //max speed with wasd
    public float airMaxXSpeed = 36f;        //max speed from being pushed etc.
    public float airMaxXAcceleration = 50;
    public float airMaxXDeacceleration = 100;
    float airXSpeed = 0;

    [Header("Jump")]
    public float jumpSpeed = 10f;
    public float gravity = 40;
    public float jumpForgivenessTime = 0.1f;

    [Header("Dash")]
    public float dashVelocity = 25;
    [Range(0, 3)] public int maxDashes = 1;
    public int dashes = 0;

    Vector2 deltaPos = Vector2.zero;
    Vector2 direction = Vector2.down;

    [Header("Jump Magnitude")]
    public float jumpMagCoeficient = 10;
    public AnimationCurve jumpMagCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    float jumpMag;
    bool jumpContinuous;
    float jumpMagnitude;
    float jumpStart;

    Vector2 velocity;
    Vector2 normal = Vector2.up;

    public bool touchingGround { get; private set; }
    public bool touchingWall { get; private set; }
    float timeLeftGround;

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
    Vector2 snapDeltaPos;

    public enum sideDir { right = 0, left = 1, up = 2, down = 3 };
    private Vector2[] contraintDirections = new Vector2[4] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

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
        anim.SetBool("climb", touchingWall && (climb || !touchingGround));


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
            if(climb && touchingWall && !touchingGround)
            {
                velocity += Vector2.right * (sides[(int)sideDir.right] ? -1 : 1) * jumpSpeed * 1f;
                //Debug.Log((sides[(int)sideDir.right] ? -1 : 1) * jumpSpeed);
            }

            touchingGround = false;
            touchingWall = false;
            //airXSpeed = groundXSpeed;
            //wallYSpeed = 0;
        }
        groundXSpeed = velocity.x;
        airXSpeed = velocity.x;

        if (touchingWall && climb)
        {
            //climb
            WallYMovement(input.y);
            velocity.y = wallYSpeed;
        }
        else if (touchingWall && !touchingGround && PushingAgainstWall(input.x))
        {
            wallYSpeed = 0;
            //Slide
            if (velocity.y < 0.1f)
            {
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

        airXSpeed = Mathf.Clamp(airXSpeed, -airMaxXSpeed, airMaxXSpeed);
        if (!(touchingWall && climb))
        {
            // if !climbing,  jump/fall
            velocity.x = airXSpeed;
            velocity += Vector2.up * Mathf.Lerp(jumpSpeed, -Mathf.Abs(gravity), jumpMagCurve.Evaluate(jumpMag)) * Time.deltaTime;
        }


        // overidde all if on the ground.
        if (touchingGround)
        {
            //Grounded movement
            GroundXMovement(input.x);
            velocity.x = groundXSpeed;
            if(!(touchingWall && climb))
                velocity.y = 0;
        }

        //Add Physics here



        //apply dashes
        if (dash && dashes > 0)
        {
            Debug.Log("Dash");
            velocity = input.normalized * dashVelocity;
            dashes--;
        }


        //Convert velocity to delta position
        deltaPos += velocity * Time.deltaTime;

        //Constraint
        deltaPos = Constrained(deltaPos);

        //Animation
        SetAnimator(climb);

        //Apply
        deltaPos += snapDeltaPos;
        pos += new Vector3(deltaPos.x, deltaPos.y, 0);
        transform.position = pos;
    }

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

    #region 1D movement
    /// <summary>
    /// Updates groundXSpeed
    /// </summary>
    /// <param name="input"></param>
    void GroundXMovement(float input)
    {
        groundXSpeed = input * groundMaxXSpeed;
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
        wallYSpeed = input * wallMaxYSpeed;
    }
    #endregion
}