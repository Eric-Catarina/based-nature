
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private DialogueSystem dialogueSystem;

    private PlayerControls _playerControls;
    private List<TalkableNPC> _nearbyTalkables = new List<TalkableNPC>();
    private List<InteractableWorldItem> _nearbyInteractables = new List<InteractableWorldItem>();

    private TalkableNPC _closestTalkable;
    private InteractableWorldItem _closestInteractable;

    [SerializeField] private Animator playerAnimator;
    private readonly int _interactTriggerHash = Animator.StringToHash("Interact");
    private float _lastInteractionTime;
    [SerializeField] private float interactionCooldown = 0.2f;


    private void Awake()
    {
        _playerControls = new PlayerControls();
        if (inventoryManager == null)
        {

        }
        if (dialogueSystem == null)
        {

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
        if (Time.time - _lastInteractionTime < interactionCooldown)
        {
            return;
        }

        if (dialogueSystem != null && dialogueSystem.IsDialogueActive())
        {
             return;
        }

        if (_closestTalkable != null)
        {
            _closestTalkable.StartOrAdvanceDialogue();
            if (playerAnimator != null) playerAnimator.SetTrigger(_interactTriggerHash);
            _lastInteractionTime = Time.time;
        }
        else if (_closestInteractable != null)
        {
            TryInteract();
            _lastInteractionTime = Time.time;
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
        if (interactable != null && _nearbyInteractables.Contains(interactable))
        {
            if (_closestInteractable == interactable)
            {
                interactable.ShowCue(false);
                interactable.RemoveOutline(); 
                _closestInteractable = null; 
            }
            _nearbyInteractables.Remove(interactable);
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
            if (oldClosest != null)
            {
                oldClosest.ShowCue(false);
                oldClosest.RemoveOutline(); 
            }
            if (_closestInteractable != null)
            {
                _closestInteractable.ShowCue(true);
                _closestInteractable.ApplyOutline(); 
            }
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
        if (talkable != null && _nearbyTalkables.Contains(talkable))
        {
             if(_closestTalkable == talkable)
             {
                talkable.ShowInteractionCue(false);
                _closestTalkable = null; 
             }
            _nearbyTalkables.Remove(talkable);
        }
    }

    private void FindClosestTalkable()
    {
        TalkableNPC oldClosest = _closestTalkable;
        _closestTalkable = null;
        float minDistance = float.MaxValue;

        _nearbyTalkables.RemoveAll(npc => npc == null);

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

        if (oldClosest != _closestTalkable)
        {
            if(oldClosest != null) oldClosest.ShowInteractionCue(false);
            if(_closestTalkable != null) _closestTalkable.ShowInteractionCue(true);
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

            return;
        }

        int quantityToPick = _closestInteractable.GetQuantity();

        if (inventoryManager.AddItem(itemToPick, quantityToPick))
        {
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(_interactTriggerHash);
            }


            AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.equipSound); 
            GameObject itemGameObjectToDestroy = _closestInteractable.gameObject;
            
            
            
            
            UnregisterInteractable(_closestInteractable); 

            Destroy(itemGameObjectToDestroy);
            
        }
        else
        {

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