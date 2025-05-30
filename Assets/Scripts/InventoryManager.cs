// Path: Assets/_ProjectName/Scripts/Inventory/InventoryManager.cs
using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
 

    [SerializeField] private int inventorySize = 20;
    public int InventorySize => inventorySize;

    private List<InventorySlotData> _inventorySlots = new List<InventorySlotData>();

    public event Action<int> OnInventorySlotChanged;
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializeInventory();
    }

    private void InitializeInventory()
    {
        _inventorySlots = new List<InventorySlotData>(inventorySize);
        for (int i = 0; i < inventorySize; i++)
        {
            _inventorySlots.Add(new InventorySlotData());
        }
    }

    public InventorySlotData GetSlotAtIndex(int index)
    {
        if (index < 0 || index >= _inventorySlots.Count) return null;
        return _inventorySlots[index];
    }

    public bool AddItem(ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return false;

        bool itemAdded = false;

        if (item.isStackable)
        {
            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                if (!_inventorySlots[i].IsEmpty() && _inventorySlots[i].itemData == item)
                {
                    int spaceInStack = item.maxStackSize - _inventorySlots[i].quantity;
                    if (spaceInStack > 0)
                    {
                        int amountToAdd = Mathf.Min(quantity, spaceInStack);
                        _inventorySlots[i].AddQuantity(amountToAdd);
                        quantity -= amountToAdd;
                        OnInventorySlotChanged?.Invoke(i);
                        itemAdded = true;
                        if (quantity <= 0) break;
                    }
                }
            }
        }

        if (quantity > 0)
        {
            for (int i = 0; i < _inventorySlots.Count; i++)
            {
                if (_inventorySlots[i].IsEmpty())
                {
                    int amountToAdd = item.isStackable ? Mathf.Min(quantity, item.maxStackSize) : 1;
                    
                    _inventorySlots[i].itemData = item;
                    _inventorySlots[i].SetQuantity(amountToAdd);
                    quantity -= amountToAdd;
                    OnInventorySlotChanged?.Invoke(i);
                    itemAdded = true;

                    if (quantity <= 0) break; 
                    if (!item.isStackable && quantity > 0) continue; 
                }
            }
        }
        
        if (!itemAdded && quantity > 0)
        {
             Debug.LogWarning($"Inventory full. Could not add any of {item.itemName}.");
             return false;
        }
        if (itemAdded && quantity > 0)
        {
             Debug.LogWarning($"Inventory partially full. Could not add all {quantity} of {item.itemName}.");
        }

        if(itemAdded) OnInventoryChanged?.Invoke();
        return itemAdded;
    }

    public bool RemoveItemFromSlot(int slotIndex, int quantityToRemove = 1)
    {
        if (slotIndex < 0 || slotIndex >= _inventorySlots.Count || _inventorySlots[slotIndex].IsEmpty() || quantityToRemove <= 0)
        {
            return false;
        }

        InventorySlotData slot = _inventorySlots[slotIndex];
        
        if (slot.quantity < quantityToRemove)
        {
            return false; 
        }

        slot.AddQuantity(-quantityToRemove);
        if (slot.quantity <= 0)
        {
            slot.Clear();
        }
        
        OnInventorySlotChanged?.Invoke(slotIndex);
        OnInventoryChanged?.Invoke();
        return true;
    }

    public void MoveItem(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= _inventorySlots.Count ||
            toIndex < 0 || toIndex >= _inventorySlots.Count ||
            fromIndex == toIndex)
        {
            return;
        }

        InventorySlotData fromSlot = _inventorySlots[fromIndex];
        InventorySlotData toSlot = _inventorySlots[toIndex];

        if (fromSlot.IsEmpty()) return;

        if (toSlot.IsEmpty())
        {
            _inventorySlots[toIndex] = new InventorySlotData(fromSlot.itemData, fromSlot.quantity);
            _inventorySlots[fromIndex].Clear();
        }
        else if (toSlot.itemData == fromSlot.itemData && toSlot.itemData.isStackable)
        {
            int spaceInToStack = toSlot.itemData.maxStackSize - toSlot.quantity;
            int amountToMove = Mathf.Min(fromSlot.quantity, spaceInToStack);

            if (amountToMove > 0)
            {
                toSlot.AddQuantity(amountToMove);
                fromSlot.AddQuantity(-amountToMove);
                if (fromSlot.quantity <= 0) fromSlot.Clear();
            }
            else
            {
                InventorySlotData temp = new InventorySlotData(fromSlot.itemData, fromSlot.quantity);
                _inventorySlots[fromIndex] = new InventorySlotData(toSlot.itemData, toSlot.quantity);
                _inventorySlots[toIndex] = temp;
            }
        }
        else 
        {
            InventorySlotData temp = new InventorySlotData(fromSlot.itemData, fromSlot.quantity);
            _inventorySlots[fromIndex] = new InventorySlotData(toSlot.itemData, toSlot.quantity);
            _inventorySlots[toIndex] = temp;
        }

        OnInventorySlotChanged?.Invoke(fromIndex);
        OnInventorySlotChanged?.Invoke(toIndex);
        OnInventoryChanged?.Invoke();
    }
    
    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _inventorySlots.Count || _inventorySlots[slotIndex].IsEmpty()) return;

        ItemData itemToUse = _inventorySlots[slotIndex].itemData;
        Debug.Log($"Attempting to use {itemToUse.itemName}");

        if (itemToUse.isUsable)
        {
            if (itemToUse.itemType == ItemType.Consumable)
            {
                Debug.Log($"{itemToUse.itemName} consumed. (Effect logic to be implemented)");
                RemoveItemFromSlot(slotIndex, 1);
            }
            // Add other use cases, e.g., equipping
        }
        else if (itemToUse.isEquippable)
        {
            Debug.Log($"Attempting to equip {itemToUse.itemName}. (Equipment logic to be implemented)");
            // EquipmentManager.Instance.EquipItem(itemToUse, slotIndex);
        }
        else
        {
            Debug.Log($"{itemToUse.itemName} is not usable or equippable.");
        }
    }

    public List<InventorySlotSaveData> GetSaveData()
    {
        List<InventorySlotSaveData> saveData = new List<InventorySlotSaveData>();
        foreach (InventorySlotData slot in _inventorySlots)
        {
            if (!slot.IsEmpty())
            {
                saveData.Add(new InventorySlotSaveData(slot.itemData.id, slot.quantity));
            }
            else
            {
                saveData.Add(new InventorySlotSaveData(null, 0));
            }
        }
        return saveData;
    }

    public void LoadSaveData(List<InventorySlotSaveData> slotSaveDataList, Dictionary<string, ItemData> itemDatabaseDict)
    {
        if (itemDatabaseDict == null)
        {
            Debug.LogError("ItemDatabase dictionary is null. Cannot load inventory.");
            InitializeInventory(); // Reset to empty or default
            OnInventoryChanged?.Invoke();
            return;
        }

        if (slotSaveDataList == null || slotSaveDataList.Count == 0) // No save data or empty save data
        {
            InitializeInventory(); // Reset to empty or default
            OnInventoryChanged?.Invoke();
            return;
        }
        
        if (_inventorySlots.Count != slotSaveDataList.Count && _inventorySlots.Count != inventorySize)
        {
             // If inventory size changed or mismatch, reinitialize to correct size first.
             // This prototype assumes inventorySize is fixed after Awake.
             // If it can change, you might need to adjust _inventorySlots list size here.
             InitializeInventory();
        }


        for (int i = 0; i < inventorySize; i++)
        {
            if (i < slotSaveDataList.Count)
            {
                InventorySlotSaveData savedSlot = slotSaveDataList[i];
                if (savedSlot != null && !string.IsNullOrEmpty(savedSlot.itemId) && savedSlot.quantity > 0)
                {
                    if (itemDatabaseDict.TryGetValue(savedSlot.itemId, out ItemData itemData))
                    {
                        _inventorySlots[i] = new InventorySlotData(itemData, savedSlot.quantity);
                    }
                    else
                    {
                        Debug.LogWarning($"Item with ID '{savedSlot.itemId}' not found in database. Slot {i} will be empty.");
                        _inventorySlots[i].Clear();
                    }
                }
                else
                {
                    _inventorySlots[i].Clear();
                }
            }
            else // If save data has fewer slots than current inventory size (e.g. size increased)
            {
                 _inventorySlots[i].Clear();
            }
        }
        OnInventoryChanged?.Invoke();
    }
}