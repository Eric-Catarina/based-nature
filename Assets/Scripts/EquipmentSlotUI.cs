
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; 

public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image itemIcon;
    public EquipmentSlotType slotType;
    private EquipmentManager _equipmentManager;
    private InventoryUI _inventoryUIController; 

    
    private ItemData _currentEquippedItem;

    public void Initialize(EquipmentManager equipmentManager)
    {
        _equipmentManager = equipmentManager;
        
        
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

    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentEquippedItem == null || _inventoryUIController == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        
        
        _inventoryUIController.OnDragStartedFromEquipment(_currentEquippedItem, slotType);

        if (itemIcon) itemIcon.color = new Color(1, 1, 1, 0.5f); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        _inventoryUIController.draggedItemImage.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemIcon) itemIcon.color = Color.white; 

        if (_inventoryUIController != null)
        {
            
            _inventoryUIController.OnDragEndedFromEquipment(slotType, eventData.pointerCurrentRaycast.gameObject);
        }
        
        
    }

    public void OnDrop(PointerEventData eventData)
    {
        
    }
}