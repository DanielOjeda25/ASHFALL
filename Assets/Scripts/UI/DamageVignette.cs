using UnityEngine;
using UnityEngine.UIElements;

namespace ShooterDem
{
// Vineta de dano: bordes rojos que PULSAN al recibir un golpe y un tinte rojo PERSISTENTE
// que crece cuando la vida baja (la pantalla "se va poniendo roja"). Se dibuja con 4 bandas
// de malla con color por vertice (opaco en el borde -> transparente hacia el centro), asi el
// centro queda despejado y se ve jugar. Va como overlay del HUD, detras del texto.
public class DamageVignette : VisualElement
{
    const float MaxAlpha = 0.55f;   // opacidad maxima en el borde
    const float BandFrac = 0.30f;   // grosor de cada banda (fraccion del lado)
    const float FlashTime = 0.5f;   // duracion del pulso al recibir dano (s)
    static readonly Color Red = new Color(0.72f, 0.05f, 0.05f);

    private float flash;       // pulso de golpe (1 -> 0)
    private float lowHealth;   // tinte persistente por vida baja (0..1)

    // Lo llama el HUD al recibir dano.
    public void Pulse() { flash = 1f; MarkDirtyRepaint(); }

    // Tinte de fondo segun lo herido que estes (0 = sano, 1 = al borde de morir).
    public float LowHealth
    {
        set
        {
            float v = Mathf.Clamp01(value);
            if (Mathf.Abs(v - lowHealth) > 0.001f) { lowHealth = v; MarkDirtyRepaint(); }
        }
    }

    public DamageVignette()
    {
        style.position = Position.Absolute;
        style.left = 0; style.top = 0; style.right = 0; style.bottom = 0;
        pickingMode = PickingMode.Ignore;
        generateVisualContent += OnGenerate;
        schedule.Execute(Tick).Every(16);
    }

    void Tick()
    {
        if (flash > 0.001f)
        {
            flash = Mathf.Max(0f, flash - 0.016f / FlashTime);
            MarkDirtyRepaint();
        }
    }

    void OnGenerate(MeshGenerationContext mgc)
    {
        float intensity = Mathf.Clamp01(Mathf.Max(flash, lowHealth));
        if (intensity <= 0.001f) return;

        Rect r = contentRect;
        float a = MaxAlpha * intensity;
        float bw = r.width * BandFrac;
        float bh = r.height * BandFrac;
        Color edge = new Color(Red.r, Red.g, Red.b, a);
        Color clear = new Color(Red.r, Red.g, Red.b, 0f);

        // Cada banda: lado del borde opaco -> lado interior transparente.
        Band(mgc, new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
                  new Vector2(r.xMax, r.yMin + bh), new Vector2(r.xMin, r.yMin + bh), edge, clear);   // arriba
        Band(mgc, new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax),
                  new Vector2(r.xMin, r.yMax - bh), new Vector2(r.xMax, r.yMax - bh), edge, clear);   // abajo
        Band(mgc, new Vector2(r.xMin, r.yMax), new Vector2(r.xMin, r.yMin),
                  new Vector2(r.xMin + bw, r.yMin), new Vector2(r.xMin + bw, r.yMax), edge, clear);   // izquierda
        Band(mgc, new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax),
                  new Vector2(r.xMax - bw, r.yMax), new Vector2(r.xMax - bw, r.yMin), edge, clear);   // derecha
    }

    // Quad: v0,v1 = lado del borde (color 'cEdge'); v2,v3 = lado interior (color 'cInner').
    void Band(MeshGenerationContext mgc, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color cEdge, Color cInner)
    {
        MeshWriteData mw = mgc.Allocate(4, 6);
        mw.SetNextVertex(new Vertex { position = new Vector3(v0.x, v0.y, Vertex.nearZ), tint = cEdge });
        mw.SetNextVertex(new Vertex { position = new Vector3(v1.x, v1.y, Vertex.nearZ), tint = cEdge });
        mw.SetNextVertex(new Vertex { position = new Vector3(v2.x, v2.y, Vertex.nearZ), tint = cInner });
        mw.SetNextVertex(new Vertex { position = new Vector3(v3.x, v3.y, Vertex.nearZ), tint = cInner });
        mw.SetNextIndex(0); mw.SetNextIndex(1); mw.SetNextIndex(2);
        mw.SetNextIndex(0); mw.SetNextIndex(2); mw.SetNextIndex(3);
    }
}
}
