namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Cartridge values of <see cref="MapAnimationRomData"/> that only verification reads.</summary>
internal static class MapAnimationRomDataConstants
{
    /// <summary>$82:C100, SpritePalette_IndexValues[3], used by DrawPauseScreenSpriteAnim for highlights and map arrows.</summary>
    public const int AnimatedSpritePalette = 0x82c100;
    /// <summary>$82:C10C contains fourteen three-byte highlight records before the loop sentinel.</summary>
    public const int PaletteFrameCount = 14;

    /// <summary>Provides map animation sprite palette location and frame count for imported map checks.</summary>
    extension(MapAnimationRomData)
    {
        /// <inheritdoc cref="MapAnimationRomDataConstants.AnimatedSpritePalette"/>
        internal static int AnimatedSpritePalette => MapAnimationRomDataConstants.AnimatedSpritePalette;
        /// <inheritdoc cref="MapAnimationRomDataConstants.PaletteFrameCount"/>
        internal static int PaletteFrameCount => MapAnimationRomDataConstants.PaletteFrameCount;
    }
}
