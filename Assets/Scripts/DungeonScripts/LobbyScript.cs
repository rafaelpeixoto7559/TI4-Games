using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para carregar a nova cena

public class LoadSceneOnTrigger : MonoBehaviour
{
    // Nome da cena que deseja carregar
    public string sceneName;

    // Este método é chamado quando o Player colide com o objeto
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica se o objeto que colidiu tem a tag "Player"
        if (other.CompareTag("Player"))
        {
            // Carrega a nova cena
            SceneManager.LoadScene(sceneName);
        }
    }
}