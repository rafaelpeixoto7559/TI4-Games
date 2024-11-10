using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int maxHealth = 3; // M�ximo de vidas
    private int currentHealth;
    SpriteRenderer sr;

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
        sr = GetComponent<SpriteRenderer>();
    }

    // Fun��o para reduzir a vida do jogador
    public void TakeDamage(int damage)
    {
        StartCoroutine(DamageEffectSequence(sr, Color.red, 0.3f, 0.0f));
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
    }

    IEnumerator DamageEffectSequence(SpriteRenderer sr, Color dmgColor, float duration, float delay)
    {
        // save origin color
        Color originColor = sr.color;

        // tint the sprite with damage color
        sr.color = dmgColor;

        // you can delay the animation
        yield return new WaitForSeconds(delay);

        // lerp animation with given duration in seconds
        for (float t = 0; t < 1.0f; t += Time.deltaTime / duration)
        {
            sr.color = Color.Lerp(dmgColor, originColor, t);

            yield return null;
        }

        // restore origin color
        sr.color = originColor;
    }
}
