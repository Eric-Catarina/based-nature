// Path: Assets/_ProjectName/Scripts/Audio/SoundEffectDefinition.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundEffect", menuName = "Audio/Sound Effect Definition")]
public class SoundEffectDefinition : ScriptableObject
{
    public AudioClip[] clips; // Array de clipes para variação

    [Range(0f, 1f)]
    public float volume = 1f;

    [Range(0.1f, 3f)]
    public float minPitch = 0.8f;

    [Range(0.1f, 3f)]
    public float maxPitch = 1.2f;

    // Você pode adicionar mais propriedades aqui como:
    // public bool loop = false;
    // public AudioMixerGroup outputAudioMixerGroup;
}