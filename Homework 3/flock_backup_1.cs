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

    // Instantiate the communication to lists and trees
    private DroneCommunication droneComm = new DroneCommunication();
    private DroneBTCommunication droneBTComm = new DroneBTCommunication();  // Binary Tree communication
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

            // Add drones to their linked list communication
            droneComm.AddToCommunication(drone);
            UnityEngine.Debug.Log($"Drone {drone.name}: Coolness = {drone.Coolness}, Assigned Colour = {drone.Colour}");
        }
    }


    //Syukri 24000074
    //function to delete drone by id from linked list
    public void LinkedListDeleteDrone()
    {
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
            RemoveDrone(drone); // Remove the found drone
        }
        else
        {
            UnityEngine.Debug.Log($"Drone with ID {droneId} not found in the network.");
        }
    }

    public void BinaryTreeDeleteDrone()
    {
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

        int idConvert = int.Parse(droneId);
        try
        {
            //find drone in both binary tree by ID/index
            Drone drone = droneBTComm.FindDroneBT(idConvert, droneBTComm.RedTreeRoot);
            if (drone != null)
            {
                RemoveDrone(drone); // Remove the found drone from red tree
            }
            else
            {
                UnityEngine.Debug.Log($"Drone with ID {droneId} not found in the red tree.");
                drone = droneBTComm.FindDroneBT(idConvert, droneBTComm.BlueTreeRoot);
                RemoveDrone(drone); // Remove the found drone from blue tree
            }
        }
        catch 
        { 
            UnityEngine.Debug.Log($"Drone with ID {droneId} not found in both tree."); 
        }
    }


    //Avinash
    //function to delete drone by id from binary tree

    void ListDeletionTimingToCSV(TimeSpan timeTaken)
    {
        string filePath = Path.Combine(Application.dataPath, "FlockLLDeletion.csv");

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

    }

    void TreeDeletionTimingToCSV(TimeSpan timeTaken)
    {
        string filePath = Path.Combine(Application.dataPath, "FlockLLDeletion.csv");

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

    }

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

    public void RemoveDrone(Drone drone)
    {
        stopwatch.Reset();
        stopwatch.Start();

        bool removedFromComm = droneComm.DeleteDrone(drone, droneComm.GetBlueCommHead(), droneComm.GetRedCommHead());

        if (removedFromComm)
        {
            agents.Remove(drone);
            drone.gameObject.SetActive(false);
            UnityEngine.Debug.Log($"Drone at index removed successfully.");
        }
        else
        {
            UnityEngine.Debug.Log($"Failed to remove drone from the communication network.");
        }

        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;

        ListDeletionTimingToCSV(timeTaken);

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


    // Update is called once per frame
    void Update()
    {
        //Measure the time for partitioning
        MeasureAndWriteTiming();

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
