namespace SuperMetroid.ResourceAudit.VramDma;

/// <summary>The source and length of a native DMA transfer.</summary>
/// <param name="Source">The source.</param>
/// <param name="Count">The count.</param>
internal readonly record struct NativeTransfer(int Source, int Count);
