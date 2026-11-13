using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Observes the bombs' frame through the requests the gameplay frame reads from it.</summary>
internal static class SamusBombProjectileSystemAccess
{
    /// <summary>Runs a bomb frame through the steps the gameplay frame runs.</summary>
    extension(SamusBombProjectileSystem self)
    {
        /// <summary>
        /// Runs one bomb frame as <see cref="SamusBombProjectileSystem.StepFrame"/> does, the explosion advancing first, and
        /// returns whether the bombs' HUD producer consumed the beam charge: the request the gameplay frame reads from
        /// <see cref="SamusBombProjectileSystem.RunHudProducer"/> to tear the charge down.
        /// </summary>
        internal bool StepFrameConsumingCharge(SamusTerrain terrain, SamusState samus, ControllerButton controllerInput, ControllerButton controllerNewInput)
        {
            self.AdvancePowerBombExplosion(samus, controllerInput);
            bool consumed = self.RunHudProducer(samus, controllerInput, controllerNewInput);
            _ = self.HandleBombs(terrain, samus);
            return consumed;
        }
    }
}
