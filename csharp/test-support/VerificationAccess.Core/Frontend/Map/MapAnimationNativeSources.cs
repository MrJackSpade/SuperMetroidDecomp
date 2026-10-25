namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Verification's native view of the map animations (#1416): what only the ROM oracles read.</summary>
internal static class MapAnimationNativeSources
{
    /// <summary>$82:C100, SpritePalette_IndexValues[3], used by DrawPauseScreenSpriteAnim for highlights and map arrows.</summary>
    public const int AnimatedSpritePalette = 0x82c100;
    /// <summary>$82:C10C contains fourteen three-byte highlight records before the loop sentinel.</summary>
    public const int PaletteFrameCount = 14;
}
