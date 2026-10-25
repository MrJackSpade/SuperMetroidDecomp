namespace SuperMetroid.Core.Map;

/// <summary>The bit fields of a <see cref="MapTileWord"/> and the HUD minimap's attribute patterns.</summary>
internal static class MapTileWordMasks
{
    /// <summary>Ten low bits selecting the BG map character independently of its attributes.</summary>
    internal const ushort CharacterMask = 0x03ff;

    /// <summary>Three SNES BG palette bits, including the explored-cell bit.</summary>
    internal const ushort PaletteMask = 0x1c00;

    /// <summary>Native BG priority bit preserved when presenting a tile.</summary>
    internal const ushort PriorityMask = 0x2000;

    /// <summary>Horizontal and vertical BG flip bits preserved by HUD projection.</summary>
    internal const ushort FlipMask = SnesBgTilemapWord.HorizontalFlipMask | SnesBgTilemapWord.VerticalFlipMask;

    /// <summary>Downloaded-map palette bit cleared when a cell has been explored.</summary>
    internal const ushort ExploredPaletteBit = 0x0400;

    /// <summary>HUD palette and priority for a visited minimap cell.</summary>
    internal const ushort HudExploredAttributes = 0x2800;

    /// <summary>HUD palette and priority for an unvisited minimap cell.</summary>
    internal const ushort HudUnexploredAttributes = 0x2c00;

    /// <summary>Alternating Samus marker palette bits applied during minimap blink.</summary>
    internal const ushort HudLocationBlinkAttributes = 0x1c00;
}
