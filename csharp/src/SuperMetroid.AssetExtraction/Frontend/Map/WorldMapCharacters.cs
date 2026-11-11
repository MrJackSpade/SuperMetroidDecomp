using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed world-map character atlases as the import pipeline reads them (#1409), kept as installed (#1615): the
/// foreground and background atlases' pixels compiled whole to the planar characters the world-map tile sheets compose
/// from.
/// </summary>
public sealed class WorldMapCharacters
{
    /// <summary>The installed foreground atlas as 4-bpp planar characters.</summary>
    private readonly byte[] foreground;
    /// <summary>The installed background atlas as 2-bpp planar characters.</summary>
    private readonly byte[] background;

    /// <summary>Retains both installed atlases' compiled characters.</summary>
    /// <param name="foreground">The foreground atlas's planar characters.</param>
    /// <param name="background">The background atlas's planar characters.</param>
    private WorldMapCharacters(byte[] foreground, byte[] background)
    {
        this.foreground = foreground;
        this.background = background;
    }

    /// <summary>Imports the installed world-map foreground and background indexed PNGs, validating geometry and bitplane index ranges.</summary>
    /// <param name="foregroundPng">Caller-owned foreground atlas stream with the format's fixed dimensions and pixel indices 0..15 for 4-bpp characters.</param>
    /// <param name="backgroundPng">Caller-owned background/font atlas stream with the format's fixed dimensions and pixel indices 0..3 for 2-bpp characters.</param>
    /// <returns>The installed atlases' characters; PNG display colors are not runtime palettes.</returns>
    /// <exception cref="InvalidDataException">PNG structure, dimensions, or a pixel's bitplane index range is invalid.</exception>
    public static WorldMapCharacters Read(Stream foregroundPng, Stream backgroundPng)
    {
        var front = IndexedPng.Read(foregroundPng, new(WorldMapArtworkFormat.Width, WorldMapArtworkFormat.ForegroundHeight));
        var back = IndexedPng.Read(backgroundPng, new(WorldMapArtworkFormat.Width, WorldMapArtworkFormat.BackgroundHeight));
        return new(SnesPlanarTileEncoder.Encode(front.Pixels, front.Size, 4), SnesPlanarTileEncoder.Encode(back.Pixels, back.Size, 2));
    }

    /// <summary>The planar 4-bpp world foreground characters.</summary>
    public byte[] ForegroundCharacters() => (byte[])foreground.Clone();

    /// <summary>The planar 2-bpp world background characters.</summary>
    public byte[] BackgroundCharacters() => (byte[])background.Clone();
}
