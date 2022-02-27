using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character.Interactions;

namespace Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerCharacter : MonoBehaviour
    {
        private IPlatformer character;
        private Interactor interactor;

        bool jump;
        bool climb;
        bool jumpCont;
        bool dash;

        void Awake()
        {
            character = GetComponent<IPlatformer>();
            interactor = GetComponent<Interactor>();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E pressed, interacting");
                interactor.Interact();
            }
            climb = Input.GetKey(KeyCode.LeftShift);
            if (Input.GetKeyDown(KeyCode.Space) && !jump)
            {
                jump = true;
            }
            jumpCont = Input.GetKey(KeyCode.Space);
            if (Input.GetKeyDown(KeyCode.V))
            {
                dash = true;
            }
        }
        void FixedUpdate()
        {
            Vector2 input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            character.Move(input, jump, climb, jumpCont, dash);
            jump = false;
            dash = false;

        }
    }

}
