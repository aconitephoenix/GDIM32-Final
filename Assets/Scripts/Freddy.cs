using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freddy : NPC
{
    public enum FreddyState
    {
        IsInteractable, IsJumpscaring, IsMoving
    }

    [SerializeField] private float _movementSpeed = 2.0f;
    [SerializeField] private GameObject _flyer;
    [SerializeField] private GameObject _slenderman;

    private FreddyState _state;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        UpdateState();
        UpdateBehavior();
    }

    // Update Freddy's state
    private void UpdateState()
    {
        if (GameController.Instance.Player._questState == PlayerController.QuestState.Quest1Complete)
        {
            _state = FreddyState.IsInteractable;
        }
    }

    public override void OnMouseOver()
    {
        if (_state == FreddyState.IsInteractable)
        {
            // If Freddy is currently interactable, do normal NPC interaction
            base.OnMouseOver();
        } else
        {
            return;
        }
    }

    private void UpdateBehavior()
    {
        switch (_state)
        {
            case FreddyState.IsInteractable:
                _movementSpeed = 0.0f;
                _flyer.SetActive(true);
                _flyer.GetComponent<Interactable>().enabled = false;
                break;
            case FreddyState.IsJumpscaring:
                break;
            case FreddyState.IsMoving:
                _movementSpeed = 2.0f;
                break;
        }
    }

    protected override void EndDialogue()
    {
        _waitingForPlayerResponse = false;

        if (_currentNode._questTrigger)
        {
            _currentNode = _questInProgressNode;

            GameController.Instance.Player._questState = PlayerController.QuestState.Quest3Started;
            
            if (!_quest1Complete)
            {
                if (!_quest3Complete)
                {
                    _uiController._questActive = true;
                }
                else
                {
                    _uiController._questActive = false;
                }
            } else
            {
                _uiController._questActive = false;
            }
            
        }
        else
        {
            if (!_currentNode._questComplete)
            {
                _currentNode = _startingNode;
            }
            else
            {
                _canContinue = false;
                _flyer.GetComponent<Interactable>().enabled = true;
                gameObject.GetComponent<NPC>().enabled = false;
            }

            _uiController._questActive = false;
        }
        _currentLine = 0;
        _runningDialogue = false;
        _uiController.HideDialogue();
        GameController.Instance.Player.SetState(PlayerController.PlayerState.Normal);
    }

    public override void QuestCheck()
    {
        base.QuestCheck();

        
    }
}
