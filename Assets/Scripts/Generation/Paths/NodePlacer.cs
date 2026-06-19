using UnityEngine;

public class NodePlacer : MonoBehaviour
{
    public Node PlaceNode(Vector3 position, Node[] neighbors)
    {
        GameObject nodeObject = new GameObject("Node");

        nodeObject.transform.position = position;

        Node newNode = nodeObject.AddComponent<Node>();

        if (neighbors == null || neighbors.Length == 0) return newNode;

        for (int i = 0; i < neighbors.Length; i++)
        {
            float cost = Mathf.Abs((neighbors[i].transform.position - newNode.transform.position).magnitude);

            newNode.connections.Add(neighbors[i], cost);
            neighbors[i].connections.Add(newNode, cost);
        }

        return newNode;
    }

    public Node PlaceNode(Vector3 position, Node neighbor)
    {
        GameObject nodeObject = new GameObject("Node");

        nodeObject.transform.position = position;

        Node newNode = nodeObject.AddComponent<Node>();

        if(neighbor == null) return newNode;

        float cost = Mathf.Abs((neighbor.transform.position - newNode.transform.position).magnitude);

        newNode.connections.Add(neighbor, cost);
        neighbor.connections.Add(newNode, cost);

        return newNode;
    }

    public Node PlaceNode(Vector3 position)
    {
        return PlaceNode(position, new Node[0]);
    }
}
