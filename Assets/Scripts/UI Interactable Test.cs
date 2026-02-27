using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class UIInteractableTest : MonoBehaviour
{
    [SerializeField] protected float _interactionDistance = 2.0f;
    [SerializeField] protected UIController _uiController;

    // Start is called before the first frame update
    void Start()
    {
        
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

            _uiController.HandleHoverText(gameObject.tag);
        }
    }

    // Disabling hover text once player has looked away from the object
    public void OnMouseExit()
    {
        _uiController.HandleHoverText("Untagged");
    }

    private void Collect()
    {
        _uiController.HandleHoverText("Untagged");
        _uiController.UpdatePageNumber();
        Destroy(gameObject);
    }
}
