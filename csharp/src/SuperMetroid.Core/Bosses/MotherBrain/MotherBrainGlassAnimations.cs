using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassCue>;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassSegment;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's typed glass animations (#641, #1600, #2011), each beat the installed <see cref="EnemyProjectileFrameId"/>
/// it shows.
/// </summary>
internal static class MotherBrainGlassAnimations
{
    /// <summary>
    /// Every glass segment: a shard spins through its eight frames with the symmetric 4, 3, 2, 3 cadence twice; the
    /// sparkle alternates six and eight updates over four frames, then deletes itself.
    /// </summary>
    internal static AnimationSegments<MotherBrainGlassSegment, EnemyProjectileFrameId, MotherBrainGlassCue> Segments { get; } = new(
        new Dictionary<MotherBrainGlassSegment, Beat[]>
    {
        [P.ShardFlyingUp] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned0, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned45, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned90, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned135, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned180, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned225, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned270, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned315, 3),
        ],
        [P.ShardFlyingUpRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned0, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned45, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned90, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned135, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned180, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned225, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned270, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned315, 3),
        ],
        [P.ShardFlyingRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned0, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned45, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned90, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned135, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned180, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned225, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned270, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned315, 3),
        ],
        [P.ShardFlyingDownRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned0, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned45, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned90, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned135, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned180, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned225, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned270, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned315, 3),
        ],
        [P.ShardFlyingDown] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned0, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned45, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned90, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned135, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned180, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned225, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned270, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned315, 3),
        ],
        [P.ShardFlyingDownLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned0, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned45, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned90, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned135, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned180, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned225, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned270, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned315, 3),
        ],
        [P.ShardFlyingLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned0, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned45, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned90, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned135, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned180, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned225, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned270, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned315, 3),
        ],
        [P.ShardFlyingUpLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned0, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned45, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned90, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned135, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned180, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned225, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned270, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned315, 3),
        ],
        [P.Sparkle] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassSparkleCross, 6),
            Show(EnemyProjectileFrameId.MotherBrainGlassSparkleCrossSmall, 8),
            Show(EnemyProjectileFrameId.MotherBrainGlassSparkleDiagonal, 6),
            Show(EnemyProjectileFrameId.MotherBrainGlassSparkleDiagonalFaint, 8),
            new Beat.Event(MotherBrainGlassCue.Delete),
        ],
    });

    /// <summary>Each glass animation: a shard's spin loops for good, and the deleted sparkle rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainGlassSegment, Animation<MotherBrainGlassSegment>> StartingAt { get; } =
        Enum.GetValues<MotherBrainGlassSegment>().ToDictionary(segment => segment, segment => new Animation<MotherBrainGlassSegment>(
            [new(segment, 0, Segments.Beats(segment).Count, 1)],
            segment == P.Sparkle ? AnimationEnd.Holds : AnimationEnd.LoopsFrom(0)));

    /// <summary>
    /// The spin a shard launched at each sixteenth of the turn plays, in turn order clockwise from straight up: the spins
    /// span one, two, three and two sixteenths, and repeat so in the opposite half-turn.
    /// </summary>
    internal static TurnArcs<MotherBrainGlassSegment> ShardAnimations { get; } = new(TrigonometryAngle.Zero,
        P.ShardFlyingUp,
        P.ShardFlyingUpRight, P.ShardFlyingUpRight,
        P.ShardFlyingRight, P.ShardFlyingRight, P.ShardFlyingRight,
        P.ShardFlyingDownRight, P.ShardFlyingDownRight,
        P.ShardFlyingDown,
        P.ShardFlyingDownLeft, P.ShardFlyingDownLeft,
        P.ShardFlyingLeft, P.ShardFlyingLeft, P.ShardFlyingLeft,
        P.ShardFlyingUpLeft, P.ShardFlyingUpLeft);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static Beat Show(EnemyProjectileFrameId frame, int duration) => new Beat.Frame(frame, duration);
}
