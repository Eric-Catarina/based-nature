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
    [SerializeField] private GameObject selectionHighlight; // Optional: for visual feedback

    private InventorySlotData _slotData;
    private int _slotIndex;
    private InventoryUI _inventoryUI; // Reference to the parent UI manager

    private static InventorySlotUI _draggedSlotIconCopy = null; // The visual copy being dragged

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

        if (_slotData != null && _slotData.itemData != null)
        {
            itemIconImage.sprite = _slotData.itemData.icon;
            itemIconImage.enabled = true;
            quantityText.text = _slotData.quantity > 1 ? _slotData.quantity.ToString() : "";
            quantityText.enabled = _slotData.quantity > 1;
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
        _slotData = null; // Ensure data is cleared too
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
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (_inventoryUI != null && _slotData != null && _slotData.itemData != null)
            {
                _inventoryUI.RequestUseItem(_slotIndex);
            }
        }
        // Left click could be for selection, or if not dragging, maybe quick equip/move to action bar
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_slotData == null || _slotData.itemData == null || _inventoryUI == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Create a draggable copy of the icon
        if (_draggedSlotIconCopy == null)
        {
            GameObject iconCopyObj = new GameObject("DraggedIcon");
            iconCopyObj.transform.SetParent(_inventoryUI.transform); // Attach to main inventory canvas
            iconCopyObj.transform.SetAsLastSibling(); // Render on top
            iconCopyObj.AddComponent<RectTransform>();
            Image img = iconCopyObj.AddComponent<Image>();
            img.sprite = itemIconImage.sprite;
            img.raycastTarget = false; // So it doesn't interfere with drop detection
            iconCopyObj.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta * 0.8f; // Slightly smaller
            _draggedSlotIconCopy = iconCopyObj.AddComponent<InventorySlotUI>(); // Add dummy component to hold reference
             _draggedSlotIconCopy.itemIconImage = img; // For potential visual changes during drag
        }
        
        if (_draggedSlotIconCopy != null && _draggedSlotIconCopy.itemIconImage != null)
        {
            _draggedSlotIconCopy.itemIconImage.enabled = true;
            _draggedSlotIconCopy.itemIconImage.sprite = itemIconImage.sprite;
        }


        itemIconImage.enabled = false; // Hide original icon
        quantityText.enabled = false;

        _inventoryUI.OnDragStarted(_slotIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggedSlotIconCopy == null || eventData.button != PointerEventData.InputButton.Left) return;
        _draggedSlotIconCopy.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        
        if (_draggedSlotIconCopy != null)
        {
            Destroy(_draggedSlotIconCopy.gameObject);
            _draggedSlotIconCopy = null;
        }

        // Restore original icon visibility before logic determines new state
        if (_slotData != null && _slotData.itemData != null) {
            itemIconImage.enabled = true;
            quantityText.enabled = _slotData.quantity > 1;
        } else {
            ClearSlotDisplay();
        }


        if (_inventoryUI != null)
        {
            _inventoryUI.OnDragEnded(_slotIndex, eventData.pointerCurrentRaycast.gameObject);
        }
    }
}