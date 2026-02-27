using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class UIInteractableTest : MonoBehaviour
{
    [SerializeField] protected float _interactionDistance = 2.0f;
    [SerializeField] protected UIController _uiController;

    private bool _collected;

    // Start is called before the first frame update
    void Start()
    {
        _collected = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.Player == null) return;
    }

    public virtual void OnMouseOver()
    {
        // Checking if player is within interaction distance
        if (Vector3.Distance(transform.position, GameController.Instance.Player.transform.position) <= _interactionDistance)
        {
            // Player interaction once they press E or click the mouse
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                Collect();
            }

            // Enable hover text if player is close enough and item has not been collected
            if (!_collected)
            {
                _uiController.HandleHoverText(gameObject.tag);
            }
            else
            {
                _uiController.HandleHoverText("Untagged");
            }
        }
    }

    // Disabling hover text once player has looked away from the object
    public void OnMouseExit()
    {
        _uiController.HandleHoverText("Untagged");
    }

    // Collect the item
    private void Collect()
    {
        _collected = true;
        _uiController.UpdatePageNumber();
        Destroy(gameObject);
    }
}
