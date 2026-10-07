using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable world-map character atlases with calculated geometry and independently editable pixels.</summary>
public sealed class WorldMapArtwork
{
    // Reviewed drawings, letter contours, ink roles and mechanical shading;
    // calculated geometry and independent supplied differences stay separate.
    private readonly Dictionary<int, byte> foreground;
    private readonly Dictionary<int, ulong> fontFill = new();
    // Exact remaining BG3 font/icon contours and selected edge decisions, plus
    // independent supplied differences from the calculated primitive/outline view.
    private readonly Dictionary<int, byte> background;
    private readonly Dictionary<int, ulong> digitFill = new();
    // One delegate for the per-pixel font derivation, rather than one per pixel.
    private readonly Func<int, ulong> fontMask;
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

    private static int TilePixel(int tile, int x, int y) =>
        (tile / WorldMapArtworkFormat.TileColumns * 8 + y) * WorldMapArtworkFormat.Width +
        tile % WorldMapArtworkFormat.TileColumns * 8 + x;
    private static int PixelTile(int index) => index / WorldMapArtworkFormat.Width / 8 *
        WorldMapArtworkFormat.TileColumns + index % WorldMapArtworkFormat.Width / 8;
    private ulong FontMask(int tile)
    {
        if (fontFill.TryGetValue(tile, out ulong mask)) return mask;
        int source = WorldMapTileDefinitions.ForegroundSourcePixel(TilePixel(tile, 0, 0));
        int sourceTile = source < 0 ? tile : PixelTile(source);
        return sourceTile == tile ? 0 : FontMask(sourceTile);
    }
    private byte ForegroundPixel(int index)
    {
        if (foreground.TryGetValue(index, out byte value)) return value;
        int source = WorldMapTileDefinitions.ForegroundSourcePixel(index);
        if (source < 0) return 0;
        if (source != index) return ForegroundPixel(source);
        return WorldMapTileDefinitions.TryForegroundFontPixel(index, fontMask, out value) ? value
            : throw new InvalidDataException("World foreground source pixel is unavailable.");
    }

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
