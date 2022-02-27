using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform2 : MonoBehaviour
{
    public bool touchGRND;

    [Header("Ground")]
    public float groundMaxSpeed = 10;
    public float groundAccel = 50;
    public float groundDeaccel = 100;

    float groundSpeed;

    //if(touching ground) velocity = vector2.right * groundspeed
    //if(input) groundSpeed += accel;
    //if(!input) groundspeed -= deaccel;

    [Header("Jump")]
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public float jumpMultiplier = 10;
    //velocity += SurfaceNormal * jump(t);
    /// <summary> Time since left the ground </summary>
    public float jumpTime;
    /// <summary> Is the jump still happening continuously since touching the ground? </summary>
    public bool jumpContinuous;

    [Header("Dash")]
    public float dashVelocity = 25;
    [Range(0, 3)] public int maxDashes = 1;
    public int dashes = 0;
    //velocity = input * dashVelocity;

    [Header("Air")]
    public float airAccel = 1;//Air control
    public float airDeaccel = 0;//Drag
    public float gravity = 20;
    public float maxFallSpeed;

    //velocity = velocity.normalized * (velocity.magnitude -airDeaccel * Time.deltaTime);
    //if(velocity.y < maxFallSpeed) velocity = velocity.x + 

    //[Header("")]

    public Vector2 velocity;
    public Vector2 deltaPos;

    //public Vector3 relitivePosition;
    //public Transform connectedBody;

    //Animation
    private Animator anim;
    Vector2 direction;
    public Vector2 facing = Vector2.right;


    private CircleCollider2D cCollider;
    public LayerMask collideLayers;
    float SkinWidth
    {
        get
        {
            return Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) * 0.1f + 0.01f;
        }
    }

    private SpriteRenderer rend;


    public float jumpForgivenessTime = 0.1f;
    float timeLeftGround;
    bool canJump
    {
        get
        {
            return Mathf.Max(Time.unscaledTime - timeLeftGround, 0) <= jumpForgivenessTime;
        }
    }

    void Awake()
    {
        anim = GetComponent<Animator>();
        cCollider = GetComponent<CircleCollider2D>();
        rend = GetComponent<SpriteRenderer>();
    }

    public void Move(Vector2 input, bool jump, bool climb, bool jumpCont, bool dash)
    {
        //velocity = VelocityAlongSurface(input, Vector2.up, velocity, 2, airAccel, airDeaccel);
        
        //return;
        touchGRND = Grounded;

        //Input Movement
        jumpContinuous = jumpContinuous && jumpCont;

        if(jumpContinuous)
        {
            Debug.Log("JUMP");
            velocity = Jump(LastNormal, jumpTime);
            velocity = VelocityAlongSurface(input, Vector2.up, velocity, 6, airAccel, airDeaccel);
        }
        else if (climb && AtWall)//if climb & at wall
        {
            Debug.Log("CLIMB");
            //Do climbing movement
            velocity = VelocityAlongSurface(input, lastClimbNormal, velocity, 6, groundAccel, groundDeaccel);

            //raycast. get normal, move along normal, snap to wall

            //jump
            if (jump && canJump)
            {
                jumpContinuous = true;
            }
        }
        else if(Grounded)//Grounded
        {
            Debug.Log("GROUND");
            //If you are grounded do walking
            velocity = VelocityAlongSurface(input, lastGroundNormal, velocity, 6, groundAccel, groundDeaccel);

            //raycast down, get normal, move along normal, snap to ground.


            //Jump
            if (jump && canJump)
            {
                jumpContinuous = true;
            }
        }
        else //Air
        {
            Debug.Log("AIR");
            velocity = VelocityAlongSurface(input, Vector2.up, velocity, 6, airAccel, airDeaccel);

            //Gravity
            velocity += Vector2.down * gravity * Time.deltaTime;
        }

        //Outside Physics / Reference Frame

        //Override with a dash
        if (dash)
        {
            Vector2 dashDir = input.magnitude < 0.1 ? facing : input.normalized;
            velocity = dashDir * dashVelocity;
        }

        //Drag
        velocity = velocity.normalized * (velocity.magnitude - airDeaccel * Time.deltaTime);
        //Clamp velocity
        //if (velocity.y < maxFallSpeed) velocity = new Vector2(velocity.x, maxFallSpeed);


        //Animation
        SetAnimator(jump);
        deltaPos = Vector2.zero;
        deltaPos += velocity * Time.deltaTime;
        Vector3 pos = transform.position;
        pos += new Vector3(deltaPos.x, deltaPos.y, 0);
        transform.position = pos;
    }

    public static Vector2 VelocityAlongSurface(Vector2 input, Vector2 normal, Vector2 velocity, float maxSpeed, float accel, float deaccel)
    {
        if(normal.sqrMagnitude < 0.01f)
        {
            normal = Vector2.up;
        }
        normal.Normalize();

        Vector2 i = input;
        
        Vector2 velNorm = velocity.normalized;

        //Deaccelerate unless zero
        if(velocity.sqrMagnitude > 0.01f)
        {
            velocity -= (velNorm * deaccel) * Time.deltaTime;
        }
        //accelerate unless no input
        if (i.sqrMagnitude > 0.01f)
        {
            velocity += i * accel * Time.deltaTime;
        }

        //Clamp Speed
        if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        Debug.Log("i:" + i + "v:" + velocity + "i Dot v:" + Vector2.Dot(i, velocity));

        return velocity;

        

        if (velNorm.magnitude < 0.1)
        {
            velocity += i * accel * Time.deltaTime;
        }
        else if (i.magnitude < 0.1)
        {
            velocity -= (velNorm * deaccel) * Time.deltaTime;
        }
        else if (Vector2.Dot(i, velNorm) > 0)
        {
            velocity += i * accel * Time.deltaTime;
        }
        else if(Vector2.Dot(i, velNorm) < 0)
        {
            velocity -= ((velNorm * deaccel) + (i * deaccel)) * Time.deltaTime;
        } 
        
        
    }
    public static Vector2 VelocityAlongSurfaceComplicated(Vector2 input, Vector2 normal, Vector2 velocity, float maxSpeed, float accel, float deaccel)
    {
        normal.Normalize();
        if (normal == Vector2.zero)
        {
            normal = Vector2.up;
        }

        Vector2 i = new Vector2(Mathf.Abs(normal.y) * input.x, Mathf.Abs(normal.x) * input.y);
        i.Normalize();
        Vector2 velNorm = velocity.normalized;
        if (velNorm.magnitude < 0.1)
        {
            velocity += i * accel;
        }
        else if (i.magnitude < 0.1)
        {
            velocity -= (velNorm * deaccel);
        }
        else if (Vector2.Dot(i, velNorm) > 0)
        {
            velocity += i * accel;
        }
        else if (Vector2.Dot(i, velNorm) < 0)
        {
            velocity -= (velNorm * deaccel) + (i * deaccel);
        }
        if (new Vector2(i.x * velocity.x, i.y * velocity.y).sqrMagnitude > maxSpeed * maxSpeed)
        {
            velocity = new Vector2(i.x * velocity.x, i.y * velocity.y).normalized * maxSpeed + new Vector2(i.y * velocity.x, i.x * velocity.y);
        }
        Debug.Log("i:" + i + "v:" + velocity + "i Dot v:" + Vector2.Dot(i, velocity));

        return velocity;
    }

    public Vector2 SnapDelta(Vector2 direction)
    {
        LayerMask mask = collideLayers;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = mask;

        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        Physics2D.CircleCast((Vector2)transform.position, cCollider.radius + SkinWidth, Vector2.down, filter, hits, 0);
        Debug.DrawRay(hits[0].point, hits[0].normal, Color.magenta);
        return (Vector2)transform.position - hits[0].point + hits[0].normal * (cCollider.radius + SkinWidth);
    }

    /// <summary>
    /// Returns the last normal of the ground that the controller was on. includes walls.
    /// </summary>
    public Vector2 LastNormal
    {
        get
        {
            return lastSurfaceClimbing ? lastClimbNormal : lastGroundNormal;
        }
    }

    bool lastSurfaceClimbing = false;
    public Vector2 lastGroundNormal;
    public Vector2 lastClimbNormal;
    /// <summary>
    /// Returns Whether the controller is on the ground. this affects whether they can jump.
    /// </summary>
    public bool Grounded
    {
        get
        {
            LayerMask mask = collideLayers;

            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask = mask;

            Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
            float distance = cCollider.radius + SkinWidth;



            RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, Vector2.down, distance, mask);
            Debug.DrawRay(transform.position + collisionOffset, Vector3.down * distance, (hit.collider != null) ? Color.blue : Color.red);
            lastGroundNormal = hit.normal;

            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            Physics2D.CircleCast((Vector2)transform.position, cCollider.radius + SkinWidth, Vector2.down, filter, hits, 0);
            
            bool o = hits.Count > 0 && hits[0].collider != null;
            Debug.Log(hits.Count);
            if (o)
            {
                lastGroundNormal = hits[0].normal;
                lastSurfaceClimbing = false;
            }

            return o;
        }
    }
    public bool AtWall
    {
        get
        {
            LayerMask mask = collideLayers;

            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask = mask;

            Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
            float distance = cCollider.radius + SkinWidth;


            RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, facing, distance, mask);
            Debug.DrawRay(transform.position + collisionOffset, Vector3.down * distance, (hit.collider != null) ? Color.blue : Color.red);
            lastClimbNormal = hit.normal;

            if (hit.collider != null)
                lastSurfaceClimbing = false;

            return hit.collider != null;
        }
    }
    public bool Raycast(Vector2 dir, float dist, out Vector2 normal)
    {
        LayerMask mask = collideLayers;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = mask;

        Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
        float distance = cCollider.radius + SkinWidth;

        RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, dir, dist, mask);
        Debug.DrawRay(transform.position + collisionOffset, Vector3.down * dist, (hit.collider != null) ? Color.blue : Color.red);

        normal = hit.normal;

        return hit.collider != null;
    }
    public bool CircleCast(Vector2 dir, float dist, out Vector2 normal)
    {
        LayerMask mask = collideLayers;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = mask;

        Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
        float distance = cCollider.radius + SkinWidth;

        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        Physics2D.CircleCast((Vector2)transform.position, cCollider.radius + SkinWidth, dir, filter, hits, 0);

        bool o = hits.Count > 0 && hits[0].collider != null;

        Debug.DrawRay(transform.position + collisionOffset, Vector3.down * dist, o ? Color.blue : Color.red);

        if (o)
            normal = hits[0].normal;
        else
            normal = Vector2.zero;

        return o;
    }

    public bool CanJump
    {
        get
        {
            return Mathf.Max(Time.unscaledTime - timeLeftGround, 0) <= jumpForgivenessTime;
        }
    }

    /// <summary>
    /// Calculated the velocity
    /// </summary>
    /// <param name="normal"> Surface Normal</param>
    /// <param name="t"> Time to evaluate at</param>
    /// <returns></returns>
    Vector2 Jump(Vector2 normal, float t)
    {
        return normal * jumpCurve.Evaluate(t) * jumpMultiplier;
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
        anim.SetBool("onGround", Grounded);

        if (direction.x > 0.1)
        {
            rend.flipX = false;
            facing = Vector2.right;
        }
        if (direction.x < -0.1)
        {
            rend.flipX = true;
            facing = Vector2.left;
        }
    }
}
