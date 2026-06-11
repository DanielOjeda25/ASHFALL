using UnityEngine;

namespace ShooterDem
{
    /// <summary>
    /// Utilidades de audio compartidas. El patron "elegir una variante al azar y sonarla"
    /// estaba copiado en PlayerAudio, EnemyAudio (y el difunto WeaponAudio); ahora vive
    /// en un solo lugar. Arrays de variantes = anti-repeticion (sabor), null-safe siempre.
    /// </summary>
    public static class AudioUtil
    {
        // Una variante al azar (null si no hay clips). Util cuando el llamador necesita
        // el clip en la mano (p. ej. para PooledSfx o volumen propio).
        public static AudioClip PickRandom(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }

        // Suena una variante al azar por la fuente dada (PlayOneShot: mezcla sin cortar loops).
        public static void PlayRandom(AudioSource source, AudioClip[] clips, float volume = 1f)
        {
            var clip = PickRandom(clips);
            if (clip != null && source != null) source.PlayOneShot(clip, volume);
        }
    }
}
