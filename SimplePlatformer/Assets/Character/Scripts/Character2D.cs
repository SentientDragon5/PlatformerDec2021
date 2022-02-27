using UnityEngine;
using System;
using System.Collections.Generic;

namespace Character
{
    public enum ControllerType { Platformer, TopDown};

    [RequireComponent(typeof(CircleCollider2D))]
    public class Character2D : MonoBehaviour
    {
        [Header("Movement")]
        public ControllerType controllerType;

        [Header("Ground Movement")]
        public float groundMaxXSpeed = 12f;
        public float groundMaxXAcceleration = 50;
        public float groundMaxXDeacceleration = 100;
        float groundXSpeed = 0;

        [Header("Air Movement")]
        public float airMaxXSpeed = 12f;
        public float airMaxXAcceleration = 50;
        public float airMaxXDeacceleration = 100;
        float airXSpeed = 0;
        [Header("Jump")]
        public float jumpSpeed = 50f;
        public float gravity = 100;
        public float aimedFallSpeed = 200f;

        //public float speed = 4f;
        Vector2 deltaPos = Vector2.zero;
        Vector2 direction = Vector2.down;

        [Header("Jump Magnitude")]
        public float jumpMagCoeficient = 15;
        public AnimationCurve jumpMagCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        float jumpMag;

        Vector2 velocity;
        Vector2 normal = Vector2.up;

        bool touchingGround;
        public float maxSpeed;

        [Header("Constraint")]
        public LayerMask collideLayers;
        float skinWidth = 0.01f;
        public bool snappingAllowed = true;
        float snapMaxDistance = 0.075f;
        Vector2 snapDeltaPos;
        private Vector2[] contraintDirections = new Vector2[4] { Vector2.right, Vector2.left, Vector2.up, Vector2.down };

        public List<CollisionData> collisions = new List<CollisionData>();

        bool connectedR, connectedL;

        private Animator anim;
        private CircleCollider2D cCollider;
        private SpriteRenderer rend;

        bool lastGround = false;

        void Awake()
        {
            anim = GetComponent<Animator>();
            cCollider = GetComponent<CircleCollider2D>();
            rend = GetComponent<SpriteRenderer>();
        }

        void SetAnimator()
        {
            if (deltaPos.magnitude > 0.1f)
            {
                direction = deltaPos;
            }

            anim.SetFloat("x", direction.x);
            anim.SetFloat("y", direction.y);
            anim.SetFloat("speed", deltaPos.magnitude);
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
        public Vector2 Constrained(Vector2 deltaPos)
        {
            LayerMask mask = collideLayers;
            Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
            float constraintDistance = cCollider.radius * transform.localScale.y + skinWidth;

            connectedR = false;
            connectedL = false;
            for (int i = 0; i < 4; i++)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, contraintDirections[i], constraintDistance, mask);
                Debug.DrawRay(transform.position + collisionOffset, new Vector3(contraintDirections[i].x, contraintDirections[i].y, 0) * constraintDistance, (hit.collider != null) ? Color.blue : Color.red);
                if (hit.collider != null)
                {
                    if (i == 0)//right
                    {
                        deltaPos = new Vector2(Mathf.Clamp(deltaPos.x, -1, 0), deltaPos.y);
                        connectedR = true;
                    }
                    if (i == 1)//left
                    {
                        deltaPos = new Vector2(Mathf.Clamp(deltaPos.x, 0, 1), deltaPos.y);
                        connectedL = true;
                    }
                    if (i == 2)//up
                    {
                        deltaPos = new Vector2(deltaPos.x, Mathf.Clamp(deltaPos.y, -1, 0));
                    }
                    if (i == 3)//down
                    {
                        deltaPos = new Vector2(deltaPos.x, Mathf.Clamp(deltaPos.y, 0, 1));
                    }
                }
            }

            return deltaPos;
        }

