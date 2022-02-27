using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public LayerMask collisionLayer;
    public float radius;
    public int radiusSubdivisions;
    public Texture2D path;
    public Vector2 direction;
    public Vector2 targetPosition;
    public Vector2Int target;

    void GenerateTexture()
    {
        // create a texture that is the amount of subdivisions wide * 2 + one that we are on.
        // 
        int diameter = radiusSubdivisions + 1 + radiusSubdivisions;
        path = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false, true);
        path.filterMode = FilterMode.Point;
        for (int x = 0; x < diameter; x++)
        {
            for (int y = 0; y < diameter; y++)
            {
                path.SetPixel(x, y, Color.black);
            }
        }

        Vector2 targetPositionLocal = targetPosition - new Vector2(transform.position.x, transform.position.y);
        target = new Vector2Int(Mathf.RoundToInt(targetPositionLocal.x), Mathf.RoundToInt(targetPositionLocal.y));

        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetPositionLocal, radius, collisionLayer);
        if(hit.distance < radius)
        {

            //either hit an obstacle

            //or a player

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