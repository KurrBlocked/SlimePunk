using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject[] keys;
    public bool opened;
    public bool isClosingDoor = false;
    public DialogueTrigger trigger;
    BoxCollider2D col;
    SpriteRenderer sprite;
    // Start is called before the first frame update
    void Start()
    {
        opened = isClosingDoor;
        col = gameObject.GetComponent<BoxCollider2D>();
        sprite = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isClosingDoor)
        {
            if (!opened)
            {
                opened = true;
                foreach (GameObject key in keys)
                {
                    if (key.activeInHierarchy)
                    {
                        opened = false;
                    }
                }
            }
            if (opened)
            {
                FindObjectOfType<AudioManager>().Play("DoorOpen");
                gameObject.SetActive(false);
            }
        }
        else
        {
            if (trigger.dialogueIsTriggered)
            {
                col.isTrigger = false;
                sprite.color = Color.gray;
            }
            else
            {
                col.isTrigger = true;
                sprite.color = Color.clear;
            }
        }
        
    }
}
