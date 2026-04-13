using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.UI;
using UnityEngine.SceneManagement;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player has died and it's time to mostrar Game Over.
    /// En lugar de recargar la escena directamente, muestra el panel de Game Over
    /// para que el jugador pueda elegir reintentar o volver al menú.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        public override void Execute()
        {
            // 1. Intentar mostrar la pantalla de Game Over (flujo normal)
            if (GameOverController.Instance != null)
            {
                GameOverController.Instance.ShowGameOver();
                return;
            }

            // 2. Fallback si no hay GameOverController pero sí GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReloadCurrentLevel();
                return;
            }

            // 3. Fallback final: recargar la escena directamente (modo testing en Editor)
            if (GameSpeedManager.Instance != null)
                GameSpeedManager.Instance.ResetSpeed();

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}