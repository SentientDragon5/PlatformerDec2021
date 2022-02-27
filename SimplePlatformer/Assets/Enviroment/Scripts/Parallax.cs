using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Vector3 movementScale = Vector3.one;
    public Vector3 offset;
    public Vector3 timeScale;

    Transform cam;

    void Awake()
    {
        cam = Camera.main.transform;
        offset = transform.position;
    }

    void LateUpdate()
    {
        transform.position = Vector3.Scale(cam.position, movementScale) + offset;
    }
}
