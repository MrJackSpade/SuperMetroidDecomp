using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable world-map character atlases with calculated geometry and independently editable pixels.</summary>
public sealed class WorldMapArtwork
{
    // Reviewed drawings, letter contours, ink roles and mechanical shading;
    // calculated geometry and independent supplied differences stay separate.
    /// <summary>Foreground atlas pixel values that differ from their calculated or copied defaults.</summary>
    private readonly Dictionary<int, byte> foreground;
    /// <summary>Per-tile authored glyph bits that supplement the calculated foreground font mask.</summary>
    private readonly Dictionary<int, ulong> fontFill = new();
    // Exact remaining BG3 font/icon contours and selected edge decisions, plus
    // independent supplied differences from the calculated primitive/outline view.
    /// <summary>Background atlas pixel values that differ from calculated tile and digit defaults.</summary>
    private readonly Dictionary<int, byte> background;
    /// <summary>Authored foreground-pixel footprints used to reconstruct outlined digit interiors.</summary>
    private readonly Dictionary<int, ulong> digitFill = new();
    // One delegate for the per-pixel font derivation, rather than one per pixel.
    /// <summary>Cached callback for deriving a foreground glyph's 8-by-8 bit mask.</summary>
    private readonly Func<int, ulong> fontMask;

    /// <summary>Builds sparse authored-difference maps from validated, row-major indexed atlas pixels.</summary>
    /// <param name="foreground">4-bpp atlas pixels used to retain deviations from calculated foreground artwork.</param>
    /// <param name="background">2-bpp atlas pixels used to retain deviations from calculated background artwork.</param>
    private WorldMapArtwork(byte[] foreground, byte[] background)
    {
        fontMask = FontMask;
        var masks = new ulong[WorldMapArtworkFormat.ForegroundTileCount];
        for (int tile = 0; tile < masks.Length; tile++)
        {
            if (!WorldMapTileDefinitions.IsForegroundFontTile(tile)) continue;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (foreground[TilePixel(tile, x, y)] == WorldMapTileDefinitions.ForegroundFontFace)
                    masks[tile] |= 1UL << (y * 8 + x);
        }
        for (int tile = 0; tile < masks.Length; tile++)
        {
            if (!WorldMapTileDefinitions.IsForegroundFontTile(tile)) continue;
            int source = WorldMapTileDefinitions.ForegroundSourcePixel(TilePixel(tile, 0, 0));
            int sourceTile = source < 0 ? tile : PixelTile(source);
            ulong expected = sourceTile == tile ? 0 : masks[sourceTile];
            if (masks[tile] != expected) fontFill.Add(tile, masks[tile]);
        }
        this.foreground = Enumerable.Range(0, foreground.Length).Where(index =>
        {
            int source = WorldMapTileDefinitions.ForegroundSourcePixel(index);
            if (source != index) return foreground[index] != (source < 0 ? 0 : foreground[source]);
            return !WorldMapTileDefinitions.TryForegroundFontPixel(index, fontMask, out byte value) || foreground[index] != value;
        }).ToDictionary(index => index, index => foreground[index]);
        for (int tile = 0; tile < WorldMapArtworkFormat.BackgroundTileCount; tile++)
        {
            if (!WorldMapTileDefinitions.IsOutlinedDigit(tile)) continue;
            ulong mask = 0;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (background[(tile / WorldMapArtworkFormat.TileColumns * 8 + y) * WorldMapArtworkFormat.Width +
                    tile % WorldMapArtworkFormat.TileColumns * 8 + x] == 2)
                    mask |= 1UL << (y * 8 + x);
            digitFill.Add(tile, mask);
        }
        this.background = Enumerable.Range(0, background.Length)
            .Where(index => !TryBackgroundDefault(index, out byte value) || background[index] != value)
            .ToDictionary(index => index, index => background[index]);
    }
    /// <summary>Imports the installed world-map foreground and background indexed PNGs, validating geometry and bitplane index ranges while retaining independent authored pixel differences.</summary>
    /// <param name="foregroundPng">Caller-owned foreground atlas stream with the format's fixed dimensions and pixel indices 0..15 for 4-bpp characters.</param>
    /// <param name="backgroundPng">Caller-owned background/font atlas stream with the format's fixed dimensions and pixel indices 0..3 for 2-bpp characters.</param>
    /// <returns>Immutable artwork that reconstructs the selected pixels from calculated geometry and owned authored differences; PNG display colors are not runtime palettes.</returns>
    /// <exception cref="InvalidDataException">PNG structure, dimensions, or a pixel's bitplane index range is invalid.</exception>
    public static WorldMapArtwork Load(Stream foregroundPng, Stream backgroundPng)
    {
        var front = IndexedPng.Read(foregroundPng, WorldMapArtworkFormat.Width, WorldMapArtworkFormat.ForegroundHeight);
        var back = IndexedPng.Read(backgroundPng, WorldMapArtworkFormat.Width, WorldMapArtworkFormat.BackgroundHeight);
        _ = SnesPlanarTileEncoder.Encode(front.Pixels, front.Width, front.Height, 4);
        // Keep the original import-time range validation and diagnostic, while
        // retaining only the source pixels needed by the calculated character views.
        _ = SnesPlanarTileEncoder.Encode(back.Pixels, back.Width, back.Height, 2);
        return new(front.Pixels, back.Pixels);
    }
    /// <summary>Reconstructs both selected atlases, encodes their row-major 8x8 characters, and loads the foreground 4-bpp and background 2-bpp streams at their fixed VRAM byte offsets.</summary>
    /// <param name="vram">Destination VRAM receiving both character transfers; this method does not install palette colors or world-map layout words.</param>
    public void LoadTo(SnesVram vram)
    {
        var front = new byte[WorldMapArtworkFormat.Width * WorldMapArtworkFormat.ForegroundHeight];
        for (int index = 0; index < front.Length; index++)
            front[index] = ForegroundPixel(index);
        vram.LoadBytes(WorldMapArtworkFormat.ForegroundDestination,
            SnesPlanarTileEncoder.Encode(front, WorldMapArtworkFormat.Width, WorldMapArtworkFormat.ForegroundHeight, 4));
        var pixels = new byte[WorldMapArtworkFormat.Width * WorldMapArtworkFormat.BackgroundHeight];
        for (int index = 0; index < pixels.Length; index++)
            pixels[index] = background.TryGetValue(index, out byte value) ? value
                : TryBackgroundDefault(index, out byte calculated) ? calculated
                : throw new InvalidDataException("World background source pixel is unavailable.");
        vram.LoadBytes(WorldMapArtworkFormat.BackgroundDestination,
            SnesPlanarTileEncoder.Encode(pixels, WorldMapArtworkFormat.Width, WorldMapArtworkFormat.BackgroundHeight, 2));
    }

