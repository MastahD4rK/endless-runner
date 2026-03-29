using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : Simulation.Event<PlayerSpawn>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            // En un Endless Runner, es más limpio recargar la escena entera para limpiar plataformas.
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            
            // Opcional: reiniciar velocidad por si el manager no se destruye entre escenas
            if (GameSpeedManager.Instance != null) 
                GameSpeedManager.Instance.ResetSpeed();
        }
    }
}