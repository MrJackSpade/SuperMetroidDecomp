namespace SuperMetroid.Core.Game;

/// <summary>Compiled launch velocities for the Norfair lava-jumping enemy (Squeept).</summary>
internal static class NorfairLavaJumpDefinitions
{
    /// <summary>
    /// The four signed 8.8 Y velocities at <c>$A2:BE86-$BE8D</c>. Native AI selects a
    /// two-byte record with <c>HIBYTE(random) &amp; 6</c>.
    /// </summary>
    private static readonly ushort[] InitialVerticalVelocities =
    [
        0xf7ff,
        0xf8fe,
        0xf9bf,
        0xfaff,
    ];

    /// <summary>Selects the exact launch velocity from the current native RNG word.</summary>
    internal static ushort InitialVerticalVelocity(ushort random) =>
        InitialVerticalVelocities[(random >> 9) & 3];
}
