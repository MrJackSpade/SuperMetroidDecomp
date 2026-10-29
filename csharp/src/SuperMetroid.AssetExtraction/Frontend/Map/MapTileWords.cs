namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Named cartridge map cell words: the characters the map's exploration rules recognise and the hidden cells the pause
/// screen, the room-select map and the HUD minimap show in place of unrevealed ones.
/// </summary>
public static class MapTileWords
{
    /// <summary>$90:AB00 recognizes character $028 and marks the map cell above Samus explored.</summary>
    public const ushort SlopedHallwayCharacter = 0x0028;
    /// <summary>$82:9517 uses character $00F for hidden cells without a downloaded map.</summary>
    public static readonly MapTileWord FileSelectUndownloadedBlank = new(0x000f);

    /// <summary>The cartridge's character-$01F empty map cell, without display attributes.</summary>
    public static readonly MapTileWord PauseBlank = new(0x001f);

    /// <summary>The same empty cell using the HUD minimap's unvisited priority/palette.</summary>
    public static readonly MapTileWord HudBlank = new(0x2c1f);
}
