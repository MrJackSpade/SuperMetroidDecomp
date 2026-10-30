using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// What the Ceres cinematics' Mode 1 backgrounds show (#1392): the characters the flight and the destruction load over
/// the OBJ characters, which their caption and the Zebes reveal read, as their pre-coloured tiles under the flight's
/// palette.
/// </summary>
internal static class CeresScreenTiles
{
    /// <summary>The Ceres flight's characters: its object characters, which its caption shows.</summary>
    internal static ScreenLayerPictures Flight(InstalledSpriteSheets sheets) => new ScreenTileSet(TileInkDepth.SixteenColours)
        .Place(ScreenTileSheets.Of(Ensure.NotNull(sheets), SpriteSheetId.CeresFlightObjectTiles), CeresTileSetDefinitions.ObjectFirstTile, 0,
            CeresFlightArtworkFormat.ObjectByteCount / CharacterBytes)
        .Pictures();

    /// <summary>
    /// The characters the destruction leaves over the OBJ characters: the station's, then, once the Zebes reveal has
    /// loaded, the reveal's over them.
    /// </summary>
    internal static ScreenLayerPictures Destruction(InstalledSpriteSheets sheets, bool zebesRevealed)
    {
        Ensure.NotNull(sheets);
        var tiles = new ScreenTileSet(TileInkDepth.SixteenColours);
        tiles.Place(ScreenTileSheets.Of(sheets, SpriteSheetId.CeresFlightObjectTiles), CeresTileSetDefinitions.ObjectFirstTile, 0,
            CeresFlightArtworkFormat.ObjectByteCount / CharacterBytes);
        tiles.Place(ScreenTileSheets.Of(sheets, SpriteSheetId.CeresIntroObjectTiles), CeresTileSetDefinitions.ObjectFirstTile, 0,
            CeresDestructionSceneDefinitions.SharedObjectCharacterBytes / CharacterBytes);
        if (zebesRevealed)
            tiles.Place(ScreenTileSheets.Of(sheets, SpriteSheetId.ZebesRevealTiles), CeresTileSetDefinitions.ObjectFirstTile, 0,
                CeresDestructionArtworkFormat.ZebesCharacterByteCount / CharacterBytes);
        return tiles.Pictures();
    }

    /// <summary>Bytes of one 4-bpp character.</summary>
    private static int CharacterBytes => ScreenTileSheet.CharacterWordsOf(TileInkDepth.SixteenColours) * sizeof(ushort);
}
