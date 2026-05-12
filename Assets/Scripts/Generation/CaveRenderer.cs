using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Android;

public class CaveRenderer : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int depth;
    [SerializeField][Range(0f, 1f)] private float fillPercent; 

    private int size;
    private int remainingVertices;

    private GameObject[,,] cells;
    private int[,,] points;

    private List<Vector3> vertices = new();
    private List<int> triangles = new();

    private void Start()
    {
        InitializeVertices();
    }

    private void InitializeVertices()
    {
        cells = new GameObject[width, height, depth];
        points = new int[width + 1, height + 1, depth + 1];

        size = width * height * depth;
        remainingVertices = size;

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for(int z = 0; z < depth; z++)
                {
                    //GameObject point = new GameObject("Cave Vertex: " + x + ", " + y + ", " + z);
                    GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    
                    point.transform.position = new Vector3(x, y, z);
                    point.tag = "Cave Vertex";
                    point.transform.localScale = Vector3.one * 0.1f;

                    Collider collider = point.GetComponent<Collider>();

                    collider.isTrigger = true;

                    cells[x, y, z] = point;
                    points[x, y, z] = 1;
                }
            }
        }
    }

    public bool Filled()
    {
        return (float)remainingVertices / (float)size <= fillPercent;
    }    

    public int RemainingVertices
    {
        get { return remainingVertices; }
        set {  remainingVertices = value; }
    }

    public void IncreaseVertexSizes()
    {
        for(int x = 0; x < cells.GetLength(0); x++)
        {
            for(int y = 0; y < cells.GetLength(1); y++)
            {
                for(int z = 0; z < cells.GetLength(2); z++)
                {
                    GameObject point = cells[x, y, z];
                    if (point == null) continue;

                    point.transform.localScale = Vector3.one;
                    point.GetComponent<Collider>().isTrigger = false;
                }
            }
        }
    }

    public void SetVerticesToActiveObjects()
    {
        for(int x = 0; x < cells.GetLength(0); x++)
        {
            for(int y = 0;y < cells.GetLength(1); y++)
            {
                for(int z = 0;z < cells.GetLength(2); z++)
                {
                    if(cells[x, y, z] == null)
                    {
                        points[x, y, z] = 0;
                        continue;
                    }

                    points[x, y, z] = 1;
                }
            }
        }
    }

    
    public void GenerateMesh()
    {
        vertices.Clear();
        triangles.Clear();

        SetVerticesToActiveObjects();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    MarchCube(x, y, z);
                }
            }
        }

        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void MarchCube(int x, int y, int z)
    {
        int config = 0;

        for (int i = 0; i < 8; i++)
        {
            Vector3Int corner = CubeLookup.EdgeVertices[i];

            int voxel = points[
                    x + corner.x,
                    y + corner.y,
                    z + corner.z
                ];

            if (voxel == 1)
            {
                config |= 1 << i;
            }
        }

        if (config == 0 || config == 255) return;

        CreateTriangles(config, x, y, z);
    }

    private void CreateTriangles(int config, int x, int y, int z)
    {
        int[] edgeIndices = CubeLookup.TriangleConnectionTable[config];

        for (int i = 0; i < edgeIndices.Length; i += 3)
        {
            if (edgeIndices[i] == -1) break;

            int vertIndex = vertices.Count;

            for (int t = 0; t < 3; t++)
            {
                int edge = edgeIndices[i + t];

                Vector3 vertex = new Vector3(x, y, z) + CubeLookup.marchVertices[edge];

                vertices.Add(vertex);
            }

            triangles.Add(vertIndex);
            triangles.Add(vertIndex + 1);
            triangles.Add(vertIndex + 2);
        }
    }

    public void DestroyAllPrimitiveCubes()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Destroy(cells[x, y, z]);
                }
            }
        }
    }
}
