using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// The memory image the cartridge's menu screen holds (#1404): the world, map and menu OBJ characters at the words
/// <c>LoadInitialMenuTiles</c> ($81:8DDB) loads them to, and the tilemaps from <see cref="MenuVramLayout.Bg1TilemapWord"/>.
/// The screen keeps no such image; verification composes it to compare against native VRAM.
/// </summary>
internal static class MenuScreenMemory
{
    /// <summary>The character image of <paramref name="screen"/>'s bound artwork.</summary>
    [WholeImageFixture("the menu's character image")]
    internal static byte[] Characters(MenuScreen screen)
    {
        var image = new SnesVram();
        screen.WorldArtwork.LoadTo(image);
        // The menu's map characters and sprite page are installed artwork, the same for every menu screen (#1463, #1466).
        MapCharacterArtwork.Of(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps).Map
            .LoadTo(new VramRegion(image, MenuVramLayout.MapCharacters));
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps.Sprites
            .LoadArtworkTo(new VramRegion(image, MenuVramLayout.ObjectCharacters));
        return image.CaptureImage();
    }

    /// <summary>
    /// The memory image <paramref name="screen"/> shows: its characters with <paramref name="tilemaps"/>, the cartridge's
    /// four menu screens (BG1, the area map's second screen, BG2, the area background) laid out from what it shows (#1467).
    /// </summary>
    [WholeImageFixture("the menu's 64 KiB memory image")]
    internal static byte[] Compose(MenuScreen screen, ReadOnlySpan<BackgroundTile> tilemaps)
    {
        byte[] image = Characters(screen);
        ushort[] words = BackgroundTileWordBytes.Words(tilemaps);
        int first = MenuVramLayout.Bg1TilemapWord * sizeof(ushort);
        for (int word = 0; word < words.Length; word++)
        {
            image[first + word * 2] = LittleEndianBytes.LowByte(words[word]);
            image[first + word * 2 + 1] = LittleEndianBytes.HighByte(words[word]);
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
