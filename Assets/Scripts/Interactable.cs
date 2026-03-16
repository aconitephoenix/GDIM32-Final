using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] protected float _interactionDistance = 2.0f;
    [SerializeField] protected UIController _uiController;
    [SerializeField] protected AudioClip _pageCollectSound;
    [SerializeField] protected AudioSource _audioSource;

    private bool _collected;

    // Start is called before the first frame update
    void Start()
    {
        _collected = false;
        
        // Get AudioSource if not assigned
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Instance.Player == null) return;
    }

    public virtual void OnMouseOver()
    {
        // Checking if player is within interaction distance
        if (Vector3.Distance(transform.position, GameController.Instance.Player.transform.position) <= _interactionDistance && gameObject.GetComponent<Interactable>().enabled == true)
        {
            // Player interaction once they press E or click the mouse
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                Collect();
            }

            // Enable hover text if player is close enough and item has not been collected
            if (!_collected)
            {
                _uiController.HandleHoverText(gameObject.tag);
            } else
            {
                _uiController.HandleHoverText("Untagged");
            }
            
        }
        else
        {
            _uiController.HandleHoverText("Untagged");
        }
    }

    // Disabling hover text once player has looked away from the object
    public void OnMouseExit()
    {
        _uiController.HandleHoverText("Untagged");
    }

    // Collect the item
    private void Collect()
    {
        _collected = true;
        
        // Play page collect sound if audio source and clip are available
        _audioSource.PlayOneShot(_pageCollectSound);
        GameController.Instance.Player.CollectPage();
        Destroy(gameObject);
    }
}
