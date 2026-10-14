namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Cartridge values of <see cref="MapStaticPalettesRomData"/> that only verification reads.</summary>
internal static class MapStaticPalettesRomDataConstants
{
    /// <summary>$81:A546 starts executable foreground-load code after all world-map palette copy records.</summary>
    public const int WorldPaletteDataEnd = 0x81a546;

    /// <summary>Provides the end of world map palette data for static palette extraction checks.</summary>
    extension(MapStaticPalettesRomData)
    {
        /// <inheritdoc cref="MapStaticPalettesRomDataConstants.WorldPaletteDataEnd"/>
        internal static int WorldPaletteDataEnd => MapStaticPalettesRomDataConstants.WorldPaletteDataEnd;
    }
}
