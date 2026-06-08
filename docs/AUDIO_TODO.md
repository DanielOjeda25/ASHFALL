# AUDIO — Inventario y sonidos que faltan (ASHFALL)

> Documento vivo. Lista lo que **ya tenemos** y lo que **falta** para el sabor de
> *horde shooter* con atmósfera. Generado revisando `Assets/Audio/` + los scripts.

**Leyenda de prioridad**
🔴 Alta (falta feedback que se nota al jugar) · 🟡 Media · ⚪ Baja / pulido · 🎵 Ambiente/música

**Leyenda de "Falta"**
- `clip` → solo falta el archivo de sonido (el código ya tiene dónde meterlo).
- `clip + código` → falta el archivo **y** el enganche en el script/SO (hay que programarlo).

---

## ✅ Lo que YA tenemos

| Categoría | Clips | Cableado en |
|---|---|---|
| **Pasos** | `Footsteps/concrete1..4`, `sprint` | `PlayerFootsteps.cs` |
| **Impactos** | `Impacts/concrete1..4` (pared), `flesh1..5` (enemigo) | `WeaponEffects` |
| **Pistola** | `fire1..2`, `reload`, `empty` | `WeaponData` (fire/reload/empty) |
| **Escopeta** | `sg_fire1..4`, `sg_reload1..3`, `sg_cock`, `sg_empty` | `WeaponData` |
| **Bazooka** | `rocketfire1` (solo el disparo) | `WeaponData` |
| **Compartidos** | `weapon_switch` (cambio de arma), `pl_shell1..3` (casquillos) | `WeaponManager`, `WeaponEffects` |

`WeaponData` (SO) ya expone: `fireClips[]`, `reloadClips[]`, `emptyClip`. Cualquier arma nueva
solo necesita arrastrar clips ahí (sin tocar código).

---

## ❌ Sonidos que FALTAN

### 1) Movimiento del jugador  (`PlayerMovement.cs` — hoy NO tiene audio)
| Sonido | Acción / tecla | Falta | Prioridad |
|---|---|---|---|
| **Dash / esquiva** | Alt (whoosh corto) | clip + código | 🔴 |
| **Salto** | Espacio (esfuerzo/tela) | clip + código | 🟡 |
| **Aterrizaje** | al tocar suelo tras caer | clip + código | 🟡 |
| **Agacharse / levantarse** | Ctrl (roce de tela) | clip + código | ⚪ |
| **Sin stamina** | al intentar sprint/dash sin barra | clip + código | ⚪ |
| Pasos por superficie (ceniza/metal) | hoy solo "concrete" | clip + código | ⚪ |

### 2) Daño y muerte del jugador  (`PlayerHealth.cs` — hoy NO tiene audio)
| Sonido | Cuándo | Falta | Prioridad |
|---|---|---|---|
| **Recibir daño / gruñido** | `TakeDamage` | clip + código | 🔴 |
| **Muerte del jugador** | `OnDeath` / `PlayerDied` | clip + código | 🔴 |
| Latido / alarma a vida baja | HP por debajo de X | clip + código | ⚪ |

### 3) Enemigos  (`EnemyData.cs` NO tiene campos de audio — hay que añadirlos)
| Sonido | Cuándo | Falta | Prioridad |
|---|---|---|---|
| **Muerte del enemigo** (vocal) | `EnemyHealth.OnDeath` | clip + código | 🔴 |
| **Ataque melee** (zarpazo/golpe) | `MeleeAttack` | clip + código | 🔴 |
| **Detección / aggro / gruñido** | al ver al jugador (`EnemyAI`) | clip + código | 🟡 |
| **Disparo enemigo ranged** | `RangedAttack` lanza proyectil | clip + código | 🟡 |
| **Proyectil enemigo: vuelo + impacto** | `EnemyProjectile.cs` (sin audio) | clip + código | 🟡 |
| **Kamikaze: mecha/silbido al cargar** | mientras corre a explotar | clip + código | 🟡 |
| **Tanque: pisada pesada / golpe fuerte** | enemigo tanque | clip + código | ⚪ |
| Quejido al ser herido (vocal) | `TakeDamage` (además del `flesh`) | clip + código | ⚪ |

