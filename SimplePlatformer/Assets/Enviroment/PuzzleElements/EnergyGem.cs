using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D),typeof(Animator))]
public class EnergyGem : MonoBehaviour
{
    public bool charged = true;
    [SerializeField] private float rechargeTime = 2f;

    Animator anim;

    private void Awake()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
        anim = GetComponent<Animator>();
        anim.SetBool("Charged", true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.TryGetComponent(out IPlatformer p))
        {
            if (charged)
            {
                p.RechargeAll();
                charged = false;
                StartCoroutine(Charge());
            }
        }
    }

    IEnumerator Charge()
    {
        anim.SetBool("Charged", false);
        yield return new WaitForSeconds(rechargeTime);
        anim.SetBool("Charged", true);
        charged = true;
    }
}
