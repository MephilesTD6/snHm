using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DroneNetworkCommunication
{
    private List<Node> nodes; // All nodes in the network
    private float neighborRadius; // Maximum distance to consider as a neighbor

    public DroneNetworkCommunication(float neighborRadius)
    {
        nodes = new List<Node>();
        this.neighborRadius = neighborRadius;
    }

    public DroneNetworkCommunication()
    {
    }

    // Add a drone to the network
    public void AddDrone(Drone drone)
    {
        Node newNode = new Node(drone);

        // Connect this drone only to others of the same color in the network
        foreach (var existingNode in nodes)
        {
            if (existingNode.Drone.Colour == drone.Colour)
            {
                float distance = Vector2.Distance(newNode.Drone.transform.position, existingNode.Drone.transform.position);

                if (distance <= neighborRadius)
                {
                    newNode.AddNeighbor(existingNode);
                    existingNode.AddNeighbor(newNode);
                }
            }
        }

        nodes.Add(newNode);
    }


    // Update connections based on distance
    private void UpdateNeighbors(Node node)
    {
        foreach (var otherNode in nodes)
        {
            // Only connect if the colors match
            if (node.Drone.Colour == otherNode.Drone.Colour)
            {
                float distance = Vector2.Distance(node.Drone.transform.position, otherNode.Drone.transform.position);

                // Add or remove neighbors based on distance
                if (distance <= neighborRadius)
                {
                    node.AddNeighbor(otherNode);
                    otherNode.AddNeighbor(node);
                }
                else
                {   
                    // Remove neighbors if outside the radius
                    node.RemoveNeighbor(otherNode);
                    otherNode.RemoveNeighbor(node);
                }
            }
            else
            {
                // Ensure no connections between nodes of different colors
                node.RemoveNeighbor(otherNode);
                otherNode.RemoveNeighbor(node);
            }
        }
    }

    // find a drone from the network
    public Node FindDroneG(int droneId, out Node startNode)
    {
        startNode = null; // Initialize the output parameter

        if (nodes.Count == 0)
        {
            UnityEngine.Debug.LogError("The graph is empty. Cannot perform BFS.");
            return null;
        }

        // Use BFS to find the drone in this network
        Queue<Node> toVisit = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        toVisit.Enqueue(nodes[0]);
        visited.Add(nodes[0]);
        startNode = nodes[0]; // Save the first visited node as startNode

        while (toVisit.Count > 0)
        {
            Node current = toVisit.Dequeue();

            // Validate both ID and color
            if (current.Drone.ID == droneId && current.Drone.Colour == nodes[0].Drone.Colour)
            {
                UnityEngine.Debug.Log($"Drone with ID {droneId} found in the graph.");
                return current;
            }

            // Add neighbors to the queue
            foreach (var neighbor in current.Neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        UnityEngine.Debug.LogError($"Drone with ID {droneId} not found in the graph.");
        return null; // Not found
    }


    // Remove the found drone from the network
    public void DeleteDrone(Node targetNode)
    {
        if (!nodes.Contains(targetNode))
        {
            UnityEngine.Debug.LogError($"Node {targetNode.Drone.name} not found in this graph. Deletion skipped.");
            return;
        }

        // Copy neighbors to a temporary list
        List<Node> neighborsCopy = new List<Node>(targetNode.Neighbors);

        // Safely remove the node from all its neighbors
        foreach (var neighbor in neighborsCopy)
        {
            neighbor.RemoveNeighbor(targetNode);
        }

        // Safely remove the node itself from the graph
        nodes.Remove(targetNode);
        UnityEngine.Debug.Log($"Node {targetNode.Drone.name} successfully removed.");
    }


    // Update the entire network for all drones
    public void UpdateNetwork()
    {
        foreach (var node in nodes)
        {
            UpdateNeighbors(node);
        }
        UnityEngine.Debug.Log($"Network updated. Total nodes: {nodes.Count}");
    }

    //function to get first node during traversal
    public Node GetFirstNode()
    {
        return nodes.Count > 0 ? nodes[0] : null;
    }
    //function to ensure the endNode is part of the relevant network:
    public bool ContainsNode(Node node)
    {
        // Ensure the node exists and belongs to the correct color network
        return nodes.Contains(node) && node.Drone.Colour == nodes[0].Drone.Colour;
    }

    //Dijikstra's Algorithm implementation
    public List<Node> CalculateShortestPath(Node startNode, Node endNode)
    {
        if (startNode.Drone.Colour != endNode.Drone.Colour)
        {
            UnityEngine.Debug.LogError("Start and End drones belong to different networks. Pathfinding is not allowed.");
            return null;
        }

        if (startNode == null || endNode == null)
        {
            UnityEngine.Debug.LogError("Start or End drone not found in the network.");
            return null;
        }

        // Initialize distances and previous nodes
        Dictionary<Node, float> distances = new Dictionary<Node, float>();
        Dictionary<Node, Node> previousNodes = new Dictionary<Node, Node>();
        HashSet<Node> visited = new HashSet<Node>();

        foreach (var node in nodes)
        {
            distances[node] = float.MaxValue; // Initialize all distances to infinity
            previousNodes[node] = null;      // Initialize previous nodes as null
        }

        distances[startNode] = 0; // Distance to the start node is 0
        PriorityQueue<Node, float> priorityQueue = new PriorityQueue<Node, float>();
        priorityQueue.Enqueue(startNode, 0);

        while (priorityQueue.Count > 0)
        {
            Node currentNode = priorityQueue.Dequeue();

            if (currentNode == endNode)
            {
                // Build the shortest path from end to start
                List<Node> shortestPath = new List<Node>();
                while (currentNode != null)
                {
                    shortestPath.Insert(0, currentNode);
                    currentNode = previousNodes[currentNode];
                }
                return shortestPath;
            }

            visited.Add(currentNode);

            // Update distances for each neighbor
            foreach (var neighbor in currentNode.Neighbors)
            {
                if (visited.Contains(neighbor)) continue;

                float newDistance = distances[currentNode] + Vector2.Distance(currentNode.Drone.transform.position, neighbor.Drone.transform.position);

                if (newDistance < distances[neighbor])
                {
                    distances[neighbor] = newDistance;
                    previousNodes[neighbor] = currentNode;
                    priorityQueue.Enqueue(neighbor, newDistance);
                }

                //Double-checks the priority queue implementation to ensure proper termination (dequeueing and sorting)
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    priorityQueue.Enqueue(neighbor, newDistance);
                }

            }
        }

        UnityEngine.Debug.LogError("No path exists between the specified drones.");
        return null; // No path found
    }

    public class Node
    {
        public Drone Drone { get; private set; }
        public List<Node> Neighbors { get; private set; }

        public Node(Drone drone)
        {
            Drone = drone;
            Neighbors = new List<Node>();
        }

        public void AddNeighbor(Node neighbor)
        {
            if (!Neighbors.Contains(neighbor))
            {
                Neighbors.Add(neighbor);
            }
        }

        public void RemoveNeighbor(Node neighbor)
        {
            Neighbors.Remove(neighbor);
        }
    }
}
