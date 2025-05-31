
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> allGameItems;
    
    public Dictionary<string, ItemData> _itemDictionary { get; private set; }
    private bool _isInitialized = false;

    public void InitializeDatabase()
    {
        if (_isInitialized && _itemDictionary != null) return;

        _itemDictionary = new Dictionary<string, ItemData>();
        if (allGameItems == null) allGameItems = new List<ItemData>();

        foreach (var item in allGameItems)
        {
            if (item == null)
            {

                continue;
            }
            if (string.IsNullOrEmpty(item.id)) 
            {

                continue;
            }
            if (!_itemDictionary.ContainsKey(item.id))
            {
                _itemDictionary.Add(item.id, item);
            }
            else
            {

            }
        }
        _isInitialized = true;
    }

    public void RefreshDatabase()
    {
        _isInitialized = false;
        InitializeDatabase();
    }

    public ItemData GetItemByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (!_isInitialized || _itemDictionary == null) InitializeDatabase();
        
        _itemDictionary.TryGetValue(id, out ItemData item);
        return item; 
    }

    #if UNITY_EDITOR
    [ContextMenu("Populate Database From Project Assets")]
    private void EditorPopulateDatabase()
    {
        allGameItems = AssetDatabase.FindAssets($"t:{nameof(ItemData)}")
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .Select(path => AssetDatabase.LoadAssetAtPath<ItemData>(path))
            .Where(item => item != null)
            .ToList();
        
        RefreshDatabase(); 
        EditorUtility.SetDirty(this);

    }
    #endif
}