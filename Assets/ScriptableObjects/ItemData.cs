// Path: Assets/_ProjectName/Scripts/Items/ItemData.cs
using UnityEngine;

public enum ItemType
{
    Generic,
    Consumable,
    Equipment
}

public enum EquipmentSlotType
{
    None,
    Weapon,
    ArmorHead,
    ArmorChest,
    ArmorLegs
}

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string id = System.Guid.NewGuid().ToString(); // Unique ID
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public GameObject worldPrefab; // Prefab to instantiate in the world

    [Header("Stats")]
    public ItemType itemType = ItemType.Generic;
    public int maxStackSize = 1;
    public bool isStackable { get { return maxStackSize > 1; } }

    [Header("Equipment Info (If Applicable)")]
    public EquipmentSlotType equipmentSlotType = EquipmentSlotType.None;
    // Add other equipment-specific stats here, e.g., attackPower, defenseValue

    [Header("Consumable Info (If Applicable)")]
    public int healthRestoreAmount;
    // Add other consumable effects here

    public virtual void UseItem(PlayerStats playerStats)
    {
        Debug.Log($"Using {displayName}");
        if (itemType == ItemType.Consumable)
        {
            if (playerStats != null && healthRestoreAmount > 0)
            {
                // playerStats.RestoreHealth(healthRestoreAmount); // Example
                Debug.Log($"{displayName} restored {healthRestoreAmount} health.");
            }
        }
    }
}