using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable UTF-8 lines and bounded tile placement for the six English opening-narration pages.
/// Typewriter timing, audio cadence, caret behavior, input waits and scene transitions remain compiled.
/// </summary>
public sealed class IntroNarrationPresentation
{
    /// <summary>Validated page layouts retained in page-ID order for direct narration lookup.</summary>
    private readonly NarrationPage page1, page2, page3, page4, page5, page6;

    /// <summary>Stores the six validated page layouts and the identity of their source document.</summary>
    /// <param name="pages">Validated layouts keyed by every supported narration page ID.</param>
    /// <param name="contentIdentity">SHA-256 identity calculated from the original JSON bytes.</param>
    private IntroNarrationPresentation(
        Dictionary<IntroNarrationPageId, NarrationPage> pages,
        string contentIdentity)
    {
        page1 = pages[IntroNarrationPageId.Page1];
        page2 = pages[IntroNarrationPageId.Page2];
        page3 = pages[IntroNarrationPageId.Page3];
        page4 = pages[IntroNarrationPageId.Page4];
        page5 = pages[IntroNarrationPageId.Page5];
        page6 = pages[IntroNarrationPageId.Page6];
        ContentIdentity = contentIdentity;
    }

    /// <summary>Gets the SHA-256 identity of the validated source document.</summary>
    public string ContentIdentity { get; }

