
[System.Serializable]
public class InventorySlotData
{
    public ItemData itemData;
    public int quantity;

    public InventorySlotData(ItemData item = null, int qty = 0)
    {
        itemData = item;
        quantity = qty;
    }

    public void Clear()
    {
        itemData = null;
        quantity = 0;
    }

    public bool IsEmpty()
    {
        return itemData == null || quantity <= 0;
    }

    public void AddQuantity(int amount)
    {
        quantity += amount;
    }

    public void SetQuantity(int amount)
    {
        quantity = amount;
    }
}