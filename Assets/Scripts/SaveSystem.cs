// Path: Assets/_ProjectName/Scripts/Core/SaveSystem.cs
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class GameSaveData
{
    public List<InventorySlotSaveData> inventoryData;

    public GameSaveData()
    {
        inventoryData = new List<InventorySlotSaveData>();
    }
}

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ItemDatabase itemDatabase;

    private const string SaveFileName = "gameSave.json";
    
    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private void Awake()
    {
        if (itemDatabase != null)
        {
            itemDatabase.InitializeDatabase();
        }
        else
        {
            Debug.LogError("ItemDatabase not assigned to SaveSystem!");
        }
    }
    
    public void TriggerLoadGame()
    {
        LoadGame();
    }
    
    public void TriggerSaveGame()
    {
        SaveGame();
    }
    
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
        if (inventoryManager == null || itemDatabase == null)
        {
            Debug.LogError("Cannot save: InventoryManager or ItemDatabase not set up.");
            return;
        }

        GameSaveData gameData = new GameSaveData
        {
            inventoryData = inventoryManager.GetSaveData()
        };

        string json = JsonUtility.ToJson(gameData, true);
        
        PlayerPrefs.SetString(SaveFileName, json);
        PlayerPrefs.Save(); 
        Debug.Log("Game Saved to PlayerPrefs. Key: " + SaveFileName);
    }

    public void LoadGame()
    {
        if (inventoryManager == null || itemDatabase == null)
        {
            Debug.LogError("Cannot load: InventoryManager or ItemDatabase not set up.");
            return;
        }
        
        itemDatabase.RefreshDatabase();

        string json = "";
        bool loadedSuccessfully = false;

        if (PlayerPrefs.HasKey(SaveFileName))
        {
            json = PlayerPrefs.GetString(SaveFileName);
            loadedSuccessfully = true;
            Debug.Log("Game Loaded from PlayerPrefs. Key: " + SaveFileName);
        }
        
        if (loadedSuccessfully && !string.IsNullOrEmpty(json))
        {
            GameSaveData gameData = JsonUtility.FromJson<GameSaveData>(json);
            if (gameData != null && gameData.inventoryData != null)
            {
                inventoryManager.LoadSaveData(gameData.inventoryData, itemDatabase._itemDictionary);
                Debug.Log("Inventory data applied.");
            }
            else
            {
                 Debug.LogWarning("Failed to parse save data or inventory data is null. Starting fresh or with defaults.");
                 inventoryManager.LoadSaveData(new List<InventorySlotSaveData>(), itemDatabase._itemDictionary);
            }
        }
        else
        {
            Debug.Log("No save file found or data is empty. Starting fresh or with defaults.");
            inventoryManager.LoadSaveData(new List<InventorySlotSaveData>(), itemDatabase._itemDictionary);
        }
    }
    
    [ContextMenu("Clear Save Data (PlayerPrefs)")]
    public void ClearPlayerPrefsSave()
    {
        PlayerPrefs.DeleteKey(SaveFileName);
        PlayerPrefs.Save();
        Debug.Log($"PlayerPrefs save data cleared for key: {SaveFileName}");
    }
}