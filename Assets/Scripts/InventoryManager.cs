// Path: Assets/_ProjectName/Scripts/Inventory/InventoryManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int inventorySize = 20;
    // [SerializeField] private int equipmentSlotsSize = 4; // Example for dedicated equipment slots

    private List<InventorySlotData> _inventorySlots = new List<InventorySlotData>();
    // private List<InventorySlotData> _equipmentSlots = new List<InventorySlotData>(); // Example

    public event Action<int> OnInventorySlotChanged; // For specific slot updates
    public event Action OnInventoryChanged; // For general updates (e.g., after sorting or full refresh)

    public int InventorySize => inventorySize;
    // public int EquipmentSlotsSize => equipmentSlotsSize;

    private void Awake()
    {
        InitializeInventory();
        // InitializeEquipmentSlots();
        // LoadInventory(); // We'll add this later
    }

    private void Start()
    {
        // Notify UI to draw initial state
        OnInventoryChanged?.Invoke();
        for(int i = 0; i < _inventorySlots.Count; i++)
        {
            OnInventorySlotChanged?.Invoke(i);
        }
    }


    private void InitializeInventory()
    {
        _inventorySlots = new List<InventorySlotData>(inventorySize);
        for (int i = 0; i < inventorySize; i++)
        {
            _inventorySlots.Add(new InventorySlotData(null, 0));
        }
    }

    public InventorySlotData GetSlotAtIndex(int index)
    {
        if (index < 0 || index >= _inventorySlots.Count)
        {
            return null;
        }
        return _inventorySlots[index];
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        // Try to stack with existing items first
        if (item.isStackable)
        {
            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                InventorySlotData slot = _inventorySlots[i];
                if (slot.itemData == item && slot.quantity < item.maxStackSize)
                {
                    int amountCanAdd = item.maxStackSize - slot.quantity;
                    int amountToAdd = Mathf.Min(quantity, amountCanAdd);
                    
                    slot.AddQuantity(amountToAdd);
                    quantity -= amountToAdd;
                    OnInventorySlotChanged?.Invoke(i);

                    if (quantity <= 0) return true;
                }
            }
        }

        // Try to add to an empty slot
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            InventorySlotData slot = _inventorySlots[i];
            if (slot.itemData == null)
            {
                int amountToAdd = item.isStackable ? Mathf.Min(quantity, item.maxStackSize) : 1;
                
                slot.itemData = item;
                slot.AddQuantity(amountToAdd); // AddQuantity will cap if it's a new item
                quantity -= amountToAdd;
                OnInventorySlotChanged?.Invoke(i);

                if (quantity <= 0) return true;

                // If item is not stackable, and we added one, we are done with this single item.
                if (!item.isStackable) {
                    if (quantity > 0) Debug.LogWarning($"Added one non-stackable {item.displayName}, but more quantity ({quantity}) was requested. Adding remaining to new slots if possible.");
                    // Continue to add remaining quantity to new slots if multiple non-stackable items were requested (e.g. picking up a pile of swords)
                     if (quantity <= 0) return true; else continue;
                }
            }
        }

        if (quantity > 0)
        {
            Debug.LogWarning($"Inventory full. Could not add {quantity}x {item.displayName}.");
            return false;
        }
        return true;
    }

    public void RemoveItem(int slotIndex, int quantity = 1)
    {
        if (slotIndex < 0 || slotIndex >= _inventorySlots.Count) return;

        InventorySlotData slot = _inventorySlots[slotIndex];
        if (slot.itemData != null && quantity > 0)
        {
            slot.RemoveQuantity(quantity);
            OnInventorySlotChanged?.Invoke(slotIndex);
        }
    }

    public void MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _inventorySlots.Count ||
            toIndex < 0 || toIndex >= _inventorySlots.Count ||
            fromIndex == toIndex)
            return;

        InventorySlotData fromSlot = _inventorySlots[fromIndex];
        InventorySlotData toSlot = _inventorySlots[toIndex];

        if (fromSlot.itemData == null) return; // Moving an empty slot

        // If target slot is empty OR items are the same and stackable
        if (toSlot.itemData == null || (toSlot.itemData == fromSlot.itemData && toSlot.itemData.isStackable))
        {
            if (toSlot.itemData == null) // Moving to empty slot
            {
                toSlot.itemData = fromSlot.itemData;
                toSlot.quantity = 0; // Ensure it's clean before adding
            }

            int spaceInToSlot = toSlot.itemData.maxStackSize - toSlot.quantity;
            int amountToMove = Mathf.Min(fromSlot.quantity, spaceInToSlot);
            
            if (toSlot.itemData == fromSlot.itemData && !toSlot.itemData.isStackable && toSlot.quantity > 0)
            {
                // Special case: swapping two non-stackable items of the same type (or any different non-stackable items)
                 // This path means target slot is NOT empty and they are NOT stackable or NOT same item
                // So we swap
                ItemData tempItem = fromSlot.itemData;
                int tempQuantity = fromSlot.quantity;

                fromSlot.itemData = toSlot.itemData;
                fromSlot.quantity = toSlot.quantity;

                toSlot.itemData = tempItem;
                toSlot.quantity = tempQuantity;
            }
            else // Stacking or moving to empty
            {
                toSlot.AddQuantity(amountToMove);
                fromSlot.RemoveQuantity(amountToMove); // This will clear fromSlot if quantity becomes 0
            }

        }
        else // Items are different, or same but not stackable and target is full: Swap them
        {
            ItemData tempItem = fromSlot.itemData;
            int tempQuantity = fromSlot.quantity;

            fromSlot.itemData = toSlot.itemData;
            fromSlot.quantity = toSlot.quantity;

            toSlot.itemData = tempItem;
            toSlot.quantity = tempQuantity;
        }

        OnInventorySlotChanged?.Invoke(fromIndex);
        OnInventorySlotChanged?.Invoke(toIndex);
    }

    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _inventorySlots.Count) return;

        InventorySlotData slot = _inventorySlots[slotIndex];
        if (slot.itemData != null)
        {
            // Assuming PlayerStats is accessible, e.g., via a singleton or passed in
            PlayerStats playerStats = GetComponentInParent<PlayerStats>(); // Example: if InventoryManager is child of Player
            // Or find it: PlayerStats playerStats = FindObjectOfType<PlayerStats>(); (Not ideal for performance)

            slot.itemData.UseItem(playerStats); // PlayerStats can be null if not implemented

            if (slot.itemData.itemType == ItemType.Consumable || 
               (slot.itemData.itemType == ItemType.Equipment /*&& autoEquipOnUse?*/ )) // Decide if 'Use' also means 'Equip'
            {
                slot.RemoveQuantity(1);
                OnInventorySlotChanged?.Invoke(slotIndex);
            }
            // If it's equipment, you might want a separate EquipItem(slotIndex) method
        }
    }

    // --- Save/Load (Basic Structure) ---
    public List<InventorySlotSaveData> GetSaveData()
    {
        var saveData = new List<InventorySlotSaveData>();
        foreach (var slot in _inventorySlots)
        {
            if (slot.itemData != null)
            {
                saveData.Add(new InventorySlotSaveData(slot.itemData.id, slot.quantity));
            }
            else
            {
                saveData.Add(new InventorySlotSaveData(null, 0)); // Represent empty slot
            }
        }
        return saveData;
    }

    public void LoadSaveData(List<InventorySlotSaveData> saveData, Dictionary<string, ItemData> itemDatabase)
    {
        if (saveData == null || itemDatabase == null)
        {
            InitializeInventory(); // Fallback to default empty inventory
            return;
        }

        // Ensure inventorySlots list matches saveData size, or re-initialize
        if (_inventorySlots.Count != saveData.Count)
        {
            _inventorySlots = new List<InventorySlotData>(saveData.Count);
            for (int i = 0; i < saveData.Count; i++)
            {
                _inventorySlots.Add(new InventorySlotData(null, 0));
            }
            inventorySize = saveData.Count; // Update size if loaded data has different size
        }


        for (int i = 0; i < saveData.Count; i++)
        {
            if (i >= _inventorySlots.Count) break; // Should not happen if lists are synced

            _inventorySlots[i].Clear();
            if (!string.IsNullOrEmpty(saveData[i].itemId) && itemDatabase.TryGetValue(saveData[i].itemId, out ItemData item))
            {
                _inventorySlots[i].itemData = item;
                _inventorySlots[i].quantity = saveData[i].quantity;
            }
        }
        OnInventoryChanged?.Invoke(); // Notify UI to refresh all slots
        for(int i = 0; i < _inventorySlots.Count; i++)
        {
            OnInventorySlotChanged?.Invoke(i); // Notify each slot individually as well
        }
    }
}

// Helper struct for saving/loading
[System.Serializable]
public struct InventorySlotSaveData
{
    public string itemId;
    public int quantity;

    public InventorySlotSaveData(string id, int qty)
    {
        itemId = id;
        quantity = qty;
    }
}