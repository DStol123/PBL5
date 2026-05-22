using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem.Android;

public class CaveData : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int depth;
    [SerializeField][Range(0f, 1f)] private float fillPercent;

    [SerializeField] private float threshold;
    [SerializeField] private int smoothingIterations;

    [SerializeField] private MarchingCubeRenderer mcRenderer;
    [SerializeField] private GameManager gameManager;

    public Image LoadingBar;

    private float startTime;

    private int size;
    private int remainingVertices;

    private float[,,] points;

    private bool initialized;

    private void Update()
    {
    }

    public void InitializeVertices()
    {
        if (initialized) return;

        startTime = Time.time;

        points = new float[width + 1, height + 1, depth + 1];

        size = width * height * depth;
        remainingVertices = size;

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for(int z = 0; z < depth; z++)
                {
                    points[x, y, z] = 1f;
                }
            }
        }

        initialized = true;
    }

    public bool Filled()
    {
        return (float)remainingVertices / (float)size <= fillPercent;
    }    

    public int RemainingVertices
    {
        get { return remainingVertices; }
        set {  remainingVertices = value;}
    }

    
    public void GenerateMesh()
    {
        Smooth(smoothingIterations);

        mcRenderer.MarchCubes();

        float timeToLoad = Time.time - startTime;

        Debug.Log("Time to load cave: " + width + " x " + height + " x " + depth + ": " + timeToLoad.ToString("0.00") + " seconds");

        gameManager.UIManager.DisableMenu(3);
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

    public float[,,] Points
    {
        get { return points; }
        set { points = value; }
    }

    public float Threshold
    {
        get { return threshold; } 
    }

    public float PercentComplete
    {
        get {
            float ratio = (float)remainingVertices / (float)size;

            float value =  Mathf.Clamp01((1f - ratio) / (1f - fillPercent));

            LoadingBar.fillAmount = value;

            return value;
        }
    }

    public Vector3 Dimensions
    {
        get { return new Vector3(width, height, depth); }
        set { 
            width = Mathf.CeilToInt(value.x);
            height = Mathf.CeilToInt(value.y);
            depth = Mathf.CeilToInt(value.z);
        }
    }

    public GameManager GameManager
    {
        get { return gameManager; }
        set { gameManager = value; }
    }
}
