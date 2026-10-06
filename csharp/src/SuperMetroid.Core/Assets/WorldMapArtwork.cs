using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable world-map character atlases with calculated BG3 geometry and independently editable pixels.</summary>
public sealed class WorldMapArtwork
{
    private readonly byte[] foreground;
    // Exact remaining BG3 font/icon contours and selected edge decisions, plus
    // independent supplied differences from the calculated primitive/outline view.
    private readonly Dictionary<int, byte> background;
    private readonly Dictionary<int, ulong> digitFill = new();
    private WorldMapArtwork(byte[] foreground, byte[] background)
    {
        this.foreground = foreground;
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
        byte[] foreground = SnesPlanarTileEncoder.Encode(front.Pixels, front.Width, front.Height, 4);
        // Keep the original import-time range validation and diagnostic, while
        // retaining only the source pixels needed by the calculated BG3 view.
        _ = SnesPlanarTileEncoder.Encode(back.Pixels, back.Width, back.Height, 2);
        return new(foreground, back.Pixels);
    }
    public void LoadTo(SnesVram vram)
    {
        vram.LoadBytes(WorldMapArtworkFormat.ForegroundDestination, foreground);
        var pixels = new byte[WorldMapArtworkFormat.Width * WorldMapArtworkFormat.BackgroundHeight];
        for (int index = 0; index < pixels.Length; index++)
            pixels[index] = background.TryGetValue(index, out byte value) ? value
                : TryBackgroundDefault(index, out byte calculated) ? calculated
                : throw new InvalidDataException("World background source pixel is unavailable.");
        vram.LoadBytes(WorldMapArtworkFormat.BackgroundDestination,
            SnesPlanarTileEncoder.Encode(pixels, WorldMapArtworkFormat.Width, WorldMapArtworkFormat.BackgroundHeight, 2));
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
