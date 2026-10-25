namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Verification's native view of the map screens' static colours (#1416): what only the ROM oracles read.</summary>
internal static class MapStaticPalettesNativeSources
{
    /// <summary>$81:A546 starts executable foreground-load code after all world-map palette copy records.</summary>
    public const int WorldPaletteDataEnd = 0x81a546;
}
