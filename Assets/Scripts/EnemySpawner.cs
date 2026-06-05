using UnityEngine;

// Genera enemigos en posiciones aleatorias alrededor de este objeto. Ya NO genera
// solo en Start: es una herramienta que el WaveSystem invoca por oleada
// (SpawnEnemies). Va en un GameObject vacio (ej. "EnemySpawner").
public class EnemySpawner : MonoBehaviour
{
    [Header("Que generar")]
    public GameObject enemyPrefab;    // arrastra aqui el prefab del Enemy

    [Header("Donde")]
    public float areaRadius = 20f;    // radio (en metros) alrededor del spawner
    public float spawnHeight = 1f;    // altura Y (centro de la capsula = 1)

    // Llamado por el WaveSystem: instancia n enemigos repartidos por el area.
    public void SpawnEnemies(int n)
    {
        for (int i = 0; i < n; i++)
            SpawnOne();
    }

    void SpawnOne()
    {
        // Random.insideUnitCircle da un punto al azar dentro de un circulo de radio 1.
        // Lo multiplicamos por el radio para repartirlos por el area.
        Vector2 circle = Random.insideUnitCircle * areaRadius;
        Vector3 pos = transform.position + new Vector3(circle.x, spawnHeight, circle.y);

        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }
}
