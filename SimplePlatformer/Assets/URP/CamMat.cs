//https://www.youtube.com/watch?v=HW8UePVtU5M
using UnityEngine;

[ExecuteAlways]
public class CamMat : MonoBehaviour
{
    public Material mat;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(mat == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        Graphics.Blit(source, destination, mat);
    }
}
