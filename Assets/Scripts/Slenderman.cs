using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slenderman : NPC
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        //every frame rotate towards player
        if (GameController.Instance.Player != null)
        {
            Vector3 direction = GameController.Instance.Player.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
    }

    public override void OnMouseOver()
    {
        base.OnMouseOver();
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
                if (GameController.Instance.Player._questState == PlayerController.QuestState.Quest3Started || GameController.Instance.Player._questState == PlayerController.QuestState.Quest2Complete)
                {
                    _uiController._questActive = false;
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
        base.EndDialogue();
    }

    public override void QuestCheck()
    {
        base.QuestCheck();

        if (GameController.Instance.Player._currentPageCount >= GameController.Instance.Player._maxPageCount)
        {
            _quest2Complete = true;
            _uiController._questActive = false;
            GameController.Instance.Player._questState = PlayerController.QuestState.Quest2Complete;
        } else
        {
            _quest2Complete = false;
        }
    }
}
