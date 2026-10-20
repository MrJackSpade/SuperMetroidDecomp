using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.Dragon.DragonCueKind;
using P = SuperMetroid.Core.Enemies.Dragon.DragonProgram;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's idle, wing and attack animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="DragonFrame"/> pose and each callback becomes a <see cref="DragonCue"/> with its typed operands.
/// </summary>
internal static class DragonAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<DragonProgram, DragonFrame, DragonCue> Programs { get; } = new(
        new Dictionary<DragonProgram, AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>[]>
    {
        [P.IdleFacingLeft] =
        [
            Show(0, 1),
            Hold,
        ],
        [P.WingsFacingLeft] =
        [
            Show(4, 5),
            Show(5, 5),
            GoTo(P.WingsFacingLeft),
        ],
        [P.IdleFacingRight] =
        [
            Show(6, 1),
            Hold,
        ],
        [P.WingsFacingRight] =
        [
            Show(10, 5),
            Show(11, 5),
            GoTo(P.WingsFacingRight),
        ],
        [P.AttackingFacingLeft] =
        [
            Show(1, 32),
            Show(2, 3),
            Show(3, 7),
            Show(2, 3),
            Show(1, 1),
            Cue(new(K.FinishAttack)),
            Hold,
        ],
        [P.AttackingFacingRight] =
        [
            Show(7, 32),
            Show(8, 3),
            Show(9, 7),
            Show(8, 3),
            Show(7, 1),
            Cue(new(K.FinishAttack)),
            Hold,
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DragonProgram, DragonFrame, DragonCue> Show(int pose, int duration) => AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<DragonProgram, DragonFrame, DragonCue> Cue(DragonCue cue) => AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DragonProgram, DragonFrame, DragonCue> GoTo(DragonProgram program) => AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<DragonProgram, DragonFrame, DragonCue> Hold => AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>.Hold;

}
