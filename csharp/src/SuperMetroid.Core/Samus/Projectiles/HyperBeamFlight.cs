namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Hyper Beam shot's flight (#1923): the Wave Beam's passage through blocks, without trails.</summary>
internal sealed class HyperBeamFlight : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.FlyHyperBeam(update.Terrain.Level, update.Held, update.Camera, update.Terrain.RoomObjects);
}
