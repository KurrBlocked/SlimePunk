using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    public Animator animator;

    public Queue<string> sentences;
    private int timer;
    public int waitTimePerWord = 60;
    public int extraBeginningDialogueTime = 5;
    private bool lastSentence;

    public float textSpeed = 0.07f;
    public float initialTextDelay = 1f;
    void Start()
    {
        timer = -1;
        sentences = new Queue<string>();
        lastSentence = false;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (timer >= 0)
        {
            timer --;
        }
        if (!(sentences.Count == 0) && (timer == 0 && timer % waitTimePerWord == 0))
        {
            DisplayNextSentence();
            if (sentences.Count == 0)
            {
                lastSentence = true;
            }
        }
        if (lastSentence && (timer == 0 && timer % waitTimePerWord == 0))
        {
            EndDialogue();
            lastSentence = false;
        }
    }
    public void StartDialogue(Dialogue dialogue)
    {
        sentences.Clear();

        animator.SetBool("IsOpen", true);
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        nameText.text = dialogue.name;
        DisplayNextSentence();
        timer += extraBeginningDialogueTime;
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        timer = sentence.Length * waitTimePerWord;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        yield return new WaitForSeconds(initialTextDelay);
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed); ;
        }
    }

    void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
    }
}