    /// <summary>Maps a tile coordinate to its row-major pixel offset in the world-map atlas.</summary>
    /// <param name="tile">Zero-based character index in the atlas.</param>
    /// <param name="x">Pixel column within the 8-by-8 character.</param>
    /// <param name="y">Pixel row within the 8-by-8 character.</param>
    /// <returns>Linear pixel index for the requested character coordinate.</returns>
    private static int TilePixel(int tile, int x, int y) =>
        (tile / WorldMapArtworkFormat.TileColumns * 8 + y) * WorldMapArtworkFormat.Width +
        tile % WorldMapArtworkFormat.TileColumns * 8 + x;
    /// <summary>Finds the 8-by-8 character containing a row-major atlas pixel.</summary>
    /// <param name="index">Linear pixel index within the atlas.</param>
    /// <returns>Zero-based character index in the atlas tile grid.</returns>
    private static int PixelTile(int index) => index / WorldMapArtworkFormat.Width / 8 *
        WorldMapArtworkFormat.TileColumns + index % WorldMapArtworkFormat.Width / 8;

    /// <summary>Reconstructs a glyph footprint from retained fill bits or the glyph's source-tile relationship.</summary>
    /// <param name="tile">Foreground font character whose 64-pixel mask is requested.</param>
    /// <returns>One bit per pixel, set where the derived glyph uses its foreground face color.</returns>
    private ulong FontMask(int tile)
    {
        if (fontFill.TryGetValue(tile, out ulong mask)) return mask;
        int source = WorldMapTileDefinitions.ForegroundSourcePixel(TilePixel(tile, 0, 0));
        int sourceTile = source < 0 ? tile : PixelTile(source);
        return sourceTile == tile ? 0 : FontMask(sourceTile);
    }
    /// <summary>Resolves one foreground atlas pixel from authored differences, source sharing, or calculated font geometry.</summary>
    /// <param name="index">Row-major pixel index in the foreground atlas.</param>
    /// <returns>The selected 4-bpp palette index for that pixel.</returns>
    private byte ForegroundPixel(int index)
    {
        if (foreground.TryGetValue(index, out byte value)) return value;
        int source = WorldMapTileDefinitions.ForegroundSourcePixel(index);
        if (source < 0) return 0;
        if (source != index) return ForegroundPixel(source);
        return WorldMapTileDefinitions.TryForegroundFontPixel(index, fontMask, out value) ? value
            : throw new InvalidDataException("World foreground source pixel is unavailable.");
    }

    /// <summary>Attempts to calculate a background pixel from outlined-digit geometry or the base tile artwork.</summary>
    /// <param name="index">Row-major pixel index in the background atlas.</param>
    /// <param name="value">Receives the calculated 2-bpp index when the pixel has a defined default.</param>
    /// <returns><see langword="true"/> when the pixel can be reconstructed without an authored override.</returns>
    private bool TryBackgroundDefault(int index, out byte value)
    {
        int x = index % WorldMapArtworkFormat.Width, y = index / WorldMapArtworkFormat.Width;
        int tile = y / 8 * WorldMapArtworkFormat.TileColumns + x / 8;
        if (digitFill.TryGetValue(tile, out ulong footprint))
            return WorldMapTileDefinitions.TryOutlinedPixel(tile, x % 8, y % 8, footprint, out value);
        return WorldMapTileDefinitions.TryBackgroundPixel(tile,
            x % 8, y % 8, out value);
    }
}
