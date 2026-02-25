using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class UIInteractableTest : MonoBehaviour
{
    [SerializeField] private float _interactionDistance = 2.0f;
    [SerializeField] private UIController _uiController;
    [SerializeField] private string[] _lines;

    private int _currentLine;
    private bool _runningDialogue;
    private bool _waitingForPlayerResponse;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.Player == null) return;

        if (Vector3.Distance(transform.position, GameController.Instance.Player.transform.position) < _interactionDistance)
        {
            if (!_waitingForPlayerResponse && Input.GetMouseButtonDown(0))
            {
                if (gameObject.CompareTag("NPC"))
                {
                    AdvanceDialogue();
                } else if (gameObject.CompareTag("Interactable")) {
                    Collect();
                }
            }
        } else
        {
            _runningDialogue = false;
            _waitingForPlayerResponse = false;
            _currentLine = 0;
            _uiController.HideDialogue();
        }
    }

    private void AdvanceDialogue()
    {
        _runningDialogue = true;

        if (_currentLine >= _lines.Length)
        {
            //Show player options
            _uiController.ShowPlayerOptions();
            _waitingForPlayerResponse = true;
        } else
        {
            // continue showing dialogue lines
            _uiController.ShowDialogue(_lines[_currentLine]);
        }

        _currentLine++;
    }

    private void Collect()
    {
        Destroy(gameObject);
    }
}
