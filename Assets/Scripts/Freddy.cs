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

        // currently set this just for testing purposes.. feel free to remove this if it conflicts with the AI -jess
        _state = FreddyState.IsMoving;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        UpdateState();
        UpdateBehavior();

        if (!_slenderman.GetComponent<NPC>().enabled && GameController.Instance.Player._questState == PlayerController.QuestState.Quest3Started)
        {
            _quest3Complete = true;
            GameController.Instance.Player._questState = PlayerController.QuestState.Quest3Complete;
        }
        else
        {
            _quest3Complete = false;
        }
    }

    // Update Freddy's state
    private void UpdateState()
    {
        if (GameController.Instance.Player._currentPageCount >= GameController.Instance.Player._maxPageCount - 1)
        {
            // If the player is currently missing the last page, make Freddy interactable
            _state = FreddyState.IsInteractable;
        }
    }

    public override void OnMouseOver()
    {
        if (_state == FreddyState.IsInteractable)
        {
            // If Freddy is currently interactable, do normal NPC interaction
            base.OnMouseOver();
        }
        else
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
                _flyer.SetActive(false);
                break;
            case FreddyState.IsMoving:
                _movementSpeed = 2.0f;
                _flyer.SetActive(false);
                break;
        }
    }

    protected override void AdvanceDialogue()
    {
        if (!_uiController._isTyping && gameObject.GetComponent<NPC>().enabled == true)
        {
            _runningDialogue = true;

            if (_currentLine < _currentNode._lines.Length)
            {
                // keep playing NPC lines if there are still any left
                _uiController.ShowDialogue(_currentNode._lines[_currentLine], _name);
                _currentLine++;
                _canContinue = true;
            }
            else if (_currentNode._playerReplyOptions != null && _currentNode._playerReplyOptions.Length > 0)
            {
                // show player dialogue options, if any
                if (GameController.Instance.Player._questState == PlayerController.QuestState.Quest3Complete)
                {
                    _uiController._questActive = false;
                }
                else if (GameController.Instance.Player._questState == PlayerController.QuestState.Quest3Started)
                {
                    _uiController._questActive = true;
                }
                _waitingForPlayerResponse = true;
                _uiController.ShowPlayerOptions(_currentNode._playerReplyOptions);
                _canContinue = false;
            }
            else
            {
                // end dialogue if none left
                EndDialogue();
                _canContinue = true;
            }
        }
    }

    protected override void EndDialogue()
    {
        _waitingForPlayerResponse = false;

        if (_currentNode._questTrigger)
        {
            _currentNode = _questInProgressNode;

            // Start Freddy's quest if the player has chosen to
            GameController.Instance.Player._questState = PlayerController.QuestState.Quest3Started;

            if (!_quest3Complete)
            {
                _uiController._questActive = true;
            }
            else
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
                // Make the flyer collectable if the player has rejected Freddy's quest
                if (GameController.Instance.Player._questState != PlayerController.QuestState.Quest3Complete)
                {
                    _flyer.GetComponent<Interactable>().enabled = true;
                }
                _canContinue = false;
                gameObject.GetComponent<NPC>().enabled = false;
            }

            _uiController._questActive = false;
        }
        _currentLine = 0;
        _runningDialogue = false;
        _uiController.HideDialogue();
        GameController.Instance.Player.SetState(PlayerController.PlayerState.Normal);
    }
}
