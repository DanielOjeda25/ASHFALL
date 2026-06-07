# ROADMAP — Shooter Demo

Fases del proyecto. Un juego se construye **de adentro hacia afuera**: primero el
suelo, luego moverte, luego el arma, luego a quién dispararle, luego las reglas.

Leyenda: ✅ hecho · 🟡 en progreso · ⬜ pendiente

---

## Fase 0 — Setup ✅
- [x] Unity 6 (URP) + proyecto creado
- [x] Paquete MCP for Unity instalado (en `Packages/manifest.json`)
- [x] MCP conectado (server HTTP 8080 + registro global) — ver `docs/MCP_SETUP_UNITY.md`
- [x] Repo Git + remoto (`shooter-dem`)
- [x] Documentación base

## Fase 1 — El mundo ✅
- [x] Suelo (Plane) creado
- [x] Centrar el suelo en (0,0,0) y agrandarlo (scale 5,1,5 → 50×50)
- [x] Material/color al suelo (`Assets/Materials/Ground.mat`)
- [x] Entender y ajustar la luz (Directional Light); skybox por defecto (aporta luz ambiente)

## Fase 2 — El jugador ✅
- [x] Decidir **FPS o TPS** → **FPS** (cámara en los ojos)
- [x] GameObject del jugador (cápsula) + movimiento (WASD) — `PlayerMovement.cs` + CharacterController
- [x] Cámara en primera persona (Main Camera hija del Player) — `MouseLook.cs`
- [x] Input System (el nuevo: `Keyboard.current` / `Mouse.current`)

## Fase 3 — El arma ✅
- [x] Arma (placeholder) en el jugador — cubo alargado, hija de la Main Camera
- [x] Disparo por raycast + efecto de impacto — `Weapon.cs` + prefab `ImpactMark`
- [x] Munición — cargador (`magazineSize`) + recarga con R (corrutina `reloadTime`)

## Fase 4 — Los enemigos ✅
- [x] Enemigo (cápsula roja) + vida — `EnemyHealth.cs`
- [x] IA con NavMesh — `EnemyAI.cs` (persecución + ataque). NavMesh horneado con
  `NavMeshSurface` en el Plane; Player/Enemy excluidos con `NavMeshModifier`
  (Remove Object). _Patrulla pendiente (opcional)._
- [x] Daño mutuo — disparo baja vida del enemigo (`Weapon.damage`); el enemigo
  golpea al jugador al acercarse (`PlayerHealth.cs`, ataque con cooldown)

## Fase 5 — Las reglas ✅
- [x] Mira / crosshair (Canvas + Image circular, `Knob`)
- [x] HUD (vida, munición) — `HUD.cs` lee `PlayerHealth` y `Weapon`, textos TMP anclados a esquinas
- [x] Spawns de enemigos — `EnemySpawner.cs` instancia N copias del prefab `Enemy` en círculo
- [x] Victoria / derrota — `GameManager.cs` (singleton) cuenta enemigos; game over real
  (`Time.timeScale = 0` + cursor libre + panel `GameOverPanel`/`GameOverText`)
- [x] Menú de pausa (Esc) — `PausePanel` con botones Reanudar/Reiniciar/Salir; reinicio
  recarga la escena (`SceneManager.LoadScene`). `MouseLook` y `Weapon` ignoran input con
  `Time.timeScale == 0` (no mover cámara ni disparar en pausa/game over)

## Fase 6 — Pulido ✅
Orden acordado: **1) partículas → 2) sonidos → 3) animaciones** (las partículas no
dependen de assets externos; los sonidos necesitan clips que aporta el autor).
- [x] Partículas — **muzzle flash** (`MuzzleFlash`, hijo del arma; `Weapon.muzzleFlash.Play()`)
  + **chispas en el impacto** (prefab `Assets/Prefabs/ImpactSparks`, instanciado en `hit.point`;
  Stretched Billboard + Trails para look de chispa). Campos `muzzleFlash`/`impactSparks` en `Weapon.cs`.
- [x] Sonidos — `AudioSource` en `Weapon` (2D). Disparo (`fire1`), sin munición (`empty`),
  recarga (`reload`) vía `PlayOneShot`; impacto en pared (`concrete1..4`) vs enemigo (`flesh1..5`)
  elegido al azar. Clips en `Assets/Audio/`. Disparo semiautomático (1 tiro por clic, sin cadencia tope).
- [x] Recoil del arma — efecto procedural por código en `Weapon.cs`: al disparar la pose
  retrocede (`recoilKickback`, eje Z local) y vuelve suave en `LateUpdate` (offsets que decaen
  con `Lerp`). `recoilPitch` (cabeceo) disponible pero a 0 por decisión del autor (solo retroceso).

---

