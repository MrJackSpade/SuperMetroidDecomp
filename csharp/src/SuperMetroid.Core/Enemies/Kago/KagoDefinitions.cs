namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago's quake and its bugs' random jump selection, waits and motion.</summary>
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

    /// <summary>How long the quake lasts when a shot strikes a Kago, in updates.</summary>
    internal const int ShotQuakeDuration = 16;

    /// <summary>How much longer than its first wait a new Kago bug waits before it first sounds, in updates.</summary>
    internal const int FirstSoundDelayPastWait = 4;

    /// <summary>The random field that adds zero to seven updates to a new Kago bug's first wait: the sample modulo 8.</summary>
    internal static readonly RandomSampleField FirstIdleFrames = new(0, 3);

    /// <summary>The random field that adds zero to thirty-one updates to a landed Kago bug's wait: the sample modulo 32.</summary>
    internal static readonly RandomSampleField LandedIdleFrames = new(0, 5);

    /// <summary>How much faster a bug falls each unobstructed airborne update: 224 256ths of a pixel (#2339).</summary>
    internal static SignedEightEight BugGravity { get; } = SignedEightEight.FromParts(0, 224);

    /// <summary>How fast a bug jumps sideways, two pixels an update, toward the way it jumps (#2339).</summary>
    internal static SignedEightEight BugHorizontalSpeed { get; } = SignedEightEight.FromParts(2, 0);

    /// <summary>The downward speed a rising bug starts falling at when it strikes a wall or the room above: a pixel an update (#2339).</summary>
    internal static SignedEightEight BugFallStartSpeed { get; } = SignedEightEight.FromParts(1, 0);

    /// <summary>How far, in pixels, a bug must be from its shell sideways before it blocks Samus's shots (#2339).</summary>
    internal const int BugBlocksShotsDistance = 23;

    /// <summary>How far, in pixels, a bug may stray from its shell sideways before its next jump heads back toward it (#2339).</summary>
    internal const int BugHomingDistance = 48;
}
