using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderSetter : MonoBehaviour
{
    private IPlatformer c;
    public Material mat;
    public Color[] DashColors = { Color.magenta, Color.red, Color.cyan };

    // Start is called before the first frame update
    void Start()
    {
        c = GetComponent<IPlatformer>();
    }

    // Update is called once per frame
    void Update()
    {
        mat.SetColor("_Color", DashColors[Mathf.Clamp(c.Dashes, 0, 2)]);
        //mat.SetColor("_Color", DashColors[0]);
        //Debug.Log("SET");
        mat.SetFloat("_Energy", Mathf.Clamp(c.Energy, 0, 1));
    }
    [ContextMenu("Reset")]
    private void OnApplicationQuit()
    {
        mat.SetColor("_Color", DashColors[1]);
        mat.SetFloat("_Energy", 1);
    }
}
