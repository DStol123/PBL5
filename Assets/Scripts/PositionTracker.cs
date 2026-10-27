using NUnit.Framework;
using UnityEngine;

public class PositionTracker : MonoBehaviour
{
    public CaveData CaveData;
    [SerializeField] private Node currentNode;

    private void FixedUpdate()
    {
        if (CaveData == null || !CaveData.Generated) return;
        
        ClosestNode();
    }

    private Node ClosestNode()
    {
        if (CaveData == null || CaveData.activeNodes == null || CaveData.activeNodes.Count == 0) return null;

        float closestDistance = float.MaxValue;
        Node closestNode = null;

        foreach(Node node in CaveData.activeNodes)
        {
            float distance = Mathf.Abs((node.transform.position - transform.position).magnitude);

            if (distance > closestDistance) continue;

            if (!node.InLineOfSightPhysics(transform.position)) continue;

            closestDistance = distance;

            closestNode = node;
        }

        if (closestNode != currentNode && closestNode != null) closestNode.timesVisited++;

        currentNode = closestNode;

        return closestNode;
    }
}
