using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// What a piece of Crocomire's crumbling spike wall keeps: its place in the wall, its sideways acceleration and its falling
/// speed.
/// </summary>
/// <param name="piece">The piece's place in the wall, zero for the top piece.</param>
internal sealed class CrocomireSpikeWallPieceState(int piece) : EnemyProjectileState
{
    /// <summary>The piece's place in the wall, zero for the top piece; it sets the piece's height, motion and landing sound.</summary>
    internal int Piece { get; } = Ensure.BetweenInclusive(0, CrocomireSpikeMotionDefinitions.PieceCount - 1, piece);

    /// <summary>The piece's starting vertical speed: four and fifteen thirty-seconds pixels per update upward (#1936, #2181).</summary>
    private static readonly SignedSixteenSixteen LaunchFallSpeed = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-1144);

    /// <summary>
    /// The piece's sideways acceleration, in pixels per update per update, of which its velocity takes the whole 256ths of a
    /// pixel each update (#1936, #2181); it starts at none and grows to its piece's cap.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The acceleration set is negative.</exception>
    internal SignedSixteenSixteen Acceleration
    {
        get;
        set => field = value.IsNegative
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "A spike-wall piece's sideways acceleration is never negative.")
            : value;
    }

    /// <summary>The piece's vertical speed, pixels per update; it starts rising and gravity pulls it down.</summary>
    internal SignedSixteenSixteen FallSpeed { get; set; } = LaunchFallSpeed;
}
