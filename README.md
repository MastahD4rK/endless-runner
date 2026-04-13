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
| Velocidad del mundo | `GameSpeedManager.cs` | Singleton. Acelera progresivamente. |
| Mover objetos | `WorldMover.cs` | Desplaza plataformas/obstáculos con FixedUpdate. |
| Generación de suelo | `LevelGenerator.cs` | Object Pooling de bloques de plataforma. |
| Obstáculos | `ObstacleSpawner.cs` | Object Pooling + dificultad progresiva. |
| Parallax | `ParallaxLayer.cs` | Fondos con efecto de profundidad. |
| Muerte / Respawn | `PlayerDeath.cs` + `PlayerSpawn.cs` | Para el mundo y recarga la escena. |

---

## 🚧 Funcionalidad Pendiente

- [ ] `ScoreManager` — distancia recorrida + monedas recolectadas
- [ ] `HUDController` — mostrar puntaje en tiempo real
- [ ] Pantalla de **Game Over** con puntaje final y botón de reinicio
- [ ] Conectar tokens al sistema de puntaje

---

## 📦 Dependencias

- **Unity 6** (URP)
- **Cinemachine** (cámara virtual)
- **Input System** (controles)
- **TextMesh Pro** (UI)
