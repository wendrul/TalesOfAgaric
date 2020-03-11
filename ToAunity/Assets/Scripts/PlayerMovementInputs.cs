using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementInputs : MonoBehaviour
{
    // Start is called before the first frame update
    private CharacterController2D character;
    private bool hop;
    private float move;

    void Start()
    {
        character = GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            hop = true;
        }
        move = Input.GetAxis("Horizontal");
        character.Move(move, false, hop);
        hop = false;
    }
}
