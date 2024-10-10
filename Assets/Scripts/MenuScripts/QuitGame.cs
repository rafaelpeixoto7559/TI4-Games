using UnityEngine;
using UnityEngine.SceneManagement; // Se você quiser carregar uma nova cena ao invés de sair

public class QuitGame : MonoBehaviour
{
    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else

            Application.Quit();
        #endif
    }
}