using UnityEngine;

[CreateAssetMenu(fileName = "DialogueLine", menuName = "ScriptableObjects/DialogueLine", order = 1)]
public class DialogueNode : ScriptableObject
{
    // lines of dialogue the NPC says this node
    public string[] _lines;

    // potential player reply options
    public string[] _playerReplyOptions;

    // player replies correspond with npc replies
    public DialogueNode[] _npcReplies;

    // whether or not this node triggers a quest
    public bool _questTrigger;
}