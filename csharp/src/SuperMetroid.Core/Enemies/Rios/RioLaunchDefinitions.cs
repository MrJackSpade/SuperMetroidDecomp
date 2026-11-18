namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// Fixed launch magnitudes for the three Rio families, each with its own behaviour, how near Samus brings each to dive
/// and how its dive slows (#2156).
/// </summary>
internal static class RioLaunchDefinitions
{
    /// <summary>Rio initial downward velocity, five and a half pixels per update.</summary>
    internal static SignedEightEight RioYVelocity { get; } = SignedEightEight.FromParts(5, 128);
    /// <summary>Rio initial horizontal speed, one and a half pixels per update, toward the side its launch chooses.</summary>
    internal static SignedEightEight RioXVelocity { get; } = SignedEightEight.FromParts(1, 128);
    /// <summary>Norfair Rio horizontal speed, one pixel per update.</summary>
    internal static SignedEightEight NorfairXVelocity { get; } = SignedEightEight.FromParts(1, 0);
    /// <summary>Lower Norfair Rio initial downward velocity, seven pixels per update.</summary>
    internal static SignedEightEight LowerNorfairYVelocity { get; } = SignedEightEight.FromParts(7, 0);
    /// <summary>How far below a live Lower Norfair Rio its flames follow, in pixels.</summary>
    internal const int LowerNorfairFlameDrop = 12;

    /// <summary>Lower Norfair Rio horizontal speed, one pixel per update.</summary>
    internal static SignedEightEight LowerNorfairXVelocity { get; } = SignedEightEight.FromParts(1, 0);

    /// <summary>The faster of the Norfair Rio's two downward launch velocities, seven pixels per update, chosen when <see cref="SlowLaunch"/> is not taken.</summary>
    internal static SignedEightEight NorfairFastYVelocity { get; } = SignedEightEight.FromParts(7, 0);
    /// <summary>The slower Norfair Rio downward launch velocity, five and three quarter pixels per update, chosen when <see cref="SlowLaunch"/> is taken.</summary>
    internal static SignedEightEight NorfairSlowYVelocity { get; } = SignedEightEight.FromParts(5, 192);

    /// <summary>
    /// The chance, taken by half the random numbers, that sends a diving Norfair Rio down at its slower launch velocity; the
    /// random number that let it attack decides it.
    /// </summary>
    internal static readonly RandomChance SlowLaunch = RandomChance.Highest(1, new RandomSampleField(2, 1));

    /// <summary>
    /// The chance, taken by one random number in four, that a Norfair or Lower Norfair Rio waiting for Samus holds back from
    /// attacking on its update, however near she is.
    /// </summary>
    internal static readonly RandomClearBitsChance AttackHeldBack = new(new RandomSampleField(0, 1), new RandomSampleField(8, 1));

    /// <summary>How close horizontally Samus comes, in pixels and exclusive, before a Rio dives.</summary>
    internal const int RioTriggerDistance = 160;

    /// <summary>How much a Rio's vertical speed falls each update of its dive and rebound, 24 256ths of a pixel.</summary>
    internal static SignedEightEight RioGravity { get; } = SignedEightEight.FromParts(0, 24);

    /// <summary>How close horizontally Samus comes, in pixels and exclusive, before a Norfair Rio may dive.</summary>
    internal const int NorfairTriggerDistance = 192;

    /// <summary>How much a Norfair Rio's vertical speed falls each update of its dive and return, an eighth of a pixel.</summary>
    internal static SignedEightEight NorfairGravity { get; } = SignedEightEight.FromParts(0, 32);

    /// <summary>How close horizontally Samus comes, in pixels and exclusive, before a Lower Norfair Rio may swoop.</summary>
    internal const int LowerNorfairTriggerDistance = 112;

    /// <summary>How much a Lower Norfair Rio's vertical speed falls each update of its flight, an eighth of a pixel.</summary>
    internal static SignedEightEight LowerNorfairGravity { get; } = SignedEightEight.FromParts(0, 32);

    /// <summary>
    /// The vertical speed every Rio family starts back up to its perch at, a 256th of a pixel an update upward: a Rio as it
    /// stops homing on Samus, a Norfair or Lower Norfair Rio as its dive ends.
    /// </summary>
    internal static SignedEightEight ReturnYVelocity { get; } = -SignedEightEight.FromParts(0, 1);
}
