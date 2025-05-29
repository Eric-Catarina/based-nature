// Path: Assets/_ProjectName/Scripts/Core/SaveSystem.cs
using UnityEngine;
using System.Collections.Generic;
using System.IO; // For more advanced saving later

[System.Serializable]
public class GameSaveData // Wrapper for all game data you want to save
{
    public List<InventorySlotSaveData> inventoryData;
    // Add other data here: player position, quest states, etc.

    public GameSaveData()
    {
        inventoryData = new List<InventorySlotSaveData>();
    }
}

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ItemDatabase itemDatabase; // Assign your ItemDatabase asset here

    private const string SaveFileName = "gameSave.json"; // For PlayerPrefs, this is the key
    
    // For file-based saving (more robust than PlayerPrefs for larger data)
    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }


    private void Awake()
    {
        // Ensure ItemDatabase is initialized
        if (itemDatabase != null)
        {
            itemDatabase.InitializeDatabase(); // Important: ensure dictionary is ready
        }
        else
        {
            Debug.LogError("ItemDatabase not assigned to SaveSystem!");
        }
    }

    // Public method to be called by a "Load Game" button or on game start
    public void TriggerLoadGame()
    {
        LoadGame();
    }

    // Public method to be called by a "Save Game" button or on significant events
    public void TriggerSaveGame()
    {
        SaveGame();
    }
    
    private void Start()
    {
        // Auto-load on start for this prototype
        // In a full game, you might load from a menu or after a loading screen
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        // Auto-save on quit for this prototype
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
            // Populate other data for gameData here
        };

        string json = JsonUtility.ToJson(gameData, true); // true for pretty print
        
        // Using PlayerPrefs (simple, but limited size and not ideal for complex data)
        PlayerPrefs.SetString(SaveFileName, json);
        PlayerPrefs.Save(); 
        Debug.Log("Game Saved to PlayerPrefs. Key: " + SaveFileName);

        // --- OR Using File System (better for larger data) ---
        // File.WriteAllText(GetSavePath(), json);
        // Debug.Log("Game Saved to: " + GetSavePath());
    }

    public void LoadGame()
    {
        if (inventoryManager == null || itemDatabase == null)
        {
            Debug.LogError("Cannot load: InventoryManager or ItemDatabase not set up.");
            return;
        }
        
        itemDatabase.RefreshDatabase(); // Ensure DB is up-to-date before loading items

        string json = "";
        bool loadedSuccessfully = false;

        // Using PlayerPrefs
        if (PlayerPrefs.HasKey(SaveFileName))
        {
            json = PlayerPrefs.GetString(SaveFileName);
            loadedSuccessfully = true;
            Debug.Log("Game Loaded from PlayerPrefs. Key: " + SaveFileName);
        }
        
        // --- OR Using File System ---
        // string path = GetSavePath();
        // if (File.Exists(path))
        // {
        //     json = File.ReadAllText(path);
        //     loadedSuccessfully = true;
        //     Debug.Log("Game Loaded from: " + path);
        // }

        if (loadedSuccessfully && !string.IsNullOrEmpty(json))
        {
            GameSaveData gameData = JsonUtility.FromJson<GameSaveData>(json);
            if (gameData != null)
            {
                inventoryManager.LoadSaveData(gameData.inventoryData, itemDatabase._itemDictionary); // Pass the actual dictionary
                // Load other game data from gameData here
                Debug.Log("Inventory data applied.");
            }
            else
            {
                 Debug.LogWarning("Failed to parse save data. Starting fresh or with defaults.");
                 inventoryManager.LoadSaveData(null, null); // Clears inventory
            }
        }
        else
        {
            Debug.Log("No save file found or data is empty. Starting fresh or with defaults.");
            // Initialize with default state if no save data (InventoryManager.InitializeInventory already does this)
            inventoryManager.LoadSaveData(null, null); // Clears inventory / loads defaults
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