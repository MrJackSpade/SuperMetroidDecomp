using CK = SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileCommonCueKind;
using P = SuperMetroid.Core.Enemies.KiHunters.KiHunterAcidSpitProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The Ki-Hunter's acid-spit programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="KiHunterAcidSpitFrame"/> pose and each command becomes a <see cref="EnemyProjectileCommonCue"/> with its typed operands.
/// </summary>
internal static class KiHunterAcidSpitAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Programs { get; } = new(
        new Dictionary<KiHunterAcidSpitProgram, AnimationProgramStep<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue>[]>
    {
        // $86:CF34
        [P.Left] =
        [
            Show(0, 3),
            Show(1, 3),
            Show(2, 4),
            Show(3, 3),
            Show(4, 1),
            Cue(new(CK.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.KiHunterAcid_Left)),
            Show(5, 1),
            Show(6, 1),
            Hold,
        ],
        // $86:CF56
        [P.HitFloor] =
        [
            Cue(new(CK.ClearPreInstruction)),
            Show(7, 12),
            Show(8, 10),
            Show(9, 10),
            Show(10, 8),
            Show(11, 8),
            Cue(new(CK.Delete)),
            Hold,
        ],
        // $86:CF6E
        [P.Right] =
        [
            Show(12, 3),
            Show(13, 3),
            Show(14, 4),
            Show(15, 3),
            Show(16, 1),
            Cue(new(CK.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.KiHunterAcid_Right)),
            Show(17, 1),
            Show(18, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Show(int pose, int duration) => AnimationProgramStep<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Cue(EnemyProjectileCommonCue cue) => AnimationProgramStep<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue> Hold => AnimationProgramStep<KiHunterAcidSpitProgram, KiHunterAcidSpitFrame, EnemyProjectileCommonCue>.Hold;
}
