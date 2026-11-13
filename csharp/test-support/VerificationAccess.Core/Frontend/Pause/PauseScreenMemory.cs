using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Map;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The memory image the cartridge's pause screen holds (#1404): the map, pause interface, menu OBJ and HUD characters at
/// the words <c>GameState_13</c> uploads them to, and the BG1, BG2 and BG3 tilemaps over them. The screen keeps no such
/// image; verification composes it to compare against native VRAM.
/// </summary>
internal static class PauseScreenMemory
{
    /// <summary>Character images already composed, by artwork (#1532): an artwork's characters never change once read.</summary>
    private static readonly ConditionalWeakTable<AreaMapPresentation, byte[]> composedCharacters = new();

    /// <summary>The character image of <paramref name="artwork"/>, as <c>GameState_13</c> uploads it: a copy the caller owns.</summary>
    [WholeImageFixture("the pause screen's character image")]
    internal static byte[] Characters(AreaMapPresentation artwork) => (byte[])ComposedCharacters(artwork).Clone();

    /// <summary>The shared character image of <paramref name="artwork"/>, composed on first use; callers only read it.</summary>
    private static byte[] ComposedCharacters(AreaMapPresentation artwork) => composedCharacters.GetValue(Ensure.NotNull(artwork), ComposeCharacters);

    /// <summary>Composes the character image of <paramref name="artwork"/>, as <c>GameState_13</c> uploads it.</summary>
    [WholeImageFixture("the pause screen's character image")]
    private static byte[] ComposeCharacters(AreaMapPresentation artwork)
    {
        var image = new SnesVram();
        var pause = new PauseVram(image);
        MapCharacterArtwork characters = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportOf(artwork).Characters;
        characters.Map.LoadTo(pause.MapCharacters);
        characters.Pause.LoadTo(pause.InterfaceCharacters);
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportOf(artwork).Sprites.LoadArtworkTo(pause.ObjectCharacters);
        characters.Hud.LoadTo(image);
        return image.CaptureImage();
    }

    /// <summary>
    /// The memory image <paramref name="menu"/> shows: the characters of <paramref name="artwork"/>, the presentation its
    /// fixture bound it to, with the tilemaps the cartridge lays out for what it shows over them (#1467).
    /// </summary>
    [WholeImageFixture("the pause screen's memory image")]
    internal static byte[] Compose(PauseMenuState menu, AreaMapPresentation artwork)
    {
        PauseScreen screen = menu.screen;
        var image = new SnesVram();
        image.LoadBytes(0, ComposedCharacters(artwork));
        var pause = new PauseVram(image);
        pause.Bg1Tilemap.Write(BackgroundTileWordBytes.Words(menu.ShownBg1Tiles(artwork)));
        pause.Bg2Tilemap.Write(BackgroundTileWordBytes.Words(menu.ShownBg2Tiles(artwork)));
        pause.Bg3Tilemap.Write(BackgroundTileWordBytes.Words(screen.Bg3Tiles(artwork)));
        return image.CaptureImage();
    }

    /// <summary>
    /// Whether <paramref name="left"/> and <paramref name="right"/>, both bound to <paramref name="artwork"/>, show the same
    /// memory image (#1532). <see cref="Compose"/> is a function of the artwork, whose characters and layouts it reads, and
    /// of what the two menus show: the page shown, the area map, the equipment page it shows on the equipment page, the area
    /// and button highlight the frame shows and the retained HUD. Menus that show equal values of each therefore hold the
    /// same image without composing either; only menus that differ in one are composed and compared byte for byte. The
    /// answer is the composed comparison's in every case.
    /// </summary>
    internal static bool SameImage(PauseMenuState left, PauseMenuState right, AreaMapPresentation artwork)
    {
        Ensure.NotNull(left);
        Ensure.NotNull(right);
        return SameShown(left, right) || Compose(left, artwork).AsSpan().SequenceEqual(Compose(right, artwork));
    }

    /// <summary>Whether the two menus show equal values of everything <see cref="Compose"/> reads besides the artwork.</summary>
    private static bool SameShown(PauseMenuState left, PauseMenuState right) =>
        left.ScreenMode == right.ScreenMode
        && left.area == right.area && left.ButtonHighlight == right.ButtonHighlight
        && SameMap(left.ShownMap, right.ShownMap)
        && (left.ScreenMode == PauseScreenMode.Map || SameEquipment(left.EquipmentPage, right.EquipmentPage))
        && SameHud(left.screen.Hud, right.screen.Hud);

    /// <summary>Whether two shown area maps show the same cells, each in the same colour variant.</summary>
    private static bool SameMap(ShownAreaMap left, ShownAreaMap right) =>
        ReferenceEquals(left, right) || left.Cells.AsSpan().SequenceEqual(right.Cells.AsSpan());

    /// <summary>Whether two equipment pages show the same labels, wireframe, reserve display and arrow.</summary>
    private static bool SameEquipment(ShownEquipmentPage left, ShownEquipmentPage right) =>
        ReferenceEquals(left, right) || (left.Labels.AsSpan().SequenceEqual(right.Labels.AsSpan())
            && left.HyperBeam == right.HyperBeam && left.Wireframe == right.Wireframe
            && Equals(left.Reserve, right.Reserve) && left.Arrow == right.Arrow);

    /// <summary>Whether two retained HUDs place the same images at the same cells in the same order.</summary>
    private static bool SameHud(ShownHud left, ShownHud right) =>
        ReferenceEquals(left, right) || left.Images.AsSpan().SequenceEqual(right.Images.AsSpan());

}
