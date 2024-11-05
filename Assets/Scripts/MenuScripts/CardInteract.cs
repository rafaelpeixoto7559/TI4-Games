using UnityEngine;

public class CardInteract : MonoBehaviour
{
    public GameObject card; // O card de texto já criado (arraste o objeto do card aqui no inspector)
    private bool isPlayerInRange = false;
    private bool isCardActive = false; // Para verificar se o card está ativo

    void Update()
    {
        // Verifica se o jogador está dentro da área do collider e apertou a tecla 'F'
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            // Alterna a visibilidade do card de texto
            isCardActive = !isCardActive;
            card.SetActive(isCardActive);
        }
    }

    // Detecta quando o jogador entra ou sai da área do collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Assegure-se de que o jogador tenha a tag "Player"
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Fecha o card se o jogador sair da zona
            if (isCardActive)
            {
                isCardActive = false;
                card.SetActive(false);
            }
        }
    }
}