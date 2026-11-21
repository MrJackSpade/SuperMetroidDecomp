using System.Collections;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Editable UTF-8 lines and bounded glyph placement for the six English opening-narration pages.
/// Typewriter timing, audio cadence, caret behavior, input waits and scene transitions are the game's own.
/// </summary>
public sealed partial class IntroNarrationPresentation
{
    /// <summary>The six opening text pages, keyed by page in typewriter order, every page checked present as they are kept.</summary>
    private readonly IdentityTable<IntroNarrationPageId, NarrationPage> pages;

    /// <summary>Retains the six pages the import validated.</summary>
    /// <exception cref="ArgumentException">A page is missing.</exception>
    internal IntroNarrationPresentation(Dictionary<IntroNarrationPageId, NarrationPage> pages) => this.pages = new(pages);

    /// <summary>Ordered editable lines for one of the six narration pages.</summary>
    internal NarrationPage GetLines(IntroNarrationPageId page) => pages[page];

    /// <summary>Builds a narration page's ordered typewriter character placements.</summary>
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
    internal sealed partial class NarrationPage : IReadOnlyList<IntroNarrationLine>
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
