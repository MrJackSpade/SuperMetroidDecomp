using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Editable, bounded post-credit labels with cinematic behavior kept in code.</summary>
public sealed class EndingTextPresentation
{
    /// <summary>Result-panel cells that differ from the stock layout after replacing the producer label.</summary>
    internal readonly Dictionary<int, BackgroundTile> resultOverrides = new();
    /// <summary>Producer label used when rebuilding unmodified result-panel cells.</summary>
    private readonly string resultText;
    /// <summary>Compiled large-font copyright tiles returned by the panel builder.</summary>
    private readonly BackgroundTile[] copyrightPanel;
    /// <summary>Japanese subtitle cells whose tiles override the stock calculated layout.</summary>
    internal readonly Dictionary<int, BackgroundTile> subtitleOverrides = new();
    /// <summary>Glyph placements for the item-percentage typewriter sequence.</summary>
    internal readonly EndingTextCharacter[] percentage;
    /// <summary>Glyph placements for the final message typewriter sequence.</summary>
    internal readonly EndingTextCharacter[] finalMessage;

    /// <summary>Separates editable panel cells from calculated stock tiles and retains compiled ending glyphs.</summary>
    private EndingTextPresentation(
        BackgroundTile[] resultPanel,
        BackgroundTile[] copyrightPanel,
        BackgroundTile[] japaneseSubtitle,
        EndingTextCharacter[] percentage,
        EndingTextCharacter[] finalMessage,
        string resultText,
        string contentIdentity)
    {
        this.resultText = resultText;
        for (int cell = 0; cell < resultPanel.Length; cell++)
            if (resultPanel[cell] != EndingTextPanels.ResultTile(cell, resultText))
                resultOverrides.Add(cell, resultPanel[cell]);
        this.copyrightPanel = copyrightPanel;
        for (int cell = 0; cell < japaneseSubtitle.Length; cell++)
            if (japaneseSubtitle[cell] != EndingTextPanels.SubtitleTile(cell))
                subtitleOverrides.Add(cell, japaneseSubtitle[cell]);
        this.percentage = percentage;
        this.finalMessage = finalMessage;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 hexadecimal digest of the original JSON bytes, including formatting, used to identify the loaded presentation.</summary>
    public string ContentIdentity { get; }
    /// <summary>Builds fresh 9-by-32 row-major tiles for the result credits, replacing the producer label and preserving independent template edits from native panel $8C:DC9B.</summary>
    public BackgroundTile[] BuildResultPanel()
    {
        var output = new BackgroundTile[EndingTextLayoutDefinitions.ResultCellCount];
        for (int cell = 0; cell < output.Length; cell++)
            output[cell] = resultOverrides.GetValueOrDefault(cell, EndingTextPanels.ResultTile(cell, resultText));
        return output;
    }
    /// <summary>Returns fresh 2-by-32 row-major tiles containing the configured large year and company labels in the layout of native panel $8C:DEDB.</summary>
    public BackgroundTile[] BuildCopyrightPanel() => copyrightPanel.ToArray();
    /// <summary>Gets the 2-by-32 Japanese subtitle tiles corresponding to $8C:DF5B, with supplied tile edits applied to the calculated stock layout.</summary>
    public SubtitleSequence JapaneseSubtitle => new(this);

    /// <summary>Calculated subtitle cells, with independent supplied edits taking precedence.</summary>
    public readonly struct SubtitleSequence : IReadOnlyList<BackgroundTile>
    {
        /// <summary>Presentation whose stock subtitle and tile overrides supply this view.</summary>
        private readonly EndingTextPresentation owner;
        /// <summary>Creates a read-only view of the presentation's resolved Japanese subtitle cells.</summary>
        internal SubtitleSequence(EndingTextPresentation owner) => this.owner = owner;
        /// <summary>Number of row-major subtitle tiles: two rows of 32 cells.</summary>
        public int Count => EndingTextLayoutDefinitions.SubtitleCellCount;
        /// <summary>Gets the tile at a zero-based row-major index from 0 through 63, honoring a supplied edit before the stock glyph layout.</summary>
        public BackgroundTile this[int index]
        {
            get
            {
                return owner.subtitleOverrides.GetValueOrDefault(index, EndingTextPanels.SubtitleTile(index));
            }
        }
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

    /// <summary>Loads and validates <c>ending-text.json</c>, compiling bounded labels and retaining result/subtitle tile edits without importing cinematic timers or instructions.</summary>
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
        EndingTextCharacter[] percentage =
        [
            .. Compile(document.PercentageHeading, EndingTextDefinitions.PercentageHeading, "percentage heading"),
            .. Compile(document.PercentageDetail, EndingTextDefinitions.PercentageDetail, "percentage detail"),
        ];
        EndingTextCharacter[] final = Compile(document.FinalMessage,
            EndingTextDefinitions.FinalMessage, "final message");
        return new(result, copyright,
            document.JapaneseSubtitle,
            percentage, final, document.ResultPanel.Text, Convert.ToHexString(SHA256.HashData(source)));
    }

    /// <summary>Serializes an editable ending-text document as UTF-8 JSON, validating it with <see cref="Load"/> before writing any bytes.</summary>
    /// <param name="output">Destination written at its current position and left open.</param>
    /// <param name="document">Versioned tilemap templates and bounded labels to serialize.</param>
    public static void Write(Stream output, EndingTextDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
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
