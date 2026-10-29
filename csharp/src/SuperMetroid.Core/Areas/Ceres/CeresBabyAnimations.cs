using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Areas.Ceres.CeresBabyProgram;
using F = SuperMetroid.Core.Areas.Ceres.CeresBabyPose;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres Baby Metroid's typed cutscene programs (#641), in place of its private bank-$A6 draw lists
/// (<c>InstList_BabyMetroidCutscene_0</c> and <c>_1</c>). The baby squishes through horizontal, round and vertical poses;
/// while it moves, the expressive loop also pulses its palette and cries.
/// </summary>
internal static class CeresBabyAnimations
{
    /// <summary>Every program.</summary>
    internal static AnimationProgramSet<P, F, CeresBabyCue> Programs { get; } = new(
        new Dictionary<P, AnimationProgramStep<P, F, CeresBabyCue>[]>
    {
        [P.Initial] =
        [
            Cue(new CeresBabyCue.GoToIfMoving(Target: P.ExpressiveLoop)),
            .. SquishCycle,
            Cue(new CeresBabyCue.GoToIfMoving(Target: P.ExpressiveLoop)),
            .. SquishCycle,
            FallInto(P.ExpressiveLoop),
        ],
        [P.ExpressiveLoop] =
        [
            Cue(new CeresBabyCue.CryOrGoTo(Target: P.Initial)),
            .. ExpressivePoses.SelectMany(pose => new[]
            {
                Cue(new CeresBabyCue.UpdateColors(Shade: Shade(pose.Pose))),
                Show(pose.Pose, pose.Duration),
            }),
            Cue(new CeresBabyCue.UpdateColors(Shade: CeresBabyShade.Resting)),
            Cue(new CeresBabyCue.GoToIfMoving(Target: P.ExpressiveLoop)),
            GoTo(P.Initial),
        ],
    });

    /// <summary>The ten-update horizontal, round, vertical, round squish.</summary>
    private static AnimationProgramStep<P, F, CeresBabyCue>[] SquishCycle =>
        [Show(F.Horizontal, 10), Show(F.Round, 10), Show(F.Vertical, 10), Show(F.Round, 10)];

    /// <summary>The expressive loop's twelve poses: the squish cycle three times, held two updates plus the distance from the fifth.</summary>
    private static IEnumerable<CeresBabyPoseHold> ExpressivePoses =>
        Enumerable.Range(0, 12).Select(index => new CeresBabyPoseHold(SquishPose(index), 2 + Math.Abs(index - 4)));

    /// <summary>The squish cycle's pose at <paramref name="index"/>.</summary>
    private static F SquishPose(int index) => (index % 4) switch
    {
        0 => F.Horizontal,
        2 => F.Vertical,
        _ => F.Round,
    };

    /// <summary>The shade that accompanies <paramref name="pose"/> in the expressive loop.</summary>
    private static CeresBabyShade Shade(F pose) => pose switch
    {
        F.Horizontal => CeresBabyShade.HorizontalSquish,
        F.Round => CeresBabyShade.Round,
        F.Vertical => CeresBabyShade.VerticalSquish,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, "Undefined Ceres Baby pose."),
    };

    /// <summary>Shows <paramref name="pose"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<P, F, CeresBabyCue> Show(F pose, int duration) =>
        AnimationProgramStep<P, F, CeresBabyCue>.Show(pose, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<P, F, CeresBabyCue> Cue(CeresBabyCue cue) =>
        AnimationProgramStep<P, F, CeresBabyCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<P, F, CeresBabyCue> GoTo(P program) =>
        AnimationProgramStep<P, F, CeresBabyCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<P, F, CeresBabyCue> FallInto(P program) =>
        AnimationProgramStep<P, F, CeresBabyCue>.FallInto(program);
}
