using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Drone : MonoBehaviour
{
    //Aiman Naim
    public int Coolness { get; private set; }
    public string Colour { get; set; }

    Flock agentFlock;
    public Flock AgentFlock { get { return agentFlock; } }

    Collider2D agentCollider;
    public Collider2D AgentCollider { get { return agentCollider; } }

    // Start is called before the first frame update
    void Start()
    {
        agentCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Coolness = Random.Range(0, 10000); // Assign random Coolness
        UpdateVisualColor(); // Update color based on the new Coolness value
    }

    public void Initialize(Flock flock)
    {
        agentFlock = flock;
        //Coolness = Random.Range(0, 10000); // Assign random Coolness   
    }

    public void Move(Vector2 velocity)
    {
        transform.up = velocity;
        transform.position += (Vector3)velocity * Time.deltaTime;
    }

    //Avinash  24000113
    public string VisualColour
    {
        get => Colour;
        set
        {
            Colour = value;
            UpdateVisualColor(); // Call a method to update the visual representation
        }
    }

    private void UpdateVisualColor()
    {
        // Assuming there's a SpriteRenderer component
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Colour == "Blue" ? Color.blue : Color.red; //to show a visual representation of colour change
            //Debug.Log($"Drone {name}: Coolness = {Coolness}, Colour = {Colour}"); // Log current state
        }
    }
}
