using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>Source and byte extent of a queue producer, optionally backed by an installed asset.</summary>
internal sealed record VramDmaTransfer(string Owner, int SourceAddress, int ByteCount,
    VramAssetId Asset = VramAssetId.None);
