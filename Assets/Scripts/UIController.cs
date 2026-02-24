using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text _pagesText;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private TMP_Text _hoverText;
    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.Player.InteractableDetected += HandleHoverText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Display text when player hovers over objects
    private void HandleHoverText(string tag)
    {
        switch (tag)
        {
            case "NPC":
                _hoverText.text = "Click to talk";
                _hoverText.gameObject.SetActive(true);
                break;
            case "Interactable":
                _hoverText.text = "Click to interact";
                _hoverText.gameObject.SetActive(true);
                break;
            default:
                _hoverText.gameObject.SetActive(false);
                break;
        }
    }
}
