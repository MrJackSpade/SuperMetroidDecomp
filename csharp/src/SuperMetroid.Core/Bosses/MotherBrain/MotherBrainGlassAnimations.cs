using SuperMetroid.Core.Programs;
using Beat = SuperMetroid.Core.Programs.AnimationBeat<SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassFrame, SuperMetroid.Core.Bosses.MotherBrain.MotherBrainGlassCue>;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's typed glass animations (#641, #1600).</summary>
internal static class MotherBrainGlassAnimations
{
    /// <summary>Frames in each shard loop.</summary>
    internal const int ShardPhaseCount = 8;
    /// <summary>Frames in the sparkle.</summary>
    internal const int SparklePhaseCount = 4;

    /// <summary>
    /// Every glass segment: a shard spins through its eight frames with the symmetric 4, 3, 2, 3 cadence twice; the
    /// sparkle alternates six and eight updates over four frames, then deletes itself.
    /// </summary>
    internal static AnimationSegments<MotherBrainGlassSegment, MotherBrainGlassFrame, MotherBrainGlassCue> Segments { get; } = new(
        Enum.GetValues<MotherBrainGlassSegment>().ToDictionary(segment => segment, Build));

    /// <summary>Each glass animation: a shard's spin loops for good, and the deleted sparkle rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainGlassSegment, Animation<MotherBrainGlassSegment>> StartingAt { get; } =
        Enum.GetValues<MotherBrainGlassSegment>().ToDictionary(segment => segment, segment => new Animation<MotherBrainGlassSegment>(
            [new(segment, 0, Segments.Beats(segment).Count, 1)],
            segment == MotherBrainGlassSegment.Sparkle ? AnimationEnd.Holds : AnimationEnd.LoopsFrom(0)));

    /// <summary>The beats of <paramref name="segment"/>.</summary>
    private static Beat[] Build(MotherBrainGlassSegment segment) =>
        segment == MotherBrainGlassSegment.Sparkle
            ?
            [
                .. Enumerable.Range(0, SparklePhaseCount).Select(phase => new Beat.Frame(new(segment, phase), phase % 2 == 0 ? 6 : 8)),
                new Beat.Event(MotherBrainGlassCue.Delete),
            ]
            :
            [
                .. Enumerable.Range(0, ShardPhaseCount).Select(phase => new Beat.Frame(new(segment, phase), ShardDuration(phase))),
            ];

    /// <summary>The 4, 3, 2, 3 cadence of a shard's spin.</summary>
    private static int ShardDuration(int phase) => (phase % 4) switch
    {
        0 => 4,
        2 => 2,
        _ => 3,
    };

    /// <summary>
    /// The spin a shard launched at each sixteenth of the turn plays, in turn order: the groups span one, two, three and two
    /// sixteenths, and repeat so in the opposite half-turn.
    /// </summary>
    internal static TurnArcs<MotherBrainGlassSegment> ShardAnimations { get; } = new(TrigonometryAngle.Zero,
        MotherBrainGlassSegment.ShardGroup0,
        MotherBrainGlassSegment.ShardGroup1, MotherBrainGlassSegment.ShardGroup1,
        MotherBrainGlassSegment.ShardGroup2, MotherBrainGlassSegment.ShardGroup2, MotherBrainGlassSegment.ShardGroup2,
        MotherBrainGlassSegment.ShardGroup3, MotherBrainGlassSegment.ShardGroup3,
        MotherBrainGlassSegment.ShardGroup4,
        MotherBrainGlassSegment.ShardGroup5, MotherBrainGlassSegment.ShardGroup5,
        MotherBrainGlassSegment.ShardGroup6, MotherBrainGlassSegment.ShardGroup6, MotherBrainGlassSegment.ShardGroup6,
        MotherBrainGlassSegment.ShardGroup7, MotherBrainGlassSegment.ShardGroup7);
}
