using UnityEngine;

namespace ShooterDem
{
// Marcas de impacto (sangre) PEGADAS al cuerpo del enemigo: quads hijos en el punto de
// impacto que se mueven con el. Anillo con tope; se LIMPIAN al reciclar el enemigo del
// pool (OnEnable) para que no reaparezcan sobre el siguiente. Lo llama WeaponEffects al
// golpear a un enemigo. Va en el prefab del enemigo, con el prefab de decal asignado.
public class BodyDecals : MonoBehaviour
{
    public GameObject decalPrefab;       // quad de sangre (BloodDecal)
    public int maxPerBody = 6;           // tope de marcas por cuerpo
    public float size = 0.3f;            // tamano en mundo
    public float surfaceOffset = 0.02f;  // separacion del cuerpo (evita z-fighting)

    private GameObject[] ring;
    private int next;

    void Awake() { ring = new GameObject[Mathf.Max(1, maxPerBody)]; }

    // Al reactivarse desde el pool: fuera las marcas de la vida anterior.
    void OnEnable() { ClearAll(); }

    public void Add(Vector3 worldPos, Vector3 worldNormal)
    {
        if (decalPrefab == null || ring == null) return;
        if (worldNormal == Vector3.zero) worldNormal = (worldPos - transform.position).normalized;
        if (worldNormal == Vector3.zero) worldNormal = transform.forward;

        var go = ring[next];
        if (go == null) { go = Instantiate(decalPrefab, transform); ring[next] = go; }
        next = (next + 1) % ring.Length;

        go.transform.position = worldPos + worldNormal * surfaceOffset;
        go.transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), worldNormal)
                                * Quaternion.FromToRotation(Vector3.forward, worldNormal);
        // Tamano en MUNDO, compensando la escala del cuerpo (p.ej. el tanque es 1.6).
        float ls = transform.lossyScale.x;
        float s = ls > 0.001f ? size / ls : size;
        go.transform.localScale = new Vector3(s, s, s);
        go.SetActive(true);
    }

    void ClearAll()
    {
        if (ring == null) return;
        for (int i = 0; i < ring.Length; i++) if (ring[i] != null) ring[i].SetActive(false);
        next = 0;
    }
}
}
