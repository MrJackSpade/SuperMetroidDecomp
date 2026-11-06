using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Named cartridge map cell words: the hidden cell the HUD minimap shows in place of unrevealed ones. The map screens show
/// the empty map cell instead (#1513), which the import decodes from the maps themselves.
/// </summary>
[EncodingOwner("area map cell word of the HUD minimap's hidden cell")]
public static class MapTileWords
{
    /// <summary>The empty cell using the HUD minimap's unvisited priority/palette.</summary>
    public static readonly MapTileWord HudBlank = new(0x2c1f);
}
