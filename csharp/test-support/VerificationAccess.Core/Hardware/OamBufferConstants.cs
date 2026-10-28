namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="OamBuffer"/> that only verification reads.</summary>
internal static class OamBufferConstants
{
    /// <summary>Total 544-byte DMA payload containing the low table followed by the high table.</summary>
    public const int UploadByteCount = SnesPpuLayoutConstants.OamUploadByteCount;
}
