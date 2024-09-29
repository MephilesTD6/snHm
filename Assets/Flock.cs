using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics; // For Stopwatch
using UnityEngine;
using System.IO; // For file writing

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

    // Performance measurement parameters
    public int numRepeat = 1; // Only simulate once
    public string outputFileName = "FlockPerformance.csv";

    void Start()
    {
        // Only simulate once with the specified startingCount
        UnityEngine.Debug.Log("Initializing flock with num drones = " + startingCount);
        InitFlock(); // Initialize the flock with startingCount drones

        // Measure performance only once
        Stopwatch watch = new Stopwatch();
        watch.Start();

        // Simulate flock behavior for a single run
        for (int rep = 0; rep < numRepeat; rep++)
        {
            SimulateFlockBehavior(); // Call the Update logic for the flock
        }

        watch.Stop();
        long time = watch.ElapsedMilliseconds;
        UnityEngine.Debug.Log("Time taken for simulation: " + time + " ms");

        // Write results to CSV (if needed)
        WriteResults(time);
    }

    // Initialize the flock
    public void InitFlock()
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

    // See Tho Soon Yinn 24000197
    void BubbleSort(int[] arr)
    {
        int n = arr.Length;
        bool swapped;
        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (arr[j] > arr[j + 1])
                {
                    // Swap the elements
                    int temp = arr[j];
                    arr[j] = arr[j + 1];
                    arr[j + 1] = temp;
                    swapped = true;
                }
            }
            // If no two elements were swapped in the inner loop, the array is sorted
            if (!swapped)
                break;
        }
    }

    // Avinash Kumar a/l Jayaseelan 24000113
    void SplitSort(int[] arr)
    {
        int n = arr.Length;
        int[] upperEqual = new int[n];  //List for >= to first element
        int[] lower = new int[n];       //List for < than first element
        int i = 1;
        int j = 0;

        upperEqual[0] = arr[0];

        for (int k = 1; k < n - 1; k++)
        {
            if (arr[j] < arr[0])
            {
                lower[j] = arr[k];
                j++;
            }
            else
            {
                upperEqual[i] = arr[k];
                i++;
            }
        }
    }

    // Simulate the flock behavior (Update logic)
    void SimulateFlockBehavior()
    {
        List<int> temperatures = new List<int>();

        foreach (Drone agent in agents)
        {
            // Collect data like temperature
            temperatures.Add(agent.Temperature);
        }

        // Convert List to array for sorting
        int[] tempArray = temperatures.ToArray();

        // Sorting drone temperatures 
        //BubbleSort(tempArray);
        SplitSort(tempArray);

        // (Optional) Log sorted temperatures
        //UnityEngine.Debug.Log("Sorted Temperatures: " + string.Join(", ", tempArray));

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

    void WriteResults(long time)
    {
        using (StreamWriter writer = new StreamWriter(outputFileName, true))
        {
            writer.WriteLine("Number of Drones,Time (ms)");
            writer.WriteLine($"{startingCount},{time}");
        }
        UnityEngine.Debug.Log("Results written to " + outputFileName);
    }

    // The usual Unity Update method continues to run the flock behavior every frame
    void Update()
    {
        SimulateFlockBehavior();
    }
}
