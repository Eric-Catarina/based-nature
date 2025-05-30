// Path: Assets/_ProjectName/Scripts/Player/PlayerInteraction.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 2f;

    private PlayerControls _playerControls;
    private List<InteractableWorldItem> _nearbyInteractables = new List<InteractableWorldItem>();

    [SerializeField]
    private InteractableWorldItem _closestInteractable = null;

    [SerializeField] private Animator playerAnimator;
    private readonly int _interactTriggerHash = Animator.StringToHash("Interact");


    private void Awake()
    {
        _playerControls = new PlayerControls();
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not assigned to PlayerInteraction.");
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
    }

    private void OnInteractInput(InputAction.CallbackContext context)
    {
        TryInteract();
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
            if(_closestInteractable == interactable)
                 interactable.ShowCue(false);

            _nearbyInteractables.Remove(interactable);
        }
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

        _nearbyInteractables.RemoveAll(item => item == null);

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

        if (oldClosest != _closestInteractable)
        {
            if(oldClosest != null) oldClosest.ShowCue(false);
            if(_closestInteractable != null) _closestInteractable.ShowCue(true);
        }
    }

    public void TryInteract()
    {
        if (_closestInteractable == null || inventoryManager == null)
        {
            return;
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
            Debug.Log($"Picked up {quantityToPick}x {itemToPick.name} from {_closestInteractable.gameObject.name}");

            // --- CORREÇÃO ---
            // 1. Obtenha a referência ao GameObject ANTES de anular _closestInteractable
            GameObject itemGameObjectToDestroy = _closestInteractable.gameObject;

            // 2. Anule a referência _closestInteractable (isso acontece na Unregister ou pode ser explícito)
            // O método UnregisterInteractable já anula _closestInteractable se ele for o item que estamos interagindo
            UnregisterInteractable(_closestInteractable);
            // _closestInteractable = null; // Esta linha é redundante se Unregister já faz isso, mas ser explícito não faz mal.

            // 3. Destrua o GameObject usando a referência que guardamos
            Destroy(itemGameObjectToDestroy);
            // --- FIM CORREÇÃO ---


        }
        else
        {
            Debug.LogWarning($"Could not pick up {itemToPick.name}. Inventory full or AddItem failed.");
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