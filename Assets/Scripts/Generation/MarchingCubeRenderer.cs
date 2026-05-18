using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class MarchingCubeRenderer : MonoBehaviour
{
    [SerializeField] private CaveData caveData;

    private List<Vector3> vertices;
    private List<int> triangles;

    private MeshFilter meshFilter;

    private void Start()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();

        meshFilter = GetComponent<MeshFilter>();


    }

    public void MarchCubes()
    {
        vertices.Clear();
        triangles.Clear();

        for(int x = 0; x < caveData.Points.GetLength(0) - 1; x++)
        {
            for(int y = 0; y < caveData.Points.GetLength(1) - 1; y++)
            {
                for(int z = 0; z <  caveData.Points.GetLength(2) - 1; z++)
                {
                    float[] cubeCorners = new float[8];

                    for(int i = 0; i < 8; i++)
                    {
                        Vector3Int corner = new Vector3Int(x, y, z) + CubeLookup.Corners[i];
                        cubeCorners[i] = caveData.Points[corner.x, corner.y, corner.z];
                    }

                    MarchCube(new Vector3(x, y, z), ConfigurationIndex(cubeCorners));
                }
            }
        }

        SetMesh();
    }

    private void MarchCube(Vector3 position, int configIndex)
    {
        if (configIndex == 0 || configIndex == 255) return;

        int edgeIndex = 0;

        int[] triangleIndices = new int[3];

        for(int t = 0; t < 5; t++)
        {
            for(int v = 0; v < 3; v++)
            {
                int triTableValue = CubeLookup.Triangles[configIndex, edgeIndex];

                if (triTableValue == -1) return;

                Vector3 edgeStart = position + CubeLookup.Edges[triTableValue, 0];
                Vector3 edgeEnd = position + CubeLookup.Edges[triTableValue, 1];

                Vector3 vertex = (edgeStart + edgeEnd) / 2f;

                vertices.Add(vertex);
                //triangles.Add(vertices.Count - 1);

                triangleIndices[v] = vertices.Count - 1;

                edgeIndex++;
            }
            // /*
            triangles.Add(triangleIndices[0]);
            triangles.Add(triangleIndices[2]);
            triangles.Add(triangleIndices[1]);
            // */
        }
    }

    private int ConfigurationIndex(float[] cubeCorners)
    {
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            if (cubeCorners[i] > caveData.Threshold) configIndex |= 1 << i;
        }

        return configIndex;
    }

    private void SetMesh()
    {
        Mesh mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    public CaveData CaveData
    {
        get { return caveData; }
        set { caveData = value; }
    }
}
