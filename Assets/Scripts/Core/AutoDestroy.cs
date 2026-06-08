using UnityEngine;

namespace ShooterDem
{
// Destruye el GameObject tras 'lifetime' segundos. Para efectos de un solo uso
// (explosiones, etc.) que no se reciclan. Reutilizable por cualquier VFX.
public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
}
