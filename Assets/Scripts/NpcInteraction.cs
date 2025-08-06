using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    public GameObject dialogueUI;   
    public Text dialogueText;      
    [TextArea(2, 5)]
    public string[] dialogueLines;

    public GameObject interactionPrompt;

    private int currentLineIndex = 0;
    private bool playerInRange = false;
    private bool dialogueActive = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogueActive)
            {
                StartDialogue();
            }
            else
            {
                NextLine();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
            EndDialogue();
        }
    }

    void StartDialogue()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
        dialogueUI.SetActive(true);
        dialogueText.text = dialogueLines[currentLineIndex];
        dialogueActive = true;
    }

    void NextLine()
    {
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLineIndex];
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueUI.SetActive(false);
        dialogueActive = false;
        currentLineIndex = 0;
    }
}
