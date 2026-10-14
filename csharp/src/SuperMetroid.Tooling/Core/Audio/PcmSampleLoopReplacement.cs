namespace SuperMetroid.Core.Audio;

/// <summary>
/// Describes how a replacement waveform obtains its loop point. Loop positions are PCM frame
/// indexes; preserving time rescales the stock position when an HD file changes sample rate.
/// </summary>
/// <param name="Kind">Policy used to derive the replacement waveform's intrinsic loop point.</param>
/// <param name="ExplicitSampleIndex">Zero-based decoded mono PCM frame, used only by the explicit-index policy.</param>
public readonly record struct PcmSampleLoopReplacement(
    PcmSampleLoopReplacementKind Kind,
    int ExplicitSampleIndex = 0)
{
    /// <summary>Preserves the existing loop's elapsed-time position by rate conversion rounded to the nearest PCM frame, or preserves the absence of a loop.</summary>
    public static PcmSampleLoopReplacement PreserveTime =>
        new(PcmSampleLoopReplacementKind.PreserveTime);

    /// <summary>Selects a replacement with no intrinsic loop point, regardless of the previous catalog entry.</summary>
    public static PcmSampleLoopReplacement Disabled =>
        new(PcmSampleLoopReplacementKind.Disabled);

    /// <summary>Selects an explicit zero-based PCM loop frame; the installer later validates that it lies within the replacement waveform.</summary>
    public static PcmSampleLoopReplacement At(int sampleIndex) =>
        new(PcmSampleLoopReplacementKind.ExplicitSampleIndex, sampleIndex);
}
