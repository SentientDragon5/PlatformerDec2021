using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.Events;

public class PlatformerVFX : MonoBehaviour
{
    //private VisualEffect effect;
    private IPlatformer c;
    public GameObject DashEffect;
    public GameObject Dash2Effect;
    public GameObject JumpEffect;

    public GameObject ClimbEffect;
    public VisualEffect TiredEffect;

    // Start is called before the first frame update
    void Awake()
    {
        //effect = GetComponent < VisualEffect>();
        c = GetComponent<IPlatformer>();

        c.OnDash.AddListener(DashFX);
        c.OnJump.AddListener(JumpFX);
    }

    // Update is called once per frame
    void Update()
    {
        float tiredRate = 0f;
        if(c.Energy < 0.5f)
        {
            if (c.Energy > Mathf.Epsilon)
                tiredRate = 1 / c.Energy;
            else
                tiredRate = 1000;
        }

        TiredEffect.SetFloat("Rate", tiredRate);
        if (c.Climbing && Mathf.Abs(c.Velocity.y) > 0.1f)
        {
            //GameObject vfx = Instantiate(ClimbEffect, transform.position, Quaternion.identity);
        }
    }

    public void DashFX()
    {
        Debug.Log("Dash");
        GameObject vfx = Instantiate(Dash2Effect, transform);
        vfx.GetComponent<VisualEffect>().SetVector2("CurrentVelocity", c.Velocity * 0.5f);
    }

    public void JumpFX()
    {

        Debug.Log("Jump");
        GameObject vfx = Instantiate(JumpEffect, transform.position, Quaternion.identity);
        //vfx.GetComponent<VisualEffect>().SetVector2("CurrentVelocity", c.Velocity * 0.5f);
    }
}
