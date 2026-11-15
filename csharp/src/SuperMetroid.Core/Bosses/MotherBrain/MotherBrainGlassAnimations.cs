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
            Show(EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassFirstShardGroupEighth, 3),
        ],
        [P.ShardFlyingUpRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassSecondShardGroupEighth, 3),
        ],
        [P.ShardFlyingRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassThirdShardGroupEighth, 3),
        ],
        [P.ShardFlyingDownRight] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassFourthShardGroupEighth, 3),
        ],
        [P.ShardFlyingDown] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassFifthShardGroupEighth, 3),
        ],
        [P.ShardFlyingDownLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassSixthShardGroupEighth, 3),
        ],
        [P.ShardFlyingLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassSeventhShardGroupEighth, 3),
        ],
        [P.ShardFlyingUpLeft] =
        [
            Show(EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFirst, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSecond, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupThird, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFourth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupFifth, 4),
            Show(EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSixth, 3),
            Show(EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupSeventh, 2),
            Show(EnemyProjectileFrameId.MotherBrainGlassEighthShardGroupEighth, 3),
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
