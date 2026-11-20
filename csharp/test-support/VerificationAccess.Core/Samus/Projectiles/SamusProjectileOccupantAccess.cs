namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A check's reach to the shot or bomb a projectile place holds, which it set up before asking.</summary>
internal static class SamusProjectileOccupantAccess
{
    /// <summary>The shot <paramref name="projectile"/>'s place holds, which the check fired or placed before asking (#2558).</summary>
    /// <exception cref="InvalidOperationException">No shot holds the place.</exception>
    internal static SamusProjectileOccupant HeldShot(this SamusProjectile projectile) =>
        Ensure.NotNull(projectile).Occupant
            ?? throw new InvalidOperationException($"A check asks for the shot in place {projectile.Place}, where none is held.");

    /// <summary>The bomb <paramref name="bomb"/>'s place holds, which the check placed before asking (#2558).</summary>
    /// <exception cref="InvalidOperationException">No bomb holds the place.</exception>
    internal static SamusBombOccupant HeldBomb(this SamusBomb bomb) =>
        Ensure.NotNull(bomb).Occupant
            ?? throw new InvalidOperationException($"A check asks for the bomb in place {bomb.Place}, where none is held.");
}
