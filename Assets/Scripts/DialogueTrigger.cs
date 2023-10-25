using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    private BoxCollider2D collider;

    public void Start()
    {
        collider = GetComponent<BoxCollider2D>();
    }
    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TriggerDialogue();
        collider.enabled = false;
    }
}
