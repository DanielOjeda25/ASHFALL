using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShooterDem
{
    /// <summary>
    /// Trepado de bordes (mantle): cerca de un muro con una plataforma encima, ESPACIO
    /// detecta el borde y sube al jugador con una curva fluida (subir → adelante).
    /// Detección con 2 raycasts: pecho→pared y, desde encima de la pared, hacia abajo
    /// para encontrar el "techo" plano. Durante la subida el Movement queda suspendido
    /// y el rigidbody en kinemático (nada de física peleando con la curva).
    /// Va en el GameObject raíz del player (junto a Rigidbody + CapsuleCollider + Movement).
    /// </summary>
    public class LedgeClimb : MonoBehaviour
    {
        [Header("Detección")]
        [Tooltip("Distancia máxima a la pared para agarrarse (desde el borde de la cápsula).")]
        public float maxGrabDistance = 0.9f;
        [Tooltip("Altura mínima del borde sobre los pies (menos que esto = es un escalón, no hace falta).")]
        public float minLedgeHeight = 0.9f;
        [Tooltip("Altura máxima alcanzable (plataformas más altas que el jugador).")]
        public float maxLedgeHeight = 2.4f;
        [Tooltip("Qué capas se pueden trepar (el layer propio se excluye solo).")]
        public LayerMask climbMask = ~0;

        [Header("Subida")]
        [Tooltip("Duración total del mantle (segundos).")]
        public float climbDuration = 0.55f;

        [Header("Vault (obstáculos bajos a sprint)")]
        [Tooltip("Hasta esta altura, ESPRINTANDO, el trepado es un vault rápido que CONSERVA el impulso.")]
        public float vaultMaxHeight = 1.2f;
        [Tooltip("Duración del vault (más corto que el mantle: no corta el flow).")]
        public float vaultDuration = 0.32f;

        // Bus estático: "empezó un trepado" (para anim del viewmodel / sonido a futuro).
        public static event System.Action ClimbStarted;

        // Alturas (sobre los pies) a las que se busca pared: pecho para muros altos,
        // rodilla para obstáculos bajos (cubos ~1m que el rayo del pecho no ve).
        static readonly float[] WallProbeHeights = { 1.2f, 0.45f };

        private Rigidbody body;
        private CapsuleCollider capsule;
        private InfimaGames.LowPolyShooterPack.Movement movement;
        private InputAction jumpAction;
        private bool climbing;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
            movement = GetComponent<InfimaGames.LowPolyShooterPack.Movement>();
        }

        void Start()
        {
            var playerInput = GetComponentInChildren<PlayerInput>(true);
            if (playerInput != null) jumpAction = playerInput.actions.FindAction("Jump");
        }

        void Update()
        {
            if (climbing || Time.timeScale <= 0f) return;

            // Manual: Espacio cerca de un borde alcanzable.
            if (jumpAction != null && jumpAction.WasPressedThisFrame())
            {
                TryClimb();
                return;
            }

            // AUTO-AGARRE en el aire: saltaste hacia un muro alto -> engancha solo cuando
            // el borde entra en rango (cerca del apex). Sin segundo boton.
            if (!IsGrounded() && body.linearVelocity.y < 2f)
            {
                Vector3 hv = body.linearVelocity; hv.y = 0f;
                Vector3 fwd = transform.forward; fwd.y = 0f;
                // solo si te MOVES hacia donde miras (evita agarres accidentales de espaldas)
                if (Vector3.Dot(hv, fwd.normalized) > 0.3f || hv.magnitude < 0.1f)
                    TryClimb();
            }
        }

        // FUENTE UNICA de verdad del piso: el Movement (filtra paredes/rampas/salto).
        // Antes habia un raycast propio aqui que duplicaba la regla y podia discrepar.
        bool IsGrounded() => movement == null || movement.Grounded;

        void TryClimb()
        {
            int mask = climbMask & ~(1 << gameObject.layer);   // nunca treparse a si mismo
            Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
            float feetY = capsule.bounds.min.y;

            // 1) ¿hay PARED al frente? Sondeamos a DOS alturas: pecho (muros altos) y
            // rodilla (cubos/cajones de ~1m — el rayo del pecho les pasaba POR ENCIMA y
            // por eso no se podian trepar). Gana el primer rayo que pegue.
            // SphereCast (no rayo fino): los bordes angostos o en ángulo que un rayo
            // esquivaba, una esfera de 15cm los agarra.
            RaycastHit wall = default;
            bool found = false;
            foreach (float probeHeight in WallProbeHeights)
            {
                Vector3 origin = new Vector3(transform.position.x, feetY + probeHeight, transform.position.z);
                if (Physics.SphereCast(origin, 0.15f, fwd, out wall, maxGrabDistance + capsule.radius, mask, QueryTriggerInteraction.Ignore))
                {
                    found = true;
                    break;
                }
            }
            if (!found) return;

            // Direccion de TREPADO = perpendicular al muro (no hacia donde miras): el cuerpo
            // sube derecho contra la pared y las manos quedan SOBRE el borde, no en diagonal.
            Vector3 climbDir = -wall.normal; climbDir.y = 0f;
            if (climbDir.sqrMagnitude < 0.01f) climbDir = fwd;
            climbDir.Normalize();

            // 2) ¿hay TECHO plano encima de esa pared? (rayo hacia abajo, un poco metido)
            Vector3 above = wall.point + climbDir * 0.15f;
            above.y = feetY + maxLedgeHeight + 0.1f;
            if (!Physics.Raycast(above, Vector3.down, out var top, maxLedgeHeight + 0.1f, mask, QueryTriggerInteraction.Ignore))
                return;
            if (top.normal.y < 0.6f) return;             // superficie inclinada: no es un borde

            float h = top.point.y - feetY;
            if (h < minLedgeHeight || h > maxLedgeHeight) return;

            // 3) ¿hay LUGAR para pararse arriba? (cápsula fantasma en el destino)
            Vector3 stand = top.point + climbDir * 0.25f;
            Vector3 capBottom = stand + Vector3.up * (capsule.radius + 0.05f);
            Vector3 capTop = stand + Vector3.up * (capsule.height - capsule.radius + 0.05f);
            if (Physics.CheckCapsule(capBottom, capTop, capsule.radius * 0.9f, mask, QueryTriggerInteraction.Ignore))
                return;

            // punto de AGARRE: pegado al muro (el cuerpo se aprieta antes de subir)
            Vector3 hang = wall.point - climbDir * (capsule.radius + 0.06f);
            hang.y = transform.position.y;

            // VAULT: obstaculo bajo + esprintando -> trepado rapido que CONSERVA el impulso
            // (pasas por encima sin cortar el flow). Mantle clasico para lo demas.
            bool vault = h <= vaultMaxHeight && movement != null && movement.SprintHeld;
            float duration = vault ? vaultDuration : climbDuration;
            Vector3 exitVelocity = Vector3.zero;
            if (vault)
            {
                Vector3 hv = body.linearVelocity; hv.y = 0f;
                exitVelocity = climbDir * Mathf.Max(hv.magnitude, 4f);   // sale al otro lado con impulso
            }

            StartCoroutine(Climb(hang, stand, duration, exitVelocity));
        }

        IEnumerator Climb(Vector3 hang, Vector3 stand, float climbTime, Vector3 exitVelocity)
        {
            climbing = true;
            movement.Suspended = true;
            body.linearVelocity = Vector3.zero;
            body.isKinematic = true;
            ClimbStarted?.Invoke();

            // destino: transform tal que los PIES queden sobre el borde
            float feetOffset = transform.position.y - capsule.bounds.min.y;
            Vector3 start = transform.position;
            Vector3 target = stand + Vector3.up * (feetOffset + 0.02f);
            Vector3 mid = new Vector3(hang.x, target.y + 0.05f, hang.z);   // sube PEGADO al muro

            // fase 0 (8%): AGARRE — el cuerpo se aprieta contra el muro (vende el grab)
            float grabDur = climbTime * 0.15f, upDur = climbTime * 0.50f, fwdDur = climbTime * 0.35f, t = 0f;
            while (t < grabDur)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, hang, Smooth(t / grabDur));
                yield return null;
            }
            t = 0f;
            while (t < upDur)    // fase 1: subir en vertical, al ras de la pared
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(hang, mid, Smooth(t / upDur));
                yield return null;
            }
            t = 0f;
            while (t < fwdDur)   // fase 2: adelante, sobre la plataforma
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(mid, target, Smooth(t / fwdDur));
                yield return null;
            }
            transform.position = target;

            body.isKinematic = false;
            body.linearVelocity = exitVelocity;   // vault: sigue de largo; mantle: queda quieto
            movement.Suspended = false;
            climbing = false;
        }

        static float Smooth(float x) => x * x * (3f - 2f * x);   // easing suave (smoothstep)
    }
}
