namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One $A6 Mode 7 low-byte tilemap DMA descriptor and its authored source bytes.</summary>
/// <param name="MapCell">
/// The Mode 7 map cell the first tile is drawn at; each tile replaces that cell's character, keeping the characters' pixels.
/// The map starts at VRAM word zero, so the cartridge's queued descriptor names the same number as its word.
/// </param>
/// <param name="TileNumbers">Ordered eight-bit character selectors for consecutive map cells, not character pixel bytes.</param>
/// <param name="SourceOffset">Bank-<c>$A6</c> offset of the descriptor's native source bytes, which the queued descriptor records.</param>
public readonly record struct CeresMode7Transfer(
int MapCell, CeresMode7TransferDefinitions.TileSequence TileNumbers, int SourceOffset);
