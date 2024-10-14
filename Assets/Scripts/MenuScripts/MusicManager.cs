using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource audioSource;

    // O Awake mantém a instância do MusicManager ao longo das cenas
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Pega o AudioSource que está no mesmo GameObject
        audioSource = GetComponent<AudioSource>();
    }

    // Função para ajustar o volume da música
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    // Função para trocar a música (somente se for uma música nova)
    public void PlayMusic(AudioClip newMusic)
    {
        if (audioSource != null && audioSource.clip != newMusic)
        {
            audioSource.clip = newMusic;
            audioSource.Play();
        }
    }

    // Função para obter a instância do MusicManager
    public static MusicManager GetInstance()
    {
        return instance;
    }
}