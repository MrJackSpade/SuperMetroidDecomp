using SuperMetroid.Core.Frontend.Intro;
using System.Text.Json;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingRewardSpritePresentation"/> members production does not use.</summary>
internal static class EndingRewardSpritePresentationAccess
{
    /// <summary>Exposes <see cref="EndingRewardSpritePresentation"/> state that only verification reads.</summary>
    extension(EndingRewardSpritePresentation self)
    {
        /// <summary>Canonical identity of the selected decoded visual frames, not JSON formatting.</summary>
        public string ContentIdentity => CinematicSpriteArtwork.ContentIdentity(nameof(EndingRewardSpritePresentation), self.frames);
    }
}
