using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class InventoryUI : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private GameObject equipmentPanel;

    [Header("Tooltip References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipItemNameText;
    [SerializeField] private TextMeshProUGUI tooltipItemDescriptionText;
    [SerializeField] private TextMeshProUGUI tooltipItemTypeText;

    [Header("Drag & Drop Visuals")]
    [SerializeField] public Image draggedItemImage;
    private CanvasGroup inventoryCanvasGroup;

    private List<InventorySlotUI> _slotUIInstances = new List<InventorySlotUI>();
    private PlayerControls _playerControls;
    private bool _isPanelOpen = false;
    private RectTransform _inventoryPanelRectTransform;

    private int _draggedFromSlotIndex = -1;

    [Header("Tooltip Animation")]
    [Tooltip("Duração da animação de scale para o tooltip.")]
    [SerializeField] private float tooltipAnimationDuration = 0.2f;
    [Tooltip("Escala máxima do tooltip durante a animação de popup.")]
    [SerializeField] private float tooltipMaxScale = 1.1f;
    [Tooltip("Facilidade (Ease) da animação de scale do tooltip.")]
    [SerializeField] private Ease tooltipEase = Ease.OutBack;

    [Header("Inventory Cooldown")] 
    [Tooltip("Tempo de cooldown para abrir/fechar o inventário.")]
    [SerializeField] private float inventoryToggleCooldown = 0.5f;
    private float _lastToggleTime; 

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
        if (equipmentPanel == null) Debug.LogError("EquipmentPanel not assigned to InventoryUI.");

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
            inventoryCanvasGroup.alpha = 1f;
        }
        _inventoryPanelRectTransform = inventoryPanel.GetComponent<RectTransform>();

        _lastToggleTime = -inventoryToggleCooldown; 
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

    }

    private void TogglePanelInput(InputAction.CallbackContext context)
    {
        
        if (Time.unscaledTime - _lastToggleTime < inventoryToggleCooldown)
        {
            
            return;
        }

        if (_isPanelOpen) ClosePanel();
        else OpenPanel();

        _lastToggleTime = Time.unscaledTime; 
    }

    public void OpenPanel()
    {
        inventoryPanel.SetActive(true);
        if (equipmentPanel) equipmentPanel.SetActive(true);
        _isPanelOpen = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UITweenAnimations.PanelAppear(_inventoryPanelRectTransform, 0.3f);
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.pickItemSound);
    }

    public void ClosePanel()
    {
        if (!_isPanelOpen) return;

        _isPanelOpen = false;

        HideTooltip();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (draggedItemImage && draggedItemImage.gameObject.activeSelf)
        {
            draggedItemImage.gameObject.SetActive(false);
            if (_draggedFromSlotIndex != -1) UpdateSpecificSlotUI(_draggedFromSlotIndex);
            _draggedFromSlotIndex = -1;
        }


        if (_inventoryPanelRectTransform != null)
        {
            UITweenAnimations.PanelDisappear(_inventoryPanelRectTransform, 0.3f, onComplete: () =>
            {
                inventoryPanel.SetActive(false);
            });
        }
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

    public void ShowTooltip(ItemData item)
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

        
        if (!tooltipPanel.activeSelf)
        {
            
            // tooltipPanel.transform.localScale = Vector3.zero;

            
            tooltipPanel.SetActive(true);

            
            // tooltipPanel.transform.DOScale(tooltipMaxScale, tooltipAnimationDuration)
            //     .From(0f)
            //     .SetEase(tooltipEase)
            //     .SetUpdate(true);

        }

    }


    public void HideTooltip()
    {
        if (tooltipPanel)
        {
            
            DOTween.Kill(tooltipPanel.transform);
tooltipPanel.SetActive(false) ;
            
            // tooltipPanel.transform.DOScale(0f, tooltipAnimationDuration * 0.5f)
            //     .SetEase(Ease.InBack)
            //     .OnComplete(() => tooltipPanel.SetActive(false)) 
            //     .SetUpdate(true);
        }
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
                return;
            }


            EquipmentSlotUI targetEquipmentSlotUI = dropTargetObject.GetComponentInParent<EquipmentSlotUI>();
            if (targetEquipmentSlotUI != null)
            {
                ItemData itemToEquip = inventoryManager.GetSlotAtIndex(fromIndex).itemData;
                if (itemToEquip != null && itemToEquip.isEquippable && itemToEquip.equipmentSlotType == targetEquipmentSlotUI.slotType)
                {
                    EquipmentManager.Instance.EquipItem(itemToEquip, fromIndex);

                }
                else
                {

                    UpdateSpecificSlotUI(fromIndex);
                }
                return;
            }
        }



        ItemData itemToReturn = inventoryManager.GetSlotAtIndex(fromIndex).itemData;
        if (itemToReturn != null)
        {
            inventoryManager.RemoveItemFromSlot(fromIndex, 1);

        }
        else
        {

        }
        UpdateSpecificSlotUI(fromIndex);
    }

    private EquipmentSlotType _draggedFromEquipmentSlotType = EquipmentSlotType.None;



    public void OnDragStartedFromEquipment(ItemData itemToDrag, EquipmentSlotType fromSlotType)
    {
        _draggedFromSlotIndex = -1;
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

        _draggedFromEquipmentSlotType = EquipmentSlotType.None;

        if (draggedItemImage != null)
        {
            draggedItemImage.gameObject.SetActive(false);
        }

        if (originalFromSlotType == EquipmentSlotType.None) return;

        if (dropTargetObject != null)
        {

            InventorySlotUI targetInventorySlotUI = dropTargetObject.GetComponentInParent<InventorySlotUI>();
            if (targetInventorySlotUI != null)
            {

                ItemData itemToUnequip = EquipmentManager.Instance.GetEquippedItem(originalFromSlotType);
                if (itemToUnequip != null)
                {
                    EquipmentManager.Instance.UnequipItem(originalFromSlotType);




                }
                return;
            }


            EquipmentSlotUI targetEquipmentSlotUI = dropTargetObject.GetComponentInParent<EquipmentSlotUI>();
            if (targetEquipmentSlotUI != null)
            {

                ItemData sourceItem = EquipmentManager.Instance.GetEquippedItem(originalFromSlotType);
                ItemData targetItem = EquipmentManager.Instance.GetEquippedItem(targetEquipmentSlotUI.slotType);


                if (sourceItem != null && sourceItem.isEquippable && sourceItem.equipmentSlotType == targetEquipmentSlotUI.slotType)
                {

                    if (targetItem != null)
                    {
                        EquipmentManager.Instance.UnequipItem(targetEquipmentSlotUI.slotType);
                    }


                    EquipmentManager.Instance.EquipItem(sourceItem, -1);
                }
                else
                {


                }
                return;
            }
        }



        ItemData itemToReturn = EquipmentManager.Instance.GetEquippedItem(originalFromSlotType);
        if (itemToReturn != null)
        {
            EquipmentManager.Instance.UnequipItem(originalFromSlotType);

        }
        else
        {

        }
    }
}