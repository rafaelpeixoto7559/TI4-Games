using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        public float moveSpeed = 3.5f;  // Velocidade de movimentação

        private Animator animator;
        private Rigidbody2D rb;
        private Vector2 movement;

        private void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();  // Referência ao Rigidbody2D do Player
        }

        private void Update()
        {
            // Capturar a entrada do teclado para as direções
            movement.x = Input.GetAxisRaw("Horizontal");  // Eixo X (A/D ou Setas Esquerda/Direita)
            movement.y = Input.GetAxisRaw("Vertical");    // Eixo Y (W/S ou Setas Cima/Baixo)

            // Configuração do Animator com base na direção
            if (movement.x < 0)
                animator.SetInteger("Direction", 3);  // Esquerda
            else if (movement.x > 0)
                animator.SetInteger("Direction", 2);  // Direita

            if (movement.y > 0)
                animator.SetInteger("Direction", 1);  // Cima
            else if (movement.y < 0)
                animator.SetInteger("Direction", 0);  // Baixo

            // Normaliza a direção para evitar movimento diagonal mais rápido
            movement.Normalize();

            // Define se o personagem está se movendo
            animator.SetBool("IsMoving", movement.magnitude > 0);
        }

        private void FixedUpdate()
        {
            // Movimentar o player com base nas entradas de teclado e velocidade
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
