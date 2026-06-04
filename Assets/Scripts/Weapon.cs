using System.Collections;          // necesario para las corrutinas (IEnumerator)
using UnityEngine;
using UnityEngine.InputSystem; // Nuevo Input System de Unity 6

// Disparo por raycast. Va en el GameObject "Weapon" (hijo de la camara).
// Al hacer clic, lanza un rayo desde el centro de la camara hacia delante
// y nos dice que objeto golpea y donde.
public class Weapon : MonoBehaviour
{
    [Header("Disparo")]
    public float range = 100f;        // alcance del rayo en metros
    public int damage = 25;           // dano por disparo
    public Camera fpsCamera;          // desde donde sale el tiro (la Main Camera)

    [Header("Que se puede golpear")]
    public LayerMask hitMask = ~0;    // ~0 = todas las capas (de momento, todo)

    [Header("Efecto de impacto")]
    public GameObject impactPrefab;   // marca que aparece donde pega el tiro
    public float impactLifetime = 5f; // segundos antes de que la marca se borre sola

    [Header("Municion")]
    public int magazineSize = 12;     // balas por cargador
    public float reloadTime = 1.5f;   // segundos que tarda la recarga

    private int currentAmmo;          // balas que quedan ahora mismo
    private bool isReloading;         // true mientras esta recargando

    // "Ventanitas" de solo-lectura para el HUD.
    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;

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
        // (Evita que un clic en un boton del menu dispare el arma.)
        if (Time.timeScale == 0f) return;

        // Mientras recarga, ignoramos disparo y recarga (no se puede hacer nada).
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

        // wasPressedThisFrame = se dispara UNA vez por clic (no mientras mantienes)
        if (mouse.leftButton.wasPressedThisFrame)
            Shoot();
    }

    void Shoot()
    {
        // Sin balas: no dispara, avisa y sugiere recargar.
        if (currentAmmo <= 0)
        {
            Debug.Log("Click! Sin municion (pulsa R para recargar)");
            return;
        }

        // Gastamos una bala.
        currentAmmo--;
        Debug.Log($"Disparo. Balas: {currentAmmo}/{magazineSize}");

        // 1) Origen y direccion del rayo: centro de la camara, hacia delante
        Vector3 origin = fpsCamera.transform.position;
        Vector3 direction = fpsCamera.transform.forward;

        // 2) Lanzar el rayo. Si golpea algo dentro del alcance, hit guarda la info.
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask))
        {
            // hit.collider = objeto golpeado, hit.point = punto exacto del impacto
            Debug.Log($"Impacto en: {hit.collider.name} (a {hit.distance:F1} m)");

            // Si lo golpeado tiene el componente EnemyHealth, le aplicamos dano.
            // GetComponent devuelve null si ese objeto no es un enemigo.
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            // Linea roja visible en la vista Scene durante 1 segundo (para depurar)
            Debug.DrawLine(origin, hit.point, Color.red, 1f);

            // Marca de impacto: si hay prefab asignado, lo creamos en el punto del golpe.
            if (impactPrefab != null)
            {
                // hit.normal = direccion "hacia afuera" de la superficie golpeada.
                // El disco (cilindro) tiene su cara plana en el eje Y, asi que
                // alineamos su Y con la normal para que quede tumbado sobre la superficie.
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                GameObject mark = Instantiate(impactPrefab, hit.point, rotation);

                // Pegamos la marca al objeto golpeado: si ese objeto se mueve o se
                // destruye (un enemigo al morir), la marca le acompana en vez de
                // quedar flotando. El "true" conserva posicion/rotacion/tamano en el
                // mundo pese a la escala del padre (p. ej. el suelo esta escalado x5).
                mark.transform.SetParent(hit.collider.transform, true);

                // La borramos sola pasados unos segundos para no llenar la escena.
                Destroy(mark, impactLifetime);
            }
        }
        else
        {
            // No golpeo nada: linea verde hacia el alcance maximo
            Debug.Log("Fallo (no golpeo nada)");
            Debug.DrawRay(origin, direction * range, Color.green, 1f);
        }
    }

    // Corrutina: se ejecuta "a trozos" en el tiempo. El yield pausa aqui
    // sin congelar el juego y reanuda cuando pasan los segundos indicados.
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Recargando...");

        yield return new WaitForSeconds(reloadTime); // espera sin bloquear el frame

        currentAmmo = magazineSize;                  // cargador lleno otra vez
        isReloading = false;
        Debug.Log($"Recargado. Balas: {currentAmmo}/{magazineSize}");
    }
}
