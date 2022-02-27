using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character.Interactions;

namespace Character
{
    [RequireComponent(typeof(Character2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Animator))]
    public class NonPlayerCharacter : MonoBehaviour
    {
        public Vector2 targetPosition = Vector2.zero;
        public bool interact;

        private Character2D character;
        private Interactor interactor;

        #region Trigger
        bool t = false;
        public bool Trigger
        {
            get
            {
                return t;
            }
            set
            {
                t = true;
                StartCoroutine(ResetTrigger());
            }
            
        }
        IEnumerator ResetTrigger()
        {
            yield return new WaitForEndOfFrame();
            t = false;
        }
        #endregion

        private void OnValidate()
        {
            targetPosition = transform.position;
        }

        // Start is called before the first frame update
        void Start()
        {
            character = GetComponent<Character2D>();
            interactor = GetComponent<Interactor>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 position = transform.position;
            Vector2 direction = targetPosition - position;
            if(direction.magnitude > 1)
            {
                direction.Normalize();
            }
            Vector2 input = direction;
            if (interact)
            {
                interactor.Interact();
                interact = false;
            }
            //character.Move(input);
        }
    }

}