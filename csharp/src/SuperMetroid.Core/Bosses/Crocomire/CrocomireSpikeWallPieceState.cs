using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// What a piece of Crocomire's crumbling spike wall keeps: which piece of the wall it is, its sideways acceleration and its
/// falling speed.
/// </summary>
/// <param name="piece">The piece of the wall it is, which sets its motion and its landing sound.</param>
internal sealed class CrocomireSpikeWallPieceState(CrocomireSpikeWallPiece piece) : EnemyProjectileState
{
    /// <summary>The piece of the wall it is, which sets its motion and its landing sound.</summary>
    internal CrocomireSpikeWallPiece Piece { get; } = Ensure.NotNull(piece);

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
    internal SignedSixteenSixteen FallSpeed { get; set; } = CrocomireSpikeMotionDefinitions.WallPieceLaunchSpeed;
}
