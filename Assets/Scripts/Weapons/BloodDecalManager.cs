using UnityEngine;

namespace ShooterDem
{
// Manchas de sangre PERSISTENTES en el suelo. Singleton ligero sobre SurfaceDecalPool.
// Lo llama WeaponEffects cuando la sangre toca el suelo. Va en un GameObject vacio
// "BloodDecalManager" con el prefab del decal de sangre asignado.
public class BloodDecalManager : SurfaceDecalPool
{
    public static BloodDecalManager Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        base.Awake();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
}
