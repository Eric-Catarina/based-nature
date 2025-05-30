// Path: Assets/_ProjectName/Scripts/Inventory/ItemData.cs
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ItemType { Generic, Consumable, Equipment, QuestItem }
public enum EquipmentSlotType { None, Weapon, Armor, Accessory }

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Core Identification")]
    public string id;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType = ItemType.Generic;

    [Header("Stacking & Quantity")]
    public bool isStackable = true;
    public int maxStackSize = 99;

    [Header("Functional Properties")]
    public bool isUsable = false;
    public bool isEquippable = false;
    public EquipmentSlotType equipmentSlotType = EquipmentSlotType.None;
    
    // public GameObject itemPrefab; // Se o item pode ser dropado no mundo

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(itemName))
        {
            itemName = name;
        }

        if (string.IsNullOrEmpty(id))
        {
            id = GUID.Generate().ToString();
            EditorUtility.SetDirty(this);
        }
    }

    [ContextMenu("Force Regenerate ID")]
    private void RegenerateId()
    {
        id = GUID.Generate().ToString();
        EditorUtility.SetDirty(this);
        Debug.Log($"Regenerated ID for {name}: {id}");
    }
    #endif
}