// Path: Assets/_ProjectName/Scripts/UI/DialogueSystem.cs
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
            playerInput = FindAnyObjectByType<PlayerInput>(); // Tenta encontrar o PlayerInput
            if (playerInput == null) Debug.LogError("PlayerInput component not found for DialogueSystem Action Map switching.");
        }
    }

    private void OnEnable()
    {
        _playerControls.UI.Enable(); // O mapa UI é onde esperamos o input de diálogo
        _playerControls.UI.Submit.performed += ctx => AdvanceFromInput(); // Usando Submit (Enter) do mapa UI
    }

    private void OnDisable()
    {
        _playerControls.UI.Submit.performed -= ctx => AdvanceFromInput();
        _playerControls.UI.Disable();
    }

    public void StartOrAdvanceDialogue(TalkableNPC npc)
    {
        if (npc == null) return;

        if (!_isDialogueActive || _currentNPC != npc) // Começando um novo diálogo ou com novo NPC
        {
            Debug.Log($"Starting dialogue with {npc.GetNpcName()}");
            _currentNPC = npc;
            _currentDialogueLineIndex = 0;
            _isDialogueActive = true;
            if (dialoguePanel) dialoguePanel.SetActive(true);
            if (npcNameText) npcNameText.text = _currentNPC.GetNpcName();

            if (playerInput != null)
            {
                 playerInput.SwitchCurrentActionMap("UI"); // Mudar para mapa de UI onde Submit avança
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // Pausa o jogo durante o diálogo
        }
        else // Avançando o diálogo com o mesmo NPC
        {
            _currentDialogueLineIndex++;
        }

        DisplayCurrentLine();
    }

    private void AdvanceFromInput()
    {
        if (_isDialogueActive && _currentNPC != null)
        {
            StartOrAdvanceDialogue(_currentNPC); // Chama para avançar a linha
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
            // --- CORREÇÃO: Chamar EndDialogue quando não houver mais linhas ---
            EndDialogue();
            // --- Fim Correção ---
        }
    }

    public void EndDialogue()
    {
        _isDialogueActive = false;
        _currentNPC = null;
        if (dialoguePanel) dialoguePanel.SetActive(false);

        if (playerInput != null)
        {
            playerInput.SwitchCurrentActionMap("Gameplay"); // Retorna ao mapa de gameplay
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f; // Despausa o jogo
    }

    public bool IsDialogueActive() => _isDialogueActive;
}