## Visión v2.0 — Arena horde shooter (estilo Serious Sam) 🎯
> **El Norte del proyecto.** Referencia explícita del autor: **Serious Sam** (First/Second
> Encounter). Objetivo: **mapas enormes** + **hordas masivas** de enemigos que rodean al
> jugador; combate de moverse sin parar (*backpedaling*) disparando a docenas a la vez.
> A partir de aquí, **toda decisión de diseño/arquitectura se evalúa por**: ¿escala a mapa
> grande + hordas? Se documenta como v2.0 pero marca el rumbo de cada paso de v1.

**Pilares para llegar ahí (fuera del alcance del v1 actual):**
- **Mapa-arena grande**: escenario amplio con cobertura, alturas y espacios abiertos para
  hordas. Modelado con **ProBuilder** (o malla externa) + **NavMesh horneado sobre área
  extensa** para que los enemigos rodeen; posibles transiciones entre zonas (triggers/puertas).
- **Hordas y oleadas**: ✅ *hecho* — `WaveSystem` híbrido (finito/infinito) con **pacing**
  (tope de vivos + spawn por tandas), **escalado de dificultad** (vida/velocidad por oleada) y
  **contador de oleada en HUD**. Falta: spawners por **zonas** del mapa grande.
- **Rendimiento para hordas** (será el tema central): ✅ *muy avanzado* — `EnemyAI` cachea al
  Player (`PlayerHealth.Current`) y hace *throttle* de repath; **object pooling** hecho para
  **enemigos** (`EnemyPool`) y **efectos de impacto** (`PrefabPool` genérico: chispas y marcas).
  Falta: límites de balas/sonidos simultáneos; pooling de proyectiles si hace falta.
- **Variedad de enemigos tipo SS**: melee que cargan (kamikaze/headless), *ranged*, distintos
  tamaños — más allá del enemigo-cápsula único actual.
- **Explosiones de área y props destructibles**: ✅ *base hecha* — `Projectile.cs` explota al
  impactar y aplica daño en área con `Physics.OverlapSphere` → `TakeDamage` a todo `IDamageable`
  del radio (con caída por distancia) + **knockback radial**. Usado por el arma en modo
  `Projectile` (bazooka). Falta: **efecto visual** de explosión (VFX) + **clip real** de sonido
  (ahora placeholder), y props destructibles (objetos `IDamageable` que al morir se cambian por
  escombros). Descartado: deformación real del terreno (caro + choca NavMesh).
- **Personajes y animaciones realistas (Blender → Unity)**: modelar/riggear/animar en
  Blender y exportar `.fbx` (malla + armature + clips). Dos frentes: **viewmodel de brazos+arma**
  para el FPS, y sobre todo **enemigos animados** (idle/andar/atacar/morir) — los que más se notan.
  En Unity: Rig Humanoid (permite reusar animaciones de **Mixamo**) + **Animator Controller**
  (máquina de estados con parámetros). El recoil procedural por código puede convivir encima de
  las animaciones o sustituirse por una animación de disparo. Reemplazaría las cápsulas placeholder.
- **Arsenal**: ✅ *hecho (base)* — armas **dirigidas por datos** (`WeaponData`: daño, falloff,
  cargador, audio por arma, `ejectsShell`, etc.) con **3 formas de disparo** (`Single` raycast,
  `Shotgun` N perdigones, `Projectile` con AoE). **`WeaponManager`** (inventario + cambio con
  1/2/3 o rueda, munición por arma + sonido de cambio) y assets **Pistola / Escopeta / Bazooka**
  funcionando. Falta: más armas, y **modelos/prefabs reales** (siguen placeholders).
- **Movimiento y feel**: ✅ *hecho* — sprint, salto y crouch sobre `CharacterController`;
  `MovementFeel` (cámara que se hunde al agacharse/esprintar, arma que se repliega).
- **Audio**: ✅ *hecho* — sonido **por arma** (en `WeaponData`), pasos/sprint (`PlayerFootsteps`),
  casquillos, cambio de arma, whoosh+explosión de bazooka. Audio organizado en carpetas.

---

### Hito actual
**v1 cerrado (Fases 0–6)** ✅ + **v2.0 en marcha**. Sobre el v1 (mundo, jugador FPS, arma,
enemigos por NavMesh, reglas/HUD/game over) ya hay buena parte de la Visión v2.0:
- **Movimiento**: sprint/salto/crouch + feel de cámara/arma.
- **Oleadas**: `WaveSystem` híbrido con pacing, escalado de dificultad y contador en HUD.
- **Rendimiento**: pooling de enemigos e impactos; `EnemyAI` cacheado + throttle.
- **Arsenal**: `WeaponManager` + Pistola/Escopeta/Bazooka data-driven; knockback; explosión AoE.
- **Audio**: por arma, pasos, casquillos, cambio de arma, bazooka.

**Siguiente (orden sugerido):** 2º **tipo de enemigo** (melee que carga/kamikaze) → **spawners
por zonas** → **mapa-arena grande**. Pendientes menores: VFX+clip real de explosión, props
destructibles, enemigos animados (Blender), más armas. **El mapa grande es el gran salto restante.**
