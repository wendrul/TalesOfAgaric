using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementInputs : MonoBehaviour
{
    // Start is called before the first frame update
    private CharacterController2D character;
    private bool jump;
    private float move;
    private float vertical;
    private bool jumpReleased;
    private bool crouch;
    private bool shortHop;
    private bool fastFall;
    private bool verticalReleased;
    private float deadZone;

    void Start()
    {
        character = GetComponent<CharacterController2D>();
        jumpReleased = true;
        verticalReleased = true;
    }

    // Update is called once per frame
    void Update()
    {
        move = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        if (jumpReleased && Input.GetKeyDown("space"))
        {
            jump = true;
            jumpReleased = false;
        }
        if (Input.GetKeyDown("k"))
            shortHop = true;
        if (Input.GetKeyUp("space"))
        {
            jumpReleased = true;
        }
        if (verticalReleased && vertical < -0.5f)
        {
            fastFall = true;
            verticalReleased = false;
        }
        if (vertical > -0.5f)
            verticalReleased = true;
        if (vertical < -0.5f)
            crouch = true;
        else
            crouch = false;

        character.Move(move, crouch, jump, vertical, jumpReleased, shortHop, fastFall);
        
        jump = false;
        shortHop = false;
        fastFall = false;
    }
}
