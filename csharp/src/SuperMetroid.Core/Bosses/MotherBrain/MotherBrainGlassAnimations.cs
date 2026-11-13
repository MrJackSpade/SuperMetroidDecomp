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
    /// The shard initializer maps one of sixteen random angle bins to its shard group; the bin widths 1, 2, 3, 2 repeat in
    /// the opposite half-turn.
    /// </summary>
    internal static MotherBrainGlassSegment ShardAnimation(int angleBin) => angleBin switch
    {
        0 => MotherBrainGlassSegment.ShardGroup0,
        1 or 2 => MotherBrainGlassSegment.ShardGroup1,
        3 or 4 or 5 => MotherBrainGlassSegment.ShardGroup2,
        6 or 7 => MotherBrainGlassSegment.ShardGroup3,
        8 => MotherBrainGlassSegment.ShardGroup4,
        9 or 10 => MotherBrainGlassSegment.ShardGroup5,
        11 or 12 or 13 => MotherBrainGlassSegment.ShardGroup6,
        14 or 15 => MotherBrainGlassSegment.ShardGroup7,
        _ => throw new ArgumentOutOfRangeException(nameof(angleBin), angleBin, "Glass-shard angle bins run from zero to fifteen."),
    };
}
