using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingLogoSpritePresentation"/> members production does not use.</summary>
internal static class EndingLogoSpritePresentationAccess
{
    /// <summary>Exposes <see cref="EndingLogoSpritePresentation"/> state that only verification reads.</summary>
    extension(EndingLogoSpritePresentation self)
    {
        /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
        public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingLogoSpritePresentation), self.frames);
    }
}
