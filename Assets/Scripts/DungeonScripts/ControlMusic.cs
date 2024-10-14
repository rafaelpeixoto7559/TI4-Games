using UnityEngine;

public class ChangeMusicOnSceneLoad : MonoBehaviour
{
    public AudioClip newMusic;  // A música específica para esta cena

    void Start()
    {
        // Ao entrar na cena, peça ao MusicManager para tocar a nova música
        if (MusicManager.GetInstance() != null && newMusic != null)
        {
            MusicManager.GetInstance().PlayMusic(newMusic);
        }
    }
}