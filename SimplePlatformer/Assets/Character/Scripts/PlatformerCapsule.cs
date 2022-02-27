using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Character
{
    [RequireComponent(typeof(CapsuleCollider2D), typeof(Rigidbody2D))]
    public class PlatformerCapsule : MonoBehaviour, IPlatformer
    {
        #region Inspector Variables
        private Animator anim;
        private CapsuleCollider2D cCollider;
        private SpriteRenderer rend; //For flipping the sprite to look the other way.
        private Rigidbody2D rb;

        [Header("Ground Movement")]
        [SerializeField] private float groundMaxXSpeed = 10f;

        [Header("Wall Movement")]
        [SerializeField] private float wallSlideFriction = 0.01f;
        [SerializeField] private float wallMaxYSpeed = 10;

        [Header("Air Movement")]
        [SerializeField] private float airMaxXControlSpeed = 10f; //max speed with wasd
        [SerializeField] private float airMaxXSpeed = 36f;        //max speed from being pushed etc.
        [SerializeField] private float airMaxXAcceleration = 50;
        [SerializeField] private float airMaxXDeacceleration = 100;

        [Header("Jump")]
        [SerializeField] private float jumpSpeed = 10f;
        [SerializeField] private float gravity = 40;
        [SerializeField] private float jumpForgivenessTime = 0.1f;

        [Header("Dash")]
        [SerializeField] private float dashVelocity = 25;
        [SerializeField] private int maxDashes = 1;
        [SerializeField] private int dashes = 0;

        [Header("Energy")]
        [SerializeField] private float energy = 1f;
        [SerializeField] private float wallJumpEnergy = 0.15f;
        [SerializeField] private float wallIdleEnergyRate = 0.01f;
        [SerializeField] private float climbEnergyRate = 0.02f;// greater than idle

        [Header("Jump Magnitude")]
        [SerializeField] private AnimationCurve jumpMagCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Constraint")]
        [SerializeField] private LayerMask collideLayers;
        [SerializeField] private float skinWidth = 0.01f;

        #endregion

        #region instance variables
        //component velocities are separate so that calulations can be done separately. See CalculateGroundMovement(float input)
        float groundXSpeed = 0;
        float wallYSpeed = 0;
        float airXSpeed = 0;

        /// <summary> Has the jump button been continuously held? </summary>
        bool jumpContinuous;
        /// <summary> Returns 0 to 1 on where to evaluate the curve </summary>
        float jumpMagnitude;
        /// <summary> Has the jump button been continuously held? </summary>
        float jumpTimeStart;

        /// <summary> the time that the character left the ground </summary>
        float timeLeftGround;

        /// <summary> velocity times delta time, updated at the end of Move </summary>
        Vector2 deltaPos = Vector2.zero;
        /// <summary> the direction that the character is facing </summary>
        Vector2 direction = Vector2.down;


        /// <summary> returns whether the time left the ground is less than or equal to the forgiveness time </summary>
        bool canJump
        {
            get
            {
                return Mathf.Max(Time.unscaledTime - timeLeftGround, 0) <= jumpForgivenessTime;
            }
        }
        /// <summary> returns whether there is energy left for the character to climb or jump from a wall </summary>
        bool canClimb
        {
            get
            {
                energy = Mathf.Clamp01(energy);
                return energy > 0;
            }
        }

        /// <summary> Returns how far the raycast should extend to check for extra large velocities </summary>
        float SkinWidth
        {
            get
            {
                return Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) * 0.1f + skinWidth;
            }
        }

        /// <summary> holds reference order of the sides for side checking </summary>
        public enum SideDir { right = 0, left = 1, up = 2, down = 3 };
        /// <summary> holds the side directions in Vector2 format </summary>
        private Vector2[] contraintDirections = new Vector2[4] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };
        /// <summary> holds which sides are being touched </summary>
        bool[] sides = new bool[4] { false, false, false, false };
        #endregion

        #region public
        /// <summary> Returns the current velocity of the character </summary>
        private Vector2 velocity;
        private bool touchingGround { get; set; }
        private bool touchingWall { get; set; }

        /// <summary> Returns the current velocity of the character </summary>
        public Vector2 Velocity { get => velocity; }
        /// <summary> Publicly Returns whether the character is on the ground </summary>
        public bool TouchingGround { get => touchingGround; }
        /// <summary> Publicly Returns whether the character is touching a wall from either side </summary>
        public bool TouchingWall { get => touchingWall; }

        /// <summary> Returns the current energy 0 to 1 where 1 is full energy. </summary>
        public float Energy
        {
            get
            {
                energy = Mathf.Clamp01(energy);
                return energy;
            }
        }
        public int Dashes { get => dashes; }

        public void RechargeAll()
        {
            dashes = maxDashes;
            energy = 1f;
        }
        

        [SerializeField] UnityEvent onDash;
        public UnityEvent OnDash { get => onDash; }
        [SerializeField] UnityEvent onJump;
        public UnityEvent OnJump { get => onJump; }

        bool lastClimb = false;
        public bool Climbing { get => lastClimb; }

        #endregion

        void Awake()
        {
            anim = GetComponent<Animator>();
            cCollider = GetComponent<CapsuleCollider2D>();
            rend = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();

            rb.freezeRotation = true;
        }

        public void Move(Vector2 input, bool jump, bool climb, bool jumpCont, bool dash)
        {
            //Set delta positon to zero
            // we keep velocity and read & write it each frame
            // then multipy velocity by delta time to get delta positon.
            // we add the delta positon to the transform position at the beginning of the frame.
            deltaPos = Vector2.zero;
            Vector3 pos = transform.position;

            lastClimb = climb;

            // we have whether jump is pressed and jump is held down continuously since pressed.
            // I convert it to a value from 0 to 1 to evaluate through a curve to find how much velocity to add.
            ConvertJumpInfo(jump, jumpCont);

            //Check whether the character is touching walls or ground.
            CheckSides();
            touchingGround = sides[(int)SideDir.down];
            touchingWall = sides[(int)SideDir.right] || sides[(int)SideDir.left];


            if (touchingGround)
            {

                dashes = maxDashes; //reset Dashes to max
                energy = 1f;// refill energy
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

            if (jump & canJump)
            {
                velocity = Vector2.up * jumpSpeed + Vector2.right * velocity.x;//apply jump at the normal of the ground;
                if (climb && touchingWall && !touchingGround)
                {
                    velocity += Vector2.right * (sides[(int)SideDir.right] ? -1 : 1) * jumpSpeed * 1f;
                }
                if (touchingWall && !touchingGround && canClimb)
                {
                    energy -= wallJumpEnergy;
                }

                touchingGround = false;
                touchingWall = false;
            }
            groundXSpeed = velocity.x;
            airXSpeed = velocity.x;

            if (touchingWall && climb && canClimb)
            {
                //climb
                WallYMovement(input.y);
                velocity.y = wallYSpeed;

                energy -= (wallIdleEnergyRate + input.y * (climbEnergyRate - wallIdleEnergyRate)) * Time.deltaTime;
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
            Vector2 constraintDistance = new Vector2(cCollider.size.x * transform.localScale.x + SkinWidth, cCollider.size.y * transform.localScale.y + SkinWidth);
            RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, Vector2.up, constraintDistance.y, mask);
            if (hit.collider != null)
                velocity.y = Mathf.Min(velocity.y, 0);

            airXSpeed = Mathf.Clamp(airXSpeed, -airMaxXSpeed, airMaxXSpeed);
            if (!(touchingWall && climb && canClimb))
            {
                // if !climbing,  jump/fall
                velocity.x = airXSpeed;
                velocity += Vector2.up * Mathf.Lerp(jumpSpeed, -Mathf.Abs(gravity), jumpMagCurve.Evaluate(jumpMagnitude)) * Time.deltaTime;
            }


            // overidde all if on the ground.
            if (touchingGround)
            {
                //Grounded movement
                GroundXMovement(input.x);
                velocity.x = groundXSpeed;
                if (!(touchingWall && climb && canClimb))
                    velocity.y = 0;
            }

            //Add Physics here



            //apply dashes
            if (dash && dashes > 0)
            {
                StopCoroutine(Dash());
                StartCoroutine(Dash());
                dashDir = input.normalized;
                dashes--;
                OnDash.Invoke();
            }
            if(dashing)
            {
                velocity = dashDir * dashVelocity;
            }

            //Convert velocity to delta position
            deltaPos += velocity * Time.deltaTime;

            //Constraint
            deltaPos = Constrained(deltaPos);

            //Animation
            SetAnimator(climb);

            //Apply
            pos += new Vector3(deltaPos.x, deltaPos.y, 0);
            transform.position = pos;
        }

        void SetAnimator(bool climb)
        {
            if (deltaPos.magnitude > 0.1f)
            {
                direction = deltaPos;
            }

            //Make sure to use blend trees or else flipping sprite rend doesnt work.
            if (anim != null)
            {
                anim.SetFloat("y", direction.y);
                anim.SetFloat("speed", Mathf.Abs(climb ? velocity.y : velocity.x));
                anim.SetBool("onGround", touchingGround);
                anim.SetBool("climb", touchingWall && (climb || !touchingGround));
            }

            if (rend != null)
            {
                if (direction.x > 0.01f)
                    rend.flipX = false;
                if (direction.x < -0.01f)
                    rend.flipX = true;
            }
        }


        #region SideChecker
        public Vector2 Constrained(Vector2 deltaPos)
        {
            LayerMask mask = collideLayers;
            Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
            Vector2 constraintDistance = new Vector2(cCollider.size.x * transform.localScale.x * 0.5f + SkinWidth, cCollider.size.y * transform.localScale.y * 0.5f + SkinWidth);

            sides = new bool[4] { false, false, false, false };

            for (int i = 0; i < 4; i++)
            {
                RaycastHit2D hit;
                if (i==0 || i==1)
                {
                    hit = Physics2D.Raycast(transform.position + collisionOffset, contraintDirections[i], constraintDistance.x, mask);
                    Debug.DrawRay(transform.position + collisionOffset, new Vector3(contraintDirections[i].x, contraintDirections[i].y, 0) * constraintDistance.x, (hit.collider != null) ? Color.blue : Color.red);
                }
                else
                {
                    hit = Physics2D.Raycast(transform.position + collisionOffset, contraintDirections[i], constraintDistance.y, mask);
                    Debug.DrawRay(transform.position + collisionOffset, new Vector3(contraintDirections[i].x, contraintDirections[i].y, 0) * constraintDistance.y, (hit.collider != null) ? Color.blue : Color.red);
                }
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
            Vector2 constraintDistance = new Vector2(cCollider.size.x * transform.localScale.x * 0.5f + SkinWidth, cCollider.size.y * transform.localScale.y * 0.5f + SkinWidth);

            sides = new bool[4] { false, false, false, false };

            for (int i = 0; i < 4; i++)
            {
                RaycastHit2D hit;
                if (i == 0 || i == 1)
                {
                    hit = Physics2D.Raycast(transform.position + collisionOffset, contraintDirections[i], constraintDistance.x, mask);
                    Debug.DrawRay(transform.position + collisionOffset, new Vector3(contraintDirections[i].x, contraintDirections[i].y, 0) * constraintDistance.x, (hit.collider != null) ? Color.blue : Color.red);
                }
                else
                {
                    hit = Physics2D.Raycast(transform.position + collisionOffset, contraintDirections[i], constraintDistance.y, mask);
                    Debug.DrawRay(transform.position + collisionOffset, new Vector3(contraintDirections[i].x, contraintDirections[i].y, 0) * constraintDistance.y, (hit.collider != null) ? Color.blue : Color.red);
                }
                if (hit.collider != null)
                {
                    sides[i] = true;
                }
            }
        }
        public bool CheckSide(SideDir direction)
        {
            CheckSides();
            return sides[(int)direction];
        }
        public bool PushingAgainstWall(float inputX)
        {
            return inputX > 0.1f && sides[(int)SideDir.right] || inputX < -0.1f && sides[(int)SideDir.left];
        }
        #endregion

        #region MovementCalulations
        /// <summary> Updates groundXSpeed </summary>
        void GroundXMovement(float input)
        {
            groundXSpeed = input * groundMaxXSpeed;
        }
        /// <summary> Updates airXSpeed </summary>
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
        /// <summary> Updates wallYSpeed </summary>
        void WallYMovement(float input)
        {
            wallYSpeed = input * wallMaxYSpeed;
        }

        void ConvertJumpInfo(bool jump, bool jumpCont)
        {
            float timeToKeepJumping = 3f;//the larger this number is the smaller the window to keep jumping
            if (jump && canJump)
            {
                jumpTimeStart = Time.unscaledTime;
                jumpContinuous = true;
                OnJump.Invoke();
            }
            if (jumpCont)
            {
                if (jumpContinuous)
                    jumpMagnitude = Mathf.Clamp((Time.unscaledTime - jumpTimeStart) * timeToKeepJumping, 0, 1);
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
        }

        bool dashing = false;
        [SerializeField] private float dashTime = 0.2f;
        Vector2 dashDir;
        private IEnumerator Dash()
        {
            dashing = true;
            yield return new WaitForSeconds(dashTime);
            dashing = false;
        }

        #endregion
    }
}