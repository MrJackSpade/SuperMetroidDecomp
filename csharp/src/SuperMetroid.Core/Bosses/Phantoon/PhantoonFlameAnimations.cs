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
    /// <summary>How many times each of a falling flame's three runs plays its flicker.</summary>
    private const int FallingRepeatCount = 4;

    /// <summary>
    /// Every flame animation's steps (#1788): a flame pose lasts five updates, a landing casual flame shows one pose for one
    /// update and a rain flame's impact one for eight; a falling flame flickers between each of its three poses and nothing,
    /// one update each, in three runs (#2250).
    /// </summary>
    internal static AnimationSegments<PhantoonFlameSegment, EnemyProjectileFrameId, PhantoonFlameCue> Segments { get; } = new(
        new Dictionary<PhantoonFlameSegment, AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>[]>
    {
        [P.DestroyableIdle] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameRingedEye, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameBrightEye, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameFlickering, 5),
        ],
        [P.CasualHitGround] = [Show(EnemyProjectileFrameId.PhantoonFlameSplashed, 1)],
        [P.CasualBouncing] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameRingedEye, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameBrightEye, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameFlickering, 5),
        ],
        [P.CasualResting] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameSwelling, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameLeaning, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameResting, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameRestingSwaying, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameRestingLow, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameResting, 5),
        ],
        [P.Dying] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameDying, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameGuttering, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameWisps, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameLastWisp, 5),
            Cue(PhantoonFlameCue.Delete),
        ],
        [P.RainImpact] = [Show(EnemyProjectileFrameId.PhantoonFlameSplashed, 8)],
        [P.CasualFallingKindling] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameKindling, 1),
            Show(EnemyProjectileFrameId.PhantoonFlameBlank, 1),
        ],
        [P.CasualFallingTeardrop] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameTeardrop, 1),
            Show(EnemyProjectileFrameId.PhantoonFlameBlank, 1),
        ],
        [P.CasualFallingSputtering] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameSputtering, 1),
            Show(EnemyProjectileFrameId.PhantoonFlameBlank, 1),
        ],
        [P.StartingFlame] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameKindling, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameTeardrop, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameSputtering, 5),
        ],
        [P.Delete] = [Cue(PhantoonFlameCue.Delete)],
        [P.DestroyableShot] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameDying, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameGuttering, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameWisps, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameLastWisp, 5),
            Cue(PhantoonFlameCue.SpawnDrop),
            Cue(PhantoonFlameCue.Delete),
        ],
    });

    /// <summary>
    /// How each flame animation plays its steps: the loops cycle for good, a resting or impacting flame runs on into dying,
    /// which no owner starts on its own (#2166), and a falling flame plays each of its three flickers four times, in turn
    /// for good.
    /// </summary>
    internal static IReadOnlyDictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>> StartingAt { get; } = new Dictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>>
    {
        [P.DestroyableIdle] = new([Whole(P.DestroyableIdle)], AnimationEnd.LoopsFrom(0)),
        [P.CasualHitGround] = new([Whole(P.CasualHitGround)], AnimationEnd.Holds),
        [P.CasualBouncing] = new([Whole(P.CasualBouncing)], AnimationEnd.LoopsFrom(0)),
        [P.CasualResting] = new([Whole(P.CasualResting), Whole(P.Dying)], AnimationEnd.Holds),
        [P.RainImpact] = new([Whole(P.RainImpact), Whole(P.Dying)], AnimationEnd.Holds),
        [P.CasualFallingKindling] = new(
            [Repeated(P.CasualFallingKindling), Repeated(P.CasualFallingTeardrop), Repeated(P.CasualFallingSputtering)],
            AnimationEnd.LoopsFrom(0)),
        [P.StartingFlame] = new([Whole(P.StartingFlame)], AnimationEnd.LoopsFrom(0)),
        [P.Delete] = new([Whole(P.Delete)], AnimationEnd.Holds),
        [P.DestroyableShot] = new([Whole(P.DestroyableShot)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PhantoonFlameSegment> StartingWith(PhantoonFlameSegment segment) => StartingAt[segment];

    /// <summary>One play of every beat of <paramref name="segment"/>.</summary>
    private static AnimationPhase<PhantoonFlameSegment> Whole(PhantoonFlameSegment segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>A falling flicker, <paramref name="segment"/>, played four times.</summary>
    private static AnimationPhase<PhantoonFlameSegment> Repeated(PhantoonFlameSegment segment) =>
        new(segment, 0, Segments.Beats(segment).Count, FallingRepeatCount);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue> Cue(PhantoonFlameCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>.Event(cue);
}
