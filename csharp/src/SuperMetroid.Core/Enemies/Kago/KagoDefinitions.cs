namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago's random jump selection.</summary>
internal static class KagoDefinitions
{
    /// <summary>The random field that adds zero to three <see cref="JumpHeightStepPixels"/> to a Kago bug's upward jump speed: the sample divided by 256, modulo 4.</summary>
    internal static readonly RandomSampleField JumpHeightSteps = new(8, 2);

    /// <summary>The upward jump speed every bug's jump starts from, in whole pixels per update.</summary>
    internal const int BaseJumpSpeedPixels = 8;

    /// <summary>The upward jump speed one jump height step adds, in whole pixels per update.</summary>
    internal const int JumpHeightStepPixels = 1;

    /// <summary>
    /// The random choice that sends a bug near its shell leftward: the sample divided by 256 being odd, the parity of
    /// <see cref="JumpHeightSteps"/>'s count, so an odd count of steps sends it left; the game reads it back from the
    /// chosen jump speed rather than drawing again.
    /// </summary>
    internal static readonly RandomSampleField LeftJump = new(8, 1);

    /// <summary>The random field that adds zero to seven updates to a new Kago bug's first wait: the sample modulo 8.</summary>
    internal static readonly RandomSampleField FirstIdleFrames = new(0, 3);

    /// <summary>The random field that adds zero to thirty-one updates to a landed Kago bug's wait: the sample modulo 32.</summary>
    internal static readonly RandomSampleField LandedIdleFrames = new(0, 5);
}
