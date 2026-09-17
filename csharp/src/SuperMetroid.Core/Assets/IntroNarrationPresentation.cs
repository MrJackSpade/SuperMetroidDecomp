using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable UTF-8 lines and bounded tile placement for the six English opening-narration pages.
/// Typewriter timing, audio cadence, caret behavior, input waits and scene transitions remain compiled.
/// </summary>
public sealed class IntroNarrationPresentation
{
    private readonly Dictionary<IntroNarrationPageId, IntroNarrationLine[]> pages;

    private IntroNarrationPresentation(
        Dictionary<IntroNarrationPageId, IntroNarrationLine[]> pages,
        string contentIdentity)
    {
        this.pages = pages;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }

    public ReadOnlySpan<IntroNarrationLine> GetLines(IntroNarrationPageId page) =>
        pages.TryGetValue(page, out IntroNarrationLine[]? lines)
            ? lines
            : throw new ArgumentOutOfRangeException(nameof(page), page,
                "The narration catalog does not contain this page.");

    public IntroNarrationCharacter[] Compile(IntroNarrationPageId page)
    {
        ReadOnlySpan<IntroNarrationLine> lines = GetLines(page);
        var result = new List<IntroNarrationCharacter>(
            lines.ToArray().Sum(static line => line.Text.Length));
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
            document = JsonSerializer.Deserialize<IntroNarrationDocument>(
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

        var pages = new Dictionary<IntroNarrationPageId, IntroNarrationLine[]>();
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
            pages.Add(page, value.Lines.ToArray());
        }

        return new(pages, Convert.ToHexString(SHA256.HashData(source)));
    }

    public static void Write(Stream output, IntroNarrationDocument document)
    {
        ArgumentNullException.ThrowIfNull(output);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

public sealed record IntroNarrationDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, IntroNarrationPage> Pages { get; init; }
}

public sealed record IntroNarrationPage
{
    public required IntroNarrationLine[] Lines { get; init; }
}

public sealed record IntroNarrationLine
{
    public required int Row { get; init; }
    public required string Text { get; init; }
}

public readonly record struct IntroNarrationCharacter(
    int Column,
    int Row,
    ushort TilemapWord,
    bool IsSpace);
