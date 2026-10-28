using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The OBJ characters the Ceres destruction's uploads leave (#1404): the Ceres flight's object characters with the shared
/// cinematic sheet over their start once the Ceres scene loads, then the Zebes reveal's characters. The scene's sprite
/// sheets are composed over this page.
/// </summary>
internal static class CeresDestructionObjectPage
{
    /// <summary>The page's characters once <paramref name="uploads"/> have been made, in order.</summary>
    internal static byte[] Characters(IntroCinematicArtwork artwork, IReadOnlyList<CeresDestructionUpload> uploads)
    {
        var page = new byte[CeresFlightArtworkFormat.ObjectByteCount];
        foreach (CeresDestructionUpload upload in uploads)
        {
            switch (upload)
            {
                case CeresDestructionUpload.CeresScene:
                    CeresFlightCharacters.Of(artwork.CeresFlight).ObjectCharacters.Span[..CeresFlightArtworkFormat.ObjectByteCount].CopyTo(page);
                    // The final C11B DMA overwrites the first $1A00 OBJ bytes with the standard cinematic sheet at $9A:D200.
                    IntroCinematicSourceArtwork.Of(artwork).IntroObjectCharacters.Characters.Span[..CeresDestructionSceneDefinitions.SharedObjectCharacterBytes].CopyTo(page);
                    break;
                case CeresDestructionUpload.ZebesReveal:
                    artwork.CeresDestruction.ZebesCharacters.Characters.Span[..CeresDestructionArtworkFormat.ZebesCharacterByteCount].CopyTo(page);
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
    internal static SpriteArtwork Of(IntroCinematicArtwork artwork, IReadOnlyList<CeresDestructionUpload> uploads) =>
        new(new SpriteArtworkId(SpriteArtworkKind.CeresDestructionObjects), Characters(artwork, uploads));
}
