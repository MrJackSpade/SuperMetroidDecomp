namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago's random jump selection.</summary>
internal static class KagoDefinitions
{
    /// <summary>The random field that adds zero to three <see cref="JumpHeightStepPixels"/> to a Kago bug's upward jump speed: bits eight and nine.</summary>
    internal static readonly RandomSampleField JumpHeightSteps = new(8, 2);

    /// <summary>The upward jump speed every bug's jump starts from, in whole pixels per update.</summary>
    internal const int BaseJumpSpeedPixels = 8;

    /// <summary>The upward jump speed one jump height step adds, in whole pixels per update.</summary>
    internal const int JumpHeightStepPixels = 1;

    /// <summary>
    /// The random bit that sends a bug near its shell leftward: bit eight, the first bit <see cref="JumpHeightSteps"/>
    /// reads, so an odd count of steps sends it left; the game reads it back from the chosen jump speed rather than
    /// drawing again.
    /// </summary>
    internal static readonly RandomSampleField LeftJump = new(8, 1);

    /// <summary>The random field that adds zero to seven updates to a new Kago bug's first wait: bits zero to two.</summary>
    internal static readonly RandomSampleField FirstIdleFrames = new(0, 3);

    /// <summary>The random field that adds zero to thirty-one updates to a landed Kago bug's wait: bits zero to four.</summary>
    internal static readonly RandomSampleField LandedIdleFrames = new(0, 5);
}
