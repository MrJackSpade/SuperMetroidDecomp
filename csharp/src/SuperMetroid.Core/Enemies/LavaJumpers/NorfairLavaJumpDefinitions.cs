namespace SuperMetroid.Core.Enemies.LavaJumpers;

/// <summary>Compiled launch velocities for the Norfair lava-jumping enemy (Squeept).</summary>
internal static class NorfairLavaJumpDefinitions
{
    /// <summary>
    /// The four signed 8.8 Y velocities. The jump selects a
    /// two-byte record with <c>HIBYTE(random) &amp; 6</c>.
    /// </summary>
    private static readonly SignedEightEight[] InitialVerticalVelocities =
    [
        SignedEightEight.FromSignedRaw(-2049),
        SignedEightEight.FromSignedRaw(-1794),
        SignedEightEight.FromSignedRaw(-1601),
        SignedEightEight.FromSignedRaw(-1281),
    ];

    /// <summary>Selects the exact launch velocity from the current RNG word.</summary>
    internal static ushort InitialVerticalVelocity(int random) =>
        InitialVerticalVelocities[(random >> 9) & 3].RawValue;
}
