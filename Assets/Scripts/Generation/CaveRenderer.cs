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

    private void Start()
    {
        InitializeVertices();
    }

    private void InitializeVertices()
    {
        cells = new GameObject[width, height, depth];
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
                }
            }
        }
    }

    private GameObject[] Neighbors(int xpos, int ypos, int zpos)
    {
        GameObject[] neighbors = new GameObject[27];

        int counter = 0;

        for(int x = -1; x < 2; x++)
        {
            for(int y = -1; y < 2; y++)
            {
                for(int z = -1; z < 2; z++)
                {
                    if((x==0 && y==0 && z==0) ||
                        xpos + x < 0 || xpos + x >= cells.GetLength(0) ||
                        ypos + y < 0 || ypos + y >= cells.GetLength(1) ||
                        zpos + z < 0 || zpos + z >= cells.GetLength(2))
                    {
                        counter++;
                        continue;
                    }
                    neighbors[counter] = cells[x, y, z];
                    counter++;
                }
            }
        }

        return neighbors;
    }

    private int NeighborCount(GameObject[] neighbors)
    {
        int count = 0;

        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] != null) count++;
        }

        return count;
    }

    private bool Contains(GameObject[,,] cells, GameObject gameObjectInput)
    {
        for (int x = 0; x < cells.GetLength(0); x++)
        {
            for(int y = 0; y < cells.GetLength(1); y++)
            {
                for(int z = 0; z < cells.GetLength(2); z++)
                {
                    if (gameObjectInput.Equals(cells[x, y, z])) return true;
                }
            }
        }

        return false;
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
}
