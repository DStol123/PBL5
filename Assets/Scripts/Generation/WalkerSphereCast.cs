using UnityEngine;
using System.Collections.Generic;

public class WalkerSphereCast : MonoBehaviour
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

    private System.Random rng;
    private Vector3 destination;

    private float previousRadius;

    private void Awake()
    {
        if (seed.Equals("")) seed = UnityEngine.Random.Range(float.MinValue, float.MaxValue).ToString();

        rng = new System.Random(seed.GetHashCode());
    }

    private void Start()
    {
        InitializeReturnGoal();
    }

    private void Update()
    {
        ClearPath();
    }

    private void ClearPath()
    {
        GameObject[] verticesToClear = SphereCastCollection();

        for(int i = 0; i < verticesToClear.Length; i++)
        {
            caveData.RemainingVertices--;
            
            Destroy(verticesToClear[i]);
        }

        Debug.Log("Destroyed: " + verticesToClear.Length + " vertices");

        if(caveData.Filled())
        {
            Finish();
        }
    }

    private void Finish()
    {
        caveData.GenerateMesh();

        caveData.DestroyAllPrimitiveCubes();

        InitializeEndGoal();

        Destroy(this.gameObject);
    }

    private GameObject[] SphereCastCollection()
    {
        currentRadius = RandomRadius();

        destination = RandomDestination();

        float distance = (destination - transform.position).magnitude;

        Vector3 direction = (destination - transform.position).normalized;

        RaycastHit[] hits = Physics.SphereCastAll(transform.position + direction * (previousRadius == 0f ? 0f : (currentRadius - previousRadius)), currentRadius, direction, distance);

        transform.position = destination;

        previousRadius = currentRadius;

        List<GameObject> gos = new List<GameObject>();

        for(int i = 0; i < hits.Length; i++)
        {
            if (!hits[i].transform.gameObject.CompareTag("Cave Vertex")) continue;

            gos.Add(hits[i].transform.gameObject);
        }

        return gos.ToArray();
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

        startGoalObject.transform.position = transform.position + Vector3.up * 3.5f;
        startGoalObject.transform.localScale = new Vector3(
            50f,
            8f,
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
}
