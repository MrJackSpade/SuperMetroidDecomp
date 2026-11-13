using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterAcidSpitProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter's acid-spit programs (#641). Each
/// frame shows a <see cref="EnemyProjectileFrameId"/> and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class KiHunterAcidSpitAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KiHunterAcidSpitProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<KiHunterAcidSpitProgram, AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>[]>
    {
        [P.Left] =
        [
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame00, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame01, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame02, 4),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame03, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame04, 1),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.KiHunterAcidLaunchLeft)),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame05, 1),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame06, 1),
        ],
        [P.HitFloor] =
        [
            Cue(new EnemyProjectileCommonCue.Command(CK.ClearFrameBehavior)),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame07, 12),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame08, 10),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame09, 10),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame10, 8),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame11, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.Right] =
        [
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame12, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame13, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame14, 4),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame15, 3),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame16, 1),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.KiHunterAcidLaunchRight)),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame17, 1),
            Show(EnemyProjectileFrameId.KiHunterAcidSpitFrame18, 1),
        ],
    });

    /// <summary>The animation that starts at each segment an owner can start or restart at.</summary>
    internal static IReadOnlyDictionary<KiHunterAcidSpitProgram, Animation<KiHunterAcidSpitProgram>> StartingAt { get; } = new Dictionary<KiHunterAcidSpitProgram, Animation<KiHunterAcidSpitProgram>>
    {
        [P.Left] = new([new(P.Left, 0, 8, 1)], AnimationEnd.Holds),
        [P.HitFloor] = new([new(P.HitFloor, 0, 7, 1)], AnimationEnd.Holds),
        [P.Right] = new([new(P.Right, 0, 8, 1)], AnimationEnd.Holds),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<KiHunterAcidSpitProgram> StartingWith(KiHunterAcidSpitProgram segment) => StartingAt[segment];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Show(EnemyProjectileFrameId frame, int duration) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Frame(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<EnemyProjectileFrameId, EnemyProjectileCommonCue>.Event(cue);
}
