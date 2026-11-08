namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago's random jump selection.</summary>
internal static class KagoDefinitions
{
    /// <summary>The random field that adds zero to three <see cref="JumpHeightStep"/>s to a Kago bug's upward jump magnitude: bits eight and nine.</summary>
    internal static readonly RandomSampleField JumpHeightSteps = new(8, 2);

    /// <summary>The upward jump magnitude one jump height step adds.</summary>
    internal const int JumpHeightStep = 256;

    /// <summary>
    /// The random bit that sends a bug near its shell leftward: bit eight, the low bit of <see cref="JumpHeightSteps"/>,
    /// so the game reads it back from the chosen jump magnitude rather than drawing again.
    /// </summary>
    internal static readonly RandomSampleField LeftJump = new(8, 1);

    /// <summary>The random field that adds zero to seven updates to a new Kago bug's first wait: bits zero to two.</summary>
    internal static readonly RandomSampleField FirstIdleFrames = new(0, 3);

    /// <summary>The random field that adds zero to thirty-one updates to a landed Kago bug's wait: bits zero to four.</summary>
    internal static readonly RandomSampleField LandedIdleFrames = new(0, 5);
}
