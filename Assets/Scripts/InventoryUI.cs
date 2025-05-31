// Path: Assets/_ProjectName/Scripts/UI/InventoryUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Core References")]

    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject inventoryPanel; // The root panel of the inventory UI
    [SerializeField] private Transform slotContainer; // Parent object with GridLayoutGroup for slots
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private GameObject equipmentPanel; // NEW: Reference to the Equipment Panel GameObject

    [Header("Tooltip References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipItemNameText;
    [SerializeField] private TextMeshProUGUI tooltipItemDescriptionText;
    [SerializeField] private TextMeshProUGUI tooltipItemTypeText;

    [Header("Drag & Drop Visuals")]
    [SerializeField] public Image draggedItemImage;
    private CanvasGroup inventoryCanvasGroup; // Optional: CanvasGroup for fade effects

    private List<InventorySlotUI> _slotUIInstances = new List<InventorySlotUI>();
    private PlayerControls _playerControls;
    private bool _isPanelOpen = false;
    private RectTransform _inventoryPanelRectTransform;

    private int _draggedFromSlotIndex = -1; // Stores index if dragging FROM inventory slot

    private void Awake()
    {
        if (Application.isPlaying)
        {
            _playerControls = new PlayerControls();
        }

        if (inventoryManager == null) Debug.LogError("InventoryManager not assigned to InventoryUI.");
        if (inventoryPanel == null) Debug.LogError("InventoryPanel not assigned to InventoryUI.");
        if (slotContainer == null) Debug.LogError("SlotContainer not assigned to InventoryUI.");
        if (slotPrefab == null) Debug.LogError("SlotPrefab not assigned to InventoryUI.");
        if (equipmentPanel == null) Debug.LogError("EquipmentPanel not assigned to InventoryUI."); // Check new reference

        if (draggedItemImage) draggedItemImage.gameObject.SetActive(false);
        if (tooltipPanel) tooltipPanel.SetActive(false);
        if (inventoryCanvasGroup == null)
        {
            inventoryCanvasGroup = inventoryPanel.GetComponent<CanvasGroup>();
            if (inventoryCanvasGroup == null)
            {
                inventoryCanvasGroup = inventoryPanel.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            inventoryCanvasGroup.alpha = 1f; // Ensure it's fully visible at start
        }
        _inventoryPanelRectTransform = inventoryPanel.GetComponent<RectTransform>();
        ClosePanel();
    }

    private void OnEnable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventorySlotChanged += UpdateSpecificSlotUI;
            inventoryManager.OnInventoryChanged += RefreshAllSlotsUI;
        }
        if (_playerControls != null)
        {
            _playerControls.Gameplay.Enable();
            _playerControls.Gameplay.OpenInventory.performed += TogglePanelInput;
        }
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventorySlotChanged -= UpdateSpecificSlotUI;
            inventoryManager.OnInventoryChanged -= RefreshAllSlotsUI;
        }
        if (_playerControls != null)
        {
            _playerControls.Gameplay.OpenInventory.performed -= TogglePanelInput;
            _playerControls.Gameplay.Disable();
        }
    }

    private void Start()
    {
        CreateSlotUIInstances();
        RefreshAllSlotsUI();
    }

    private void Update()
    {
        // No need to update draggedItemImage position here, InventorySlotUI.OnDrag does it
    }

    private void TogglePanelInput(InputAction.CallbackContext context)
    {
        if (_isPanelOpen) ClosePanel();
        else OpenPanel();
    }

    public void OpenPanel()
    {
        inventoryPanel.SetActive(true);
        if (equipmentPanel) equipmentPanel.SetActive(true); // NEW: Activate equipment panel
        _isPanelOpen = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UITweenAnimations.PanelAppear(_inventoryPanelRectTransform, 0.3f);
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.pickItemSound); // Play hover sound when opening panel
    }

    public void ClosePanel()
    {
        inventoryPanel.SetActive(false);
        if (equipmentPanel) equipmentPanel.SetActive(false); // NEW: Deactivate equipment panel
        _isPanelOpen = false;
        HideTooltip();
        if (draggedItemImage && draggedItemImage.gameObject.activeSelf)
        {
            draggedItemImage.gameObject.SetActive(false);
            if (_draggedFromSlotIndex != -1) UpdateSpecificSlotUI(_draggedFromSlotIndex);
            _draggedFromSlotIndex = -1;
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.dropItemSound); // Play hover sound when opening panel

    }

    private void CreateSlotUIInstances()
    {
        if (inventoryManager == null || slotPrefab == null || slotContainer == null) return;

        for (int i = 0; i < inventoryManager.InventorySize; i++)
        {
            InventorySlotUI newSlotUI = Instantiate(slotPrefab, slotContainer);
            newSlotUI.gameObject.name = $"InventorySlot_{i}";
            newSlotUI.Initialize(this, i);
            _slotUIInstances.Add(newSlotUI);
        }
    }

    private void UpdateSpecificSlotUI(int slotIndex)
    {
        if (inventoryManager == null || slotIndex < 0 || slotIndex >= _slotUIInstances.Count) return;

        InventorySlotData slotData = inventoryManager.GetSlotAtIndex(slotIndex);
        _slotUIInstances[slotIndex].UpdateSlotDisplay(slotData);
    }

    private void RefreshAllSlotsUI()
    {
        if (inventoryManager == null) return;

        if (_slotUIInstances.Count != inventoryManager.InventorySize)
        {
            foreach (var slot in _slotUIInstances) { if (slot != null) Destroy(slot.gameObject); }
            _slotUIInstances.Clear();
            CreateSlotUIInstances();
        }

        for (int i = 0; i < inventoryManager.InventorySize; i++)
        {
            if (i < _slotUIInstances.Count)
            {
                InventorySlotData slotData = inventoryManager.GetSlotAtIndex(i);
                _slotUIInstances[i].UpdateSlotDisplay(slotData);
            }
        }
    }

    public void ShowTooltip(ItemData item, RectTransform slotRectTransform)
    {
        if (tooltipPanel == null || item == null) return;

        tooltipItemNameText.text = item.itemName;
        tooltipItemDescriptionText.text = item.description;
        if (tooltipItemTypeText)
        {
            tooltipItemTypeText.text = item.itemType.ToString();
            if (item.itemType == ItemType.Equipment)
            {
                tooltipItemTypeText.text += $" ({item.equipmentSlotType})";
            }
        }

        tooltipPanel.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel.GetComponent<RectTransform>());

        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        Vector3 slotGlobalPosition;

        Vector3[] slotCorners = new Vector3[4];
        slotRectTransform.GetWorldCorners(slotCorners);
        slotGlobalPosition = slotCorners[2];

        Canvas canvas = GetComponentInParent<Canvas>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, slotGlobalPosition, canvas.worldCamera, out localPoint);

        tooltipRect.anchoredPosition = localPoint + new Vector2(tooltipRect.sizeDelta.x * tooltipRect.pivot.x + 5, -tooltipRect.sizeDelta.y * (1 - tooltipRect.pivot.y) - 5);

        Vector2 canvasSize = (canvas.transform as RectTransform).sizeDelta;
        Vector2 anchoredPos = tooltipRect.anchoredPosition;
        Vector2 tooltipSize = tooltipRect.sizeDelta;

        if (anchoredPos.x + tooltipSize.x * (1 - tooltipRect.pivot.x) > canvasSize.x / 2)
        {
            slotGlobalPosition = slotCorners[3];
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, slotGlobalPosition, canvas.worldCamera, out localPoint);
            anchoredPos.x = localPoint.x - tooltipSize.x * tooltipRect.pivot.x - 5;
        }
        if (anchoredPos.y - tooltipSize.y * tooltipRect.pivot.y < -canvasSize.y / 2)
        {
            slotGlobalPosition = slotCorners[0];
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, slotGlobalPosition, canvas.worldCamera, out localPoint);
            anchoredPos.y = localPoint.y + tooltipSize.y * (1 - tooltipRect.pivot.y) + 5 + slotRectTransform.sizeDelta.y;
        }
        tooltipRect.anchoredPosition = anchoredPos;
    }

    public void HideTooltip()
    {
        if (tooltipPanel) tooltipPanel.SetActive(false);
    }

    public void RequestUseItem(int slotIndex)
    {
        if (inventoryManager != null)
        {
            inventoryManager.UseItem(slotIndex);
        }
    }

    public void OnDragStarted(int fromSlotIndex)
    {
        _draggedFromSlotIndex = fromSlotIndex;
        if (draggedItemImage != null)
        {
            InventorySlotData slotData = inventoryManager.GetSlotAtIndex(fromSlotIndex);
            if (slotData != null && !slotData.IsEmpty())
            {
                draggedItemImage.sprite = slotData.itemData.icon;
                draggedItemImage.gameObject.SetActive(true);
            }
            else
            {
                draggedItemImage.gameObject.SetActive(false);
                _draggedFromSlotIndex = -1;
            }
        }
    }

    public void OnDragEnded(int originalFromSlotIndex, GameObject dropTargetObject)
    {
        int fromIndex = _draggedFromSlotIndex != -1 ? _draggedFromSlotIndex : originalFromSlotIndex;
        _draggedFromSlotIndex = -1;

        if (draggedItemImage != null)
        {
            draggedItemImage.gameObject.SetActive(false);
        }

        if (fromIndex == -1) return;

        if (dropTargetObject != null)
        {
            // First, try to drop on an InventorySlotUI
            InventorySlotUI targetInventorySlotUI = dropTargetObject.GetComponentInParent<InventorySlotUI>();
            if (targetInventorySlotUI != null)
            {
                int toIndex = _slotUIInstances.IndexOf(targetInventorySlotUI);
                if (toIndex != -1 && fromIndex != toIndex)
                {
                    inventoryManager.MoveItem(fromIndex, toIndex);
                }
                else
                {
                    UpdateSpecificSlotUI(fromIndex);
                }
                return; // Handled drop on an InventorySlotUI
            }

            // If not an InventorySlotUI, try to drop on an EquipmentSlotUI
            EquipmentSlotUI targetEquipmentSlotUI = dropTargetObject.GetComponentInParent<EquipmentSlotUI>();
            if (targetEquipmentSlotUI != null)
            {
                ItemData itemToEquip = inventoryManager.GetSlotAtIndex(fromIndex).itemData;
                if (itemToEquip != null && itemToEquip.isEquippable && itemToEquip.equipmentSlotType == targetEquipmentSlotUI.slotType)
                {
                    EquipmentManager.Instance.EquipItem(itemToEquip, fromIndex);
                    // EquipItem will handle removal from inventory and updating both UIs
                }
                else
                {
                    Debug.LogWarning($"Cannot equip {itemToEquip?.itemName ?? "null"} to {targetEquipmentSlotUI.slotType} slot. Item not equipable or wrong slot type.");
                    UpdateSpecificSlotUI(fromIndex); // Visually return item to original inventory slot
                }
                return; // Handled drop on an EquipmentSlotUI
            }
        }

        // If dropped outside any valid slot (Inventory or Equipment)
        Debug.Log("Item dropped outside a valid slot.");
        // Delete item
        ItemData itemToReturn = inventoryManager.GetSlotAtIndex(fromIndex).itemData;
        if (itemToReturn != null)
        {
            inventoryManager.RemoveItemFromSlot(fromIndex, 1); // Remove item from inventory
            Debug.Log($"Item {itemToReturn.itemName} removed from inventory.");
        }
        else
        {
            Debug.LogWarning("No item to return from inventory slot.");
        }
        UpdateSpecificSlotUI(fromIndex);
    }
    
      private EquipmentSlotType _draggedFromEquipmentSlotType = EquipmentSlotType.None;

    // ... (rest of Awake, OnEnable, OnDisable, Start, Update, TogglePanelInput, OpenPanel, ClosePanel) ...

    public void OnDragStartedFromEquipment(ItemData itemToDrag, EquipmentSlotType fromSlotType)
    {
        _draggedFromSlotIndex = -1; // Ensure inventory index is not used
        _draggedFromEquipmentSlotType = fromSlotType;

        if (draggedItemImage != null)
        {
            if (itemToDrag != null)
            {
                draggedItemImage.sprite = itemToDrag.icon;
                draggedItemImage.gameObject.SetActive(true);
            }
            else
            {
                 draggedItemImage.gameObject.SetActive(false);
                 _draggedFromEquipmentSlotType = EquipmentSlotType.None; 
            }
        }
    }

    public void OnDragEndedFromEquipment(EquipmentSlotType originalFromSlotType, GameObject dropTargetObject)
    {
        // Reset drag source
        _draggedFromEquipmentSlotType = EquipmentSlotType.None;

        if (draggedItemImage != null)
        {
            draggedItemImage.gameObject.SetActive(false);
        }

        if (originalFromSlotType == EquipmentSlotType.None) return; // No valid item was dragged

        if (dropTargetObject != null)
        {
            // Try to drop on an InventorySlotUI
            InventorySlotUI targetInventorySlotUI = dropTargetObject.GetComponentInParent<InventorySlotUI>();
            if (targetInventorySlotUI != null)
            {
                // Unequip item and try to add to inventory
                ItemData itemToUnequip = EquipmentManager.Instance.GetEquippedItem(originalFromSlotType);
                if (itemToUnequip != null)
                {
                    EquipmentManager.Instance.UnequipItem(originalFromSlotType);
                    // Add the unequipped item to the inventory (it will try to add to targetInventorySlotUI's index if possible)
                    // InventoryManager.Instance.AddItem(itemToUnequip); // AddItem will find an available slot
                    // The specific target slot for unequipped items is usually handled by AddItem finding the next available.
                    // If you want to force it to a specific slot, InventoryManager would need a modified AddItem or SetSlotData.
                }
                return; // Handled drop on an InventorySlotUI
            }

            // Try to drop on another EquipmentSlotUI (for swapping)
            EquipmentSlotUI targetEquipmentSlotUI = dropTargetObject.GetComponentInParent<EquipmentSlotUI>();
            if (targetEquipmentSlotUI != null)
            {

                ItemData sourceItem = EquipmentManager.Instance.GetEquippedItem(originalFromSlotType);
                ItemData targetItem = EquipmentManager.Instance.GetEquippedItem(targetEquipmentSlotUI.slotType);

                // Check if target slot is suitable for source item
                if (sourceItem != null && sourceItem.isEquippable && sourceItem.equipmentSlotType == targetEquipmentSlotUI.slotType)
                {
                    // Unequip target item first (it goes to inventory)
                    if (targetItem != null)
                    {
                        EquipmentManager.Instance.UnequipItem(targetEquipmentSlotUI.slotType);
                    }
                    
                    // Equip source item to target slot (it will remove from originalFromSlotType)
                    EquipmentManager.Instance.EquipItem(sourceItem, -1); // -1 indicates it's already "removed" from equipment slot logic
                }
                else
                {
                    Debug.LogWarning($"Cannot swap {sourceItem?.itemName ?? "null"} to {targetEquipmentSlotUI.slotType} slot. Item not suitable.");
                    // Item returns visually to original equipment slot via event update
                }
                return; // Handled drop on an EquipmentSlotUI (potentially a swap)
            }
        }
        
        // If dropped outside any valid slot (Inventory or Equipment)
        // Item dragged from equipment, unequip it back to inventory
        ItemData itemToReturn = EquipmentManager.Instance.GetEquippedItem(originalFromSlotType);
        if (itemToReturn != null)
        {
            EquipmentManager.Instance.UnequipItem(originalFromSlotType); // This adds it to inventory
            Debug.Log($"Item {itemToReturn.itemName} unequipped and returned to inventory.");
        }
        else
        {
            Debug.LogWarning("No item to return from equipment slot.");
        }
    }
}