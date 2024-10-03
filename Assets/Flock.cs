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
    public static int max = 5000; //1000000;
    public static int min = 100;
    public int numRepeat = 100;
    public static int stepsize = 100;
    int numsteps = (max - min) / stepsize;
    public string outputFileName = "FlockPerformance.csv";

    void Start()
    {
        float[] timeAverage = new float[numsteps];
        for (int x = 0; x < numsteps; x++)
        {
            int numdrones = x * stepsize + min;
            UnityEngine.Debug.Log("Current num drones = " + numdrones);

            UnityEngine.Debug.Log("Initializing flock with num drones = " + numdrones);
            InitFlock((int)(0.9 * numdrones)); // fill up 90% with drones

            // Measure performance for the specified number of repeats
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Simulate flock behavior for each repeat
            for (int rep = 0; rep < numRepeat; rep++)
            {
                SimulateFlockBehavior();
            }

            watch.Stop();
            long time = watch.ElapsedMilliseconds;
            UnityEngine.Debug.Log("Time taken for simulation: " + time + " ms");

            // Store average time for the current step
            timeAverage[x] = time / (float)numRepeat;

            // Write results to CSV
            WriteResults(numdrones, timeAverage[x]);
        }
    }

    // Initialize the flock
    public void InitFlock(int numAgents)
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        for (int i = 0; i < numAgents; i++)
        {
            Drone newAgent = Instantiate(
                agentPrefab,
                UnityEngine.Random.insideUnitCircle * numAgents * AgentDensity,
                Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)),
                transform
            );
            newAgent.name = "Agent " + i;
            newAgent.Initialize(this);
            agents.Add(newAgent);
        }
    }

    // BubbleSort for sorting drone temperatures
    void BubbleSort(int[] tempArray)
    {
        int n = tempArray.Length;
        bool swapped;
        for (int i = 0; i < n - 1; i++)
        {
            swapped = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (tempArray[j] > tempArray[j + 1])
                {
                    // Swap the elements
                    int temp = tempArray[j];
                    tempArray[j] = tempArray[j + 1];
                    tempArray[j + 1] = temp;
                    swapped = true;
                }
            }
            // If no two elements were swapped in the inner loop, the array is sorted
            if (!swapped)
                break;
        }
    }

    // Additional helper functions to manage drone data
    public int NumDrones = 0;


    public void append(int val, int[] tempArray)
    {
        if (NumDrones < tempArray.Length)
        {
            tempArray[NumDrones] = val;
            NumDrones++;
        }
        else
        {
            UnityEngine.Debug.LogError("Error: Cannot append, array is full.");
        }
    }

    public void appendFront(int val, int[] tempArray)
    {
        if (NumDrones < tempArray.Length)
        {
            // Shift to the right
            for (int i = NumDrones - 1; i >= 0; i--)
            {
                tempArray[i + 1] = tempArray[i];
            }
            tempArray[0] = val;
            NumDrones++;
        }
        else
        {
            UnityEngine.Debug.LogError("Error: Cannot append, array is full.");
        }
    }

    public void insert(int val, int index, int[] tempArray)
    {
        if (index >= 0 && index <= NumDrones && NumDrones < tempArray.Length)
        {
            // Shift elements to the right from the given index
            for (int i = NumDrones - 1; i >= index; i--)
            {
                tempArray[i + 1] = tempArray[i];
            }

            // Insert the new value
            tempArray[index] = val;
            NumDrones++;
        }
        else
        {
            UnityEngine.Debug.LogError("Error: Invalid index or array is full.");
        }
    }

    public void deleteFront(int[] tempArray)
    {
        if (NumDrones > 0)
        {
            for (int i = 0; i < NumDrones - 1; i++)
            {
                tempArray[i] = tempArray[i + 1];
            }

            NumDrones--;
        }
        else
        {
            UnityEngine.Debug.LogError("Error: No elements to delete.");
        }
    }

    public void deleteBack()
    {
        if (NumDrones > 0)
        {
            NumDrones--;
        }
        else
        {
            UnityEngine.Debug.LogError("Error: No elements to delete.");
        }
    }

    // Simulate the flock behavior (Update logic)
    void SimulateFlockBehavior()
    {
        List<int> temperaturesList = new List<int>();

        foreach (Drone agent in agents)
        {
            // Collect data like temperature
            temperaturesList.Add(agent.Temperature);
        }

        // Convert List to array for sorting
        int[] tempArray = temperaturesList.ToArray();

        
        //BubbleSort(tempArray);
        append(42, tempArray);
        //appendFront(42, tempArray);
        //insert(42, 69, tempArray);
        //deleteFront(tempArray);
        //deleteBack();


        // Log sorted temperatures
        //UnityEngine.Debug.Log("Sorted Temperatures: " + string.Join(", ", tempArray));

        foreach (Drone agent in agents)
        {
            // Decide on next movement direction
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

    void WriteResults(int numdrones, float timeAverage)
    {
        using (StreamWriter writer = new StreamWriter(outputFileName, true))
        {
            writer.WriteLine("Number of Drones,Time (ms)");
            writer.WriteLine($"{numdrones},{timeAverage}");
        }
        UnityEngine.Debug.Log("Results written to " + outputFileName);
    }

    // The usual Unity Update method continues to run the flock behavior every frame
    void Update()
    {
        SimulateFlockBehavior();
    }
}
