using UnityEngine;
using System.Collections.Generic;

public class CaveRenderer : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int depth;

    private int size;

    private GameObject[,,] cells;

    private void InitializeVertices()
    {
        size = width * height * depth;
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for(int z = 0; z < depth; z++)
                {
                    GameObject point = new GameObject();
                    point.transform.position = new Vector3(x, y, z);
                    point.tag = "Cave Vertex";

                    cells[x, y, z] = point;
                }
            }
        }
    }
}
