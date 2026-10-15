using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Rooms.Plms.NoobTubeProjectileCueKind;
using P = SuperMetroid.Core.Rooms.Plms.NoobTubeProjectileProgram;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The n00b tube's crack, glass shard and released-air bubble programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="NoobTubeProjectileFrame"/> pose and each command becomes a <see cref="NoobTubeProjectileCue"/> with its typed operands.
/// </summary>
internal static class NoobTubeProjectileAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> Programs { get; } = new(
        new Dictionary<NoobTubeProjectileProgram, AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>[]>
    {
        // $86:D3D7
        [P.Crack] =
        [
            Show(0, 12),
            Show(1, 10),
            Show(2, 8),
            Show(3, 6),
            Show(4, 6),
            Show(5, 6),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeCrackFlickering)),
            Show(6, 1),
            Show(7, 1),
            Show(8, 1),
            Show(9, 1),
            Show(10, 1),
            Show(11, 2),
            Show(12, 3),
            Show(13, 6),
            Show(14, 9),
            Show(15, 8),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeCrackFalling)),
            Show(16, 7),
            Show(17, 7),
            Show(18, 7),
            Show(19, 7),
            Show(20, 7),
            Show(21, 7),
            Show(22, 7),
            Show(23, 7),
            Show(24, 7),
            Show(25, 7),
            Show(26, 7),
            Show(27, 7),
            Show(28, 7),
            Show(29, 7),
            Show(30, 7),
            Show(31, 7),
            Show(32, 7),
            Show(33, 7),
            Show(34, 16),
            SetCounter(6),
            FallInto(P.CrackLoop),
        ],
        // $86:D46F
        [P.CrackLoop] =
        [
            Show(35, 16),
            Show(36, 16),
            RepeatWhileCounting(P.CrackLoop),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D47D
        [P.Shard0] =
        [
            SetCounter(32),
            FallInto(P.Shard0Loop),
        ],
        // $86:D481
        [P.Shard0Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(37, 38),
            RepeatWhileCounting(P.Shard0Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard0Loop2),
        ],
        // $86:D495
        [P.Shard0Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(39, 40),
            RepeatWhileCounting(P.Shard0Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D4A1
        [P.Shard1] =
        [
            SetCounter(32),
            FallInto(P.Shard1Loop),
        ],
        // $86:D4A5
        [P.Shard1Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(41, 42),
            RepeatWhileCounting(P.Shard1Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard1Loop2),
        ],
        // $86:D4B9
        [P.Shard1Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(43, 44),
            RepeatWhileCounting(P.Shard1Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D4C5
        [P.Shard2] =
        [
            SetCounter(32),
            FallInto(P.Shard2Loop),
        ],
        // $86:D4C9
        [P.Shard2Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(45, 46),
            RepeatWhileCounting(P.Shard2Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard2Loop2),
        ],
        // $86:D4DD
        [P.Shard2Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(47, 48),
            RepeatWhileCounting(P.Shard2Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D4E9
        [P.Shard3] =
        [
            SetCounter(32),
            FallInto(P.Shard3Loop),
        ],
        // $86:D4ED
        [P.Shard3Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(49, 50),
            RepeatWhileCounting(P.Shard3Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard3Loop2),
        ],
        // $86:D501
        [P.Shard3Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(51, 52),
            RepeatWhileCounting(P.Shard3Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D50D
        [P.Shard4] =
        [
            SetCounter(32),
            FallInto(P.Shard4Loop),
        ],
        // $86:D511
        [P.Shard4Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(53, 54),
            RepeatWhileCounting(P.Shard4Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard4Loop2),
        ],
        // $86:D525
        [P.Shard4Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(55, 56),
            RepeatWhileCounting(P.Shard4Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D531
        [P.Shard5] =
        [
            SetCounter(32),
            FallInto(P.Shard5Loop),
        ],
        // $86:D535
        [P.Shard5Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(57, 58),
            RepeatWhileCounting(P.Shard5Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard5Loop2),
        ],
        // $86:D549
        [P.Shard5Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(59, 60),
            RepeatWhileCounting(P.Shard5Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D555
        [P.Shard6] =
        [
            SetCounter(32),
            FallInto(P.Shard6Loop),
        ],
        // $86:D559
        [P.Shard6Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(61, 62),
            RepeatWhileCounting(P.Shard6Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard6Loop2),
        ],
        // $86:D56D
        [P.Shard6Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(63, 64),
            RepeatWhileCounting(P.Shard6Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D579
        [P.Shard7] =
        [
            SetCounter(32),
            FallInto(P.Shard7Loop),
        ],
        // $86:D57D
        [P.Shard7Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(65, 66),
            RepeatWhileCounting(P.Shard7Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard7Loop2),
        ],
        // $86:D591
        [P.Shard7Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(67, 68),
            RepeatWhileCounting(P.Shard7Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D59D
        [P.Shard8] =
        [
            SetCounter(32),
            FallInto(P.Shard8Loop),
        ],
        // $86:D5A1
        [P.Shard8Loop] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(69),
            RepeatWhileCounting(P.Shard8Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard8Loop2),
        ],
        // $86:D5B3
        [P.Shard8Loop2] =
        [
            Cue(new(K.FlickerHidden)),
            Flicker(70),
            RepeatWhileCounting(P.Shard8Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D5BD
        [P.Shard9] =
        [
            SetCounter(32),
            FallInto(P.Shard9Loop),
        ],
        // $86:D5C1
        [P.Shard9Loop] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(71, 72),
            RepeatWhileCounting(P.Shard9Loop),
            Cue(new(K.AssignShardFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeShardFalling)),
            SetCounter(272),
            FallInto(P.Shard9Loop2),
        ],
        // $86:D5D5
        [P.Shard9Loop2] =
        [
            Cue(new(K.FlickerReflected)),
            Flicker(73, 74),
            RepeatWhileCounting(P.Shard9Loop2),
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:D652
        [P.ReleasedAirBubble] =
        [
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeBubbleFlying)),
            Show(75, 2),
            Show(76, 2),
            Show(77, 2),
            Show(78, 2),
            Cue(new(K.AssignBubbleFallingAngle)),
            Cue(new(K.SetPreInstruction, PreInstruction: EnemyProjectilePreInstruction.NoobTubeBubbleFalling)),
            Show(79, 2),
            Show(80, 2),
            Show(81, 2),
            Show(82, 2),
            Show(83, 2),
            Show(84, 2),
            Show(85, 4),
            Show(86, 4),
            Show(87, 4),
            Show(88, 4),
            Show(89, 4),
            Cue(new(K.Delete)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> Show(int pose, int duration) => AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> Cue(NoobTubeProjectileCue cue) => AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>.Publish(cue);

    /// <summary>Loads the loop counter with <paramref name="count"/>.</summary>
    private static AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> SetCounter(int count) => AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>.SetCounter(count);

    /// <summary>Repeats from <paramref name="program"/> while the loop counter counts down.</summary>
    private static AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> RepeatWhileCounting(NoobTubeProjectileProgram program) => AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>.RepeatWhileCounting(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> FallInto(NoobTubeProjectileProgram program) => AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> Hold => AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>.Hold;

    /// <summary>Shows <paramref name="pose"/>, or on even frames <paramref name="reflectedPose"/>, for the one update a native flicker lasts.</summary>
    private static AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> Flicker(int pose, int reflectedPose) => AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>.Show(new(pose, reflectedPose), 1);

    /// <summary>Shows <paramref name="pose"/> for the one update a native flicker lasts.</summary>
    private static AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue> Flicker(int pose) => AnimationProgramStep<NoobTubeProjectileProgram, NoobTubeProjectileFrame, NoobTubeProjectileCue>.Show(new(pose), 1);
}
