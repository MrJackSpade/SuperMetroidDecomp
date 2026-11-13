using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Areas.Ceres.CeresBabyProgram;
using F = SuperMetroid.Core.Areas.Ceres.CeresBabyPose;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres Baby Metroid's typed cutscene animations (#641, #1600):
/// its resting and expressive cutscene loops. The baby squishes through horizontal, round and vertical poses;
/// while it moves, the expressive loop also pulses its palette and cries.
/// </summary>
internal static class CeresBabyAnimations
{
    /// <summary>Every segment's beats.</summary>
    internal static AnimationSegments<P, F, CeresBabyCue> Segments { get; } = new(
        new Dictionary<P, AnimationBeat<F, CeresBabyCue>[]>
    {
        [P.Initial] =
        [
            Cue(new CeresBabyCue.GoToIfMoving(Target: P.ExpressiveLoop)),
            .. SquishCycle,
            Cue(new CeresBabyCue.GoToIfMoving(Target: P.ExpressiveLoop)),
            .. SquishCycle,
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
        ],
    });

    /// <summary>
    /// The animation that starts at each segment: the resting loop runs on into the expressive loop, which goes back to the
    /// resting loop.
    /// </summary>
    internal static IReadOnlyDictionary<P, Animation<P>> StartingAt { get; } = new Dictionary<P, Animation<P>>
    {
        [P.Initial] = new([Whole(P.Initial), Whole(P.ExpressiveLoop)], AnimationEnd.LoopsFrom(0)),
        [P.ExpressiveLoop] = new([Whole(P.ExpressiveLoop), Whole(P.Initial)], AnimationEnd.LoopsFrom(0)),
    };

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal static Animation<P> StartingWith(P segment) => StartingAt[segment];

    /// <summary>A phase playing every beat of <paramref name="segment"/> once.</summary>
    private static AnimationPhase<P> Whole(P segment) => new(segment, 0, Segments.Beats(segment).Count, 1);

    /// <summary>The ten-update horizontal, round, vertical, round squish.</summary>
    private static AnimationBeat<F, CeresBabyCue>[] SquishCycle =>
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
    private static AnimationBeat<F, CeresBabyCue> Show(F pose, int duration) =>
        new AnimationBeat<F, CeresBabyCue>.Frame(pose, duration);

    /// <summary>Raises <paramref name="cue"/>.</summary>
    private static AnimationBeat<F, CeresBabyCue> Cue(CeresBabyCue cue) =>
        new AnimationBeat<F, CeresBabyCue>.Event(cue);
}
