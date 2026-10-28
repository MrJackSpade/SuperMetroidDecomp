using SuperMetroid.Core.Hardware;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>Editable, bounded post-credit labels with cinematic behavior kept in code.</summary>
public sealed class EndingTextPresentation
{
    /// <summary>Result-panel cells that differ from the stock layout after replacing the producer label.</summary>
    internal readonly Dictionary<int, ushort> resultOverrides = new();
    /// <summary>Producer label used when rebuilding unmodified result-panel cells.</summary>
    private readonly string resultText;
    /// <summary>Compiled large-font copyright tilemap returned by the panel builder.</summary>
    private readonly ushort[] copyrightPanel;
    /// <summary>Japanese subtitle cells whose raw words override the stock calculated layout.</summary>
    internal readonly Dictionary<int, ushort> subtitleOverrides = new();
    /// <summary>Glyph placements for the item-percentage typewriter sequence.</summary>
    private readonly EndingTextCharacter[] percentage;
    /// <summary>Glyph placements for the final message typewriter sequence.</summary>
    private readonly EndingTextCharacter[] finalMessage;

    /// <summary>Separates editable panel cells from calculated stock words and retains compiled ending glyphs.</summary>
    private EndingTextPresentation(
        ushort[] resultPanel,
        ushort[] copyrightPanel,
        EndingTextCell[] japaneseSubtitle,
        EndingTextCharacter[] percentage,
        EndingTextCharacter[] finalMessage,
        string resultText,
        string contentIdentity)
    {
        this.resultText = resultText;
        for (int cell = 0; cell < resultPanel.Length; cell++)
            if (resultPanel[cell] != EndingTextLayoutDefinitions.ResultWord(cell, resultText))
                resultOverrides.Add(cell, resultPanel[cell]);
        this.copyrightPanel = copyrightPanel;
        for (int cell = 0; cell < japaneseSubtitle.Length; cell++)
            if (japaneseSubtitle[cell].Word.Raw != EndingTextLayoutDefinitions.SubtitleWord(cell))
                subtitleOverrides.Add(cell, japaneseSubtitle[cell].Word.Raw);
        this.percentage = percentage;
        this.finalMessage = finalMessage;
        ContentIdentity = contentIdentity;
    }

    /// <summary>Uppercase SHA-256 hexadecimal digest of the original JSON bytes, including formatting, used to identify the loaded presentation.</summary>
    public string ContentIdentity { get; }
    /// <summary>Builds a fresh 9-by-32 row-major BG tilemap for the result credits, replacing the producer label and preserving independent template edits from native panel $8C:DC9B.</summary>
    public ushort[] BuildResultPanel()
    {
        var output = new ushort[EndingTextLayoutDefinitions.ResultCellCount];
        for (int cell = 0; cell < output.Length; cell++)
            output[cell] = resultOverrides.GetValueOrDefault(cell, EndingTextLayoutDefinitions.ResultWord(cell, resultText));
        return output;
    }
    /// <summary>Returns a fresh 2-by-32 row-major BG tilemap containing the configured large year and company labels in the layout of native panel $8C:DEDB.</summary>
    public ushort[] BuildCopyrightPanel() => copyrightPanel.ToArray();
    /// <summary>Gets the 2-by-32 Japanese subtitle tilemap corresponding to $8C:DF5B, with supplied raw-cell edits applied to the calculated stock layout.</summary>
    public SubtitleSequence JapaneseSubtitle => new(this);

