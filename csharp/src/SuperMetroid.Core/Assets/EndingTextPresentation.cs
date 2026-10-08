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

    public string ContentIdentity { get; }
    public ushort[] BuildResultPanel()
    {
        var output = new ushort[EndingTextLayoutDefinitions.ResultCellCount];
        for (int cell = 0; cell < output.Length; cell++)
            output[cell] = resultOverrides.GetValueOrDefault(cell, EndingTextLayoutDefinitions.ResultWord(cell, resultText));
        return output;
    }
    public ushort[] BuildCopyrightPanel() => copyrightPanel.ToArray();
    public SubtitleSequence JapaneseSubtitle => new(this);

    /// <summary>Calculated subtitle cells, with independent supplied edits taking precedence.</summary>
    public readonly struct SubtitleSequence : IReadOnlyList<ushort>
    {
        private readonly EndingTextPresentation owner;
        internal SubtitleSequence(EndingTextPresentation owner) => this.owner = owner;
        public int Count => EndingTextLayoutDefinitions.SubtitleCellCount;
        public ushort this[int index]
        {
            get
            {
                ushort stock = EndingTextLayoutDefinitions.SubtitleWord(index);
                return owner.subtitleOverrides.GetValueOrDefault(index, stock);
            }
        }
        public void CopyTo(Span<ushort> destination)
        {
            if (destination.Length < Count)
                throw new ArgumentException("Destination is too short.", nameof(destination));
            for (int cell = 0; cell < Count; cell++) destination[cell] = this[cell];
        }
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int cell = 0; cell < Count; cell++) yield return this[cell];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    public ReadOnlySpan<EndingTextCharacter> Compile(EndingTextSequence sequence) => sequence switch
    {
        EndingTextSequence.ItemPercentage => percentage,
        EndingTextSequence.FinalMessage => finalMessage,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence), sequence, null),
    };

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

public sealed record EndingTextDocument
{
    public required int Version { get; init; }
    public required EndingResultPanel ResultPanel { get; init; }
    public required string CopyrightYear { get; init; }
    public required string CopyrightCompany { get; init; }
    public required string PercentageHeading { get; init; }
    public required string PercentageDetail { get; init; }
    public required EndingTextCell[] JapaneseSubtitle { get; init; }
    public required string FinalMessage { get; init; }
}

public sealed record EndingResultPanel
{
    public required EndingTextCell[] Template { get; init; }
    public required string Text { get; init; }
}

public sealed record EndingTextCell
{
    public required ushort Raw { get; init; }
}
