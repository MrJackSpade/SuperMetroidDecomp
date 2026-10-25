using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed world-map character atlases as the import pipeline reads them (#1409): calculated geometry and
/// independently editable pixels, compiled to the planar characters the world-map tile sheets compose from.
/// </summary>
public sealed class WorldMapCharacters : WorldMapArtwork
{
    /// <summary>Foreground pixels differing from reviewed font contours and calculated shared geometry.</summary>
    internal readonly Dictionary<int, byte> foreground;
    /// <summary>Independently supplied foreground letter-face masks.</summary>
    internal readonly Dictionary<int, ulong> fontFill = new();
    /// <summary>BG3 font and icon pixels differing from calculated primitives and outlines.</summary>
    internal readonly Dictionary<int, byte> background;
    /// <summary>Independently supplied fill footprints for outlined map digits.</summary>
    internal readonly Dictionary<int, ulong> digitFill = new();
    /// <summary>Reusable font-mask lookup passed to per-pixel contour derivation.</summary>
    private readonly Func<int, ulong> fontMask;
    /// <summary>Separates selected PNG pixels into calculated drawing rules and independent authored differences.</summary>
    /// <param name="foreground">The foreground atlas's row-major pixel indices.</param>
    /// <param name="background">The background atlas's row-major pixel indices.</param>
    private WorldMapCharacters(byte[] foreground, byte[] background)
        : base(foreground.Length / PixelsPerCharacter, background.Length / PixelsPerCharacter)
    {
        fontMask = FontMask;
        var masks = new ulong[WorldMapArtworkFormat.ForegroundTileCount];
        for (int tile = 0; tile < masks.Length; tile++)
        {
            if (!WorldMapTileDefinitions.IsForegroundFontTile(tile)) continue;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                if (foreground[TilePixel(tile, new(checked((ushort)x), checked((ushort)y)))] == WorldMapTileDefinitions.ForegroundFontFace)
                    masks[tile] |= 1UL << (y * 8 + x);
        }
        for (int tile = 0; tile < masks.Length; tile++)
        {
            if (!WorldMapTileDefinitions.IsForegroundFontTile(tile)) continue;
            int source = WorldMapTileDefinitions.ForegroundSourcePixel(TilePixel(tile, new(0, 0)));
            int sourceTile = source < 0 ? tile : PixelTile(source);
            ulong expected = sourceTile == tile ? 0 : masks[sourceTile];
            if (masks[tile] != expected) fontFill.Add(tile, masks[tile]);
        }
        this.foreground = Enumerable.Range(0, foreground.Length).Where(index =>
        {
            int source = WorldMapTileDefinitions.ForegroundSourcePixel(index);
            if (source != index) return foreground[index] != (source < 0 ? 0 : foreground[source]);
            return !WorldMapTileDefinitions.TryForegroundFontPixel(index, fontMask, out int value) || foreground[index] != value;
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
    public static WorldMapCharacters Read(Stream foregroundPng, Stream backgroundPng)
    {
        var front = IndexedPng.Read(foregroundPng, new(WorldMapArtworkFormat.Width, WorldMapArtworkFormat.ForegroundHeight));
        var back = IndexedPng.Read(backgroundPng, new(WorldMapArtworkFormat.Width, WorldMapArtworkFormat.BackgroundHeight));
        _ = SnesPlanarTileEncoder.Encode(front.Pixels, front.Size, 4);
        // Keep the original import-time range validation and diagnostic, while
        // retaining only the source pixels needed by the calculated character views.
        _ = SnesPlanarTileEncoder.Encode(back.Pixels, back.Size, 2);
        return new(front.Pixels, back.Pixels);
    }

    /// <summary>The planar 4-bpp world foreground characters.</summary>
    public byte[] ForegroundCharacters()
    {
        var front = new byte[WorldMapArtworkFormat.Width * WorldMapArtworkFormat.ForegroundHeight];
        for (int index = 0; index < front.Length; index++)
            front[index] = ForegroundPixel(index);
        return SnesPlanarTileEncoder.Encode(front, new(WorldMapArtworkFormat.Width, WorldMapArtworkFormat.ForegroundHeight), 4);
    }

    /// <summary>The planar 2-bpp world background characters.</summary>
    /// <exception cref="InvalidDataException">A background pixel is unavailable.</exception>
    public byte[] BackgroundCharacters()
    {
        var pixels = new byte[WorldMapArtworkFormat.Width * WorldMapArtworkFormat.BackgroundHeight];
        for (int index = 0; index < pixels.Length; index++)
            pixels[index] = background.TryGetValue(index, out byte value) ? value
                : TryBackgroundDefault(index, out byte calculated) ? calculated
                : throw new InvalidDataException("World background source pixel is unavailable.");
        return SnesPlanarTileEncoder.Encode(pixels, new(WorldMapArtworkFormat.Width, WorldMapArtworkFormat.BackgroundHeight), 2);
    }

    /// <summary>The characters <paramref name="artwork"/> was read with.</summary>
    /// <exception cref="InvalidDataException">The artwork was not read from installed artwork.</exception>
    public static WorldMapCharacters Of(WorldMapArtwork artwork) => artwork as WorldMapCharacters
        ?? throw new InvalidDataException("The world-map characters were not read from installed artwork.");

    /// <summary>The pixels of one 8-by-8 character.</summary>
    private const int PixelsPerCharacter = 64;

    /// <summary>Converts a tile-local pixel to a row-major atlas pixel index.</summary>
    private static int TilePixel(int tile, Position16<CharacterPixels> point) =>
        (tile / WorldMapArtworkFormat.TileColumns * 8 + point.Y) * WorldMapArtworkFormat.Width +
        tile % WorldMapArtworkFormat.TileColumns * 8 + point.X;
    /// <summary>Finds the atlas tile containing a row-major pixel index.</summary>
    private static int PixelTile(int index) => index / WorldMapArtworkFormat.Width / 8 *
        WorldMapArtworkFormat.TileColumns + index % WorldMapArtworkFormat.Width / 8;
    /// <summary>Resolves a letter-face mask from an authored fill or its calculated source tile.</summary>
    private ulong FontMask(int tile)
    {
        if (fontFill.TryGetValue(tile, out ulong mask)) return mask;
        int source = WorldMapTileDefinitions.ForegroundSourcePixel(TilePixel(tile, new(0, 0)));
        int sourceTile = source < 0 ? tile : PixelTile(source);
        return sourceTile == tile ? 0 : FontMask(sourceTile);
    }
    /// <summary>Restores a foreground pixel from an authored edit, shared source, or calculated font contour.</summary>
    private byte ForegroundPixel(int index)
    {
        if (foreground.TryGetValue(index, out byte value)) return value;
        int source = WorldMapTileDefinitions.ForegroundSourcePixel(index);
        if (source < 0) return 0;
        if (source != index) return ForegroundPixel(source);
        return WorldMapTileDefinitions.TryForegroundFontPixel(index, fontMask, out int pen) ? (byte)pen
            : throw new InvalidDataException("World foreground source pixel is unavailable.");
    }

    /// <summary>Calculates a BG3 font, icon, or outlined-digit pixel when no authored difference exists.</summary>
    private bool TryBackgroundDefault(int index, out byte value)
    {
        int x = index % WorldMapArtworkFormat.Width, y = index / WorldMapArtworkFormat.Width;
        int tile = y / 8 * WorldMapArtworkFormat.TileColumns + x / 8;
        int pen;
        bool found = digitFill.TryGetValue(tile, out ulong footprint)
            ? WorldMapTileDefinitions.TryOutlinedPixel(tile, new(checked((ushort)(x % 8)), checked((ushort)(y % 8))), footprint, out pen)
            : WorldMapTileDefinitions.TryBackgroundPixel(tile, new(checked((ushort)(x % 8)), checked((ushort)(y % 8))), out pen);
        value = (byte)pen;
        return found;
    }
}
