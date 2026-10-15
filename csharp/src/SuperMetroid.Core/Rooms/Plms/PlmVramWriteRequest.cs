namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One packed VRAM write-table record emitted by bank-$84 PLM bytecode.</summary>
public readonly record struct PlmVramWriteRequest(
    ushort SizeInBytes,
    int SourceAddress,
    ushort EncodedVramDestination)
{
    /// <summary>The queued-DMA destination the bytecode's native destination word encodes.</summary>
    [PpuLayoutBoundary("codec: PLM bytecode VRAM write-table destination word")]
    public VramDmaDestination Destination => VramDmaDestination.FromNative(EncodedVramDestination);
}
