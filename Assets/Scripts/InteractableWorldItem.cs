// Path: Assets/_ProjectName/Scripts/Items/InteractableWorldItem.cs
using UnityEngine;
using System.Collections.Generic; // Para usar List<Material>

[RequireComponent(typeof(Collider))]
public class InteractableWorldItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    [SerializeField] private GameObject interactionCue;
    [SerializeField] private Material outlineMaterial; // Atribua o seu material de outline aqui

    private Renderer _itemRenderer; // Cache do Renderer do item
    private List<Material> _originalMaterials = new List<Material>();
    private bool _isOutlined = false;

    public ItemData GetItemData() => itemData;
    public int GetQuantity() => quantity;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        else Debug.LogError("InteractableWorldItem requires a Collider component.", this);

        if (interactionCue) interactionCue.SetActive(false);

        // Tenta pegar o Renderer no próprio objeto ou nos filhos (para modelos mais complexos)
        _itemRenderer = GetComponent<Renderer>();
        if (_itemRenderer == null)
        {
            _itemRenderer = GetComponentInChildren<Renderer>();
        }

        if (_itemRenderer != null)
        {
            // Salva os materiais originais
            _originalMaterials.AddRange(_itemRenderer.materials);
        }
        else
        {
            Debug.LogWarning("InteractableWorldItem could not find a Renderer component. Outline will not work.", this);
        }
    }

    public void ShowCue(bool show)
    {
        if (interactionCue) interactionCue.SetActive(show);
    }

    // Este método será chamado pelo PlayerInteraction quando o jogador estiver perto
    public void ApplyOutline()
    {
        if (_itemRenderer == null || outlineMaterial == null || _isOutlined) return;

        List<Material> currentMaterials = new List<Material>(_itemRenderer.materials);
        currentMaterials.Add(outlineMaterial);
        _itemRenderer.materials = currentMaterials.ToArray();
        _isOutlined = true;
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.hoverSound); // Toca o som de hover, se necessário
    }

    // Este método será chamado pelo PlayerInteraction quando o jogador se afastar
    public void RemoveOutline()
    {
        if (_itemRenderer == null || !_isOutlined) return;

        _itemRenderer.materials = _originalMaterials.ToArray(); // Restaura os materiais originais
        _isOutlined = false;
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.hoverSound); // Toca o som de hover, se necessário

    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.RegisterInteractable(this);
                // Não mostra o cue aqui, PlayerInteraction gerencia isso com base no _closestInteractable
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.UnregisterInteractable(this);
                // RemoveOutline(); // PlayerInteraction deve chamar RemoveOutline se este não for mais o _closest
            }
        }
    }
}