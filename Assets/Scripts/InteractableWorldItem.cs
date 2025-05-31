// Path: Assets/_ProjectName/Scripts/Items/InteractableWorldItem.cs
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening; // Importa a biblioteca DOTween

[RequireComponent(typeof(Collider))]
public class InteractableWorldItem : MonoBehaviour
{
    [Header("Configuração do Item")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int quantity = 1;
    [Tooltip("GameObject visual que indica que o item pode ser interagido (ex: um 'E' flutuante).")]
    [SerializeField] private GameObject interactionCue; // Objeto de feedback visual (ex: um 'E')

    [Header("Outline")]
    [Tooltip("Material usado para aplicar o contorno (outline) no item.")]
    [SerializeField] private Material outlineMaterial; // Atribua o seu material de outline aqui

    [Header("Animação Wave e Rotate (Opcional)")] // Novo Header para as configurações da animação
    [Tooltip("Controla se o item deve ter a animação de 'onda' e rotação.")]
    [SerializeField] private bool shouldWaveAndRotate = false; // Nova flag para ativar/desativar a animação

    [Tooltip("Altura máxima que o item subirá/descerá na animação wave.")]
    [SerializeField] private float waveHeight = 0.2f; // Altura do movimento para cima e para baixo
    [Tooltip("Duração de um ciclo completo (subir + descer) da animação wave.")]
    [SerializeField] private float waveDuration = 2f; // Duração do ciclo wave

    [Tooltip("Velocidade da rotação em torno do eixo Y (em graus por segundo).")]
    [SerializeField] private float rotateSpeed = 45f; // Velocidade de rotação em graus/segundo
    [Tooltip("Amplitude do tilt lateral na animação (em graus).")]
    [SerializeField] private float tiltAmount = 5f; // Quantidade de tilt lateral (pitch)
    [Tooltip("Duração de um ciclo completo (tilt para um lado + volta + tilt para o outro + volta) do tilt.")]
    [SerializeField] private float tiltDuration = 3f; // Duração do ciclo de tilt

    [SerializeField, Tooltip("Força do efeito de 'punch' aplicado ao outline quando ativado.")]
    private float outlinePunchStrength = 0.1f; // Força do efeito de punch
    [SerializeField, Tooltip("Duração do efeito de 'punch' aplicado ao outline quando ativado.")]
    private float outlinePunchDuration = 0.2f; // Duração do punch

    [SerializeField] private float outlineWidth = 1.1f;

    // --- Componentes e Estado ---
    private Renderer _itemRenderer; // Cache do Renderer do item (para outline)
    private List<Material> _originalMaterials = new List<Material>(); // Materiais originais do item
    private bool _isOutlined = false; // Flag para saber se o outline está aplicado

    // Tweens para as animações (mantemos referências para controlar)
    private Tween _waveTween;
    private Tween _rotateTween; // Usaremos DOTween.To para rotação contínua
    private Tween _tiltTween;


    // --- Propriedades ---
    public ItemData GetItemData() => itemData; // Permanece público para PlayerInteraction/Inventory
    public int GetQuantity() => quantity; // Permanece público


    // --- Inicialização ---
    private void Awake()
    {
        // Configura o Collider como Trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        else Debug.LogError("InteractableWorldItem requires a Collider component.", this);

        // Garante que o cue de interação esteja desativado no início
        if (interactionCue) interactionCue.SetActive(false);

        // Tenta pegar o Renderer no próprio objeto ou nos filhos (para modelos mais complexos)
        _itemRenderer = GetComponent<Renderer>();
        if (_itemRenderer == null)
        {
            _itemRenderer = GetComponentInChildren<Renderer>();
        }

        // Salva os materiais originais para o outline
        if (_itemRenderer != null)
        {
            _originalMaterials.AddRange(_itemRenderer.materials);
            // Clona os materiais originais para evitar modificar assets diretamente
            for (int i = 0; i < _originalMaterials.Count; i++)
            {
                _originalMaterials[i] = new Material(_originalMaterials[i]);
            }
        }
        else
        {
            Debug.LogWarning("InteractableWorldItem could not find a Renderer component. Outline will not work.", this);
        }

        // Inicia as animações DOTween se shouldWaveAndRotate for true
        if (shouldWaveAndRotate)
        {
            StartWaveAndRotateAnimation();
        }
    }

    private void OnDestroy()
    {
        // Garante que os tweens DOTween associados a este objeto sejam mortos ao destruir
        DOTween.Kill(this.transform); // Mata tweens no transform deste item
        DOTween.Kill(this); // Mata tweens com este script como ID, se usar (menos comum para animação de transform)
         // Opcional: Destruir os materiais clonados se foram criados
         foreach(Material mat in _originalMaterials)
         {
             Destroy(mat);
         }
    }


    // --- Gerenciamento do Cue de Interação (Visual Flutuante) ---
    public void ShowCue(bool show) // Permanece público
    {
        if (interactionCue) interactionCue.SetActive(show);
    }

    // --- Gerenciamento do Outline ---
    // Chamado pelo PlayerInteraction quando o jogador está perto
  public void ApplyOutline() // Permanece público
    {
        if (_itemRenderer == null || outlineMaterial == null || _isOutlined) return;

        // Cria uma nova lista de materiais combinando os originais e o outline
        List<Material> currentMaterials = new List<Material>();
        currentMaterials.AddRange(_originalMaterials); // Adiciona os originais clonados
        currentMaterials.Add(outlineMaterial); // Adiciona o material de outline
        _itemRenderer.materials = currentMaterials.ToArray(); // Aplica os materiais
        _isOutlined = true;
        // AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.hoverSound); // Toca o som, se necessário

        // Apply DOTween Punch Animation
        transform.DOPunchScale(Vector3.one * outlinePunchStrength, outlinePunchDuration, 10, 1f); // Punch
    }

    // Chamado pelo PlayerInteraction quando o jogador se afastar
    public void RemoveOutline() // Permanece público
    {
        if (_itemRenderer == null || !_isOutlined) return;

        _itemRenderer.materials = _originalMaterials.ToArray(); // Restaura os materiais originais clonados
        _isOutlined = false;
        // AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.hoverSound); // Toca o som, se necessário

         // Apply DOTween Punch Animation
        transform.DOPunchScale(Vector3.one * -outlinePunchStrength, outlinePunchDuration, 10, 1f); // Punch para o outro lado
    }

    // --- Gerenciamento do Trigger (Comunicação com PlayerInteraction) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Use a tag correta do player
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.RegisterInteractable(this); // Passa a referência deste script
                // PlayerInteraction gerencia a aplicação do outline e cue
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Use a tag correta do player
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction != null)
            {
                playerInteraction.UnregisterInteractable(this); // Passa a referência deste script
                // PlayerInteraction gerencia a remoção do outline e cue
            }
        }
    }


    // --- Animação Wave e Rotate (Controlada por shouldWaveAndRotate) ---
        private void StartWaveAndRotateAnimation()
    {
        // Garante que o objeto tenha sido inicializado e que shouldWaveAndRotate é true
        if (!shouldWaveAndRotate) return;

        // Stop e mata quaisquer animações DOTween existentes no transform deste objeto
        DOTween.Kill(this.transform);

        // 1. Animação Wave (Move para cima e para baixo)
        // Move para cima (relativo à posição local atual)
        _waveTween = transform.DOLocalMoveY(transform.position.y + waveHeight, waveDuration * 0.5f)
            .SetEase(Ease.InOutSine) // Facilidade suave para subir e descer
            .SetLoops(-1, LoopType.Yoyo) // Loop infinito, indo e voltando
            .SetRelative(true); // Move relativo à posição inicial

        // 2. Animação Rotate (Rotação contínua em Y)
        // Ajuste: Agora a rotação é feita em torno do eixo Y LOCAL, e não em ângulos absolutos.
        // Isso evita o problema de rotação "travada" em certas posições.
        // A rotação agora é sobre o EULER angles, que tem mais controle
        _rotateTween = transform.DORotate(new Vector3(0, 360, 0), rotateSpeed, RotateMode.FastBeyond360)
            .SetRelative(true) // Importantíssimo para rotacionar em volta de si mesmo
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);

        // 3. Animação Tilt (Inclinação lateral com volta)
        // Sequência para inclinar para um lado, voltar, inclinar para o outro, voltar
        _tiltTween = DOTween.Sequence()
            .Append(transform.DOLocalRotate(new Vector3(tiltAmount, 0, 0), tiltDuration * 0.25f).SetEase(Ease.InOutSine).SetRelative(true)) // Tilt para frente
            .Append(transform.DOLocalRotate(new Vector3(-2 * tiltAmount, 0, 0), tiltDuration * 0.5f).SetEase(Ease.InOutSine).SetRelative(true)) // Tilt para trás
            .Append(transform.DOLocalRotate(new Vector3(tiltAmount, 0, 0), tiltDuration * 0.25f).SetEase(Ease.InOutSine).SetRelative(true)); // Tilt para frente
            //Nota: Removido o método "From", que foi considerado inútil e incorreto, e adicionada a lógica de voltar para a posição original no Tween
            //Voltar para a posição original = Ir para -angulo , e não partir do 0.

        _tiltTween.SetLoops(-1, LoopType.Restart); // Loop infinito
         // Nota: Se quiser tilt para os lados (roll), use o eixo Z na rotação.

        // Opcional: Agrupar todos os tweens em uma Sequence ou Join se quiser controlá-los juntos.
        // sequence.Play(); // Inicia a sequência principal.

        // Guardar referências dos tweens (_waveTween, _rotateTween, _tiltTween) permite pausar/retomar se necessário.
        // DOTween.Kill(this.transform) mata todos que usam o transform como target, o que é o caso aqui.
    }

    private void StopWaveAndRotateAnimation()
    {
        // Stop e mata as animações DOTween associadas ao transform deste objeto
        DOTween.Kill(this.transform);
        _waveTween = null;
        _rotateTween = null;
        _tiltTween = null;
    }

    // Método público para ligar/desligar a animação (se quiser controlá-la externamente)
    public void SetWaveAndRotateAnimationEnabled(bool enabled)
    {
        if (shouldWaveAndRotate == enabled) return; // Sem mudança

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

    // TODO: Pode adicionar uma verificação em LateUpdate para garantir que as animações
    // sejam reiniciadas se shouldWaveAndRotate mudar para true no Inspector em runtime.
    // void LateUpdate() { if (shouldWaveAndRotate && _waveTween == null && _rotateTween == null && _tiltTween == null) StartWaveAndRotateAnimation(); }
}