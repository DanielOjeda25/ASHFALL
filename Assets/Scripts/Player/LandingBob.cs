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
        [Tooltip("Velocidad de caída para el rebote máximo.")]
        public float maxImpactSpeed = 16f;
        [Tooltip("Rigidez del resorte (más alto = vuelve más rápido).")]
        public float spring = 140f;
        [Tooltip("Amortiguación (más alto = menos rebote).")]
        public float damping = 14f;

        // Bus estatico: "el player aterrizo" (0..1 = fuerza del impacto). Lo escucha
        // PlayerAudio para el sonido de caida, sin cableado en el Inspector.
        public static event System.Action<float> Landed;

        private Rigidbody body;
        private Vector3 baseLocalPos;
        private float offset, velocity, prevYVel;

        void Start()
        {
            body = GetComponentInParent<Rigidbody>();
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

            transform.localPosition = baseLocalPos + Vector3.up * offset;
        }
    }
}
