using UnityEngine;

public class PlayerSkills : MonoBehaviour
{
    public bool hasDash = false;     // Habilidade de Dash
    public bool hasDoubleJump = false; // Habilidade de pulo duplo
    public bool hasShield = false;    // Habilidade de escudo

    // Exemplos de como usar essas habilidades no jogo:
    void Update()
    {
        if (hasDash && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("Usou Dash!");
            // Código para Dash aqui
        }

        if (hasDoubleJump)
        {
            Debug.Log("Pulo duplo desbloqueado!");
            // Código para pulo duplo aqui
        }
    }
}
