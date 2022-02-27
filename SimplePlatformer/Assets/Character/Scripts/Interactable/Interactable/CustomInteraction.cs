using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Character.Interactions
{
    public class CustomInteraction : Interactable
    {
        public UnityEvent OnInteraction;
        public override void Interact(Interactor interactor)
        {
            OnInteraction.Invoke();
        }
    }
}