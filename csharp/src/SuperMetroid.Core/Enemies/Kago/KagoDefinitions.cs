namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago's random jump selection.</summary>
internal static class KagoDefinitions
{
    /// <summary>The random field that adds zero to three <see cref="JumpHeightStepPixels"/> to a Kago bug's upward jump speed.</summary>
    internal static readonly RandomSampleField JumpHeightSteps = new(8, 2);

    /// <summary>The upward jump speed every bug's jump starts from, in whole pixels per update.</summary>
    internal const int BaseJumpSpeedPixels = 8;

    /// <summary>The upward jump speed one jump height step adds, in whole pixels per update.</summary>
    internal const int JumpHeightStepPixels = 1;

    /// <summary>
    /// The random choice that sends a bug near its shell leftward: an odd count of <see cref="JumpHeightSteps"/> sends it
    /// left, so the jump's height and its way come from one random number rather than two.
    /// </summary>
    internal static readonly RandomSampleField LeftJump = new(8, 1);

    /// <summary>How long the quake lasts when a shot strikes a Kago, in updates.</summary>
    internal const int ShotQuakeDuration = 16;

    /// <summary>How much longer than its first wait a new Kago bug waits before it first sounds, in updates.</summary>
    internal const int FirstSoundDelayPastWait = 4;

    /// <summary>The random field that adds zero to seven updates to a new Kago bug's first wait.</summary>
    internal static readonly RandomSampleField FirstIdleFrames = new(0, 3);

    /// <summary>The random field that adds zero to thirty-one updates to a landed Kago bug's wait.</summary>
    internal static readonly RandomSampleField LandedIdleFrames = new(0, 5);
}
