
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of Mother Brain's glass (#641): the installed artwork key of every typed frame, the
/// bank-$86 address of the spritemap operand that drew it.
/// </summary>
internal static class MotherBrainGlassVisualDefinitions
{
    /// <summary>Distinct frames: eight per shard group, then the sparkle's four.</summary>
    internal const int ProjectileFrameCount = 8 * MotherBrainGlassAnimations.ShardPhaseCount + MotherBrainGlassAnimations.SparklePhaseCount;

    /// <summary>The first shard group's list at $86:CC93; each 36-byte group holds eight frames and its loop.</summary>
    private const ushort FirstShardProgram = 0xcc93;
    /// <summary>Bytes in each shard group's list.</summary>
    private const int ShardProgramBytes = 36;
    /// <summary>The sparkle's list at $86:CDB3.</summary>
    private const ushort SparkleProgram = 0xcdb3;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;

    /// <summary>Every frame in installed-artwork order: the shard groups in turn, then the sparkle.</summary>
    internal static IReadOnlyList<MotherBrainGlassFrame> ProjectileFrames { get; } =
    [
        .. Enum.GetValues<MotherBrainGlassProgram>().Where(program => program != MotherBrainGlassProgram.Sparkle)
            .SelectMany(program => Enumerable.Range(0, MotherBrainGlassAnimations.ShardPhaseCount)
                .Select(phase => new MotherBrainGlassFrame(program, phase))),
        .. Enumerable.Range(0, MotherBrainGlassAnimations.SparklePhaseCount)
            .Select(phase => new MotherBrainGlassFrame(MotherBrainGlassProgram.Sparkle, phase)),
    ];

    /// <summary>The installed presentation key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(MotherBrainGlassFrame frame) => unchecked((ushort)(
        (frame.Program == MotherBrainGlassProgram.Sparkle
            ? SparkleProgram
            : FirstShardProgram + ShardProgramBytes * (int)frame.Program)
        + FrameBytes * frame.Phase + OperandOffset));
}
