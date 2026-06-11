using UnityEngine;
using UnityEngine.UIElements;

namespace ShooterDem
{
    /// <summary>
    /// Overlay de feedback de combate, dibujado con Painter2D (sin sprites):
    ///  - Indicadores DIRECCIONALES de daño (estilo CS): arcos rojos que apuntan al origen
    ///    del golpe y se desvanecen.
    ///  - HITMARKER: X breve en el centro al confirmar impacto sobre un enemigo.
    /// Sucesor de CrosshairArcs: los arcos de stats y la retícula murieron con el Camino A
    /// (la mira/munición las pone el pack; la vida, la caja SALUD del HUD).
    /// Lo crea HudController por código y lo añade encima del HUD.
    /// </summary>
    public class HitFeedback : VisualElement
    {
        // --- Daño direccional ---
        const int MaxDamage = 6;
        const float DamageLife = 1.3f;                      // segundos que dura cada indicador
        readonly float[] dmgAngle = new float[MaxDamage];   // angulo: 0=de frente, +=derecha
        readonly float[] dmgLife = new float[MaxDamage];    // 1..0 (se desvanece)

        // --- Hitmarker ---
        const float HitmarkerLife = 0.25f;   // segundos visible
        private float hitmarkerLife;          // 1..0

        static readonly Color Bone = new Color(0.843f, 0.910f, 0.816f);
        static readonly Color Warn = new Color(0.90f, 0.20f, 0.15f);

        public HitFeedback()
        {
            style.position = Position.Absolute;
            style.left = 0; style.top = 0; style.right = 0; style.bottom = 0;
            pickingMode = PickingMode.Ignore;
            generateVisualContent += OnGenerate;
            schedule.Execute(Tick).Every(16);
        }

        // Lo llama el HUD al recibir dano. angleFromFront: 0=de frente, +90=derecha,
        // -90=izquierda, +-180=a la espalda.
        public void AddDamage(float angleFromFront)
        {
            int slot = 0; float lowest = float.MaxValue;        // reusa el slot mas gastado
            for (int i = 0; i < MaxDamage; i++)
                if (dmgLife[i] < lowest) { lowest = dmgLife[i]; slot = i; }
            dmgAngle[slot] = angleFromFront;
            dmgLife[slot] = 1f;
            MarkDirtyRepaint();
        }

        // Lo llama el HUD cuando un disparo conecta con un enemigo: X breve en la mira.
        public void Hitmarker() { hitmarkerLife = 1f; MarkDirtyRepaint(); }

        // Decaimiento de los efectos (corre cada ~16ms; solo repinta si algo cambio).
        void Tick()
        {
            bool changed = false;

            for (int i = 0; i < MaxDamage; i++)
                if (dmgLife[i] > 0f)
                {
                    dmgLife[i] = Mathf.Max(0f, dmgLife[i] - 0.016f / DamageLife);
                    changed = true;
                }

            if (hitmarkerLife > 0f)
            {
                hitmarkerLife = Mathf.Max(0f, hitmarkerLife - 0.016f / HitmarkerLife);
                changed = true;
            }

            if (changed) MarkDirtyRepaint();
        }

        void OnGenerate(MeshGenerationContext mgc)
        {
            var p = mgc.painter2D;
            Vector2 c = contentRect.center;
            p.lineCap = LineCap.Round;
            DrawDamage(p, c);
            DrawHitmarker(p, c);
        }

        // Arcos rojos que apuntan al origen del dano reciente y se desvanecen.
        void DrawDamage(Painter2D p, Vector2 c)
        {
            // Radio amplio: hacia los laterales/bordes, NO pegado a la mira. Grande y grueso.
            float r = Mathf.Min(contentRect.width, contentRect.height) * 0.30f;
            const float half = 26f;
            p.lineWidth = 14f;
            for (int i = 0; i < MaxDamage; i++)
            {
                if (dmgLife[i] <= 0f) continue;
                // En pantalla Painter2D 0deg=derecha y crece en horario; "de frente"=arriba=270.
                float screen = 270f + dmgAngle[i];
                Color col = Warn; col.a = Mathf.Clamp01(dmgLife[i]);
                p.strokeColor = col;
                p.BeginPath();
                p.Arc(c, r, Deg(screen - half), Deg(screen + half));
                p.Stroke();
            }
        }

        // X (4 diagonales con hueco central) que parpadea al confirmar impacto y se desvanece.
        void DrawHitmarker(Painter2D p, Vector2 c)
        {
            if (hitmarkerLife <= 0f) return;
            Color col = Bone; col.a = Mathf.Clamp01(hitmarkerLife);
            p.strokeColor = col;
            p.lineWidth = 2.5f;
            const float gap = 5f, len = 6f, d = 0.70710678f;  // d = 1/sqrt(2) -> 45 grados
            HitTick(p, c,  d,  d, gap, len);
            HitTick(p, c, -d,  d, gap, len);
            HitTick(p, c,  d, -d, gap, len);
            HitTick(p, c, -d, -d, gap, len);
        }

        void HitTick(Painter2D p, Vector2 c, float dx, float dy, float gap, float len)
        {
            Vector2 dir = new Vector2(dx, dy);
            p.BeginPath();
            p.MoveTo(c + dir * gap);
            p.LineTo(c + dir * (gap + len));
            p.Stroke();
        }

        static Angle Deg(float d) => new Angle(d, AngleUnit.Degree);
    }
}
