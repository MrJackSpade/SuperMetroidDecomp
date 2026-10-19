using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// Composes a menu screen's published artwork and tilemaps into the memory image the backends draw (#1358): the
/// world, map and menu OBJ characters at their native words, and the tilemaps from <see cref="MenuVramLayout.Bg1TilemapWord"/>.
/// </summary>
[PpuLayoutBoundary("codec: the shared menu screen's characters and tilemaps at their native words")]
internal static class MenuScreenImage
{
    /// <summary>The character image of <paramref name="screen"/>'s bound artwork.</summary>
    internal static byte[] Characters(MenuScreen screen)
    {
        var image = new SnesVram();
        screen.WorldArtwork.LoadTo(image);
        screen.MapTiles.LoadTo(new VramRegion(image, MenuVramLayout.MapCharacters));
        screen.Sprites.LoadArtworkTo(new VramRegion(image, MenuVramLayout.ObjectCharacters));
        return image.CaptureImage();
    }

    /// <summary>The memory image <paramref name="screen"/> shows: its characters with its tilemaps over them.</summary>
    internal static byte[] Compose(MenuScreen screen)
    {
        byte[] image = screen.Characters.ToArray();
        ReadOnlySpan<ushort> tilemaps = screen.Tilemaps.All;
        int first = MenuVramLayout.Bg1TilemapWord * sizeof(ushort);
        for (int word = 0; word < tilemaps.Length; word++)
        {
            image[first + word * 2] = SnesLittleEndian.LowByte(tilemaps[word]);
            image[first + word * 2 + 1] = SnesLittleEndian.HighByte(tilemaps[word]);
        }
        return image;
    }

    /// <summary>The frame <paramref name="screen"/> shows: its composed image, palette and sprite entries.</summary>
    internal static PpuMemorySnapshot Capture(MenuScreen screen) =>
        new(Compose(screen), screen.Palette.Colors, screen.Objects.Draws);
}
