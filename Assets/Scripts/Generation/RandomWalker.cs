using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class RandomWalker : MonoBehaviour
{
    [SerializeField] private CaveData caveData;
    [SerializeField] private string seed;

    [SerializeField] private GameManager gameManager;

    [SerializeField] private float maxSectionLength;

    [SerializeField] private float currentRadius;
    [SerializeField] private float minRadius;
    [SerializeField] private float maxRadius;

    [SerializeField] private Vector3 negativeBounds;
    [SerializeField] private Vector3 positiveBounds;

    private GoalBehavior startGoal;
    private GoalBehavior endGoal;
    [SerializeField] private Material goalMaterial;

    public System.Random rng;
    private Vector3 destination;

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

    private void ClearPathPureMath()
    {
        currentRadius = RandomRadius();

        destination = RandomDestination();

        Vector3 direction = (destination - transform.position).normalized;

        if (previousRadius != 0f)
        {
            transform.position += direction * (currentRadius - previousRadius);
        }

        int removedPoints = 0;

        for(int x = 0; x < caveData.Points.GetLength(0); x++)
        {
            for(int y = 0; y < caveData.Points.GetLength(1); y++)
            {
                for(int z = 0; z < caveData.Points.GetLength(2); z++)
                {
                    if(Captured(new Vector3(x, y, z)))
                    {
                        caveData.Points[x, y, z] = 0f;
                        caveData.RemainingVertices--;

                        removedPoints++;
                    }
                }
            }
        }

        transform.position = destination;

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
        float newX = transform.position.x + (float)(rng.NextDouble() * (2f * maxSectionLength) - maxSectionLength);
        float newY = transform.position.y + (float)(rng.NextDouble() * (2f * maxSectionLength) - maxSectionLength);
        float newZ = transform.position.z + (float)(rng.NextDouble() * (2f * maxSectionLength) - maxSectionLength);

        destination = new Vector3(
             newX,
             newY,
             newZ
            );

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

        endGoalObject.transform.localScale = new Vector3(
            currentRadius,
            currentRadius,
            currentRadius
            ) * 2f;

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

    public float MaxLength
    {
        get { return maxSectionLength; }
        set { maxSectionLength = value; }
    }
}
