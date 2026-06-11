# Animaciones — Checklist (viewmodel FPS)

> Lista viva de animaciones del arma/brazos. Marcamos a medida que avanzamos.
> Rig: `BlenderWork/LVAA.blend` (LVA4). Export → `Assets/Characters/FPSArms/`.

## Por ARMA (cada arma necesita su set)

| Animación | Qué es | Estado (Pistola) | Prioridad |
|---|---|---|---|
| **Idle** | Quieta, respiración en loop | ✅ `Pistol_Idle` | — |
| **Draw / Equip** (sacar) | El arma sube al equiparla | 🟡 `Pistol_Draw` (scaffold, pulir) | Alta |
| **Holster** (guardar) | El arma baja al cambiar | 🟡 `Pistol_Holster` (scaffold, pulir) | Alta |
| **Fire / Shot** (disparo) | Kick del arma + corredera atrás 1 frame | ❌ falta | **Crítica** |
| **Reload** | Recarga normal (con cargador) | ✅ `Pistol_Reload` + `Handgun_Reload` | — |
| **Reload Empty** (sin munición) | Recarga con corredera trabada (rack) | ❌ falta | Alta |
| **Run / Sprint** | Pose/clip al correr con el arma | ❌ falta (solo idle) | Media |
| **Inspect** | Revisar el arma (flourish, no funcional) | ❌ falta | Baja |
| **Aim / ADS** | Apuntar con mira | ❌ falta — ¿lo usamos? (estilo SS no suele) | Opcional |

## Compartidas / personaje (desarmado y general)

| Animación | Qué es | Estado | Prioridad |
|---|---|---|---|
| Idle desarmado | Manos vacías, respiración | ✅ `Idle_Unarmed` | — |
| Run desarmado | Correr | ✅ `Run_Unarmed` | — |
| Climb (trepar borde) | Subir un borde | ✅ `Climb_Unarmed` | — |
| Carry Hold / Throw | Sostener / arrojar objeto | ✅ `Carry_Hold` / `Carry_Throw` | — |
| **Puñetazo** | Golpe desarmado | ❌ falta | Media |
| **Salto / Caída / Aterrizaje** | Manos en el aire | ❌ falta (cámara ya tiene bob) | Media |
| Culatazo / Melee con arma | Golpe rápido con el arma equipada | ❌ falta | Baja |

## Orden sugerido (lo que de verdad mueve la aguja)
1. **Fire/Shot pistola** — sin esto el arma no dispara visualmente. *Crítica.*
2. **Reload Empty pistola** — el "sin munición" que ya querías.
3. **Pulir Draw/Holster** — las poses de mano (ahora solo suben/bajan).
4. **Run con pistola** — para que no quede en idle al correr.
5. De ahí: repetir el set para la **2ª arma**, y las compartidas (puñetazo, salto).

## Notas de pipeline (no olvidar al exportar)
- **30 fps** la escena (no 24).
- Animar partes del arma como **objetos** (slide/mag), no huesos — OK.
- **Visibilidad** (mostrar/ocultar piezas): usar **escala 0/1**, NO `hide_viewport` (el hide no se exporta a FBX).
- **Nada de puntos** en nombres de objetos (rompe el FBX).
- **No animar el hueso `root`** para movimiento visible (Unity lo trata como root motion y lo descarta con `applyRootMotion=false`) — mover huesos hijos.
