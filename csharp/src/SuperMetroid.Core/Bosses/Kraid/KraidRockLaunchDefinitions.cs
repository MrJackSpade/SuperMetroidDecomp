using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The horizontal launch speeds of rocks spat by Kraid.</summary>
public static class KraidRockLaunchDefinitions
{
    /// <summary>Leftward 3.75-pixel/frame launch.</summary>
    private static SignedEightEight SlowLeft { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-960);
    /// <summary>Leftward 4-pixel/frame launch.</summary>
    private static SignedEightEight NormalLeft { get; } = SignedEightEight.FromPixels(-4);
    /// <summary>Leftward 4.5-pixel/frame launch.</summary>
    private static SignedEightEight FastLeft { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1152);
    /// <summary>Leftward 4.75-pixel/frame launch.</summary>
    private static SignedEightEight FastestLeft { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(-1216);

    /// <summary>
    /// Selects one of four launch speeds, each assigned two of the eight
    /// random choices. Accepts every sample and never advances RNG.
    /// </summary>
    /// <remarks>
    /// The rock spit selects a launch category from the sample divided by 2, modulo 8, not a motion
    /// sample or chronological random-number generator. Preserve the game's
    /// branch assignment even though each speed has the same one-quarter weight.
    /// </remarks>
    public static SignedEightEight FromRandom(int random) => Speeds[LaunchChoice.Of(random)];

    /// <summary>
    /// The gravity on a spat or rising rock, in 256ths of a pixel an update, each update. A rock is removed when its
    /// terrain probe finds a block, which it does at the latest as it leaves the room, at most fifty screens tall
    /// (<see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/>); from its steepest launch, five pixels an update
    /// upward, its fall speed therefore stays below 96 pixels an update.
    /// </summary>
    internal static SignedEightEight RockGravity { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>
    /// The gravity on a rock falling from the ceiling, each update; the fall restarts as <see cref="RestartingFallSpeed"/>
    /// states.
    /// </summary>
    internal static SignedEightEight CeilingRockGravity { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

    /// <summary>The horizontal speed a spat rock gains each update, drifting back toward Kraid's side.</summary>
    internal static SignedEightEight RockDrift { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(8);

    /// <summary>
    /// The horizontal speed at which a spat rock's drift starts again: reaching one pixel an update rightward, it
    /// drifts on from one pixel an update leftward.
    /// </summary>
    internal static SignedEightEight RockDriftRestart { get; } = SignedEightEight.FromPixels(1);

    /// <summary>The launch choice, one of eight: the sample divided by 2, modulo 8.</summary>
    private static readonly RandomSampleField LaunchChoice = new(1, 3);

    /// <summary>The launch speed of each of the eight <see cref="LaunchChoice"/> values, in order.</summary>
    private static readonly ImmutableArray<SignedEightEight> Speeds =
    [
        NormalLeft,
        SlowLeft,
        FastestLeft,
        FastLeft,
        FastestLeft,
        NormalLeft,
        FastLeft,
        SlowLeft,
    ];
}
