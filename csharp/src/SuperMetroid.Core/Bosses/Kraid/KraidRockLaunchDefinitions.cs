namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The horizontal launch speeds of rocks spat by Kraid.</summary>
public static class KraidRockLaunchDefinitions
{
    /// <summary>Leftward 3.75-pixel/frame launch.</summary>
    private static SignedEightEight SlowLeft { get; } = SignedEightEight.FromSignedRaw(-15 * 64);
    /// <summary>Leftward 4-pixel/frame launch.</summary>
    private static SignedEightEight NormalLeft { get; } = SignedEightEight.FromSignedRaw(-4 * 256);
    /// <summary>Leftward 4.5-pixel/frame launch.</summary>
    private static SignedEightEight FastLeft { get; } = SignedEightEight.FromSignedRaw(-18 * 64);
    /// <summary>Leftward 4.75-pixel/frame launch.</summary>
    private static SignedEightEight FastestLeft { get; } = SignedEightEight.FromSignedRaw(-19 * 64);

    /// <summary>
    /// Selects one of four launch speeds, each assigned two of the eight
    /// RNG-bit choices. Preserves every ushort input and never advances RNG.
    /// </summary>
    /// <remarks>
    /// The rock spit selects a launch category from RNG bits1..3, not a motion
    /// sample or chronological random-number generator. Preserve the original
    /// branch assignment even though each speed has the same one-quarter weight.
    /// </remarks>
    public static SignedEightEight FromRandom(int random) => Speeds[random & ChoiceMask];

    /// <summary>
    /// The gravity on a spat or rising rock, in 256ths of a pixel an update, each update. A rock is removed when its
    /// terrain probe finds a block, which it does at the latest as it leaves the room, at most fifty screens tall
    /// (<see cref="Rooms.Scrolling.RoomScrollGrid.StorageByteCount"/>); from its steepest launch, five pixels an update
    /// upward, its fall speed therefore stays below 96 pixels an update.
    /// </summary>
    internal static SignedEightEight RockGravity { get; } = SignedEightEight.FromSignedRaw(64);

    /// <summary>
    /// The gravity on a rock falling from the ceiling, each update; the fall restarts as <see cref="RestartingFallSpeed"/>
    /// states.
    /// </summary>
    internal static SignedEightEight CeilingRockGravity { get; } = SignedEightEight.FromSignedRaw(24);

    /// <summary>The horizontal speed a spat rock gains each update, drifting back toward Kraid's side.</summary>
    internal static SignedEightEight RockDrift { get; } = SignedEightEight.FromSignedRaw(8);

    /// <summary>
    /// The horizontal speed at which a spat rock's drift starts again: reaching one pixel an update rightward, it
    /// drifts on from one pixel an update leftward.
    /// </summary>
    internal static SignedEightEight RockDriftRestart { get; } = SignedEightEight.FromSignedRaw(256);

    /// <summary>Keeps RNG bits 1..3, the eight even launch choices.</summary>
    private const int ChoiceMask = 0b1110;

    /// <summary>The launch speed of each of the eight even RNG choices.</summary>
    private static readonly Dictionary<int, SignedEightEight> Speeds = new()
    {
        [0] = NormalLeft,
        [2] = SlowLeft,
        [4] = FastestLeft,
        [6] = FastLeft,
        [8] = FastestLeft,
        [10] = NormalLeft,
        [12] = FastLeft,
        [14] = SlowLeft,
    };
}
