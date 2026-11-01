using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

/// <summary>Reads and writes the installed six-page English opening narration.</summary>
public static class IntroNarrationFile
{
    /// <summary>Required schema revision of the six-page editable English narration document.</summary>
    public const int Version = 1;

    /// <summary>JSON presentation-resource filename containing named pages and their ordered UTF-8 text lines.</summary>
    public const string FileName = "intro-narration.json";

    /// <summary>Last permitted text row in tile cells, sixteen, inclusive.</summary>
    public const int LastTextRow = 16;

    /// <summary>Loads and validates the exact six-page opening-narration document.</summary>
    /// <param name="json">Caller-owned stream containing the narration document.</param>
    /// <returns>The validated narration, identified by the hash of its source bytes.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, page set, rows, widths or glyphs are invalid.</exception>
    public static IntroNarrationPresentation Read(Stream json)
    {
        Ensure.NotNull(json);
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
                source, ArtworkJson.Options)
                ?? throw new InvalidDataException("Opening-narration document is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid opening-narration JSON.", error);
        }

        IntroNarrationPageId[] expected = Enum.GetValues<IntroNarrationPageId>();
        if (document.Version != Version || document.Pages is null ||
            document.Pages.Count != expected.Length ||
            expected.Any(page => !document.Pages.ContainsKey(page.ToString())))
        {
            throw new InvalidDataException(
                "Opening narration requires version 1 and the exact six-page name set.");
        }

        var pages = new Dictionary<IntroNarrationPageId, IntroNarrationPresentation.NarrationPage>();
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
                    line.Row > LastTextRow ||
                    (line.Row & 1) != 0 || !usedRows.Add(line.Row) ||
                    line.Text.Any(static character =>
                        !IntroNarrationGlyphs.IsSupportedGlyph(character)))
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
            pages.Add(page, new IntroNarrationPresentation.NarrationPage(value.Lines));
        }

        return new(pages, Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Validates and writes an opening-narration document as JSON.</summary>
    /// <param name="output">Caller-owned destination stream.</param>
    /// <param name="document">The narration document to validate and serialize.</param>
    public static void Write(Stream output, IntroNarrationDocument document)
    {
        Ensure.NotNull(output);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(
            document, ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}
