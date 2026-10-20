using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Selects each narration page's stock script from <see cref="IntroNarrationScriptDefinitions"/>.</summary>
internal static class IntroNarrationScriptSources
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

    /// <summary>The six stock page scripts in narrative order, page one through page six.</summary>
    internal static IEnumerable<IntroNarrationScriptPage> Pages
    {
        get
        {
            foreach (IntroNarrationPageId page in Enum.GetValues<IntroNarrationPageId>())
                yield return Page(page);
        }
    }
}
