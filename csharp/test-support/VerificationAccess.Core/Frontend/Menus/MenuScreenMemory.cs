using System.Runtime.CompilerServices;
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
    /// <summary>Character images already composed, by world-map characters (#1532): installed artwork never changes once read.</summary>
    private static readonly ConditionalWeakTable<WorldMapCharacters, byte[]> composedCharacters = new();

    /// <summary>
    /// The character image a menu shows from <paramref name="maps"/>: its world-map characters, which the menu's installed
    /// sheets are composed from (#1583), with the installation's map characters and sprite page; a copy the caller owns.
    /// </summary>
    [WholeImageFixture("the menu's character image")]
    internal static byte[] Characters(AreaMapPresentation maps) =>
        (byte[])composedCharacters.GetValue(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportOf(maps).WorldCharacters, ComposeCharacters).Clone();

    /// <summary>
    /// Composes the character image a menu showing <paramref name="worldCharacters"/> holds: its world characters with the
    /// installed map characters and sprite page.
    /// </summary>
    [WholeImageFixture("the menu's character image")]
    private static byte[] ComposeCharacters(WorldMapCharacters worldCharacters)
    {
        var image = new SnesVram();
        worldCharacters.LoadTo(image);
        // The menu's map characters and sprite page are installed artwork, the same for every menu screen (#1463, #1466).
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportMaps.Characters.Map
            .LoadTo(new VramRegion(image, MenuVramLayout.MapCharacters));
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportMaps.Sprites
            .LoadArtworkTo(new VramRegion(image, MenuVramLayout.ObjectCharacters));
        return image.CaptureImage();
    }

    /// <summary>
    /// The memory image a menu shows from <paramref name="maps"/>: its characters with <paramref name="tilemaps"/>, the
    /// cartridge's four menu screens (BG1, the area map's second screen, BG2, the area background) laid out from what it
    /// shows (#1467).
    /// </summary>
    [WholeImageFixture("the menu's 64 KiB memory image")]
    internal static byte[] Compose(AreaMapPresentation maps, ReadOnlySpan<BackgroundTile> tilemaps)
    {
        byte[] image = Characters(maps);
        ushort[] words = BackgroundTileWordBytes.Words(tilemaps);
        int first = MenuVramLayout.Bg1TilemapWord * sizeof(ushort);
        for (int word = 0; word < words.Length; word++)
        {
            image[first + word * 2] = LittleEndianBytes.LowByte(words[word]);
            image[first + word * 2 + 1] = LittleEndianBytes.HighByte(words[word]);
        }
        return image;
    }

}
