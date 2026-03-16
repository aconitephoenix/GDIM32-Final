using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    public TMP_Text _pagesText;
    [SerializeField] private TMP_Text _dialogueText;
    [SerializeField] private TMP_Text _hoverText;
    [SerializeField] private GameObject _dialogueBox;
    [SerializeField] private TMP_Text _continueDialogueText;
    [SerializeField] private GameObject _playerOptions;
    [SerializeField] private GameObject _sprintBar;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private GameObject _compass;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _option1;
    [SerializeField] private TMP_Text _option2;
    [SerializeField] private float _typingSpeed = 0.04f;
    [SerializeField] private Button _dialogueButton1;
    [SerializeField] private Button _dialogueButton2;
    [SerializeField] private DialogueAudioController _dialogueAudioController;

    public GameObject CurrentNPC;

    private Coroutine _typeLineCoroutine;
    public bool _isTyping;
    public bool _questActive = false;

    private bool _skipDialogue = false;
    private bool _canSkip = false;

    // Start is called before the first frame update
    void Start()
    {
        GameController.Instance.Player.PageCollected += UpdatePageNumber;
        GameController.Instance.Player.NPCDetected += SetNPC;
        _pagesText.text = "Pages: 0/" + GameController.Instance.Player._maxPageCount;
    }

    public void SetNPC(GameObject npc)
    {
        CurrentNPC = npc;
    }

    // Display text when player hovers over objects
    public void HandleHoverText(string tag)
    {
        if (tag != "Untagged")
        {
            if (tag == "NPC")
            {
                NPC npc = CurrentNPC.GetComponent<NPC>();
                if (npc != null)
                {
                    _hoverText.text = npc.Name;
                }
                else
                {
                    _hoverText.text = "NPC";
                }
            }
            else if (tag == "Interactable")
            {
                _hoverText.text = "Page";
            }
            else if (tag == "Door")
            {
                _hoverText.text = "Door";
            }

            _hoverText.gameObject.SetActive(true);
        }
        else
        {
            _hoverText.gameObject.SetActive(false);
        }
    }

    // Show dialogue box
    public void ShowDialogue(string dialogue, string name)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _crosshair.SetActive(false);
        if (_compass != null)
        {
            _compass.SetActive(false);
        }
        _dialogueBox.SetActive(true);
        _playerOptions.SetActive(false);
        _sprintBar.SetActive(false);
        _hoverText.gameObject.SetActive(false);
        _continueDialogueText.gameObject.SetActive(false);

        if (_typeLineCoroutine != null)
        {
            StopCoroutine(_typeLineCoroutine);
        }

        _canSkip = false;
        _typeLineCoroutine = StartCoroutine(TypeLine(dialogue));

        _nameText.text = name;
    }

    // type dialogue letter by letter
    private IEnumerator TypeLine(string dialogue)
    {
        _isTyping = true;
        _skipDialogue = false;

        _dialogueText.text = dialogue;
        _dialogueText.maxVisibleCharacters = 0;

        yield return new WaitForEndOfFrame();
        _canSkip = true;

        for (int i = 0; i < dialogue.Length + 1; i++)
        {
            if (_skipDialogue)
            {
                _dialogueText.maxVisibleCharacters = dialogue.Length + 1;
                _skipDialogue = false;
                _dialogueAudioController.StopClip();
                _dialogueAudioController.RemoveAudioClips();
                _continueDialogueText.gameObject.SetActive(true);
                _isTyping = false;
                yield break;
            }

            _dialogueText.maxVisibleCharacters = i;
            _dialogueAudioController.SetClip();
            _dialogueAudioController.PlayClip();
            yield return new WaitForSeconds(_typingSpeed);
        }

        _isTyping = false;
        _skipDialogue = false;
        _dialogueAudioController.StopClip();
        _dialogueAudioController.RemoveAudioClips();
        _continueDialogueText.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (_isTyping && _canSkip && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)))
        {
            _skipDialogue = true;
        }
    }

    // Hide dialogue box
    public void HideDialogue()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _crosshair.SetActive(true);
        if (_compass != null)
        {
            _compass.SetActive(true);
        }
        _dialogueAudioController.RemoveAudioClips();
        _dialogueBox.SetActive(false);
        _playerOptions.SetActive(false);
        _sprintBar.SetActive(true);
        //remove all listeners here
        _dialogueButton1.onClick.RemoveAllListeners();
        _dialogueButton2.onClick.RemoveAllListeners();
    }

    // Show the player dialogue options
    public void ShowPlayerOptions(string[] options)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerOptions.SetActive(true);
        _sprintBar.SetActive(false);
        _hoverText.gameObject.SetActive(false);
        _dialogueAudioController.RemoveAudioClips();
        _dialogueButton1.onClick.AddListener(delegate { CurrentNPC.gameObject.GetComponent<NPC>().SelectedOption(0); });

        _option1.text = options[0];

        // If there is more than 1 dialogue option, add a second button
        if (options.Length >= 2 && !_questActive)
        {
            _option2.transform.parent.gameObject.SetActive(true);

            // Changing the second option's text + resulting dialogue depending on the quest condition
            if (options.Length >= 4 && GameController.Instance.Player._questState == PlayerController.QuestState.Quest3Started)
            {
                _option2.text = options[3];
                _dialogueButton2.onClick.AddListener(delegate { CurrentNPC.gameObject.GetComponent<NPC>().SelectedOption(3); });
            }
            else if (options.Length >= 3 && GameController.Instance.Player._questState == PlayerController.QuestState.Quest2Complete)
            {
                _option2.text = options[2];
                _dialogueButton2.onClick.AddListener(delegate { CurrentNPC.gameObject.GetComponent<NPC>().SelectedOption(2); });
            } else
            {
                _option2.text = options[1];
                _dialogueButton2.onClick.AddListener(delegate { CurrentNPC.gameObject.GetComponent<NPC>().SelectedOption(1); });
            }
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

    // Set a different font for specific NPC dialogue
    public void SetDialogueFont(TMP_FontAsset font)
    {
        if (font != null)
        {
            _dialogueText.font = font;
        } else
        {
            _dialogueText.font = default;
        }
    }
}
