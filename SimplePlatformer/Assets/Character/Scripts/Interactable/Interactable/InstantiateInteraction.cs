using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character.Interactions
{
    public class InstantiateInteraction : Interactable
    {
        public GameObject prefab;
        public override void Interact(Interactor interactor)
        {
            Debug.Log("Interacted!");
            Instantiate(prefab, transform);
        }
    }
}