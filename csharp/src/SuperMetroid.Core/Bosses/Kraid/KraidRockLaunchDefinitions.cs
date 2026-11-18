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
    /// The launch speed a spat rock takes, one of four, chosen from the random number's eight rolls in turn (#2510): normal,
    /// slow, fastest and fast, then fastest, normal, fast and slow, so each speed is taken by two rolls, a quarter of them.
    /// </summary>
    /// <remarks>
    /// The rock spit picks its launch from the current random number, not from its motion or a fresh random number.
    /// </remarks>
    internal static readonly WeightedRandomChoice<SignedEightEight> SpitLaunch = WeightedRandomChoice<SignedEightEight>.InTurn(
        new RandomSampleField(1, 3), NormalLeft, SlowLeft, FastestLeft, FastLeft, FastestLeft, NormalLeft, FastLeft, SlowLeft);

    /// <summary>
    /// The gravity on a spat or rising rock, each update. A rock is removed when its
    /// terrain probe finds a block, which it does at the latest as it leaves the room, at most fifty screens tall
    /// (<see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/>); from its steepest launch, five pixels an update
    /// upward, its fall speed therefore stays below 96 pixels an update.
    /// </summary>
    internal static SignedEightEight RockGravity { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(64);

    /// <summary>
    /// The gravity on a rock falling from the ceiling, each update. The rock starts falling
    /// at under half a pixel an update and is removed when its terrain probe finds a block, at the latest as it leaves the
    /// room, at most fifty screens tall (<see cref="Rooms.Scrolling.RoomScrollGrid.KeptCellCount"/>); its fall speed
    /// therefore stays below 50 pixels an update, and the game's restart of a fall reaching 64 pixels an update is never
    /// reached (#1795).
    /// </summary>
    internal static SignedEightEight CeilingRockGravity { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(24);

    /// <summary>The upward speed a rock thrown up by Kraid's rise or growth starts with: five pixels an update.</summary>
    internal static SignedEightEight RisingRockRise { get; } = SignedEightEight.FromPixels(-5);

    /// <summary>The horizontal speed a spat rock gains each update, drifting back toward Kraid's side.</summary>
    internal static SignedEightEight RockDrift { get; } = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(8);

    /// <summary>
    /// The horizontal speed at which a spat rock's drift starts again: reaching one pixel an update rightward, it
    /// drifts on from one pixel an update leftward.
    /// </summary>
    internal static SignedEightEight RockDriftRestart { get; } = SignedEightEight.FromPixels(1);
}
