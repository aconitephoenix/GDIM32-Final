using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class UIInteractableTest : MonoBehaviour
{
    [SerializeField] private float _interactionDistance = 2.0f;
    [SerializeField] private UIController _uiController;
    [SerializeField] private string _name;
    [SerializeField] private DialogueNode _startingNode;

    private DialogueNode _currentNode;
    private int _currentLine = 0;
    private bool _waitingForPlayerResponse;
    private bool _runningDialogue;

    // Start is called before the first frame update
    void Start()
    {
        _currentNode = _startingNode;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.Player == null) return;
    }

    private void OnMouseOver()
    {
        // Checking if player is within interaction distance
        if (Vector3.Distance(transform.position, GameController.Instance.Player.transform.position) <= _interactionDistance && gameObject != null)
        {
            // Player interaction once they press E or click the mouse
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                if (!_waitingForPlayerResponse && gameObject.CompareTag("NPC"))
                {
                    AdvanceDialogue();
                }
                else if (gameObject.CompareTag("Interactable"))
                {
                    Collect();
                }
                else
                {
                    EndDialogue();
                }
            }

            // Disabling hover text if dialogue is currently playing
            if (!_runningDialogue)
            {
                _uiController.HandleHoverText(gameObject.tag);
            } else
            {
                _uiController.HandleHoverText("Untagged");
            }
        }
    }

    // Disabling hover text once player has looked away from the object
    private void OnMouseExit()
    {
        _uiController.HandleHoverText("Untagged");
    }

    private void AdvanceDialogue()
    {
        _runningDialogue = true;

        if (_currentLine < _currentNode._lines.Length)
        {
            // keep playing NPC lines if there are still any left
            _uiController.ShowDialogue(_currentNode._lines[_currentLine], _name);
            _currentLine++;
        }
        else if (_currentNode._playerReplyOptions != null && _currentNode._playerReplyOptions.Length > 0)
        {
            // show player dialogue options, if any
            _waitingForPlayerResponse = true;
            _uiController.ShowPlayerOptions(_currentNode._playerReplyOptions);
        }
        else
        {
            // end dialogue if none left
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        _waitingForPlayerResponse = false;
        _currentNode = _startingNode;
        _currentLine = 0;
        _runningDialogue = false;
        _uiController.HideDialogue();
    }

    public void SelectedOption(int option)
    {
        _currentLine = 0;
        _waitingForPlayerResponse = false;

        _currentNode = _currentNode._npcReplies[option];
        AdvanceDialogue();
    }

    private void Collect()
    {
        _uiController.HandleHoverText("Untagged");
        _uiController.UpdatePageNumber();
        Destroy(gameObject);
    }
}