    /// <summary>Calculated subtitle cells, with independent supplied edits taking precedence.</summary>
    public readonly struct SubtitleSequence : IReadOnlyList<ushort>
    {
        /// <summary>Presentation whose stock subtitle and raw overrides supply this view.</summary>
        private readonly EndingTextPresentation owner;
        /// <summary>Creates a read-only view of the presentation's resolved Japanese subtitle cells.</summary>
        internal SubtitleSequence(EndingTextPresentation owner) => this.owner = owner;
        /// <summary>Number of row-major subtitle tilemap words: two rows of 32 cells.</summary>
        public int Count => EndingTextLayoutDefinitions.SubtitleCellCount;
        /// <summary>Gets a packed BG tilemap word at a zero-based row-major index from 0 through 63, honoring a supplied edit before the stock glyph layout.</summary>
        public ushort this[int index]
        {
            get
            {
                return owner.subtitleOverrides.GetValueOrDefault(index, EndingTextLayoutDefinitions.SubtitleWord(index));
            }
        }
        /// <summary>Copies all 64 subtitle words in row-major order, leaving any additional destination cells unchanged.</summary>
        /// <param name="destination">Storage for at least two 32-cell tilemap rows.</param>
        /// <exception cref="ArgumentException">The destination has fewer than 64 cells.</exception>
        public void CopyTo(Span<ushort> destination)
        {
            if (destination.Length < Count)
                throw new ArgumentException("Destination is too short.", nameof(destination));
            for (int cell = 0; cell < Count; cell++) destination[cell] = this[cell];
        }
        /// <summary>Enumerates the resolved packed tilemap words from the first row's left edge through the second row's right edge.</summary>
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int cell = 0; cell < Count; cell++) yield return this[cell];
        }
        /// <summary>Enumerates resolved subtitle words through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Gets the precompiled glyph placements for the item-percentage labels ($8C:DFDB) or final message ($8C:E0AF); dynamic percentage digits and cinematic timing are supplied by the frontend.</summary>
    /// <param name="sequence">The bounded ending typewriter program whose configured characters are requested.</param>
    /// <returns>Characters in drawing order, with tilemap coordinates and one- or two-row packed glyph words.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The selector is not a supported ending text sequence.</exception>
    public ReadOnlySpan<EndingTextCharacter> Compile(EndingTextSequence sequence) => sequence switch
    {
        EndingTextSequence.ItemPercentage => percentage,
        EndingTextSequence.FinalMessage => finalMessage,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence), sequence, null),
    };

    /// <summary>Loads and validates version 1 of <c>ending-text.json</c>, compiling bounded labels and retaining raw result/subtitle tilemap edits without importing cinematic timers or instructions.</summary>
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
            throw new InvalidDataException("Ending text requires version 1 and exact result/subtitle template dimensions.");
        }

        ushort[] result = document.ResultPanel.Template.Select(static cell => cell.Word.Raw).ToArray();
        Array.Fill(result, EndingTextDefinitions.ResultBlankWord,
            EndingTextDefinitions.ResultProducedBy.Row * EndingTextDefinitions.TilemapWidth +
                EndingTextDefinitions.ResultProducedBy.Column,
            EndingTextDefinitions.ResultProducedBy.Width);
        Apply(result, document.ResultPanel.Text, EndingTextDefinitions.ResultProducedBy,
            EndingTextDefinitions.ResultPanelRows, "result panel");
        var copyright = Enumerable.Repeat(EndingTextDefinitions.LargeBlankWord,
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
    private static void Apply(ushort[] target, string text,
        EndingTextRegionDefinition region, int rows, string identity)
    {
        EndingTextCharacter[] characters = Compile(text, region, identity);
        foreach (EndingTextCharacter character in characters)
        {
            int index = character.Row * EndingTextDefinitions.TilemapWidth + character.Column;
            if ((uint)index >= target.Length || character.Row >= rows)
                throw new InvalidDataException($"Ending {identity} exceeds its bounded tilemap.");
            target[index] = character.TopWord.Raw;
            if (character.BottomWord is { } bottom)
                target[index + EndingTextDefinitions.TilemapWidth] = bottom.Raw;
        }
    }

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
                var top = new SnesBgTilemapWord(EndingTextDefinitions.CompileGlyph(glyph, region.Style));
                SnesBgTilemapWord? bottom = region.Style is EndingTextStyle.CopyrightLarge or EndingTextStyle.FinalLarge
                    ? new SnesBgTilemapWord(EndingTextDefinitions.CompileGlyph(glyph, region.Style, bottom: true))
                    : null;
                result[index] = new(region.Row, region.Column + index, top, bottom);
            }
            catch (ArgumentOutOfRangeException error)
            {
                throw new InvalidDataException($"Ending {identity} contains an unsupported glyph.", error);
            }
        }
        return result;
    }
}
