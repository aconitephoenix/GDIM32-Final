using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : Interactable
{
    [SerializeField] private Transform _destination;
    [SerializeField] private Vector3 _offset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.Player == null) return;
    }

    public override void OnMouseOver()
    {
        // Checking if player is within interaction distance
        if (Vector3.Distance(transform.position, GameController.Instance.Player.transform.position) <= _interactionDistance)
        {
            // Player interaction once they press E or click the mouse
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                GameController.Instance.Player.transform.position = _destination.position + _offset;
            }

            _uiController.HandleHoverText(gameObject.tag);

        }
        else 
        {
            _uiController.HandleHoverText("Untagged");
        }
    }
}
