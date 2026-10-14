namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Independent horizontal/vertical flip bits shared by SNES BG tilemap and OBJ attribute
/// words. These are genuine flags: the PPU supports neither, either, or both simultaneously.
/// </summary>
[Flags]
public enum SnesTileFlipFlags : ushort
{
    /// <summary>Leaves both axes in their stored pixel order.</summary>
    None = 0,
    /// <summary>Sets bit 14 to reverse the tile's horizontal pixel order.</summary>
    Horizontal = 0x4000,
    /// <summary>Sets bit 15 to reverse the tile's vertical pixel order.</summary>
    Vertical = 0x8000,
}
