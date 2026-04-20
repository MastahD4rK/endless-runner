# 🎮 Endless Runner

Juego 2D de tipo Endless Runner desarrollado en Unity 6. Convertido desde el template Platformer Microgame. El jugador corre automáticamente hacia la derecha mientras el mundo se mueve hacia él; debe saltar obstáculos para sobrevivir el mayor tiempo posible.

---

## Desarrolladores

- Cristóbal Gómez  | usuarios: cristobalGomez189
- Cristhian Quiroz | usuarios: mastah_d4rk

## 🕹️ Controles

| Acción | Teclado | Gamepad |
|--------|---------|---------|
| Saltar | `Space` / `W` / `↑` | `South Button` (A/X) |
| Menu   | `Escape` | `Start` |

> El salto es **variable**: soltarlo antes corta la altura.

---

## 📁 Estructura del Proyecto

```
Assets/
│
├── _Game/              ← TODO lo del juego en sí
│   ├── Audio/          ← Música y efectos de sonido
│   ├── Prefabs/        ← Prefabs: Player, Enemy, Obstacle, UI Canvas, etc.
│   ├── Scenes/         ← Escenas del juego (SampleScene)
│   └── Scripts/        ← Código fuente (ver detalle abajo)
│
├── Art/                ← Assets visuales
│   ├── Characters/
│   │   ├── Wizard/     ← Personaje brujita (sprites, animaciones, prefab)
│   │   └── Character/  ← Personaje raptor (sprites, animaciones)
│   ├── Environment/    ← Backgrounds de escenario (paquetes Craftpix)
│   │   ├── craftpix-net-404331 (Nature Backgrounds)
│   │   └── craftpix-net-437584 (Moon Backgrounds)
│   └── Sky/            ← Fondos de cielo y nubes (Craftpix 1-bit)
│
├── Engine/             ← Configuración técnica del motor
│   ├── Rendering/      ← Post-process, URP renderer
│   ├── Settings/       ← URP Pipeline Asset, Input System Actions
│   └── TextMesh Pro/   ← Fuentes y recursos TMP
│
└── _Legacy/            ← Assets del template original (NO usar en desarrollo)
    ├── Documentation/  ← Guía del template + ThirdPartyNotice
    ├── Editor/         ← PatrolPathEditor (legado)
    ├── Environment_Template/ ← Sprites/tiles del Platformer original
    ├── Mod Assets/     ← Assets decorativos del Platformer mod
    ├── Tiles/          ← Tile assets del template
    └── Tutorials/      ← Steps del tutorial de Unity
```

### `_Game/Scripts/` en detalle

```
Scripts/
├── Core/       ← Motor de simulación de eventos (HeapQueue, Simulation)
├── Gameplay/   ← Eventos del juego: muertes, saltos, spawning, WorldMover
├── Mechanics/  ← Física base: KinematicObject, PlayerController, Health
├── Model/      ← Datos compartidos (PlatformerModel: player, cámara, spawn)
├── UI/         ← MetaGameController, MainUIController
└── View/       ← Efectos visuales: ParallaxLayer, AnimatedTile
```

---

## 🔧 Sistemas Clave

| Sistema | Script(s) | Descripción |
|---------|-----------|-------------|
| **Core del Juego** | `GameManager.cs` | Singleton persistente. Gestiona escenas, niveles y puntaje de sesión. |
| **Velocidad del mundo** | `GameSpeedManager.cs` | Acelera progresivamente y afecta puntaje/movimiento. |
| **Mover objetos** | `WorldMover.cs` | Desplaza plataformas/obstáculos con FixedUpdate hacia el jugador. |
| **Generación de niveles** | `LevelGenerator.cs` | Object Pooling de bloques/plataformas generadas a la derecha. |
| **Obstáculos** | `ObstacleSpawner.cs` | Object Pooling + dificultad progresiva de elementos letales. |
| **Monedas (Coins)** | `CoinSpawner.cs` / `ScoreCoin.cs` | Generación dinámica de monedas coleccionables con bonus de puntos al HUD. |
| **Enemigos (Slimes)** | `EnemySpawner.cs` / `SlimeController.cs`| Spawner de slimes con físicas personalizadas como obstáculos móviles. |
| **Transiciones de interfaz** | `SceneTransitionController.cs` | Permite cross-fade visual y carga asíncrona de niveles/menú. |
| **HUD & Score** | `ScoreCounter.cs` | Sube puntos dinámicamente con la distancia en tiempo real, tipo Dino. |
| **Game Over** | `GameOverController.cs` | Pantalla con resultado final, tiempo y reinicio/salida. |

---

## 🚧 Estado y Funcionalidad Pendiente

- [x] **Arquitectura Multi-Escena**: GameManager para persistir de menú principal a gameplay.
- [x] **HUD y Tracking de Puntos**: Contador dinámico visible (`ScoreCounter`) atado al tiempo y velocidad de avance.
- [x] **Game Over**: Pantalla robusta que frena el mundo, detiene puntaje y permite volver al menú o reintentar.
- [x] **Tokens (Monedas)**: Sistema de Object Pooling (`CoinSpawner.cs`) y collider (`ScoreCoin.cs`) para spawn aleatorio y sumar bonus animado al HUD.
- [x] **Enemigos y Dificultad**: Enemigos inicializados correctamente que actúan como obstáculos en movimiento a lo largo del nivel.
- [ ] **Progresión y Niveles**: Implementar varios niveles con aumento de dificultad dinámico de acuerdo al score/tiempo, y posibles batallas contra jefes.

---

## 📦 Dependencias

- **Unity 6** (URP)
- **Cinemachine** (cámara virtual)
- **Input System** (controles)
- **TextMesh Pro** (UI)
