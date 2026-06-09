using UnityEngine;

namespace ShooterDem
{
// Audio del enemigo (data-driven en el prefab), con su propio AudioSource 3D: en una horda
// oyes DE DONDE viene cada enemigo. Reacciona a eventos de la IA y de la vida (no al reves:
// ellos solo emiten). Reutilizable por cualquier tipo de enemigo (melee, kamikaze, ranged, tanque).
[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [Header("Clips (arrays = variantes elegidas al azar)")]
    public AudioClip idleLoop;        // bucle mientras vive (3D: solo se oyen los cercanos)
    public AudioClip[] alertClips;    // al DETECTAR al jugador (evento EnemyAI.Aggroed)
    public AudioClip[] attackClips;   // al ATACAR (lo llama EnemyAI tras Execute; ranged = disparo)
    public AudioClip[] hurtClips;     // al recibir dano NO letal (Health.Damaged)
    public AudioClip[] deathClips;    // al MORIR (suena en un objeto pooleado que sobrevive)

    [Header("Muerte (audio que sobrevive al reciclaje)")]
    public GameObject deathSfxPrefab; // prefab con PooledSfx (SfxOneShot); null si no hay deathClips
    public float deathVolume = 1f;

    [Header("Alerta")]
    public bool loopAlert = false;    // true: al detectar, el grito entra en BUCLE hasta morir
                                      // (kamikaze estilo Serious Sam: grita mientras carga)

    [Header("Variacion por instancia")]
    [Range(0f, 0.4f)]
    public float pitchJitter = 0f;    // +-tono aleatorio por enemigo: evita que varios suenen
                                      // clonados/sincronizados y desincroniza sus loops

    private AudioSource source;
    private EnemyAI ai;
    private Health health;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        ai = GetComponent<EnemyAI>();
        health = GetComponent<Health>();
    }

    // OnEnable/OnDisable (no Awake): asi tambien suscribe/reinicia al RECICLAR del pool.
    void OnEnable()
    {
        // Tono ligeramente distinto por instancia: desincroniza loops y evita el efecto "clon"
        // cuando varios del mismo tipo suenan a la vez. Se re-sortea en cada reciclaje del pool.
        source.pitch = pitchJitter > 0f ? 1f + Random.Range(-pitchJitter, pitchJitter) : 1f;

        if (idleLoop != null)
        {
            source.clip = idleLoop;
            source.loop = true;
            source.Play();              // arranca el ambiente al aparecer (o revivir del pool)
        }
        if (ai != null) ai.Aggroed += OnAggro;
        if (health != null)
        {
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }
    }

    void OnDisable()
    {
        if (ai != null) ai.Aggroed -= OnAggro;
        if (health != null)
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }
        source.Stop();                  // corta el loop al volver al pool / morir
    }

    // Al DETECTAR al jugador. El "idle" lejano (ambiente de "hay un enemigo en la zona") ya no
    // tiene sentido —el bicho te vio—, asi que el grito SUSTITUYE al idle (no se solapan).
    //  - loopAlert=true  (kamikaze): el grito entra en BUCLE 3D desde su posicion hasta morir;
    //    asi 3 kamikazes se oyen por separado y nunca quedan en silencio (estilo Serious Sam).
    //  - loopAlert=false: un grito puntual (PlayOneShot) y, si tenia idle, se corta.
    // Aggro es sticky (una sola vez); al reciclar del pool, OnEnable reactiva el idle.
    void OnAggro()
    {
        var clip = PickRandom(alertClips);
        if (loopAlert && clip != null)
        {
            source.clip = clip;
            source.loop = true;
            source.Play();              // bucle de grito mientras carga
        }
        else
        {
            if (idleLoop != null) { source.loop = false; source.Stop(); }
            if (clip != null) source.PlayOneShot(clip);
        }
    }

    // La llama EnemyAI justo despues de Execute (sirve a cualquier tipo de ataque).
    public void PlayAttack() => PlayRandom(attackClips);

    // Quejido SOLO en golpe no letal (en el letal suena la muerte, no el quejido).
    void OnDamaged(int current, int max)
    {
        if (current > 0) PlayRandom(hurtClips);
    }

    // Muerte: el enemigo vuelve al pool (su AudioSource se cortaria), asi que el clip se
    // reproduce en un objeto INDEPENDIENTE pooleado que sobrevive (PooledSfx).
    void OnDied()
    {
        if (deathClips == null || deathClips.Length == 0) return;
        var clip = deathClips[Random.Range(0, deathClips.Length)];
        PooledSfx.Play(deathSfxPrefab, clip, transform.position, deathVolume);
    }

    // PlayOneShot mezcla el efecto SOBRE el loop sin cortarlo.
    void PlayRandom(AudioClip[] clips)
    {
        var clip = PickRandom(clips);
        if (clip != null) source.PlayOneShot(clip);
    }

    static AudioClip PickRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
}
