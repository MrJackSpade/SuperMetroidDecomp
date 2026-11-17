namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What the Spacetime shot, an uncharged Spazer, Plasma and Ice beam, runs (#1923): from an animation that reaches far
/// enough it turns saved progression on and off (<see cref="SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/>), and it
/// never moves.
/// </summary>
internal sealed class SpacetimeShotOverwrite : SamusShotBehavior
{
    /// <inheritdoc/>
    internal override void Run(SamusShotUpdate update) =>
        update.Shots.Combos.RunSpaceTimeProgressionOverwrite(update.Progression, update.Shot);
}
