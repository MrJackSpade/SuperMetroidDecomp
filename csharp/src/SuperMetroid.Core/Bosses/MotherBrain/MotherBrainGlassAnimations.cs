using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassCue>.Unpaused;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassSegment;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's typed glass animations (#641, #1600, #2011), each beat the installed <see cref="EnemyProjectileFrameId"/>
/// it shows.
/// </summary>
internal static partial class MotherBrainGlassAnimations
{
    /// <summary>
    /// Every glass segment: a shard spins through its frames with a symmetric cadence, twice; the sparkle alternates
    /// its two holds over its frames, then deletes itself.
    /// </summary>
    internal static UnpausedAnimationSegments<MotherBrainGlassSegment, EnemyProjectileFrameId, MotherBrainGlassCue> Segments { get; } = new(
        new Dictionary<MotherBrainGlassSegment, Beat[]>
    {
        [P.ShardFlyingUp] = [
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned0, EnemyProjectileFrameId.MotherBrainGlassShardUpTurned45, EnemyProjectileFrameId.MotherBrainGlassShardUpTurned90, EnemyProjectileFrameId.MotherBrainGlassShardUpTurned135),
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardUpTurned180, EnemyProjectileFrameId.MotherBrainGlassShardUpTurned225, EnemyProjectileFrameId.MotherBrainGlassShardUpTurned270, EnemyProjectileFrameId.MotherBrainGlassShardUpTurned315),
        ],
        [P.ShardFlyingUpRight] = [
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned0, EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned45, EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned90, EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned135),
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned180, EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned225, EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned270, EnemyProjectileFrameId.MotherBrainGlassShardUpRightTurned315),
        ],
        [P.ShardFlyingRight] = [
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned0, EnemyProjectileFrameId.MotherBrainGlassShardRightTurned45, EnemyProjectileFrameId.MotherBrainGlassShardRightTurned90, EnemyProjectileFrameId.MotherBrainGlassShardRightTurned135),
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardRightTurned180, EnemyProjectileFrameId.MotherBrainGlassShardRightTurned225, EnemyProjectileFrameId.MotherBrainGlassShardRightTurned270, EnemyProjectileFrameId.MotherBrainGlassShardRightTurned315),
        ],
        [P.ShardFlyingDownRight] = [
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned0, EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned45, EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned90, EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned135),
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned180, EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned225, EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned270, EnemyProjectileFrameId.MotherBrainGlassShardDownRightTurned315),
        ],
        [P.ShardFlyingDown] = [
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned0, EnemyProjectileFrameId.MotherBrainGlassShardDownTurned45, EnemyProjectileFrameId.MotherBrainGlassShardDownTurned90, EnemyProjectileFrameId.MotherBrainGlassShardDownTurned135),
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardDownTurned180, EnemyProjectileFrameId.MotherBrainGlassShardDownTurned225, EnemyProjectileFrameId.MotherBrainGlassShardDownTurned270, EnemyProjectileFrameId.MotherBrainGlassShardDownTurned315),
        ],
        [P.ShardFlyingDownLeft] = [
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned0, EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned45, EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned90, EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned135),
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned180, EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned225, EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned270, EnemyProjectileFrameId.MotherBrainGlassShardDownLeftTurned315),
        ],
        [P.ShardFlyingLeft] = [
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned0, EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned45, EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned90, EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned135),
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned180, EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned225, EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned270, EnemyProjectileFrameId.MotherBrainGlassShardLeftTurned315),
        ],
        [P.ShardFlyingUpLeft] = [
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned0, EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned45, EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned90, EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned135),
            .. HalfSpin(EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned180, EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned225, EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned270, EnemyProjectileFrameId.MotherBrainGlassShardUpLeftTurned315),
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

    /// <summary>The glass segments whose animation holds once shown rather than looping: the deleted sparkle.</summary>
    private static readonly MotherBrainGlassSegment[] HoldingSegments = [P.Sparkle];

    /// <summary>Each glass animation: a shard's spin loops for good, and the deleted sparkle rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainGlassSegment, Animation<MotherBrainGlassSegment>> StartingAt { get; } =
        Enum.GetValues<MotherBrainGlassSegment>().ToDictionary(segment => segment, segment => HoldingSegments.Contains(segment)
            ? new Animation<MotherBrainGlassSegment>([Segments.Whole(segment)], AnimationEnd.Holds)
            : Animation<MotherBrainGlassSegment>.Looping([Segments.Whole(segment)]));

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

    /// <summary>
    /// Half a shard's spin through its heading's turns, <paramref name="square"/> to <paramref name="leaning"/>, each shown as
    /// long as the spin's cadence holds it (#2557): longest square on, shortest edge on; a spin is two such halves.
    /// </summary>
    private static Beat[] HalfSpin(EnemyProjectileFrameId square, EnemyProjectileFrameId turning, EnemyProjectileFrameId edgeOn,
        EnemyProjectileFrameId leaning) =>
        [Show(square, 4), Show(turning, 3), Show(edgeOn, 2), Show(leaning, 3)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static Beat Show(EnemyProjectileFrameId frame, int duration) => new Beat.Frame(frame, duration);
}
