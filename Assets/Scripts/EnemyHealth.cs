using UnityEngine;

// Vida de un enemigo. Va en el GameObject "Enemy".
// El arma le llama a TakeDamage() cuando el raycast lo golpea.
public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;

    private int currentHealth;
    private bool isDead;

    void Start()
    {
        // Empieza con la vida llena.
        currentHealth = maxHealth;

        // Nos apuntamos en el GameManager para que lleve la cuenta.
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemy();
    }

    // Metodo PUBLICO: lo llama Weapon cuando un disparo acierta a este enemigo.
    public void TakeDamage(int amount)
    {
        if (isDead) return; // ya muerto: ignora golpes extra

        currentHealth -= amount;
        Debug.Log($"{name} recibe {amount} de dano. Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;
        Debug.Log($"{name} ha muerto.");

        // Avisamos al arbitro antes de destruirnos.
        if (GameManager.Instance != null)
            GameManager.Instance.EnemyKilled();

        Destroy(gameObject); // de momento desaparece; efectos vendran despues
    }
}
