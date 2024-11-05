using UnityEngine;

public class OpenCloseConfig : MonoBehaviour
{
    public GameObject canvas; // Arraste seu Canvas aqui no Inspector

    // Método para abrir o Canvas
    public void OpenCanvas()
    {
        canvas.SetActive(true);
    }

    // Método para fechar o Canvas
    public void CloseCanvas()
    {
        canvas.SetActive(false);
    }
}
