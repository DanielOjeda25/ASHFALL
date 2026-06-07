using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // NavMesh.SamplePosition vive aqui

namespace ShooterDem
{
// Genera enemigos de VARIOS tipos (EnemyData) en posiciones validas alrededor del
// spawner, con un POOL por prefab (reciclaje). Lo invoca el WaveSystem por oleada.
// Va en un GameObject vacio (ej. "EnemySpawner").
public class EnemySpawner : MonoBehaviour
{
    [Header("Que generar")]
    public EnemyData[] enemyTypes;    // tipos disponibles (cada uno con su peso de spawn)

    [Header("Donde")]
    public float areaRadius = 20f;    // radio (en metros) alrededor del spawner
    public float spawnHeight = 1f;    // altura Y (centro de la capsula = 1)

    [Header("Validacion de sitio")]
    public float minDistanceFromPlayer = 6f; // no aparecer pegado al jugador
    public float navSampleMaxDistance = 4f;   // cuanto busca NavMesh el punto valido
    public int maxSpawnAttempts = 20;         // intentos por enemigo antes de rendirse
    public bool spawnOutOfView = true;        // preferir nacer fuera de camara

    private readonly Dictionary<GameObject, EnemyPool> pools = new Dictionary<GameObject, EnemyPool>();
    private float totalWeight;
    private Camera cam;

    void Awake()
    {
        // Un pool por prefab distinto; sumamos pesos para el sorteo ponderado.
        if (enemyTypes != null)
        {
            foreach (var t in enemyTypes)
            {
                if (t == null || t.prefab == null) continue;
                if (!pools.ContainsKey(t.prefab))
                    pools[t.prefab] = new EnemyPool(t.prefab, transform);
                totalWeight += Mathf.Max(0f, t.spawnWeight);
            }
        }
    }

    // Llamado por el WaveSystem: intenta generar n enemigos y devuelve cuantos
    // se crearon de VERDAD (algunos pueden omitirse si no hay sitio valido).
    public int SpawnEnemies(int n)
    {
        int spawned = 0;
        for (int i = 0; i < n; i++)
            if (SpawnOne()) spawned++;
        return spawned;
    }

    bool SpawnOne()
    {
        var type = PickType();
        if (type == null || type.prefab == null) return false;

        Vector3? playerPos = PlayerHealth.Current != null
            ? PlayerHealth.Current.transform.position
            : (Vector3?)null;

        bool hasFallback = false;
        Vector3 fallback = Vector3.zero;

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 circle = Random.insideUnitCircle * areaRadius;
            Vector3 candidate = transform.position + new Vector3(circle.x, spawnHeight, circle.y);

            if (playerPos.HasValue &&
                Vector3.Distance(candidate, playerPos.Value) < minDistanceFromPlayer)
                continue;

            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, navSampleMaxDistance, NavMesh.AllAreas))
                continue;

            Vector3 pos = new Vector3(hit.position.x, hit.position.y + spawnHeight, hit.position.z);

            if (spawnOutOfView && IsInView(pos))
            {
                if (!hasFallback) { fallback = pos; hasFallback = true; }
                continue;
            }

            pools[type.prefab].Get(pos, Quaternion.identity);
            return true;
        }

        if (hasFallback)
        {
            pools[type.prefab].Get(fallback, Quaternion.identity);
            return true;
        }

        Debug.LogWarning($"EnemySpawner: sin punto valido en {maxSpawnAttempts} intentos. Enemigo omitido.");
        return false;
    }

    // Sorteo ponderado por spawnWeight.
    EnemyData PickType()
    {
        if (enemyTypes == null || enemyTypes.Length == 0) return null;
        if (totalWeight <= 0f) return enemyTypes[0];

        float r = Random.value * totalWeight;
        foreach (var t in enemyTypes)
        {
            if (t == null || t.prefab == null) continue;
            r -= Mathf.Max(0f, t.spawnWeight);
            if (r <= 0f) return t;
        }
        return enemyTypes[0];
    }

    bool IsInView(Vector3 worldPos)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return false;
        Vector3 vp = cam.WorldToViewportPoint(worldPos);
        return vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;
    }
}
}
