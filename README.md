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
| **Core del Juego** | `GameManager.cs` | Singleton persistente. Gestiona escenas, niveles y puntaje de sesión. Aplica preferencias del jugador al arrancar. |
| **Velocidad del mundo** | `GameSpeedManager.cs` | Acelera progresivamente y afecta puntaje/movimiento. |
| **Mover objetos** | `WorldMover.cs` | Se registra al Manager central para ser movido eficientemente en O(1). |
| **Optimizador de Físicas**| `WorldMoverManager.cs` | **[NUEVO]** Singleton encargado de ejecutar un único ciclo `FixedUpdate` ultra rápido para desplazar docenas de objetos sincronizados, evitando cuellos de botella de CPU. |
| **Generación de niveles** | `LevelGenerator.cs` | Object Pooling de bloques/plataformas generadas a la derecha. |
| **Obstáculos** | `ObstacleSpawner.cs` | Object Pooling + dificultad progresiva de elementos letales. |
| **Monedas (Coins)** | `CoinSpawner.cs` / `ScoreCoin.cs` | Spawner dinámico optimizado (con swap-remove) para sumar bonus al score. |
| **Economía Persistente** | `CurrencyManager.cs` | Singleton auto-instanciable que guarda permanentemente las monedas totales en `PlayerPrefs`. |
| **Enemigos (Slimes)** | `EnemySpawner.cs` / `SlimeController.cs`| Spawner optimizado que cachea componentes (`PooledEnemy`) y usa swap-remove O(1) en sus colas. |
| **Transiciones** | `SceneTransitionController.cs` | Permite cross-fade visual y carga asíncrona de niveles/menú. |
| **HUD & Score** | `ScoreCounter.cs` | Ultra-optimizado (0 GC allocs). Muestra Score, High Score y Monedas. |
| **Game Over** | `GameOverController.cs` | Pantalla construida por código con: puntaje, récord, indicador "NUEVO RECORD", monedas recolectadas y botones de acción. |
| **Menús** | `MainMenuController.cs` / `OptionsController.cs` | Interfaces 100% generadas por código. Opciones incluye control de volumen, FPS Counter y modo **Pantalla Completa**. |
| **Contador FPS** | `FPSCounter.cs` | Script auto-instanciable activable desde Opciones que muestra rendimiento real de FPS y se ajusta con códigos de color. |

---

## 🚧 Estado y Funcionalidad Pendiente

- [x] **Arquitectura Multi-Escena**: GameManager para persistir de menú principal a gameplay.
- [x] **HUD, High Score y Monedas**: Contador dinámico, persistencia en PlayerPrefs y panel Game Over ultra completo.
- [x] **Performance & Estabilidad**: Eliminados los GC Allocs por frame de la UI, implementado *swap-remove* universal en spawners y movimiento centralizado por `WorldMoverManager`.
- [x] **Opciones & QoL**: Menú de opciones generado por UI, toggle de FPS Counter y modo Pantalla Completa.
- [ ] **Borrar Progreso (Reset)**: Agregar botón seguro en opciones para borrar monedas y high score en `PlayerPrefs`.
- [ ] **Items en la Tienda**: Utilizar los assets disponibles (ej: Wizard.prefab) como cosméticos desbloqueables con las monedas recolectadas.
- [ ] **Progresión Avanzada y Biomas**: Implementar transición entre escenarios o aumentar la dificultad drásticamente con patrones de spawn más agresivos.

---

## 📦 Dependencias

- **Unity 6** (URP)
- **Cinemachine** (cámara virtual)
- **Input System** (controles)
- **TextMesh Pro** (UI)
