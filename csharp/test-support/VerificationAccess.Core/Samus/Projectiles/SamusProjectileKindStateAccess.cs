namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Reads the state the kind of the projectile holding a shot place keeps, as checks name it: production reads the
/// occupant's kind state where it holds one, so the lookup that names the kind it must be is the checks' own (#2554).
/// </summary>
internal static class SamusProjectileKindStateAccess
{
    /// <summary>The shot place's projectile reads.</summary>
    extension(SamusProjectile shot)
    {
        /// <summary>The state the kind of the projectile holding the place keeps, which must be a <typeparamref name="TState"/>.</summary>
        /// <exception cref="InvalidOperationException">No projectile holds the place, or the one holding it keeps no such state.</exception>
        internal TState KindStateAs<TState>() where TState : SamusShotKindState => shot.Occupant switch
        {
            { KindState: TState state } => state,
            { } held => throw new InvalidOperationException(
                $"The projectile in place {shot.Place} keeps no {typeof(TState).Name}; it keeps {held.KindState?.GetType().Name ?? "nothing"}."),
            null => throw new InvalidOperationException($"No projectile holds place {shot.Place}, so none keeps a {typeof(TState).Name}."),
        };
    }
}
