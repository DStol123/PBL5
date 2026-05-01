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

    private Vector3 destination;
    
    private void Start()
    {
        if (seed.Equals("")) seed = UnityEngine.Random.Range(float.MinValue, float.MaxValue).ToString();

        rng = new System.Random(seed.GetHashCode());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Cave Vertex"))
        {
            Destroy(other.gameObject);
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
        transform.LookAt(destination);

        transform.position += (destination - transform.position).normalized * sphereCollider.radius * 0.2f;
    }
}
