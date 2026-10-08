using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable, bounded post-credit labels with cinematic behavior kept in code.</summary>
public sealed class EndingTextPresentation
{
    private readonly Dictionary<int, ushort> resultOverrides = new();
    private readonly string resultText;
    private readonly ushort[] copyrightPanel;
    private readonly Dictionary<int, ushort> subtitleOverrides = new();
    private readonly EndingTextCharacter[] percentage;
    private readonly EndingTextCharacter[] finalMessage;

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
            if (japaneseSubtitle[cell].Raw != EndingTextLayoutDefinitions.SubtitleWord(cell))
                subtitleOverrides.Add(cell, japaneseSubtitle[cell].Raw);
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
        private readonly EndingTextPresentation owner;
        internal SubtitleSequence(EndingTextPresentation owner) => this.owner = owner;
        /// <summary>Number of row-major subtitle tilemap words: two rows of 32 cells.</summary>
        public int Count => EndingTextLayoutDefinitions.SubtitleCellCount;
        /// <summary>Gets a packed BG tilemap word at a zero-based row-major index from 0 through 63, honoring a supplied edit before the stock glyph layout.</summary>
        public ushort this[int index]
        {
            get
            {
                ushort stock = EndingTextLayoutDefinitions.SubtitleWord(index);
                return owner.subtitleOverrides.GetValueOrDefault(index, stock);
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
        ArgumentNullException.ThrowIfNull(json);
        byte[] source;
        using (var buffer = new MemoryStream()) { json.CopyTo(buffer); source = buffer.ToArray(); }
        EndingTextDocument document;
        try
        {
            document = JsonAssetDocument.Read<EndingTextDocument>(source,
                MapPresentationFormat.JsonOptions)
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

        ushort[] result = document.ResultPanel.Template.Select(static cell => cell.Raw).ToArray();
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
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }

    private static void Apply(ushort[] target, string text,
        EndingTextRegionDefinition region, int rows, string identity)
    {
        EndingTextCharacter[] characters = Compile(text, region, identity);
        foreach (EndingTextCharacter character in characters)
        {
            int index = character.Row * EndingTextDefinitions.TilemapWidth + character.Column;
            if ((uint)index >= target.Length || character.Row >= rows)
                throw new InvalidDataException($"Ending {identity} exceeds its bounded tilemap.");
            target[index] = character.TopWord;
            if (character.BottomWord is { } bottom)
                target[index + EndingTextDefinitions.TilemapWidth] = bottom;
        }
    }

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
                ushort top = EndingTextDefinitions.CompileGlyph(glyph, region.Style);
                ushort? bottom = region.Style is EndingTextStyle.CopyrightLarge or EndingTextStyle.FinalLarge
                    ? EndingTextDefinitions.CompileGlyph(glyph, region.Style, bottom: true)
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

/// <summary>Editable <c>ending-text.json</c> schema for post-credit panels and typewriter labels; sequence timing, actual item percentage, and subtitle visibility remain runtime behavior.</summary>
public sealed record EndingTextDocument
{
    /// <summary>Schema revision; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Nine-row raw result-credit template and its independently configurable producer label.</summary>
    public required EndingResultPanel ResultPanel { get; init; }
    /// <summary>One through four large-font characters at row 0, column 9 of the copyright panel; uppercase letters, digits, and spaces are supported.</summary>
    public required string CopyrightYear { get; init; }
    /// <summary>One through eight large-font characters at row 0, column 15 of the copyright panel; uppercase letters, digits, and spaces are supported.</summary>
    public required string CopyrightCompany { get; init; }
    /// <summary>One through thirteen uppercase letters or spaces beginning at row 10, column 9; drawn before the detail line in the item-percentage typewriter sequence.</summary>
    public required string PercentageHeading { get; init; }
    /// <summary>One through nineteen uppercase letters or spaces beginning at row 12, column 6; the runtime draws the actual inventory percentage after this label.</summary>
    public required string PercentageDetail { get; init; }
    /// <summary>Exactly 64 raw BG tilemap cells in two 32-cell rows, retaining the native Japanese subtitle glyphs or independently edited words.</summary>
    public required EndingTextCell[] JapaneseSubtitle { get; init; }
    /// <summary>One through twenty uppercase letters or spaces beginning at row 2, column 6; compiled into two-row large glyphs for the final typewriter message.</summary>
    public required string FinalMessage { get; init; }
}

/// <summary>Editable result-credit tilemap based on $8C:DC9B, with a bounded producer label layered over the first row.</summary>
public sealed record EndingResultPanel
{
    /// <summary>Exactly 288 packed BG tilemap cells in nine 32-cell rows; the producer-label region is cleared and replaced during loading.</summary>
    public required EndingTextCell[] Template { get; init; }
    /// <summary>One through eleven uppercase letters or spaces drawn in the small font beginning at row 0, column 10 of the result panel.</summary>
    public required string Text { get; init; }
}

/// <summary>JSON wrapper for an independently editable SNES BG tilemap cell, rather than a character in a compiled label.</summary>
public sealed record EndingTextCell
{
    /// <summary>Packed 16-bit SNES BG tilemap word: ten-bit tile index, three-bit palette selection, priority, and horizontal/vertical flip bits.</summary>
    public required ushort Raw { get; init; }
}
