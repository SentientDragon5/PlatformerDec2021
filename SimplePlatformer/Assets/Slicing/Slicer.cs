using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class Slicer : MonoBehaviour
{

    public string assetPath = "Images/";


    public Texture2D texture;
    public List<Texture2D> textures;
    public Vector2Int size;
    [ContextMenu("Slice")]
    public void Slice()
    {
        List<Texture2D> sliced = new List<Texture2D>();
        sliced.Clear(); 
        int texX = 0;
        int texY = 0;
        int i = 0;
        int MAX = 200;
        while(texY < texture.height && i < MAX)
        {
            sliced.Add(new Texture2D(size.x, size.y));


            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    sliced[i].SetPixel(x, y, texture.GetPixel(x + texX, y + texY));
                }
            }
            sliced[i].Apply();
            texX += size.x;
            if(texture.width <= texX)
            {
                texX = 0;
                texY += size.y;
            }
            /* byte[] bytes = texture.EncodeToPNG();
             var dirPath = Application.dataPath + "Assets/SaveImages/" + texture.name + i + ".png";
             if (!Directory.Exists(dirPath))
             {
                 Directory.CreateDirectory(dirPath);
             }
             File.WriteAllBytes(dirPath, bytes);*/

            string cardPath = "Assets/" + assetPath + texture.name + "_" + i + ".png";
            byte[] bytes = sliced[i].EncodeToPNG();
            System.IO.File.WriteAllBytes(cardPath, bytes);
            //AssetDatabase.ImportAsset(cardPath);
            //TextureImporter ti = (TextureImporter)TextureImporter.GetAtPath(cardPath);
            //ti.textureType = TextureImporterType.Sprite;
            //ti.SaveAndReimport();
            i++;
        }
    }

    public string FlipVName = "_FlipedV";
    [ContextMenu("Flip V")]
    public void FlipV()
    {
        Texture2D o = new Texture2D(texture.width, texture.height);
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                o.SetPixel(x,y,texture.GetPixel(texture.width - x - 1, y));
            }
        }
        string cardPath = "Assets/" + assetPath + texture.name + "" + FlipVName + ".png";
        byte[] bytes = o.EncodeToPNG();
        System.IO.File.WriteAllBytes(cardPath, bytes);
    }
    [ContextMenu("Flip all V")]
    public void FlipAllV()
    {
        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D texture = textures[i];
            Texture2D o = new Texture2D(texture.width, texture.height);
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    o.SetPixel(x, y, texture.GetPixel(texture.width - x - 1, y));
                }
            }
            string cardPath = "Assets/" + assetPath + texture.name + "" + FlipVName + ".png";
            byte[] bytes = o.EncodeToPNG();
            System.IO.File.WriteAllBytes(cardPath, bytes);
        }
    }
    [ContextMenu("Slice all")]
    public void SliceAll()
    {
        for (int i = 0; i < textures.Count; i++)
        {
            texture = textures[i];
            Slice();
        }
    }
}
