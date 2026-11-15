namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What the Chainsaw shot, an uncharged Spazer, Plasma and Wave beam, runs (#1923): a Power Bomb's fuse over the shot,
/// then the Power Bomb's block reactions (<see cref="ChainsawFuse"/>), and it never moves.
/// </summary>
internal sealed class ChainsawPowerBombShot : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Chainsaw.RunPowerBombFuse(update.Terrain.Level, update.Shot, update.SharedProjectiles,
            update.Terrain.RoomObjects);
}
