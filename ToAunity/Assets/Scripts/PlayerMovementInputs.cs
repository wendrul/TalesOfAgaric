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

    void Start()
    {
        character = GetComponent<CharacterController2D>();
        jumpReleased = true;
    }

    // Update is called once per frame
    void Update()
    {
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
        move = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        if (vertical < -0.5f)
            crouch = true;
        else
            crouch = false;
        print(vertical);
        character.Move(move, crouch, jump, vertical, jumpReleased, shortHop);
        jump = false;
        shortHop = false;
    }
}
