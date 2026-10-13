using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The presentation boundary of the red hand beam (#641): the installed artwork key of every typed frame, the
/// bank-$86 address of the spritemap operand that drew it.
/// </summary>
internal static class MotherBrainHandBeamVisualDefinitions
{
    /// <summary>Distinct frames: seven in each of three stages.</summary>
    internal const int ProjectileFrameCount = MotherBrainHandBeamAnimations.StageCount * MotherBrainHandBeamAnimations.FramesPerStage;

    /// <summary>The program's list at $86:C796.</summary>
    private const ushort Program = 0xc796;
    /// <summary>Bytes in each stage: seven frames and the five-byte external call after the first.</summary>
    private const int StageBytes = 33;
    /// <summary>The external call's five bytes, between a stage's first and second frames.</summary>
    private const int CallBytes = 5;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;

    /// <summary>Every frame in installed-artwork order: the stages in turn.</summary>
    internal static IReadOnlyList<MotherBrainHandBeamFrame> ProjectileFrames { get; } =
    [
        .. Enumerable.Range(0, MotherBrainHandBeamAnimations.StageCount).SelectMany(stage =>
            Enumerable.Range(0, MotherBrainHandBeamAnimations.FramesPerStage).Select(phase => new MotherBrainHandBeamFrame(stage, phase))),
    ];

    /// <summary>The installed presentation key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(MotherBrainHandBeamFrame frame) => unchecked((ushort)(
        Program + StageBytes * frame.Stage + FrameBytes * frame.Phase + (frame.Phase == 0 ? 0 : CallBytes) + OperandOffset));
}
