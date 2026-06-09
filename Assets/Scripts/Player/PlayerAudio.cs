using UnityEngine;

namespace ShooterDem
{
    /// <summary>
    /// Voces del jugador (2D, feedback directo): quejido al recibir daño y grito al morir.
    /// Escucha los eventos de `PlayerHealth` (Damaged / Died). Es 2D porque es feedback del
    /// propio jugador, no algo posicional. El GameObject del player NO se destruye al morir
    /// (solo hay game over), así que el grito de muerte suena bien desde su propia fuente.
    /// Va en el GameObject raíz del player, junto a `PlayerHealth`.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerAudio : MonoBehaviour
    {
        [Header("Clips (variantes = se elige una al azar)")]
        public AudioClip[] hurtClips;   // al recibir daño NO letal
        public AudioClip[] deathClips;  // al morir

        [Range(0f, 1f)] public float volume = 1f;

        private AudioSource source;
        private PlayerHealth health;

        void Awake()
        {
            health = GetComponent<PlayerHealth>();
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;   // 2D: feedback del jugador
        }

        void OnEnable()
        {
            if (health != null)
            {
                health.Damaged += OnDamaged;
                health.Died += OnDied;
            }
        }

        void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
                health.Died -= OnDied;
            }
        }

        // Quejido SOLO en golpe no letal (en el letal suena el grito de muerte).
        void OnDamaged(int current, int max)
        {
            if (current > 0) PlayRandom(hurtClips);
        }

        void OnDied() => PlayRandom(deathClips);

        void PlayRandom(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return;
            var clip = clips[Random.Range(0, clips.Length)];
            if (clip != null) source.PlayOneShot(clip, volume);
        }
    }
}
