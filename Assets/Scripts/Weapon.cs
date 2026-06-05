using System;
using System.Collections;          // corrutinas (IEnumerator)
using UnityEngine;
using UnityEngine.InputSystem;     // Nuevo Input System de Unity 6

// Responsabilidad UNICA: leer el input de disparo/recarga, lanzar el raycast,
// aplicar dano y gestionar la municion. NO sabe de sonido, particulas ni recoil:
// emite EVENTOS y los componentes satelite (WeaponAudio, WeaponEffects,
// WeaponRecoil) reaccionan suscribiendose. Va en el GameObject "Weapon".
public class Weapon : MonoBehaviour
{
    [Header("Disparo")]
    public float range = 100f;        // alcance del rayo en metros
    public int damage = 25;           // dano por disparo
    public Camera fpsCamera;          // desde donde sale el tiro (la Main Camera)

    [Header("Que se puede golpear")]
    public LayerMask hitMask = ~0;    // ~0 = todas las capas (de momento, todo)

    [Header("Municion")]
    public int magazineSize = 12;     // balas por cargador
    public float reloadTime = 1.5f;   // segundos que tarda la recarga

    private int currentAmmo;          // balas que quedan ahora mismo
    private bool isReloading;         // true mientras esta recargando

    // "Ventanitas" de solo-lectura para el HUD.
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;

    // Eventos para los componentes de efectos (patron observador).
    public event Action Fired;                  // disparo efectivo (gasta bala)
    public event Action DryFired;               // clic sin municion
    public event Action ReloadStarted;          // empieza la recarga
    public event Action<RaycastHit, bool> Hit;  // (info del golpe, golpeoAlgoDanable)

    void Awake()
    {
        // Si no arrastramos la camara en el Inspector, usamos la principal.
        if (fpsCamera == null)
            fpsCamera = Camera.main;
    }

    void Start()
    {
        // Empezamos con el cargador lleno.
        currentAmmo = magazineSize;
    }

    void Update()
    {
        // Si el juego esta congelado (pausa o game over), no disparamos ni recargamos.
        if (Time.timeScale == 0f) return;

        // Mientras recarga, ignoramos disparo y recarga.
        if (isReloading) return;

        var kb = Keyboard.current;
        // Recargar con R (solo si no esta ya lleno).
        if (kb != null && kb.rKey.wasPressedThisFrame && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        var mouse = Mouse.current;
        if (mouse == null) return;

        // wasPressedThisFrame = un disparo por clic (semiautomatico).
        if (mouse.leftButton.wasPressedThisFrame)
            Shoot();
    }

    void Shoot()
    {
        // Sin balas: avisa y emite DryFired (el sonido lo pone WeaponAudio).
        if (currentAmmo <= 0)
        {
            Debug.Log("Click! Sin municion (pulsa R para recargar)");
            DryFired?.Invoke();
            return;
        }

        // Gastamos una bala y avisamos del disparo (muzzle, sonido, recoil reaccionan).
        currentAmmo--;
        Debug.Log($"Disparo. Balas: {currentAmmo}/{magazineSize}");
        Fired?.Invoke();

        // Rayo desde el centro de la camara hacia delante.
        Vector3 origin = fpsCamera.transform.position;
        Vector3 direction = fpsCamera.transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask))
        {
            Debug.Log($"Impacto en: {hit.collider.name} (a {hit.distance:F1} m)");

            // Si lo golpeado se puede danar (IDamageable), le aplicamos dano.
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null)
                damageable.TakeDamage(damage);

            Debug.DrawLine(origin, hit.point, Color.red, 1f); // depuracion en Scene

            // Avisamos del impacto. El bool indica si golpeamos algo danable
            // (lo usa WeaponAudio para elegir sonido de carne vs pared).
            Hit?.Invoke(hit, damageable != null);
        }
        else
        {
            Debug.Log("Fallo (no golpeo nada)");
            Debug.DrawRay(origin, direction * range, Color.green, 1f);
        }
    }

    // Corrutina: espera reloadTime sin congelar el frame y rellena el cargador.
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recargando...");
        ReloadStarted?.Invoke();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log($"Recargado. Balas: {currentAmmo}/{magazineSize}");
    }
}
