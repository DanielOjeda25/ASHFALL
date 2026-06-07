using System.Collections.Generic;
using UnityEngine;

namespace ShooterDem
{
// Ataque kamikaze: al alcanzar al objetivo EXPLOTA -> dano en area a todo IDamageable
// del radio (con caida por distancia) + knockback, y el propio enemigo MUERE.
// Reusa el mismo patron de explosion que el Projectile. Va en el prefab del Kamikaze
// (en lugar de MeleeAttack) junto a EnemyAI/EnemyHealth.
[RequireComponent(typeof(EnemyHealth))]
public class KamikazeAttack : EnemyAttack
{
    [Header("Explosion")]
    public int damage = 40;
    public float radius = 3.5f;
    public float knockback = 6f;
    public LayerMask hitMask = ~0;

    private EnemyHealth self;
    private readonly HashSet<IDamageable> alreadyHit = new HashSet<IDamageable>();

    void Awake()
    {
        self = GetComponent<EnemyHealth>();
    }

    public override void Execute(Transform target)
    {
        // Dano en area: cada IDamageable del radio (menos uno mismo) recibe dano con
        // caida lineal. HashSet para no golpear dos veces al mismo si tiene varios colliders.
        alreadyHit.Clear();
        foreach (var col in Physics.OverlapSphere(transform.position, radius, hitMask))
        {
            var dmgable = col.GetComponentInParent<IDamageable>();
            if (dmgable == null || ReferenceEquals(dmgable, self) || !alreadyHit.Add(dmgable))
                continue;

            float t = Mathf.Clamp01(Vector3.Distance(transform.position, col.transform.position) / radius);
            dmgable.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(damage * (1f - t))));

            if (knockback > 0f)
            {
                var kb = col.GetComponentInParent<IKnockbackable>();
                if (kb != null)
                    kb.ApplyKnockback(col.transform.position - transform.position, knockback * (1f - t));
            }
        }

        // El kamikaze muere al explotar (vuelve al pool via OnDeath).
        if (self != null) self.Kill();
    }
}
}
