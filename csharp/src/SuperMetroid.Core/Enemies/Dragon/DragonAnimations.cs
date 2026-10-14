using K = SuperMetroid.Core.Enemies.Dragon.DragonCueKind;
using P = SuperMetroid.Core.Enemies.Dragon.DragonProgram;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Fx;

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
        // $A2:E59B
        [P.IdleFacingLeft] =
        [
            Show(0, 1),
            Hold,
        ],
        // $A2:E5A1
        [P.WingsFacingLeft] =
        [
            Show(4, 5),
            Show(5, 5),
            GoTo(P.WingsFacingLeft),
        ],
        // $A2:E5AD
        [P.IdleFacingRight] =
        [
            Show(6, 1),
            Hold,
        ],
        // $A2:E5B3
        [P.WingsFacingRight] =
        [
            Show(10, 5),
            Show(11, 5),
            GoTo(P.WingsFacingRight),
        ],
        // $A2:E5BF
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
        // $A2:E5D7
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

    /// <summary>The spritemaps <paramref name="programs"/> show, in authored order with repeats.</summary>
    internal static IEnumerable<ushort> Spritemaps(params DragonProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => DragonFrameDefinitions.Spritemap(step.Frame));

    /// <summary>Shows pose <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<DragonProgram, DragonFrame, DragonCue> Show(int pose, int duration) => AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>.Show(new(pose), duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<DragonProgram, DragonFrame, DragonCue> Cue(DragonCue cue) => AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<DragonProgram, DragonFrame, DragonCue> GoTo(DragonProgram program) => AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>.GoTo(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<DragonProgram, DragonFrame, DragonCue> Hold => AnimationProgramStep<DragonProgram, DragonFrame, DragonCue>.Hold;

}
