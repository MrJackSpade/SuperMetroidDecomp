using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// What the pause screen's background characters show (#1392): the map, pause interface and HUD characters it loads, as
/// their pre-coloured tiles under the pause palette.
/// </summary>
[PpuLayoutBoundary("codec: the pause screen's background characters at their native words")]
internal static class PauseScreenTiles
{
    /// <summary>The pause screen's characters under the pause palette.</summary>
    internal static ScreenCharacters Of(InstalledSpriteSheets sheets)
    {
        Ensure.NotNull(sheets);
        return new ScreenCharacters()
            .Show(ScreenTileSheets.Of(sheets, SpriteSheetId.PauseMapTiles), PauseVramLayout.MapCharacters.Offset, 0, MapTileAtlasFormat.CharacterCount)
            .Show(ScreenTileSheets.Of(sheets, SpriteSheetId.PauseInterfaceTiles), PauseVramLayout.InterfaceCharacters.Offset, 0,
                PauseTileAtlasFormat.ByteCount / (ScreenTileSheet.CharacterWordsOf(BgBitDepth.FourBpp) * sizeof(ushort)))
            .Show(ScreenTileSheets.Of(sheets, SpriteSheetId.PauseHudTiles), GameplayVramLayout.HudCharacterUpload.Offset, 0,
                HudTileAtlasFormat.CharacterByteCount / (ScreenTileSheet.CharacterWordsOf(BgBitDepth.TwoBpp) * sizeof(ushort)));
    }
}
