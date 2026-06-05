using UnityEngine;

// Efectos visuales del arma: fogonazo al disparar y chispas + marca en el impacto.
// Escucha los eventos de Weapon. Va en el mismo GameObject que Weapon.
[RequireComponent(typeof(Weapon))]
public class WeaponEffects : MonoBehaviour
{
    [Header("Disparo")]
    public ParticleSystem muzzleFlash;   // fogonazo en la punta (hijo del arma)

    [Header("Impacto")]
    public GameObject impactSparks;      // prefab de chispas (esfera de particulas)
    public GameObject impactMark;        // prefab de marca/decal
    public float impactLifetime = 5f;    // segundos antes de borrar la marca

    private Weapon weapon;

    void Awake()
    {
        weapon = GetComponent<Weapon>();
    }

    void OnEnable()  { weapon.Fired += HandleFired; weapon.Hit += HandleHit; }
    void OnDisable() { weapon.Fired -= HandleFired; weapon.Hit -= HandleHit; }

    void HandleFired()
    {
        if (muzzleFlash != null)
            muzzleFlash.Play();
    }

    void HandleHit(RaycastHit hit, bool hitDamageable)
    {
        // Chispas: esfera de particulas en el punto, sin orientar. Se autodestruye.
        if (impactSparks != null)
        {
            GameObject sparks = Instantiate(impactSparks, hit.point, Quaternion.identity);
            Destroy(sparks, 1f);
        }

        // Marca: alineada con la normal de la superficie y pegada al objeto golpeado.
        if (impactMark != null)
        {
            Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
            GameObject mark = Instantiate(impactMark, hit.point, rot);
            mark.transform.SetParent(hit.collider.transform, true);
            Destroy(mark, impactLifetime);
        }
    }
}
