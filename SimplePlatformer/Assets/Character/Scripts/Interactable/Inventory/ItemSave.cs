using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character.Interactions
{
    [CreateAssetMenu(fileName = "inventorySave", menuName = "Inventory Save")]
    public class ItemSave : ScriptableObject
    {
        public List<Item> items = new List<Item>();
    }

    [System.Serializable]
    public class Item
    {
        public GameObject prefab;
        public int amount;
    }
}