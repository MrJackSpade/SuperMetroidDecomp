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
    internal static ScreenLayerPictures Flight(InstalledSpriteSheets sheets) => new ScreenTileSet(BgBitDepth.FourBpp)
        .Place(ScreenTileSheets.Of(Ensure.NotNull(sheets), SpriteSheetId.CeresFlightObjectTiles), CeresTileSetDefinitions.ObjectFirstTile, 0,
            CeresFlightArtworkFormat.ObjectByteCount / CharacterBytes)
        .Pictures();

    /// <summary>The characters the destruction's <paramref name="uploads"/> leave over the OBJ characters, in the order it made them.</summary>
    internal static ScreenLayerPictures Destruction(InstalledSpriteSheets sheets, IEnumerable<CeresDestructionUpload> uploads)
    {
        Ensure.NotNull(sheets);
        var tiles = new ScreenTileSet(BgBitDepth.FourBpp);
        foreach (CeresDestructionUpload upload in Ensure.NotNull(uploads))
        {
            if (upload == CeresDestructionUpload.CeresScene)
            {
                tiles.Place(ScreenTileSheets.Of(sheets, SpriteSheetId.CeresFlightObjectTiles), CeresTileSetDefinitions.ObjectFirstTile, 0,
                    CeresFlightArtworkFormat.ObjectByteCount / CharacterBytes);
                tiles.Place(ScreenTileSheets.Of(sheets, SpriteSheetId.CeresIntroObjectTiles), CeresTileSetDefinitions.ObjectFirstTile, 0,
                    CeresDestructionSceneDefinitions.SharedObjectCharacterBytes / CharacterBytes);
            }
            else if (upload == CeresDestructionUpload.ZebesReveal)
                tiles.Place(ScreenTileSheets.Of(sheets, SpriteSheetId.ZebesRevealTiles), CeresTileSetDefinitions.ObjectFirstTile, 0,
                    CeresDestructionArtworkFormat.ZebesCharacterByteCount / CharacterBytes);
        }
        return tiles.Pictures();
    }

    /// <summary>The Ceres flight's Mode 7 view: its Mode 7 characters.</summary>
    internal static FloorTilePictures FlightFloor(InstalledSpriteSheets sheets) => new FloorCharacters()
        .Show(ScreenTileSheets.Floor(Ensure.NotNull(sheets), SpriteSheetId.CeresFlightMode7Tiles), 0, 0,
            CeresFlightArtworkFormat.FloorByteCount / Mode7CharacterSheet.TileBytes)
        .Pictures();

    /// <summary>The Mode 7 characters the destruction's <paramref name="uploads"/> leave: the flight's, once the Ceres scene loads them.</summary>
    internal static FloorTilePictures DestructionFloor(InstalledSpriteSheets sheets, IEnumerable<CeresDestructionUpload> uploads)
    {
        Ensure.NotNull(sheets);
        var tiles = new FloorCharacters();
        if (Ensure.NotNull(uploads).Contains(CeresDestructionUpload.CeresScene))
            tiles.Show(ScreenTileSheets.Floor(sheets, SpriteSheetId.CeresFlightMode7Tiles), 0, 0,
                CeresFlightArtworkFormat.FloorByteCount / Mode7CharacterSheet.TileBytes);
        return tiles.Pictures();
    }

    /// <summary>Bytes of one 4-bpp character.</summary>
    private static int CharacterBytes => ScreenTileSheet.CharacterWordsOf(BgBitDepth.FourBpp) * sizeof(ushort);
}
