using System.Collections;
using System.Text;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Editable UTF-8 lines and bounded tile placement for the six English opening-narration pages.
/// Typewriter timing, audio cadence, caret behavior, input waits and scene transitions remain compiled.
/// </summary>
public sealed class IntroNarrationPresentation
{
    /// <summary>The six opening text pages, keyed by page in typewriter order.</summary>
    private readonly Dictionary<IntroNarrationPageId, NarrationPage> pages;

    /// <summary>Retains the six pages the import validated and the hash of their installed bytes.</summary>
    internal IntroNarrationPresentation(
        Dictionary<IntroNarrationPageId, NarrationPage> pages,
        string contentIdentity)
    {
        this.pages = pages;
        ContentIdentity = contentIdentity;
    }

    /// <summary>SHA-256 identity of the validated source document, including its source encoding.</summary>
    public string ContentIdentity { get; }

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

    /// <summary>Calculated word wrapping and two-row spacing. Stock story wording and its one deliberate hard break are reviewed narrative content; independent edits remain exact.</summary>
    internal sealed class NarrationPage : IReadOnlyList<IntroNarrationLine>
    {
        /// <summary>Joined narrative text used when word wrapping exactly reproduces the supplied rows.</summary>
        internal readonly string? text;
        /// <summary>Independent edited rows retained when calculated wrapping would change their placement.</summary>
        internal readonly IntroNarrationLine[]? supplied;

        /// <summary>Reconstructs paragraph breaks and shares calculated wrapping only when every row matches.</summary>
        internal NarrationPage(IntroNarrationLine[] lines)
        {
            var candidate = new StringBuilder();
            for (int index = 0; index < lines.Length; index++)
            {
                candidate.Append(lines[index].Text);
                if (index + 1 == lines.Length) continue;
                string next = lines[index + 1].Text;
                int nextWord = next.IndexOf(' ');
                if (nextWord < 0) nextWord = next.Length;
                bool hardBreak = lines[index].Text.Length + 1 + nextWord <= IntroNarrationDefinitions.MaximumColumns;
                candidate.Append(hardBreak ? '\n' : ' ');
            }
            string joined = candidate.ToString();
            if (CalculateLines(joined).SequenceEqual(lines)) text = joined;
            else supplied = lines.ToArray();
        }

        /// <summary>Number of independently supplied or calculated two-row-spaced lines.</summary>
        public int Count => supplied?.Length ?? CalculateLines(CalculatedText).Count();

        /// <summary>The page text whose stock wrapping supplies the lines when no lines were supplied.</summary>
        private string CalculatedText => text ??
            throw new InvalidOperationException("A narration page has neither supplied lines nor wrapped text.");
        /// <summary>Line at a page position, calculated on demand when stock wrapping matches.</summary>
        public IntroNarrationLine this[int index]
        {
            get
            {
                Ensure.AtLeastZero(index);
                if (supplied is not null) return supplied[index];
                foreach (IntroNarrationLine line in CalculateLines(CalculatedText))
                    if (index-- == 0) return line;
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>Places hard-break-separated paragraphs into the available columns with two tile rows between lines.</summary>
        private static IEnumerable<IntroNarrationLine> CalculateLines(string text)
        {
            int row = IntroNarrationDefinitions.FirstTextRow;
            foreach (string paragraph in text.Split('\n'))
            {
                string remaining = paragraph;
                while (remaining.Length > IntroNarrationDefinitions.MaximumColumns)
                {
                    int boundary = remaining.LastIndexOf(' ', IntroNarrationDefinitions.MaximumColumns);
                    if (boundary < 1) boundary = IntroNarrationDefinitions.MaximumColumns;
                    yield return new() { Row = row, Text = remaining[..boundary] };
                    row += 2;
                    remaining = remaining[(boundary + 1)..];
                }
                yield return new() { Row = row, Text = remaining };
                row += 2;
            }
        }

        /// <summary>Visits selected page lines in ascending tilemap-row order.</summary>
        public IEnumerator<IntroNarrationLine> GetEnumerator() => supplied is not null
            ? ((IEnumerable<IntroNarrationLine>)supplied).GetEnumerator()
            : CalculateLines(CalculatedText).GetEnumerator();
        /// <summary>Exposes the same row order to non-generic enumeration.</summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
