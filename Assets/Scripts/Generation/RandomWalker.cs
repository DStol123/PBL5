using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class RandomWalker : MonoBehaviour
{
    [SerializeField] private CaveData caveData;
    [SerializeField] private string seed;

    [SerializeField] private GameManager gameManager;

    [SerializeField] private float maxSectionLength;
    public float minSectionLength;

    [SerializeField] private float currentRadius;
    [SerializeField] private float minRadius;
    [SerializeField] private float maxRadius;

    [SerializeField] private Vector3 negativeBounds;
    [SerializeField] private Vector3 positiveBounds;

    private GoalBehavior startGoal;
    private GoalBehavior endGoal;
    [SerializeField] private Material goalMaterial;

    [SerializeField] private GameObject enclosure;
    [SerializeField] private GameObject waterPlane;

    public System.Random rng;
    private Vector3 destination;

    private NodePlacer nodePlacer;
    private Node previousNode;
    private Node nextNode;

    private float previousRadius;

    private bool initialized;

    private void Update()
    {
        if (!initialized) return;

        ClearPathPureMath();
    }

    public void Initialize()
    {
        if (initialized) return;

        if (nodePlacer == null) nodePlacer = gameObject.AddComponent<NodePlacer>();

        if (seed.Equals("")) seed = UnityEngine.Random.Range(float.MinValue, float.MaxValue).ToString();

        rng = new System.Random(seed.GetHashCode());

        transform.position = new Vector3(caveData.Dimensions.x / 2f, caveData.Dimensions.y + 2f, caveData.Dimensions.z / 2f);

        negativeBounds = Vector3.zero;
        positiveBounds = caveData.Dimensions;

        InitializeReturnGoal();

        initialized = true;
    }

    private void Finish()
    {
        caveData.GenerateMesh();

        InitializeEndGoal();

        Destroy(this.gameObject);
    }

    private void DownwardBias(float bias)
    {
        destination -= Vector3.up * bias;
    }

    private void ClearPathPureMath()
    {
        currentRadius = RandomRadius();

        if (previousNode == null)
        {
            previousNode = nodePlacer.PlaceNode(transform.position);
            previousNode.startingNode = true;
        }

        else
        {
            previousNode = nodePlacer.PlaceNode(transform.position, previousNode);
        }

        destination = RandomDestination();

        nextNode = nodePlacer.PlaceNode(destination, previousNode);

        HashSet<(Node, Node)> intersectedPairs = new();

        for (int x = 0; x < caveData.Points.GetLength(0); x++)
        {
            for (int y = 0; y < caveData.Points.GetLength(1); y++)
            {
                for (int z = 0; z < caveData.Points.GetLength(2); z++)
                {
                    if (Captured(new Vector3(x, y, z)))
                    {
                        caveData.AppendVoxelNodes(new Vector3(x, y, z), previousNode);
                        caveData.AppendVoxelNodes(new Vector3(x, y, z), nextNode);

                        if (caveData.Points[x, y, z] == 0f)
                        {
                            Node[] intersectedNodePair = { caveData.voxelNodes[new Vector3(x, y, z)][0],
                                caveData.voxelNodes[new Vector3(x, y, z)][1] };

                            if (!intersectedPairs.Contains((intersectedNodePair[0], intersectedNodePair[1])))
                            {
                                Node intersectionNode = nodePlacer.PlaceNode(new Vector3(x, y, z), intersectedNodePair);
                                intersectionNode.AddNode(previousNode);
                                intersectionNode.AddNode(nextNode);

                                intersectedPairs.Add((intersectedNodePair[0], intersectedNodePair[1]));

                                caveData.AppendVoxelNodes(new Vector3(x, y, z), intersectionNode);
                            }

                            continue;
                        }

                        caveData.Points[x, y, z] = 0f;
                        caveData.RemainingVertices--;
                    }
                }
            }
        }

        transform.position = destination;

        previousNode = nextNode;

        previousRadius = currentRadius;

        Debug.Log("Cave Generation " + caveData.PercentComplete * 100f + "% Complete");

        if (caveData.Filled()) Finish();
    }

    private bool Captured(Vector3 point)
    {
        Vector3 tunnelLine = destination - transform.position;

        Vector3 lineToPoint = point - transform.position;

        float distanceToPoint = lineToPoint.magnitude;

        float angle = Vector3.Angle(tunnelLine, lineToPoint) * Mathf.Deg2Rad;

        return (Mathf.Sin(angle) * distanceToPoint <= currentRadius) && (Mathf.Cos(angle) * distanceToPoint <= tunnelLine.magnitude) && Vector3.Dot(lineToPoint, tunnelLine) >= 0;
    }


    private Vector3 RandomDestination()
    {
        float newX = transform.position.x + ((float)(rng.NextDouble() - 0.5f) * 2f * (maxSectionLength - minSectionLength));
        float newY = transform.position.y + ((float)(rng.NextDouble() - 0.5f) * 2f * (maxSectionLength - minSectionLength));
        float newZ = transform.position.z + ((float)(rng.NextDouble() - 0.5f) * 2f * (maxSectionLength - minSectionLength));

        destination = new Vector3(
             newX + minSectionLength * Mathf.Abs(newX - transform.position.x) / (newX - transform.position.x),
             newY + minSectionLength * Mathf.Abs(newY - transform.position.y) / (newY - transform.position.y),
             newZ + minSectionLength * Mathf.Abs(newZ - transform.position.z) / (newZ - transform.position.z)
            );

        DownwardBias(0.15f);

        destination = BoundVector(destination, negativeBounds + Vector3.one * maxRadius * 2f, positiveBounds - Vector3.one * maxRadius * 2f);

        return destination;
    }

    private float RandomRadius()
    {
        return (float)rng.NextDouble() * (maxRadius - minRadius) + minRadius;
    }

    private Vector3 BoundVector(Vector3 input, Vector3 nBounds, Vector3 pBounds)
    {
        return new Vector3(
            Mathf.Clamp(input.x, nBounds.x, pBounds.x),
            Mathf.Clamp(input.y, nBounds.y, pBounds.y),
            Mathf.Clamp(input.z, nBounds.z, pBounds.z)
            );
    }

    private void InitializeReturnGoal()
    {
        GameObject en = Instantiate(enclosure);
        en.transform.position = transform.position - Vector3.up * 2f;

        GameObject wp = Instantiate(waterPlane);
        wp.transform.position = transform.position - Vector3.up;

        GameObject startGoalObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        startGoalObject.name = "Return Goal";
        startGoal = startGoalObject.AddComponent<GoalBehavior>();

        startGoal.GameManager = gameManager;

        startGoalObject.transform.position = transform.position + Vector3.up * 3f;
        startGoalObject.transform.localScale = new Vector3(
            50f,
            15f,
            50f
            );

        BoxCollider bc = startGoalObject.GetComponent<BoxCollider>();
        bc.isTrigger = true;

        MeshRenderer meshRenderer = startGoalObject.GetComponent<MeshRenderer>();
        Destroy(meshRenderer);

        startGoal.Active = false;
        startGoal.Reached = false;
    }

    private void InitializeEndGoal()
    {
        if (endGoal != null) return;

        GameObject endGoalObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        endGoalObject.name = "End Goal";
        endGoalObject.transform.position = caveData.LowestPoint();
        endGoal = endGoalObject.AddComponent<GoalBehavior>();

        endGoal.GameManager = gameManager;

        MeshRenderer meshRenderer = endGoalObject.GetComponent<MeshRenderer>();
        meshRenderer.material = goalMaterial;

        endGoal.SphereCollider = endGoalObject.GetComponent<SphereCollider>();

        endGoalObject.transform.localScale = Vector3.one * 2f;

        endGoal.Active = false;
        endGoal.Reached = false;

        endGoal.ReturnGoal = startGoal;

        startGoal.EndGoal = endGoal;
    }

    public string Seed
    {
        get { return seed; }
        set { seed = value; }
    }

    public GameManager GameManager
    {
        get { return gameManager; }
        set { gameManager = value; }
    }

    public float MaxRadius
    {
        get { return maxRadius; }
        set { maxRadius = value; }
    }

    public float MinRadius
    {
        get { return minRadius; }
        set { minRadius = value; }
    }

    public float MaxLength
    {
        get { return maxSectionLength; }
        set { maxSectionLength = value; }
    }
}
