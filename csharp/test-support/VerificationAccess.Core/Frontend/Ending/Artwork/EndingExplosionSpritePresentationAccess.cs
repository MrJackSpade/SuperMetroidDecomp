using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingExplosionSpritePresentation"/> members production does not use.</summary>
internal static class EndingExplosionSpritePresentationAccess
{
    /// <summary>Exposes <see cref="EndingExplosionSpritePresentation"/> state that only verification reads.</summary>
    extension(EndingExplosionSpritePresentation self)
    {
        /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
        public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingExplosionSpritePresentation), self.frames);
    }
}
