// Path: Assets/_ProjectName/Scripts/UI/InventorySlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                                         IBeginDragHandler, IDragHandler, IEndDragHandler,
                                         IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject selectionHighlight;

    private InventorySlotData _slotData;
    private int _slotIndex;
    private InventoryUI _inventoryUI;


    public void Initialize(InventoryUI inventoryUI, int slotIndex)
    {
        _inventoryUI = inventoryUI;
        _slotIndex = slotIndex;
        if (itemIconImage) itemIconImage.preserveAspect = true;
        ClearSlotDisplay();
    }

    public void UpdateSlotDisplay(InventorySlotData slotData)
    {
        _slotData = slotData;
        if (itemIconImage == null || quantityText == null) return;

        if(selectionHighlight) selectionHighlight.SetActive(false); // Hide highlight when updating

        if (_slotData != null && _slotData.itemData != null)
        {
            itemIconImage.sprite = _slotData.itemData.icon;
            itemIconImage.enabled = true;
            quantityText.text = _slotData.quantity >= 1 ? _slotData.quantity.ToString() : "";
            quantityText.enabled = _slotData.quantity >= 1;
        }
        else
        {
            ClearSlotDisplay();
        }
    }

    public void ClearSlotDisplay()
    {
        if (itemIconImage)
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
        }
        if (quantityText)
        {
            quantityText.text = "";
            quantityText.enabled = false;
        }
        _slotData = null;
        if(selectionHighlight) selectionHighlight.SetActive(false); // Hide highlight when clearing
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_inventoryUI != null && _slotData != null && _slotData.itemData != null)
        {
            _inventoryUI.ShowTooltip(_slotData.itemData, GetComponent<RectTransform>());
        }
        if (selectionHighlight) selectionHighlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_inventoryUI != null)
        {
            _inventoryUI.HideTooltip();
        }
        if (selectionHighlight) selectionHighlight.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Debug.Log($"Slot {_slotIndex} clicked with button {eventData.button}");
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (_inventoryUI != null && _slotData != null && _slotData.itemData != null)
            {
                _inventoryUI.RequestUseItem(_slotIndex);
                // Tooltip will be hidden by InventoryUI.RequestUseItem -> HideTooltip
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Debug.Log($"Begin drag on slot {_slotIndex} with button {eventData.button}");
        if (_slotData == null || _slotData.itemData == null || _inventoryUI == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Use the central draggedItemImage from InventoryUI for visual feedback
        if (_inventoryUI.draggedItemImage != null)
        {
             _inventoryUI.draggedItemImage.sprite = itemIconImage.sprite;
             _inventoryUI.draggedItemImage.gameObject.SetActive(true);
             _inventoryUI.draggedItemImage.raycastTarget = false; // Ensure it doesn't block raycasts
        }

        // Hide the original icon and quantity text while dragging
        itemIconImage.enabled = false;
        quantityText.enabled = false;

        _inventoryUI.OnDragStarted(_slotIndex);
        _inventoryUI.HideTooltip(); // Hide tooltip when dragging starts
    }

    public void OnDrag(PointerEventData eventData)
    {
        // The visual drag is handled by InventoryUI.Update()
        // This method in InventorySlotUI is not strictly needed if only the visual is updated externally
        // Debug.Log($"OnDrag on slot {_slotIndex} (original icon hidden)");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Debug.Log($"End drag on slot {_slotIndex}");
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Hide the central draggedItemImage is handled by InventoryUI.OnDragEnded

        // Restore original icon visibility.
        // The UpdateSlotDisplay will be called by InventoryUI.OnDragEnded (via InventoryManager events)
        // to set the final state based on where the item ended up.
        itemIconImage.enabled = true;
        quantityText.enabled = _slotData != null && _slotData.itemData != null && _slotData.quantity >= 1;

        if (_inventoryUI != null)
        {
            _inventoryUI.OnDragEnded(_slotIndex, eventData.pointerCurrentRaycast.gameObject);
        }

        // Re-evaluate selection highlight state after drop
         if (selectionHighlight) selectionHighlight.SetActive(eventData.pointerCurrentRaycast.gameObject == gameObject || eventData.pointerCurrentRaycast.gameObject.transform.IsChildOf(transform));
    }
}