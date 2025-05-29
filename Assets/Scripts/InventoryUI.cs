// Path: Assets/_ProjectName/Scripts/UI/InventoryUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro; // For Tooltip

public class InventoryUI : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject inventoryPanel; // The root panel of the inventory UI
    [SerializeField] private Transform slotContainer; // Parent object with GridLayoutGroup for slots
    [SerializeField] private InventorySlotUI slotPrefab;

    [Header("Tooltip References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipItemNameText;
    [SerializeField] private TextMeshProUGUI tooltipItemDescriptionText;
    [SerializeField] private TextMeshProUGUI tooltipItemTypeText; // Optional

    [Header("Drag & Drop Visuals")]
    [SerializeField] private Image draggedItemImage; // A separate image on canvas for drag visual

    private List<InventorySlotUI> _slotUIInstances = new List<InventorySlotUI>();
    private PlayerControls _playerControls;
    private bool _isPanelOpen = false;

    private int _draggedFromSlotIndex = -1;


    private void Awake()
    {
        _playerControls = new PlayerControls();
        if (inventoryManager == null) Debug.LogError("InventoryManager not assigned to InventoryUI.");
        if (inventoryPanel == null) Debug.LogError("InventoryPanel not assigned to InventoryUI.");
        if (slotContainer == null) Debug.LogError("SlotContainer not assigned to InventoryUI.");
        if (slotPrefab == null) Debug.LogError("SlotPrefab not assigned to InventoryUI.");

        if (draggedItemImage) draggedItemImage.gameObject.SetActive(false);
        if (tooltipPanel) tooltipPanel.SetActive(false);
        ClosePanel(); // Start closed
    }

    private void OnEnable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventorySlotChanged += UpdateSpecificSlotUI;
            inventoryManager.OnInventoryChanged += RefreshAllSlotsUI; // For full redraws
        }
        _playerControls.Gameplay.Enable(); // Assuming inventory is part of Gameplay map
        _playerControls.Gameplay.OpenInventory.performed += TogglePanelInput;
    }

    private void OnDisable()
    {
        if (inventoryManager != null)
        {
            inventoryManager.OnInventorySlotChanged -= UpdateSpecificSlotUI;
            inventoryManager.OnInventoryChanged -= RefreshAllSlotsUI;
        }
        _playerControls.Gameplay.OpenInventory.performed -= TogglePanelInput;
        _playerControls.Gameplay.Disable();
    }

    private void Start()
    {
        CreateSlotUIInstances();
        RefreshAllSlotsUI(); // Initial draw
    }

    private void TogglePanelInput(InputAction.CallbackContext context)
    {
        if (_isPanelOpen) ClosePanel();
        else OpenPanel();
    }

    public void OpenPanel()
    {
        inventoryPanel.SetActive(true);
        _isPanelOpen = true;
        Time.timeScale = 0f; // Pause game
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Potentially switch Input Action Map to a "UI" map
    }

    public void ClosePanel()
    {
        inventoryPanel.SetActive(false);
        _isPanelOpen = false;
        HideTooltip(); // Ensure tooltip is hidden when panel closes
        Time.timeScale = 1f; // Resume game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Switch back to "Gameplay" Input Action Map
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
        _slotUIInstances[slotIndex].UpdateSlotDisplay(inventoryManager.GetSlotAtIndex(slotIndex));
    }

    private void RefreshAllSlotsUI()
    {
        if (inventoryManager == null || _slotUIInstances.Count != inventoryManager.InventorySize)
        {
            // This might happen if inventory size changes dynamically, though not planned for this setup
            // Re-create slots if counts don't match (more robust)
             foreach(var slot in _slotUIInstances) if(slot != null) Destroy(slot.gameObject);
            _slotUIInstances.Clear();
            CreateSlotUIInstances(); // This will create the correct number
        }

        for (int i = 0; i < inventoryManager.InventorySize; i++)
        {
            if (i < _slotUIInstances.Count) // Check to prevent out of bounds if lists somehow mismatch
            {
                _slotUIInstances[i].UpdateSlotDisplay(inventoryManager.GetSlotAtIndex(i));
            }
        }
    }

    public void ShowTooltip(ItemData item, RectTransform slotRectTransform)
    {
        if (tooltipPanel == null || item == null) return;

        tooltipItemNameText.text = item.displayName;
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

        // Position tooltip (simple example: to the right of the slot)
        // More complex positioning might be needed to keep it on screen
        Canvas canvas = GetComponentInParent<Canvas>();
        Vector3 slotWorldPos = slotRectTransform.TransformPoint(slotRectTransform.rect.center);
        Vector2 slotScreenPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            slotWorldPos, 
            canvas.worldCamera, 
            out slotScreenPos
        );

        // Crude positioning logic, adjust as needed
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        tooltipRect.anchoredPosition = slotRectTransform.anchoredPosition + new Vector2(slotRectTransform.sizeDelta.x * 0.75f, -slotRectTransform.sizeDelta.y * 0.5f);

        // Ensure it fits on screen (basic implementation)
        Vector2 tooltipSize = tooltipRect.sizeDelta;
        Vector2 screenRes = new Vector2(Screen.width / canvas.scaleFactor, Screen.height / canvas.scaleFactor); // Adjust for canvas scaler

        if (tooltipRect.anchoredPosition.x + tooltipSize.x > screenRes.x / 2)
            tooltipRect.anchoredPosition = new Vector2(slotRectTransform.anchoredPosition.x - tooltipSize.x - (slotRectTransform.sizeDelta.x * 0.25f) , tooltipRect.anchoredPosition.y);
        if (tooltipRect.anchoredPosition.y - tooltipSize.y < -screenRes.y / 2)
             tooltipRect.anchoredPosition = new Vector2(tooltipRect.anchoredPosition.x, slotRectTransform.anchoredPosition.y + tooltipSize.y + (slotRectTransform.sizeDelta.y * 0.5f));
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

    // --- Drag and Drop Logic ---
    public void OnDragStarted(int fromSlotIndex)
    {
        _draggedFromSlotIndex = fromSlotIndex;
        if (draggedItemImage != null)
        {
            InventorySlotData slotData = inventoryManager.GetSlotAtIndex(fromSlotIndex);
            if (slotData != null && slotData.itemData != null)
            {
                draggedItemImage.sprite = slotData.itemData.icon;
                draggedItemImage.gameObject.SetActive(true);
            }
        }
    }

    public void OnDragEnded(int originalFromSlotIndex, GameObject dropTargetObject)
    {
        if (draggedItemImage != null)
        {
            draggedItemImage.gameObject.SetActive(false);
        }

        int fromIndex = _draggedFromSlotIndex != -1 ? _draggedFromSlotIndex : originalFromSlotIndex; // Use the stored one if valid
         _draggedFromSlotIndex = -1; // Reset for next drag

        if (dropTargetObject != null)
        {
            InventorySlotUI targetSlotUI = dropTargetObject.GetComponentInParent<InventorySlotUI>(); // GetComponentInParent because raycast might hit child Image/Text
            if (targetSlotUI != null)
            {
                int toIndex = _slotUIInstances.IndexOf(targetSlotUI);
                if (toIndex != -1 && fromIndex != toIndex) // Ensure it's a valid, different slot
                {
                    inventoryManager.MoveItem(fromIndex, toIndex);
                }
                else
                {
                    // Dropped on itself or invalid target, refresh original slot display
                     UpdateSpecificSlotUI(fromIndex);
                }
                return; // Handled drop on a slot
            }
        }
        
        // If dropped outside a valid slot (e.g., on the panel background or outside inventory)
        // Or if dropTargetObject is null (dropped outside UI)
        // For now, just return item to original slot visually (logic for dropping items in world would go here)
        Debug.Log("Item dropped outside a valid slot.");
        UpdateSpecificSlotUI(fromIndex); // Refresh the original slot as nothing changed
    }
}