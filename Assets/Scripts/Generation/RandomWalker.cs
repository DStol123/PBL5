using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Rendering;

public class RandomWalker : MonoBehaviour
{
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private float minimumRadius;
    [SerializeField] private float maximumRadius;

    [SerializeField] private float maximumTunnelLength;

    [SerializeField] private Vector3 negativeBounds;
    [SerializeField] private Vector3 positiveBounds;

    [SerializeField] private string seed;
    private System.Random rng;

    [SerializeField] private CaveData caveRenderer;
    [SerializeField] private Rigidbody rb;

    private GoalBehavior startGoal;
    private GoalBehavior endGoal;
    [SerializeField] private Material goalMaterial;

    private Vector3 destination;
    private float targetRadius;
    private float radiusChangeRate;
    
    private void Start()
    {
        if (seed.Equals("")) seed = UnityEngine.Random.Range(float.MinValue, float.MaxValue).ToString();

        rng = new System.Random(seed.GetHashCode());

        RandomDestination();

        RandomRadius();

        InitializeReturnGoal();
    }

    private void Update()
    {
        MoveWalker();
        ChangeRadius();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object entered walker trigger");
        if (other.gameObject.CompareTag("Cave Vertex"))
        {
            Debug.Log("Cave Vertex Hit");
            caveRenderer.RemainingVertices--;
            Destroy(other.gameObject);

            if (caveRenderer.Filled())
            {
                caveRenderer.DestroyAllPrimitiveCubes();
                caveRenderer.GenerateMesh();

                InitializeEndGoal();

                Destroy(this.gameObject);
            }
        }
    }

    private void InitializeReturnGoal()
    {
        GameObject startGoalObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        startGoalObject.name = "Return Goal";
        startGoal = startGoalObject.AddComponent<GoalBehavior>();

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
        endGoalObject.transform.position = transform.position;
        endGoal = endGoalObject.AddComponent<GoalBehavior>();

        MeshRenderer meshRenderer = endGoalObject.GetComponent<MeshRenderer>();
        meshRenderer.material = goalMaterial;

        endGoal.SphereCollider = endGoalObject.GetComponent<SphereCollider>();

        endGoalObject.transform.localScale = new Vector3(
            sphereCollider.radius,
            sphereCollider.radius,
            sphereCollider.radius
            ) * 2f;

        endGoal.Active = false;
        endGoal.Reached = false;

        endGoal.ReturnGoal = startGoal;
    }

    private Vector3 BoundVector(Vector3 input, Vector3 nBounds, Vector3 pBounds)
    {
        return new Vector3(
            Mathf.Clamp(input.x, nBounds.x, pBounds.x),
            Mathf.Clamp(input.y, nBounds.y, pBounds.y),
            Mathf.Clamp(input.z, nBounds.z, pBounds.z)
            );
    }

    private void RandomDestination()
    {
        float newX = transform.position.x + (float)(rng.NextDouble() * (2f * maximumTunnelLength) - maximumTunnelLength);
        float newY = transform.position.y + (float)(rng.NextDouble() * (2f * maximumTunnelLength) - maximumTunnelLength);
        float newZ = transform.position.z + (float)(rng.NextDouble() * (2f * maximumTunnelLength) - maximumTunnelLength);

        destination = new Vector3(
             newX,
             newY,
             newZ
            );

        destination = BoundVector(destination, negativeBounds + Vector3.one * maximumRadius * 1.5f, positiveBounds - Vector3.one * maximumRadius * 1.5f);
    }

    private void MoveWalker()
    {
        rb.MovePosition(transform.position += (destination - transform.position).normalized * sphereCollider.radius * 0.2f);
        
        if (DestinationReached()) RandomDestination();
    }

    private bool DestinationReached()
    {
        float distanceToDestination = (destination - transform.position).magnitude;
        
        return distanceToDestination <= sphereCollider.radius * 0.5f;
    }

    private void RandomRadius()
    {
        targetRadius = (float)rng.NextDouble() * (maximumRadius - minimumRadius) + minimumRadius;
        radiusChangeRate = (float)rng.NextDouble() * 0.8f;
    }

    private void ChangeRadius()
    {
        sphereCollider.radius = Mathf.Lerp(sphereCollider.radius, targetRadius, radiusChangeRate);

        if (Mathf.Abs(sphereCollider.radius - targetRadius) <= 0.01f) RandomRadius();
    }
}
