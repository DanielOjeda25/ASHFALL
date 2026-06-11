using UnityEngine;

namespace ShooterDem
{
    /// <summary>
    /// "Aterrizaje": al caer y tocar el suelo, la cámara baja un toque y rebota (resorte
    /// amortiguado). El golpe escala con la velocidad de caída. Sirve para saltos, caídas
    /// y el aterrizaje tras la explosión de un barril.
    /// Va en la CÁMARA (hijo del player); lee la velocidad vertical del Rigidbody del player.
    /// </summary>
    public class LandingBob : MonoBehaviour
    {
        [Tooltip("Cuánto baja la cámara en el aterrizaje más fuerte (metros).")]
        public float maxDip = 0.12f;
        [Tooltip("Velocidad de caída mínima para que se note el rebote.")]
        public float minImpactSpeed = 3f;
        [Tooltip("Velocidad de caída para el rebote máximo (10 m/s ≈ caída de 5m).")]
        public float maxImpactSpeed = 10f;
        [Tooltip("Rigidez del resorte (más alto = vuelve más rápido).")]
        public float spring = 90f;
        [Tooltip("Amortiguación (más alto = menos rebote). Baja = se nota la 'recuperación'.")]
        public float damping = 9f;

        // Bus estatico: "el player aterrizo" (0..1 = fuerza del impacto). Lo escucha
        // PlayerAudio para el sonido de caida, sin cableado en el Inspector.
        public static event System.Action<float> Landed;

        // Offset vertical EXTRA de los ojos (lo escribe Movement al agacharse). Vive aqui
        // porque este script es el UNICO que escribe localPosition de la camara cada frame:
        // si el crouch la moviera por su cuenta, se pisarian.
        public static float ExtraEyeOffset;

        [Header("Bob al moverse (sincronizado con la anim de correr)")]
        [Tooltip("Amplitud del bamboleo a velocidad de CORRER (m).")]
        public float bobRunAmount = 0.035f;
        [Tooltip("PISADAS por segundo. Derivado de la anim: 2 zancadas / 0.6667s * timeScale " +
                 "del run en el Blend Tree (1.3) = 3.9. Si cambias el timeScale, recalcular.")]
        public float stepsPerSecond = 3.9f;
        [Tooltip("Velocidad donde ARRANCA el blend a run (= threshold idle del Blend Tree).")]
        public float blendStartSpeed = 4.3f;
        [Tooltip("Velocidad de run pleno (= threshold run del Blend Tree).")]
        public float blendEndSpeed = 6.8f;
        [Tooltip("Vaiven lateral como fraccion del vertical.")]
        [Range(0f, 1f)] public float swayFactor = 0.6f;

        [Header("Suavizado de escalones (la escalera se siente rampa)")]
        [Tooltip("Velocidad de recuperación del contra-offset (más alto = alcanza antes al cuerpo).")]
        public float stepSmoothing = 11f;
        [Tooltip("Techo del contra-offset (que la cámara nunca quede muy lejos del cuerpo).")]
        public float stepSmoothMax = 0.45f;

        private Rigidbody body;
        private CapsuleCollider bodyCapsule;   // fallback del chequeo de piso (raycast)
        private InfimaGames.LowPolyShooterPack.Movement movement;   // fuente de verdad del grounded
        private Vector3 baseLocalPos;
        private float offset, velocity, prevYVel;
        private float bobTimer, bobAmp;   // fase del paso + amplitud suavizada
        private float stepSmoothY;        // contra-offset vertical (amortigua escalones)
        private float prevBodyY;
        private bool prevGrounded, bodyYInit;

        // MISMA fuente de verdad que stamina/salto: el grounded del Movement (que ya filtra
        // paredes por normal). Fallback: raycast bajo la capsula si no hay Movement.
        bool IsGrounded()
        {
            if (movement != null) return movement.Grounded;
            if (bodyCapsule == null) return true;
            var b = bodyCapsule.bounds;
            return Physics.Raycast(b.center, Vector3.down, b.extents.y + 0.12f,
                                   ~0, QueryTriggerInteraction.Ignore);
        }

        void Start()
        {
            body = GetComponentInParent<Rigidbody>();
            bodyCapsule = GetComponentInParent<CapsuleCollider>();
            movement = GetComponentInParent<InfimaGames.LowPolyShooterPack.Movement>();
            baseLocalPos = transform.localPosition;
        }

