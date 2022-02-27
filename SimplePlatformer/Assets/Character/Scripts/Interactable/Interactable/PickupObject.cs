using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character.Interactions
{
    [RequireComponent(typeof(Rigidbody))]
    public class PickupObject : Interactable
    {
        private Rigidbody rb;
        public GameObject Prefab;
        public bool destroyOnPickup = true;

        public override void Interact(Interactor interactor)
        {
            InventoryManager manager = interactor.GetComponent<InventoryManager>();
            manager.PickUp(Prefab);
            //InventoryManager.instance.PickUp(Prefab);
            if (destroyOnPickup)
            {
                Destroy(this.gameObject);
                ItemSpawn itemSpawn = GetComponentInParent<ItemSpawn>();
                //TerrainItemManager itemManager = WorldManager.instance.itemManagers[WorldManager.instance.PositionToTerrainID(Vector2.zero)];//itemManager1;//
                //int index;
                //for(int i=0;i< itemManager.itemSpawns.Count; i++)
                //{
                //    if(itemManager.itemSpawns[i] = itemSpawn)
                //    {
                //        index = i;
                //        itemManager.chunckSave.itemsExist[index] = false;
                //        continue;
                //    }
                //}
            }
        }

        private void Start()
        {
            rb = transform.GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            Prefab = GetComponentInParent<ItemSpawn>().spawnPrefab;
        }

        private void OnCollisionEnter(Collision collision)
        {
            rb.useGravity = true;
        }
    }

}