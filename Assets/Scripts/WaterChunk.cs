using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class WaterChunk : MonoBehaviour
{
    public (int, int) coords;
    public Mesh Mesh { get; set; }
 
    public void UpdateMesh()
    {
        GetComponent<MeshFilter>().mesh = Mesh;
        GetComponent<MeshCollider>().sharedMesh = Mesh;
    }

    public void CreateMesh()
    {
        Mesh = new Mesh();
    }
}
