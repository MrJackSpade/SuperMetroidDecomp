using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The powered Work Robots' glow (#2380): its steps, each shown for its hold, which the room loops while the glow runs. A
/// powered robot's setup starts the glow on its last step for one update, so its first step shows on the next.
/// </summary>
internal static class WorkRobotGlowAnimations
{
    /// <summary>Every run's beats.</summary>
    internal static AnimationSegments<WorkRobotGlowSegment, WorkRobotGlowStep, NoAnimationCue> Segments { get; } = new(
        new Dictionary<WorkRobotGlowSegment, AnimationBeat<WorkRobotGlowStep, NoAnimationCue>[]>
    {
        [WorkRobotGlowSegment.Glowing] =
        [
            Show(WorkRobotGlowStep.AtFirstEnd, WorkRobotGlowDefinitions.EndHold),
            Show(WorkRobotGlowStep.LeavingFirstEnd, WorkRobotGlowDefinitions.PassingHold),
            Show(WorkRobotGlowStep.NearingFarEnd, WorkRobotGlowDefinitions.PassingHold),
            Show(WorkRobotGlowStep.AtFarEnd, WorkRobotGlowDefinitions.EndHold),
            Show(WorkRobotGlowStep.LeavingFarEnd, WorkRobotGlowDefinitions.PassingHold),
            Show(WorkRobotGlowStep.NearingFirstEnd, WorkRobotGlowDefinitions.PassingHold),
        ],
    });

    /// <summary>The glow's steps in the order it shows them, from its first; each step's place is its installed shade's step.</summary>
    internal static IReadOnlyList<WorkRobotGlowStep> Cycle { get; } =
        [.. Segments.Beats(WorkRobotGlowSegment.Glowing).Select(beat => beat.ShownFrame ?? throw new InvalidOperationException("Every beat of the glow shows a step."))];

    /// <summary>
    /// The glow as a powered robot's setup starts it: its last step for the update the setup gives it, then its steps from
    /// the first, over and over.
    /// </summary>
    internal static Animation<WorkRobotGlowSegment> Restarted { get; } = Animation<WorkRobotGlowSegment>.LeadingInto(
        [new(WorkRobotGlowSegment.Glowing, Cycle.Count - 1, 1, 1)],
        [Segments.Whole(WorkRobotGlowSegment.Glowing)]);

    /// <summary>The place of <paramref name="step"/> in <see cref="Cycle"/>, from zero.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The glow shows no such step.</exception>
    internal static int PlaceInCycle(WorkRobotGlowStep step)
    {
        for (int place = 0; place < Cycle.Count; place++)
        {
            if (Cycle[place] == step)
                return place;
        }
        throw new ArgumentOutOfRangeException(nameof(step), step, "The Work Robot glow shows no such step.");
    }

    /// <summary>Shows <paramref name="step"/> for <paramref name="updates"/> updates.</summary>
    private static AnimationBeat<WorkRobotGlowStep, NoAnimationCue> Show(WorkRobotGlowStep step, int updates) =>
        new AnimationBeat<WorkRobotGlowStep, NoAnimationCue>.Frame(step, updates);
}