    /// <summary>Gets the ordered editable lines for one of the six narration pages.</summary>
    public IReadOnlyList<IntroNarrationLine> GetLines(IntroNarrationPageId page) => page switch
    {
        IntroNarrationPageId.Page1 => page1,
        IntroNarrationPageId.Page2 => page2,
        IntroNarrationPageId.Page3 => page3,
        IntroNarrationPageId.Page4 => page4,
        IntroNarrationPageId.Page5 => page5,
        IntroNarrationPageId.Page6 => page6,
        _ => throw new ArgumentOutOfRangeException(nameof(page), page,
            "The narration catalog does not contain this page."),
    };
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
                char glyph = line.Text[index];
                result.Add(new(
                    IntroNarrationDefinitions.FirstColumn + index,
                    line.Row,
                    IntroNarrationDefinitions.CompileGlyph(glyph),
                    glyph == ' '));
            }
        }
        return result.ToArray();
    }

    /// <summary>Loads and validates the exact six-page opening-narration document.</summary>
    public static IntroNarrationPresentation Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] source;
        using (var buffer = new MemoryStream())
        {
            json.CopyTo(buffer);
            source = buffer.ToArray();
        }

        IntroNarrationDocument document;
        try
        {
            document = JsonAssetDocument.Read<IntroNarrationDocument>(
                source, MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Opening-narration document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid opening-narration JSON.", error);
        }

        IntroNarrationPageId[] expected = Enum.GetValues<IntroNarrationPageId>();
        if (document.Version != IntroNarrationDefinitions.Version || document.Pages is null ||
            document.Pages.Count != expected.Length ||
            expected.Any(page => !document.Pages.ContainsKey(page.ToString())))
        {
            throw new InvalidDataException(
                "Opening narration requires version 1 and the exact six-page name set.");
        }

        var pages = new Dictionary<IntroNarrationPageId, NarrationPage>();
        foreach (IntroNarrationPageId page in expected)
        {
            IntroNarrationPage value = document.Pages[page.ToString()]
                ?? throw new InvalidDataException($"Opening-narration {page} is null.");
            if (value.Lines is null || value.Lines.Length == 0)
                throw new InvalidDataException($"Opening-narration {page} has no lines.");

            var usedRows = new HashSet<int>();
            foreach (IntroNarrationLine line in value.Lines)
            {
                if (string.IsNullOrEmpty(line.Text) ||
                    line.Text.Length > IntroNarrationDefinitions.MaximumColumns ||
                    line.Row < IntroNarrationDefinitions.FirstTextRow ||
                    line.Row > IntroNarrationDefinitions.LastTextRow ||
                    (line.Row & 1) != 0 || !usedRows.Add(line.Row) ||
                    line.Text.Any(static character =>
                        !IntroNarrationDefinitions.IsSupportedGlyph(character)))
                {
                    throw new InvalidDataException(
                        $"Opening-narration {page} contains an invalid row, width or glyph.");
                }
            }
            if (!value.Lines.Select(static line => line.Row).SequenceEqual(
                    value.Lines.Select(static line => line.Row).Order()))
            {
                throw new InvalidDataException(
                    $"Opening-narration {page} lines must be in ascending row order.");
            }
            pages.Add(page, new NarrationPage(value.Lines));
        }

        return new(pages, Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Calculated word wrapping and two-row spacing. Stock story wording and its one deliberate hard break are reviewed narrative content; independent edits remain exact.</summary>
    private sealed class NarrationPage : IReadOnlyList<IntroNarrationLine>
    {
        /// <summary>Joined source text used when its calculated wrapping reproduces the supplied lines exactly.</summary>
        private readonly string? text;
        /// <summary>Original ordered lines retained when calculating from joined text would alter their layout.</summary>
        private readonly IntroNarrationLine[]? supplied;

        /// <summary>Chooses compact joined-text storage only when its layout round-trips without changing the lines.</summary>
        /// <param name="lines">Validated lines for this page in ascending row order.</param>
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

        /// <summary>Number of lines in the retained layout, whether stored directly or calculated from text.</summary>
        public int Count => supplied?.Length ?? CalculateLines(text!).Count();

        /// <summary>Gets a line by zero-based position without changing the page's authored layout.</summary>
        /// <exception cref="IndexOutOfRangeException">The index is negative or past the final line.</exception>
        public IntroNarrationLine this[int index]
        {
            get
            {
                if (index < 0) throw new IndexOutOfRangeException();
                if (supplied is not null) return supplied[index];
                foreach (IntroNarrationLine line in CalculateLines(text!))
                    if (index-- == 0) return line;
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>Wraps each paragraph to the narration column limit and places successive lines two tilemap rows apart.</summary>
        /// <param name="text">Page text whose newline characters preserve deliberate paragraph breaks.</param>
        /// <returns>Lines with calculated row positions and wrapped text segments.</returns>
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

        /// <summary>Enumerates this page's lines in display order, using the retained layout or calculated wrapping.</summary>
        public IEnumerator<IntroNarrationLine> GetEnumerator() => supplied is not null
            ? ((IEnumerable<IntroNarrationLine>)supplied).GetEnumerator()
            : CalculateLines(text!).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Validates and writes an opening-narration document as JSON.</summary>
    public static void Write(Stream output, IntroNarrationDocument document)
    {
        ArgumentNullException.ThrowIfNull(output);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

/// <summary>Defines all six editable opening-narration pages.</summary>
public sealed record IntroNarrationDocument
{
    /// <summary>Gets the narration document schema revision.</summary>
    public required int Version { get; init; }
    /// <summary>Gets the pages keyed by <see cref="IntroNarrationPageId"/> name.</summary>
    public required Dictionary<string, IntroNarrationPage> Pages { get; init; }
}

/// <summary>Defines the ordered lines displayed on one opening-narration page.</summary>
public sealed record IntroNarrationPage
{
    /// <summary>Gets the nonempty lines in ascending tilemap-row order.</summary>
    public required IntroNarrationLine[] Lines { get; init; }
}

/// <summary>Defines one line of opening-narration text and its tilemap row.</summary>
public sealed record IntroNarrationLine
{
    /// <summary>Gets the even tilemap row from the first through last supported text row.</summary>
    public required int Row { get; init; }
    /// <summary>Gets the supported narration glyphs, bounded to the page's available columns.</summary>
    public required string Text { get; init; }
}

/// <summary>Identifies one compiled character emitted in typewriter order.</summary>
/// <param name="Column">Zero-based tilemap column for the character.</param>
/// <param name="Row">Zero-based tilemap row for the character.</param>
/// <param name="TilemapWord">Compiled BG tilemap word for the narration glyph.</param>
/// <param name="IsSpace">Whether the source character is a space.</param>
public readonly record struct IntroNarrationCharacter(
    int Column,
    int Row,
    ushort TilemapWord,
    bool IsSpace);
