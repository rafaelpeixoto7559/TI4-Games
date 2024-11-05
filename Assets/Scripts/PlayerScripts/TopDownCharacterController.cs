using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        public float moveSpeed = 3.5f;  // Velocidade de movimentação
        public GameObject textBox;      // Arraste a caixa de texto aqui no Inspector
        public GameObject interactIcon; // Arraste o ícone "F" (objeto Keyboard) aqui no Inspector

        private Animator animator;
        private Rigidbody2D rb;
        private Vector2 movement;
        private bool isInInteractableZone = false;  // Flag para verificar se o player está na área de interação

        private void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();  // Referência ao Rigidbody2D do Player

            if (textBox != null)
            {
                textBox.SetActive(false); // Inicialmente desativa a caixa de texto
            }

            if (interactIcon != null)
            {
                interactIcon.SetActive(false); // Inicialmente desativa o ícone de interação
            }
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

            // Verifica se o jogador está na área interativa e pressionou a tecla "F"
            if (isInInteractableZone && Input.GetKeyDown(KeyCode.F))
            {
                if (textBox != null)
                {
                    // Alterna a visibilidade da caixa de texto
                    textBox.SetActive(!textBox.activeSelf);
                }
            }
        }

        private void FixedUpdate()
        {
            // Movimentar o player com base nas entradas de teclado e velocidade
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Verifica se o player entrou na área de interação
            if (other.CompareTag("Interactable"))
            {
                isInInteractableZone = true;

                // Ativa o ícone de interação (o "F" acima da cabeça)
                if (interactIcon != null)
                {
                    interactIcon.SetActive(true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Verifica se o player saiu da área de interação
            if (other.CompareTag("Interactable"))
            {
                isInInteractableZone = false;

                // Desativa o ícone de interação e a caixa de texto
                if (interactIcon != null)
                {
                    interactIcon.SetActive(false);
                }

                if (textBox != null)
                {
                    textBox.SetActive(false);
                }
            }
        }
    }
}
