using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The OBJ characters the Ceres destruction loads (#1404, #1467): the Ceres flight's object characters with the shared
/// cinematic sheet over their start, then, once the Zebes reveal loads, the reveal's characters over them. The scene's
/// sprite sheets are composed over this page.
/// </summary>
internal static class CeresDestructionObjectPage
{
    /// <summary>The page's characters, with the Zebes reveal's loaded when <paramref name="zebesRevealed"/>.</summary>
    internal static byte[] Characters(IntroCinematicArtwork artwork, bool zebesRevealed)
    {
        var page = new byte[CeresFlightArtworkFormat.ObjectByteCount];
        CeresFlightCharacters.Of(artwork.CeresFlight).ObjectCharacters.Span[..CeresFlightArtworkFormat.ObjectByteCount].CopyTo(page);
        // The station's setup loads the standard cinematic sheet over the start of the flight's object characters.
        IntroCinematicSourceArtwork.Of(artwork).IntroObjectCharacters.Characters.Span[..CeresDestructionSceneDefinitions.SharedObjectCharacterBytes].CopyTo(page);
        if (zebesRevealed)
            artwork.CeresDestruction.ZebesCharacters.Characters.Span[..CeresDestructionArtworkFormat.ZebesCharacterByteCount].CopyTo(page);
        return page;
    }

    /// <summary>The page as the scene's sprite artwork, with the Zebes reveal's characters loaded when <paramref name="zebesRevealed"/>.</summary>
    internal static SpriteArtwork Of(IntroCinematicArtwork artwork, bool zebesRevealed) =>
        new(new SpriteArtworkId(SpriteArtworkKind.CeresDestructionObjects), Characters(artwork, zebesRevealed));
}
