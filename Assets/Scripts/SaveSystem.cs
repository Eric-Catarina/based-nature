// Path: Assets/Scripts/SaveSystem.cs
using UnityEngine;
using System.Collections.Generic;
using System.IO; // Necessário para File I/O
using System;   // Necessário para System.Action

// Definições de Enum (coloque em um arquivo Enums.cs ou similar se preferir)
// public enum EquipmentSlotType { None, Head, Torso, Legs, Feet, Weapon, Shield, Accessory }
// public enum ItemType { Generic, Consumable, Equipment, Material, Key }


[System.Serializable]
public class EquippedItemSaveData
{
    public EquipmentSlotType slotType;
    public string itemId;

    public EquippedItemSaveData() { } // Construtor padrão para desserialização
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
        // Certifique-se de que a ordem de execução está correta para que LoadGame()
        // funcione após os managers terem inicializado.
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
            Debug.LogError("[SaveSystem] Cannot save: Required managers are not set up.");
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
            Debug.Log($"[SaveSystem] Game Saved to: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save game to {savePath}. Error: {e.Message}");
        }
    }

    public void LoadGame()
    {
        if (inventoryManager == null || itemDatabase == null || equipmentManager == null)
        {
            Debug.LogError("[SaveSystem] Cannot load: Required managers are not set up.");
            return;
        }
        
        itemDatabase.RefreshDatabase(); // Garante que o ItemDatabase está atualizado
        
        // Crucial: Limpa os slots de equipamento ANTES de carregar os itens equipados.
        // Isso deve acontecer depois que EquipmentManager.Start() já rodou (devido à ordem de execução).
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
            Debug.LogError($"[SaveSystem] Failed to read save file from {savePath}. Error: {e.Message}");
        }
        
        if (fileExists && !string.IsNullOrEmpty(json))
        {
            GameSaveData gameData = JsonUtility.FromJson<GameSaveData>(json);
            if (gameData != null)
            {
                // 1. Carregar Inventário
                inventoryManager.LoadSaveData(gameData.inventoryData ?? new List<InventorySlotSaveData>(), itemDatabase._itemDictionary);

                // 2. Carregar Itens Equipados
                if (gameData.equippedItemsData != null)
                {
                    foreach (var equippedItemSave in gameData.equippedItemsData)
                    {
                        ItemData itemToEquip = itemDatabase.GetItemByID(equippedItemSave.itemId);
                        if (itemToEquip != null && itemToEquip.isEquippable && itemToEquip.equipmentSlotType == equippedItemSave.slotType)
                        {
                            bool equipped = equipmentManager.EquipItem(itemToEquip, -1); // -1 indica que não veio do inventário ao vivo
                            if (equipped)
                            {
                                // Remove o item do inventário se ele também foi carregado lá (para evitar duplicatas)
                                // inventoryManager.RemoveSpecificItem(itemToEquip, 1); 
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[SaveSystem] Could not load equipped item (ID: {equippedItemSave.itemId}, Slot: {equippedItemSave.slotType}). Item not found, not equippable, or slot mismatch.");
                        }
                    }
                }
                Debug.Log("[SaveSystem] Game data loaded and applied.");
            }
            else
            {
                 Debug.LogWarning("[SaveSystem] Failed to parse save data from JSON. Starting fresh.");
                 inventoryManager.LoadSaveData(new List<InventorySlotSaveData>(), itemDatabase._itemDictionary);
            }
        }
        else
        {
            Debug.Log("[SaveSystem] No save file found or data is empty. Starting fresh.");
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
            Debug.Log($"[SaveSystem] Save file deleted: {savePath}");
        }
        else
        {
            Debug.Log($"[SaveSystem] Save file not found at: {savePath}. Nothing to delete.");
        }
    }
}