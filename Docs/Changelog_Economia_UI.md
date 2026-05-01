# Sesión: Implementación de Economía, High Score y Optimizaciones UI

**Fecha:** 1 de Mayo, 2026

## Resumen de la Tarea
El objetivo principal de esta sesión fue dotar al Endless Runner de una sensación de progresión real. Se introdujeron sistemas de economía persistente (Monedas) y un registro permanente del mejor puntaje del jugador (High Score), integrados en pantallas UI dinámicas generadas enteramente por código.

## Sistemas Implementados

### 1. `CurrencyManager.cs` (Economía Persistente)
- **Patrón:** Singleton con auto-instanciación perezosa (Lazy instantiation).
- **Funcionalidad:** Mantiene un registro de `SessionCoins` (monedas recolectadas en la partida actual) y `TotalCoins` (monedas acumuladas históricamente).
- **Persistencia:** Guarda `TotalCoins` en `PlayerPrefs`. Los guardados ocurren al finalizar una partida (`CommitSessionCoins`).

### 2. High Score Persistente (`GameManager.cs`)
- Se extendió el `GameManager` para cargar y guardar la propiedad `HighScore` usando `PlayerPrefs`.
- Se introdujo `TrySetHighScore(int score)` para actualizar automáticamente el récord si el puntaje de la sesión es mayor.

### 3. Contador de Rendimiento (`FPSCounter.cs`)
- **Patrón:** Singleton con auto-instanciación y UI Overlay generada por código.
- **Funcionalidad:** Mide el frame rate en tiempo real usando `Time.unscaledDeltaTime`. Cambia de color (Verde/Amarillo/Rojo) según el rendimiento (>= 50 FPS, >= 30 FPS).
- **Persistencia:** Su estado activo/inactivo está controlado por la clave de PlayerPrefs `ShowFPS`.

## Cambios en la UI (Generación en Runtime)

Todas las interfaces del menú principal se construyen programáticamente en tiempo real en `Awake()`, prescindiendo de dependencias rígidas en la escena.

- **`MainMenuController.cs`**:
  - Se agregó el botón **TIENDA**.
  - Se añadió un subtítulo dinámico debajo del logotipo mostrando el saldo actual de monedas (`MONEDAS: X`).
  - Se añadió la instanciación de `FPSCounter` en el arranque para respetar preferencias guardadas.
- **`ShopController.cs` (Nuevo)**:
  - Interfaz de "Próximamente" para la tienda, mostrando el total de `CurrencyManager.TotalCoins`.
- **`OptionsController.cs`**:
  - Se integró un botón toggle ("MOSTRAR FPS: SI/NO") que guarda en `PlayerPrefs` y notifica a `FPSCounter.Instance`.
- **`GameOverController.cs`**:
  - Se rediseñó el panel de Game Over.
  - Ahora muestra el Puntaje Final, el **High Score** (MEJOR: X), un mensaje parpadeante de **NUEVO RECORD**, tiempo de supervivencia, y monedas recolectadas en total.
  - *Fix:* Se ajustaron los parámetros del `VerticalLayoutGroup` (spacing, padding, y heights) para evitar overlapping en la pantalla.
- **`ScoreCounter.cs`**:
  - El HUD in-game ahora auto-genera un texto de High Score (estilo Chrome Dino) a la izquierda y un contador de monedas de sesión debajo del score actual.

## Integración con Físicas / Items
- **`ScoreCoin.cs`**: Modificado para que, además de sumar puntos visuales (`ScoreCounter.AddBonusScore`), también envíe las monedas recolectadas al registro de sesión (`CurrencyManager.AddSessionCoins`).

## Próximos Pasos (Pendientes)
1. Botón de **Borrar Progreso (Reset)** dentro de las opciones.
2. Expansión de `ShopController` para usar las monedas en cosméticos (ej. skin Wizard).
3. Transiciones dinámicas a nuevos biomas o progresión de dificultad mediante patrones de *ObstacleSpawner*.
