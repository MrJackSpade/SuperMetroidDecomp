namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One fixed Dead Torizo corpse-transfer descriptor; source bytes remain mutable WRAM.</summary>
internal readonly record struct DeadTorizoVramTransferDefinition(
    ushort SizeInBytes, ushort SourceBankWord, ushort SourceOffset,
    ushort EncodedVramDestination)
{
    /// <summary>Combines the descriptor's encoded bank byte and source offset into the WRAM DMA address.</summary>
    internal int SourceAddress => ((SourceBankWord & 0xff00) << 8) | SourceOffset;

    /// <summary>The queued-DMA destination the descriptor's native destination word encodes.</summary>
    [PpuLayoutBoundary("codec: Dead Torizo corpse DMA descriptor destination word")]
    internal VramDmaDestination Destination => VramDmaDestination.FromNative(EncodedVramDestination);
}
