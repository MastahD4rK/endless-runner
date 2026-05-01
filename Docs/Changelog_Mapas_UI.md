# Registro de Cambios: Optimización de Mapas y UI (Mayo 2026)

Este documento resume las modificaciones y optimizaciones realizadas recientemente en el sistema de gestión de fondos, mapas y elementos visuales del Endless Runner.

## 1. Refactorización de `MapManager.cs`

El script encargado de transicionar los fondos a medida que el jugador avanza fue reescrito para priorizar la estabilidad visual.

*   **Eliminación del Sistema de Parallax:**
    *   **Problema:** El sistema original basaba la velocidad de las capas en su índice (hijos del prefab), lo cual fallaba sistemáticamente porque cada pack de assets tenía estructuras diferentes (algunos con 1 capa, otros con 8). Además, al no ser *tileables* (seamless), las capas terminaban desplazándose fuera de la cámara.
    *   **Solución:** Los backgrounds ahora son estáticos. Se prioriza una transición suave en lugar de desplazamiento de capas.
*   **Transiciones (Crossfade) por Tiempo Real:**
    *   **Problema:** Antes, el fade dependía de la puntuación del jugador. A medida que el juego se aceleraba, las transiciones se volvían instantáneas o casi invisibles.
    *   **Solución:** El crossfade ahora dura exactamente `fadeDuration` (2 segundos por defecto) en tiempo real, garantizando una transición visual fluida sin importar la velocidad de juego.
*   **Alineamiento por Referencia (No más escalados impredecibles):**
    *   **Problema:** Intentar calcular los *bounds* dinámicamente causaba desajustes masivos si los prefabs tenían elementos invisibles o proporciones extrañas.
    *   **Solución:** El script ahora toma el **Background Inicial** (colocado y escalado manualmente en el editor) como referencia absoluta. Los nuevos backgrounds simplemente copian la posición y escala de este fondo inicial.
*   **Offset Vertical (`backgroundYOffset`):**
    *   Se añadió un valor ajustable desde el Inspector (por defecto `-1.5`) para bajar todos los fondos ligeramente. Esto permite alinear el "suelo dibujado" del prefab con el suelo físico donde corre el jugador.

## 2. Ajustes Visuales y de Capas

*   **Fondo de Cámara Negro:**
    *   Se forzó por código que `Camera.main.backgroundColor = Color.black`. Así, si existe algún pequeño espacio (gap) en los bordes debido a diferencias de proporciones entre prefabs, se mostrará negro (disimulándose con la temática del juego) en lugar del gris por defecto de Unity.
*   **Ocultar el Piso Base (Línea Verde):**
    *   **Problema:** Se veía una línea verde debajo del jugador que rompía la inmersión. Esta línea provenía del `SpriteRenderer` del objeto `Piso_Base`, el cual tenía un color verdoso (`r=0.39, g=0.70, b=0.37`) y un `SortingOrder` de `0`.
    *   **Solución:** En la escena `level_01`, se cambió el `SortingOrder` de `Piso_Base` a `-10`. De esta manera, se dibuja por detrás de todos los backgrounds (que operan entre `-1` y `-8`), volviéndose invisible pero manteniendo su `BoxCollider2D` intacto para las físicas del jugador.

## 3. Curación de Prefabs de Fondo

*   **Problema:** Algunos fondos seleccionados de forma aleatoria no tenían capa de suelo (ej. solo montañas o cielo). Cuando aparecían, el jugador parecía correr en el aire, lo cual no tiene sentido en un Endless Runner tradicional.
*   **Solución:** Se revisaron las capas de los prefabs en uso y se eliminaron del array `mapPrefabs` en la escena `level_01` aquellos problemáticos:
    *   `Background_6` (2 capas, sin suelo) -> Eliminado
    *   `Background_7` (2 capas, sin suelo) -> Eliminado
    *   `Background_8` (1 capa, solo cielo) -> Eliminado
*   **Actuales:** Ahora el juego solo rotará entre `Background_3`, `Background_4` y `Background_5`, los cuales tienen entre 4 y 5 capas, garantizando siempre la presencia de un suelo visual para el jugador.
