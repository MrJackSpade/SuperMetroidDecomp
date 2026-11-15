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
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpEighth, 3),
        ],
        [P.ShardFlyingUpRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpRightFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpRightSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpRightThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpRightFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpRightFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpRightSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpRightSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpRightEighth, 3),
        ],
        [P.ShardFlyingRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingRightFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingRightSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingRightThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingRightFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingRightFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingRightSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingRightSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingRightEighth, 3),
        ],
        [P.ShardFlyingDownRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownRightFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownRightSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownRightThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownRightFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownRightFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownRightSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownRightSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownRightEighth, 3),
        ],
        [P.ShardFlyingDown] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownEighth, 3),
        ],
        [P.ShardFlyingDownLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownLeftFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownLeftSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownLeftThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownLeftFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownLeftFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownLeftSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownLeftSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingDownLeftEighth, 3),
        ],
        [P.ShardFlyingLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingLeftFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingLeftSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingLeftThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingLeftFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingLeftFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingLeftSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingLeftSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingLeftEighth, 3),
        ],
        [P.ShardFlyingUpLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpLeftFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpLeftSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpLeftThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpLeftFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpLeftFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpLeftSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpLeftSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassShardFlyingUpLeftEighth, 3),
        ],
        [P.Sparkle] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassSparkleFirst, 6),
            Show(EnemyProjectileFrameId.MotherBrainGlassSparkleSecond, 8),
            Show(EnemyProjectileFrameId.MotherBrainGlassSparkleThird, 6),
            Show(EnemyProjectileFrameId.MotherBrainGlassSparkleFourth, 8),
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
