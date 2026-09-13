using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable indexed world-map character atlases. Palette indexes remain authored pixels.</summary>
public sealed class WorldMapArtwork
{
    private readonly byte[] foreground, background;
    private WorldMapArtwork(byte[] foreground, byte[] background) { this.foreground = foreground; this.background = background; }
    public static WorldMapArtwork Load(Stream foregroundPng, Stream backgroundPng)
    {
        var front = IndexedPng.Read(foregroundPng, WorldMapArtworkFormat.Width, WorldMapArtworkFormat.ForegroundHeight);
        var back = IndexedPng.Read(backgroundPng, WorldMapArtworkFormat.Width, WorldMapArtworkFormat.BackgroundHeight);
        return new(SnesPlanarTileEncoder.Encode(front.Pixels, front.Width, front.Height, 4),
            SnesPlanarTileEncoder.Encode(back.Pixels, back.Width, back.Height, 2));
    }
    public void LoadTo(SnesVram vram)
    {
        vram.LoadBytes(WorldMapArtworkFormat.ForegroundDestination, foreground);
        vram.LoadBytes(WorldMapArtworkFormat.BackgroundDestination, background);
    }
}
