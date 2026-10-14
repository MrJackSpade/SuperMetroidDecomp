namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Sixteen-pixel blocks of the circular BG1/BG2 VRAM tilemap: a PPU scroll register shifted
/// right four times, as the level and background streamers address it.
/// </summary>
public sealed class TilemapBlocks : ISpatialSpace;
