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
