using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    private DialogueNode _activeDialogue;

    public void SetActiveNpc(UINPCTest npc) 
    {
        _activeDialogue = npc._startingNode;
    }


}
