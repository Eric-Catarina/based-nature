// Path: Assets/_ProjectName/Scripts/Inventory/InventorySlotSaveData.cs
[System.Serializable]
public class InventorySlotSaveData
{
    public string itemId;
    public int quantity;

    public InventorySlotSaveData() { }

    public InventorySlotSaveData(string id, int qty)
    {
        itemId = id;
        quantity = qty;
    }
}