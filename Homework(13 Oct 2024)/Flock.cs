using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics; // For Stopwatch
using System.IO; // For StreamWriter
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
            newAgent.Initialize(this);
            agents.Add(newAgent);
        }
    }

    void MeasureAndWriteTiming()
    {
        // Start the stopwatch to measure the time
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        PartitionAndColorDrones(); // The method we want to measure

        // Stop the stopwatch
        stopwatch.Stop();
        TimeSpan timeTaken = stopwatch.Elapsed;

        // Write the timing result to a CSV file
        WriteTimingToCSV(timeTaken);
    }

    void PartitionAndColorDrones()
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
            }
            else
            {
                drone.VisualColour = "Red";
            }

            UnityEngine.Debug.Log($"Drone {drone.name}: Coolness = {drone.Coolness}, Assigned Colour = {drone.Colour}");
        }
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
                writer.WriteLine("Timestamp, Time Taken (ms)"); // CSV header
            }

            // Write the current timestamp and time taken for partitioning
            writer.WriteLine($"{DateTime.Now}, {timeTaken.TotalMilliseconds}");
        }

        UnityEngine.Debug.Log($"Timing result written to {filePath}: {timeTaken.TotalMilliseconds} ms");
    }

    // Update is called once per frame
    void Update()
    {
        // Measure the time for partitioning
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
