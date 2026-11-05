using UnityEngine;

public sealed class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip forestAmbience;
    [SerializeField] private AudioClip horrorMusic;
    [SerializeField] private AudioClip playerFootsteps;
    [SerializeField] private AudioClip collisionSound;
    [SerializeField] private AudioClip zombieGrowl;

    private AudioSource ambienceSource;
    private AudioSource musicSource;
    private AudioSource footstepsSource;
    private AudioSource effectsSource;

    public void Configure(AudioClip ambience, AudioClip music, AudioClip footsteps, AudioClip collision, AudioClip zombie)
    {
        forestAmbience = ambience;
        horrorMusic = music;
        playerFootsteps = footsteps;
        collisionSound = collision;
        zombieGrowl = zombie;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        footstepsSource = gameObject.AddComponent<AudioSource>();
        effectsSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.loop = true;
        musicSource.loop = true;
        footstepsSource.loop = true;
        ambienceSource.volume = 0.45f;
        musicSource.volume = 0.35f;
        footstepsSource.volume = 0.6f;
    }

    public void PlayRunAudio()
    {
        PlayLoop(ambienceSource, forestAmbience);
        PlayLoop(musicSource, horrorMusic);
    }

    public void PlayCollision()
    {
        PlayEffect(collisionSound);
    }

    public void PlayZombie()
    {
        PlayEffect(zombieGrowl);
    }

    public void SetFootsteps(bool playing)
    {
        if (footstepsSource == null || playerFootsteps == null)
            return;

        if (playing && !footstepsSource.isPlaying)
        {
            footstepsSource.clip = playerFootsteps;
            footstepsSource.Play();
        }
        else if (!playing && footstepsSource.isPlaying)
        {
            footstepsSource.Stop();
        }
    }

    public void StopFootsteps()
    {
        SetFootsteps(false);
    }

    private static void PlayLoop(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null)
            return;

        source.clip = clip;
        if (!source.isPlaying)
            source.Play();
    }

    private void PlayEffect(AudioClip clip)
    {
        if (clip != null && effectsSource != null)
            effectsSource.PlayOneShot(clip);
    }
}
