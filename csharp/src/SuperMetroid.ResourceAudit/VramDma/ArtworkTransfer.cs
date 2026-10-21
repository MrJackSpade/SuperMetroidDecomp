using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>A native DMA transfer and the artwork it carries.</summary>
/// <param name="Source">The source.</param>
/// <param name="Count">The count.</param>
/// <param name="Asset">The asset.</param>
internal readonly record struct ArtworkTransfer(int Source, int Count, VramAssetId Asset);
