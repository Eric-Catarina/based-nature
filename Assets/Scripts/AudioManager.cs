// Path: Assets/_ProjectName/Scripts/Audio/AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    [SerializeField] private AudioClip backgroundMusic; // Música de fundo, se necessário
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject audioManagerGO = new GameObject("AudioManager");
                _instance = audioManagerGO.AddComponent<AudioManager>();
                // Opcional: DontDestroyOnLoad(audioManagerGO); se você quiser que ele persista entre cenas
            }
            return _instance;
        }
    }

    public SoundEffectDefinition clickSound, conversationSound, endConversationSound, hoverSound, pickItemSound, dropItemSound, openSound, closeSound, equipSound, onDragSound, onDropSound; // Definição de som padrão, se necessário

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // Opcional: DontDestroyOnLoad(gameObject);
    }

    public void PlaySoundEffect(SoundEffectDefinition soundDef)
    {
        if (soundDef == null || soundDef.clips == null || soundDef.clips.Length == 0)
        {
            Debug.LogWarning("SoundEffectDefinition is null or has no clips.");
            return;
        }

        // Escolhe um clipe aleatório da definição
        AudioClip clipToPlay = soundDef.clips[Random.Range(0, soundDef.clips.Length)];

        if (clipToPlay == null)
        {
            Debug.LogWarning("Selected AudioClip is null.");
            return;
        }

        // Cria um GameObject temporário para o AudioSource
        GameObject tempAudioSourceGO = new GameObject("TempAudio_" + clipToPlay.name);

        AudioSource audioSource = tempAudioSourceGO.AddComponent<AudioSource>();
        audioSource.clip = clipToPlay;
        audioSource.volume = soundDef.volume;
        audioSource.pitch = Random.Range(soundDef.minPitch, soundDef.maxPitch);

        // Configurações importantes para sons 3D
        audioSource.spatialBlend = 1.0f; // 1.0 para 3D, 0.0 para 2D
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Ou Logarithmic, conforme preferir
        audioSource.minDistance = 1f;    // Distância mínima para volume máximo
        audioSource.maxDistance = 50f;  // Distância máxima onde o som ainda é audível (ajuste conforme sua escala)

        audioSource.Play();

        // Destrói o GameObject temporário após o clipe terminar
        Destroy(tempAudioSourceGO, clipToPlay.length / audioSource.pitch); // Considera o pitch na duração
    }

    // Sobrecarga para sons 2D (sem posição)
    public void PlaySoundEffectUI(SoundEffectDefinition soundDef)
    {
        if (soundDef == null || soundDef.clips == null || soundDef.clips.Length == 0)
        {
            Debug.LogWarning("SoundEffectDefinition is null or has no clips for UI sound.");
            return;
        }

        AudioClip clipToPlay = soundDef.clips[Random.Range(0, soundDef.clips.Length)];

        if (clipToPlay == null)
        {
            Debug.LogWarning("Selected AudioClip for UI sound is null.");
            return;
        }

        GameObject tempAudioSourceGO = new GameObject("TempUIAudio_" + clipToPlay.name);
        // Não defina DontDestroyOnLoad para sons de UI que devem ser específicos da cena,
        // a menos que você tenha um gerenciador de UI persistente.

        AudioSource audioSource = tempAudioSourceGO.AddComponent<AudioSource>();
        audioSource.clip = clipToPlay;
        audioSource.volume = soundDef.volume;
        audioSource.pitch = Random.Range(soundDef.minPitch, soundDef.maxPitch);
        audioSource.spatialBlend = 0.0f; // Som 2D para UI

        audioSource.Play();
        Destroy(tempAudioSourceGO, clipToPlay.length / audioSource.pitch);
    }

    private void PlayBackgroundMusic(AudioClip musicClip)
    {
        if (musicClip == null)
        {
            Debug.LogWarning("Background music clip is null.");
            return;
        }

        GameObject musicGO = new GameObject("BackgroundMusic");
        AudioSource audioSource = musicGO.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true; // Loop para música de fundo
        audioSource.volume = 0.5f; // Volume padrão, ajuste conforme necessário
        audioSource.Play();

        // Opcional: Não destruir o GameObject se você quiser que a música continue tocando entre cenas
        // DontDestroyOnLoad(musicGO);
    }
    private void Start()
    {
        // Se você tiver música de fundo, inicie-a aqui
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic(backgroundMusic);
        }
    }
}