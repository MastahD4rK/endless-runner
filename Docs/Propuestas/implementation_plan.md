# Implementación de Sistema de Árbol de Habilidades (Skill Tree)

Este plan describe cómo estructurar e integrar un árbol de habilidades en tu Endless Runner actual, aprovechando tu `CurrencyManager` (para comprar mejoras con las monedas recolectadas) y tu arquitectura de UI generada por código.

## User Review Required

> [!IMPORTANT]
> Revisa la lista de "Posibles Habilidades" en la sección de preguntas abiertas y confirma cuáles te gustaría incluir en la primera versión.

## Open Questions

> [!NOTE]
> 1. **¿Qué habilidades exactas te gustaría tener?** Algunas ideas comunes para Endless Runners:
>    - **Doble Salto:** Desbloquea un salto en el aire.
>    - **Imán de Monedas:** Atrae las monedas cercanas sin tocarlas (mejorable: más radio de atracción).
>    - **Escudo/Vida Extra:** Sobrevive a un golpe antes de morir.
>    - **Multiplicador de Monedas/Puntos:** Aumenta pasivamente el puntaje o las monedas generadas.
> 2. **¿El árbol de habilidades reemplazará a la Tienda (ShopController) o coexistirán?** (Ej. Tienda para cosméticos/personajes, Árbol para mecánicas).

---

## Proposed Changes

La implementación se dividirá en tres capas: Datos (Persistencia), UI (Interfaz visual) y Gameplay (Efectos en el juego).

### 1. Sistema de Datos: `SkillManager.cs`

Crearemos un Singleton persistente (al estilo `CurrencyManager`) encargado de gestionar el nivel de cada habilidad.

- **Enumerador `SkillType`**: `DoubleJump`, `CoinMagnet`, `Shield`, etc.
- **Diccionario/Datos**: Un registro de `Nivel Actual` y `Costo` por cada nivel.
- **Persistencia**: Usará `PlayerPrefs` para guardar el nivel de cada habilidad (ej. `PlayerPrefs.GetInt("Skill_DoubleJump_Level", 0)`).
- **Lógica de Compra**: Un método `TryUpgradeSkill(SkillType)` que consultará a `CurrencyManager.Instance.SpendCoins(costo)` y, si es exitoso, aumentará el nivel de la habilidad y lo guardará.

### 2. Interfaz de Usuario: `SkillTreeController.cs`

Siguiendo tu patrón arquitectónico de UI (`MainMenuController`, `ShopController`):

- Se generará un panel 100% por código en *Runtime*.
- Mostrará un diseño en árbol o lista de tarjetas con:
  - Título y Descripción de la habilidad.
  - Nivel actual vs Nivel máximo.
  - Costo de la siguiente mejora.
  - Botón de "Mejorar" (que se deshabilita si no hay suficientes monedas, leyendo de `CurrencyManager.TotalCoins`).
- Se añadirá un botón en el `MainMenuController` para acceder a este panel.

### 3. Integración en el Gameplay

Los scripts existentes se modificarán ligeramente para "leer" el estado del `SkillManager`.

#### Modificaciones a `PlayerController.cs` (o similar)
- **Doble Salto**: En el método de salto, si el jugador no está en el suelo pero `SkillManager.Instance.GetSkillLevel(SkillType.DoubleJump) > 0`, permitir un salto adicional y restar un contador de saltos aéreos.

#### Creación de `CoinMagnet.cs` (Nuevo Componente)
- Si `SkillManager.Instance.GetSkillLevel(SkillType.CoinMagnet) > 0`, el jugador tendrá un collider tipo Trigger (o hará un OverlapSphere en `Update`) que atraerá hacia él a los objetos con el tag `Coin`.
- El radio y velocidad de atracción dependerán del nivel de la habilidad.

---

## Verification Plan

### Automated Tests / Lógica
1. Verificar que al comprar una habilidad, las monedas de `CurrencyManager` se deducen correctamente.
2. Verificar que al reiniciar el juego, `SkillManager` carga correctamente los niveles de `PlayerPrefs`.

### Manual Verification
1. Abrir el juego y recolectar suficientes monedas (o forzarlas vía Inspector).
2. Entrar al Menú Principal -> Árbol de Habilidades y comprar el Doble Salto.
3. Iniciar una partida y verificar que el segundo salto en el aire funciona.
4. Reiniciar Unity y confirmar que la habilidad sigue desbloqueada.
