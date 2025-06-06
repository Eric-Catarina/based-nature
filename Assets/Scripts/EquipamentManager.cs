
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour 
{
    public static EquipmentManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] public List<EquipmentSlotUI> equipmentSlotUIs = new List<EquipmentSlotUI>();
    [SerializeField] public InventoryManager inventoryManager;

    private Dictionary<EquipmentSlotType, ItemData> _equippedItems = new Dictionary<EquipmentSlotType, ItemData>();
    private Dictionary<EquipmentSlotType, EquipmentSlotUI> _uiSlotMapping = new Dictionary<EquipmentSlotType, EquipmentSlotUI>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        
        
        
        InitializeEquipmentSlots();
    }

    public void InitializeEquipmentSlots()
    {
        if (inventoryManager == null) Debug.LogError("[EquipmentManager] InventoryManager not assigned!");

        _equippedItems.Clear();
        _uiSlotMapping.Clear();

        foreach (EquipmentSlotUI uiSlot in equipmentSlotUIs)
        {
            if (uiSlot != null)
            {
                uiSlot.Initialize(this);
                if (_uiSlotMapping.ContainsKey(uiSlot.slotType))
                {

                }
                _uiSlotMapping[uiSlot.slotType] = uiSlot;
                _equippedItems[uiSlot.slotType] = null;
                uiSlot.ClearSlotDisplay();
            }
            else
            {

            }
        }
    }

    public bool EquipItem(ItemData itemToEquip, int inventorySlotIndexToRemoveFrom)
    {
        if (itemToEquip == null || !itemToEquip.isEquippable || itemToEquip.equipmentSlotType == EquipmentSlotType.None)
        {
            return false;
        }

        EquipmentSlotType targetSlotType = itemToEquip.equipmentSlotType;

        if (!_uiSlotMapping.ContainsKey(targetSlotType))
        {

            return false;
        }

        ItemData previouslyEquippedItem = null;
        if (_equippedItems.TryGetValue(targetSlotType, out ItemData currentItemInSlot))
        {
            previouslyEquippedItem = currentItemInSlot;
        }

        _equippedItems[targetSlotType] = itemToEquip;
        if (_uiSlotMapping.TryGetValue(targetSlotType, out EquipmentSlotUI slotUI))
        {
            slotUI.DisplayItem(itemToEquip);
        }

        if (inventoryManager != null)
        {
            if (inventorySlotIndexToRemoveFrom != -1) 
            {
                inventoryManager.RemoveItemFromSlot(inventorySlotIndexToRemoveFrom, 1);
            }
            
            if (previouslyEquippedItem != null) 
            {
                inventoryManager.AddItem(previouslyEquippedItem, 1);
            }
        }
        else
        {

        }
        return true;
    }

    public void UnequipItem(EquipmentSlotType slotTypeToUnequip)
    {
        if (!_equippedItems.TryGetValue(slotTypeToUnequip, out ItemData itemToUnequip) || itemToUnequip == null)
        {
            return;
        }

        _equippedItems[slotTypeToUnequip] = null;

        if (_uiSlotMapping.TryGetValue(slotTypeToUnequip, out EquipmentSlotUI slotUI))
        {
            slotUI.ClearSlotDisplay();
        }

        if (inventoryManager != null)
        {
            inventoryManager.AddItem(itemToUnequip, 1);
        }
        else
        {

        }
    }

    public ItemData GetEquippedItem(EquipmentSlotType type)
    {
        _equippedItems.TryGetValue(type, out ItemData item);
        return item;
    }

    public Dictionary<EquipmentSlotType, ItemData> GetAllEquippedItems()
    {
        return new Dictionary<EquipmentSlotType, ItemData>(_equippedItems); 
    }
}