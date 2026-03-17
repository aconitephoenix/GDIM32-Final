using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageCompass : MonoBehaviour
{
    [SerializeField] private Transform[] _pageTransform; 
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private Transform freddyTransform;

    private Transform _closestPage;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _closestPage = _pageTransform[0];

        // Get CanvasGroup component for visibility control
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Initially hide the compass
        _canvasGroup.alpha = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.Player == null) return;

        // Check if player has 7/8 pages - point to Freddy's transformation
        if (GameController.Instance.Player._currentPageCount >= GameController.Instance.Player._maxPageCount - 1)
        {
            
            
            if (freddyTransform != null)
            {
                RotateTowardsFreddy(freddyTransform);
                return;
            }
        }

        //Checks if the closest page exists. If it doesn't, sets new transform as closest page
        int n = 0;
        while (_closestPage == null && n < _pageTransform.Length) 
        {
            if (_pageTransform[n] != null)
            {
                _closestPage = (Transform)_pageTransform[n];
            }
            n++;
        }

        //Checks if closest page is still null after the while loop. If it is, kill the gameObject
        if (_closestPage == null)
        {
            Destroy(gameObject);
        }
        else
        {

            for (int i = 0; i < _pageTransform.Length; i++)
            {
                if (_pageTransform[i] != null)
                {
                    //compares the distance between two pages, closest to the player gets set as closest page
                    float currentDist = Vector3.Distance(transform.position, _closestPage.position);
                    float otherDist = Vector3.Distance(transform.position, _pageTransform[i].position);

                    if (otherDist < currentDist)
                    {
                        _closestPage = (Transform)_pageTransform[i];
                    }
                }
            }

            //Handles the arrow's rotation
            Vector3 compassPos = new Vector3(GameController.Instance.Player.transform.position.x, 0, GameController.Instance.Player.transform.position.z);

            Vector3 pagePos = new Vector3(_closestPage.position.x, 0, _closestPage.position.z);
            Vector3 playertoPage = (pagePos - compassPos).normalized;

            RotateTowards(playertoPage);
        }
    }

    private void RotateTowards(Vector3 direction) 
    {
        Vector3 currentForward = new Vector3(transform.forward.x, 0, transform.forward.z);
        Vector3 newForward = Vector3.RotateTowards(currentForward, direction, _rotateSpeed * Time.deltaTime, 0.0f);
        transform.forward = newForward;
    }

    private void RotateTowardsFreddy(Transform freddyTransform)
    {
        Vector3 compassPos = new Vector3(GameController.Instance.Player.transform.position.x, 0, GameController.Instance.Player.transform.position.z);
        Vector3 freddyPos = new Vector3(freddyTransform.position.x, 0, freddyTransform.position.z);
        Vector3 playerToFreddy = (freddyPos - compassPos).normalized;

        RotateTowards(playerToFreddy);
    }
}
