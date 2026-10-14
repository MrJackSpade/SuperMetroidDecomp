namespace SuperMetroid.Core.Game;

/// <summary>One native 12-byte <c>SamusSpeedTableEntry</c> from cartridge bank $90.</summary>
public readonly record struct SpeedTableEntry(
    ushort Acceleration,
    ushort AccelerationSubspeed,
    ushort MaximumSpeed,
    ushort MaximumSubspeed,
    ushort Deceleration,
    ushort DecelerationSubspeed)
{
    /// <summary>All six table fields are little-endian 16-bit words.</summary>
    public const int ByteCount = 12;
}
