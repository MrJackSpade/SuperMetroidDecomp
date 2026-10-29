using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map.Artwork;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// The memory image the cartridge's pause screen holds (#1404): the map, pause interface, menu OBJ and HUD characters at
/// the words <c>GameState_13</c> uploads them to, and the BG1, BG2 and BG3 tilemaps over them. The screen keeps no such
/// image; verification composes it to compare against native VRAM.
/// </summary>
internal static class PauseScreenMemory
{
    /// <summary>The character image of <paramref name="artwork"/>, as <c>GameState_13</c> uploads it.</summary>
    [WholeImageFixture("the pause screen's character image")]
    internal static byte[] Characters(AreaMapPresentation artwork)
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
    [WholeImageFixture("the pause screen's memory image")]
    internal static byte[] Compose(PauseScreen screen)
    {
        var image = new SnesVram();
        image.LoadBytes(0, Characters(screen.Artwork));
        var pause = new PauseVram(image);
        pause.Bg1Tilemap.Write(BackgroundTileWordBytes.Words(screen.Tilemaps.Bg1));
        pause.Bg2Tilemap.Write(BackgroundTileWordBytes.Words(screen.Tilemaps.Bg2));
        pause.Bg3Tilemap.Write(BackgroundTileWordBytes.Words(screen.Tilemaps.Bg3));
        return image.CaptureImage();
    }

    /// <summary>Reads a pause screen's memory image.</summary>
    extension(PauseScreen self)
    {
        /// <summary>The character image of the screen's bound artwork.</summary>
        internal byte[] Characters => Characters(self.Artwork);
    }
}
