//Aiman Naim
using System.Collections.Generic;

public class DroneBTCommunication
{
    // Backing fields for binary tree roots
    internal Node _redTreeRoot;
    internal Node _blueTreeRoot;

    // Public properties to access the roots, if needed
    public Node RedTreeRoot
    {
        get { return _redTreeRoot; }
        set { _redTreeRoot = value; }
    }

    public Node BlueTreeRoot
    {
        get { return _blueTreeRoot; }
        set { _blueTreeRoot = value; }
    }

    // Method to add a drone to the appropriate binary tree
    public void AddDrone(Drone drone)
    {
        Node newNode = new Node(drone);

        if (drone.Colour == "Red")
        {
            AddToTree(ref _redTreeRoot, newNode);
        }
        else if (drone.Colour == "Blue")
        {
            AddToTree(ref _blueTreeRoot, newNode);
        }
    }

    // Stack-based iterative method to add a node to a binary tree
    private void AddToTree(ref Node root, Node newNode)
    {
        if (root == null)
        {
            root = newNode;
            return;
        }

        Node current = root;
        Node parent = null;

        while (current != null)
        {
            parent = current;
            if (newNode.Data.ID < current.Data.ID)
            {
                current = current.Left;
            }
            else
            {
                current = current.Right;
            }
        }

        if (newNode.Data.ID < parent.Data.ID)
        {
            parent.Left = newNode;
        }
        else
        {
            parent.Right = newNode;
        }
    }

    // Stack-based iterative method to find a drone by ID in the binary tree
    public Drone FindDroneBT(int id, Node root)
    {
        Node current = root;
        while (current != null)
        {
            if (current.Data.ID == id)
            {
                return current.Data;
            }
            else if (id < current.Data.ID)
            {
                current = current.Left;
            }
            else
            {
                current = current.Right;
            }
        }
        return null; // Drone not found
    }

    //Aiman Naim
    // Stack-based method to delete a drone by ID from a specified tree root
    public bool DeleteDroneById(int id, ref Node root)
    {
        Node parent = null;
        Node current = root;
        Stack<Node> stack = new Stack<Node>();

        // Step 1: Find the node to delete
        while (current != null && current.Data.ID != id)
        {
            parent = current;
            stack.Push(current);

            if (id < current.Data.ID)
            {
                current = current.Left;
            }
            else
            {
                current = current.Right;
            }
        }

        // If the node was not found, return false
        if (current == null)
        {
            return false;
        }

        // Step 2: Handle deletion cases

        // Case 1: Node has no children (leaf)
        if (current.Left == null && current.Right == null)
        {
            if (parent == null)
            {
                root = null; // The tree had only one node
            }
            else if (parent.Left == current)
            {
                parent.Left = null;
            }
            else
            {
                parent.Right = null;
            }
        }
        // Case 2: Node has one child
        else if (current.Left == null || current.Right == null)
        {
            Node child = current.Left ?? current.Right; // Get the non-null child

            if (parent == null)
            {
                root = child; // Node to delete is root
            }
            else if (parent.Left == current)
            {
                parent.Left = child;
            }
            else
            {
                parent.Right = child;
            }
        }
        // Case 3: Node has two children
        else
        {
            // Step 3: Find the in-order successor (smallest node in the right subtree)
            Node successorParent = current;
            Node successor = current.Right;

            while (successor.Left != null)
            {
                successorParent = successor;
                successor = successor.Left;
            }

            // Replace current node's value with the successor's value
            current.Data = successor.Data;

            // Remove the successor node (it has at most one child)
            if (successorParent.Left == successor)
            {
                successorParent.Left = successor.Right;
            }
            else
            {
                successorParent.Right = successor.Right;
            }
        }

        return true;
    }


    // Node class for binary tree
    public class Node
    {
        public Drone Data { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public Node(Drone data)
        {
            Data = data;
            Left = null;
            Right = null;
        }
    }
}
