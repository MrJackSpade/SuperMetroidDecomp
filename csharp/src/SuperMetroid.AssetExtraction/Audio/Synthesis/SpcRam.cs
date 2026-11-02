namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The SPC700's 64 KiB audio RAM, which the managed driver and the S-DSP share (#1503). Every read the driver or the
/// DSP makes goes through the indexer, so a render can record exactly which bytes its output depended on.
/// </summary>
internal sealed class SpcRam
{
    /// <summary>The bytes of audio RAM.</summary>
    private readonly byte[] bytes = new byte[SpcDriverDefinitions.ApuRamSize];

    /// <summary>The addresses read since recording started, or null while reads are not recorded.</summary>
    private bool[]? reads;

    /// <summary>The byte at <paramref name="address"/>; a read is recorded while recording.</summary>
    internal byte this[int address]
    {
        get
        {
            if (reads is not null)
                reads[address] = true;
            return bytes[address];
        }
        set => bytes[address] = value;
    }

    /// <summary>Zeroes <paramref name="length"/> bytes from <paramref name="start"/>.</summary>
    internal void Clear(int start, int length) => Array.Clear(bytes, start, length);

    /// <summary>A copy of every byte, taken without recording reads.</summary>
    internal byte[] Snapshot() => [.. bytes];

    /// <summary>Starts recording every address read from now on.</summary>
    internal void RecordReads() => reads = new bool[bytes.Length];

    /// <summary>The addresses read since <see cref="RecordReads"/>.</summary>
    /// <exception cref="InvalidOperationException">Reads are not being recorded.</exception>
    internal IEnumerable<int> RecordedReads()
    {
        bool[] recorded = reads ?? throw new InvalidOperationException("Audio RAM reads are not being recorded.");
        for (int address = 0; address < recorded.Length; address++)
        {
            if (recorded[address])
                yield return address;
        }
    }
}
