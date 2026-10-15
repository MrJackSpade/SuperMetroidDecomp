using SuperMetroid.Core.Audio;
using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// Replaceable mono PCM source consumed by a managed S-DSP voice. The sample rate controls
/// pitch normalization; loop position is expressed in decoded PCM frames, not BRR bytes.
/// </summary>
public sealed class ManagedPcmSample
{
    /// <summary>Private copy of signed mono frames so caller edits cannot change an installed sample.</summary>
    private readonly short[] samples;

    /// <summary>Creates an immutable validated mono PCM replacement sample.</summary>
    /// <param name="id">Stable sample identity used in diagnostics.</param>
    /// <param name="sampleRate">Source sample rate in hertz.</param>
    /// <param name="samples">Signed 16-bit mono PCM frames, copied by the constructor.</param>
    /// <param name="loopSampleIndex">Optional zero-based PCM frame at which looping resumes.</param>
    public ManagedPcmSample(
        string id,
        int sampleRate,
        short[] samples,
        int? loopSampleIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Ensure.NotNull(samples);
        if (sampleRate is < PcmSampleFormat.MinimumReplacementSampleRate or
            > PcmSampleFormat.MaximumReplacementSampleRate)
        {
            throw new InvalidDataException(
                $"PCM sample '{id}' rate {sampleRate} Hz is outside " +
                $"{PcmSampleFormat.MinimumReplacementSampleRate}-" +
                $"{PcmSampleFormat.MaximumReplacementSampleRate} Hz.");
        }
        if (samples.Length == 0)
            throw new InvalidDataException($"PCM sample '{id}' is empty.");
        if (loopSampleIndex is int loop && (uint)loop >= samples.Length)
            throw new InvalidDataException(
                $"PCM sample '{id}' loop {loop} is outside its {samples.Length} samples.");

        Id = id;
        SampleRate = sampleRate;
        this.samples = [.. samples];
        LoopSampleIndex = loopSampleIndex;
    }

    /// <summary>Stable source identity carried into missing-sample diagnostics.</summary>
    public string Id { get; }

    /// <summary>Source hertz used to normalize S-DSP voice pitch during playback.</summary>
    public int SampleRate { get; }

    /// <summary>Copied signed mono frames exposed without a mutable array reference.</summary>
    public ReadOnlyMemory<short> Samples => samples;

    /// <summary>Gets the optional zero-based PCM loop frame, or <see langword="null"/> for no intrinsic loop.</summary>
    public int? LoopSampleIndex { get; }
}
