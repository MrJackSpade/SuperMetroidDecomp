using SuperMetroid.Core.Programs;
using P = SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTopTubeSegment;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's falling ceiling-tube animations (#641, #1600), in place of their lists: one pose, held.</summary>
internal static class MotherBrainTopTubeAnimations
{
    /// <summary>Every segment; each shows its own tube.</summary>
    internal static AnimationSegments<MotherBrainTopTubeSegment, MotherBrainTopTubeSegment, NoAnimationCue> Segments { get; } = new(
        Enum.GetValues<P>().ToDictionary(tube => tube, tube => new AnimationBeat<P, NoAnimationCue>[]
        {
            new AnimationBeat<P, NoAnimationCue>.Frame(tube, 1),
        }));

    /// <summary>Each tube's animation: its pose, held.</summary>
    internal static IReadOnlyDictionary<MotherBrainTopTubeSegment, Animation<MotherBrainTopTubeSegment>> StartingAt { get; } =
        Enum.GetValues<P>().ToDictionary(tube => tube, tube => new Animation<P>([new(tube, 0, 1, 1)], AnimationEnd.Holds));
}
