// Path: Assets/_ProjectName/Scripts/Player/PlayerInteraction.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager; // Assign in Inspector
    [SerializeField] private Transform interactionPoint; // Optional: for raycasting from a specific point
    [SerializeField] private float interactionRadius = 2f; // If not using raycasting

    private PlayerControls _playerControls;
    private List<InteractableWorldItem> _nearbyInteractables = new List<InteractableWorldItem>();

    [SerializeField]
    private InteractableWorldItem _closestInteractable = null;

    // Animator reference for interaction animation
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
            // Try to get it from self or children if not assigned
            playerAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable()
    {
        _playerControls.Gameplay.Enable();
        _playerControls.Gameplay.Interact.performed += OnInteractInput;
    }

    private void OnDisable()
    {
        _playerControls.Gameplay.Interact.performed -= OnInteractInput;
        _playerControls.Gameplay.Disable();
    }

    private void Update()
    {
        FindClosestInteractable();
        // Optionally, highlight _closestInteractable here if it's not null
    }

    private void OnInteractInput(InputAction.CallbackContext context)
    {
        TryInteract();
    }

    public void RegisterInteractable(InteractableWorldItem interactable)
    {
        if (!_nearbyInteractables.Contains(interactable))
        {
            _nearbyInteractables.Add(interactable);
        }
    }

    public void UnregisterInteractable(InteractableWorldItem interactable)
    {
        if (_nearbyInteractables.Contains(interactable))
        {
            interactable.ShowCue(false); // Ensure cue is hidden if it was the closest
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

        // Cleanup null entries that might occur if items are destroyed externally
        _nearbyInteractables.RemoveAll(item => item == null);

        foreach (var interactable in _nearbyInteractables)
        {
            if (interactable == null) continue; // Should be caught by RemoveAll, but good for safety

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
            // Try a raycast as a fallback or primary method if desired
            // PerformRaycastInteraction();
            return;
        }

        ItemData itemToPick = _closestInteractable.GetItemData();
        int quantityToPick = _closestInteractable.GetQuantity();

        if (itemToPick != null)
        {
            if (inventoryManager.AddItem(itemToPick, quantityToPick))
            {
                if (playerAnimator != null)
                {
                    playerAnimator.SetTrigger(_interactTriggerHash);
                }
                // Unregister before destroying to avoid issues
                UnregisterInteractable(_closestInteractable);
                // _closestInteractable.DestroyItem(); // This will destroy the GameObject
                _closestInteractable = null; // Clear it immediately
                Debug.Log($"Picked up {quantityToPick}x {itemToPick.displayName}");
            }
            else
            {
                Debug.LogWarning($"Could not pick up {itemToPick.displayName}. Inventory full or error.");
            }
        }
    }

    // Example Raycast Interaction (alternative or complementary to trigger-based)
    private void PerformRaycastInteraction()
    {
        // Ensure interactionPoint and Camera.main are set
        if (interactionPoint == null || Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2)); // Center of screen
        // Or, if you prefer using the interactionPoint:
        // Ray ray = new Ray(interactionPoint.position, interactionPoint.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRadius))
        {
            InteractableWorldItem interactable = hit.collider.GetComponent<InteractableWorldItem>();
            if (interactable != null)
            {
                // Logic to highlight or show cue for raycast-detected item
                // On interact press:
                // ItemData itemToPick = interactable.GetItemData();
                // ... rest of pickup logic ...
            }
        }
    }

    // Optional: Draw gizmo for interaction radius if not using triggers primarily
    private void OnDrawGizmosSelected()
    {
        if (interactionPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}