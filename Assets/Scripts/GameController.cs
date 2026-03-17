using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    public PlayerController Player { get; private set; }

    [SerializeField] private GameObject _interactableFreddy;
    [SerializeField] private GameObject _arrow;

    //Setting up player locator
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        GameObject playerObj = GameObject.FindWithTag("Player");
        Player = playerObj.GetComponent<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _interactableFreddy.SetActive(false);
        if (_arrow != null)
        {
            _arrow.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Instance.Player._currentPageCount >= Instance.Player._maxPageCount - 1)
        {
            // If the player is currently missing the last page, make Freddy interactable
            _interactableFreddy.SetActive(true);
        }

        // Activate arrow when Slenderman's quest starts
        if (Instance.Player._questState == PlayerController.QuestState.Quest3Started && _arrow != null)
        {
            _arrow.SetActive(true);
        }
    }
}
