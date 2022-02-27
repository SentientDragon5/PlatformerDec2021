using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character.Interactions;

namespace Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Animator))]
    public class UserCharacter : MonoBehaviour
    {
        private Platformer3 character;
        private Interactor interactor;

        bool jump;
        bool climb;
        bool jumpCont;
        bool dash;

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<Platformer3>();
            interactor = GetComponent<Interactor>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E pressed, interacting");
                interactor.Interact();
            }
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Debug.Log(" Climbing");
            }
            climb = Input.GetKey(KeyCode.LeftShift);
            if (Input.GetKeyDown(KeyCode.Space) && !jump)
            {
                jump = true;
            }
            jumpCont = Input.GetKey(KeyCode.Space);
            if(Input.GetKeyDown(KeyCode.V))
            {
                dash = true;
            }
            
            //character.Move(input);
        }
        void FixedUpdate()
        {
            Vector2 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            character.MovePlatformer(input, jump, climb, jumpCont, dash);
            //character.Move(input, jump, climb, jumpCont, dash);
            jump = false;
            dash = false;
        }
    }

}
