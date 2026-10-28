using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>
/// The OBJ characters the intro's uploads leave: the intro sheet with the decompressed cinematic sheet over its tail. The
/// intro's cinematic and projectile sheets are composed over this page.
/// </summary>
internal static class IntroObjectPage
{
    /// <summary>The intro OBJ page of <paramref name="intro"/>.</summary>
    internal static SpriteArtwork Of(IntroCinematicArtwork intro)
    {
        IntroCinematicSourceArtwork sources = IntroCinematicSourceArtwork.Of(Ensure.NotNull(intro));
        var id = new SpriteArtworkId(SpriteArtworkKind.IntroObjects);
        return ObjCharacterPage.With(id, ObjCharacterPage.Create(id, sources.IntroObjectCharacters.Characters.Span),
            sources.CinematicObjectCharacters.Characters.Span, ProjectileObjectPageDefinitions.IntroCinematicObjectsFirstCharacter);
    }
}
