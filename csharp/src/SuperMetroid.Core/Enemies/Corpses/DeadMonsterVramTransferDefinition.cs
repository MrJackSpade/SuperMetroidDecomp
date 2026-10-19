namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One immutable DMA descriptor selecting live corpse WRAM, not immutable artwork.</summary>
internal readonly record struct DeadMonsterVramTransferDefinition(
    ushort SizeInBytes, ushort SourceBankWord, ushort SourceOffset, ushort EncodedVramDestination)
{
    /// <summary>Combines the live bank-$7E source word with its bank byte for DMA addressing.</summary>
    internal int SourceAddress => ((SourceBankWord & 0xff00) << 8) | SourceOffset;

    /// <summary>The queued-DMA destination the descriptor's native destination word encodes.</summary>
    [PpuLayoutBoundary("codec: corpse rot DMA descriptor destination word")]
    internal VramDmaDestination Destination => VramDmaDestination.FromNative(EncodedVramDestination);
}
