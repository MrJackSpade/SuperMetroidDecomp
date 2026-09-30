namespace SuperMetroid.AssetExtraction;

/// <summary>
/// One-time import of the retired ROM payload in old debugger states. Bytes are drained,
/// never materialized as a cartridge image or returned to executable gameplay state.
/// </summary>
public static class LegacyCartridgeStateImport
{
    public static void DiscardPayload(Stream source, int byteCount)
    {
        ArgumentNullException.ThrowIfNull(source);
        if ((uint)byteCount > LegacyCartridgeStateFormat.MaximumRomByteCount ||
            byteCount % LegacyCartridgeStateFormat.BankByteCount != 0)
            throw new InvalidDataException("Legacy debugger cartridge payload has an invalid LoROM size.");
        Span<byte> buffer = stackalloc byte[LegacyCartridgeStateFormat.DiscardBufferByteCount];
        while (byteCount > 0)
        {
            int length = Math.Min(buffer.Length, byteCount);
            source.ReadExactly(buffer[..length]);
            byteCount -= length;
        }
    }
}

/// <summary>Retired state-import geometry; these sizes do not expose a runtime cartridge map.</summary>
public static class LegacyCartridgeStateFormat
{
    /// <summary>The original address-space constructor accepted up to 128 complete LoROM banks.</summary>
    public const int MaximumRomByteCount = 0x400000;
    public const int BankByteCount = 0x8000;
    public const int DiscardBufferByteCount = 8192;
}
