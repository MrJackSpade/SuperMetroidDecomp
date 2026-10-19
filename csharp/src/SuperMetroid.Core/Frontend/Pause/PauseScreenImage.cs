using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Composes the pause screen's published artwork and tilemaps into the memory image the backends draw (#1358): the map,
/// pause interface, menu OBJ and HUD characters at their native words, and the BG1, BG2 and BG3 tilemaps over them.
/// </summary>
[PpuLayoutBoundary("codec: the pause screen's characters and tilemaps at their native words")]
internal static class PauseScreenImage
{
    /// <summary>The character image of <paramref name="artwork"/>, as <c>GameState_13</c> uploads it.</summary>
    internal static byte[] Characters(AreaMapPresentationCatalog artwork)
    {
        var image = new SnesVram();
        var pause = new PauseVram(image);
        artwork.Tiles.LoadTo(pause.MapCharacters);
        artwork.PauseTiles.LoadTo(pause.InterfaceCharacters);
        artwork.Sprites.LoadArtworkTo(pause.ObjectCharacters);
        artwork.HudTiles.LoadTo(image);
        return image.CaptureImage();
    }

    /// <summary>The memory image <paramref name="screen"/> shows: its characters with its tilemaps over them.</summary>
    internal static byte[] Compose(PauseScreen screen)
    {
        var image = new SnesVram();
        image.LoadBytes(0, screen.Characters);
        var pause = new PauseVram(image);
        pause.Bg1Tilemap.Write(screen.Tilemaps.Bg1);
        pause.Bg2Tilemap.Write(screen.Tilemaps.Bg2);
        pause.Bg3Tilemap.Write(screen.Tilemaps.Bg3);
        return image.CaptureImage();
    }

    /// <summary>The frame <paramref name="screen"/> shows: its composed image, palette and sprite entries.</summary>
    internal static PpuMemorySnapshot Capture(PauseScreen screen) =>
        new(Compose(screen), screen.Palette.Colors, screen.Objects.Draws);
}
