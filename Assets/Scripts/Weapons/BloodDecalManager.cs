using UnityEngine;

namespace ShooterDem
{
// Manchas de sangre PERSISTENTES en el suelo (decals tipo quad). Mantiene un ANILLO de
// quads reutilizables: al superar 'maxDecals' recicla el mas viejo, asi nunca acumula
// infinito (clave para hordas: memoria y draw calls acotados). Lo llama WeaponEffects
// cuando la sangre toca el suelo.
//
// Va en un GameObject vacio "BloodDecalManager" con el prefab del decal asignado.
public class BloodDecalManager : MonoBehaviour
{
    public static BloodDecalManager Instance { get; private set; }

    [Header("Decal")]
    public GameObject decalPrefab;       // quad con material de sangre
    public int maxDecals = 60;           // tope simultaneo (anillo)
    public float minSize = 0.5f;         // variedad de tamano
    public float maxSize = 1.3f;
    public float surfaceOffset = 0.02f;  // separacion del suelo (evita z-fighting)

    private GameObject[] ring;
    private int next;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ring = new GameObject[Mathf.Max(1, maxDecals)];
    }

    void OnDestroy() { if (Instance == this) Instance = null; }

    // Pone una mancha en 'pos' apoyada sobre una superficie de normal 'normal'.
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

        // Giro aleatorio alrededor de la normal (que no se vean clonadas) + tamano variable.
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
