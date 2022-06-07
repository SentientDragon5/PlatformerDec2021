using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    public Platformer4 character;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Die
        Debug.Log("die");
    }
}
