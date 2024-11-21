using UnityEngine;
using UnityEngine.UI;

public class SkillShop : MonoBehaviour
{
    public static SkillShop Instance; // Singleton para acesso global
    public Text cyanText;
    public Text greenText;
    public Text purpleText;

    // Referência ao Player para desbloquear habilidades
    public PlayerSkills player;

    // Custo das habilidades
    public int dashCostCyan = 10;
    public int dashCostGreen = 5;
    public int dashCostPurple = 0;

    public int doubleJumpCostCyan = 15;
    public int doubleJumpCostGreen = 10;
    public int doubleJumpCostPurple = 5;

    public int shieldCostCyan = 20;
    public int shieldCostGreen = 15;
    public int shieldCostPurple = 10;

    // Referência aos contadores de itens
    private int cyanCount;
    private int greenCount;
    private int purpleCount;

    private void Awake()
    {
        // Configura o singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        // Inicializa os textos com os valores iniciais
        UpdateUI();
    }

    public void SetCounts(int cyan, int green, int purple)
    {
        cyanCount = cyan;
        greenCount = green;
        purpleCount = purple;
        UpdateUI();
    }

    private void UpdateUI()
    {
        cyanText.text = cyanCount.ToString();
        greenText.text = greenCount.ToString();
        purpleText.text = purpleCount.ToString();
    }

        // Função para comprar Dash
    public void BuyDash()
    {
        if (player.hasDash)
        {
            Debug.Log("Dash já desbloqueado!");
            return; // Interrompe a execução, pois a habilidade já está desbloqueada
        }

        if (cyanCount >= dashCostCyan && greenCount >= dashCostGreen && purpleCount >= dashCostPurple)
        {
            cyanCount -= dashCostCyan;
            greenCount -= dashCostGreen;
            purpleCount -= dashCostPurple;

            player.hasDash = true; // Desbloqueia a habilidade
            Debug.Log("Dash desbloqueado!");

            UpdateUI();
        }
        else
        {
            Debug.Log("Não há itens suficientes para comprar Dash.");
        }
    }

    // Função para comprar Pulo Duplo
    public void BuyDoubleJump()
    {
        if (player.hasDoubleJump)
        {
            Debug.Log("Pulo Duplo já desbloqueado!");
            return; // Interrompe a execução
        }

        if (cyanCount >= doubleJumpCostCyan && greenCount >= doubleJumpCostGreen && purpleCount >= doubleJumpCostPurple)
        {
            cyanCount -= doubleJumpCostCyan;
            greenCount -= doubleJumpCostGreen;
            purpleCount -= doubleJumpCostPurple;

            player.hasDoubleJump = true; // Desbloqueia a habilidade
            Debug.Log("Pulo Duplo desbloqueado!");

            UpdateUI();
        }
        else
        {
            Debug.Log("Não há itens suficientes para comprar Pulo Duplo.");
        }
    }

    // Função para comprar Escudo
    public void BuyShield()
    {
        if (player.hasShield)
        {
            Debug.Log("Escudo já desbloqueado!");
            return; // Interrompe a execução
        }

        if (cyanCount >= shieldCostCyan && greenCount >= shieldCostGreen && purpleCount >= shieldCostPurple)
        {
            cyanCount -= shieldCostCyan;
            greenCount -= shieldCostGreen;
            purpleCount -= shieldCostPurple;

            player.hasShield = true; // Desbloqueia a habilidade
            Debug.Log("Escudo desbloqueado!");

            UpdateUI();
        }
        else
        {
            Debug.Log("Não há itens suficientes para comprar Escudo.");
        }
    }
}
