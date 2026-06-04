using UnityEngine;

// Vida del jugador. Va en el GameObject "Player".
// El enemigo le llama a TakeDamage() cuando esta cerca y ataca.
public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    // "Ventanita" de solo-lectura para que el HUD pueda consultar la vida actual.
    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // ya muerto: no acumular golpes ni repetir Die()

        currentHealth -= amount;
        Debug.Log($"Player recibe {amount} de dano. Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Has muerto.");

        // Avisamos al arbitro (la pantalla de derrota se monta en 4b).
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }
}
