using System;
using System.Collections;
using UnityEngine;

// Cerebro de las hordas. Genera oleadas crecientes via EnemySpawner y avanza a la
// siguiente cuando se limpia la actual. Cuenta enemigos vivos con los eventos
// EnemyHealth.Spawned/Killed (mismo patron observador del resto del proyecto).
//
// Modo HIBRIDO: totalWaves = 0 -> infinitas (survival, sin victoria);
//               totalWaves = N -> finitas, victoria al limpiar la oleada N.
public class WaveSystem : MonoBehaviour
{
    [Header("Oleadas")]
    public int totalWaves = 0;            // 0 = infinitas; N = finitas con victoria
    public int baseEnemies = 5;           // enemigos en la oleada 1
    public int enemiesGrowthPerWave = 3;  // +N enemigos por cada oleada siguiente
    public float timeBetweenWaves = 3f;   // descanso entre oleadas (segundos)
    public float firstWaveDelay = 2f;     // margen antes de la primera oleada

    [Header("Generador")]
    public EnemySpawner spawner;          // quien instancia los enemigos

    private int currentWave;              // oleada actual (1-based)
    private int enemiesAlive;             // vivos de la oleada en curso

    public int CurrentWave => currentWave;
    public event Action<int> WaveChanged; // nuevo numero de oleada (para el HUD)

    void OnEnable()
    {
        EnemyHealth.Spawned += OnEnemySpawned;
        EnemyHealth.Killed += OnEnemyKilled;
    }

    void OnDisable()
    {
        EnemyHealth.Spawned -= OnEnemySpawned;
        EnemyHealth.Killed -= OnEnemyKilled;
    }

    void Start()
    {
        if (spawner == null)
        {
            Debug.LogError("WaveSystem: falta asignar el EnemySpawner.");
            return;
        }
        StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        while (true)
        {
            currentWave++;
            WaveChanged?.Invoke(currentWave);

            int count = Mathf.Max(1, baseEnemies + (currentWave - 1) * enemiesGrowthPerWave);
            Debug.Log($"=== OLEADA {currentWave} — {count} enemigos ===");
            spawner.SpawnEnemies(count);
            // Los Awake de los enemigos ya han incrementado enemiesAlive (Instantiate
            // ejecuta Awake en el acto), asi que aqui enemiesAlive == count.

            // Esperamos a que caigan todos los de esta oleada.
            yield return new WaitUntil(() => enemiesAlive <= 0);

            // Modo finito: si era la ultima oleada, victoria y fin.
            if (totalWaves > 0 && currentWave >= totalWaves)
            {
                Debug.Log("=== Todas las oleadas superadas ===");
                if (GameManager.Instance != null)
                    GameManager.Instance.TriggerVictory();
                yield break;
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void OnEnemySpawned(EnemyHealth enemy) => enemiesAlive++;
    void OnEnemyKilled(EnemyHealth enemy) => enemiesAlive--;
}
