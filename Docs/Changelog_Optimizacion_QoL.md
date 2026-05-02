# Changelog: Optimización de Rendimiento y Quality of Life (QoL)

**Fecha:** 1 de Mayo de 2026

En esta sesión nos centramos en resolver cuellos de botella de CPU y memoria, solucionar errores críticos durante las transiciones de escena e implementar mejoras para la experiencia del jugador.

## 🛠️ 1. Optimizaciones de Rendimiento (CPU & Memoria)

### 1.1 Reducción de Overhead en Físicas (`WorldMoverManager`)
- **Problema Inicial:** Cientos de objetos (`WorldMover`) ejecutando el método `FixedUpdate()` individualmente causaban un pico innecesario en la CPU.
- **Solución:** Se creó el Singleton `WorldMoverManager`. Este se encarga de ejecutar **un único** `FixedUpdate()` para iterar sobre una lista centralizada y desplazar todos los objetos registrados al mismo tiempo.
- **Implementación:** `WorldMover.cs` ahora usa `OnEnable()` y `OnDisable()` para registrarse y desuscribirse de esta lista dinámicamente usando *swap-remove* O(1).

### 1.2 Eliminación de GC Allocations en Interfaz
- **Problema Inicial:** `ScoreCounter.cs` actualizaba el texto de las monedas *cada frame*, recreando strings constantes (`$"x{coins}"`) e invocando continuamente al *Garbage Collector* de Unity, lo cual generaba micro-tirones.
- **Solución:** Implementación de caché de estado. Se añadió la variable `_lastCoinCount` para verificar si la cantidad real de monedas ha cambiado antes de forzar la asignación del nuevo string a la interfaz gráfica.

### 1.3 Caché de Componentes en Object Pooling
- **Problema Inicial:** El script `EnemySpawner.cs` hacía una llamada a `GetComponent<SlimeController>()` cada vez que reciclaba y reaparecía un slime.
- **Solución:** Introducción del `struct PooledEnemy`. Al instanciar un enemigo por primera vez, el *spawner* almacena el componente interno permanentemente. Cuando el slime se necesita de nuevo, se extrae de la cola pre-procesado.
- Se reemplazó el ineficiente `List.RemoveAt(i)` por la técnica O(1) **Swap-Remove** para la limpieza de enemigos fuera de pantalla.

---

## 🐛 2. Corrección de Bugs Críticos (Bugfix)

**Error Solucionado:** `Some objects were not cleaned up when closing the scene. (Did you spawn new GameObjects from OnDestroy?)`

- **Contexto:** Al perder el juego e intentar volver al menú principal, Unity destruía la escena y mostraba un error rojo.
- **Causa:** `WorldMover` intentaba desuscribirse de `WorldMoverManager` en su `OnDisable`. Si el Manager había sido destruido primero por Unity (el orden de destrucción es impredecible), el acceso a `.Instance` provocaba que el getter del *singleton* intentara crear un nuevo GameObject (`new GameObject("[WorldMoverManager]")`) en plena destrucción masiva de la escena.
- **Solución Aplicada:** Se añadió la propiedad `HasInstance` al `WorldMoverManager`. Ahora los objetos comprueban si el mánager *sigue vivo* en vez de instanciar uno por accidente antes de desuscribirse.

---

## ⚙️ 3. Calidad de Vida e Interfaz (QoL)

### Modo Pantalla Completa
- Se añadió un botón de **"PANTALLA COMPLETA: SI / NO"** dentro de las Opciones generadas por código (`OptionsController.cs`).
- Su estado se persiste utilizando `PlayerPrefs` bajo la clave `Fullscreen`.
- **Integración:** Se modificó el `GameManager.Awake()` para invocar un nuevo método estático llamado `OptionsController.ApplyStartupPreferences()`. Esto permite que el juego aplique inmediatamente el ajuste de Pantalla Completa (y el volumen guardado) ni bien inicia el juego sin necesidad de abrir el panel de opciones.

---

### Mantenimiento de Repositorio
- El archivo `README.md` principal fue actualizado para integrar el `WorldMoverManager` a la tabla de arquitectura e indicar el éxito de estas optimizaciones.