        void LateUpdate()
        {
            float yv = body != null ? body.linearVelocity.y : 0f;

            // Aterrizaje: venía cayendo (prevYVel muy negativo) y se frenó de golpe.
            if (prevYVel < -minImpactSpeed && yv > prevYVel + 1.5f)
            {
                float impact = Mathf.InverseLerp(minImpactSpeed, maxImpactSpeed, -prevYVel);
                offset = -maxDip * impact;   // la cámara baja de golpe
                velocity = 0f;
                Landed?.Invoke(impact);      // avisa (sonido de caida, etc.)
            }
            prevYVel = yv;

            // Resorte amortiguado: el offset vuelve a 0 (baja y sube).
            float accel = -spring * offset - damping * velocity;
            velocity += accel * Time.deltaTime;
            offset += velocity * Time.deltaTime;

            // --- Bob de pasos: ESPEJO del Blend Tree de los brazos ---
            // La anim de correr NO cambia de velocidad con el movimiento (el blend cambia
            // PESOS, no ritmo) -> el bob tampoco: frecuencia CONSTANTE (= pisadas de la anim)
            // y la AMPLITUD sigue el mismo peso idle->run que el Blend Tree (4.3 a 6.8 m/s).
            // Asi caminar = sin bob (los brazos estan en idle) y esprintar = bob clavado.
            Vector3 bob = Vector3.zero;
            if (Time.timeScale > 0f)
            {
                Vector3 hv = body != null ? body.linearVelocity : Vector3.zero;
                hv.y = 0f;
                bool grounded = IsGrounded();   // chequeo real: sin bob en salto/caida/apex
                // Bob SOLO esprintando de verdad (fuente unica del Movement): caminar a 5 m/s
                // superaba el umbral de 4.3 y metia bob de carrera caminando (p.ej. agotado
                // con Shift apretado). El blend por velocidad sigue suavizando la entrada.
                bool sprinting = movement == null || movement.SprintingEffective;
                float runWeight = grounded && sprinting
                    ? Mathf.InverseLerp(blendStartSpeed, blendEndSpeed, hv.magnitude) : 0f;
                // entrada suave (3/s) pero corte RAPIDO al despegar (8/s): sin trote fantasma en el aire
                bobAmp = Mathf.MoveTowards(bobAmp, runWeight, Time.deltaTime * (grounded ? 3f : 8f));
                if (bobAmp > 0.001f)
                {
                    // fase en pasos: |sin| tiene periodo PI -> una hundida por pisada
                    bobTimer += Time.deltaTime * stepsPerSecond * Mathf.PI;
                    float a = bobRunAmount * bobAmp;
                    // vertical: 1 golpe por pisada · lateral: 1 vaiven por ciclo de brazos (2 pisadas)
                    bob = new Vector3(Mathf.Sin(bobTimer) * a * swayFactor,
                                      -Mathf.Abs(Mathf.Sin(bobTimer)) * a, 0f);
                }
            }

            // --- Suavizado de escalones: la escalera se SIENTE rampa ---
            // El step assist teletransporta el CUERPO 0.3m hacia arriba por peldano (un
            // Rigidbody no tiene step offset); la camara lo CONTRARRESTA en el momento y
            // despues desliza hasta alcanzarlo -> subida continua, sin saltos de vista.
            // Solo amortigua cambios estando en el piso de forma CONTINUA (subir/bajar
            // peldanos); saltos y aterrizajes siguen 1:1 (de eso se encarga el resorte).
            if (body != null && Time.timeScale > 0f)
            {
                float bodyY = body.transform.position.y;
                bool groundedNow = IsGrounded();
                if (!bodyYInit) { prevBodyY = bodyY; bodyYInit = true; }
                float dy = bodyY - prevBodyY;
                if (groundedNow && prevGrounded && Mathf.Abs(dy) > 0.005f && Mathf.Abs(dy) < 0.5f)
                    stepSmoothY = Mathf.Clamp(stepSmoothY - dy, -stepSmoothMax, stepSmoothMax);
                prevBodyY = bodyY;
                prevGrounded = groundedNow;
                // decaimiento exponencial: rapido al principio, suave al final
                stepSmoothY = Mathf.Lerp(stepSmoothY, 0f, 1f - Mathf.Exp(-stepSmoothing * Time.deltaTime));
            }

            transform.localPosition = baseLocalPos + Vector3.up * (offset + ExtraEyeOffset + stepSmoothY) + bob;
        }
    }
}
