using UnityEngine;
using UnityEngine.SceneManagement;  // Necessário para carregar cenas

public class SceneLoader : MonoBehaviour
{
    // Nome da cena que você quer carregar
    public string sceneName;

    // Função que será chamada quando o botão for clicado
    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
