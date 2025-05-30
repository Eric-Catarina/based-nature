// Path: Assets/_ProjectName/Scripts/EquipmentSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Importar para interfaces de evento

public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemIcon;
    public EquipmentSlotType slotType;
    private EquipmentManager _equipmentManager;
    private InventoryUI _inventoryUIController; // NEW: Reference to InventoryUI

    // Item atualmente equipado neste slot
    private ItemData _currentEquippedItem;

    public void Initialize(EquipmentManager equipmentManager)
    {
        _equipmentManager = equipmentManager;
        // Find the InventoryUI controller. This assumes InventoryUI is on the same Canvas or easily accessible.
        // For larger projects, consider a dedicated UI Manager or event bus.
        _inventoryUIController = GetComponentInParent<Canvas>().GetComponentInChildren<InventoryUI>();
        if (_inventoryUIController == null)
        {
            Debug.LogError("InventoryUI controller not found in parent Canvas for EquipmentSlotUI!");
        }

        ClearSlotDisplay();
    }

    public void DisplayItem(ItemData item)
    {
        _currentEquippedItem = item;
        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }
    }

    public void ClearSlotDisplay()
    {
        _currentEquippedItem = null;
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_inventoryUIController != null && _currentEquippedItem != null)
        {
            _inventoryUIController.ShowTooltip(_currentEquippedItem, GetComponent<RectTransform>());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_inventoryUIController != null)
        {
            _inventoryUIController.HideTooltip();
        }
    }

    // --- Drag and Drop from Equipment Slot ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentEquippedItem == null || _inventoryUIController == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        // Inform InventoryUI to start visual drag with this item
        // We'll use a special index (-1) and pass the slotType to differentiate
        _inventoryUIController.OnDragStartedFromEquipment(_currentEquippedItem, slotType);

        if (itemIcon) itemIcon.color = new Color(1, 1, 1, 0.5f); // Fade out original icon
    }

    public void OnDrag(PointerEventData eventData)
    {
        _inventoryUIController.draggedItemImage.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemIcon) itemIcon.color = Color.white; // Restore full opacity

        if (_inventoryUIController != null)
        {
            // Inform InventoryUI about the end of the drag and the target
            _inventoryUIController.OnDragEndedFromEquipment(slotType, eventData.pointerCurrentRaycast.gameObject);
        }
        // Update display will be handled by EquipmentManager events or by InventoryUI dispatch
        // ClearSlotDisplay(); // Don't clear immediately, let the manager decide if unequipped
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Drop logic for this slot type is handled by InventoryUI.OnDragEnded / OnDragEndedFromEquipment
    }
}