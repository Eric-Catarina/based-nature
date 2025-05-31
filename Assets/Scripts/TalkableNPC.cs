
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TalkableNPC : MonoBehaviour
{
    [Header("NPC Info")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField] [TextArea(3,10)] private string[] dialogueLines;

    [Header("Interaction Visuals")]
    [SerializeField] private GameObject interactionCue; 

    private DialogueSystem _dialogueSystem;
    private bool _playerInRange = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        else Debug.LogError("TalkableNPC requires a Collider component.", this);

        if (interactionCue) interactionCue.SetActive(false);
        _dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    public void Initialize(DialogueSystem system)
    {
        _dialogueSystem = system;
    }

    public void ShowInteractionCue(bool show)
    {
        if (interactionCue) interactionCue.SetActive(show);
    }

    public string GetNpcName() => npcName;
    public string[] GetDialogueLines() => dialogueLines;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.RegisterTalkable(this); 
            }
            _playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
             PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.UnregisterTalkable(this); 
            }
            _playerInRange = false;
        }
    }

    
    public void StartOrAdvanceDialogue()
    {
        if (_dialogueSystem != null)
        {
            _dialogueSystem.StartOrAdvanceDialogue(this);
        }
        else
        {
            Debug.LogWarning("DialogueSystem not initialized for this NPC.");
        }
    }
}