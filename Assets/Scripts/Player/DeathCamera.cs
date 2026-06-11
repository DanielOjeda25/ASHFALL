using System.Collections;
using UnityEngine;

namespace ShooterDem
{
    /// <summary>
    /// Muerte procedural de cámara: DESPLOME LATERAL. Al morir, la cámara cae hasta la
    /// altura del piso mientras rueda de costado (lado al azar) con un rebote chico al
    /// tocar — "el cuerpo se desploma". Corre en tiempo UNSCALED porque el game over
    /// congela el juego (timeScale 0) justo al morir.
    /// Toma control exclusivo: apaga LandingBob y CameraShake (los otros escritores de
    /// la cámara); CameraLook ya lo apaga LpspGameStateFreeze en el game over.
    /// Va en la cámara base (junto a LandingBob). Se resetea solo al recargar la escena.
    /// </summary>
    public class DeathCamera : MonoBehaviour
    {
        [Tooltip("Duración de la caída (segundos reales, el juego ya está congelado).")]
        public float fallDuration = 0.9f;
        [Tooltip("Cuánto rueda la cabeza al costado (grados).")]
        public float rollAngle = 80f;
        [Tooltip("Altura final de los 'ojos' sobre el piso (m).")]
        public float floorHeight = 0.22f;
        [Tooltip("Rebote al tocar el piso (m).")]
        public float bounce = 0.05f;

        void OnEnable()  { PlayerHealth.PlayerDied += OnDied; }
        void OnDisable() { PlayerHealth.PlayerDied -= OnDied; }

        void OnDied()
        {
            // control exclusivo de la cámara durante el desplome
            var bob = GetComponent<LandingBob>();
            if (bob != null) bob.enabled = false;
            var shake = GetComponent<CameraShake>();
            if (shake != null) shake.enabled = false;

            StartCoroutine(Collapse());
        }

        IEnumerator Collapse()
        {
            float side = Random.value < 0.5f ? -1f : 1f;   // cae a izquierda o derecha
            Vector3 startPos = transform.localPosition;
            Quaternion startRot = transform.localRotation;

            // caída: desde los ojos hasta floorHeight sobre los PIES del cuerpo
            var capsule = GetComponentInParent<CapsuleCollider>();
            float feetY = capsule != null ? capsule.bounds.min.y : transform.position.y - 1.6f;
            float drop = Mathf.Max(0f, transform.position.y - (feetY + floorHeight));

            // de costado + un toque de cabeceo hacia adelante
            Quaternion targetRot = startRot * Quaternion.Euler(8f, 0f, rollAngle * side);

            float t = 0f;
            while (t < fallDuration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / fallDuration);
                float ease = k * k;   // ease-in: acelera como una caída real
                transform.localPosition = startPos + Vector3.down * (drop * ease);
                transform.localRotation = Quaternion.Slerp(startRot, targetRot, ease);
                yield return null;
            }

            // rebote chico al tocar el piso (vende el golpe)
            float bt = 0f;
            const float bounceDur = 0.18f;
            Vector3 floorPos = startPos + Vector3.down * drop;
            while (bt < bounceDur)
            {
                bt += Time.unscaledDeltaTime;
                float k = Mathf.Sin(Mathf.Clamp01(bt / bounceDur) * Mathf.PI);
                transform.localPosition = floorPos + Vector3.up * (bounce * k);
                yield return null;
            }
            transform.localPosition = floorPos;
            transform.localRotation = targetRot;
        }
    }
}
