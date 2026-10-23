namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Cartridge values and helpers of <see cref="WorldMapArtworkFormat"/> that only verification reads.</summary>
internal static class WorldMapArtworkFormatConstants
{
    /// <summary>Menu BG1 character base in byte-addressed VRAM.</summary>
    public const int ForegroundDestination = 0;
    /// <summary>Menu BG3 character base in byte-addressed VRAM, BG34NBA=$04.</summary>
    public const int BackgroundDestination = WorldMapVramLayout.BackgroundByteAddress;
}
