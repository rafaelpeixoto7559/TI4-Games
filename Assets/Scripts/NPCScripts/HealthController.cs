using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth = 3; // M�ximo de vidas
    private int currentHealth;
    

    private void Update()
    {
        if(currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Define a vida inicial do jogador
        currentHealth = maxHealth;  
    }

    // Fun��o para reduzir a vida do jogador
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
    }

}
