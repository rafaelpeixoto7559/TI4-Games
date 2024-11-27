using System.Collections;
using UnityEngine;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        public float moveSpeed = 3.5f;  // Velocidade de movimentação
        public float dashSpeed = 10f;   // Velocidade do Dash
        public float dashDuration = 0.2f; // Duração do Dash
        public float dashCooldown = 1f;   // Tempo de cooldown do Dash

        public GameObject textBox;      // Caixa de texto no Inspector
        public GameObject interactIcon; // Ícone de interação no Inspector
        public static int direction;

        private Animator animator;
        private Rigidbody2D rb;
        private Vector2 movement;
        private bool isInInteractableZone = false; // Flag para interação
        private bool isDashing = false;           // Flag para o estado de dash
        private float dashCooldownTimer = 0f;     // Timer de cooldown do dash

        private PlayerSkills playerSkills;        // Referência ao script PlayerSkills

        private void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();

            playerSkills = FindObjectOfType<PlayerSkills>();

            if (playerSkills == null)
            {
                Debug.LogError("PlayerSkills não encontrado! Certifique-se de que o script PlayerSkills está na cena.");
            }

            if (textBox != null) textBox.SetActive(false);
            if (interactIcon != null) interactIcon.SetActive(false);
        }

        private void Update()
        {
            // Atualiza o cooldown do dash
            if (dashCooldownTimer > 0)
            {
                dashCooldownTimer -= Time.deltaTime;
            }

            // Captura a entrada do teclado para as direções
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");

            // Configuração do Animator com base na direção
            if (movement.x < 0) direction = 3;
            else if (movement.x > 0) direction = 2;
            else if (movement.y > 0) direction = 1;
            else if (movement.y < 0) direction = 0;

            animator.SetInteger("Direction", direction);
            animator.SetBool("IsMoving", movement.magnitude > 0);

            // Verifica se o jogador pode usar o dash
            if (playerSkills != null && playerSkills.hasDash && Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0 && !isDashing)
            {
                StartCoroutine(PerformDash());
            }

            // Interação
            if (isInInteractableZone && Input.GetKeyDown(KeyCode.F))
            {
                if (textBox != null) textBox.SetActive(!textBox.activeSelf);
            }
        }

        private void FixedUpdate()
        {
            if (!isDashing)
            {
                rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
            }
        }

        private IEnumerator PerformDash()
        {
            isDashing = true;

            // Captura a direção do dash baseada no input atual
            Vector2 dashDirection = movement.normalized;
            if (dashDirection == Vector2.zero) // Se não houver input, usa a última direção conhecida
            {
                dashDirection = GetDirectionVector();
            }

            rb.velocity = dashDirection * dashSpeed; // Aplica a velocidade do dash

            yield return new WaitForSeconds(dashDuration);

            rb.velocity = Vector2.zero;
            isDashing = false;
            dashCooldownTimer = dashCooldown;
        }

        private Vector2 GetDirectionVector()
        {
            switch (direction)
            {
                case 0: return Vector2.down;
                case 1: return Vector2.up;
                case 2: return Vector2.right;
                case 3: return Vector2.left;
                default: return Vector2.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Interactable"))
            {
                isInInteractableZone = true;
                if (interactIcon != null) interactIcon.SetActive(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Interactable"))
            {
                isInInteractableZone = false;
                if (interactIcon != null) interactIcon.SetActive(false);
                if (textBox != null) textBox.SetActive(false);
            }
        }
    }
}