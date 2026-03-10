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
    }

    public override void OnMouseOver()
    {
        base.OnMouseOver();
    }

    public override bool QuestCheck()
    {
        if (GameController.Instance.Player._currentPageCount >= GameController.Instance.Player._maxPageCount)
        {
            _questComplete = true;
        } else
        {
            _questComplete = false;
        }

        return _questComplete;
    }
}
