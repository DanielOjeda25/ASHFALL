using UnityEngine;
using InfimaGames.LowPolyShooterPack;

namespace ShooterDem
{
    /// <summary>
    /// PUENTE pack &lt;-&gt; viewmodel custom: reusa el sistema de disparo del Low Poly Shooter Pack
    /// (fire, munición, mira, daño, recarga) cuando se equipa un arma, dejando como VISUAL
    /// nuestros brazos propios. El arma del pack equipada (la Handgun, puesta como primer hijo
    /// del Inventory = arma por defecto) provee toda la lógica; su modelo se oculta.
    ///   - SetArmed(true)  → habilita Fire/Aim/Reload (UnarmedMode off) + prende la retícula.
    ///   - SetArmed(false) → desarmado.
    /// Lo llama <see cref="WeaponSwitch"/> al cambiar de slot. Va en el player.
    /// </summary>
    public class PackArming : MonoBehaviour
    {
        [Tooltip("UnarmedMode del player (habilita/deshabilita las acciones de arma del pack).")]
        public UnarmedMode unarmedMode;
        [Tooltip("CanvasSpawner del pack = la retícula. Su .enabled prende/apaga la mira.")]
        public Behaviour reticleCanvas;

        private InventoryBehaviour inventory;
        private bool modelsHidden;

        // En cuanto el Character inicializa el inventario (runtime), ocultamos los modelos de
        // armas del pack: el visual son SIEMPRE nuestros brazos. Reintenta hasta lograrlo.
        void Update()
        {
            if (modelsHidden || ServiceLocator.Current == null) return;
            var gms = ServiceLocator.Current.Get<IGameModeService>();
            var ch = gms != null ? gms.GetPlayerCharacter() : null;
            inventory = ch != null ? ch.GetInventory() : null;
            if (inventory == null) return;

            var invGo = (inventory as MonoBehaviour).gameObject;
            foreach (var r in invGo.GetComponentsInChildren<Renderer>(true))
                r.enabled = false;
            modelsHidden = true;
        }

        // armed = el slot equipado dispara (pistola); false = desarmado (manos).
        public void SetArmed(bool armed)
        {
            if (unarmedMode != null)
            {
                unarmedMode.unarmed = !armed;
                unarmedMode.Apply();
            }
            if (reticleCanvas != null) reticleCanvas.enabled = armed;
        }
    }
}
