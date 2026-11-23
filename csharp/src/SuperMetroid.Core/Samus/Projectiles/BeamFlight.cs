namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A beam's flight without the Wave Beam (#1923): it leaves a trail every <see cref="ProjectileTimingDefinitions.TrailInterval"/>, accelerates, and stops at the
/// first block it meets.
/// </summary>
internal sealed partial class BeamFlight : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.FlyBeam(update.Terrain, update.Held, update.Camera);
}
