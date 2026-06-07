using UnityEngine;

namespace ShooterDem
{
// Estrategia de ataque de un enemigo (patron Strategy via componente). EnemyAI gestiona
// el objetivo, el movimiento, el alcance y el cooldown; cuando toca atacar, delega el
// EFECTO a este componente. Asi un enemigo nuevo = otro EnemyAttack (melee, a distancia,
// kamikaze...) en su prefab, SIN tocar EnemyAI.
public abstract class EnemyAttack : MonoBehaviour
{
    // Ejecuta el ataque contra 'target' (lo llama EnemyAI al estar en rango + sin cooldown).
    public abstract void Execute(Transform target);
}
}
