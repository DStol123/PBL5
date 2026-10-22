using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

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

    [SerializeField] ScavengerManager scavengerManager;

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

        caveData.FindPathToBottom();

        Destroy(this.gameObject);
    }

    private void DownwardBias(float bias)
    {
        destination -= Vector3.up * bias;
    }

    private void ClearPathPureMath()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        currentRadius = RandomRadius();

        if (previousNode == null)
        {
            previousNode = nodePlacer.PlaceNode(transform.position);
            previousNode.startingNode = true;

            caveData.allNodes.Add(previousNode);
        }

        previousNode.tunnelEnd = true;
        previousNode.rw = this;

        previousNode.caveData = caveData;

        destination = RandomDestination();

        Node potentialReplacement = ReplacementNode(destination, 10f);

        if (potentialReplacement != null)
        {
            nextNode = potentialReplacement;

            destination = nextNode.transform.position;
        }

        else
        {
            nextNode = nodePlacer.PlaceNode(destination, previousNode);

            nextNode.tunnelEnd = true;

            nextNode.rw = this;

            nextNode.caveData = caveData;

            caveData.allNodes.Add(nextNode);
        }

        Vector3 min = Vector3.Min(transform.position, destination) - Vector3.one * currentRadius;
        Vector3 max = Vector3.Max(transform.position, destination) + Vector3.one * currentRadius;

        HashSet<(Node, Node)> intersectedPairs = new();

        HashSet<(Node, Node)> processedPairs = new();

        sw.Stop();

        Debug.Log($"Loop setup time: {sw.ElapsedMilliseconds} ms");

        for (int x = Mathf.FloorToInt(min.x); x < Mathf.CeilToInt(max.x); x++)
        {
            for (int y = Mathf.FloorToInt(min.y); y < Mathf.CeilToInt(max.y); y++)
            {
                for (int z = Mathf.FloorToInt(min.z); z < Mathf.CeilToInt(max.z); z++)
                {
                    if (x < 0 || x >= caveData.Points.GetLength(0) ||
                        y < 0 || y >= caveData.Points.GetLength(1) ||
                        z < 0 || z >= caveData.Points.GetLength(2)
                        )
                        continue;

                    if (Captured(new Vector3(x, y, z)))
                    {
                        sw.Restart();
                        caveData.AppendVoxelNodes(new Vector3(x, y, z), previousNode);
                        caveData.AppendVoxelNodes(new Vector3(x, y, z), nextNode);
                        sw.Stop();

                        Debug.Log($"Appending Voxel Nodes: {sw.ElapsedMilliseconds} ms");

                        if (caveData.Points[x, y, z] == 0f)
                        {
                            Node[] intersectedNodePair = { caveData.voxelNodes[new Vector3(x, y, z)][0],
                                caveData.voxelNodes[new Vector3(x, y, z)][1] };

                            bool validIntersectionExists = false;

                            Node intersectionNode = ReplacementNode(new Vector3(x, y, z), 10f);

                            if (intersectionNode != null) validIntersectionExists = true;

                            if (!validIntersectionExists && !intersectedPairs.Contains((intersectedNodePair[0], intersectedNodePair[1])))
                            {
                                intersectionNode = nodePlacer.PlaceNode(new Vector3(x, y, z), intersectedNodePair);
                                intersectionNode.AddNode(previousNode);
                                intersectionNode.AddNode(nextNode);

                                intersectionNode.rw = this;

                                caveData.allNodes.Add(intersectionNode);

                                intersectedPairs.Add((intersectedNodePair[0], intersectedNodePair[1]));

                                intersectionNode.caveData = caveData;

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
        Vector3 line = destination - transform.position;

        float t = Vector3.Dot(point - transform.position, line) / line.sqrMagnitude;

        t = Mathf.Clamp01(t);

        Vector3 closest = transform.position + line * t;

        float sqrDistance = (point - closest).sqrMagnitude;

        return sqrDistance <= currentRadius * currentRadius;
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

    public Vector3 BoundVector(Vector3 input, Vector3 nBounds, Vector3 pBounds)
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

    public bool InLineOfSight(Vector3 pos, Node node)
    {
        Vector3 dir = (node.transform.position - pos).normalized;

        float distance = Vector3.Distance(node.transform.position, pos);

        for (float d = 0; d < distance; d += 0.5f)
        {
            Vector3 p = pos + dir * d;

            if (p != BoundVector(p, NegativeBounds, PositiveBounds)) continue;

            if (caveData.Points[
                Mathf.FloorToInt(p.x),
                Mathf.FloorToInt(p.y),
                Mathf.FloorToInt(p.z)] > caveData.Threshold)
            {
                return false;
            }
        }

        return true;
    }

    public Node ReplacementNode(Vector3 pos, float maxDistance)
    {
        float bestDistance = maxDistance;
        Node bestNode = null;

        foreach(Node node in caveData.allNodes)
        {
            if (!(Mathf.Abs((node.transform.position - pos).magnitude) < maxDistance &&
                InLineOfSight(pos, node))
                )
                continue;
                
            if(Mathf.Abs((node.transform.position - pos).magnitude) < bestDistance)
            {
                bestDistance = (node.transform.position - pos).magnitude;
                bestNode = node;
            }
        }

        return bestNode;
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

    public Vector3 NegativeBounds
    {
        get { return  negativeBounds; }
    }

    public Vector3 PositiveBounds
    {
        get { return positiveBounds; }
    }
}
