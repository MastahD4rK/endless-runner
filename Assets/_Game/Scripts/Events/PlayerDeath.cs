using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player has died.
    /// </summary>
    /// <typeparam name="PlayerDeath"></typeparam>
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            var player = model.player;
            if (player.health.IsAlive)
            {
                player.health.Die();
                model.virtualCamera.Follow = null;
                model.virtualCamera.LookAt = null;
                player.controlEnabled = false;

                if (GameSpeedManager.Instance != null) GameSpeedManager.Instance.StopWorld();

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);

                // Ir directamente al estado de muerte sin pasar por Hurt.
                // El Raptor.overrideController mapea AMBOS clips (PlayerHurt y PlayerDeath)
                // a la misma animación raptor-dead, así que pasar por Hurt primero causaba
                // que la animación de caída se reprodujera dos veces seguidas.
                if (player.animator != null)
                {
                    player.animator.SetBool("dead", true);
                    player.animator.Play("Player-Death");
                }

                Simulation.Schedule<PlayerSpawn>(2);
            }
        }
    }
}