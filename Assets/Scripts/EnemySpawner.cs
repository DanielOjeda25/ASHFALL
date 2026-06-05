using UnityEngine;
using UnityEngine.AI; // NavMesh.SamplePosition vive aqui

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

    [Header("Validacion de sitio")]
    // Distancia minima al jugador: evita que un enemigo aparezca pegado/encima.
    public float minDistanceFromPlayer = 6f;
    // Cuanto puede "buscar" NavMesh.SamplePosition el punto valido mas cercano.
    public float navSampleMaxDistance = 4f;
    // Intentos por enemigo para encontrar un punto bueno antes de rendirse.
    public int maxSpawnAttempts = 20;

    private EnemyPool pool;  // recicla enemigos (creado en Awake, sin cablear en editor)

    void Awake()
    {
        pool = new EnemyPool(enemyPrefab, transform);
    }

    // Llamado por el WaveSystem: instancia n enemigos repartidos por el area.
    public void SpawnEnemies(int n)
    {
        for (int i = 0; i < n; i++)
            SpawnOne();
    }

    void SpawnOne()
    {
        // Posicion del jugador (O(1) via el localizador) para la distancia minima.
        Vector3? playerPos = PlayerHealth.Current != null
            ? PlayerHealth.Current.transform.position
            : (Vector3?)null;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Punto al azar dentro del circulo de radio areaRadius.
            Vector2 circle = Random.insideUnitCircle * areaRadius;
            Vector3 candidate = transform.position + new Vector3(circle.x, spawnHeight, circle.y);

            // #2 Demasiado cerca del jugador? descarta y reintenta.
            if (playerPos.HasValue &&
                Vector3.Distance(candidate, playerPos.Value) < minDistanceFromPlayer)
                continue;

            // #1 Ajusta el punto al NavMesh mas cercano. Si no hay NavMesh en
            // navSampleMaxDistance, este candidato no sirve: reintenta.
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleMaxDistance, NavMesh.AllAreas))
                continue;

            // Punto valido: spawneamos sobre el NavMesh y mantenemos la altura de
            // la capsula (centro a spawnHeight sobre el suelo encontrado).
            Vector3 pos = new Vector3(hit.position.x, hit.position.y + spawnHeight, hit.position.z);
            pool.Get(pos, Quaternion.identity);   // reutiliza si hay; si no, crea uno
            return;
        }

        // Tras N intentos no hubo sitio valido. No instanciamos uno "roto" fuera del
        // NavMesh (causaria un agente que no se mueve y colgaria la oleada). Avisamos.
        Debug.LogWarning(
            $"EnemySpawner: no encontre un punto valido en {maxSpawnAttempts} intentos " +
            $"(radio {areaRadius}, NavMesh? distancia minima {minDistanceFromPlayer}). Enemigo omitido.");
    }
}
