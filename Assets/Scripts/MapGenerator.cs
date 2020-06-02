using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int resolution = 32;
    public int xOffset = 0;
    public int zOffset = 0;
    [Space]
    public float noiseAmp1 = 1f;
    public float noiseScale1 = 1f;
    public float noiseAmp2 = .5f;
    public float noiseScale2 = 2f;
    public float noiseAmp3 = .25f;
    public float noiseScale3 = 4f;
    [Space]
    public Gradient gradient;
    public Transform player;
    public Transform water;
    public Chunk terrainChunk;
    public Material chunkMaterial;
    
    private float maxHeight;
    private float minHeight;
    private (int, int) currentChunkPos;

    private Mesh mesh;
    private Color[] colors;
    private Vector3[] vertices;
    private int[] triangles;
    private Dictionary<(int, int), Chunk> chunks = new Dictionary<(int, int), Chunk>();

    void Start()
    {
        player.position = new Vector3(resolution / 2, 10f, resolution / 2);
        currentChunkPos = (-1, -1);
    }

    void Update() 
    {
        GenerateMap();
    }

    public void GenerateMap() 
    {
        CreateChunks();
    }

    private void CreateChunks()
    {
        var chunkPos = GetChunkPostion(player.position);

        if (currentChunkPos != chunkPos)
        {
            currentChunkPos = chunkPos;

            foreach (var item in chunks)
            {
                chunks[item.Key].gameObject.SetActive(false);
            }

            var chunkCoords = new (int, int)[]{
                currentChunkPos,
                (currentChunkPos.Item1 + resolution, currentChunkPos.Item2),
                (currentChunkPos.Item1 + resolution, currentChunkPos.Item2 + resolution),
                (currentChunkPos.Item1, currentChunkPos.Item2 + resolution),
                (currentChunkPos.Item1 - resolution, currentChunkPos.Item2 + resolution),
                (currentChunkPos.Item1 - resolution, currentChunkPos.Item2),
                (currentChunkPos.Item1 - resolution, currentChunkPos.Item2 - resolution),
                (currentChunkPos.Item1, currentChunkPos.Item2 - resolution),
                (currentChunkPos.Item1 + resolution, currentChunkPos.Item2 - resolution)
            };

            foreach (var (x, z) in chunkCoords)
            {   
                if (chunks.ContainsKey((x, z)))
                {
                    chunks[(x, z)].gameObject.SetActive(true);
                    continue;
                }

                var currentChunk = Instantiate<Chunk>(terrainChunk, new Vector3(x / resolution, 0, z / resolution), Quaternion.identity, transform);
                currentChunk.coords = (x, z);
                currentChunk.gameObject.layer = LayerMask.NameToLayer("Land");
                currentChunk.GetComponent<MeshRenderer>().material = chunkMaterial;
                
                chunks[(x, z)] = currentChunk;

                CreateShape(x, z);
                UpdateMesh(currentChunk);
            }
        }
    }

    private (int, int) GetChunkPostion(Vector3 pos)
    {
        return (
            Mathf.FloorToInt(pos.x / resolution) * resolution, 
            Mathf.FloorToInt(pos.z / resolution) * resolution
        );
    }

    private void CreateShape(int x0, int z0)
    {
        minHeight = 0;
        maxHeight = 0;

        vertices = new Vector3[(resolution + 2) * (resolution + 2)];
       
        for (int i = 0, z = z0; z <= z0 + resolution + 1; z++)
        {
            for (int x = x0; x <= x0 + resolution + 1; x++, i++)
            {
                var y = CalculateHeight(x, z);
                
                if (y > maxHeight) maxHeight = y;
                if (y < minHeight) minHeight = y;
                
                vertices[i] = new Vector3(x, y, z);
            }
        }

        triangles = new int[(resolution + 1) * (resolution + 1) * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < resolution + 1; z++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + resolution + 1 + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + resolution + 1 + 1;
                triangles[tris + 5] = vert + resolution + 2 + 1;
                
                vert++;
                tris += 6;
            }
            vert++;
        }

        colors = new Color[vertices.Length];

        int j = 0;
        foreach (var v in vertices)
        {
            var height = Mathf.InverseLerp(minHeight, maxHeight, v.y);
            colors[j++] = gradient.Evaluate(height);
        }

        // water.position = new Vector3(transform.position.x, Mathf.Abs(minHeight - maxHeight) * .356f, transform.position.z);
    }

    private void UpdateMesh(Chunk chunk) 
    {
        if (chunk.Mesh == null)
            chunk.CreateMesh();

        chunk.Mesh.Clear();

        chunk.Mesh.vertices = vertices;
        chunk.Mesh.triangles = triangles;
        chunk.Mesh.colors = colors;

        chunk.Mesh.RecalculateNormals();

        chunk.UpdateMesh();
    }

    public float CalculateHeight(float x, float z) 
    {
        var x1 = x / resolution * noiseScale1 + xOffset;
        var z1 = z / resolution * noiseScale1 + zOffset;

        var e1 = noiseAmp1 * Mathf.PerlinNoise(x1, z1);

        var x2 = x / resolution * noiseScale2 + xOffset;
        var z2 = z / resolution * noiseScale2 + zOffset;

        var e2 = noiseAmp2 * Mathf.PerlinNoise(x2, z2);;
    
        var x3 = x / resolution * noiseScale3 + xOffset;
        var z3 = z / resolution * noiseScale3 + zOffset;

        var e3 = noiseAmp3 * Mathf.PerlinNoise(x3, z3);;
    
        return e1 + e2 + e3;
    }
}

public static class MiniNoise
{
    public static float Noise (float x, float z, int resolution, float s1, float a1, float s2, float a2, float s3, float a3, int xOff, int zOff) 
    {
        var x1 = x / resolution * s1 + xOff;
        var z1 = z / resolution * s1 + zOff;

        var e1 = a1 * Mathf.PerlinNoise(x1, z1);

        var x2 = x / resolution * s2 + xOff;
        var z2 = z / resolution * s2 + zOff;

        var e2 = a2 * Mathf.PerlinNoise(x2, z2);;
    
        var x3 = x / resolution * s3 + xOff;
        var z3 = z / resolution * s3 + zOff;

        var e3 = a3 * Mathf.PerlinNoise(x3, z3);;
    
        return e1 + e2 + e3;
    }
}
