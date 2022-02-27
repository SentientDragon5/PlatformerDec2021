using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Character.Interactions
{
    [RequireComponent(typeof(UserCharacter))]
    public class Interactor : MonoBehaviour
    {
        [Header("Settings")]
        public Vector3 offset = Vector3.zero;
        public float interactionRadius = 1f;

        [Header("Current Nearby Interactables")]
        public List<Interactable> interactionQueue = new List<Interactable>();

        /// <summary>
        /// Call this to update the interaction Queue.
        /// </summary>
        public void CheckForInteractables()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position + offset, interactionRadius);
            interactionQueue.Clear();
            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent<Interactable>(out Interactable interactable))
                {
                    interactionQueue.Add(interactable);
                }
            }
            interactionQueue = interactionQueue.OrderBy(i => Vector3.Distance(this.transform.position, i.transform.position)).ToList();//using Linq
        }
        /// <summary>
        /// Call this to interact with the nearest object.
        /// </summary>
        public void Interact()
        {
            CheckForInteractables();

            if (interactionQueue.Count > 0)
            {
                interactionQueue[0].Interact(this);
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(offset + transform.position, interactionRadius);
            Gizmos.color = Color.Lerp(Color.yellow, Color.red, 0.3f);
            Gizmos.DrawWireSphere(offset + transform.position, 0.01f);
        }
    }
}
