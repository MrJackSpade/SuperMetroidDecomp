using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingCloudSpritePresentation"/> members production does not use.</summary>
internal static class EndingCloudSpritePresentationAccess
{
    /// <summary>Exposes <see cref="EndingCloudSpritePresentation"/> state that only verification reads.</summary>
    extension(EndingCloudSpritePresentation self)
    {
        /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
        public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingCloudSpritePresentation), self.frames);
    }
}
