using UnityEngine;
using TMPro; // TextMeshPro

// Muestra vida y municion. En vez de leer cada frame (polling + alloc de string
// cada frame), se SUSCRIBE a los eventos y solo redibuja cuando algo cambia:
// PlayerHealth.Damaged para la vida y Weapon.AmmoChanged para la municion.
public class HUD : MonoBehaviour
{
    [Header("Fuentes de datos")]
    public PlayerHealth playerHealth; // arrastra el Player
    public Weapon weapon;             // arrastra el Weapon

    [Header("Textos (TextMeshPro)")]
    public TMP_Text healthText;       // arrastra el texto de vida
    public TMP_Text ammoText;         // arrastra el texto de municion

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.Damaged += OnHealthChanged;
        if (weapon != null) weapon.AmmoChanged += RefreshAmmo;
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.Damaged -= OnHealthChanged;
        if (weapon != null) weapon.AmmoChanged -= RefreshAmmo;
    }

    void Start()
    {
        // Pintamos el estado inicial (los eventos solo disparan al CAMBIAR).
        RefreshHealth();
        RefreshAmmo();
    }

    // La firma encaja con Health.Damaged (int, int); leemos del componente igualmente.
    void OnHealthChanged(int current, int max) => RefreshHealth();

    void RefreshHealth()
    {
        if (playerHealth != null && healthText != null)
            healthText.text = $"VIDA  {playerHealth.CurrentHealth}/{playerHealth.maxHealth}";
    }

    void RefreshAmmo()
    {
        if (weapon != null && ammoText != null)
        {
            ammoText.text = weapon.IsReloading
                ? "RECARGANDO..."
                : $"MUNICION  {weapon.CurrentAmmo}/{weapon.magazineSize}";
        }
    }
}
