using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's typed glass programs (#641), in place of their bank-$86 instruction lists.</summary>
internal static class MotherBrainGlassAnimations
{
    /// <summary>Frames in each shard loop.</summary>
    internal const int ShardPhaseCount = 8;
    /// <summary>Frames in the sparkle.</summary>
    internal const int SparklePhaseCount = 4;

    /// <summary>The timeline of <paramref name="program"/>.</summary>
    internal static AnimationTimeline<MotherBrainGlassFrame, MotherBrainGlassCue> Timeline(MotherBrainGlassProgram program) =>
        Timelines[(int)program];

    /// <summary>Every program's timeline, by program.</summary>
    private static readonly AnimationTimeline<MotherBrainGlassFrame, MotherBrainGlassCue>[] Timelines =
        [.. Enum.GetValues<MotherBrainGlassProgram>().Select(Build)];

    /// <summary>
    /// A shard spins through its eight frames with the symmetric 4, 3, 2, 3 cadence twice and loops; the sparkle
    /// alternates six and eight updates over four frames, then deletes itself.
    /// </summary>
    private static AnimationTimeline<MotherBrainGlassFrame, MotherBrainGlassCue> Build(MotherBrainGlassProgram program) =>
        program == MotherBrainGlassProgram.Sparkle
            ? new(
            [
                .. Enumerable.Range(0, SparklePhaseCount).Select(phase =>
                    AnimationStep<MotherBrainGlassFrame, MotherBrainGlassCue>.Show(new(program, phase), phase % 2 == 0 ? 6 : 8)),
                AnimationStep<MotherBrainGlassFrame, MotherBrainGlassCue>.Publish(MotherBrainGlassCue.Delete),
                AnimationStep<MotherBrainGlassFrame, MotherBrainGlassCue>.Hold,
            ])
            : new(
            [
                .. Enumerable.Range(0, ShardPhaseCount).Select(phase =>
                    AnimationStep<MotherBrainGlassFrame, MotherBrainGlassCue>.Show(new(program, phase), ShardDuration(phase))),
                AnimationStep<MotherBrainGlassFrame, MotherBrainGlassCue>.Loop,
            ]);

    /// <summary>The 4, 3, 2, 3 cadence of a shard's spin.</summary>
    private static int ShardDuration(int phase) => (phase % 4) switch
    {
        0 => 4,
        2 => 2,
        _ => 3,
    };

    /// <summary>
    /// <c>$86:CE41</c> maps one of sixteen random angle bins to its shard group; the bin widths 1, 2, 3, 2 repeat in
    /// the opposite half-turn.
    /// </summary>
    internal static MotherBrainGlassProgram ShardProgram(int angleBin) => angleBin switch
    {
        0 => MotherBrainGlassProgram.ShardGroup0,
        1 or 2 => MotherBrainGlassProgram.ShardGroup1,
        3 or 4 or 5 => MotherBrainGlassProgram.ShardGroup2,
        6 or 7 => MotherBrainGlassProgram.ShardGroup3,
        8 => MotherBrainGlassProgram.ShardGroup4,
        9 or 10 => MotherBrainGlassProgram.ShardGroup5,
        11 or 12 or 13 => MotherBrainGlassProgram.ShardGroup6,
        14 or 15 => MotherBrainGlassProgram.ShardGroup7,
        _ => throw new ArgumentOutOfRangeException(nameof(angleBin), angleBin, "Glass-shard angle bins run from zero to fifteen."),
    };
}
