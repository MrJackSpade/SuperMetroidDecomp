namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>Immutable authored Bull shot-reaction angles; live timers remain enemy state.</summary>
public static class BullMovementDefinitions
{
    /// <summary>
    /// The compass angle each of the ten facing-dependent projectile directions
    /// moves the Bull at. Down occupies two slots; up appears at both ends; each distinct direction advances a 32-unit
    /// octant.
    /// </summary>
    internal static readonly int[] ShotAngles = [192, 224, 0, 32, 64, 64, 96, 128, 160, 192];
}
