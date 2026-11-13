using SuperMetroid.Core.Animation;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterAcidSpitSegment;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter's acid-spit animations (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each cue is a <see cref="EnemyProjectileCommonCue"/> with the values it acts on.
/// </summary>
internal static class KiHunterAcidSpitAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KiHunterAcidSpitSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<KiHunterAcidSpitSegment, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Left] =
        [
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftFirst, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftSecond, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftThird, 4),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftFourth, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftFifth, 1),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.KiHunterAcidLaunchLeft)),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftSixth, 1),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitLeftSeventh, 1),
        ],
        [P.HitFloor] =
        [
            Cue(new EnemyProjectileCommonCue.Signal(CK.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitHitFloorFirst, 12),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitHitFloorSecond, 10),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitHitFloorThird, 10),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitHitFloorFourth, 8),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitHitFloorFifth, 8),
            Cue(new EnemyProjectileCommonCue.Signal(CK.Delete)),
        ],
        [P.Right] =
        [
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightFirst, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightSecond, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightThird, 4),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightFourth, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightFifth, 1),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.KiHunterAcidLaunchRight)),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightSixth, 1),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitRightSeventh, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>> StartingAt { get; } = new Dictionary<KiHunterAcidSpitSegment, Animation<KiHunterAcidSpitSegment>>
    {
        [P.Left] = new([new(P.Left, 0, 8, 1)], AnimationEnd.Holds),
        [P.HitFloor] = new([new(P.HitFloor, 0, 7, 1)], AnimationEnd.Holds),
        [P.Right] = new([new(P.Right, 0, 8, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterAcidSpitSegment> StartingWith(KiHunterAcidSpitSegment segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
