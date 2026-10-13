using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Mother Brain's glass projectile programs: eight spinning shard loops by flight angle, and the sparkle.</summary>
public enum MotherBrainGlassProgram
{
    /// <summary>Shard angle group 0 ($86:CC93).</summary>
    ShardGroup0,
    /// <summary>Shard angle group 1 ($86:CCB7).</summary>
    ShardGroup1,
    /// <summary>Shard angle group 2 ($86:CCDB).</summary>
    ShardGroup2,
    /// <summary>Shard angle group 3 ($86:CCFF).</summary>
    ShardGroup3,
    /// <summary>Shard angle group 4 ($86:CD23).</summary>
    ShardGroup4,
    /// <summary>Shard angle group 5 ($86:CD47).</summary>
    ShardGroup5,
    /// <summary>Shard angle group 6 ($86:CD6B).</summary>
    ShardGroup6,
    /// <summary>Shard angle group 7 ($86:CD8F).</summary>
    ShardGroup7,
    /// <summary>The finite sparkle a falling shard sheds ($86:CDB3).</summary>
    Sparkle,
}

/// <summary>One glass frame: its program and its position there.</summary>
/// <param name="Program">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
public readonly record struct MotherBrainGlassFrame(MotherBrainGlassProgram Program, int Phase);

/// <summary>Gameplay cues of the glass programs.</summary>
internal enum MotherBrainGlassCue
{
    /// <summary>Deletes the finished sparkle.</summary>
    Delete,
}

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

/// <summary>A glass shard's or sparkle's typed animation.</summary>
internal sealed class MotherBrainGlassAnimation(MotherBrainGlassProgram program)
    : EnemyProjectileTimelineAnimation<MotherBrainGlassFrame, MotherBrainGlassCue>
{
    /// <summary>The program played.</summary>
    internal MotherBrainGlassProgram Program { get; } = program;

    /// <inheritdoc/>
    private protected override AnimationTimeline<MotherBrainGlassFrame, MotherBrainGlassCue> Timeline =>
        MotherBrainGlassAnimations.Timeline(Program);

    /// <inheritdoc/>
    private protected override ushort PresentationKey(MotherBrainGlassFrame frame) => MotherBrainGlassVisualDefinitions.PresentationKey(frame);
}
