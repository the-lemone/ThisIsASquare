
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;
    [SerializeField] private AudioSource soundMove;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public void PlaySoundMove(AudioClip clip, Transform spawnTransform, float volume, float pitch)
    {
        // spawn in game object
        AudioSource audioSource = Instantiate(soundMove, spawnTransform.position, Quaternion.identity);
        
        // assign the audioClip
        audioSource.clip = clip;
        
        // assign volume
        audioSource.volume = volume;
        
        audioSource.pitch = pitch;
        
        // play sound
        audioSource.Play();
        
        // get length of sound FX clip
        float clipLength = audioSource.clip.length;
        
        // Destroy the clip after it is done
        Destroy(audioSource.gameObject, clipLength);
    }
}
