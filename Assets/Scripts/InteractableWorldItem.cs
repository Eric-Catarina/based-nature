// Path: Assets/_ProjectName/Scripts/Items/InteractableWorldItem.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableWorldItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    [SerializeField] private GameObject interactionCue; // Visual cue like "E to pick up"

    private bool _playerInRange = false;

    public ItemData GetItemData() => itemData;
    public int GetQuantity() => quantity;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true; // Ensure it's a trigger for detection
        if(interactionCue) interactionCue.SetActive(false);
    }

    public void ShowCue(bool show)
    {
        if(interactionCue) interactionCue.SetActive(show);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure your player GameObject has the "Player" tag
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.RegisterInteractable(this);
                ShowCue(true);
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
                playerInteraction.UnregisterInteractable(this);
                ShowCue(false);
            }
            _playerInRange = false;
        }
    }

    // Optional: Add highlight effect on mouse hover or when in range
    // public void Highlight(bool active) { ... }
}