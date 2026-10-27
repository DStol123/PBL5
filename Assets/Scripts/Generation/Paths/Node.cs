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
    public bool flaggedRedundant;
    public bool active;
    public float totalCost;

    public RandomWalker rw;

    public CaveData caveData;

    public bool tunnelEnd;

    private void Awake()
    {
        connections = new Dictionary<Node, float> { };
        active = true;
    }

    public void AddNode(Node node)
    {
        if (connections.ContainsKey(node)) return;

        node.active = true;

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

    public bool InLineOfSight(Vector3 pos)
    {
        Vector3 dir = (pos - transform.position).normalized;

        float distance = Vector3.Distance(pos, transform.position);

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

    public bool InLineOfSightPhysics(Vector3 pos)
    {
        if (Physics.Raycast(transform.position, (pos - transform.position).normalized, Mathf.Abs((pos - transform.position).magnitude)))
            return false;
        else return true;
    }

    public List<Node> FindDirectPath()
    {
        List<Node> path = new List<Node>();
        path.Add(this);

        List<Node> explored = new List<Node>();
        explored.Add(this);

        onPath = true;

        bool found = false;

        Node currentNode = this;

        Node destination = null;

        int counter = 1;

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
            Debug.Log("DFS pass " + counter);

            counter++;

            float lowestScore = float.MaxValue;
            Node bestNode = null;

            foreach(Node node in currentNode.connections.Keys)
            {
                if (explored.Contains(node)) continue;

                if(node.PathConnectionCount() > 1)
                {
                    continue;
                }

                if(node.bottomNode)
                {
                    Debug.Log("Path complete");

                    path.Add(node);

                    node.onPath = true;

                    found = true;

                    return path;
                }

                float score = currentNode.connections[node] + Mathf.Abs((node.transform.position - destination.transform.position).magnitude);

                if(score < lowestScore)
                {
                    lowestScore = score;
                    bestNode = node;
                }
            }

            if(bestNode == null)
            {
                Debug.Log("No valid node found, backtracking");

                path.Remove(currentNode);
                currentNode.onPath = false;
                explored.Add(currentNode); 

                if(path.Count > 1) currentNode = path[path.Count - 1];
            }

            else
            {
                Debug.Log("Node appended to path");

                currentNode = bestNode;

                if(!path.Contains(currentNode)) path.Add(currentNode);

                explored.Add(currentNode);
                currentNode.onPath = true;
            }

            lowestScore = float.MaxValue;
        }

        return null;
    }

    public int PathConnectionCount()
    {
        int counter = 0;

        foreach(Node node in connections.Keys)
        {
            if (node.onPath) counter++;
        }

        return counter;
    }

    public float CostToHere()
    {
        if (!onPath) return float.MaxValue;

        if (startingNode) return 0f;

        float total = 0f;

        for(int i = 1; i < caveData.pathToBottom.Count; i++)
        {
            total += caveData.pathToBottom[i - 1].connections[caveData.pathToBottom[i]];

            if (caveData.pathToBottom[i] == this) break;
        }

        return total;
    }

    private void OnDrawGizmos()
    {
        if ((caveData.onlyDisplayRoute && !onPath) || !active) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.6f);


        Gizmos.color = Color.turquoise;
        
        foreach(Node node in connections.Keys)
        {
            if (!node.active || (caveData.onlyDisplayRoute && !node.onPath)) continue;

            Gizmos.DrawLine(transform.position, node.transform.position);
        }
        
    }
}

