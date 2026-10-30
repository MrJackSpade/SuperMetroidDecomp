using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// A map screen as cartridge-transfer checks read it (#1358, #1467): the image the cartridge's PPU memory would hold for what
/// it shows, its pages laid out from the import's sources of the pictures it places.
/// </summary>
internal static class MenuScreenFixtures
{
    /// <summary>Cells of one menu screen.</summary>
    private const int ScreenCells = 1024;

    /// <summary>A VRAM holding the world map <paramref name="graphics"/> shows, its pages read from <paramref name="maps"/>.</summary>
    [WholeImageFixture("the menu's 64 KiB VRAM image")]
    internal static SnesVram Vram(FileSelectAreaMapGraphics graphics, AreaMapPresentation maps)
    {
        Ensure.NotNull(graphics);
        MapScreenPresentation screens = InstalledAreaMapPresentation.ScreensOf(maps);
        var tiles = new BackgroundTile[4 * ScreenCells];
        screens.Page(MapScreenDefinitions.WorldForeground).CopyTo(tiles);
        screens.Page(MapScreenDefinitions.WorldBackground(AreaIds.At(graphics.SelectedArea))).CopyTo(tiles.AsSpan(3 * ScreenCells));
        return Load(MenuScreenMemory.Compose(graphics.screen, tiles));
    }

    /// <summary>A VRAM holding the room map <paramref name="graphics"/> shows, its frame read from <paramref name="maps"/>.</summary>
    [WholeImageFixture("the menu's 64 KiB VRAM image")]
    internal static SnesVram Vram(FileSelectRoomMapGraphics graphics, AreaMapPresentation maps)
    {
        Ensure.NotNull(graphics);
        var tiles = new BackgroundTile[4 * ScreenCells];
        ShownAreaMapTiles.Of(graphics.AreaMap).CopyTo(tiles, 0);
        InstalledAreaMapPresentation.ScreensOf(maps).Page(MapScreenDefinitions.RoomFrame(graphics.icons.MapArea)).CopyTo(tiles.AsSpan(2 * ScreenCells));
        return Load(MenuScreenMemory.Compose(graphics.screen, tiles));
    }

    /// <summary>A VRAM holding <paramref name="image"/>.</summary>
    private static SnesVram Load(byte[] image)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, image);
        return vram;
    }
}
