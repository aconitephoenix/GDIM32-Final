using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : Interactable
{
    [SerializeField] protected string _name;
    public DialogueNode _startingNode;
    [SerializeField] protected DialogueNode _questInProgressNode;
    [SerializeField] public List<AudioClip> _dialogueAudioClips = new List<AudioClip>();
    [SerializeField] private DialogueAudioController _dialogueAudioController;

    protected DialogueNode _currentNode;
    protected int _currentLine = 0;
    protected bool _waitingForPlayerResponse;
    protected bool _runningDialogue;
    protected bool _canContinue;

    // Start is called before the first frame update
    public virtual void Start()
    {
        _currentNode = _startingNode;
        _canContinue = true;
        GameController.Instance.Player.PageCollected += QuestCheck;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (GameController.Instance.Player == null) return;
    }

    public override void OnMouseOver()
    {
        // Checking if player is within interaction distance
        if (Vector3.Distance(transform.position, GameController.Instance.Player.transform.position) <= _interactionDistance && gameObject.GetComponent<NPC>().enabled == true)
        {
            // Player interaction once they press E or click the mouse
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                GameController.Instance.Player.SetState(PlayerController.PlayerState.InDialogue);

                if (!_waitingForPlayerResponse && (!_currentNode._questComplete || _currentLine < _currentNode._lines.Length))
                {
                    AdvanceDialogue();

                    _dialogueAudioController.RemoveAudioClips();

                    if (_dialogueAudioClips.Count > 0)
                    {
                        _dialogueAudioController.AddAudioClips(_dialogueAudioClips);
                    }
                    else
                    {
                        _dialogueAudioController.RemoveAudioClips();
                    }

                }
                else if (_canContinue)
                {
                    EndDialogue();
                    _dialogueAudioController.RemoveAudioClips();
                }
            }

            // Disabling hover text if dialogue is currently playing/quest has been finished
            // otherwise, enable hover text if player is close enough
            if (!_runningDialogue && !_currentNode._questComplete)
            {
                _uiController.HandleHoverText(gameObject.tag);
            }
        }
        else
        {
            _uiController.HandleHoverText("Untagged");
        }
    }

    protected void AdvanceDialogue()
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

    protected void EndDialogue()
    {
        _waitingForPlayerResponse = false;
        if (_currentNode._questTrigger)
        {
            _currentNode = _questInProgressNode;
            _uiController._questActive = true;
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
                gameObject.GetComponent<NPC>().enabled = false;
            }

            _uiController._questActive = false;
        }
        _currentLine = 0;
        _runningDialogue = false;
        _uiController.HideDialogue();
        GameController.Instance.Player.SetState(PlayerController.PlayerState.Normal);

    }

    // Which option the player chose
    public void SelectedOption(int option)
    {
        if (!_uiController._isTyping)
        {
            _currentLine = 0;
            _waitingForPlayerResponse = false;
            _canContinue = true;

            if (option < _currentNode._npcReplies.Length)
            {
                _currentNode = _currentNode._npcReplies[option];
                AdvanceDialogue();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    // Check if NPC quest is complete
    public virtual void QuestCheck()
    {
        //temporarily put this here as i figure out inheritance -jess
        if (GameController.Instance.Player._currentPageCount >= GameController.Instance.Player._maxPageCount)
        {
            _uiController._questActive = false;
        }
        else
        {
            _uiController._questActive = true;
        }
    }
}
