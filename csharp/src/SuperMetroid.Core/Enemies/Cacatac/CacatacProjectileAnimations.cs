using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacSpikeDirection;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The Cacatac's spike animations, one per launch direction, which names it (#641, #2067). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static partial class CacatacProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static UnpausedAnimationSegments<CacatacSpikeDirection, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<CacatacSpikeDirection, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused[]>
    {
        [P.LeftFacingUp] = [Hold(EnemyProjectileFrameId.CacatacSpikeLeftHigh)],
        [P.UpLeft] = [Hold(EnemyProjectileFrameId.CacatacSpikeUpLeft)],
        [P.Up] = [Hold(EnemyProjectileFrameId.CacatacSpikeUp)],
        [P.UpRight] = [Hold(EnemyProjectileFrameId.CacatacSpikeUpRight)],
        [P.RightFacingUp] = [Hold(EnemyProjectileFrameId.CacatacSpikeRightHigh)],
        [P.LeftFacingDown] = [Hold(EnemyProjectileFrameId.CacatacSpikeLeftLow)],
        [P.DownLeft] = [Hold(EnemyProjectileFrameId.CacatacSpikeDownLeft)],
        [P.Down] = [Hold(EnemyProjectileFrameId.CacatacSpikeDown)],
        [P.DownRight] = [Hold(EnemyProjectileFrameId.CacatacSpikeDownRight)],
        [P.RightFacingDown] = [Hold(EnemyProjectileFrameId.CacatacSpikeRightLow)],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at: each spike a still picture (#2275).</summary>
    internal static IdentityTable<CacatacSpikeDirection, Animation<CacatacSpikeDirection>> StartingAt { get; } = new(Segments.EachEnding(AnimationEnd.Holds,
        P.LeftFacingUp, P.UpLeft, P.Up, P.UpRight, P.RightFacingUp, P.LeftFacingDown, P.DownLeft, P.Down, P.DownRight,
        P.RightFacingDown));

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Unpaused Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.HeldFrame(frame);
}
