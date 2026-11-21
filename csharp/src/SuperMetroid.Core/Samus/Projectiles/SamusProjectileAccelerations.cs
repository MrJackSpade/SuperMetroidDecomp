namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The acceleration each projectile takes by its direction.</summary>
internal static partial class SamusProjectileAccelerations
{
    /// <summary>
    /// The accelerations of a projectile moving in <paramref name="direction"/>, one of the ten firing directions.
    /// </summary>
    internal static SamusDirectionAccelerations For(SamusProjectileDirection direction) =>
        SamusProjectileMotionDefinitions.Accelerations[direction];
}
