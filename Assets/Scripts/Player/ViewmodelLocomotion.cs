using UnityEngine;

namespace ShooterDem
{
    /// <summary>
    /// Alimenta el Animator del viewmodel (brazos propios) con la velocidad REAL del
    /// player: el Blend Tree "Locomotion" mezcla Idle (0) y Run (runSpeedReference) solo.
    /// Lee el Rigidbody del player (padre) — sin acoplarse al codigo del pack.
    /// Va en el objeto de los brazos (junto al Animator).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class ViewmodelLocomotion : MonoBehaviour
    {
        [Tooltip("Suavizado del parametro Speed (mas alto = reacciona mas rapido).")]
        public float damping = 8f;

        private Animator animator;
        private Rigidbody body;
        private float speed;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int ClimbHash = Animator.StringToHash("Climb");
        private static readonly int CarryingHash = Animator.StringToHash("Carrying");
        private static readonly int ThrowHash = Animator.StringToHash("Throw");

        private PhysicsCarry carry;   // estado del agarre fisico (manos en pose de sosten)

        void Awake()
        {
            animator = GetComponent<Animator>();
            body = GetComponentInParent<Rigidbody>();
            carry = GetComponentInParent<PhysicsCarry>();
        }

        // Buses estaticos: trepado (LedgeClimb) y lanzamiento (PhysicsCarry).
        void OnEnable()
        {
            LedgeClimb.ClimbStarted += OnClimb;
            PhysicsCarry.Thrown += OnThrow;
        }
        void OnDisable()
        {
            LedgeClimb.ClimbStarted -= OnClimb;
            PhysicsCarry.Thrown -= OnThrow;
        }
        void OnClimb() { animator.SetTrigger(ClimbHash); }
        void OnThrow() { animator.SetTrigger(ThrowHash); }

        void Update()
        {
            if (body == null || Time.timeScale <= 0f) return;
            Vector3 hv = body.linearVelocity;
            hv.y = 0f;
            // suavizado: el blend no salta de golpe al frenar/arrancar
            speed = Mathf.Lerp(speed, hv.magnitude, Time.deltaTime * damping);
            animator.SetFloat(SpeedHash, speed);

            // sosteniendo un objeto: manos en pose de carry (estado real de PhysicsCarry)
            if (carry != null)
                animator.SetBool(CarryingHash, carry.State == PhysicsCarry.CarryState.Holding);
        }
    }
}
