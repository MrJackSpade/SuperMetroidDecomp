namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A Wave Beam shot's flight (#1923): it passes through blocks, reacting to the shootable ones, and leaves a trail every
/// <see cref="TrailInterval"/> updates after its first.
/// </summary>
/// <param name="trailInterval">The updates between the shot's trails after its first.</param>
internal sealed class WaveBeamFlight(int trailInterval) : SamusShotBehavior
{
    /// <summary>
    /// The updates between the shot's trails after its first: three for an uncharged Wave or Ice and Wave shot, four for
    /// every other Wave shot. Every shot leaves its first trail four updates after it is fired.
    /// </summary>
    internal int TrailInterval { get; } = Ensure.BetweenInclusive(1, int.MaxValue, trailInterval);

    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Motion.FlyWaveBeam(update.Terrain.Level, update.Held, update.Camera, update.Terrain.RoomObjects, TrailInterval);
}
