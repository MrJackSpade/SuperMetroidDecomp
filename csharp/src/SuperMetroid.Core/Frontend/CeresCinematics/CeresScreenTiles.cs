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

    /// <summary>Bytes of one 4-bpp character.</summary>
    private static int CharacterBytes => ScreenTileSheet.CharacterWordsOf(BgBitDepth.FourBpp) * sizeof(ushort);
}
