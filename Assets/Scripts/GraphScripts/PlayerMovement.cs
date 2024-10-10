using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;  // Velocidade de movimentação
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();  // Referência ao Rigidbody2D do Player
    }

    void Update()
    {
        // Capturar a entrada do teclado para as direções
        movement.x = Input.GetAxisRaw("Horizontal");  // Eixo X (A/D ou Setas Esquerda/Direita)
        movement.y = Input.GetAxisRaw("Vertical");    // Eixo Y (W/S ou Setas Cima/Baixo)
    }

    void FixedUpdate()
    {
        // Movimentar o player com base nas entradas de teclado
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
