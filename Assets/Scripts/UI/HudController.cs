using UnityEngine;
using UnityEngine.UIElements;

namespace ShooterDem
{
// Controlador del HUD en UI Toolkit. Consulta los elementos del UXML por nombre y los
// actualiza reaccionando a EVENTOS del juego (patron observador, como todo el proyecto):
// vida, oleada, dano direccional, hitmarker y prompt de agarre. La municion, el nombre
// del arma y la stamina llegan del arma/movimiento del PACK via LpspHudAmmo (metodos
// Set*External / SetStamina01). Va en el GameObject con el UIDocument (HUD_UITK).
[RequireComponent(typeof(UIDocument))]
public class HudController : MonoBehaviour
{
    [Header("Fuentes de datos")]
    public PlayerHealth playerHealth;
    public WaveSystem waveSystem;

    [Header("Feedback de dano")]
    public float playerHitShake = 0.35f;   // sacudida de camara al recibir dano

    [Header("Hitmarker")]
    public AudioClip hitmarkerClip;        // tic 2D al confirmar impacto en un enemigo
    private AudioSource hitAudio;          // fuente 2D (se crea sola en Start)
    // Anti-spam del tic por TIEMPO: N impactos casi simultaneos (perdigones) = 1 solo tic.
    private float lastHitmarkerTime;
    const float HitmarkerSoundCooldown = 0.08f;

    private Label healthValue, ammoValue, weaponName, waveValue;
    private VisualElement staminaFill;   // barra de stamina (sprint/dash)
    private VisualElement ammoPanel;     // caja de municion (se oculta en modo desarmado)
    private HitFeedback hitFeedback;     // overlay: dano direccional + X de hitmarker
    private DamageVignette vignette;     // bordes rojos al recibir dano / vida baja
    private Camera cam;                  // para calcular la direccion del dano (cacheada)
    private PhysicsCarry physicsCarry;   // estado del agarre fisico (prompt E/click)
    private Label carryPrompt;           // texto "E AGARRAR" bajo la mira

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.Damaged += OnHealthChanged;
        if (playerHealth != null) playerHealth.Hit += OnPlayerHit;
        if (waveSystem != null) waveSystem.WaveChanged += OnWaveChanged;
        // Balas del pack (Camino A): bus estatico, no necesita referencia en el Inspector.
        LpspBulletDamage.HitConfirmed += OnHitConfirmed;
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.Damaged -= OnHealthChanged;
        if (playerHealth != null) playerHealth.Hit -= OnPlayerHit;
        if (waveSystem != null) waveSystem.WaveChanged -= OnWaveChanged;
        LpspBulletDamage.HitConfirmed -= OnHitConfirmed;
    }

    // Al recibir dano: arco rojo apuntando al origen + sacudida de camara.
    void OnPlayerHit(Vector3 source)
    {
        if (cam == null) cam = Camera.main;
        if (cam != null && hitFeedback != null)
        {
            Vector3 to = source - cam.transform.position; to.y = 0f;
            Vector3 fwd = cam.transform.forward; fwd.y = 0f;
            if (to.sqrMagnitude > 0.0001f && fwd.sqrMagnitude > 0.0001f)
                hitFeedback.AddDamage(Vector3.SignedAngle(fwd, to, Vector3.up));  // 0=frente, +=derecha
        }
        if (vignette != null) vignette.Pulse();   // borde rojo al recibir dano
        CameraShake.Add(playerHitShake);
    }

    // Impacto confirmado sobre un enemigo: X en la mira + tic 2D. La X puede repetirse
    // sin coste; el SONIDO respeta un cooldown corto (varios impactos casi simultaneos
    // = 1 tic, no una rafaga).
    void OnHitConfirmed()
    {
        if (hitFeedback != null) hitFeedback.Hitmarker();
        if (hitmarkerClip != null && hitAudio != null
            && Time.time >= lastHitmarkerTime + HitmarkerSoundCooldown)
        {
            hitAudio.PlayOneShot(hitmarkerClip);
            lastHitmarkerTime = Time.time;
        }
    }

