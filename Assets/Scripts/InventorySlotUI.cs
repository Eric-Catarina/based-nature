// Path: Assets/_ProjectName/Scripts/UI/InventorySlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject quantityBackground;

    private InventoryUI _inventoryUIController;
    private int _slotIndex;
    private InventorySlotData _currentSlotData;

    public void Initialize(InventoryUI inventoryUI, int index)
    {
        _inventoryUIController = inventoryUI;
        _slotIndex = index;
        if (itemIconImage) itemIconImage.enabled = false;
        if (quantityText) quantityText.enabled = false;
        if (quantityBackground) quantityBackground.SetActive(false);
    }

    public void UpdateSlotDisplay(InventorySlotData slotData)
    {
        _currentSlotData = slotData;
        if (slotData != null && !slotData.IsEmpty())
        {
            if (itemIconImage)
            {
                itemIconImage.sprite = slotData.itemData.icon;
                itemIconImage.enabled = true;
            }
            if (quantityText)
            {
                bool showQuantity = slotData.quantity > 1 && slotData.itemData.isStackable;
                quantityText.text = showQuantity ? slotData.quantity.ToString() : "";
                quantityText.enabled = showQuantity;
                if (quantityBackground) quantityBackground.SetActive(showQuantity);
            }
        }
        else
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
                if (quantityBackground) quantityBackground.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_inventoryUIController != null && _currentSlotData != null && !_currentSlotData.IsEmpty())
        {
            _inventoryUIController.ShowTooltip(_currentSlotData.itemData, GetComponent<RectTransform>());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_inventoryUIController != null)
        {
            _inventoryUIController.HideTooltip();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (_inventoryUIController != null && _currentSlotData != null && !_currentSlotData.IsEmpty())
            {
                 _inventoryUIController.RequestUseItem(_slotIndex);
            }
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentSlotData == null || _currentSlotData.IsEmpty() || _inventoryUIController == null)
        {
            eventData.pointerDrag = null; 
            return;
        }
        
        _inventoryUIController.OnDragStarted(_slotIndex);
        if (itemIconImage) itemIconImage.color = new Color(1,1,1, 0.5f); 
        if (quantityText) quantityText.enabled = false;
        if (quantityBackground) quantityBackground.SetActive(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_inventoryUIController != null && _inventoryUIController.draggedItemImage.gameObject.activeSelf)
        {
             _inventoryUIController.draggedItemImage.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemIconImage) itemIconImage.color = Color.white;

        if (_inventoryUIController != null)
        {
             _inventoryUIController.OnDragEnded(_slotIndex, eventData.pointerCurrentRaycast.gameObject);
        }
        UpdateSlotDisplay(_currentSlotData); // Ensure the original slot visuals are restored or updated
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        // The drop logic is handled by OnEndDrag of the item being dragged,
        // using eventData.pointerCurrentRaycast.gameObject to determine the drop target.
    }
}