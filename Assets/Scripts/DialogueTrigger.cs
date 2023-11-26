using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public bool dialogueIsTriggered;
    public bool dialogueIsFinished;
    public Dialogue dialogue;
    private BoxCollider2D col;
    public bool isConditional = false;
    public GameObject conditional;

    public void Start()
    {
        col = GetComponent<BoxCollider2D>();
        dialogueIsTriggered = false;
        dialogueIsFinished = false;
    }
    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue, this);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (isConditional)
            {
                if (conditional.name == "BossDrone")
                {
                    if (conditional.GetComponent<BossDroneController>().health <= 0)
                    {
                        TriggerDialogue();
                        col.enabled = false;
                        dialogueIsTriggered = true;
                    }
                }
                if (conditional.tag == "Dialogue")
                {
                    if (conditional.GetComponent<DialogueTrigger>().dialogueIsFinished)
                    {
                        TriggerDialogue();
                        col.enabled = false;
                        dialogueIsTriggered = true;
                    }
                }
            }
            else
            {
                TriggerDialogue();
                col.enabled = false;
                dialogueIsTriggered = true;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (isConditional)
            {
                if (conditional.name == "BossDrone")
                {
                    if (conditional.GetComponent<BossDroneController>().health <= 0)
                    {
                        TriggerDialogue();
                        col.enabled = false;
                        dialogueIsTriggered = true;
                    }
                }
                if (conditional.tag == "Dialogue")
                {
                    if (conditional.GetComponent<DialogueTrigger>().dialogueIsFinished)
                    {
                        TriggerDialogue();
                        col.enabled = false;
                        dialogueIsTriggered = true;
                    }
                }
            }
            else
            {
                TriggerDialogue();
                col.enabled = false;
                dialogueIsTriggered = true;
            }
        }
    }
}
