namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What the Chainsaw shot, an uncharged Spazer, Plasma and Wave beam, runs (#1923): a Power Bomb's fuse over the shot,
/// the fuse the beam carries (<see cref="SamusProjectile.BeamFuse"/>), then the Power Bomb's block reactions
/// (<see cref="ChainsawFuse"/>), and it never moves.
/// </summary>
internal sealed partial class ChainsawPowerBombShot : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Chainsaw.RunPowerBombFuse(update.Terrain.Level, update.Shot, update.SamusBombs,
            update.Terrain.RoomObjects);
}
