// Path: Assets/_ProjectName/Scripts/UI/InventorySlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("References")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject quantityBackground;
    [SerializeField] private GameObject selectionHighlight;

    [Header("Settings")]
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private float jiggleStrength = 10f;
    [SerializeField] private float jiggleDuration = 0.3f;
    [SerializeField] private int jiggleVibrato = 10;
    [SerializeField] private Ease jiggleEase = Ease.OutElastic;




    private InventorySlotData _slotData;
    private int _slotIndex;
    private InventoryUI _inventoryUIController;
    private Vector3 _originalScale;
    private Color _originalColor;

    private Sequence _jiggleSequence;
    private InventorySlotData _currentSlotData;

    public void Initialize(InventoryUI inventoryUI, int index)
    {
        _inventoryUIController = inventoryUI;
        _slotIndex = index;
        if (itemIconImage) itemIconImage.enabled = false;
        if (quantityText) quantityText.enabled = false;
        if (quantityBackground) quantityBackground.SetActive(false);
        _originalColor = itemIconImage ? itemIconImage.color : Color.white;
        _originalScale = transform.localScale;
    }

    public void UpdateSlotDisplay(InventorySlotData slotData)
    {
        _currentSlotData = slotData;
        if (slotData != null && !slotData.IsEmpty())
        {
            if (itemIconImage)
            {
                itemIconImage.sprite = slotData.itemData.icon;
                itemIconImage.enabled = true;
            }
            if (quantityText)
            {
                bool showQuantity = slotData.quantity > 1 && slotData.itemData.isStackable;
                quantityText.text = showQuantity ? slotData.quantity.ToString() : "";
                quantityText.enabled = showQuantity;
                if (quantityBackground) quantityBackground.SetActive(showQuantity);
            }
        }
        else
        {
            if (itemIconImage)
            {
                itemIconImage.sprite = null;
                itemIconImage.enabled = false;
            }
            if (quantityText)
            {
                quantityText.text = "";
                quantityText.enabled = false;
                if (quantityBackground) quantityBackground.SetActive(false);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_inventoryUIController != null && _currentSlotData != null && !_currentSlotData.IsEmpty())
        {
            AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.hoverSound); // Play hover sound when showing tooltip

            _inventoryUIController.ShowTooltip(_currentSlotData.itemData, GetComponent<RectTransform>());

            UITweenAnimations.HoverScale(transform as RectTransform, _originalScale.x * 1.1f);
            if (itemIconImage) UITweenAnimations.HoverColor(itemIconImage, hoverColor);
            if (selectionHighlight) selectionHighlight.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_inventoryUIController != null)
        {
            _inventoryUIController.HideTooltip();
            UITweenAnimations.UnhoverScale(transform as RectTransform, _originalScale.x);
            if (itemIconImage) UITweenAnimations.UnhoverColor(itemIconImage, _originalColor); // Reseta a cor original
            if (selectionHighlight) selectionHighlight.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (_inventoryUIController != null && _currentSlotData != null && !_currentSlotData.IsEmpty())
            {
                _inventoryUIController.RequestUseItem(_slotIndex);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentSlotData == null || _currentSlotData.IsEmpty() || _inventoryUIController == null)
        {
            eventData.pointerDrag = null;
            return;
        }

        _inventoryUIController.OnDragStarted(_slotIndex);
        if (itemIconImage) itemIconImage.color = new Color(1, 1, 1, 0.5f);
        if (quantityText) quantityText.enabled = false;
        if (quantityBackground) quantityBackground.SetActive(false);
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.onDragSound); // Play hover sound when showing tooltip
        UITweenAnimations.HoverScale(itemIconImage.rectTransform); // Optional: Add a hover effect on drag start
        StartJiggle(); // Inicia o jiggle quando o item é arrastado
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_inventoryUIController != null && _inventoryUIController.draggedItemImage.gameObject.activeSelf)
        {
            _inventoryUIController.draggedItemImage.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (itemIconImage) itemIconImage.color = Color.white;

        if (_inventoryUIController != null)
        {
            _inventoryUIController.OnDragEnded(_slotIndex, eventData.pointerCurrentRaycast.gameObject);
        }
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.onDropSound); // Play hover sound when showing tooltip

        UpdateSlotDisplay(_currentSlotData); // Ensure the original slot visuals are restored or updated
        StopJiggle(); // Para o jiggle quando o item é solto
    }

    public void OnDrop(PointerEventData eventData)
    {
                 StopJiggle();
        // The drop logic is handled by OnEndDrag of the item being dragged,
        // using eventData.pointerCurrentRaycast.gameObject to determine the drop target.
    }
    private void StartJiggle()
    {
        if (_jiggleSequence != null && _jiggleSequence.IsActive()) _jiggleSequence.Kill();

        _jiggleSequence = DOTween.Sequence();
        _jiggleSequence.Append(( _inventoryUIController.draggedItemImage.transform as RectTransform).DOShakeRotation(jiggleDuration, new Vector3(0,0,jiggleStrength), jiggleVibrato, 90, false).SetEase(jiggleEase))
                       .SetLoops(-1, LoopType.Yoyo)
                       .SetUpdate(true);
    }

    private void StopJiggle()
    {
        if (_jiggleSequence != null && _jiggleSequence.IsActive())
        {
            _jiggleSequence.Kill(); // Para a animação
            (_inventoryUIController.draggedItemImage.transform as RectTransform).DORotate(Vector3.zero, 0.1f).SetUpdate(true); // Garante que volta para a rotação inicial
        }
    }
}