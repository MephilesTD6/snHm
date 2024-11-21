using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float MovementSpeed;
    public float JumpForce;
    public LayerMask WhatIsGround;
    public Transform GroundCheck;

    private Rigidbody2D m_Rb;
    private bool m_IsGrounded;
    private float m_GroundedRadiusCheck = .2f;
    private bool m_IsFacingRight = true;
    private float m_InputX;

    private void Start()
    {
        m_Rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Getting input
        m_InputX = Input.GetAxis("Horizontal");
        bool m_JumpInput = Input.GetButtonDown("Jump");

        // Check IsGrounded
        m_IsGrounded = Physics2D.OverlapCircle(GroundCheck.position, m_GroundedRadiusCheck, WhatIsGround);

        // Jump when spacebar is pressed and the player is grounded
        if (m_IsGrounded && m_JumpInput)
        {
            m_IsGrounded = false;
            m_Rb.velocity = Vector2.up * JumpForce;
        }

        // Flip character sprite in direction
        if (m_InputX > 0 && !m_IsFacingRight)
        {
            Flip();
        }
        else if (m_InputX < 0 && m_IsFacingRight)
        {
            Flip();
        }

    }

    private void FixedUpdate()
    {
        // Moving the player in X-axis using the InputX every physics cycle
        m_Rb.velocity = new Vector2(m_InputX * MovementSpeed, m_Rb.velocity.y);
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_IsFacingRight = !m_IsFacingRight;

        // Flip the local scale rotation
        Vector3 transformScale = transform.localScale;
        transformScale.x *= -1;
        transform.localScale = transformScale;
    }
}


