namespace SuperMetroid.Core.Audio;

/// <summary>
/// Replaceable mono PCM source consumed by a managed S-DSP voice. The sample rate controls
/// pitch normalization; loop position is expressed in decoded PCM frames, not BRR bytes.
/// </summary>
public sealed class ManagedPcmSample
{
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

    /// <summary>Gets the stable sample identity.</summary>
    public string Id { get; }

    /// <summary>Gets the source sample rate in hertz.</summary>
    public int SampleRate { get; }

    /// <summary>Gets the immutable signed 16-bit mono PCM frames.</summary>
    public ReadOnlyMemory<short> Samples => samples;

    /// <summary>Gets the optional zero-based PCM loop frame, or <see langword="null"/> for no intrinsic loop.</summary>
    public int? LoopSampleIndex { get; }
}

/// <summary>Source-number mappings installed by one common or music-bank upload.</summary>
public sealed class ManagedPcmSampleBank
{
    private readonly Dictionary<byte, ManagedPcmSample> samples;
    private readonly Dictionary<byte, byte> loopEntrySources;

    /// <summary>Creates an immutable source-number mapping for one native sample upload.</summary>
    /// <param name="name">Stable upload or bank name used in diagnostics.</param>
    /// <param name="uploadAddress">Native cartridge upload source identity.</param>
    /// <param name="samples">Managed samples keyed by S-DSP source number.</param>
    /// <param name="loopEntrySources">Optional mappings from a source's loop entry to another canonical source.</param>
    public ManagedPcmSampleBank(string name, int uploadAddress, IReadOnlyDictionary<byte, ManagedPcmSample> samples,
        IReadOnlyDictionary<byte, byte>? loopEntrySources = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(samples);
        Name = name;
        UploadAddress = uploadAddress;
        this.samples = new Dictionary<byte, ManagedPcmSample>(samples);
        this.loopEntrySources = loopEntrySources is null ? [] : new(loopEntrySources);
        foreach (var entry in this.loopEntrySources)
            if (!this.samples.ContainsKey(entry.Key) || !this.samples.ContainsKey(entry.Value))
                throw new InvalidDataException($"PCM bank '{name}' has an unmapped loop-entry source ${entry.Key:X2}->${entry.Value:X2}.");
    }

    /// <summary>Gets the stable upload or bank name.</summary>
    public string Name { get; }

    /// <summary>Gets the native cartridge address identifying the sample upload.</summary>
    public int UploadAddress { get; }

    /// <summary>Gets the installed managed samples keyed by S-DSP source number.</summary>
    public IReadOnlyDictionary<byte, ManagedPcmSample> Samples => samples;

    /// <summary>Resolves a key-on source number to its installed managed sample.</summary>
    /// <param name="sourceNumber">S-DSP source number selected by the voice.</param>
    /// <returns>The mapped PCM sample.</returns>
    public ManagedPcmSample Resolve(byte sourceNumber) =>
        samples.TryGetValue(sourceNumber, out ManagedPcmSample? sample)
            ? sample
            : throw new InvalidDataException(
                $"PCM bank '{Name}' (${UploadAddress:X6}) does not map source ${sourceNumber:X2}.");

    /// <summary>
    /// Resolves DIR's loop entry independently of key-on's start entry. A non-looping
    /// source can still be selected while a different voice sample reaches END/LOOP;
    /// its native loop address can point to the start of another canonical sample.
    /// </summary>
    public (ManagedPcmSample Sample, int Cursor) ResolveLoopEntry(byte sourceNumber)
    {
        if (loopEntrySources.TryGetValue(sourceNumber, out byte target))
            return (Resolve(target), 0);
        ManagedPcmSample sample = Resolve(sourceNumber);
        return (sample, sample.LoopSampleIndex ?? 0);
    }
}
