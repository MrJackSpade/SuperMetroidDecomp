using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies.KiHunters;

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

    /// <summary>Parameter-one bit 15: the Ki-Hunter starts without wings; its wing record starts deleted.</summary>
    public const ushort WinglessParameterBit = 0x8000;

    /// <summary>A leftward swoop's angular acceleration whole word ($FFFF.E000, -1/8).</summary>
    public const ushort LeftSwoopAccelerationWhole = 0xffff;

    /// <summary>A leftward swoop's angular acceleration fraction.</summary>
    public const ushort LeftSwoopAccelerationFraction = 0xe000;

    /// <summary>A rightward swoop's angular acceleration fraction ($0000.2000, +1/8).</summary>
    public const ushort RightSwoopAccelerationFraction = 0x2000;

    /// <summary>The detached wing orbit's offset angle, seven-eighths of a byte-angle turn.</summary>
    public static SnesByteAngle DetachedOrbitOffsetAngle { get; } = new(0xe0);

    /// <summary>The detached wing's falling-arc offset angle, five-eighths of a byte-angle turn.</summary>
    public static SnesByteAngle FallingArcOffsetAngle { get; } = new(0xa0);

    /// <summary>The detached wing orbit's starting 16-bit angle, an eighth of a turn below zero.</summary>
    public static SnesAngle DetachedOrbitStartAngle { get; } = SnesAngle.FromRaw(0xe000);

    /// <summary>The detached wing's falling-arc starting 16-bit angle, three eighths of a turn below zero.</summary>
    public static SnesAngle FallingArcStartAngle { get; } = SnesAngle.FromRaw(0xa000);

    /// <summary>The detached speed accumulator stops summing once it reaches this.</summary>
    public const ushort DetachedSpeedThreshold = 0x2000;
}
