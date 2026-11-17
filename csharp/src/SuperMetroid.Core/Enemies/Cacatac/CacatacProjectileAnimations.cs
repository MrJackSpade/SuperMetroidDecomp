using SuperMetroid.Core.Animation;
using P = SuperMetroid.Core.Enemies.Cacatac.CacatacSpikeDirection;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>
/// The Cacatac's spike animations, one per launch direction, which names it (#641, #2067). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class CacatacProjectileAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<CacatacSpikeDirection, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<CacatacSpikeDirection, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
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
    internal static IReadOnlyDictionary<CacatacSpikeDirection, Animation<CacatacSpikeDirection>> StartingAt { get; } = new Dictionary<CacatacSpikeDirection, Animation<CacatacSpikeDirection>>
    {
        [P.LeftFacingUp] = new([Whole(P.LeftFacingUp)], AnimationEnd.Holds),
        [P.UpLeft] = new([Whole(P.UpLeft)], AnimationEnd.Holds),
        [P.Up] = new([Whole(P.Up)], AnimationEnd.Holds),
        [P.UpRight] = new([Whole(P.UpRight)], AnimationEnd.Holds),
        [P.RightFacingUp] = new([Whole(P.RightFacingUp)], AnimationEnd.Holds),
        [P.LeftFacingDown] = new([Whole(P.LeftFacingDown)], AnimationEnd.Holds),
        [P.DownLeft] = new([Whole(P.DownLeft)], AnimationEnd.Holds),
        [P.Down] = new([Whole(P.Down)], AnimationEnd.Holds),
        [P.DownRight] = new([Whole(P.DownRight)], AnimationEnd.Holds),
        [P.RightFacingDown] = new([Whole(P.RightFacingDown)], AnimationEnd.Holds),
    };

    /// <summary>The animation of a spike launched in <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the ten spike directions.</exception>
    internal static Animation<CacatacSpikeDirection> StartingWith(CacatacSpikeDirection direction) =>
        StartingAt.TryGetValue(direction, out Animation<CacatacSpikeDirection>? animation)
            ? animation
            : throw new InvalidDataException($"Cacatac spike direction {direction} is outside the ten spike directions.");

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<CacatacSpikeDirection> Whole(CacatacSpikeDirection segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A still picture (#2275): <paramref name="frame"/>, held from the update it shows.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Hold(EnemyProjectileFrameId frame) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.HeldFrame(frame);
}
