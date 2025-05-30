// Path: Assets/_ProjectName/Scripts/EquipmentSlotUI.cs
using UnityEngine;
using UnityEngine.UI; // Required for Image

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    public EquipmentSlotType slotType; // Changed to EquipmentSlotType
    private EquipmentManager _equipmentManager;

    public void Initialize(EquipmentManager equipmentManager)
    {
        _equipmentManager = equipmentManager;
    }

    public void DisplayItem(ItemData item)
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }
    }

    public void ClearSlotDisplay()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
    }
}