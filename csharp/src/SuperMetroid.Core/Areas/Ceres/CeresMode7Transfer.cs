namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One $A6 Mode 7 low-byte tilemap DMA descriptor and its authored source bytes.</summary>
/// <param name="DestinationWord">VRAM word address of the first map cell; each tile replaces its low byte while preserving the high-byte Mode 7 character data.</param>
/// <param name="TileNumbers">Ordered eight-bit character selectors for consecutive map cells, not character pixel bytes.</param>
/// <param name="SourceOffset">Bank-<c>$A6</c> offset of the descriptor's native source bytes, which the queued descriptor records.</param>
public readonly record struct CeresMode7Transfer(
int DestinationWord, CeresMode7TransferDefinitions.TileSequence TileNumbers, int SourceOffset);
