using K = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEffectCueKind;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEffectProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's onion-ring, bomb, rainbow-charge, drool, escape-door, purple-breath and subtitle programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MotherBrainEffectFrame"/> pose and each command becomes a <see cref="MotherBrainEffectCue"/> with its typed operands.
/// </summary>
internal static class MotherBrainEffectAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue> Programs { get; } = new(
        new Dictionary<MotherBrainEffectProgram, AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue>[]>
    {
        // $86:C432
        [P.OnionRing] =
        [
            Cue(new(K.SetRadius, Value: 257)),
            Show(0, 16),
            Cue(new(K.SetRadius, Value: 514)),
            Show(1, 10),
            Cue(new(K.SetRadius, Value: 771)),
            Show(2, 8),
            Cue(new(K.SetRadius, Value: 1028)),
            Show(3, 7),
            Cue(new(K.SetRadius, Value: 1285)),
            Show(4, 6),
            Cue(new(K.SetRadius, Value: 1542)),
            Show(5, 5),
            Hold,
        ],
        // $86:C464
        [P.OnionRingHit] =
        [
            Cue(new(K.UsePaletteZero)),
            Cue(new(K.ClearPreInstruction)),
            Show(6, 5),
            Show(7, 5),
            Show(8, 5),
            Show(9, 5),
            Show(10, 5),
            Show(11, 5),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:C76E
        [P.Bomb] =
        [
            Show(12, 6),
            Show(13, 5),
            Show(14, 4),
            Show(15, 3),
            Show(16, 2),
            Show(17, 2),
            Show(18, 3),
            Show(19, 4),
            Show(20, 5),
            GoTo(P.Bomb),
        ],
        // $86:C829
        [P.RainbowBeamCharging] =
        [
            Show(21, 5),
            Show(22, 5),
            Show(23, 5),
            Show(24, 5),
            Show(25, 5),
            Show(26, 5),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:C8B0
        [P.Drool] =
        [
            Show(27, 10),
            Show(28, 10),
            Show(29, 10),
            Show(30, 10),
            Show(31, 10),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.MotherBrainsDrool_Falling)),
            Cue(new(K.DropDrool)),
            Show(32, 10),
            Hold,
        ],
        // $86:C8E1
        [P.DroolHitFloor] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(33, 10),
            Show(34, 10),
            Show(35, 10),
            Show(36, 10),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:CA22
        [P.EscapeDoorParticle] =
        [
            Show(37, 1),
            Show(38, 1),
            Show(39, 1),
            Show(40, 1),
            Show(41, 3),
            Show(42, 3),
            Show(43, 4),
            Show(44, 4),
            GoTo(P.EscapeDoorParticle),
        ],
        // $86:CAA4
        [P.PurpleBreathBig] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(45, 8),
            Show(46, 8),
            Show(47, 9),
            Show(48, 9),
            Show(49, 10),
            Show(50, 10),
            Show(51, 11),
            Show(52, 11),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:CAC8
        [P.PurpleBreathSmall] =
        [
            Cue(new(K.ClearPreInstruction)),
            Show(53, 8),
            Show(54, 8),
            Show(55, 10),
            Show(56, 10),
            Show(57, 11),
            Show(58, 11),
            Show(59, 12),
            Show(60, 12),
            Cue(new(K.EndSmallPurpleBreath)),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:CB0D
        [P.EscapeSubtitle] =
        [
            Show(61, 1),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue> Show(int pose, int duration) => AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue> Cue(MotherBrainEffectCue cue) => AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue> GoTo(MotherBrainEffectProgram program) => AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue> Hold => AnimationProgramStep<MotherBrainEffectProgram, MotherBrainEffectFrame, MotherBrainEffectCue>.Hold;
}
