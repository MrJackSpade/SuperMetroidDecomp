namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>
/// The SPC700's 64 KiB audio RAM, which the managed driver and the S-DSP share (#1503). Every read the driver or the
/// DSP makes goes through the indexer, so a render can record exactly which bytes its output depended on.
/// </summary>
internal sealed class SpcRam
{
    /// <summary>The bytes of audio RAM.</summary>
    private readonly byte[] bytes = new byte[SpcDriverDefinitions.ApuRamSize];

    /// <summary>The byte at <paramref name="address"/>.</summary>
    internal byte this[int address]
    {
        get => bytes[address];
        set => bytes[address] = value;
    }

    /// <summary>Zeroes <paramref name="length"/> bytes from <paramref name="start"/>.</summary>
    internal void Clear(int start, int length) => Array.Clear(bytes, start, length);
}
