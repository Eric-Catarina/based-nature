// Path: Assets/_ProjectName/Scripts/EquipmentManager.cs
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    public List<EquipmentSlotUI> equipmentSlotUIs = new List<EquipmentSlotUI>();
    public InventoryManager inventoryManager;

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

    void InitializeEquipmentSlots()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not assigned in EquipmentManager!");
        }

        _equippedItems.Clear();
        _uiSlotMapping.Clear();

        foreach (EquipmentSlotUI uiSlot in equipmentSlotUIs)
        {
            if (uiSlot != null)
            {
                uiSlot.Initialize(this);
                if (_uiSlotMapping.ContainsKey(uiSlot.slotType))
                {
                     Debug.LogWarning($"Duplicate EquipmentType {uiSlot.slotType} configured in UI slots. Check your EquipmentSlotUI components.");
                }
                _uiSlotMapping[uiSlot.slotType] = uiSlot;
                _equippedItems[uiSlot.slotType] = null;
                uiSlot.ClearSlotDisplay();
            }
            else
            {
                Debug.LogError("An EquipmentSlotUI is not assigned in the EquipmentManager's list.");
            }
        }
    }

    public bool EquipItem(ItemData itemToEquip, int inventorySlotIndexToRemoveFrom) // Pass -1 if item is already 'removed' (e.g., from another equip slot)
    {
        if (itemToEquip == null || !itemToEquip.isEquippable || itemToEquip.equipmentSlotType == EquipmentSlotType.None)
        {
            return false;
        }

        EquipmentSlotType targetSlotType = itemToEquip.equipmentSlotType;

        if (!_uiSlotMapping.ContainsKey(targetSlotType))
        {
            Debug.LogWarning($"No UI slot configured for equipment type: {targetSlotType}");
            return false;
        }

        ItemData previouslyEquippedItem = null;
        if (_equippedItems.ContainsKey(targetSlotType) && _equippedItems[targetSlotType] != null)
        {
            previouslyEquippedItem = _equippedItems[targetSlotType];
        }

        _equippedItems[targetSlotType] = itemToEquip;
        _uiSlotMapping[targetSlotType].DisplayItem(itemToEquip); // Update UI for the equipped slot

        if (inventoryManager != null)
        {
            if (inventorySlotIndexToRemoveFrom != -1) // If it came from an inventory slot, remove it
            {
                inventoryManager.RemoveItemFromSlot(inventorySlotIndexToRemoveFrom, 1);
            }
            // If it came from another equipment slot, it's already logically "removed"
            // The item previously equipped in this slot goes back to inventory
            if (previouslyEquippedItem != null)
            {
                inventoryManager.AddItem(previouslyEquippedItem, 1);
            }
        }
        else
        {
             Debug.LogError("InventoryManager not found to manage items.");
        }

        Debug.Log($"Equipped: {itemToEquip.itemName} in slot {targetSlotType}. Returned to inventory: {(previouslyEquippedItem != null ? previouslyEquippedItem.itemName : "None")}");
        return true;
    }

    public void UnequipItem(EquipmentSlotType slotTypeToUnequip)
    {
        if (!_equippedItems.ContainsKey(slotTypeToUnequip) || _equippedItems[slotTypeToUnequip] == null)
        {
            return;
        }

        ItemData itemToUnequip = _equippedItems[slotTypeToUnequip];
        _equippedItems[slotTypeToUnequip] = null;

        if (_uiSlotMapping.ContainsKey(slotTypeToUnequip))
        {
            _uiSlotMapping[slotTypeToUnequip].ClearSlotDisplay(); // Update UI for the unequipped slot
        }

        if (inventoryManager != null)
        {
            inventoryManager.AddItem(itemToUnequip, 1);
        }
        else
        {
            Debug.LogWarning($"Could not return {itemToUnequip.itemName} to inventory: InventoryManager not assigned.");
        }
        
        Debug.Log($"Unequipped: {itemToUnequip.itemName} from slot {slotTypeToUnequip}");
    }

    public ItemData GetEquippedItem(EquipmentSlotType type)
    {
        _equippedItems.TryGetValue(type, out ItemData item);
        return item;
    }
}