// Path: Assets/_ProjectName/Scripts/Player/PlayerInteraction.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq; // Importar para usar Linq (RemoveAll)

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private DialogueSystem dialogueSystem;

    // --- Correção: Declarar ambas as listas aqui ---
    private List<TalkableNPC> _nearbyTalkables = new List<TalkableNPC>();
    private List<InteractableWorldItem> _nearbyInteractables = new List<InteractableWorldItem>();
    // --- Fim Correção ---
    private PlayerControls _playerControls;

    private TalkableNPC _closestTalkable;
    private InteractableWorldItem _closestInteractable;

    [SerializeField] private Animator playerAnimator;
    private readonly int _interactTriggerHash = Animator.StringToHash("Interact");


    private void Awake()
    {
        _playerControls = new PlayerControls();
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not assigned to PlayerInteraction.");
        }
        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem not assigned to PlayerInteraction.");
        }
        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        if (_playerControls != null)
        {
            _playerControls.Gameplay.Enable();
            _playerControls.Gameplay.Interact.performed += OnInteractInput;
        }
    }

    private void OnDisable()
    {
        if (_playerControls != null)
        {
            _playerControls.Gameplay.Interact.performed -= OnInteractInput;
            _playerControls.Gameplay.Disable();
        }
    }

    private void Update()
    {
        FindClosestInteractable();
        FindClosestTalkable();
    }

    private void OnInteractInput(InputAction.CallbackContext context)
    {
        // Se o diálogo está ativo, este input não faz nada no PlayerInteraction.
        // O DialogueSystem gerencia o input para avançar o diálogo.
        if (dialogueSystem != null && dialogueSystem.IsDialogueActive())
        {
             // Debug.Log("Dialogue is active, PlayerInteraction ignoring Interact input.");
             return;
        }

        // Prioriza interação com NPC se houver um próximo e diálogo não estiver ativo
        if (_closestTalkable != null)
        {
            // Inicia/Avança o diálogo via método do NPC (que chama o DialogueSystem)
            _closestTalkable.StartOrAdvanceDialogue();
            // Animação de interação pode ser disparada aqui
            if (playerAnimator != null) playerAnimator.SetTrigger(_interactTriggerHash);
        }
        else if (_closestInteractable != null) // Só interage com item se não houver NPC
        {
            TryInteract(); // Lógica para pegar itens
        }
        else
        {
             // Opcional: Debug.Log("No nearby interactable or talkable object.");
        }
    }

    public void RegisterInteractable(InteractableWorldItem interactable)
    {
        if (interactable != null && !_nearbyInteractables.Contains(interactable))
        {
            _nearbyInteractables.Add(interactable);
        }
    }

    public void UnregisterInteractable(InteractableWorldItem interactable)
    {
        if (_nearbyInteractables.Contains(interactable))
        {
            // Esconder cue apenas se era o mais próximo
            if (_closestInteractable == interactable)
                interactable.ShowCue(false);

            _nearbyInteractables.Remove(interactable);
        }
        // Limpar referência _closestInteractable se for o item que saiu do raio ou foi pego
        if (_closestInteractable == interactable)
        {
            _closestInteractable = null;
        }
    }

    private void FindClosestInteractable()
    {
        InteractableWorldItem oldClosest = _closestInteractable;
        _closestInteractable = null;
        float minDistance = float.MaxValue;

        // --- Correção: Limpeza de itens nulos para a lista correta ---
        _nearbyInteractables.RemoveAll(item => item == null);
        // --- Fim Correção ---

        foreach (var interactable in _nearbyInteractables)
        {
            if (interactable == null) continue;

            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                _closestInteractable = interactable;
            }
        }

        // Mostrar/Esconder cue apenas se o item mais próximo mudou
        if (oldClosest != _closestInteractable)
        {
            if (oldClosest != null) oldClosest.ShowCue(false);
            if (_closestInteractable != null) _closestInteractable.ShowCue(true);
        }
    }

     public void RegisterTalkable(TalkableNPC talkable)
    {
        if (talkable != null && !_nearbyTalkables.Contains(talkable))
        {
            _nearbyTalkables.Add(talkable);
        }
    }

    public void UnregisterTalkable(TalkableNPC talkable)
    {
        if (_nearbyTalkables.Contains(talkable))
        {
             // Esconder cue apenas se era o mais próximo
             if(_closestTalkable == talkable) talkable.ShowInteractionCue(false);

            _nearbyTalkables.Remove(talkable);
        }
         // Limpar referência _closestTalkable se for o NPC que saiu do raio
        if (_closestTalkable == talkable)
        {
            _closestTalkable = null;
        }
    }

    private void FindClosestTalkable()
    {
        TalkableNPC oldClosest = _closestTalkable;
        _closestTalkable = null;
        float minDistance = float.MaxValue;

        // --- Correção: Limpeza de NPCs nulos para a lista correta ---
        _nearbyTalkables.RemoveAll(npc => npc == null);
        // --- Fim Correção ---

        foreach (var talkable in _nearbyTalkables)
        {
            if (talkable == null) continue;
            float distance = Vector3.Distance(transform.position, talkable.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                _closestTalkable = talkable;
            }
        }

        // Mostrar/Esconder cue apenas se o NPC mais próximo mudou
        if (oldClosest != _closestTalkable)
        {
            if(oldClosest != null) oldClosest.ShowInteractionCue(false);
            if(_closestTalkable != null) _closestTalkable.ShowInteractionCue(true);
        }
    }


    public void TryInteract()
    {
        // Este método agora só é chamado se _closestTalkable for null
        if (_closestInteractable == null || inventoryManager == null)
        {
            return; // Nada para interagir (item) ou InventoryManager faltando
        }

        ItemData itemToPick = _closestInteractable.GetItemData();
        if (itemToPick == null)
        {
            Debug.LogWarning($"Attempted to pick up item on object {_closestInteractable.gameObject.name}, but ItemData is not assigned!");
            return;
        }

        int quantityToPick = _closestInteractable.GetQuantity();

        if (inventoryManager.AddItem(itemToPick, quantityToPick))
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(_interactTriggerHash);
            }
            // --- Correção: Usar itemToPick.displayName ---
            Debug.Log($"Picked up {quantityToPick}x {itemToPick.name} from {_closestInteractable.gameObject.name}");
            // --- Fim Correção ---

            GameObject itemGameObjectToDestroy = _closestInteractable.gameObject;

            UnregisterInteractable(_closestInteractable); // Remove da lista _nearbyInteractables e limpa _closestInteractable se for o item correto

            Destroy(itemGameObjectToDestroy);
        }
        else
        {
            // --- Correção: Usar itemToPick.name ---
            Debug.LogWarning($"Could not pick up {itemToPick.name}. Inventory full or AddItem failed.");
             // --- Fim Correção ---
        }
    }

    private void PerformRaycastInteraction()
    {
        if (interactionPoint == null || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRadius))
        {
            InteractableWorldItem interactable = hit.collider.GetComponent<InteractableWorldItem>();
            if (interactable != null)
            {

            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = interactionPoint != null ? interactionPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, interactionRadius);
    }
}