using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{

    private PlayerController player;
    private CircleCollider2D collider;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(true);
        player = FindAnyObjectByType<PlayerController>();
        collider = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame

    private void Update()
    {
        /*if (player.isBouncing)
        {
            collider.isTrigger = true;
        }
        else
        {
            collider.isTrigger = false;
        }*/
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            gameObject.SetActive(false);
        }
    }
}
