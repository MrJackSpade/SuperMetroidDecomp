namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a power bomb's explosion does to a Draygon goop or wall-turret shot it catches (#1590): the shot frees its
/// enemy-projectile place at once but finishes the update it was caught in. It still moves, a flying goop still tests
/// Samus and, on reaching her, sticks to her and slows her again after the explosion has cleared her slowdown, and its
/// animation still runs this pass; where it moved to is what it leaves in its place for the next projectile spawned there
/// (<see cref="EnemyProjectileHandOver"/>). From the next pass on it takes no part in the game.
/// </summary>
/// <remarks>
/// Consulted by the goop's and the turret shot's updates, each time the explosion catches one, before the rest of the
/// update runs. Evidence: the original power-bomb test clears only the projectile's identity and Samus's slowdown and
/// returns to the routine that called it, which goes on moving the projectile, testing it against Samus and the room,
/// and the projectile pass then runs its instructions without re-reading the identity.
/// </remarks>
[CompatibilityService("a Draygon shot caught in a power bomb frees its place at once but finishes the update it was caught in: it still moves, can still stick to Samus, and leaves where it moved to in its place")]
public static class PowerBombedDraygonShotFinishesItsUpdate
{
    /// <summary>
    /// Frees a caught shot's place, through <paramref name="freePlace"/>, leaving the shot itself to the rest of its
    /// update.
    /// </summary>
    /// <typeparam name="TShot">A Draygon goop or turret shot.</typeparam>
    /// <param name="shot">The shot the explosion caught.</param>
    /// <param name="freePlace">Frees the shot's place without ending the shot.</param>
    public static void Catch<TShot>(TShot shot, Action<TShot> freePlace)
        where TShot : class =>
        Ensure.NotNull(freePlace)(Ensure.NotNull(shot));
}
