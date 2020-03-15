using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    private float m_CrouchSpeed = .36f;          // Amount of maxSpeed applied to crouching movement. 1 = 100%
    //[SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    public Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    public Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    //[SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    private bool m_Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    private bool m_FacingRight = true;  // For determining which way the player is currently facing.

    private bool fastFall;

    private float runSpeed = 0;
    private float airSpeed = 0;
    private float jumpSpeed = 10;
    private float jumpHeight = 80;
    private float maxFallSpeed = 10;
    private float fastFallSpeed = 20;
    private float descendingAcceleration = 3;
    private float forceStopAcceleration;
    private float shortHopTimeLimit;
    private float airTime;
    private float ascendingAcceleration;
    private bool shortHop;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;
    private float verticalSpeed;



    private void Awake()
    {
        verticalSpeed = 0f;
        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
        ascendingAcceleration = jumpSpeed * jumpSpeed / (2 * jumpHeight);
    }

    private void Update()
    {
        ascendingAcceleration = jumpSpeed * jumpSpeed / (2 * jumpHeight);
        //remove line above after debug

        /*bool wasGrounded = m_Grounded;
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
        }*/
    }

    private void verticalMovement(bool hopReleased, float vertical)
    {
        if (!m_Grounded)
        {
            print($"runSpeed {runSpeed}");
            print($"airSpeed {airSpeed}");
            print($"jumpSpeed {jumpSpeed}");
            print($"jumpHeight {jumpHeight} = 80;");
            print($"maxFallSpeed {maxFallSpeed} = 10;");
            print($"fastFallSpeed {fastFallSpeed} = 20;");
            print($"descendingAcceleration {descendingAcceleration}");
            airTime += Time.deltaTime;
            if (verticalSpeed > 0)
            {
                verticalSpeed -= ascendingAcceleration;
                RaycastHit2D hitCeil = Physics2D.Raycast(m_CeilingCheck.position, Vector2.up);
                if (hitCeil.collider != null && hitCeil.distance < Mathf.Abs(verticalSpeed * Time.deltaTime) + 0.1f)
                {
                    transform.Translate(new Vector3(0, hitCeil.distance));
                    verticalSpeed = 0;
                }
            }
            else
            {
                verticalSpeed -= descendingAcceleration;
                RaycastHit2D hitFloor = Physics2D.Raycast(m_GroundCheck.position, Vector2.down);
                if (hitFloor.collider != null && hitFloor.distance < Mathf.Abs(verticalSpeed * Time.deltaTime) + 0.1f)
                {
                    transform.Translate(new Vector3(0, -hitFloor.distance));
                    verticalSpeed = 0;
                    m_Grounded = true;
                }
            }
            if (vertical < -0.5)
                fastFall = true;
            if (!fastFall && verticalSpeed <= -maxFallSpeed)
            {
                verticalSpeed = -maxFallSpeed;
            }
            if (fastFall && verticalSpeed <= -fastFallSpeed)
            {
                verticalSpeed = -fastFallSpeed;
            }
        }
    }

    public void Move(float move, bool crouch, bool jump, float vertical, bool hopReleased)
    {
        // If crouching, check to see if the character can stand up
        verticalMovement(hopReleased, vertical);
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            //if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            //{
            //    crouch = true;
            //}
        }

        //only control the player if grounded or airControl is turned on
        if (m_Grounded)
        {

            // If crouching
            /*if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= m_CrouchSpeed;

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
            */
            
            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && m_FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            //move the character horizontally
            transform.Translate(new Vector3(move * runSpeed * Time.deltaTime, verticalSpeed * Time.deltaTime));

        }
        else
        {
            transform.Translate(new Vector3(move * airSpeed * Time.deltaTime, verticalSpeed * Time.deltaTime));
        }

        if (m_Grounded && jump)
        {
            airTime = 0;
            shortHop = false;
            m_Grounded = false;
            fastFall = false;
            verticalSpeed = jumpSpeed;
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