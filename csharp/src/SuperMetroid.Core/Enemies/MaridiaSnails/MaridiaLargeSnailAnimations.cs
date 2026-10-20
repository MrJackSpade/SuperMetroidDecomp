using SuperMetroid.Core.Programs;
using K = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailCueKind;
using P = SuperMetroid.Core.Enemies.MaridiaSnails.MaridiaLargeSnailProgram;

namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations as typed programs (#641), in place of their bank-$A2 lists. Generated from the verified native lists: each
/// frame shows a <see cref="MaridiaLargeSnailFrame"/> pose and each callback becomes a <see cref="MaridiaLargeSnailCue"/> with its typed operands.
/// </summary>
internal static class MaridiaLargeSnailAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue> Programs { get; } = new(
        new Dictionary<MaridiaLargeSnailProgram, AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue>[]>
    {
        [P.FacingLeftIdle] =
        [
            Show(0, 1),
            Hold,
        ],
        [P.FacingLeftAttacking] =
        [
            Show(0, 16),
            Show(1, 16),
            Cue(new(K.PlaySplash)),
            Show(2, 16),
            Show(3, 2),
            Show(4, 3),
            Show(5, 4),
            Show(6, 2),
            Show(7, 3),
            Show(6, 1),
            Show(5, 3),
            Show(4, 2),
            Show(3, 1),
            Show(2, 18),
            Cue(new(K.FinishAttack)),
            Hold,
        ],
        [P.FacingLeftRollingForwards] =
        [
            Show(2, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(8, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(9, 7),
            Show(10, 7),
            Show(11, 7),
            Show(12, 7),
            Show(13, 7),
            Show(14, 7),
            GoTo(P.FacingLeftRollingForwards),
        ],
        [P.FacingLeftRollingBackwards] =
        [
            Show(14, 7),
            Show(13, 7),
            Show(12, 7),
            Show(11, 7),
            Show(10, 7),
            Show(9, 7),
            Show(8, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(2, 7),
            Cue(new(K.DisallowAttackRotation)),
            GoTo(P.FacingLeftRollingBackwards),
        ],
        [P.FacingRightIdle] =
        [
            Show(15, 1),
            Hold,
        ],
        [P.FacingRightAttacking] =
        [
            Show(15, 16),
            Show(16, 16),
            Show(17, 16),
            Cue(new(K.PlaySplash)),
            Show(18, 2),
            Show(19, 3),
            Show(20, 4),
            Show(21, 2),
            Show(22, 3),
            Show(21, 1),
            Show(20, 3),
            Show(19, 2),
            Show(18, 1),
            Show(17, 18),
            Cue(new(K.FinishAttack)),
            Hold,
        ],
        [P.FacingRightRollingForwards] =
        [
            Show(17, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(23, 7),
            Cue(new(K.DisallowAttackRotation)),
            Show(24, 7),
            Show(25, 7),
            Show(26, 7),
            Show(27, 7),
            Show(28, 7),
            Show(29, 7),
            GoTo(P.FacingRightRollingForwards),
        ],
        [P.FacingRightRollingBackwards] =
        [
            Show(29, 7),
            Show(28, 7),
            Show(27, 7),
            Show(26, 7),
            Show(25, 7),
            Show(24, 7),
            Show(23, 7),
            Cue(new(K.AllowAttackRotation)),
            Show(17, 7),
            Cue(new(K.DisallowAttackRotation)),
            GoTo(P.FacingRightRollingBackwards),
        ],
    });

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue> Show(int pose, int duration) => AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue> Cue(MaridiaLargeSnailCue cue) => AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue> GoTo(MaridiaLargeSnailProgram program) => AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue> Hold => AnimationProgramStep<MaridiaLargeSnailProgram, MaridiaLargeSnailFrame, MaridiaLargeSnailCue>.Hold;

}
