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
    [SerializeField] private TMP_Text _continueDialogueText;
    [SerializeField] private GameObject _playerOptions;
    [SerializeField] private GameObject _sprintBar;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _option1;
    [SerializeField] private TMP_Text _option2;
    [SerializeField] private float _typingSpeed = 0.04f;

    private Coroutine _typeLineCoroutine;
    public bool _isTyping;
    public bool _questActive;

    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.Player.PageCollected += UpdatePageNumber;
        _pagesText.text = "Pages: 0/" + GameController.Instance.Player._maxPageCount;
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

    // Show dialogue box
    public void ShowDialogue(string dialogue, string name)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _crosshair.SetActive(false);
        _dialogueBox.SetActive(true);
        _playerOptions.SetActive(false);
        _sprintBar.SetActive(false);
        _hoverText.gameObject.SetActive(false);
        _continueDialogueText.gameObject.SetActive(false);
        
        if (_typeLineCoroutine != null)
        {
            StopCoroutine(_typeLineCoroutine);
        }

        _typeLineCoroutine = StartCoroutine(TypeLine(dialogue));

        _nameText.text = name;
    }

    // type dialogue letter by letter
    private IEnumerator TypeLine(string dialogue)
    {
        _isTyping = true;

        _dialogueText.text = dialogue;
        _dialogueText.maxVisibleCharacters = 0;

        for (int i = 1; i < dialogue.Length + 1; i++)
        {
            // skip to the end of the line (i'll fix this later) -jess
            if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)) && i > 1)
            {
                _dialogueText.maxVisibleCharacters = dialogue.Length + 1;
                break;
            }

            _dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(_typingSpeed);
        }

        _isTyping = false;
        _continueDialogueText.gameObject.SetActive(true);
    }
    

    // Hide dialogue box
    public void HideDialogue()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _crosshair.SetActive(true);
        _dialogueBox.SetActive(false);
        _playerOptions.SetActive(false);
        _sprintBar.SetActive(true);
    }

    // Show the player dialogue options
    public void ShowPlayerOptions(string[] options)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerOptions.SetActive(true);
        _sprintBar.SetActive(false);
        _hoverText.gameObject.SetActive(false);

        _option1.text = options[0];

        // If there is more than 1 dialogue option, add a second button
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

    // Update page count
    public void UpdatePageNumber()
    {
        _pagesText.text = "Pages: " + GameController.Instance.Player._currentPageCount + "/" + GameController.Instance.Player._maxPageCount;
    }
}