> Sugerencia técnica: añadir a `EnemyData` campos como `deathClips[]`, `attackClips[]`,
> `alertClips[]` (igual que `WeaponData`) para que sea data-driven por tipo de enemigo.

### 4) Explosiones  (`Projectile.cs` y `KamikazeAttack.cs` — sin clip)
| Sonido | Cuándo | Falta | Prioridad |
|---|---|---|---|
| **Explosión** (bazooka + kamikaze) | al impactar / al morir kamikaze | clip + código | 🔴 |

> El `ROADMAP` ya marca la explosión como **placeholder**. Es **1 clip reutilizable** para
> ambos (bazooka y kamikaze), igual que el prefab de VFX de explosión.

### 5) Armas — huecos sueltos
| Sonido | Arma | Falta | Prioridad |
|---|---|---|---|
| **Recarga / cargar cohete** | Bazooka (solo tiene `rocketfire1`) | clip | 🟡 |
| **Clic sin munición** | Bazooka (`emptyClip` vacío) | clip | ⚪ |
| Cola/silbido del cohete en vuelo | Bazooka | clip + código | ⚪ |
| **Hitmarker** (confirmación de impacto) | todas | clip + código | 🟡 |

### 6) Oleadas y fin de partida  (`WaveSystem.cs`, `GameManager.cs` — sin audio)
| Sonido | Cuándo | Falta | Prioridad |
|---|---|---|---|
| **Inicio de oleada / "horda entrante"** | nueva oleada | clip + código | 🟡 |
| **Oleada superada** | se limpia la oleada | clip + código | 🟡 |
| **Victoria** ("GANASTE") | `TriggerVictory` | clip + código | 🟡 |
| **Derrota** ("PERDISTE") | `HandlePlayerDied` | clip + código | 🟡 |

### 7) UI / menús  (`PauseMenuController.cs`, `GameOverController.cs` — sin audio)
| Sonido | Cuándo | Falta | Prioridad |
|---|---|---|---|
| **Clic de botón** | menús (pausa, game over) | clip + código | ⚪ |
| Hover de botón | menús | clip + código | ⚪ |
| **Abrir pausa / reanudar** | Esc | clip + código | ⚪ |

### 8) Ambiente y música  (no existe nada aún) 🎵
| Sonido | Uso | Falta | Prioridad |
|---|---|---|---|
| **Ambiente decadente** (viento, ceniza, drones) | loop de fondo | clip + código | 🎵 |
| **Música de combate** | sube en oleadas intensas | clip + código | 🎵 |
| Stinger | inicio de oleada / hito | clip + código | 🎵 |

### 9) Futuro (cuando existan estos sistemas)
- **Pickups** (munición / vida): aún no hay sistema de recogidas → pendiente de diseño.
- **Props destructibles**: sonido de rotura cuando se implementen.

---

## Resumen rápido — lo más urgente (🔴)
1. **Dash** (whoosh).
2. **Daño y muerte del jugador**.
3. **Muerte del enemigo** + **ataque melee**.
4. **Explosión** (bazooka + kamikaze) — reemplazar placeholder.

## Notas
- **Formato**: `.wav` (como el resto del proyecto). Mono para SFX 3D posicional.
- **Convención de carpetas**: replicar el estilo actual, p. ej.
  `Assets/Audio/Player/`, `Assets/Audio/Enemies/<tipo>/`, `Assets/Audio/Explosions/`,
  `Assets/Audio/UI/`, `Assets/Audio/Ambience/`.
- **Data-driven**: meter los clips en los SO (`WeaponData`, y un futuro audio en `EnemyData`)
  evita tocar código por cada variante.
- **Fuentes gratis** sugeridas (verificar licencia CC0/atribución): Freesound, Sonniss GDC,
  Kenney (UI), Mixkit. Igual que se hizo con VFX por packs gratis.
