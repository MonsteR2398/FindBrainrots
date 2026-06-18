using Treasures.Services;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _defaultAudioClip;

    public void Play()
    {
        AppServices.Audio.PlaySfx(_defaultAudioClip);
    }
}