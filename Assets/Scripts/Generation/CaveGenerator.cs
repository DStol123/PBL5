using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    [SerializeField] private int[] caveDimensions; //1 values for each dimension
    [SerializeField] private string seed;
    [Range(0f, 1f)][SerializeField] private float openPercent;
    [SerializeField] private int smoothingThreshold;
    [SerializeField] private int smoothingIterations;
    private int[,,] cells; //1 represents a filled cell, 0 represents an empty one
    private System.Random rng;
    
    void Awake()
    {
        cells = new int[caveDimensions[0], caveDimensions[1], caveDimensions[2]];

        if (seed.Equals("")) seed = UnityEngine.Random.Range(float.MinValue, float.MaxValue).ToString();
        rng = new System.Random(seed.GetHashCode());
    }

    private void Start()
    {
        InitialFill();
        Smooth(smoothingIterations);
        PlaceCubes();
    }

    private void PlaceCubes()
    {
        if (cells == null) return;

        Debug.Log("Placing Cave Cubes...");

        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for(int y = 0;  y < cells.GetLength(1); y++)
            {
                for(int z = 0; z < cells.GetLength(2); z++)
                {
                    if (cells[x, y, z] == 0) continue;

                    Vector3 pos = new Vector3(-caveDimensions[0] / 2 + x + 0.5f, -caveDimensions[1] / 2 + y + 0.5f, -caveDimensions[2] / 2 + z + 0.5f);
                    
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = pos;
                    cube.transform.SetParent(gameObject.transform);
                }
            }
        }

        Debug.Log("Merging Meshes...");

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int width = cells.GetLength(0);
        int height = cells.GetLength(1);
        int depth = cells.GetLength(2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (cells[x, y, z] == 0) continue;

                    Vector3 pos = new Vector3(x, y, z);

                    AddFace(x, y, z, Vector3.forward, pos, vertices, triangles);
                    AddFace(x, y, z, Vector3.back, pos, vertices, triangles);
                    AddFace(x, y, z, Vector3.left, pos, vertices, triangles);
                    AddFace(x, y, z, Vector3.right, pos, vertices, triangles);
                    AddFace(x, y, z, Vector3.up, pos, vertices, triangles);
                    AddFace(x, y, z, Vector3.down, pos, vertices, triangles);
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        var mf = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();

        mf.mesh = mesh;
    }

    private void InitialFill()
    {
        Debug.Log("Filling Initial Cave Cells...");

        for(int x = 0; x < caveDimensions[0]; x++)
        {
            for(int y = 0; y < caveDimensions[1]; y++)
            {
                for(int z = 0; z < caveDimensions[2]; z++)
                {
                    float fillRNG = (float)rng.NextDouble();

                    if (fillRNG >= openPercent) cells[x, y, z] = 1;
                }
            }
        }
    }

    private void Smooth(int iterations)
    {
        Debug.Log("Smoothing Cave Walls...");

        int[,,] cellsCopy = cells;

        for(int i = 0; i < iterations; i++)
        {
            for (int x = 0; x < caveDimensions[0]; x++)
            {
                for (int y = 0; y < caveDimensions[1]; y++)
                {
                    for (int z = 0; z < caveDimensions[2]; z++)
                    {
                        int[] cell = { x, y, z };

                        if (SurroundingCellCount(cell) < smoothingThreshold)
                        {
                            cellsCopy[x, y, z] = 0;
                        }

                        else if (SurroundingCellCount(cell) > smoothingThreshold)
                        {
                            cellsCopy[x, y, z] = 1;
                        }
                    }
                }
            }
        }

        cells = cellsCopy;
    }

    private int[] GetNeighbors(int[] cell)
    {
        int[] returnCells = new int[26];

        if (cell.Length != 3)
        {
            Debug.LogErrorFormat("Invalid Input for GetNeighbors(). Dimensions don't match.");
            return returnCells;
        }

        int counter = 0;

        for(int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    if ((x == y && y == z && x == 0) ||
                        cell[0] + x >= cells.GetLength(0) || cell[0] + x < 0 ||
                        cell[1] + y >= cells.GetLength(1) || cell[1] + y < 0 ||
                        cell[2] + z >= cells.GetLength(2) || cell[2] + z < 0)
                    {
                        continue;
                    }

                    int targetCell = cells[cell[0] + x, cell[1] + y, cell[2] + z];

                    if (targetCell == 1)
                    {
                        returnCells[counter] = 1;

                        counter++;

                        continue;
                    }
                    returnCells[counter] = 0;
                    counter++;
                }
            }
        }

        return returnCells;
    }

    private int SurroundingCellCount(int[] cell)
    {
        int[] neighbours = GetNeighbors(cell);

        int count = 0;

        for(int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] == 1) count++;
        }

        return count;
    }

    private void AddFace(int x, int y, int z, Vector3 direction, Vector3 pos, List<Vector3> vertices, List<int> triangles)
    {
        int nx = x + (int)direction.x;
        int ny = y + (int)direction.y;
        int nz = z + (int)direction.z;

        if (IsInside(nx, ny, nz) && cells[nx, ny, nz] != 0) return;

        int vIndex = vertices.Count;

        Vector3[] face = GetFaceVertices(pos, direction);

        vertices.AddRange(face);

        triangles.Add(vIndex + 0);
        triangles.Add(vIndex + 1);
        triangles.Add(vIndex + 2);

        triangles.Add(vIndex + 0);
        triangles.Add(vIndex + 2);
        triangles.Add(vIndex + 3);
    }

    private Vector3[] GetFaceVertices(Vector3 pos, Vector3 direction)
    {
        float x = pos.x;
        float y = pos.y;
        float z = pos.z;

        float s = 1f;

        if (direction == Vector3.forward)
            return new Vector3[]
            {
            new Vector3(x, y, z + s),
            new Vector3(x + s, y, z + s),
            new Vector3(x + s, y + s, z + s),
            new Vector3(x, y + s, z + s)
            };

        if (direction == Vector3.back)
            return new Vector3[]
            {
            new Vector3(x + s, y, z),
            new Vector3(x, y, z),
            new Vector3(x, y + s, z),
            new Vector3(x + s, y + s, z)
            };

        if (direction == Vector3.left)
            return new Vector3[]
            {
            new Vector3(x, y, z),
            new Vector3(x, y, z + s),
            new Vector3(x, y + s, z + s),
            new Vector3(x, y + s, z)
            };

        if (direction == Vector3.right)
            return new Vector3[]
            {
            new Vector3(x + s, y, z + s),
            new Vector3(x + s, y, z),
            new Vector3(x + s, y + s, z),
            new Vector3(x + s, y + s, z + s)
            };

        if (direction == Vector3.up)
            return new Vector3[]
            {
            new Vector3(x, y + s, z + s),
            new Vector3(x + s, y + s, z + s),
            new Vector3(x + s, y + s, z),
            new Vector3(x, y + s, z)
            };

        return new Vector3[]
        {
        new Vector3(x, y, z),
        new Vector3(x + s, y, z),
        new Vector3(x + s, y, z + s),
        new Vector3(x, y, z + s)
        };
    }

    bool IsInside(int x, int y, int z)
    {
        return x >= 0 && y >= 0 && z >= 0 &&
               x < cells.GetLength(0) &&
               y < cells.GetLength(1) &&
               z < cells.GetLength(2);
    }
}
