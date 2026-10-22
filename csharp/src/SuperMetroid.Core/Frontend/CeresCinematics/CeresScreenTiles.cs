using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// What the Ceres cinematics' Mode 1 backgrounds show (#1392): the characters the flight and the destruction load over
/// the OBJ characters, which their caption and the Zebes reveal read, as their pre-coloured tiles under the flight's
/// palette.
/// </summary>
[PpuLayoutBoundary("codec: the Ceres cinematics' OBJ character loads at their native words")]
internal static class CeresScreenTiles
{
    /// <summary>The Ceres flight's characters: its object characters, which its caption shows.</summary>
    internal static ScreenCharacters Flight(InstalledSpriteSheets sheets) => new ScreenCharacters()
        .Show(ScreenTileSheets.Of(Ensure.NotNull(sheets), SpriteSheetId.CeresFlightObjectTiles), CeresVramLayout.ObjectCharacters.Offset, 0,
            CeresFlightRomData.Vram.ObjectCharacterByteCount / CharacterBytes);

    /// <summary>The characters the destruction's <paramref name="uploads"/> leave over the OBJ characters, in the order it made them.</summary>
    internal static ScreenCharacters Destruction(InstalledSpriteSheets sheets, IEnumerable<CeresDestructionUpload> uploads)
    {
        Ensure.NotNull(sheets);
        var tiles = new ScreenCharacters();
        foreach (CeresDestructionUpload upload in Ensure.NotNull(uploads))
        {
            if (upload == CeresDestructionUpload.CeresScene)
            {
                tiles.Show(ScreenTileSheets.Of(sheets, SpriteSheetId.CeresFlightObjectTiles), CeresVramLayout.ObjectCharacters.Offset, 0,
                    CeresDestructionRomData.Vram.Mode7CharacterBytes / CharacterBytes);
                tiles.Show(ScreenTileSheets.Of(sheets, SpriteSheetId.CeresIntroObjectTiles), CeresVramLayout.ObjectCharacters.Offset, 0,
                    CeresDestructionRomData.Vram.SharedObjectCharacterBytes / CharacterBytes);
            }
            else if (upload == CeresDestructionUpload.ZebesReveal)
                tiles.Show(ScreenTileSheets.Of(sheets, SpriteSheetId.ZebesRevealTiles), CeresVramLayout.ObjectCharacters.Offset, 0,
                    CeresDestructionRomData.Vram.Mode7CharacterBytes / CharacterBytes);
        }
        return tiles;
    }

    /// <summary>The Ceres flight's Mode 7 view: its Mode 7 characters.</summary>
    internal static Mode7TilePictures FlightMode7(InstalledSpriteSheets sheets) => new Mode7Characters()
        .Show(ScreenTileSheets.Mode7(Ensure.NotNull(sheets), SpriteSheetId.CeresFlightMode7Tiles), 0, 0,
            CeresFlightRomData.Vram.Mode7CharacterByteCount / Mode7CharacterSheet.TileBytes)
        .Pictures();

    /// <summary>The Mode 7 characters the destruction's <paramref name="uploads"/> leave: the flight's, once the Ceres scene loads them.</summary>
    internal static Mode7TilePictures DestructionMode7(InstalledSpriteSheets sheets, IEnumerable<CeresDestructionUpload> uploads)
    {
        Ensure.NotNull(sheets);
        var tiles = new Mode7Characters();
        if (Ensure.NotNull(uploads).Contains(CeresDestructionUpload.CeresScene))
            tiles.Show(ScreenTileSheets.Mode7(sheets, SpriteSheetId.CeresFlightMode7Tiles), 0, 0,
                CeresDestructionRomData.Vram.Mode7CharacterBytes / Mode7CharacterSheet.TileBytes);
        return tiles.Pictures();
    }

    /// <summary>Bytes of one 4-bpp character.</summary>
    private static int CharacterBytes => ScreenTileSheet.CharacterWordsOf(BgBitDepth.FourBpp) * sizeof(ushort);
}
