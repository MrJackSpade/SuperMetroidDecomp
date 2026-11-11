using SuperMetroid.Core.Programs;
using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterAcidSpitProgram;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter's acid-spit programs (#641). Each
/// frame shows a <see cref="KiHunterAcidSpitFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class KiHunterAcidSpitAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Segments { get; } = new(
        new Dictionary<KiHunterAcidSpitProgram, AnimationBeat<KiHunterAcidSpitFrame, EnemyProjectileCommonCue>[]>
    {
        [P.Left] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 4),
            Show(3, 3),
            Show(4, 1),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.KiHunterAcidLaunchLeft)),
            Show(5, 1),
            Show(6, 1),
        ],
        [P.HitFloor] =
        [
            Cue(new EnemyProjectileCommonCue.Command(CK.ClearFrameBehavior)),
            Show(7, 12),
            Show(8, 10),
            Show(9, 10),
            Show(10, 8),
            Show(11, 8),
            Cue(new EnemyProjectileCommonCue.Command(CK.Delete)),
        ],
        [P.Right] =
        [
            Show(12, 3),
            Show(13, 3),
            Show(14, 4),
            Show(15, 3),
            Show(16, 1),
            Cue(new EnemyProjectileCommonCue.SetFrameBehavior(FrameBehavior: EnemyProjectileFrameBehavior.KiHunterAcidLaunchRight)),
            Show(17, 1),
            Show(18, 1),
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

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationBeat<KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => new AnimationBeat<KiHunterAcidSpitFrame, EnemyProjectileCommonCue>.Frame(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationBeat<KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => new AnimationBeat<KiHunterAcidSpitFrame, EnemyProjectileCommonCue>.Event(cue);
}
