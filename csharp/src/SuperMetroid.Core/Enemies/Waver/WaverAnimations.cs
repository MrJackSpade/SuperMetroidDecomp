using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Waver.WaverCueKind;
using P = SuperMetroid.Core.Enemies.Waver.WaverProgram;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Waver's steady and spinning animations as typed programs (#641), in place of their bank-$A3 lists. Generated from the verified native lists: each
/// frame shows a <see cref="WaverFrame"/> pose and each callback becomes a <see cref="WaverCue"/> with its typed operands.
/// </summary>
internal static class WaverAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<WaverProgram, WaverFrame, WaverCue> Programs { get; } = new(
        new Dictionary<WaverProgram, AnimationProgramStep<WaverProgram, WaverFrame, WaverCue>[]>
    {
        [P.SteadyFacingLeft] =
        [
            Show(2, 1),
            Hold,
        ],
        [P.SteadyFacingRight] =
        [
            Show(7, 1),
            Hold,
        ],
        [P.SpinningFacingLeft] =
        [
            Show(3, 8),
            Show(4, 8),
            Show(0, 8),
            Show(1, 8),
            Cue(new(K.FinishSpin)),
            Hold,
        ],
        [P.SpinningFacingRight] =
        [
            Show(8, 8),
            Show(9, 8),
            Show(5, 8),
            Show(6, 8),
            Cue(new(K.FinishSpin)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<WaverProgram, WaverFrame, WaverCue> Show(int pose, int duration) => AnimationProgramStep<WaverProgram, WaverFrame, WaverCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<WaverProgram, WaverFrame, WaverCue> Cue(WaverCue cue) => AnimationProgramStep<WaverProgram, WaverFrame, WaverCue>.Publish(cue);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<WaverProgram, WaverFrame, WaverCue> Hold => AnimationProgramStep<WaverProgram, WaverFrame, WaverCue>.Hold;

}
