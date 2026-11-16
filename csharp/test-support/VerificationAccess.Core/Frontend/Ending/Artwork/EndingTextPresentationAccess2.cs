using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingTextPresentation"/> members production does not use.</summary>
internal static class EndingTextPresentationAccess2
{
    /// <summary>Exposes <see cref="EndingTextPresentation"/> state that only verification reads.</summary>
    extension(EndingTextPresentation self)
    {
        /// <summary>The characters the ending's typewriter types, which the game reads instead of this presentation (#1467).</summary>
        public EndingTypewriterText Typewriter() => new([.. self.percentage], [.. self.finalMessage]);
    }
}
