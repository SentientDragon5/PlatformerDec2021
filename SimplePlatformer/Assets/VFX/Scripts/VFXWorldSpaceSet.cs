using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class VFXWorldSpaceSet : MonoBehaviour
{
    private VisualEffect vfx;
    public string parameter = "Pos";

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        vfx.SetVector3(parameter, transform.position);
    }
    private void OnValidate()
    {
        Awake();
    }
    private void FixedUpdate()
    {
        vfx.SetVector3(parameter, transform.position);
    }
}
