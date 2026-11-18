namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago's quake and its bugs' random jump selection, waits and motion.</summary>
internal static class KagoDefinitions
{
    /// <summary>The random field that adds zero to three <see cref="JumpHeightStepPixels"/> to a Kago bug's upward jump speed.</summary>
    internal static readonly RandomSampleField JumpHeightSteps = new(8, 2);

    /// <summary>The upward jump speed every bug's jump starts from, in whole pixels per update.</summary>
    internal const int BaseJumpSpeedPixels = 8;

    /// <summary>The upward jump speed one jump height step adds, in whole pixels per update.</summary>
    internal const int JumpHeightStepPixels = 1;

    /// <summary>
    /// Whether a bug near its shell jumps leftward (#2479): an odd count of <see cref="JumpHeightSteps"/> sends it left, one
    /// jump in two, so the jump's height and its way come from one random number rather than two.
    /// </summary>
    internal static readonly RandomChance BugJumpsLeft = RandomChance.Highest(1, new RandomSampleField(8, 1));

    /// <summary>How long the quake lasts when a shot strikes a Kago, in updates.</summary>
    internal const int ShotQuakeDuration = 16;

    /// <summary>How many updates after its first hop a new Kago bug first sounds.</summary>
    internal const int FirstSoundPastFirstHop = 3;

    /// <summary>
    /// The shortest wait of a Kago bug on the ground, in updates, the one it hops on included: two, to which a random field
    /// adds more.
    /// </summary>
    internal const int ShortestGroundWait = 2;

    /// <summary>The random field that adds zero to seven updates to a new Kago bug's first wait.</summary>
    internal static readonly RandomSampleField FirstIdleFrames = new(0, 3);

    /// <summary>The random field that adds zero to thirty-one updates to a landed Kago bug's wait.</summary>
    internal static readonly RandomSampleField LandedIdleFrames = new(0, 5);

    /// <summary>How much faster a bug falls each unobstructed airborne update (#2339).</summary>
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
