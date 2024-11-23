using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics; // For Stopwatch
using System.Drawing;
using System.IO; // For StreamWriter
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.SceneManagement;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public Drone agentPrefab;
    List<Drone> agents = new List<Drone>();
    public FlockBehavior behavior;

    [Range(10, 5000)]
    public int startingCount = 250;
    const float AgentDensity = 0.08f;

    [Range(1f, 100f)]
    public float driveFactor = 10f;
    [Range(1f, 100f)]
    public float maxSpeed = 5f;
    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }

    // Instantiate the communication to lists, trees and graph
    private DroneCommunication droneComm = new DroneCommunication();            //Linked list communication object
    private DroneBTCommunication droneBTComm = new DroneBTCommunication();      // Binary Tree communication object
    private DroneNetworkCommunication redNetwork ;                              // Red Graph(network) communication object
    private DroneNetworkCommunication blueNetwork;                              // Blue Graph(network) communication object
    private Stopwatch stopwatch = new Stopwatch();


    
    [SerializeField] private TimeDisplay timeDisplay; // to display time taken
    [SerializeField] private TMP_InputField droneIdInputField; //to display input field to recieve drone id


    // Start is called before the first frame update
    void Start()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        for (int i = 0; i < startingCount; i++)
        {
            Drone newAgent = Instantiate(
                agentPrefab,
                UnityEngine.Random.insideUnitCircle * startingCount * AgentDensity,
                Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)),
                transform
        );
            newAgent.name = "Agent " + i;

            // Pass the index to the Initialize method
            newAgent.Initialize(this, i);  // Pass the index to set the ID

            agents.Add(newAgent);

            // Add to communication network
            droneComm.AddToCommunication(newAgent);

            // Initialize blue and red networks
            float neighborRadius = 1.5f; // Adjust as needed
            blueNetwork = new DroneNetworkCommunication(neighborRadius);
            redNetwork = new DroneNetworkCommunication(neighborRadius);
        }
    }

    void MeasureAndWriteTiming()
    {
        // Start the stopwatch to measure the time
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        PartitionAndAssignDronesToNetworks(); // The method we want to measure

        // Stop the stopwatch
        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;

        // Write the timing result to a CSV file
        WriteTimingToCSV(timeTaken);

    }

    //See Tho Soon Yinn 24000187
    void PartitionAndAssignDronesToNetworks()
    {
        if (agents.Count == 0) return; // Exit if the list is empty

        int totalCoolness = 0;
        foreach (var drone in agents)
        {
            totalCoolness += drone.Coolness;
        }
        int averageCoolness = totalCoolness / agents.Count; // Use average Coolness as the pivot

        foreach (var drone in agents)
        {
            if (drone.Coolness > averageCoolness)
            {
                drone.VisualColour = "Blue";
                droneBTComm.AddDrone(drone);  // Add to BlueTree in binary tree communication
            }
            else
            {
                drone.VisualColour = "Red";
                droneBTComm.AddDrone(drone);  // Add to RedTree in binary tree communication
            }

            if (drone.Colour == "Blue")
            {
                blueNetwork.AddDrone(drone);
                UnityEngine.Debug.Log($"Drone {drone.ID} added to Blue network.");  //Add to Blue network(graph) communication
            }
            else if (drone.Colour == "Red")
            {
                redNetwork.AddDrone(drone);
                UnityEngine.Debug.Log($"Drone {drone.ID} added to Red network.");  //Add to Red network(graph) communication
            }
            else
            {
                UnityEngine.Debug.LogError($"Drone {drone.ID} has an invalid color: {drone.Colour}");
            }


            // Add drones to their linked list communication
            droneComm.AddToCommunication(drone);
            UnityEngine.Debug.Log($"Drone {drone.name}: Coolness = {drone.Coolness}, Assigned Colour = {drone.Colour}");
        }
    }


    //See tho and Ashwin
    //function to delete drone by id from linked list and calc time taken
    public void LinkedListDeleteDrone()
    {   
        // Start timing
        stopwatch.Reset();
        stopwatch.Start();

        // Get the id input from the input field
        string droneId = droneIdInputField.text;

        //check if it exists
        if (!int.TryParse(droneId, out int index))
        {
            UnityEngine.Debug.LogError("Invalid index input. Please enter a valid number.");
            return;
        }

        if (index < 0 || index >= agents.Count)
        {
            UnityEngine.Debug.LogError("Id out of range. Please enter a valid id.");
            return;
        }

        // Find the drone in linked list by ID/index
        Drone drone = droneComm.FindDroneLL(droneId);
        if (drone != null)
        {
            //removes the drone from linked list
            bool removedFromComm = droneComm.DeleteDrone(drone, droneComm.GetBlueCommHead(), droneComm.GetRedCommHead());

            if (removedFromComm)
            {
                agents.Remove(drone);
                drone.gameObject.SetActive(false);
                UnityEngine.Debug.Log($"Drone with ID {drone.ID} removed successfully.");
            }
            else
            {
                UnityEngine.Debug.Log($"Failed to remove drone from the linked list.");
            }

        }
        else
        {
            UnityEngine.Debug.Log($"Drone with ID {droneId} not found in the network.");
        }

        stopwatch.Stop();   
        ListDeletionTimingToCSV(stopwatch.Elapsed);
    }

    public DroneBTCommunication GetDroneBTComm()
    {
        return droneBTComm;
    }

    //See Tho and Aiman Naim
    // //function to delete drone by id from binary tree and calc time taken
    public void BinaryTreeDeleteDrone()
    {
        // Start timing
        stopwatch.Reset();
        stopwatch.Start();

        // Get the id input from the input field
        string droneId = droneIdInputField.text;

        //check if it exists
        if (!int.TryParse(droneId, out int index))
        {
            UnityEngine.Debug.LogError("Invalid index input. Please enter a valid number.");
            return;
        }

        if (index < 0 || index >= agents.Count)
        {
            UnityEngine.Debug.LogError("Id out of range. Please enter a valid id.");
            return;
        }

        int idConvert = int.Parse(droneId); //convert string into int
        bool deletionSuccessful = false; 

        //find drone in both binary trees by ID/index

        // Try deleting from the Red Tree
        Drone drone = droneBTComm.FindDroneBT(idConvert, droneBTComm.RedTreeRoot);
            if (drone != null)
            {
                deletionSuccessful = droneBTComm.DeleteDroneById(idConvert, ref droneBTComm._redTreeRoot);
                if (deletionSuccessful)
                {
                agents.Remove(drone);
                drone.gameObject.SetActive(false);
                UnityEngine.Debug.Log($"Drone with ID {idConvert} deleted from the Red Tree.");
                }
            }

        // If not found in Red Tree, try the Blue Tree
        if (!deletionSuccessful)
        {
            drone = droneBTComm.FindDroneBT(idConvert, droneBTComm.BlueTreeRoot);
            if (drone != null)
            {
                deletionSuccessful = droneBTComm.DeleteDroneById(idConvert, ref droneBTComm._blueTreeRoot); // Use the backing field here
                if (deletionSuccessful)
                {
                    agents.Remove(drone);
                    drone.gameObject.SetActive(false);
                    UnityEngine.Debug.Log($"Drone with ID {idConvert} deleted from the Blue Tree.");
                }
            }
        }

        if (!deletionSuccessful)
        {
            UnityEngine.Debug.Log($"Drone with ID {droneId} not found in either tree.");
        }

        stopwatch.Stop();
        TreeDeletionTimingToCSV(stopwatch.Elapsed);
    }

    //See Tho
    public void GraphDeleteDrone()
    {
        // Start timing
        stopwatch.Reset();
        stopwatch.Start();

        // Get the id input from the input field
        string droneId = droneIdInputField.text;

        //check if it exists
        if (!int.TryParse(droneId, out int index))
        {
            UnityEngine.Debug.LogError("Invalid index input. Please enter a valid number.");
            return;
        }

        if (index < 0 || index >= agents.Count)
        {
            UnityEngine.Debug.LogError("Id out of range. Please enter a valid id.");
            return;
        }

        int idConvert = int.Parse(droneId); //convert string into int
        DroneNetworkCommunication.Node startNode; // Start drone (the first drone during traversal)

        bool deletionSuccessful = false;

        // Check and delete the drone in the Blue graph
        DroneNetworkCommunication.Node targetNode = blueNetwork.FindDroneG(idConvert, out startNode);

        if (targetNode != null && blueNetwork.ContainsNode(targetNode))
        {
            UnityEngine.Debug.Log($"Drone {targetNode.Drone.name} is in the Blue network.");
            deletionSuccessful = true;
        }

        // If not found in the Blue graph or mistakenly identified, check the Red graph
        if (!deletionSuccessful)
        {
            targetNode = redNetwork.FindDroneG(idConvert, out startNode);

            if (targetNode != null && redNetwork.ContainsNode(targetNode))
            {
                UnityEngine.Debug.Log($"Drone {targetNode.Drone.name} is in the Red network.");
                deletionSuccessful = true;
            }
        }

        UnityEngine.Debug.Log($"Start Node: {startNode?.Drone.name}, Color: {startNode?.Drone.Colour}");
        UnityEngine.Debug.Log($"Target Node: {targetNode?.Drone.name}, Color: {targetNode?.Drone.Colour}");

        if (!deletionSuccessful)
        {
            UnityEngine.Debug.LogError($"Drone with ID {droneId} not found in any network.");
            return;
        }

        // Ensure both startNode and endNode(targetNode) are valid
        if (startNode == null || targetNode == null)
        {
            UnityEngine.Debug.LogError("Start or End node is null. Cannot calculate shortest path.");
            return;
        }

        // Perform Dijkstra's algorithm on the appropriate network
        List<DroneNetworkCommunication.Node> shortestPath = null;
        if (blueNetwork.ContainsNode(targetNode))
        {
            shortestPath = blueNetwork.CalculateShortestPath(startNode, targetNode);
        }
        else if (redNetwork.ContainsNode(targetNode))
        {
            shortestPath = redNetwork.CalculateShortestPath(startNode, targetNode);
        }

        // Log the result
        if (shortestPath != null)
        {
            UnityEngine.Debug.Log("Shortest path:");
            foreach (var node in shortestPath)
            {
                UnityEngine.Debug.Log(node.Drone.name);
            }
        }
        else
        {
            UnityEngine.Debug.LogError("No path exists between the specified drones.");
        }

        // Proceed with deletion after path calculation
        if (blueNetwork.ContainsNode(targetNode))
        {
            blueNetwork.DeleteDrone(targetNode);
            targetNode.Drone.gameObject.SetActive(false);
            UnityEngine.Debug.Log($"Drone {targetNode.Drone.name} deleted from Blue network.");
        }
        else if (redNetwork.ContainsNode(targetNode))
        {
            redNetwork.DeleteDrone(targetNode);
            targetNode.Drone.gameObject.SetActive(false);
            UnityEngine.Debug.Log($"Drone {targetNode.Drone.name} deleted from Red network.");
        }

        stopwatch.Stop();
        GraphDeletionTimingToCSV(stopwatch.Elapsed);
    }

    //Avinash Kumar
    void ListDeletionTimingToCSV(TimeSpan timeTaken)
    {
        string filePath = Path.Combine(Application.dataPath, "FlockListDeletion.csv");

        bool fileExists = File.Exists(filePath);
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (!fileExists)
            {
                writer.WriteLine("Timestamp, Time Taken To Delete(ms)");
            }

            writer.WriteLine($"{DateTime.Now}, {timeTaken.TotalMilliseconds}");
        }
        UnityEngine.Debug.Log($"Deletion timing result written to {filePath}: {timeTaken.TotalMilliseconds} ms");

        // Pass value into TimeDisplay to update the UI text
        if (timeDisplay != null)
        {
            timeDisplay.updateTimeTaken(timeTaken);
        }
        else
        {
            UnityEngine.Debug.LogError("TimeDisplay reference is missing in Flock.");
        }
    }
    
     //Avinash Kumar
    void TreeDeletionTimingToCSV(TimeSpan timeTaken)
    {
        string filePath = Path.Combine(Application.dataPath, "FlockTreeDeletion.csv");

        bool fileExists = File.Exists(filePath);
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (!fileExists)
            {
                writer.WriteLine("Timestamp, Time Taken To Delete(ms)");
            }

            writer.WriteLine($"{DateTime.Now}, {timeTaken.TotalMilliseconds}");
        }
        UnityEngine.Debug.Log($"Deletion timing result written to {filePath}: {timeTaken.TotalMilliseconds} ms");

        // Pass value into TimeDisplay to update the UI text
        if (timeDisplay != null)
        {
            timeDisplay.updateTimeTaken(timeTaken);
        }
        else
        {
            UnityEngine.Debug.LogError("TimeDisplay reference is missing in Flock.");
        }
    }
    
     //Avinash Kumar
    void GraphDeletionTimingToCSV(TimeSpan timeTaken)
    {
        string filePath = Path.Combine(Application.dataPath, "FlockGraphDeletion.csv");

        bool fileExists = File.Exists(filePath);
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (!fileExists)
            {
                writer.WriteLine("Timestamp, Time Taken To Delete(ms)");
            }

            writer.WriteLine($"{DateTime.Now}, {timeTaken.TotalMilliseconds}");
        }
        UnityEngine.Debug.Log($"Deletion timing result written to {filePath}: {timeTaken.TotalMilliseconds} ms");

        // Pass value into TimeDisplay to update the UI text
        if (timeDisplay != null)
        {
            timeDisplay.updateTimeTaken(timeTaken);
        }
        else
        {
            UnityEngine.Debug.LogError("TimeDisplay reference is missing in Flock.");
        }
    }

    //Avinash Kumar
    void WriteTimingToCSV(TimeSpan timeTaken)
    {
        // Define the path for the CSV file
        string filePath = Path.Combine(Application.dataPath, "TimingResults.csv");

        // Check if the file exists; if not, create it and write the header
        bool fileExists = File.Exists(filePath);

        using (StreamWriter writer = new StreamWriter(filePath, true)) // Append to the file
        {
            if (!fileExists)
            {
                writer.WriteLine("Timestamp, Time Taken to Partition(ms)"); // CSV header
            }

            // Write the current timestamp and time taken for partitioning
            writer.WriteLine($"{DateTime.Now}, {timeTaken.TotalMilliseconds}");
        }

        UnityEngine.Debug.Log($"Timing result written to {filePath}: {timeTaken.TotalMilliseconds} ms");
    }


    // Update is called once per frame
    void Update()
    {
        //Measure the time for partitioning
        MeasureAndWriteTiming();
        redNetwork.UpdateNetwork();
        blueNetwork.UpdateNetwork();

        foreach (Drone agent in agents)
        {
            List<Transform> context = GetNearbyObjects(agent);

            Vector2 move = behavior.CalculateMove(agent, context, this);
            move *= driveFactor;
            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }
            agent.Move(move);
        }
    }

    List<Transform> GetNearbyObjects(Drone agent)
    {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighborRadius);
        foreach (Collider2D c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }
        return context;
    }
}
