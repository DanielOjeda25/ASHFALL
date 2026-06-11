using UnityEngine;
using UnityEngine.InputSystem;

namespace ShooterDem
{
    /// <summary>
    /// Viewmodel de PISTOLA: maneja el Animator de brazos+arma (AC_Pistol). El idle
    /// loopea solo; R dispara el trigger de RECARGA (el clip mueve manos + corredera +
    /// cargador, animados como objetos desde Blender). Primer test de arma animada:
    /// el disparo y la munición se cablean después.
    /// Va en el GameObject del viewmodel de pistola (junto al Animator).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PistolViewmodel : MonoBehaviour
    {
        private Animator animator;
        private static readonly int ReloadHash = Animator.StringToHash("Reload");

        void Awake() { animator = GetComponent<Animator>(); }

        void Update()
        {
            if (Time.timeScale <= 0f) return;
            var kb = Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame)
                animator.SetTrigger(ReloadHash);
        }
    }
}
