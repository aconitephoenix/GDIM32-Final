using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageCompass : MonoBehaviour
{
    [SerializeField] private Transform _pageTransform; //temporary for testing - Kaleb
    [SerializeField] private float _rotateSpeed;

    // Update is called once per frame
    void Update()
    {
        if (_pageTransform == null)
        {
            Destroy(gameObject);
        }
        else
        {
            Vector3 compassPos = new Vector3(transform.position.x, 0, transform.position.z);

            Vector3 pagePos = new Vector3(_pageTransform.position.x, 0, _pageTransform.position.z);
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
