
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    [SerializeField] private AudioClip backgroundMusic; 
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject audioManagerGO = new GameObject("AudioManager");
                _instance = audioManagerGO.AddComponent<AudioManager>();
                
            }
            return _instance;
        }
    }

    public SoundEffectDefinition clickSound, conversationSound, endConversationSound, hoverSound, pickItemSound, dropItemSound, openSound, closeSound, equipSound, onDragSound, onDropSound; 

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
    }

    public void PlaySoundEffect(SoundEffectDefinition soundDef)
    {
        if (soundDef == null || soundDef.clips == null || soundDef.clips.Length == 0)
        {
            Debug.LogWarning("SoundEffectDefinition is null or has no clips.");
            return;
        }

        
        AudioClip clipToPlay = soundDef.clips[Random.Range(0, soundDef.clips.Length)];

        if (clipToPlay == null)
        {
            Debug.LogWarning("Selected AudioClip is null.");
            return;
        }

        
        GameObject tempAudioSourceGO = new GameObject("TempAudio_" + clipToPlay.name);

        AudioSource audioSource = tempAudioSourceGO.AddComponent<AudioSource>();
        audioSource.clip = clipToPlay;
        audioSource.volume = soundDef.volume;
        audioSource.pitch = Random.Range(soundDef.minPitch, soundDef.maxPitch);

        
        audioSource.spatialBlend = 1.0f; 
        audioSource.rolloffMode = AudioRolloffMode.Linear; 
        audioSource.minDistance = 1f;    
        audioSource.maxDistance = 50f;  

        audioSource.Play();

        
        Destroy(tempAudioSourceGO, clipToPlay.length / audioSource.pitch); 
    }

    
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
        
        

        AudioSource audioSource = tempAudioSourceGO.AddComponent<AudioSource>();
        audioSource.clip = clipToPlay;
        audioSource.volume = soundDef.volume;
        audioSource.pitch = Random.Range(soundDef.minPitch, soundDef.maxPitch);
        audioSource.spatialBlend = 0.0f; 

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
        audioSource.loop = true; 
        audioSource.volume = 0.5f; 
        audioSource.Play();

        
        
    }
    private void Start()
    {
        
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic(backgroundMusic);
        }
    }
}