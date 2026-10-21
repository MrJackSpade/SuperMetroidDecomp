using SuperMetroid.Core.Audio;
using SuperMetroid.Core;
namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Source-number mappings installed by one common or music-bank upload.</summary>
public sealed class ManagedPcmSampleBank
{
    /// <summary>Installed PCM keyed by the source numbers written into S-DSP voices.</summary>
    private readonly Dictionary<int, ManagedPcmSample> samples;
    /// <summary>DIR loop entries that point to another sample's start instead of their own loop cursor.</summary>
    private readonly Dictionary<int, int> loopEntrySources;

    /// <summary>Creates an immutable source-number mapping for one native sample upload.</summary>
    /// <param name="name">Stable upload or bank name used in diagnostics.</param>
    /// <param name="bank">The upload that installs this sample map.</param>
    /// <param name="samples">Managed samples keyed by S-DSP source number.</param>
    /// <param name="loopEntrySources">Optional mappings from a source's loop entry to another canonical source.</param>
    public ManagedPcmSampleBank(string name, AudioBank bank, IReadOnlyDictionary<int, ManagedPcmSample> samples,
        IReadOnlyDictionary<int, int>? loopEntrySources = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Ensure.NotNull(samples);
        Name = name;
        Bank = bank;
        this.samples = new Dictionary<int, ManagedPcmSample>(samples);
        this.loopEntrySources = loopEntrySources is null ? [] : new(loopEntrySources);
        foreach (var entry in this.loopEntrySources)
            if (!this.samples.ContainsKey(entry.Key) || !this.samples.ContainsKey(entry.Value))
                throw new InvalidDataException($"PCM bank '{name}' has an unmapped loop-entry source ${entry.Key:X2}->${entry.Value:X2}.");
    }

    /// <summary>Gets the stable upload or bank name.</summary>
    public string Name { get; }

    /// <summary>Gets the upload that installs this sample map.</summary>
    public AudioBank Bank { get; }

    /// <summary>Gets the installed managed samples keyed by S-DSP source number.</summary>
    public IReadOnlyDictionary<int, ManagedPcmSample> Samples => samples;

    /// <summary>Resolves a key-on source number to its installed managed sample.</summary>
    /// <param name="sourceNumber">S-DSP source number selected by the voice.</param>
    /// <returns>The mapped PCM sample.</returns>
    public ManagedPcmSample Resolve(int sourceNumber) =>
        samples.TryGetValue(sourceNumber, out ManagedPcmSample? sample)
            ? sample
            : throw new InvalidDataException(
                $"PCM bank '{Name}' ({Bank}) does not map source ${sourceNumber:X2}.");

    /// <summary>
    /// Resolves DIR's loop entry independently of key-on's start entry. A non-looping
    /// source can still be selected while a different voice sample reaches END/LOOP;
    /// its native loop address can point to the start of another canonical sample.
    /// </summary>
    public PcmLoopEntry ResolveLoopEntry(int sourceNumber)
    {
        if (loopEntrySources.TryGetValue(sourceNumber, out int target))
            return new PcmLoopEntry(Resolve(target), 0);
        ManagedPcmSample sample = Resolve(sourceNumber);
        return new PcmLoopEntry(sample, sample.LoopSampleIndex ?? 0);
    }
}
