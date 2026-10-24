using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The OBJ characters the Ceres destruction's uploads leave (#1404): the Ceres flight's object characters with the shared
/// cinematic sheet over their start once the Ceres scene loads, then the Zebes reveal's characters. The scene's sprite
/// sheets are composed over this page.
/// </summary>
internal static class CeresDestructionObjectPage
{
    /// <summary>The page's characters once <paramref name="uploads"/> have been made, in order.</summary>
    internal static byte[] Characters(IntroCinematicArtworkCatalog artwork, IReadOnlyList<CeresDestructionUpload> uploads)
    {
        var page = new byte[CeresDestructionRomData.Transfers.FloorCharacterBytes];
        foreach (CeresDestructionUpload upload in uploads)
        {
            switch (upload)
            {
                case CeresDestructionUpload.CeresScene:
                    artwork.CeresFlight.ObjectCharacters.Span[..CeresDestructionRomData.Transfers.FloorCharacterBytes].CopyTo(page);
                    // The final C11B DMA overwrites the first $1A00 OBJ bytes with the standard cinematic sheet at $9A:D200.
                    artwork.IntroObjectCharacters.Characters.Span[..CeresDestructionRomData.Transfers.SharedObjectCharacterBytes].CopyTo(page);
                    break;
                case CeresDestructionUpload.ZebesReveal:
                    artwork.CeresDestruction.ZebesCharacters.Characters.Span[..CeresDestructionRomData.Transfers.FloorCharacterBytes].CopyTo(page);
                    break;
                case CeresDestructionUpload.FinalExplosionMaps:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(uploads), upload, "Undefined Ceres destruction upload.");
            }
        }
        return page;
    }

    /// <summary>The page as the scene's sprite artwork once <paramref name="uploads"/> have been made.</summary>
    internal static SpriteArtwork Of(IntroCinematicArtworkCatalog artwork, IReadOnlyList<CeresDestructionUpload> uploads) =>
        new(new SpriteArtworkId(SpriteArtworkKind.CeresDestructionObjects), Characters(artwork, uploads));
}
