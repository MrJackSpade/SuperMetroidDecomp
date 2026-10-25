using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Draygon tables extraction reads (#1413): the import pipeline's private knowledge of the
/// cartridge, decoded once into the installed Draygon colour document.
/// </summary>
[CpuBusBoundary("catalog: Draygon cartridge sources")]
internal static class DraygonSourceDefinitions
{
    /// <summary>
    /// $A5:A217, the body initializer $A5:8687's 25 colours: <c>Palette_Draygon_Sprite1</c> continuing into
    /// <c>Palette_Draygon_Sprite2</c>.
    /// </summary>
    internal const int IntroColors = 0xa5a217;

    /// <summary>$A5:A277, <c>Palette_Draygon_BG12_5</c>, the body palette the hurt AI $A5:954D copies.</summary>
    internal const int BackgroundColors = 0xa5a277;

    /// <summary>$A5:A1F7, <c>Palette_Draygon_Sprite7</c>, the sprite palette the hurt AI $A5:954D copies.</summary>
    internal const int SpriteColors = 0xa5a1f7;

    /// <summary>$A5:A297, <c>Palette_Draygon_WhiteFlash</c>.</summary>
    internal const int WhiteFlashColors = 0xa5a297;

    /// <summary>$A5:96AF, <c>DraygonHealthBasedPaletteTable</c>: eight consecutive four-colour rows.</summary>
    internal const int HealthBandColors = 0xa596af;
}
