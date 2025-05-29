// Path: Assets/_ProjectName/Scripts/Inventory/InventorySlotData.cs
using System;

[Serializable]
public class InventorySlotData
{
    public ItemData itemData;
    public int quantity;

    public InventorySlotData(ItemData item, int amount)
    {
        itemData = item;
        quantity = amount;
    }

    public void Clear()
    {
        itemData = null;
        quantity = 0;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    public void RemoveQuantity(int amount)
    {
        quantity -= amount;
        if (quantity <= 0)
        {
            Clear();
        }
    }
}