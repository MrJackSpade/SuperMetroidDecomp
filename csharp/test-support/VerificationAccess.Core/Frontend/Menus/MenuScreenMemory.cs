using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The memory image the cartridge's menu screen holds (#1404): the world, map and menu OBJ characters at the words
/// <c>LoadInitialMenuTiles</c> ($81:8DDB) loads them to, and the tilemaps from <see cref="MenuVramLayout.Bg1TilemapWord"/>.
/// The screen keeps no such image; verification composes it to compare against native VRAM.
/// </summary>
internal static class MenuScreenMemory
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
        byte[] image = Characters(screen);
        ushort[] tilemaps = BackgroundTileWordBytes.Words(screen.Tilemaps.All);
        int first = MenuVramLayout.Bg1TilemapWord * sizeof(ushort);
        for (int word = 0; word < tilemaps.Length; word++)
        {
            image[first + word * 2] = LittleEndianBytes.LowByte(tilemaps[word]);
            image[first + word * 2 + 1] = LittleEndianBytes.HighByte(tilemaps[word]);
        }
        return image;
    }

    /// <summary>Reads a menu screen's memory image.</summary>
    extension(MenuScreen self)
    {
        /// <summary>The character image of the screen's bound artwork.</summary>
        internal byte[] Characters => Characters(self);
    }
}
