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
    /// Every flame segment's steps (#1788): a flame pose lasts its beat, a landing casual flame shows one pose for one
    /// update and a rain flame's impact one for a longer beat; a falling flame flickers between each of its three poses and nothing,
    /// one update each, in three runs (#2250).
    /// </summary>
    internal static AnimationSegments<PhantoonFlameSegment, EnemyProjectileFrameId, PhantoonFlameCue> Segments { get; } = new(
        new Dictionary<PhantoonFlameSegment, AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>[]>
    {
        [P.Burning] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameRingedEye, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameBrightEye, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameFlickering, 5),
        ],
        [P.CasualHitGround] = [Show(EnemyProjectileFrameId.PhantoonFlameSplashed, 1)],
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
        ],
        [P.RainImpact] = [Show(EnemyProjectileFrameId.PhantoonFlameSplashed, 8)],
        [P.CasualFallingKindling] = Flicker(EnemyProjectileFrameId.PhantoonFlameKindling),
        [P.CasualFallingTeardrop] = Flicker(EnemyProjectileFrameId.PhantoonFlameTeardrop),
        [P.CasualFallingSputtering] = Flicker(EnemyProjectileFrameId.PhantoonFlameSputtering),
        [P.StartingFlame] =
        [
            Show(EnemyProjectileFrameId.PhantoonFlameKindling, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameTeardrop, 5),
            Show(EnemyProjectileFrameId.PhantoonFlameSputtering, 5),
        ],
        [P.Delete] = [Cue(PhantoonFlameCue.Delete)],
        [P.Drop] = [Cue(PhantoonFlameCue.SpawnDrop)],
    });

    /// <summary>
    /// How each flame animation plays its steps: the loops cycle for good, a resting or impacting flame runs on into dying
    /// and its deletion, a shot flame dies, leaves its drop and is deleted, and a falling flame plays each of its
    /// flickers over again (<see cref="FallingRepeatCount"/>), in turn for good.
    /// </summary>
    internal static IReadOnlyDictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>> StartingAt { get; } = new Dictionary<PhantoonFlameSegment, Animation<PhantoonFlameSegment>>
    {
        [P.Burning] = Animation<PhantoonFlameSegment>.Looping([Segments.Whole(P.Burning)]),
        [P.CasualHitGround] = new([Segments.Whole(P.CasualHitGround)], AnimationEnd.Holds),
        [P.CasualResting] = new([Segments.Whole(P.CasualResting), Segments.Whole(P.Dying), Segments.Whole(P.Delete)], AnimationEnd.Holds),
        [P.Dying] = new([Segments.Whole(P.Dying), Segments.Whole(P.Drop), Segments.Whole(P.Delete)], AnimationEnd.Holds),
        [P.RainImpact] = new([Segments.Whole(P.RainImpact), Segments.Whole(P.Dying), Segments.Whole(P.Delete)], AnimationEnd.Holds),
        [P.CasualFallingKindling] = Animation<PhantoonFlameSegment>.Looping(
            [
                Segments.Repeated(P.CasualFallingKindling, FallingRepeatCount),
                Segments.Repeated(P.CasualFallingTeardrop, FallingRepeatCount),
                Segments.Repeated(P.CasualFallingSputtering, FallingRepeatCount),
            ]),
        [P.StartingFlame] = Animation<PhantoonFlameSegment>.Looping([Segments.Whole(P.StartingFlame)]),
        [P.Delete] = new([Segments.Whole(P.Delete)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<PhantoonFlameSegment> StartingWith(PhantoonFlameSegment segment) => StartingAt[segment];

    /// <summary>A falling flame's flicker: <paramref name="pose"/> for one update, then nothing for one.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>[] Flicker(EnemyProjectileFrameId pose) =>
        [Show(pose, 1), Show(EnemyProjectileFrameId.Blank, 1)];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> passes.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue> Show(EnemyProjectileFrameId frame, int duration) =>
        new AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>.Frame(frame, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue> Cue(PhantoonFlameCue cue) =>
        new AnimationBeat<EnemyProjectileFrameId, PhantoonFlameCue>.Event(cue);
}
