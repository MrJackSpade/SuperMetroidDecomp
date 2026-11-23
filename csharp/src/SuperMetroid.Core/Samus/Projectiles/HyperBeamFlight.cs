namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Hyper Beam shot's flight (#1923): the Wave Beam's passage through blocks, without trails.</summary>
internal sealed partial class HyperBeamFlight : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.FlyHyperBeam(update.Terrain, update.Held, update.Camera);
}
