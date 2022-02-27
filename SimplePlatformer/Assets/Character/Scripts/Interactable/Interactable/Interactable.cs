using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Character.Interactions.InteractableExtras;

namespace Character.Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        public float interactionRadius = 1f;
        public Vector3 offset = Vector3.zero;

        public abstract void Interact(Interactor interactor);

        public virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(offset + transform.position, interactionRadius);
            Gizmos.color = Color.Lerp(Color.yellow, Color.red, 0.3f);
            Gizmos.DrawWireSphere(offset + transform.position, 0.01f);
        }
    }
}
namespace Character.Interactions.InteractableExtras
{
    [System.Serializable]
    public class TriggerZone
    {
        public float interactionRadius;
        public Vector3 interactionOffset;
        public UnityEvent OnInteraction;

        public TriggerZone(float radius, Vector3 offset)
        {
            interactionRadius = radius;
            interactionOffset = offset;
        }
    }
}