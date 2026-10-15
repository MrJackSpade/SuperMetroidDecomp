using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Magdollite.MagdolliteLavaCueKind;
using P = SuperMetroid.Core.Enemies.Magdollite.MagdolliteLavaProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// The Magdollite's thrown-lava programs (#641), in place of their bank-$86 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MagdolliteLavaFrame"/> pose and each command becomes a <see cref="MagdolliteLavaCue"/> with its typed operands.
/// </summary>
internal static class MagdolliteLavaAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue> Programs { get; } = new(
        new Dictionary<MagdolliteLavaProgram, AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue>[]>
    {
        // $86:84FC
        [P.SharedDelete] =
        [
            Cue(new(K.Delete)),
            Hold,
        ],
        // $86:DFD8
        [P.Left] =
        [
            Show(0, 1),
            Hold,
        ],
        // $86:DFDE
        [P.Right] =
        [
            Show(1, 1),
            Hold,
        ],
        // $86:DFE4
        [P.Shot] =
        [
            Cue(new(K.SpawnDrops)),
            GoTo(P.SharedDelete),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue> Show(int pose, int duration) => AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue> Cue(MagdolliteLavaCue cue) => AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue> GoTo(MagdolliteLavaProgram program) => AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue> Hold => AnimationProgramStep<MagdolliteLavaProgram, MagdolliteLavaFrame, MagdolliteLavaCue>.Hold;
}
