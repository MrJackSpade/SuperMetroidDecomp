namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Checks, once as a species' collision loads, that its hitboxes name only the touch and shot reactions its
/// handlers answer (#2553), so a handler distinguishes those reactions alone and has no arm for another.
/// </summary>
internal static partial class EnemyHitboxReactionCheck
{
    /// <summary>
    /// Returns <paramref name="collision"/> once every one of <paramref name="hitboxes"/> names a touch in
    /// <paramref name="touches"/> and a shot in <paramref name="shots"/>.
    /// </summary>
    /// <param name="collision">The species' collision, returned unchanged.</param>
    /// <param name="hitboxes">Every hitbox the collision holds.</param>
    /// <param name="species">The species, for the failure.</param>
    /// <param name="touches">The touch reactions the species answers.</param>
    /// <param name="shots">The shot reactions the species answers.</param>
    /// <exception cref="InvalidDataException">A hitbox names a reaction the species does not answer.</exception>
    internal static TCollision Checked<TCollision>(
        TCollision collision,
        IEnumerable<EnemyCollisionHitbox> hitboxes,
        string species,
        IReadOnlyCollection<EnemyReaction> touches,
        IReadOnlyCollection<EnemyReaction> shots)
    {
        foreach (EnemyCollisionHitbox hitbox in hitboxes)
        {
            if (!touches.Contains(hitbox.Touch) || !shots.Contains(hitbox.Shot))
            {
                throw new InvalidDataException(
                    $"A hitbox of {species} names the touch {hitbox.Touch} and the shot {hitbox.Shot}, which {species} does not answer.");
            }
        }
        return collision;
    }
}