    void Start()
    {
        // El UIDocument construye su arbol en su OnEnable; Start corre despues, asi
        // que aqui rootVisualElement ya esta listo para consultar.
        var root = GetComponent<UIDocument>().rootVisualElement;
        healthValue = root.Q<Label>("health-value");
        ammoValue = root.Q<Label>("ammo-value");
        weaponName = root.Q<Label>("weapon-name");
        waveValue = root.Q<Label>("wave-value");
        staminaFill = root.Q<VisualElement>("stamina-fill");
        ammoPanel = root.Q<VisualElement>("ammo-panel");
        // Estado inicial segun el modo desarmado (independiente del orden de Start).
        var unarmedMode = FindAnyObjectByType<UnarmedMode>();
        if (unarmedMode != null) SetAmmoPanelVisible(!unarmedMode.unarmed);

        // Vineta de dano: overlay DETRAS del texto del HUD (Insert(0)).
        vignette = new DamageVignette();
        root.Insert(0, vignette);

        // Overlay de feedback de combate (dano direccional + X de hitmarker), encima.
        hitFeedback = new HitFeedback();
        root.Add(hitFeedback);

        // Fuente 2D para el hitmarker (la creamos si el GameObject no tiene AudioSource).
        hitAudio = GetComponent<AudioSource>();
        if (hitAudio == null) hitAudio = gameObject.AddComponent<AudioSource>();
        hitAudio.playOnAwake = false;
        hitAudio.spatialBlend = 0f;   // 2D: feedback directo para el jugador

        // Prompt del agarre fisico ("E AGARRAR" / "CLICK ARROJAR · E SOLTAR"): un Label
        // por codigo, centrado bajo la mira; aparece solo cuando PhysicsCarry lo pide.
        physicsCarry = FindAnyObjectByType<PhysicsCarry>();
        carryPrompt = new Label();
        carryPrompt.pickingMode = PickingMode.Ignore;
        carryPrompt.style.position = Position.Absolute;
        carryPrompt.style.left = 0;
        carryPrompt.style.right = 0;
        carryPrompt.style.top = Length.Percent(58);   // bajo la mira, sin taparla
        carryPrompt.style.unityTextAlign = TextAnchor.MiddleCenter;
        carryPrompt.style.fontSize = 14;
        carryPrompt.style.letterSpacing = 3;
        carryPrompt.style.color = new Color(0.843f, 0.910f, 0.816f, 0.85f);   // Bone del HUD
        carryPrompt.style.display = DisplayStyle.None;
        root.Add(carryPrompt);

        RefreshHealth();
        RefreshWave();
    }

    void Update()
    {
        // Prompt del agarre: refleja el estado real de PhysicsCarry (misma regla que E).
        if (carryPrompt == null || physicsCarry == null) return;
        switch (physicsCarry.State)
        {
            case PhysicsCarry.CarryState.CanGrab:
                carryPrompt.text = "[ E ]  AGARRAR";
                carryPrompt.style.display = DisplayStyle.Flex;
                break;
            case PhysicsCarry.CarryState.Holding:
                carryPrompt.text = "[ CLICK ]  ARROJAR      [ E ]  SOLTAR";
                carryPrompt.style.display = DisplayStyle.Flex;
                break;
            default:
                carryPrompt.style.display = DisplayStyle.None;
                break;
        }
    }

    void OnHealthChanged(int current, int max) => RefreshHealth();

    void RefreshHealth()
    {
        if (playerHealth == null) return;

        int cur = playerHealth.CurrentHealth, max = playerHealth.maxHealth;
        float pct = max > 0 ? Mathf.Clamp01((float)cur / max) : 0f;

        if (healthValue != null) healthValue.text = cur.ToString();
        // Tinte rojo persistente: empieza por debajo del 50% de vida, maximo al borde de morir.
        if (vignette != null) vignette.LowHealth = 1f - Mathf.Clamp01(pct / 0.5f);
    }

    // ---------- API para LpspHudAmmo (datos que vienen del arma/movimiento del pack) ----------

    // Municion del arma del pack: "actual / cargador".
    public void SetAmmoExternal(int current, int magazine)
    {
        if (ammoValue != null) ammoValue.text = $"{current} / {magazine}";
    }

    // Nombre del arma del pack.
    public void SetWeaponNameExternal(string name)
    {
        if (weaponName != null) weaponName.text = name;
    }

    // Mostrar/ocultar la caja de municion (modo desarmado: jugando con las manos).
    public void SetAmmoPanelVisible(bool visible)
    {
        if (ammoPanel != null)
            ammoPanel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // Stamina (0..1) del Movement del pack.
    public void SetStamina01(float value)
    {
        if (staminaFill != null)
            staminaFill.style.width = Length.Percent(Mathf.Clamp01(value) * 100f);
    }

    // ---------- Oleadas ----------

    void OnWaveChanged(int wave) => RefreshWave();

    void RefreshWave()
    {
        if (waveSystem == null || waveValue == null) return;
        int w = waveSystem.CurrentWave;
        waveValue.text = w <= 0
            ? "PREPARATE"
            : (waveSystem.totalWaves > 0 ? $"OLEADA {w}/{waveSystem.totalWaves}" : $"OLEADA {w}");
    }
}
}
