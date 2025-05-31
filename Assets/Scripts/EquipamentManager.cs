// Path: Assets/Scripts/EquipamentManager.cs
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour // Corrigido para EquipmentManager
{
    public static EquipmentManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] public List<EquipmentSlotUI> equipmentSlotUIs = new List<EquipmentSlotUI>();
    [SerializeField] public InventoryManager inventoryManager;

    private Dictionary<EquipmentSlotType, ItemData> _equippedItems = new Dictionary<EquipmentSlotType, ItemData>();
    private Dictionary<EquipmentSlotType, EquipmentSlotUI> _uiSlotMapping = new Dictionary<EquipmentSlotType, EquipmentSlotUI>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // A inicialização é crítica. Se SaveSystem.LoadGame() for chamado antes deste Start,
        // esta chamada a InitializeEquipmentSlots() limpará os itens carregados.
        // A ordem de execução de scripts deve garantir que este Start rode antes do Start do SaveSystem.
        InitializeEquipmentSlots();
    }

    public void InitializeEquipmentSlots()
    {
        if (inventoryManager == null) Debug.LogError("[EquipmentManager] InventoryManager not assigned!");

        _equippedItems.Clear();
        _uiSlotMapping.Clear();

        foreach (EquipmentSlotUI uiSlot in equipmentSlotUIs)
        {
            if (uiSlot != null)
            {
                uiSlot.Initialize(this);
                if (_uiSlotMapping.ContainsKey(uiSlot.slotType))
                {
                     Debug.LogWarning($"[EquipmentManager] Duplicate EquipmentType {uiSlot.slotType} configured in UI slots.");
                }
                _uiSlotMapping[uiSlot.slotType] = uiSlot;
                _equippedItems[uiSlot.slotType] = null;
                uiSlot.ClearSlotDisplay();
            }
            else
            {
                Debug.LogError("[EquipmentManager] An EquipmentSlotUI in the list is null.");
            }
        }
    }

    public bool EquipItem(ItemData itemToEquip, int inventorySlotIndexToRemoveFrom)
    {
        if (itemToEquip == null || !itemToEquip.isEquippable || itemToEquip.equipmentSlotType == EquipmentSlotType.None)
        {
            return false;
        }

        EquipmentSlotType targetSlotType = itemToEquip.equipmentSlotType;

        if (!_uiSlotMapping.ContainsKey(targetSlotType))
        {
            Debug.LogWarning($"[EquipmentManager] No UI slot configured for equipment type: {targetSlotType}. Cannot equip {itemToEquip.itemName}.");
            return false;
        }

        ItemData previouslyEquippedItem = null;
        if (_equippedItems.TryGetValue(targetSlotType, out ItemData currentItemInSlot))
        {
            previouslyEquippedItem = currentItemInSlot;
        }

        _equippedItems[targetSlotType] = itemToEquip;
        if (_uiSlotMapping.TryGetValue(targetSlotType, out EquipmentSlotUI slotUI))
        {
            slotUI.DisplayItem(itemToEquip);
        }

        if (inventoryManager != null)
        {
            if (inventorySlotIndexToRemoveFrom != -1) // Se veio de um slot de inventário
            {
                inventoryManager.RemoveItemFromSlot(inventorySlotIndexToRemoveFrom, 1);
            }
            
            if (previouslyEquippedItem != null) // Se havia um item antes, retorna ao inventário
            {
                inventoryManager.AddItem(previouslyEquippedItem, 1);
            }
        }
        else
        {
             Debug.LogError("[EquipmentManager] InventoryManager not found to manage item transfers.");
        }
        return true;
    }

    public void UnequipItem(EquipmentSlotType slotTypeToUnequip)
    {
        if (!_equippedItems.TryGetValue(slotTypeToUnequip, out ItemData itemToUnequip) || itemToUnequip == null)
        {
            return;
        }

        _equippedItems[slotTypeToUnequip] = null;

        if (_uiSlotMapping.TryGetValue(slotTypeToUnequip, out EquipmentSlotUI slotUI))
        {
            slotUI.ClearSlotDisplay();
        }

        if (inventoryManager != null)
        {
            inventoryManager.AddItem(itemToUnequip, 1);
        }
        else
        {
            Debug.LogWarning($"[EquipmentManager] Could not return {itemToUnequip.itemName} to inventory: InventoryManager not assigned.");
        }
    }

    public ItemData GetEquippedItem(EquipmentSlotType type)
    {
        _equippedItems.TryGetValue(type, out ItemData item);
        return item;
    }

    public Dictionary<EquipmentSlotType, ItemData> GetAllEquippedItems()
    {
        return new Dictionary<EquipmentSlotType, ItemData>(_equippedItems); // Retorna uma cópia
    }
}