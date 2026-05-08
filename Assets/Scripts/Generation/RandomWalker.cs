using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Rendering;

public class RandomWalker : MonoBehaviour
{
    [SerializeField] private SphereCollider sphereCollider;
    [SerializeField] private float minimumRadius;
    [SerializeField] private float maximumRadius;

    [SerializeField] private Vector3 negativeBounds;
    [SerializeField] private Vector3 positiveBounds;

    [SerializeField] private string seed;
    private System.Random rng;

    [SerializeField] private CaveRenderer caveRenderer;
    [SerializeField] private Rigidbody rb;

    private Vector3 destination;
    private float targetRadius;
    private float radiusChangeRate;
    
    private void Start()
    {
        if (seed.Equals("")) seed = UnityEngine.Random.Range(float.MinValue, float.MaxValue).ToString();

        rng = new System.Random(seed.GetHashCode());

        RandomRadius();
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
                caveRenderer.IncreaseVertexSizes();
                Destroy(this.gameObject);
            }
        }
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
        float newX = (float)(rng.NextDouble() * (positiveBounds.x - negativeBounds.x - 2 * sphereCollider.radius) + negativeBounds.x + sphereCollider.radius);
        float newY = (float)(rng.NextDouble() * (positiveBounds.y - negativeBounds.y - 2 * sphereCollider.radius) + negativeBounds.y + sphereCollider.radius);
        float newZ = (float)(rng.NextDouble() * (positiveBounds.z - negativeBounds.z - 2 * sphereCollider.radius) + negativeBounds.z + sphereCollider.radius);
        destination = new Vector3(
             newX,
             newY,
             newZ
            );
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
