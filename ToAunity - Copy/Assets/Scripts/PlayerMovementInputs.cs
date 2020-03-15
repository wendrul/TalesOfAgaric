using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementInputs : MonoBehaviour
{
    // Start is called before the first frame update
    private CharacterController2D character;
    private bool hop;
    private float move;
    private float vertical;
    private bool hopReleased;

    void Start()
    {
        character = GetComponent<CharacterController2D>();
        hopReleased = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (hopReleased && Input.GetKeyDown("space"))
        {
            hop = true;
            hopReleased = false;
        }
        if (Input.GetKeyUp("space"))
        {
            hopReleased = true;
        }
        move = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        character.Move(move, false, hop, vertical, hopReleased);
        hop = false;
    }
}
