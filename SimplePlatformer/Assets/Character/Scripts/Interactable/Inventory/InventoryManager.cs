using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Character.Interactions
{
    public class InventoryManager : MonoBehaviour
    {
        #region Singleton
        public static InventoryManager instance;
        void Awake()
        {
            if (instance == null) instance = this;
            //if (instance != this) Destroy(this);
        }
        #endregion

        public ItemSave inventory;

        public void PickUp(GameObject item)
        {
            for (int i = 0; i < inventory.items.Count; i++)
            {
                Debug.Log(item.name + " i:" + i + " of:" + inventory.items.Count + " is true:" + (item == inventory.items[i].prefab) + "1:" + item.name + " 2:" + inventory.items[i].prefab);
                if (item == inventory.items[i].prefab)
                {
                    inventory.items[i].amount += 1;

                    Debug.Log(inventory.items[i].prefab.name + ": " + inventory.items[i].amount);
                }
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}