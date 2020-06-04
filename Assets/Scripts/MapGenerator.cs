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
    public TerrainChunk terrainChunk;
    public WaterChunk waterChunk;
    public Material chunkMaterial;
    public Material waterMaterial;

    private float maxHeight;
    private float minHeight;
    private (int, int) currentChunkPos;

    private Mesh mesh;
    private Color[] colors;
    private Vector3[] vertices;
    private int[] triangles;
    private Dictionary<(int, int), TerrainChunk> chunks = new Dictionary<(int, int), TerrainChunk>();
    private Dictionary<(int, int), WaterChunk> waters = new Dictionary<(int, int), WaterChunk>();

    void Start()
    {
        player.position = new Vector3(resolution / 2, 10f, resolution / 2);
        waterChunk.GetComponent<MeshRenderer>().material = waterMaterial; 
        currentChunkPos = (-1, -1);
    }

    void Update() 
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        var chunkPos = GetChunkPostion(player.position);

        if (currentChunkPos != chunkPos)
        {
            currentChunkPos = chunkPos;

            foreach (var key in chunks.Keys)
            {
                chunks[key].gameObject.SetActive(false);
                waters[key].gameObject.SetActive(false);
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
                    waters[(x, z)].gameObject.SetActive(true);
                    continue;
                }

                CreateChunk(x, z);
                CreateWater(x, z);
            }
        }
    }

    private void CreateWater(int x, int z) 
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(x, 0, z),
            new Vector3(x + resolution + 1, 0, z),
            new Vector3(x, 0, z + resolution + 1),
            new Vector3(x + resolution + 1, 0, z + resolution + 1),
        };
        
        int[] triangles = new int[] {0, 2, 1, 1, 2, 3};

        Vector2[] uv = new Vector2[] 
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };

        WaterChunk water = Instantiate<WaterChunk>(waterChunk, new Vector3(x / resolution, 0, z / resolution), Quaternion.identity, transform);
        
        water.CreateMesh();

        water.transform.position += new Vector3(0, Mathf.Abs(minHeight - maxHeight) * .359f, 0);      
        
        water.Mesh.vertices = vertices;
        water.Mesh.triangles = triangles;
        water.Mesh.uv = uv;

        water.Mesh.RecalculateNormals();
        water.UpdateMesh();

        waters[(x, z)] = water;
    }

    

    private void CreateChunk(int x, int z)
    {
        TerrainChunk currentChunk = Instantiate<TerrainChunk>(terrainChunk, new Vector3(x / resolution, 0, z / resolution), Quaternion.identity, transform);
        currentChunk.coords = (x, z);
        currentChunk.gameObject.layer = LayerMask.NameToLayer("Land");
        currentChunk.GetComponent<MeshRenderer>().material = chunkMaterial;
        
        minHeight = 0;
        maxHeight = 0;

        vertices = new Vector3[(resolution + 2) * (resolution + 2)];
       
        for (int i = 0, z0 = z; z0 <= z + resolution + 1; z0++)
        {
            for (int x0 = x; x0 <= x + resolution + 1; x0++, i++)
            {
                var y = CalculateHeight(x0, z0);
                
                if (y > maxHeight) maxHeight = y;
                if (y < minHeight) minHeight = y;
                
                vertices[i] = new Vector3(x0, y, z0);
            }
        }

        triangles = new int[(resolution + 1) * (resolution + 1) * 6];

        int vert = 0;
        int tris = 0;
        for (int z0 = 0; z0 < resolution + 1; z0++)
        {
            for (int x0 = 0; x0 < resolution + 1; x0++)
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

        UpdateMesh(currentChunk);
        chunks[(x, z)] = currentChunk;
    }

    private void UpdateMesh(TerrainChunk chunk) 
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

    private (int, int) GetChunkPostion(Vector3 pos)
    {
        return (
            Mathf.FloorToInt(pos.x / resolution) * resolution, 
            Mathf.FloorToInt(pos.z / resolution) * resolution
        );
    }
}
