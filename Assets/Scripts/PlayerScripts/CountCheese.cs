using UnityEngine;
using UnityEngine.UI;

public class CoinCounter : MonoBehaviour
{
    public Text coinText;   // O campo de texto para exibir a quantidade de moedas
    public int currentCoins = 0;   // Quantidade inicial de moedas

    void Start()
    {
        UpdateCoinUI();
    }

    // Função para adicionar moedas e atualizar a UI
    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinUI();
    }

    // Função para atualizar o texto de moedas
    void UpdateCoinUI()
    {
        coinText.text = currentCoins.ToString();
    }
}