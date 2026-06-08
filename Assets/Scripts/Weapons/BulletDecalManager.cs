using UnityEngine;

namespace ShooterDem
{
// Agujeros de bala PERSISTENTES en geometria del mundo (pared/suelo). Singleton ligero
// sobre SurfaceDecalPool. Lo llama WeaponEffects al impactar el mundo (NO enemigos: esos
// vuelven al pool y la marca reaparecaria; en enemigos el feedback es la sangre).
// Va en un GameObject vacio "BulletDecalManager" con el prefab del agujero asignado.
public class BulletDecalManager : SurfaceDecalPool
{
    public static BulletDecalManager Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        base.Awake();
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}
}
