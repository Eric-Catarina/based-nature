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
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotUI slotPrefab;

    [Header("Tooltip References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipItemNameText;
    [SerializeField] private TextMeshProUGUI tooltipItemDescriptionText;
    [SerializeField] private TextMeshProUGUI tooltipItemTypeText;

    [Header("Drag & Drop Visuals")]
    [SerializeField] public Image draggedItemImage;

    private List<InventorySlotUI> _slotUIInstances = new List<InventorySlotUI>();
    private PlayerControls _playerControls; // Certifique-se que PlayerControls.cs existe e está configurado
    private bool _isPanelOpen = false;

    private int _draggedFromSlotIndex = -1;

    private void Awake()
    {
        if (Application.isPlaying) // PlayerControls pode dar erro no editor se não estiver em play mode
        {
            _playerControls = new PlayerControls();
        }

        if (inventoryManager == null) Debug.LogError("InventoryManager not assigned to InventoryUI.");
        if (inventoryPanel == null) Debug.LogError("InventoryPanel not assigned to InventoryUI.");
        if (slotContainer == null) Debug.LogError("SlotContainer not assigned to InventoryUI.");
        if (slotPrefab == null) Debug.LogError("SlotPrefab not assigned to InventoryUI.");

        if (draggedItemImage) draggedItemImage.gameObject.SetActive(false);
        if (tooltipPanel) tooltipPanel.SetActive(false);
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
        if (_draggedFromSlotIndex != -1 && draggedItemImage != null && draggedItemImage.gameObject.activeSelf)
        {
            // A posição é atualizada pelo OnDrag do InventorySlotUI
        }
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
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePanel()
    {
        inventoryPanel.SetActive(false);
        _isPanelOpen = false;
        HideTooltip();
        if (draggedItemImage && draggedItemImage.gameObject.activeSelf) // Cancel drag if panel closes
        {
            draggedItemImage.gameObject.SetActive(false);
            if(_draggedFromSlotIndex != -1) UpdateSpecificSlotUI(_draggedFromSlotIndex); // Restore visual of dragged slot
            _draggedFromSlotIndex = -1;
        }
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            foreach(var slot in _slotUIInstances) { if(slot != null) Destroy(slot.gameObject); }
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
        slotRectTransform.GetWorldCorners(slotCorners); // slotCorners[2] is top-right
        slotGlobalPosition = slotCorners[2]; 

        Canvas canvas = GetComponentInParent<Canvas>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, slotGlobalPosition, canvas.worldCamera, out localPoint);
        
        tooltipRect.anchoredPosition = localPoint + new Vector2(tooltipRect.sizeDelta.x * tooltipRect.pivot.x + 5, -tooltipRect.sizeDelta.y * (1-tooltipRect.pivot.y) - 5) ;


        Vector2 canvasSize = (canvas.transform as RectTransform).sizeDelta;
        Vector2 anchoredPos = tooltipRect.anchoredPosition;
        Vector2 tooltipSize = tooltipRect.sizeDelta;

        if (anchoredPos.x + tooltipSize.x * (1 - tooltipRect.pivot.x) > canvasSize.x / 2)
        {
            slotGlobalPosition = slotCorners[3]; // top-left
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, slotGlobalPosition, canvas.worldCamera, out localPoint);
            anchoredPos.x = localPoint.x - tooltipSize.x * tooltipRect.pivot.x - 5;
        }
        if (anchoredPos.y - tooltipSize.y * tooltipRect.pivot.y < -canvasSize.y / 2)
        {
            slotGlobalPosition = slotCorners[0]; // bottom-right (if changing vertical anchor)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, slotGlobalPosition, canvas.worldCamera, out localPoint);
            anchoredPos.y = localPoint.y + tooltipSize.y * (1-tooltipRect.pivot.y) + 5 + slotRectTransform.sizeDelta.y; // Adjust to be above slot
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

        if (fromIndex == -1) return; // Drag was not properly started or was cancelled

        if (dropTargetObject != null)
        {
            InventorySlotUI targetSlotUI = dropTargetObject.GetComponentInParent<InventorySlotUI>();
            if (targetSlotUI != null)
            {
                int toIndex = _slotUIInstances.IndexOf(targetSlotUI);
                if (toIndex != -1 && fromIndex != toIndex)
                {
                    inventoryManager.MoveItem(fromIndex, toIndex);
                }
                else
                {
                    UpdateSpecificSlotUI(fromIndex);
                }
                return;
            }
        }
        
        UpdateSpecificSlotUI(fromIndex);
    }
}