using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingCompletionTextSpritePresentation"/> members production does not use.</summary>
internal static class EndingCompletionTextSpritePresentationAccess
{
    /// <summary>Exposes <see cref="EndingCompletionTextSpritePresentation"/> state that only verification reads.</summary>
    extension(EndingCompletionTextSpritePresentation self)
    {
        /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
        public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingCompletionTextSpritePresentation), self.frames);
    }
}
