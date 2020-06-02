using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterGenerator : MonoBehaviour
{
    Mesh mesh;

    void Startt() 
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh  = mesh;
    }

    void Update () 
    {

    }
}
