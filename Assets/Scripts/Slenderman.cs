using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slenderman : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //every frame rotate towards player
        if (GameController.Instance.Player != null)
        {
            Vector3 direction = GameController.Instance.Player.transform.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        }
    }
}
