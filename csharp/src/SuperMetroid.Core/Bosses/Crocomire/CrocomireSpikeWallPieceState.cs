using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What a piece of Crocomire's crumbling spike wall keeps: its sideways acceleration and its falling speed.</summary>
internal sealed class CrocomireSpikeWallPieceState : EnemyProjectileState
{
    /// <summary>The piece's starting vertical speed: just under five pixels per update upward.</summary>
    private static readonly SignedSixteenSixteen LaunchFallSpeed = SignedSixteenSixteen.FromParts(-5, 34816);

    /// <summary>The piece's sideways acceleration, in 1/256 pixel per update per update; it grows to its slot's cap.</summary>
    internal int Acceleration
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>The piece's vertical speed, pixels per update; it starts rising and gravity pulls it down.</summary>
    internal SignedSixteenSixteen FallSpeed { get; set; } = LaunchFallSpeed;
}
