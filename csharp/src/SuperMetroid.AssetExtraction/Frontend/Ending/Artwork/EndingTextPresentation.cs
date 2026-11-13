using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable, bounded post-credit labels with cinematic behavior kept in code.</summary>
public sealed class EndingTextPresentation
{
    /// <summary>The installed result panel's tiles, with the installed producer label written in.</summary>
    private readonly BackgroundTile[] resultPanel;
    /// <summary>Compiled large-font copyright tiles returned by the panel builder.</summary>
    private readonly BackgroundTile[] copyrightPanel;
    /// <summary>The installed Japanese subtitle's tiles.</summary>
    private readonly BackgroundTile[] japaneseSubtitle;
    /// <summary>Glyph placements for the item-percentage typewriter sequence.</summary>
    internal readonly EndingTextCharacter[] percentage;
    /// <summary>Glyph placements for the final message typewriter sequence.</summary>
    internal readonly EndingTextCharacter[] finalMessage;

    /// <summary>Retains the installed panels and the compiled ending glyphs.</summary>
    private EndingTextPresentation(
        BackgroundTile[] resultPanel,
        BackgroundTile[] copyrightPanel,
        BackgroundTile[] japaneseSubtitle,
        EndingTextCharacter[] percentage,
        EndingTextCharacter[] finalMessage,
        string contentIdentity)
    {
        this.resultPanel = resultPanel;
        this.copyrightPanel = copyrightPanel;
        this.japaneseSubtitle = [.. japaneseSubtitle];
        this.percentage = percentage;
        this.finalMessage = finalMessage;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 hexadecimal digest of the original JSON bytes, including formatting, used to identify the loaded presentation.</summary>
    public string ContentIdentity { get; }
    /// <summary>Builds fresh 9-by-32 row-major tiles for the result credits: the installed panel of native panel $8C:DC9B with the installed producer label.</summary>
    public BackgroundTile[] BuildResultPanel() => resultPanel.ToArray();
    /// <summary>Returns fresh 2-by-32 row-major tiles containing the configured large year and company labels in the layout of native panel $8C:DEDB.</summary>
    public BackgroundTile[] BuildCopyrightPanel() => copyrightPanel.ToArray();
    /// <summary>Gets the installed 2-by-32 Japanese subtitle tiles corresponding to $8C:DF5B.</summary>
    public SubtitleSequence JapaneseSubtitle => new(this);

    /// <summary>The installed subtitle cells.</summary>
    public readonly struct SubtitleSequence : IReadOnlyList<BackgroundTile>
    {
        /// <summary>Presentation whose installed subtitle supplies this view.</summary>
        private readonly EndingTextPresentation owner;
        /// <summary>Creates a read-only view of the presentation's resolved Japanese subtitle cells.</summary>
        internal SubtitleSequence(EndingTextPresentation owner) => this.owner = owner;
        /// <summary>Number of row-major subtitle tiles: two rows of 32 cells.</summary>
        public int Count => EndingTextLayoutDefinitions.SubtitleCellCount;
        /// <summary>Gets the installed tile at a zero-based row-major index from 0 through 63.</summary>
        public BackgroundTile this[int index] => (uint)index < (uint)Count
            ? owner.japaneseSubtitle[index]
            : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Enumerates the resolved tiles from the first row's left edge through the second row's right edge.</summary>
        public IEnumerator<BackgroundTile> GetEnumerator()
        {
            for (int cell = 0; cell < Count; cell++) yield return this[cell];
        }
        /// <summary>Enumerates resolved subtitle tiles through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>The characters the ending's typewriter types, which the game reads instead of this presentation (#1467).</summary>
    public EndingTypewriterText Typewriter() => new([.. percentage], [.. finalMessage]);

    /// <summary>The installed typewriter text's revision (#1638).</summary>
    public const int TypewriterVersion = 1;

    /// <summary>The typewriter text of the extracted artwork document <paramref name="artwork"/>, as the installation holds it (#1638).</summary>
    /// <exception cref="InvalidDataException">The artwork document does not decode.</exception>
    internal static byte[] ExtractTypewriter(byte[] artwork)
    {
        EndingTextDocument document = JsonAssetDocument.Read<EndingTextDocument>(artwork, ArtworkJson.Options)
            ?? throw new InvalidDataException("Ending-text document is null.");
        return JsonSerializer.SerializeToUtf8Bytes(new EndingTypewriterDocument
        {
            Version = TypewriterVersion, PercentageHeading = document.PercentageHeading, PercentageDetail = document.PercentageDetail,
            FinalMessage = document.FinalMessage,
        }, ArtworkJson.Options);
    }

    /// <summary>
    /// Reads and validates the installed typewriter text (#1638): the characters the ending types, which the game binds. The
    /// panels and subtitle are import data the game never reads.
    /// </summary>
    /// <exception cref="InvalidDataException">The JSON, version, text lengths or glyphs are invalid.</exception>
    public static EndingTypewriterText ReadTypewriter(Stream json)
    {
        Ensure.NotNull(json);
        EndingTypewriterDocument document;
        try
        {
            document = JsonAssetDocument.Read<EndingTypewriterDocument>(json, ArtworkJson.Options)
                ?? throw new InvalidDataException("Ending typewriter document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid ending typewriter JSON.", error); }
        if (document.Version != TypewriterVersion)
            throw new InvalidDataException($"Ending typewriter text requires version {TypewriterVersion}.");
        return new([.. Percentage(document.PercentageHeading, document.PercentageDetail)], [.. Final(document.FinalMessage)]);
    }

    /// <summary>The item-percentage characters, compiled from the heading and detail labels.</summary>
    /// <exception cref="InvalidDataException">A label's length or glyphs are invalid.</exception>
    private static EndingTextCharacter[] Percentage(string heading, string detail) =>
    [
        .. Compile(heading, EndingTextDefinitions.PercentageHeading, "percentage heading"),
        .. Compile(detail, EndingTextDefinitions.PercentageDetail, "percentage detail"),
    ];

    /// <summary>The final message's characters, compiled from its label.</summary>
    /// <exception cref="InvalidDataException">The label's length or glyphs are invalid.</exception>
    private static EndingTextCharacter[] Final(string finalMessage) => Compile(finalMessage, EndingTextDefinitions.FinalMessage, "final message");

    /// <summary>Loads and validates <c>ending-text.json</c>, compiling bounded labels and keeping the installed result and subtitle tiles, without importing cinematic timers or instructions.</summary>
    /// <param name="json">UTF-8 JSON read from the current stream position to its end; the stream remains open.</param>
    /// <returns>A presentation with validated dimensions and glyphs, compiled text sequences, and an identity derived from the source bytes.</returns>
    /// <exception cref="InvalidDataException">The JSON, version, template dimensions, text lengths, or supported glyphs are invalid.</exception>
    public static EndingTextPresentation Load(Stream json)
    {
        Ensure.NotNull(json);
        byte[] source;
        using (var buffer = new MemoryStream()) { json.CopyTo(buffer); source = buffer.ToArray(); }
        EndingTextDocument document;
        try
        {
            document = JsonAssetDocument.Read<EndingTextDocument>(source,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ending-text document is null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid ending-text JSON.", error); }

        if (document.Version != EndingTextDefinitions.Version ||
            document.ResultPanel is null ||
            document.ResultPanel.Template is null ||
            document.ResultPanel.Template.Length != EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth ||
            document.JapaneseSubtitle is null ||
            document.JapaneseSubtitle.Length != EndingTextDefinitions.JapaneseSubtitleRows * EndingTextDefinitions.TilemapWidth)
        {
            throw new InvalidDataException($"Ending text requires version {EndingTextDefinitions.Version} and exact result/subtitle template dimensions.");
        }

        BackgroundTile[] result = [.. document.ResultPanel.Template];
        Array.Fill(result, EndingTextDefinitions.ResultBlankTile,
            EndingTextDefinitions.ResultProducedBy.Row * EndingTextDefinitions.TilemapWidth +
                EndingTextDefinitions.ResultProducedBy.Column,
            EndingTextDefinitions.ResultProducedBy.Width);
        Apply(result, document.ResultPanel.Text, EndingTextDefinitions.ResultProducedBy,
            EndingTextDefinitions.ResultPanelRows, "result panel");
        var copyright = Enumerable.Repeat(EndingTextDefinitions.LargeBlankTile,
            EndingTextDefinitions.CopyrightRows * EndingTextDefinitions.TilemapWidth).ToArray();
        Apply(copyright, document.CopyrightYear, EndingTextDefinitions.CopyrightYear,
            EndingTextDefinitions.CopyrightRows, "copyright year");
        Apply(copyright, document.CopyrightCompany, EndingTextDefinitions.CopyrightCompany,
            EndingTextDefinitions.CopyrightRows, "copyright company");
        EndingTextCharacter[] percentage = Percentage(document.PercentageHeading, document.PercentageDetail);
        EndingTextCharacter[] final = Final(document.FinalMessage);
        return new(result, copyright,
            document.JapaneseSubtitle,
            percentage, final, Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Serializes an editable ending-text document as UTF-8 JSON.</summary>
    /// <param name="output">Destination written at its current position and left open.</param>
    /// <param name="document">Versioned tilemap templates and bounded labels to serialize.</param>
    public static void Write(Stream output, EndingTextDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }

    /// <summary>Compiles a bounded label into its target tilemap, including the lower row of large glyphs.</summary>
    private static void Apply(BackgroundTile[] target, string text,
        EndingTextRegionDefinition region, int rows, string identity)
    {
        EndingTextCharacter[] characters = Compile(text, region, identity);
        foreach (EndingTextCharacter character in characters)
        {
            int index = character.Row * EndingTextDefinitions.TilemapWidth + character.Column;
            if ((uint)index >= target.Length || character.Row >= rows)
                throw new InvalidDataException($"Ending {identity} exceeds its bounded tilemap.");
            target[index] = EndingTextGlyphs.CompileGlyph(character.Character, character.Style);
            if (IsLarge(character.Style))
                target[index + EndingTextDefinitions.TilemapWidth] = EndingTextGlyphs.CompileGlyph(character.Character, character.Style, bottom: true);
        }
    }

    /// <summary>Whether <paramref name="style"/>'s glyphs are two rows tall.</summary>
    public static bool IsLarge(EndingTextStyle style) => style is EndingTextStyle.CopyrightLarge or EndingTextStyle.FinalLarge;

    /// <summary>Validates label length and glyph support, then places each glyph within the specified region.</summary>
    private static EndingTextCharacter[] Compile(string text,
        EndingTextRegionDefinition region, string identity)
    {
        if (string.IsNullOrEmpty(text) || text.Length > region.Width)
            throw new InvalidDataException($"Ending {identity} must contain 1 through {region.Width} characters.");
        var result = new EndingTextCharacter[text.Length];
        for (int index = 0; index < text.Length; index++)
        {
            char glyph = text[index];
            try
            {
                _ = EndingTextGlyphs.CompileGlyph(glyph, region.Style);
                if (IsLarge(region.Style))
                    _ = EndingTextGlyphs.CompileGlyph(glyph, region.Style, bottom: true);
                result[index] = new(region.Row, region.Column + index, region.Style, glyph);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException($"Ending {identity} contains an unsupported glyph.", error);
            }
        }
        return result;
    }
}
