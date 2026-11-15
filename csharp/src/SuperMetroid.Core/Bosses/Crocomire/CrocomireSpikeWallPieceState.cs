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

    /// <summary>The piece's starting vertical speed: about four and a half pixels per update upward (#1936).</summary>
    private static readonly SignedSixteenSixteen LaunchFallSpeed = SignedSixteenSixteen.FromParts(-5, 34816);

    /// <summary>
    /// The piece's sideways acceleration, in 65536ths of a pixel per update per update, of which its velocity takes the
    /// whole 256ths each update (#1936); it starts at none and grows to its piece's cap.
    /// </summary>
    internal int Acceleration
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>The piece's vertical speed, pixels per update; it starts rising and gravity pulls it down.</summary>
    internal SignedSixteenSixteen FallSpeed { get; set; } = LaunchFallSpeed;
}
