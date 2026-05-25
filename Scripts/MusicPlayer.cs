using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] tracks;

    private int currentTrack = 0;

    void Start()
    {
        if (tracks.Length > 0)
        {
            PlayTrack(currentTrack);
        }
    }

    void Update()
    {
        if (!audioSource.isPlaying)
        {
            NextTrack();
        }
    }

    void PlayTrack(int index)
    {
        audioSource.clip = tracks[index];
        audioSource.Play();
    }

    void NextTrack()
    {
        currentTrack++;

        if (currentTrack >= tracks.Length)
            currentTrack = 0;

        PlayTrack(currentTrack);
    }
}