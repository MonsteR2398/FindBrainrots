using Treasures.Services;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip _music;

    private void Start()
    {
        AppServices.Audio.PlaySfx(_music);
    }
}
