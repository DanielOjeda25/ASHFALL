using UnityEngine;

namespace ShooterDem
{
// Pool de DECALS PERSISTENTES sobre superficies (quads). Mantiene un ANILLO de quads
// reutilizables: al superar 'maxDecals' recicla el mas viejo, asi nunca acumula infinito
// (clave para hordas: memoria y draw calls acotados). Base comun de los managers de
// sangre y de agujeros de bala (cada uno es un singleton ligero sobre esto).
public abstract class SurfaceDecalPool : MonoBehaviour
{
    [Header("Decal")]
    public GameObject decalPrefab;       // quad con el material del decal
    public int maxDecals = 60;           // tope simultaneo (anillo)
    public float minSize = 0.5f;         // variedad de tamano
    public float maxSize = 1.3f;
    public float surfaceOffset = 0.02f;  // separacion de la superficie (evita z-fighting)

    private GameObject[] ring;
    private int next;

    protected virtual void Awake()
    {
        ring = new GameObject[Mathf.Max(1, maxDecals)];
    }

    // Pone un decal en 'pos' apoyado sobre una superficie de normal 'normal'.
    public void Spawn(Vector3 pos, Vector3 normal)
    {
        if (decalPrefab == null || ring == null) return;
        if (normal == Vector3.zero) normal = Vector3.up;

        // Reutiliza el slot 'next' (instancia la 1a vez; luego recicla el mas viejo).
        var go = ring[next];
        if (go == null)
        {
            go = Instantiate(decalPrefab, transform);
            ring[next] = go;
        }
        next = (next + 1) % ring.Length;

        // Giro aleatorio alrededor de la normal (que no se vean clonados) + tamano variable.
        float spin = Random.Range(0f, 360f);
        float size = Random.Range(minSize, maxSize);
        Quaternion rot = Quaternion.AngleAxis(spin, normal) * Quaternion.FromToRotation(Vector3.forward, normal);

        var t = go.transform;
        t.SetPositionAndRotation(pos + normal * surfaceOffset, rot);
        t.localScale = new Vector3(size, size, size);
        go.SetActive(true);
    }
}
}
