using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Analytics;

public class Node : MonoBehaviour
{
    public Dictionary<Node, float> connections;

    public bool startingNode;
    public bool bottomNode;
    public bool onPath;

    public RandomWalker rw;

    public CaveData caveData;

    public bool tunnelEnd;

    private void Awake()
    {
        connections = new Dictionary<Node, float> { };
    }

    public void AddNode(Node node)
    {
        if (connections.ContainsKey(node)) return;

        connections.Add(node, Mathf.Abs((node.transform.position - transform.position).magnitude));
    }

    public bool InLineOfSight(Node node)
    {
        Vector3 dir = (node.transform.position - transform.position).normalized;

        float distance = Vector3.Distance(node.transform.position, transform.position);

        for (float d = 0; d < distance; d += 0.5f)
        {
            Vector3 p = transform.position + dir * d;

            if (p != rw.BoundVector(p, rw.NegativeBounds, rw.PositiveBounds)) continue;

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

    public List<Node> AStar()
    {
        List<Node> path = new List<Node>();
        path.Add(this);

        List<Node> explored = new List<Node>();
        explored.Add(this);

        onPath = true;

        bool found = false;

        Node currentNode = this;

        Node destination = null;

        float totalScore = 0f;

        foreach (Node node in caveData.allNodes)
        {
            if (node.bottomNode)
            {
                destination = node;
                break;
            }
        }

        if (destination == null) return null;

        while(!found)
        {
            float lowestScore = float.MaxValue;
            Node bestNode = null;

            foreach(Node node in currentNode.connections.Keys)
            {
                if (explored.Contains(node)) continue;

                if(node.bottomNode)
                {
                    path.Add(node);

                    node.onPath = true;

                    found = true;

                    return path;
                }

                float score = totalScore + currentNode.connections[node] + Mathf.Abs((node.transform.position - destination.transform.position).magnitude);

                if(score < lowestScore)
                {
                    lowestScore = score;
                    bestNode = node;
                }
            }

            if(bestNode == null)
            {
                path.Remove(currentNode);
                currentNode.onPath = false;

                if(path.Count > 1) currentNode = path[path.Count - 1];
            }

            else
            {
                currentNode = bestNode;
                path.Add(currentNode);
                explored.Add(currentNode);
                bestNode.onPath = true;
            }

            lowestScore = float.MaxValue;
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if (caveData.onlyDisplayRoute && !onPath) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.6f);


        Gizmos.color = Color.turquoise;

        foreach(Node node in connections.Keys)
        {
            if (caveData.onlyDisplayRoute && !node.onPath) continue;

            Gizmos.DrawLine(transform.position, node.transform.position);
        }
    }
}

