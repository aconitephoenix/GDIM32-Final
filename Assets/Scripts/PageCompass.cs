using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageCompass : MonoBehaviour
{
    [SerializeField] private Transform[] _pageTransform; 
    [SerializeField] private float _rotateSpeed;

    private Transform _closestPage;
    private void Start()
    {
        _closestPage = _pageTransform[0];
    }

    // Update is called once per frame
    void Update()
    {
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
            Vector3 compassPos = new Vector3(transform.position.x, 0, transform.position.z);

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
}
