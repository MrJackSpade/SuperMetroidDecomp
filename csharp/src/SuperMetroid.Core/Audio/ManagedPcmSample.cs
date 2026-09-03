namespace SuperMetroid.Core.Audio;

/// <summary>
/// Replaceable mono PCM source consumed by a managed S-DSP voice. The sample rate controls
/// pitch normalization; loop position is expressed in decoded PCM frames, not BRR bytes.
/// </summary>
public sealed class ManagedPcmSample
{
    private readonly short[] samples;

    public ManagedPcmSample(
        string id,
        int sampleRate,
        short[] samples,
        int? loopSampleIndex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(samples);
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

    public string Id { get; }

    public int SampleRate { get; }

    public ReadOnlyMemory<short> Samples => samples;

    public int? LoopSampleIndex { get; }
}

/// <summary>Source-number mappings installed by one common or music-bank upload.</summary>
public sealed class ManagedPcmSampleBank
{
    private readonly Dictionary<byte, ManagedPcmSample> samples;

    public ManagedPcmSampleBank(string name, int uploadAddress, IReadOnlyDictionary<byte, ManagedPcmSample> samples)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(samples);
        Name = name;
        UploadAddress = uploadAddress;
        this.samples = new Dictionary<byte, ManagedPcmSample>(samples);
    }

    public string Name { get; }

    public int UploadAddress { get; }

    public IReadOnlyDictionary<byte, ManagedPcmSample> Samples => samples;

    public ManagedPcmSample Resolve(byte sourceNumber) =>
        samples.TryGetValue(sourceNumber, out ManagedPcmSample? sample)
            ? sample
            : throw new InvalidDataException(
                $"PCM bank '{Name}' (${UploadAddress:X6}) does not map source ${sourceNumber:X2}.");
}