        public void MovePlatformer(Vector2 input, bool jump, bool climb, bool jumpCont)
        {
            jumpMag = ConvertJumpInfo(jump, jumpCont);

            //initialization
            SnapTo(out normal, out snapDeltaPos, out touchingGround);
            deltaPos = snapDeltaPos = Vector2.zero;
            Vector3 pos = transform.position;
            //input
            //deltaPos = deltaPos + (Vector2)(Quaternion.Euler(0, 0, Vector2.Dot(Vector2.down, normal)) * input);
            if(climb)//climbing
            {

            }
            else
            {
                deltaPos.y = 0;
            }

            if (deltaPos.magnitude > 1)
                deltaPos.Normalize();

            //Physics
            if (jump & touchingGround)
            {
                velocity += normal * jumpSpeed;//apply jump at the normal of the ground;
                touchingGround = false;
                airXSpeed = groundXSpeed;
            }
            else
            {
                
            }
            if (climb)
            {
                deltaPos.y += input.y;
            }
            else if (!touchingGround)
            {
                LayerMask mask = collideLayers;
                Vector3 collisionOffset = new Vector3(cCollider.offset.x, cCollider.offset.y, 0f);
                float constraintDistance = cCollider.radius * transform.localScale.y + skinWidth;
                RaycastHit2D hit = Physics2D.Raycast(transform.position + collisionOffset, Vector2.up, constraintDistance, mask);
                if (hit.collider != null)
                    velocity.y = Mathf.Min(velocity.y, 0);

                velocity += Vector2.up * Mathf.Lerp(jumpSpeed, -Mathf.Abs(gravity), jumpMagCurve.Evaluate(jumpMag)) * Time.deltaTime;
                velocity += Vector2.up * Mathf.Clamp(input.y, -1, 0) * aimedFallSpeed * Time.deltaTime;                
            }
            else
            {
                velocity = new Vector2(velocity.x * Mathf.Lerp(1, 0, normal.x), velocity.y * Mathf.Lerp(1, 0, normal.y));
            }
            //velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
            

            //x acceleration
            //This is so that the velocities are consistant
            //if (lastGround && !touchingGround)
            //    airXSpeed = groundXSpeed;
            //if (touchingGround && !lastGround)
            //    groundXSpeed = airXSpeed;
            //lastGround = touchingGround;
            
            groundXSpeed = velocity.x;
            airXSpeed = velocity.x;
            float threshold = 0.1f; // should be positive
            //add input
            if (input.x > 0.1f)
            {
                if (groundXSpeed < -threshold)
                    groundXSpeed += groundMaxXDeacceleration * Time.deltaTime;
                else// if (groundXSpeed > 0)
                    groundXSpeed += groundMaxXAcceleration * Time.deltaTime;

                if (airXSpeed < -threshold)
                    airXSpeed += airMaxXDeacceleration * Time.deltaTime;
                else// if(airXSpeed > 0)
                    airXSpeed += airMaxXAcceleration * Time.deltaTime;

            }
            else if (input.x < -0.1f)
            {
                if(groundXSpeed > threshold)
                    groundXSpeed -= groundMaxXDeacceleration * Time.deltaTime;
                else //if (groundXSpeed < 0)
                    groundXSpeed -= groundMaxXAcceleration * Time.deltaTime;

                if (airXSpeed > threshold)
                    airXSpeed -= airMaxXDeacceleration * Time.deltaTime;
                else// if (airXSpeed < 0)
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
            airXSpeed = Mathf.Clamp(airXSpeed, -airMaxXSpeed, airMaxXSpeed);
            //float deltaX = deltaPos.x * Time.deltaTime * (touchingGround ? groundXSpeed : airXSpeed);
            //float deltaY = deltaPos.y * Time.deltaTime * speed;
            //deltaPos = new Vector2(deltaX, deltaY);
            velocity.x = (touchingGround ? groundXSpeed : airXSpeed);
            //velocity.x = groundXSpeed;
            deltaPos += velocity * Time.deltaTime;

            //Constraint
            deltaPos = Constrained(deltaPos);

            //Animation
            SetAnimator(jump);

            //Apply
            
            deltaPos += snapDeltaPos;
            pos += new Vector3(deltaPos.x, deltaPos.y, 0);
            transform.position = pos;
        }

        enum SnapType { none, ground, wall};
        void SnapTo(out Vector2 normal, out Vector2 deltaPos, out bool touchingGround)
        {
            LayerMask mask = collideLayers;

            ContactFilter2D filter = new ContactFilter2D();
            filter.useLayerMask = true;
            filter.layerMask = mask;

            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            Physics2D.CircleCast((Vector2)transform.position, cCollider.radius + skinWidth, Vector2.down, filter, hits, 0);
            collisions.Clear();
            foreach(RaycastHit2D r in hits)
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
            if(Vector2.Distance(hit.point, (Vector2)transform.position) <= distance)
                return;

            //Snap right
            hit = Physics2D.Raycast(transform.position + collisionOffset, Vector2.right, distance, mask);
            Debug.DrawRay(transform.position + collisionOffset, Vector3.right * distance, (hit.collider != null) ? Color.blue : Color.red);
            normal = hit.normal;
            snapPoint = Physics2D.Raycast(transform.position + collisionOffset, -normal, distance, mask);
            touchingGround = hit.collider != null;
            deltaPos = -snapPoint.point + new Vector2(transform.position.x, transform.position.y);
            if (Vector2.Distance(hit.point, (Vector2)transform.position) <= distance)
                return;

            //Snap left
            hit = Physics2D.Raycast(transform.position + collisionOffset, Vector2.down, distance, mask);
            Debug.DrawRay(transform.position + collisionOffset, Vector3.down * distance, (hit.collider != null) ? Color.blue : Color.red);
            normal = hit.normal;
            snapPoint = Physics2D.Raycast(transform.position + collisionOffset, -normal, distance, mask);
            touchingGround = hit.collider != null;
            deltaPos = -snapPoint.point + new Vector2(transform.position.x, transform.position.y);
        }

        bool jumpContinuous;
        float jumpMagnitude;

        float jumpStart;

        float ConvertJumpInfo(bool jump, bool jumpCont)
        {
            float timeToKeepJumping = 3f;//the larger this number is the smaller the window to keep jumping
            if (jump && touchingGround)
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
}


[System.Serializable]
public class CollisionData
{
    public float dotProduct;
    public Vector2 normal;
    public Collider2D collider;
    public CollisionData(Vector2 normal, Collider2D collider)
    {
        this.normal = normal;
        this.collider = collider;
    }

}