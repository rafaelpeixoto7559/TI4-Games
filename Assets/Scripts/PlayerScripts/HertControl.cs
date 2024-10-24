using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3; // Máximo de vidas
    private int currentHealth;

    // Referências aos corações (arraste as imagens do coração no Inspector)
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    void Start()
    {
        // Define a vida inicial do jogador
        currentHealth = maxHealth;
        UpdateHearts();
    }

    // Função para reduzir a vida do jogador
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        UpdateHearts();
    }

    // Função para atualizar os corações na tela
    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].sprite = fullHeart; // Mostra o coração cheio
            }
            else
            {
                hearts[i].sprite = emptyHeart; // Mostra o coração vazio
            }
        }
    }
}