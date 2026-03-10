using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Freddy : NPC
{
    public enum FreddyState
    {
        IsInteractable, IsJumpscaring, IsMoving
    }

    private FreddyState _state;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        //setting this just for dialogue testing purposes -jess
        _state = FreddyState.IsMoving;
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        UpdateState();
        UpdateBehavior();
    }

    private void UpdateState()
    {
        if (QuestCheck())
        {
            _state = FreddyState.IsInteractable;
        }
    }

    public override void OnMouseOver()
    {
        if (_state == FreddyState.IsInteractable)
        {
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
                base.OnMouseOver();
                break;
            case FreddyState.IsJumpscaring:
                break;
            case FreddyState.IsMoving:
                break;
        }
    }

    public override bool QuestCheck()
    {
        if (GameController.Instance.Player._currentPageCount == GameController.Instance.Player._maxPageCount - 1)
        {
            _questComplete = true;
        } else
        {
            _questComplete = false;
        }

        return _questComplete;
    }
}
