using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Android;

public class CaveRenderer : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int depth;
    [SerializeField][Range(0f, 1f)] private float fillPercent;

    [SerializeField] private float threshold;
    [SerializeField] private int smoothingIterations;

    private int size;
    private int remainingVertices;

    private GameObject[,,] cells;
    private float[,,] points;

    private List<Vector3> vertices = new();
    private List<int> triangles = new();

    private void Start()
    {
        InitializeVertices();
    }

    private void InitializeVertices()
    {
        cells = new GameObject[width, height, depth];
        points = new float[width + 1, height + 1, depth + 1];

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

                    //Destroy(point.GetComponent<MeshRenderer>());

                    Collider collider = point.GetComponent<Collider>();

                    collider.isTrigger = true;

                    cells[x, y, z] = point;
                    points[x, y, z] = 1f;
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
        SetVerticesToActiveObjects();

        Smooth(smoothingIterations);

        vertices.Clear();
        triangles.Clear(); 

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

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

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

            float voxel = points[
                    x + corner.x,
                    y + corner.y,
                    z + corner.z
                ];

            if (voxel >= threshold)
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

    private void Smooth(int iterations)
    {
        if (iterations == 0) return;

        float[,,] newPoints = new float[points.GetLength(0), points.GetLength(1), points.GetLength(2)];

        for (int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < points.GetLength(0); x++)
            {
                for (int y = 0; y < points.GetLength(1); y++)
                {
                    for (int z = 0; z < points.GetLength(2); z++)
                    {
                        newPoints[x, y, z] = AverageNeighbors(Neighbors(x, y, z));
                    }
                }
            }
        }
        points = newPoints;
    }

    private float[] Neighbors(int xpos, int ypos, int zpos)
    {
        float[] neighbors = new float[27];

        int counter = 0;

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    if ((x == 0 && y == 0 && z == 0) ||
                        xpos + x < 0 || xpos + x >= points.GetLength(0) ||
                        ypos + y < 0 || ypos + y >= points.GetLength(1) ||
                        zpos + z < 0 || zpos + z >= points.GetLength(2))
                    {
                        counter++;
                        continue;
                    }

                    neighbors[counter] = points[xpos + x, ypos + y, zpos + z];
                    counter++;
                }
            }
        }

        return neighbors;
    }

    public Vector3 LowestPoint()
    {
        for(int y = 0; y < points.GetLength(1); y++)
        {
            for(int x = 0; x < points.GetLength(0); x++)
            {
                for(int z = 0; z < points.GetLength(2); z++)
                {
                    if (points[x, y, z] == 0f)
                    {
                        return new Vector3((float)x, (float)y, (float)z);
                    }
                }
            }
        }
        return new Vector3(width, height, depth);
    }

    private float AverageNeighbors(float[] neighbors)
    {
        float sum = 0f;

        for (int i = 0; i < neighbors.Length; i++)
        {
            sum += neighbors[i];
        }

        return sum / (float)neighbors.Length;
    }

    public GameObject[,,] Cells
    {
        get { return cells; }
        set { cells = value; }
    }

    public float[,,] Points
    {
        get { return points; }
    }
}
