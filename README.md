# 🎮 Endless Runner

Juego 2D de tipo Endless Runner desarrollado en Unity 6. Convertido desde el template Platformer Microgame. El jugador corre automáticamente hacia la derecha mientras el mundo se mueve hacia él; debe saltar obstáculos para sobrevivir el mayor tiempo posible.

---

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
| **Core del Juego** | `GameManager.cs` | Singleton persistente. Gestiona escenas, niveles y puntaje de sesión. Además persiste el High Score usando `PlayerPrefs`. |
| **Velocidad del mundo** | `GameSpeedManager.cs` | Acelera progresivamente y afecta puntaje/movimiento. |
| **Mover objetos** | `WorldMover.cs` | Desplaza plataformas/obstáculos con FixedUpdate hacia el jugador. |
| **Generación de niveles** | `LevelGenerator.cs` | Object Pooling de bloques/plataformas generadas a la derecha. |
| **Obstáculos** | `ObstacleSpawner.cs` | Object Pooling + dificultad progresiva de elementos letales. |
| **Monedas (Coins)** | `CoinSpawner.cs` / `ScoreCoin.cs` | Spawner dinámico y lógica para sumar bonus al score y moneda a la sesión actual. |
| **Economía Persistente** | `CurrencyManager.cs` | Singleton auto-instanciable que guarda permanentemente las monedas totales en `PlayerPrefs`. |
| **Enemigos (Slimes)** | `EnemySpawner.cs` / `SlimeController.cs`| Spawner de slimes con físicas personalizadas como obstáculos móviles. |
| **Transiciones** | `SceneTransitionController.cs` | Permite cross-fade visual y carga asíncrona de niveles/menú. |
| **HUD & Score** | `ScoreCounter.cs` | Sube puntos dinámicamente. Muestra en pantalla: Score Actual, High Score (estilo Dino) y Monedas de sesión en tiempo real. |
| **Game Over** | `GameOverController.cs` | Pantalla construida por código con: puntaje, récord, indicador "NUEVO RECORD", monedas recolectadas y botones de acción. |
| **Menús** | `MainMenuController.cs` / `ShopController.cs` / `OptionsController.cs` | Interfaces 100% generadas por código. El menú principal incluye acceso a Opciones (control de FPS, audio) y Tienda (preview). |
| **Contador FPS** | `FPSCounter.cs` | Script auto-instanciable activable desde Opciones que muestra rendimiento real de FPS y se ajusta con códigos de color. |

---

## 🚧 Estado y Funcionalidad Pendiente

- [x] **Arquitectura Multi-Escena**: GameManager para persistir de menú principal a gameplay.
- [x] **HUD, High Score y Monedas**: Contador dinámico, persistencia en PlayerPrefs y panel Game Over ultra completo.
- [x] **Enemigos y Obstáculos**: Físicas correctas, pooling y dificultad básica.
- [x] **Tienda (Base) y UI por Código**: Menús de opciones, tienda vacía con display de monedas y FPS Counter funcionando nativamente por UI generada en runtime.
- [ ] **Borrar Progreso (Reset)**: Agregar botón seguro en opciones para borrar monedas y high score en `PlayerPrefs`.
- [ ] **Items en la Tienda**: Utilizar los assets disponibles (ej: Wizard.prefab) como cosméticos desbloqueables con las monedas recolectadas.
- [ ] **Progresión Avanzada y Biomas**: Implementar transición entre escenarios o aumentar la dificultad drásticamente con patrones de spawn más agresivos.

---

## 📦 Dependencias

- **Unity 6** (URP)
- **Cinemachine** (cámara virtual)
- **Input System** (controles)
- **TextMesh Pro** (UI)
