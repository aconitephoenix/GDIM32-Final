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

    private FreddyState _state;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        //setting this just for dialogue testing purposes -jess
        _state = FreddyState.IsInteractable;
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
        if (!_uiController._questActive)
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
                break;
            case FreddyState.IsJumpscaring:
                break;
            case FreddyState.IsMoving:
                _movementSpeed = 2.0f;
                break;
        }
    }

    public override void QuestCheck()
    {
        if (GameController.Instance.Player._currentPageCount == GameController.Instance.Player._maxPageCount - 1 && GameController.Instance.Player._currentPageCount > 0)
        {
            _uiController._questActive = false;
        } else
        {
            _uiController._questActive = true;
        }
    }
}
