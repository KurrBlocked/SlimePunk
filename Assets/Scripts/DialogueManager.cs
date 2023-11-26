using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public PlayerController player;
    public Animator animator;

    public Queue<string> sentences;
    public float waitTimePerWord = 60;
    public int extraBeginningDialogueTime = 5;


    public float textSpeed = 0.07f;
    public float initialTextDelay = 1f;
    public int periodWaitTime = 6;
    public bool isTyping;
    public bool isPlayerFrozen;
    public DialogueTrigger currentTrigger;

    void Start()
    {
        isTyping = false;
        sentences = new Queue<string>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    // Update is called once per frame
    private void Update()
    {  
        if (player.nextDialogueInput && !isTyping && isPlayerFrozen)
        {
            DisplayNextSentence();
        }
    }
    public void StartDialogue(Dialogue dialogue, DialogueTrigger dTrigger)
    {
        sentences.Clear();

        animator.SetBool("IsOpen", true);
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        nameText.text = dialogue.name;
        DisplayNextSentence();
        if (dialogue.freezeMovementTillDone)
        {
            player.freezePlayerMovement = true;
            isPlayerFrozen = true;
        }
        else
        {
            isPlayerFrozen = false;
        }
        currentTrigger = dTrigger;
    }

    public void DisplayNextSentence()
    {
        isTyping = true;
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }
    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        yield return new WaitForSeconds(initialTextDelay);
        int index = 0;
        foreach (char letter in sentence.ToCharArray())
        {
            index++;
            dialogueText.text += letter;
            if (!player.talkFasterInput)
            {
                isTyping = true;
                if ((letter == '.') || (letter == '?') || (letter == '!'))
                {
                    yield return new WaitForSeconds(textSpeed * periodWaitTime);
                    FindObjectOfType<AudioManager>().Play("Talk");
                }
                else if (index % 3 == 0 )
                {
                    FindObjectOfType<AudioManager>().Play("Talk");
                }
                yield return new WaitForSeconds(textSpeed);
            }
        }
        player.talkFasterInput = false;
        isTyping = false;
        if (!isPlayerFrozen)
        {
            yield return new WaitForSeconds(waitTimePerWord * sentence.Length);
            DisplayNextSentence();
        }
    }
    IEnumerator AddDelayBeforeClosing(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        dialogueText.text = "";
        animator.SetBool("IsOpen", false);
        player.freezePlayerMovement = false;
        isTyping = false;
        currentTrigger.dialogueIsFinished = true;
    }
    void EndDialogue()
    {
        StartCoroutine(AddDelayBeforeClosing(initialTextDelay));
    }
}
