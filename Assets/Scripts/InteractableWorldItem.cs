
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; 

[RequireComponent(typeof(Collider))]
public class InteractableWorldItem : MonoBehaviour
{
    [Header("Configuração do Item")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    [Tooltip("GameObject visual que indica que o item pode ser interagido (ex: um 'E' flutuante).")]
    [SerializeField] private GameObject interactionCue; 

    [Header("Outline")]
    [Tooltip("Material usado para aplicar o contorno (outline) no item.")]
    [SerializeField] private Material outlineMaterial; 

    [Header("Animação Wave e Rotate (Opcional)")] 
    [Tooltip("Controla se o item deve ter a animação de 'onda' e rotação.")]
    [SerializeField] private bool shouldWaveAndRotate = false; 

    [Tooltip("Altura máxima que o item subirá/descerá na animação wave.")]
    [SerializeField] private float waveHeight = 0.2f; 
    [Tooltip("Duração de um ciclo completo (subir + descer) da animação wave.")]
    [SerializeField] private float waveDuration = 2f; 

    [Tooltip("Velocidade da rotação em torno do eixo Y (em graus por segundo).")]
    [SerializeField] private float rotateSpeed = 45f; 
    [Tooltip("Amplitude do tilt lateral na animação (em graus).")]
    [SerializeField] private float tiltAmount = 5f; 
    [Tooltip("Duração de um ciclo completo (tilt para um lado + volta + tilt para o outro + volta) do tilt.")]
    [SerializeField] private float tiltDuration = 3f; 

    [SerializeField, Tooltip("Força do efeito de 'punch' aplicado ao outline quando ativado.")]
    private float outlinePunchStrength = 0.1f; 
    [SerializeField, Tooltip("Duração do efeito de 'punch' aplicado ao outline quando ativado.")]
    private float outlinePunchDuration = 0.2f; 

    [SerializeField] private float outlineWidth = 1.1f;

    
    private Renderer _itemRenderer; 
    private List<Material> _originalMaterials = new List<Material>(); 
    private bool _isOutlined = false; 

    
    private Tween _waveTween;
    private Tween _rotateTween; 
    private Tween _tiltTween;


    
    public ItemData GetItemData() => itemData; 
    public int GetQuantity() => quantity; 


    
    private void Awake()
    {
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        else Debug.LogError("InteractableWorldItem requires a Collider component.", this);

        
        if (interactionCue) interactionCue.SetActive(false);

        
        _itemRenderer = GetComponent<Renderer>();
        if (_itemRenderer == null)
        {
            _itemRenderer = GetComponentInChildren<Renderer>();
        }

        
        if (_itemRenderer != null)
        {
            _originalMaterials.AddRange(_itemRenderer.materials);
            
            for (int i = 0; i < _originalMaterials.Count; i++)
            {
                _originalMaterials[i] = new Material(_originalMaterials[i]);
            }
        }
        else
        {
            Debug.LogWarning("InteractableWorldItem could not find a Renderer component. Outline will not work.", this);
        }

        
        if (shouldWaveAndRotate)
        {
            StartWaveAndRotateAnimation();
        }
    }

    private void OnDestroy()
    {
        
        DOTween.Kill(this.transform); 
        DOTween.Kill(this); 
         
         foreach(Material mat in _originalMaterials)
         {
             Destroy(mat);
         }
    }


    
    public void ShowCue(bool show) 
    {
        if (interactionCue) interactionCue.SetActive(show);
    }

    
    
  public void ApplyOutline() 
    {
        if (_itemRenderer == null || outlineMaterial == null || _isOutlined) return;

        
        List<Material> currentMaterials = new List<Material>();
        currentMaterials.AddRange(_originalMaterials); 
        currentMaterials.Add(outlineMaterial); 
        _itemRenderer.materials = currentMaterials.ToArray(); 
        _isOutlined = true;
        

        
        transform.DOPunchScale(Vector3.one * outlinePunchStrength, outlinePunchDuration, 10, 1f); 
    }

    
    public void RemoveOutline() 
    {
        if (_itemRenderer == null || !_isOutlined) return;

        _itemRenderer.materials = _originalMaterials.ToArray(); 
        _isOutlined = false;
        

         
        transform.DOPunchScale(Vector3.one * -outlinePunchStrength, outlinePunchDuration, 10, 1f); 
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.RegisterInteractable(this); 
                
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
                
            }
        }
    }


    
        private void StartWaveAndRotateAnimation()
    {
        
        if (!shouldWaveAndRotate) return;

        
        DOTween.Kill(this.transform);

        
        
        _waveTween = transform.DOLocalMoveY(transform.position.y + waveHeight, waveDuration * 0.5f)
            .SetEase(Ease.InOutSine) 
            .SetLoops(-1, LoopType.Yoyo) 
            .SetRelative(true); 

        
        
        
        
        _rotateTween = transform.DORotate(new Vector3(0, 360, 0), rotateSpeed, RotateMode.FastBeyond360)
            .SetRelative(true) 
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);

        
        
        _tiltTween = DOTween.Sequence()
            .Append(transform.DOLocalRotate(new Vector3(tiltAmount, 0, 0), tiltDuration * 0.25f).SetEase(Ease.InOutSine).SetRelative(true)) 
            .Append(transform.DOLocalRotate(new Vector3(-2 * tiltAmount, 0, 0), tiltDuration * 0.5f).SetEase(Ease.InOutSine).SetRelative(true)) 
            .Append(transform.DOLocalRotate(new Vector3(tiltAmount, 0, 0), tiltDuration * 0.25f).SetEase(Ease.InOutSine).SetRelative(true)); 
            
            

        _tiltTween.SetLoops(-1, LoopType.Restart); 
         

        
        

        
        
    }

    private void StopWaveAndRotateAnimation()
    {
        
        DOTween.Kill(this.transform);
        _waveTween = null;
        _rotateTween = null;
        _tiltTween = null;
    }

    
    public void SetWaveAndRotateAnimationEnabled(bool enabled)
    {
        if (shouldWaveAndRotate == enabled) return; 

        shouldWaveAndRotate = enabled;

        if (shouldWaveAndRotate)
        {
            StartWaveAndRotateAnimation();
        }
        else
        {
            StopWaveAndRotateAnimation();
        }
    }

    
    
    
}