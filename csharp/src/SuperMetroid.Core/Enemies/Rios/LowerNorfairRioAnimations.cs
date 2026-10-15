using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Rios.LowerNorfairRioCueKind;
using P = SuperMetroid.Core.Enemies.Rios.LowerNorfairRioProgram;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The Lower Norfair Rio's (Holtz's) idle, swoop and cooldown animations and its flames' animation as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="LowerNorfairRioFrame"/> pose and each callback becomes a <see cref="LowerNorfairRioCue"/> with its typed operands.
/// </summary>
internal static class LowerNorfairRioAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue> Programs { get; } = new(
        new Dictionary<LowerNorfairRioProgram, AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue>[]>
    {
        // $A2:C61A
        [P.Idle] =
        [
            Cue(new(K.HideFlames)),
            FallInto(P.IdleLoop),
        ],
        // $A2:C61C
        [P.IdleLoop] =
        [
            Show(0, 11),
            Show(1, 11),
            Show(2, 11),
            Show(1, 11),
            GoTo(P.IdleLoop),
        ],
        // $A2:C630
        [P.PrepareToSwoop] =
        [
            Cue(new(K.HideFlames)),
            Show(2, 3),
            Show(3, 3),
            Show(4, 3),
            Show(5, 3),
            Show(6, 2),
            Show(7, 1),
            Show(8, 2),
            Show(9, 3),
            Show(10, 3),
            Cue(new(K.AnimationFinished)),
            Hold,
        ],
        // $A2:C65A
        [P.Descending] =
        [
            Cue(new(K.HideFlames)),
            Show(10, 1),
            Hold,
        ],
        // $A2:C662
        [P.AscendingPart1] =
        [
            Cue(new(K.HideFlames)),
            Show(10, 3),
            Show(9, 3),
            Show(11, 3),
            Cue(new(K.AnimationFinished)),
            Hold,
        ],
        // $A2:C674
        [P.AscendingPart2] =
        [
            Cue(new(K.ShowFlames)),
            FallInto(P.AscendingPart2Loop),
        ],
        // $A2:C676
        [P.AscendingPart2Loop] =
        [
            Show(14, 2),
            Show(12, 2),
            Show(13, 2),
            GoTo(P.AscendingPart2Loop),
        ],
        // $A2:C686
        [P.Cooldown] =
        [
            Cue(new(K.ShowFlames)),
            Show(11, 3),
            Show(9, 3),
            Show(8, 2),
            Show(7, 1),
            Show(6, 2),
            Show(5, 3),
            Show(4, 1),
            Show(3, 1),
            Show(2, 1),
            Cue(new(K.AnimationFinished)),
            Hold,
        ],
        // $A2:C6B0
        [P.Flames] =
        [
            Show(15, 6),
            Show(16, 4),
            Show(17, 3),
            GoTo(P.Flames),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue> Show(int pose, int duration) => AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue> Cue(LowerNorfairRioCue cue) => AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue> GoTo(LowerNorfairRioProgram program) => AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue> FallInto(LowerNorfairRioProgram program) => AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue> Hold => AnimationProgramStep<LowerNorfairRioProgram, LowerNorfairRioFrame, LowerNorfairRioCue>.Hold;

}
