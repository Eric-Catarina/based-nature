
using UnityEngine;
using System.Collections.Generic;
using System.IO; 
using System;   






[System.Serializable]
public class EquippedItemSaveData
{
    public EquipmentSlotType slotType;
    public string itemId;

    public EquippedItemSaveData() { } 
    public EquippedItemSaveData(EquipmentSlotType type, string id)
    {
        slotType = type;
        itemId = id;
    }
}

[System.Serializable]
public class GameSaveData
{
    public List<InventorySlotSaveData> inventoryData;
    public List<EquippedItemSaveData> equippedItemsData;

    public GameSaveData()
    {
        inventoryData = new List<InventorySlotSaveData>();
        equippedItemsData = new List<EquippedItemSaveData>();
    }
}

public class SaveSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private EquipmentManager equipmentManager;

    private const string SaveFileName = "save.json";
    
    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private void Awake()
    {
        if (itemDatabase == null) Debug.LogError("[SaveSystem] ItemDatabase not assigned!");
        else itemDatabase.InitializeDatabase();
        
        if (inventoryManager == null) Debug.LogError("[SaveSystem] InventoryManager not assigned!");
        if (equipmentManager == null) Debug.LogError("[SaveSystem] EquipmentManager not assigned!");
    }
    
    public void TriggerLoadGame() => LoadGame();
    public void TriggerSaveGame() => SaveGame();
    
    private void Start()
    {
        
        
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        if (inventoryManager == null || itemDatabase == null || equipmentManager == null)
        {

            return;
        }

        GameSaveData gameData = new GameSaveData
        {
            inventoryData = inventoryManager.GetSaveData()
        };

        gameData.equippedItemsData = new List<EquippedItemSaveData>();
        Dictionary<EquipmentSlotType, ItemData> allEquipped = equipmentManager.GetAllEquippedItems();
        foreach (var kvp in allEquipped)
        {
            if (kvp.Value != null)
            {
                gameData.equippedItemsData.Add(new EquippedItemSaveData(kvp.Key, kvp.Value.id));
            }
        }

        string json = JsonUtility.ToJson(gameData, true);
        string savePath = GetSavePath();

        try
        {
            File.WriteAllText(savePath, json);

        }
        catch (Exception e)
        {

        }
    }

    public void LoadGame()
    {
        if (inventoryManager == null || itemDatabase == null || equipmentManager == null)
        {

            return;
        }
        
        itemDatabase.RefreshDatabase(); 
        
        
        
        equipmentManager.InitializeEquipmentSlots(); 

        string json = "";
        string savePath = GetSavePath();
        bool fileExists = false;

        try
        {
            if (File.Exists(savePath))
            {
                json = File.ReadAllText(savePath);
                fileExists = true;
            }
        }
        catch (Exception e)
        {

        }
        
        if (fileExists && !string.IsNullOrEmpty(json))
        {
            GameSaveData gameData = JsonUtility.FromJson<GameSaveData>(json);
            if (gameData != null)
            {
                
                inventoryManager.LoadSaveData(gameData.inventoryData ?? new List<InventorySlotSaveData>(), itemDatabase._itemDictionary);

                
                if (gameData.equippedItemsData != null)
                {
                    foreach (var equippedItemSave in gameData.equippedItemsData)
                    {
                        ItemData itemToEquip = itemDatabase.GetItemByID(equippedItemSave.itemId);
                        if (itemToEquip != null && itemToEquip.isEquippable && itemToEquip.equipmentSlotType == equippedItemSave.slotType)
                        {
                            bool equipped = equipmentManager.EquipItem(itemToEquip, -1); 
                            if (equipped)
                            {
                                
                                
                            }
                        }
                        else
                        {

                        }
                    }
                }

            }
            else
            {

                 inventoryManager.LoadSaveData(new List<InventorySlotSaveData>(), itemDatabase._itemDictionary);
            }
        }
        else
        {

            inventoryManager.LoadSaveData(new List<InventorySlotSaveData>(), itemDatabase._itemDictionary);
        }
    }
    
    [ContextMenu("Delete Save File")] 
    public void DeleteSaveFile()
    {
        string savePath = GetSavePath();
        if (File.Exists(savePath))
        {
            File.Delete(savePath);

        }
        else
        {

        }
    }
}