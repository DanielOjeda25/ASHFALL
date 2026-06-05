using UnityEngine;
using UnityEngine.AI; // NavMeshAgent vive aqui

// IA basica: el enemigo persigue al jugador usando el NavMesh.
// Va en el GameObject "Enemy" (necesita un NavMeshAgent).
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target; // arrastra aqui el "Player"

    [Header("Ataque")]
    public float attackRange = 2f;     // a que distancia puede golpear
    public int attackDamage = 10;      // dano por golpe
    public float attackCooldown = 1f;  // segundos entre golpes

    [Header("Rendimiento")]
    // Recalcular la ruta cada frame por enemigo no escala a hordas. Lo hacemos
    // cada repathInterval segundos, escalonado entre enemigos para no sincronizar.
    public float repathInterval = 0.2f;

    private NavMeshAgent agent;
    private IDamageable targetDamageable;  // a quien golpeamos (el jugador, vía interfaz)
    private float lastAttackTime;          // cuando golpeo por ultima vez
    private float repathTimer;             // cuenta atras para el proximo SetDestination

    void Awake()
    {
        // Cacheamos el agente una vez.
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        // Si nadie nos asigno un target, usamos el localizador del jugador (O(1),
        // sin escanear la escena). Se resuelve una vez al nacer.
        if (target == null && PlayerHealth.Current != null)
            target = PlayerHealth.Current.transform;

        // Guardamos su "danable" para poder restarle vida al atacar.
        if (target != null)
            targetDamageable = target.GetComponent<IDamageable>();

        // Escalonamos el primer repath para repartir la carga entre enemigos.
        repathTimer = Random.value * repathInterval;
    }

    void Update()
    {
        if (target == null) return;

        // Repath con throttle: solo recalculamos la ruta cada repathInterval seg
        // (no cada frame). El NavMeshAgent sigue moviendose suave entre recalculos.
        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            agent.SetDestination(target.position);
        }

        // El chequeo de ataque es barato: lo dejamos cada frame.
        // Si estamos lo bastante cerca, atacamos (con un cooldown entre golpes).
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            targetDamageable?.TakeDamage(attackDamage);
        }
    }
}
