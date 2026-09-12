namespace SuperMetroid.Core.Game;

/// <summary>Native pogo launch velocities and asymmetric accelerations, not presentation data.</summary>
public static class RidleyPogoDefinitions
{
    /// <summary>$A6:B94D, SetRidleyPogoSpeeds.upwardsAcceleration; six authored stages.</summary>
    private static ReadOnlySpan<ushort> Upward => [0x0a, 0x10, 0x20, 0x30, 0x40, 0x50];
    /// <summary>$A6:B959, SetRidleyPogoSpeeds.downwardsAccel; six authored stages.</summary>
    private static ReadOnlySpan<ushort> Downward => [0x10, 0x20, 0x40, 0x80, 0x400, 0x500];
    /// <summary>$A6:B975..B9A4, four .randomXSpeed rows; unsigned 8.8 magnitudes for NTSC.</summary>
    private static ReadOnlySpan<ushort> Horizontal =>
    [
        0x58, 0x70, 0xa0, 0xa8, 0xb0, 0xb8,
        0x78, 0x90, 0xc0, 0xc8, 0xd0, 0xd8,
        0x98, 0xb0, 0xe0, 0xe8, 0xf0, 0xf8,
        0xb8, 0xd0, 0x100, 0x108, 0x110, 0x118,
    ];
    /// <summary>$A6:B9A5..B9D4, four .randomYSpeed rows; signed 8.8 velocities for NTSC.</summary>
    private static ReadOnlySpan<short> Vertical =>
    [
        -0x1a0, -0x220, -0x320, -0x3e0, -0x580, -0x680,
        -0x200, -0x280, -0x380, -0x440, -0x5a0, -0x6a0,
        -0x220, -0x2a0, -0x3a0, -0x480, -0x5c0, -0x6c0,
        -0x240, -0x2c0, -0x3c0, -0x4a0, -0x5e0, -0x6e0,
    ];

    /// <summary>
    /// Resolves the four native pointer pairs at $A6:B965/B96D to their authored
    /// rows. Stage is the native acceleration index plus two; the first two rows
    /// remain represented even though ordinary health stages select two through five.
    /// </summary>
    public static (ushort X, ushort Y, ushort UpwardAcceleration, ushort DownwardAcceleration) Read(int pattern, int stage)
    {
        if ((uint)pattern >= 4) throw new ArgumentOutOfRangeException(nameof(pattern));
        if ((uint)stage >= 6) throw new ArgumentOutOfRangeException(nameof(stage));
        int index = pattern * 6 + stage;
        return (Horizontal[index], unchecked((ushort)Vertical[index]), Upward[stage], Downward[stage]);
    }
}
