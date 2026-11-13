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
    internal static AnimationSegments<MotherBrainGlassProgram, MotherBrainGlassFrame, MotherBrainGlassCue> Segments { get; } = new(
        Enum.GetValues<MotherBrainGlassProgram>().ToDictionary(program => program, Build));

    /// <summary>Each glass animation: a shard's spin loops for good, and the deleted sparkle rests.</summary>
    internal static IReadOnlyDictionary<MotherBrainGlassProgram, Animation<MotherBrainGlassProgram>> StartingAt { get; } =
        Enum.GetValues<MotherBrainGlassProgram>().ToDictionary(program => program, program => new Animation<MotherBrainGlassProgram>(
            [new(program, 0, Segments.Beats(program).Count, 1)],
            program == MotherBrainGlassProgram.Sparkle ? AnimationEnd.Holds : AnimationEnd.LoopsFrom(0)));

    /// <summary>The beats of <paramref name="program"/>.</summary>
    private static Beat[] Build(MotherBrainGlassProgram program) =>
        program == MotherBrainGlassProgram.Sparkle
            ?
            [
                .. Enumerable.Range(0, SparklePhaseCount).Select(phase => new Beat.Frame(new(program, phase), phase % 2 == 0 ? 6 : 8)),
                new Beat.Event(MotherBrainGlassCue.Delete),
            ]
            :
            [
                .. Enumerable.Range(0, ShardPhaseCount).Select(phase => new Beat.Frame(new(program, phase), ShardDuration(phase))),
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
    internal static TurnArcs<MotherBrainGlassProgram> ShardPrograms { get; } = new(TrigonometryAngle.Zero,
        MotherBrainGlassProgram.ShardGroup0,
        MotherBrainGlassProgram.ShardGroup1, MotherBrainGlassProgram.ShardGroup1,
        MotherBrainGlassProgram.ShardGroup2, MotherBrainGlassProgram.ShardGroup2, MotherBrainGlassProgram.ShardGroup2,
        MotherBrainGlassProgram.ShardGroup3, MotherBrainGlassProgram.ShardGroup3,
        MotherBrainGlassProgram.ShardGroup4,
        MotherBrainGlassProgram.ShardGroup5, MotherBrainGlassProgram.ShardGroup5,
        MotherBrainGlassProgram.ShardGroup6, MotherBrainGlassProgram.ShardGroup6, MotherBrainGlassProgram.ShardGroup6,
        MotherBrainGlassProgram.ShardGroup7, MotherBrainGlassProgram.ShardGroup7);
}
