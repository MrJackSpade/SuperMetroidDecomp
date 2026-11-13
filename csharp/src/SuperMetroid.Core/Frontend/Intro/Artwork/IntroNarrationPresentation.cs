using System.Collections;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Editable UTF-8 lines and bounded glyph placement for the six English opening-narration pages.
/// Typewriter timing, audio cadence, caret behavior, input waits and scene transitions remain compiled.
/// </summary>
public sealed class IntroNarrationPresentation
{
    /// <summary>The six opening text pages, keyed by page in typewriter order.</summary>
    private readonly Dictionary<IntroNarrationPageId, NarrationPage> pages;

    /// <summary>Retains the six pages the import validated.</summary>
    internal IntroNarrationPresentation(Dictionary<IntroNarrationPageId, NarrationPage> pages) => this.pages = pages;

    /// <summary>Ordered editable lines for one of the six narration pages.</summary>
    public IReadOnlyList<IntroNarrationLine> GetLines(IntroNarrationPageId page) =>
        pages.TryGetValue(page, out NarrationPage? lines)
            ? lines
            : throw new ArgumentOutOfRangeException(nameof(page), page, "The narration catalog does not contain this page.");
    /// <summary>Compiles a narration page into its ordered typewriter character placements.</summary>
    public IntroNarrationCharacter[] Compile(IntroNarrationPageId page)
    {
        IReadOnlyList<IntroNarrationLine> lines = GetLines(page);
        var result = new List<IntroNarrationCharacter>(
            lines.Sum(static line => line.Text.Length));
        foreach (IntroNarrationLine line in lines)
        {
            for (int index = 0; index < line.Text.Length; index++)
            {
                result.Add(new(IntroNarrationDefinitions.FirstColumn + index, line.Row, line.Text[index]));
            }
        }
        return result.ToArray();
    }

    /// <summary>One page's installed lines, in ascending text-row order.</summary>
    internal sealed class NarrationPage : IReadOnlyList<IntroNarrationLine>
    {
        /// <summary>The page's installed lines.</summary>
        private readonly IntroNarrationLine[] lines;

        /// <summary>Retains the page's installed lines.</summary>
        internal NarrationPage(IntroNarrationLine[] lines) => this.lines = [.. lines];

        /// <summary>Number of installed lines.</summary>
        public int Count => lines.Length;

        /// <summary>The installed line at a page position.</summary>
        public IntroNarrationLine this[int index] =>
            (uint)index < lines.Length ? lines[index] : throw new ArgumentOutOfRangeException(nameof(index));

        /// <summary>Visits the installed lines in ascending text-row order.</summary>
        public IEnumerator<IntroNarrationLine> GetEnumerator() => ((IEnumerable<IntroNarrationLine>)lines).GetEnumerator();
        /// <summary>Exposes the same row order to non-generic enumeration.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
