using UnityEngine;

// Sonidos del arma. Escucha los eventos de Weapon y reproduce los clips (2D).
// Va en el mismo GameObject que Weapon. Requiere un AudioSource (el "altavoz").
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Weapon))]
public class WeaponAudio : MonoBehaviour
{
    [Header("Clips")]
    // Arrays = variantes; elegimos una al azar para que no suene repetitivo.
    public AudioClip[] fireClips;            // disparo
    public AudioClip emptyClip;             // clic sin municion
    public AudioClip reloadClip;            // recarga
    public AudioClip[] concreteImpactClips; // golpe en pared/suelo
    public AudioClip[] fleshImpactClips;    // golpe en enemigo

    private Weapon weapon;
    private AudioSource audioSource;

    void Awake()
    {
        weapon = GetComponent<Weapon>();
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        weapon.Fired += HandleFired;
        weapon.DryFired += HandleDryFired;
        weapon.ReloadStarted += HandleReload;
        weapon.Hit += HandleHit;
    }

    void OnDisable()
    {
        weapon.Fired -= HandleFired;
        weapon.DryFired -= HandleDryFired;
        weapon.ReloadStarted -= HandleReload;
        weapon.Hit -= HandleHit;
    }

    void HandleFired() => PlayRandom(fireClips);

    void HandleDryFired()
    {
        if (emptyClip != null) audioSource.PlayOneShot(emptyClip);
    }

    void HandleReload()
    {
        if (reloadClip != null) audioSource.PlayOneShot(reloadClip);
    }

    // Carne si golpeamos algo danable (un enemigo); pared si no.
    void HandleHit(RaycastHit hit, bool hitDamageable)
        => PlayRandom(hitDamageable ? fleshImpactClips : concreteImpactClips);

    // PlayOneShot permite que varios sonidos se solapen sin cortarse.
    void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip != null)
            audioSource.PlayOneShot(clip);
    }
}
