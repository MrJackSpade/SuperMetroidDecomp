using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The powered Work Robots' glow (#2380): its steps, each showing a place of the glow for its hold, which the room loops
/// while the glow runs. The way back passes the places between the ends again (#2393). A powered robot's setup starts the
/// glow on its last step for one update, so its first step shows on the next.
/// </summary>
internal static partial class WorkRobotGlowAnimations
{
    /// <summary>Every run's beats.</summary>
    internal static AnimationSegments<WorkRobotGlowSegment, WorkRobotGlowPlace, NoAnimationCue> Segments { get; } = new(
        new Dictionary<WorkRobotGlowSegment, AnimationBeat<WorkRobotGlowPlace, NoAnimationCue>[]>
    {
        [WorkRobotGlowSegment.Glowing] =
        [
            Show(WorkRobotGlowPlace.AtFirstEnd, WorkRobotGlowDefinitions.EndHold),
            Show(WorkRobotGlowPlace.NearFirstEnd, WorkRobotGlowDefinitions.PassingHold),
            Show(WorkRobotGlowPlace.NearFarEnd, WorkRobotGlowDefinitions.PassingHold),
            Show(WorkRobotGlowPlace.AtFarEnd, WorkRobotGlowDefinitions.EndHold),
            Show(WorkRobotGlowPlace.NearFarEnd, WorkRobotGlowDefinitions.PassingHold),
            Show(WorkRobotGlowPlace.NearFirstEnd, WorkRobotGlowDefinitions.PassingHold),
        ],
        [WorkRobotGlowSegment.Restarting] =
        [
            Show(WorkRobotGlowPlace.NearFirstEnd, WorkRobotGlowDefinitions.RestartHold),
        ],
    });

    /// <summary>The places the glow's steps show, in the order it shows them, from its first step.</summary>
    internal static IReadOnlyList<WorkRobotGlowPlace> Cycle { get; } =
        [.. Segments.Beats(WorkRobotGlowSegment.Glowing).Select(beat => beat.ShownFrame ?? throw new InvalidOperationException("Every beat of the glow shows a place."))];

    /// <summary>
    /// Each place the glow shows, once, in the order its cycle first shows them (#2393): the import installs one shade for
    /// each, named by the place (<see cref="Artwork.WorkRobotShades.Glow"/>).
    /// </summary>
    internal static IReadOnlyList<WorkRobotGlowPlace> Places { get; } = [.. Cycle.Distinct()];

    /// <summary>
    /// The glow as a powered robot's setup starts it (#2451): resting on its last step's place for the update the setup gives
    /// it, then its steps from the first, over and over.
    /// </summary>
    internal static Animation<WorkRobotGlowSegment> Restarted { get; } = Animation<WorkRobotGlowSegment>.LeadingInto(
        [Segments.Whole(WorkRobotGlowSegment.Restarting)],
        [Segments.Whole(WorkRobotGlowSegment.Glowing)]);

    /// <summary>Shows <paramref name="place"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<WorkRobotGlowPlace, NoAnimationCue> Show(WorkRobotGlowPlace place, int updates) =>
        new AnimationBeat<WorkRobotGlowPlace, NoAnimationCue>.Frame(place, updates);
}
