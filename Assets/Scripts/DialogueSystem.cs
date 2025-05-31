
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private PlayerInput playerInput;

    private TalkableNPC _currentNPC;
    private int _currentDialogueLineIndex;
    private bool _isDialogueActive = false;

    private PlayerControls _playerControls;

    void Awake()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        _playerControls = new PlayerControls();

        if (playerInput == null)
        {
            playerInput = FindAnyObjectByType<PlayerInput>(); 
            if (playerInput == null) Debug.LogError("PlayerInput component not found for DialogueSystem Action Map switching.");
        }
    }

    private void OnEnable()
    {
        _playerControls.UI.Enable(); 
        _playerControls.UI.Submit.performed += ctx => AdvanceFromInput(); 
    }

    private void OnDisable()
    {
        _playerControls.UI.Submit.performed -= ctx => AdvanceFromInput();
        _playerControls.UI.Disable();
    }

    public void StartOrAdvanceDialogue(TalkableNPC npc)
    {
        if (npc == null) return;

        if (!_isDialogueActive || _currentNPC != npc) 
        {
            Debug.Log($"Starting dialogue with {npc.GetNpcName()}");
            _currentNPC = npc;
            _currentDialogueLineIndex = 0;
            _isDialogueActive = true;
            if (dialoguePanel) dialoguePanel.SetActive(true);
            if (npcNameText) npcNameText.text = _currentNPC.GetNpcName();

            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("UI"); 
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; 
            UITweenAnimations.PanelAppear(dialoguePanel.GetComponent<RectTransform>(), 0.3f);

        }
        else 
        {
            _currentDialogueLineIndex++;
        }

        DisplayCurrentLine();
    }

    private void AdvanceFromInput()
    {
        if (_isDialogueActive && _currentNPC != null)
        {
            AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.conversationSound); 

            StartOrAdvanceDialogue(_currentNPC); 
        }
    }

    private void DisplayCurrentLine()
    {
        if (_currentNPC == null || dialogueText == null) return;

        string[] lines = _currentNPC.GetDialogueLines();
        if (_currentDialogueLineIndex < lines.Length)
        {
            dialogueText.text = lines[_currentDialogueLineIndex];
        }
        else
        {
            
            EndDialogue();
            
        }
    }

    public void EndDialogue()
    {
        _isDialogueActive = false;
        _currentNPC = null;
        

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Gameplay"); 
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f; 
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.endConversationSound); 
            UITweenAnimations.PanelDisappear(dialoguePanel.GetComponent<RectTransform>(), 0.3f, onComplete: () =>
            {
                dialoguePanel.SetActive(false);
            });
    }

    public bool IsDialogueActive() => _isDialogueActive;
}