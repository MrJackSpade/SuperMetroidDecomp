namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The acceleration each projectile takes by its direction.</summary>
internal static class SamusProjectileAccelerations
{
    /// <summary>
    /// The accelerations of a projectile moving in <paramref name="direction"/>. A projectile's direction is always one
    /// of the ten firing directions, so no read passes the tables.
    /// </summary>
    /// <exception cref="InvalidDataException">The direction is not one of the ten firing directions.</exception>
    internal static SamusDirectionAccelerations For(SamusProjectileDirection direction) =>
        SamusProjectileMotionDefinitions.Accelerations.TryGetValue(direction, out SamusDirectionAccelerations? accelerations)
            ? accelerations
            : throw new InvalidDataException($"Projectile direction {direction} is not a firing direction.");
}
