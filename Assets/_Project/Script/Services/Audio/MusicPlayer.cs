using Treasures.Services;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _music;

    private void Start()
    {
        AppServices.Audio.PlayMusic(_music);
    }
}
