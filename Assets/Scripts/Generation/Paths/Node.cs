using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    public Dictionary<Node, float> connections;
    public bool startingNode;
    public bool bottomNode;

    private void Awake()
    {
        connections = new Dictionary<Node, float> { };
    }

    public void AddNode(Node node)
    {
        connections.Add(node, Mathf.Abs((node.transform.position - transform.position).magnitude));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.6f);

        Gizmos.color = Color.turquoise;

        foreach(Node node in connections.Keys)
        {
            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}
