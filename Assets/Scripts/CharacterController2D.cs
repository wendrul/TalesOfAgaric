﻿using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.

    private bool fastFall;

    [Header("lateral movement")]
    [SerializeField] private float runSpeed = 6;
    [SerializeField] private float airSpeed = 3.5f;
    [SerializeField] private float crawlSpeed = 0f;
    [Space]
    [SerializeField] private float groundAcceleration;
    [SerializeField] private float groundDeceleration;
    [SerializeField] private float airAcceleration;

    [Header("Jump parameters")]
    [SerializeField] private float jumpSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float downAcceleration = 13f;
    [Space]
    [SerializeField] private float maxFallSpeed = 4f;
    [Range(0, 1)][SerializeField] private float fastFallMultiplier = 0.6f;
    private float airTime;
    private float fastFallSpeed;
    [Header("Double Jump parameters")]
    [SerializeField] private float dJumpSpeed = 7f;
    [SerializeField] private float dJumpHeight = 0.9f;
    [SerializeField] private float dDownAcceleration = 13f;
    [Space]
    [SerializeField] private int defaultAirHops= 1;
    [Header("Short Hop parameters")]
    [SerializeField] private float shortHopSpeed = 3;
    [SerializeField] private float shortHopHeight = 0.5f;
    [SerializeField] private float shortHopDownAccel = 10f;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;
    private float verticalSpeed;
    private int airHops;
    struct JumpMode
    {
        public float ascendingAcceleration;
        public float downAcceleration;
    }

    private JumpMode currentJump;
    private JumpMode normalJump;
    private JumpMode doubleJump;
    private JumpMode shortHopJump;

    private Animator animator;
    private float horizontalSpeed;

    private void Awake()
    {
        verticalSpeed = 0f;
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
        JumpSetup();
        fastFall = false;
        fastFallSpeed = maxFallSpeed * (1 + fastFallMultiplier);
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void JumpSetup()
    {
        normalJump.ascendingAcceleration = jumpSpeed * jumpSpeed / (2 * jumpHeight);
        normalJump.downAcceleration = downAcceleration;
        doubleJump.ascendingAcceleration = dJumpSpeed * dJumpSpeed / (2 * dJumpHeight);
        doubleJump.downAcceleration = dDownAcceleration;
        shortHopJump.ascendingAcceleration = shortHopSpeed * shortHopSpeed / (2 * shortHopHeight);
        shortHopJump.downAcceleration = shortHopDownAccel;
    }

    private void Update()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    public void Move(float move, bool crouch, bool jump, float vertical, bool jumpReleased, bool shortHop, bool fastFall)
    {
        animator.SetBool("isWalking", false);
        VerticalMovement(jumpReleased, fastFall);
        CrouchUpdate(crouch);
        if (m_Grounded)
        {
            if (move > 0 && !m_FacingRight)
            {
                Flip();
            }
            else if (move < 0 && m_FacingRight)
            {
                Flip();
            }

            if (Mathf.Abs(move) > 0)
            {
                animator.SetBool("isWalking", true);
                horizontalSpeed += Mathf.Sign(move) * groundAcceleration * Time.deltaTime;
            }
            else
            {
                if (Mathf.Abs(horizontalSpeed) < groundDeceleration * Time.deltaTime + 0.01f)
                {
                    horizontalSpeed = 0;
                }
                if (horizontalSpeed != 0)
                    horizontalSpeed -= Mathf.Sign(horizontalSpeed) * groundDeceleration * Time.deltaTime;
            }
            if (crouch && Mathf.Abs(horizontalSpeed) > crawlSpeed)
                horizontalSpeed = Mathf.Sign(horizontalSpeed) * crawlSpeed;
            if (!crouch && Mathf.Abs(horizontalSpeed) > runSpeed)
                horizontalSpeed = Mathf.Sign(horizontalSpeed) * runSpeed;
        }
        else
        {
            if (Mathf.Abs(move) > 0)
            {
                horizontalSpeed += Mathf.Sign(move) * airAcceleration * Time.deltaTime;
            }
            else
            {
                if (Mathf.Abs(horizontalSpeed) < airAcceleration * Time.deltaTime + 0.01f)
                {
                    horizontalSpeed = 0;
                }
                horizontalSpeed -= Mathf.Sign(horizontalSpeed) * airAcceleration * Time.deltaTime;
            }
            if (Mathf.Abs(horizontalSpeed) > airSpeed)
                horizontalSpeed = Mathf.Sign(horizontalSpeed) * airSpeed;
        }
        transform.Translate(new Vector3(horizontalSpeed * Time.deltaTime, verticalSpeed * Time.deltaTime));
        SelectJump(jump, shortHop);
        
    }
    private void SelectJump(bool jump, bool shortHop)
    {
        if (!m_Grounded && jump && airHops > 0)
        {
            airHops--;
            fastFall = false;
            verticalSpeed = dJumpSpeed;
            currentJump = doubleJump;
        }
        if (m_Grounded && jump)
        {
            airHops = defaultAirHops;
            airTime = 0;
            m_Grounded = false;
            fastFall = false;
            verticalSpeed = jumpSpeed;
            currentJump = normalJump;
        }
        if (m_Grounded && shortHop)
        {
            airHops = defaultAirHops;
            airTime = 0;
            m_Grounded = false;
            fastFall = false;
            verticalSpeed = shortHopSpeed;
            currentJump = shortHopJump;
        }
    }
    private void VerticalMovement(bool jumpReleased, bool inputFastFall)
    {
        if (!m_Grounded)
        {
            airTime += Time.deltaTime;
            if (verticalSpeed > 0)
            {
                verticalSpeed -= currentJump.ascendingAcceleration * Time.deltaTime;
                RaycastHit2D hitCeil = Physics2D.Raycast(m_CeilingCheck.position, Vector2.up);
                if (hitCeil.collider != null && hitCeil.distance < Mathf.Abs(verticalSpeed * Time.deltaTime) + 0.01f)
                {
                    transform.Translate(new Vector3(0, hitCeil.distance));
                    verticalSpeed = 0;
                }
            }
            else
            {
                verticalSpeed -= currentJump.downAcceleration * Time.deltaTime;
                RaycastHit2D hitFloor = Physics2D.Raycast(m_GroundCheck.position, Vector2.down);
                if (hitFloor.collider != null && hitFloor.distance < Mathf.Abs(verticalSpeed * Time.deltaTime) + 0.01f)
                {
                    transform.Translate(new Vector3(0, -hitFloor.distance));
                    verticalSpeed = 0;
                    m_Grounded = true;
                }
            }
            if (inputFastFall)
                fastFall = true;
            if (!fastFall && verticalSpeed <= -maxFallSpeed)
            {
                verticalSpeed = -maxFallSpeed;
            }
            if (fastFall && verticalSpeed < 0)
            {
                verticalSpeed = -fastFallSpeed;
            }
        }
    }
    private void CrouchUpdate(bool crouch)
    {
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            //if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            //{
            //    crouch = true;
            //}
        }
        if (m_Grounded)
        {
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier

                // Disable one of the colliders when crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }
        }
    }
    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
}