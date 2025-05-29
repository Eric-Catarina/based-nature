// Path: Assets/_ProjectName/Scripts/Items/ItemDatabase.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems;

    public Dictionary<string, ItemData> _itemDictionary;

    public void OnEnable() // Called when the ScriptableObject is loaded
    {
        InitializeDatabase();
    }

    public void OnValidate() // Called in the editor when the script is loaded or a value is changed in the Inspector.
    {
        // This helps ensure the dictionary is updated if items are changed in the editor list
        // Can be heavy if list is huge, use with caution or provide a manual "Rebuild Database" button
        // InitializeDatabase(); // Commented out for performance, consider a button
    }
    
    [ContextMenu("Rebuild Database Dictionary")]
    public void InitializeDatabase()
    {
        _itemDictionary = new Dictionary<string, ItemData>();
        if (allItems == null) allItems = new List<ItemData>();

        foreach (var item in allItems)
        {
            if (item == null) continue;
            if (string.IsNullOrEmpty(item.id))
            {
                Debug.LogError($"Item {item.name} has no ID and cannot be added to the database.");
                continue;
            }
            if (!_itemDictionary.ContainsKey(item.id))
            {
                _itemDictionary.Add(item.id, item);
            }
            else
            {
                Debug.LogWarning($"Duplicate Item ID '{item.id}' found for item '{item.name}'. Original: '{_itemDictionary[item.id].name}'. Skipping duplicate.");
            }
        }
    }

    public ItemData GetItemByID(string id)
    {
        if (_itemDictionary == null) InitializeDatabase(); // Ensure initialized
        _itemDictionary.TryGetValue(id, out ItemData item);
        return item;
    }

    // Optional: Call this if you add/remove items from the `allItems` list at runtime or need to force a refresh
    public void RefreshDatabase()
    {
        InitializeDatabase();
    }
}