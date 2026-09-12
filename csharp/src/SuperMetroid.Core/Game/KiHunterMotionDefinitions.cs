namespace SuperMetroid.Core.Game;

/// <summary>Native KiHunter trigger, fixed-point acceleration and detached-wing orbit constants.</summary>
public static class KiHunterMotionDefinitions
{
    /// <summary>$A8:F180, XProximityToActivateSwoop, tested with native signed distance comparison.</summary>
    public const ushort SwoopTriggerDistance = 96;
    /// <summary>$A8:F182, fractional acceleration added by both F55A falling and F5E4 hopping.</summary>
    public const ushort GravityFraction = 0xe000;
    /// <summary>$A8:F184, whole acceleration plus carry from the fractional addition.</summary>
    public const ushort GravityWhole = 0;
    /// <summary>$A8:F186 low byte, fallingWingsArcRadius, used by detached-wing setup and both arcs.</summary>
    public const byte DetachedWingRadius = 48;
}
