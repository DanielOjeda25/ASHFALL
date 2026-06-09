# AUDIO — Inventario y sonidos que faltan (ASHFALL)

> Documento vivo. Actualizado al cierre de la **sección de audio** (jun 2026): voces del
> jugador, audio de enemigos, UI, **música adaptativa de 2 capas** y fixes de pooling/pausa.

**Leyenda**: 🟡 Media · ⚪ Baja / pulido · ⏸️ en pausa (depende de otro sistema)

---

## ✅ Lo que YA tenemos

### Jugador (`PlayerAudio` — fuente 2D PROPIA, separada de los pasos del pack)
| Sonido | Clips | Detalle |
|---|---|---|
| Daño (voz) | `player_hurt1..3` | cooldown 0.35s anti-eco con varios enemigos |
| Muerte | `player_death1/2` (+ bonus 😄) | fuente propia → no lo corta el freeze del game over |
| Salto | `jump.wav` | recortado por análisis de samples (0.113s de silencio fuera); jump buffer 0.15s en `Movement` |
| Aterrizaje | `land.wav` | volumen según fuerza del golpe (`LandingBob.Landed`) |
| Dash | `dash_1.wav` | evento `Movement.Dashed` |
| Sin stamina | `no_stamina.wav` | evento `Movement.StaminaDenied` |
| Latido vida baja | `heartbeat.wav` | loop en fuente propia, activa < 30% HP, para al morir |
| Pasos | pack (`Movement`) + `Footsteps/` legado | el pack pausa su fuente con timeScale 0 |

### Enemigos (`EnemyAudio` por prefab — `AudioSource` 3D, cableado en los 4 tipos)
| Tipo | Clips |
|---|---|
| Común | `enemyHurt(2)` (daño), `enemyDeath` (muerte vía **`PooledSfx`**: sobrevive al reciclaje del pool) |
| Melee | `monsterAttack-mele` (golpe) |
| Kamikaze | `kamikazeIdle` (loop), `alert`+`scream` (al detectar, corta el idle), `kamikazeExplosion` (muerte = prefab `ExplosionKamikaze`) |
| Ranged | `enemyShoot` + `ranged` (variantes de disparo) |
| Tanque | `tank_growl1..3`, `tank_step1..3` |

### Combate / feedback
| Sonido | Detalle |
|---|---|
| Hitmarker | `UI/hitmarker.mp3` + **X** procedural en `CrosshairArcs`; bus `LpspBulletDamage.HitConfirmed` (funciona con las balas del pack); tic con cooldown 0.08s |
| Impacto en carne | `flesh1..8` + sangre + decal (`LpspBulletDamage`) |
| Explosión | `bazookaExplosion` (prefab `Explosion`), barriles del pack |
| Daño direccional | arcos rojos en `CrosshairArcs` (vuelto a activar, sin los arcos de stats) |

### UI (`UiAudio` en `PauseMenu_UITK` — `ignoreListenerPause`)
`click.wav` + `hover.wav` (todos los botones de pausa y game over) · `esc_sound.wav` (abrir/cerrar pausa).

### 🎵 Música adaptativa (`MusicManager`) — estilo Serious Sam
- `Music/peace-ambience.ogg` + `Music/fight-ambience-definitive.ogg` (generadas con IA,
  frase firma compartida, E frigia dominante, Streaming).
- **2 capas en loop permanente + crossfade de volúmenes** (1.5s). Señal: enemigos vivos
  (bus `EnemyHealth.Spawned/Killed`). Histéresis de 3s al limpiar la arena.
- **Pausa**: `AudioListener.pause` en `GameManager` (interruptor maestro; reset en
  `RestartGame` porque es estático y sobrevive recargas).
- Hueco listo para **stinger** de transición (campo `stingerClip`, opcional).

---

## ❌ Lo que FALTA

| Sonido | Dónde engancha | Prioridad |
|---|---|---|
| Proyectil del ranged: vuelo + impacto | `EnemyProjectile.cs` (sin audio) | 🟡 |
| Inicio de oleada / oleada superada | `WaveSystem` (eventos ya existen) | 🟡 |
| Victoria / derrota (stinger de fin) | `GameManager.TriggerVictory/Lose` | 🟡 |
| Stinger de transición de música | `MusicManager.stingerClip` (solo falta el clip) | 🎵⚪ |
| Agacharse / levantarse | `Movement` del pack | ⚪ |
| Pasos por superficie (ceniza/metal) | sistema de pasos | ⚪ |
| Ambiente de fondo (viento/ceniza) además de la música | loop 2D aparte | ⚪ |

### ⏸️ En pausa (Camino A archivó el arsenal propio)
Recarga/empty/silbido de **bazooka** y variantes de escopeta — vuelven cuando se
reconstruyan armas sobre el sistema del pack.

### Futuro (cuando existan los sistemas)
Pickups (munición/vida) · props destructibles (rotura).

---

## Notas
- **Formatos**: SFX cortos `.wav`/`.mp3` mono (3D posicional); música `.ogg` **Streaming**
  (MP3 mete silencio de padding y rompe loops).
- **Patrones del proyecto**: eventos/bus estático (sin cableado de Inspector) · `PooledSfx`
  para audio que debe sobrevivir a quien lo dispara · cooldowns anti-spam (hurt 0.35s,
  hitmarker 0.08s) · fuente propia por responsabilidad (voces / latido / pasos).
- **Fuentes gratis**: Freesound, Sonniss GDC, Mixkit, Kenney (UI). Música: Suno (no
  comercial en plan gratis — regenerar/licenciar si ASHFALL se publica).
