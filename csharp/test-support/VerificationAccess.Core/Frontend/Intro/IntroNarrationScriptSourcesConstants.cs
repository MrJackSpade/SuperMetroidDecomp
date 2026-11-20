using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Cartridge values and helpers of <see cref="IntroNarrationScriptSources"/> that only verification reads.</summary>
internal static class IntroNarrationScriptSourcesConstants
{
    /// <summary>The stock script location and bounding callbacks of one narration page.</summary>
    /// <param name="page">One of the six defined English story-page identities.</param>
    /// <returns>The page's script list offset and expected begin/finish callbacks, used by extraction.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="page"/> is not a defined narration page.</exception>
    internal static IntroNarrationScriptPage Page(IntroNarrationPageId page)
    {
        foreach (IntroNarrationScriptPage source in IntroNarrationScriptDefinitions.Pages)
        {
            if (source.Id == page)
                return source;
        }
        throw new ArgumentOutOfRangeException(nameof(page), page, "Unknown narration page.");
    }
}
