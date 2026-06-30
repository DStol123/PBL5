using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private RandomWalker rw;

    [SerializeField] private GameObject bubbleEmitter;

    public Dictionary<Vector3, List<Node>> voxelNodes;
    public List<Node> allNodes = new();

    public List<Node> pathToBottom = new List<Node>();

    public Image LoadingBar;

    private float startTime;

    private int size;
    private int remainingVertices;

    private float[,,] points;

    private bool initialized;

    public bool onlyDisplayRoute;

    private void Update()
    {
    }

    public void InitializeVertices()
    {
        if (initialized) return;

        voxelNodes = new Dictionary<Vector3, List<Node>>();

        startTime = Time.time;

        points = new float[width + 1, height + 1, depth + 1];

        size = width * height * depth;
        remainingVertices = size;

        for(int x = 0; x <= width; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for(int z = 0; z <= depth; z++)
                {
                    points[x, y, z] = 1f;
                    voxelNodes.Add(new Vector3(x, y, z), new List<Node>());
                }
            }
        }

        initialized = true;
    }

    private void PlaceBubbleEmitters()
    {
        if (!gameManager.BubbleEmitters) return;

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                for(int z = 0; z < depth; z++)
                {
                    if (!OnFloor(x, y, z)) continue;
                    
                    if(rw.rng.NextDouble() <= gameManager.EmitterDensity)
                    {
                        Instantiate(bubbleEmitter).transform.position = new Vector3(x, y, z);
                    }
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
        set {  remainingVertices = value;}
    }

    
    public void GenerateMesh()
    {
        //Smooth(smoothingIterations);
        ConnectNodes(30f);

        mcRenderer.MarchCubes();

        PlaceBubbleEmitters();

        float timeToLoad = Time.time - startTime;

        Debug.Log("Time to load cave: " + width + " x " + height + " x " + depth + ": " + timeToLoad.ToString("0.00") + " seconds");

        gameManager.UIManager.DisableMenu(3);
        gameManager.UIManager.EnableMenu(4);
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

    public void FindPathToBottom()
    {
        float length = 0;

        pathToBottom = allNodes[0].FindDirectPath();

        for(int i = 0; i < pathToBottom.Count - 1; i++)
        {
            length += pathToBottom[i].connections[pathToBottom[i + 1]];
        }

        gameManager.pathLength = length;
        gameManager.pathNodes = pathToBottom.Count;
    }

    private float[] Neighbors(int xpos, int ypos, int zpos)
    {
        float[] neighbors = new float[27];

        int counter = 0;

        for (int x = -1; x < 2; x++)
        {
            for (int y = 1; y > -2; y--)
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
                        float lowestNodeHeight = height;
                        int lowestIndex = 0;

                        for(int i = 0; i < voxelNodes[new Vector3(x, y, z)].Count; i++)
                        {
                            if (voxelNodes[new Vector3(x, y, z)][i].transform.position.y < lowestNodeHeight)
                            {
                                lowestNodeHeight = voxelNodes[new Vector3(x, y, z)][i].transform.position.y;
                                lowestIndex = i;
                            }
                        }
                        voxelNodes[new Vector3(x, y, z)][lowestIndex].bottomNode = true;

                        return new Vector3((float)x, (float)y, (float)z);
                    }
                }
            }
        }
        return new Vector3(width, height, depth);
    }

    private bool OnFloor(int x, int y, int z)
    {
        if (y <= 0 || y >= height - 1)
            return false;

        if (points[x, y, z] >= threshold)
            return false;

        if (points[x, y - 1, z] < threshold)
            return false;

        if (points[x, y + 1, z] >= threshold)
            return false;

        return true;
    }

    public void ConnectNodes(float maxDistance)
    {
        for(int i = 0; i < allNodes.Count; i++)
        {
            for(int j = 0; j < allNodes.Count; j++)
            {
                if (allNodes[i].InLineOfSight(allNodes[j]) && 
                    Mathf.Abs((allNodes[i].transform.position - allNodes[j].transform.position).magnitude) <= maxDistance) 
                allNodes[i].AddNode(allNodes[j]);
            }
        }
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

    public void AppendVoxelNodes(Vector3 voxel, Node node)
    {
        if (voxel.x < 0 || voxel.x > width ||
            voxel.y < 0 || voxel.y > height ||
            voxel.z < 0 || voxel.z > depth ||
            Mathf.Ceil(voxel.x) != voxel.x || Mathf.Ceil(voxel.y) != voxel.y || Mathf.Ceil(voxel.z) != voxel.z) return;

        if (voxelNodes[voxel].Contains(node)) return;

        voxelNodes[voxel].Add(node);
    }
}
