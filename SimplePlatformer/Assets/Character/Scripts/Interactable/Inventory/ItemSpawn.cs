/*

A simple script that respawn's its item when told to.

*/
using UnityEngine;

namespace Character.Interactions
{
    public class ItemSpawn : MonoBehaviour
    {
        public GameObject spawnPrefab;


        public void Respawn()
        {
            if (transform.childCount > 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }
            }
            GameObject child = Instantiate(spawnPrefab, this.transform);
        }
    }
}