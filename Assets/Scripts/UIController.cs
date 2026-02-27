using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private TMP_Text _pagesText;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private TMP_Text _hoverText;
    [SerializeField] private GameObject _dialogueBox;
    [SerializeField] private GameObject _playerOptions;
    [SerializeField] private GameObject _sprintBar;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _option1;
    [SerializeField] private TMP_Text _option2;

    private int _pageNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        //GameController.Instance.Player.InteractableDetected += HandleHoverText;
        _pagesText.text = "Pages: " + _pageNumber + "/8";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Display text when player hovers over objects
    public void HandleHoverText(string tag)
    {
        switch (tag)
        {
            case "NPC":
                _hoverText.text = "Click or press E to talk";
                _hoverText.gameObject.SetActive(true);
                break;
            case "Interactable":
                _hoverText.text = "Click or press E to interact";
                _hoverText.gameObject.SetActive(true);
                break;
            default:
                _hoverText.gameObject.SetActive(false);
                break;
        }
    }

    public void ShowDialogue(string dialogue, string name)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _dialogueBox.SetActive(true);
        _playerOptions.SetActive(false);
        _sprintBar.SetActive(false);
        _hoverText.gameObject.SetActive(false);

        _dialogueText.text = dialogue;
        _nameText.text = name;
    }

    public void HideDialogue()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _dialogueBox.SetActive(false);
        _playerOptions.SetActive(false);
        _sprintBar.SetActive(true);
    }

    public void ShowPlayerOptions(string[] options)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerOptions.SetActive(true);
        _sprintBar.SetActive(false);
        _hoverText.gameObject.SetActive(false);

        _option1.text = options[0];

        if (options.Length >= 2)
        {
            _option2.transform.parent.gameObject.SetActive(true);
            _option2.text = options[1];
        }
        else
        {
            _option2.transform.parent.gameObject.SetActive(false);
        }
    }

    public void UpdatePageNumber()
    {
        _pageNumber++;
        _pagesText.text = "Pages: " + _pageNumber + "/8";
    }
}
