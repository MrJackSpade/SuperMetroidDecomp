using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using P = SuperMetroid.Core.Bosses.Phantoon.PhantoonFlameSegment;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's typed flame animations (#641, #1600, #2011), each beat the installed <see cref="EnemyProjectileFrameId"/> it
/// shows.
/// </summary>
internal static class PhantoonFlameAnimations
{
    /// <summary>The poses in each of a casual flame's three falling phases.</summary>
    private const int FallingPoseCount = 2;
    /// <summary>How many times each falling phase plays its two poses.</summary>
    private const int FallingRepeatCount = 4;

    /// <summary>
    /// Every flame animation's steps (#1788): a flame pose lasts five updates, a landing casual flame shows one pose for one
    /// update and a rain flame's impact one for eight; a falling flame's steps are its three falling phases' two one-update
    /// poses each, in turn. A resting casual flame starts from the bouncing flame's last two poses, and a falling one from the
    /// rain impact's.
    /// </summary>
    internal static AnimationSegments<PhantoonFlameSegment, EnemyProjectileFrameId, PhantoonFlameCue> Segments { get; } = new(
        new Dictionary<PhantoonFlameSegment, AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>[]>
    {
        [P.DestroyableIdle] =
        [
            Show(EnemyProjectileFrameId.PhantoonDestroyableIdleFirst, 5),
            Show(EnemyProjectileFrameId.PhantoonDestroyableIdleSecond, 5),
            Show(EnemyProjectileFrameId.PhantoonDestroyableIdleThird, 5),
        ],
        [P.CasualHitGround] = [Show(EnemyProjectileFrameId.PhantoonDestroyableIdleFifth, 1)],
        [P.CasualBouncing] =
        [
            Show(EnemyProjectileFrameId.PhantoonCasualBouncingFirst, 5),
            Show(EnemyProjectileFrameId.PhantoonCasualBouncingSecond, 5),
            Show(EnemyProjectileFrameId.PhantoonCasualBouncingThird, 5),
        ],
        [P.CasualResting] =
        [
            Show(EnemyProjectileFrameId.PhantoonCasualBouncingFifth, 5),
            Show(EnemyProjectileFrameId.PhantoonCasualBouncingSixth, 5),
            Show(EnemyProjectileFrameId.PhantoonCasualRestingThird, 5),
            Show(EnemyProjectileFrameId.PhantoonCasualRestingFourth, 5),
            Show(EnemyProjectileFrameId.PhantoonCasualRestingFifth, 5),
            Show(EnemyProjectileFrameId.PhantoonCasualRestingSixth, 5),
        ],
        [P.Dying] =
        [
            Show(EnemyProjectileFrameId.PhantoonDyingFirst, 5),
            Show(EnemyProjectileFrameId.PhantoonDyingSecond, 5),
            Show(EnemyProjectileFrameId.PhantoonDyingThird, 5),
            Show(EnemyProjectileFrameId.PhantoonDyingFourth, 5),
            Cue(PhantoonFlameCue.Delete),
        ],
        [P.RainImpact] = [Show(EnemyProjectileFrameId.PhantoonRainImpactFirst, 8)],
        [P.CasualFalling] =
        [
            Show(EnemyProjectileFrameId.PhantoonRainImpactFourth, 1),
            Show(EnemyProjectileFrameId.PhantoonRainImpactFifth, 1),
            Show(EnemyProjectileFrameId.PhantoonCasualFallingThird, 1),
            Show(EnemyProjectileFrameId.PhantoonCasualFallingFourth, 1),
            Show(EnemyProjectileFrameId.PhantoonCasualFallingFifth, 1),
            Show(EnemyProjectileFrameId.PhantoonCasualFallingSixth, 1),
        ],
        [P.StartingFlame] =
        [
            Show(EnemyProjectileFrameId.PhantoonStartingFlameFirst, 5),
            Show(EnemyProjectileFrameId.PhantoonStartingFlameSecond, 5),
            Show(EnemyProjectileFrameId.PhantoonStartingFlameThird, 5),
        ],
        [P.Delete] = [Cue(PhantoonFlameCue.Delete)],
        [P.DestroyableShot] =
        [
            Show(EnemyProjectileFrameId.PhantoonDestroyableShotFirst, 5),
            Show(EnemyProjectileFrameId.PhantoonDestroyableShotSecond, 5),
            Show(EnemyProjectileFrameId.PhantoonDestroyableShotThird, 5),
            Show(EnemyProjectileFrameId.PhantoonDestroyableShotFourth, 5),
            Cue(PhantoonFlameCue.SpawnDrop),
            Cue(PhantoonFlameCue.Delete),
        ],
    });

    /// <summary>
    /// How each flame animation plays its steps: the loops cycle for good, a resting or impacting flame runs on into dying,
    /// which no owner starts on its own (#2166), and a falling flame plays each falling phase's poses four times, the three
    /// phases in turn for good.
    /// </summary>
    internal static IReadOnlyDictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>> StartingAt { get; } = new Dictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>>
    {
        [P.DestroyableIdle] = new([Whole(P.DestroyableIdle)], AnimationEnd.LoopsFrom(0)),
        [P.CasualHitGround] = new([Whole(P.CasualHitGround)], AnimationEnd.Holds),
        [P.CasualBouncing] = new([Whole(P.CasualBouncing)], AnimationEnd.LoopsFrom(0)),
        [P.CasualResting] = new([Whole(P.CasualResting), Whole(P.Dying)], AnimationEnd.Holds),
        [P.RainImpact] = new([Whole(P.RainImpact), Whole(P.Dying)], AnimationEnd.Holds),
        [P.CasualFalling] = new([Falling(0), Falling(1), Falling(2)], AnimationEnd.LoopsFrom(0)),
        [P.StartingFlame] = new([Whole(P.StartingFlame)], AnimationEnd.LoopsFrom(0)),
        [P.Delete] = new([Whole(P.Delete)], AnimationEnd.Holds),
        [P.DestroyableShot] = new([Whole(P.DestroyableShot)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PhantoonFlameSegment> StartingWith(PhantoonFlameSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<PhantoonFlameSegment> Whole(PhantoonFlameSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>Falling phase <paramref name="phase"/>'s two poses, played four times.</summary>
    private static AnimationPhase<PhantoonFlameSegment> Falling(int phase) => new(P.CasualFalling, phase * FallingPoseCount, FallingPoseCount, FallingRepeatCount);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue> Cue(PhantoonFlameCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>.Event(cue);
}
