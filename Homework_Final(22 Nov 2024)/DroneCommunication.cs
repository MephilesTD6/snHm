//Ashwin
public class DroneCommunication
{
    // Using private backing fields for the communication heads
    private Node _redCommHead;
    private Node _blueCommHead;

    // Public properties to access the heads (if needed)
    public Node RedCommHead
    {
        get { return _redCommHead; }
        private set { _redCommHead = value; }
    }

    public Node BlueCommHead
    {
        get { return _blueCommHead; }
        private set { _blueCommHead = value; }
    }


    //Ashwin 22012188
    public void AddToCommunication(Drone drone)
    {
        Node newNode = new Node(drone);

        // Check the drone's color to decide the communication network
        if (drone.Colour == "Red")
        {
            AddToRedComm(newNode);
        }
        else if (drone.Colour == "Blue")
        {
            AddToBlueComm(newNode);
        }
    }

    //Ashwin 22012188
    private void AddToRedComm(Node newNode)
    {
        if (RedCommHead == null)
        {
            RedCommHead = newNode;
        }
        else
        {
            Node current = RedCommHead;
            while (current.Next != null)
            {
                current = current.Next;
            }
            current.Next = newNode;
        }
    }

    //Ashwin 22012188
    private void AddToBlueComm(Node newNode)
    {
        if (BlueCommHead == null)
        {
            BlueCommHead = newNode;
        }
        else
        {
            Node current = BlueCommHead;
            while (current.Next != null)
            {
                current = current.Next;
            }
            current.Next = newNode;
        }
    }

    public Node GetBlueCommHead()
    {
        return BlueCommHead;
    }

    public Node GetRedCommHead()
    {
        return RedCommHead;
    }

    // Method to find a drone by ID
    public Drone FindDroneLL(string id)
    {
        int idConvert = int.Parse(id);  //changing string to integer to traverse

        // Search in RedComm
        Node current = _redCommHead;
        while (current != null)
        {
            if (current.Data.ID == idConvert)
            {
                return current.Data;
            }
            current = current.Next;
        }

        // Search in BlueComm
        current = _blueCommHead;
        while (current != null)
        {
            if (current.Data.ID == idConvert)
            {
                return current.Data;
            }
            current = current.Next;
        }

        return null; // Drone with the specified ID not found
    }

    //Avinash 24000113
    // Method to delete a drone from the network
     public bool DeleteDrone(Drone drone, Node blueCommHead, Node redCommHead)
     {
         if (drone.Colour == "Red")
         {
             return DeleteFromList(ref redCommHead, drone);
         }
         else if (drone.Colour == "Blue")
         {
             return DeleteFromList(ref blueCommHead, drone);
         }
         return false;
     }

     private bool DeleteFromList(ref Node head, Drone drone)
     {
         // If the head node is the one to be deleted
         if (head != null && head.Data == drone)
         {
             head = head.Next; // Move head to the next node
             return true; // Successful deletion
         }

         // Traverse the linked list to find the drone
         Node current = head;
         while (current != null && current.Next != null)
         {
             if (current.Next.Data == drone)
             {
                 current.Next = current.Next.Next; // Bypass the node
                 return true; // Successful deletion
             }
             current = current.Next;
         }

         return false; // Drone not found
     }
    

    // Node class for linked list
    public class Node
    {
        public Drone Data { get; set; }
        public Node Next { get; set; }

        public Node(Drone data)
        {
            Data = data;
            Next = null;
        }
    }
}
