using System.Security.Cryptography;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable, bounded post-credit labels with cinematic behavior kept in code.</summary>
public sealed class EndingTextPresentation
{
    private readonly ushort[] resultPanel;
    private readonly ushort[] copyrightPanel;
    private readonly ushort[] japaneseSubtitle;
    private readonly EndingTextCharacter[] percentage;
    private readonly EndingTextCharacter[] finalMessage;

    private EndingTextPresentation(
        ushort[] resultPanel,
        ushort[] copyrightPanel,
        ushort[] japaneseSubtitle,
        EndingTextCharacter[] percentage,
        EndingTextCharacter[] finalMessage,
        string contentIdentity)
    {
        this.resultPanel = resultPanel;
        this.copyrightPanel = copyrightPanel;
        this.japaneseSubtitle = japaneseSubtitle;
        this.percentage = percentage;
        this.finalMessage = finalMessage;
        ContentIdentity = contentIdentity;
    }

    public string ContentIdentity { get; }
    public ushort[] BuildResultPanel() => resultPanel.ToArray();
    public ushort[] BuildCopyrightPanel() => copyrightPanel.ToArray();
    public ReadOnlySpan<ushort> JapaneseSubtitle => japaneseSubtitle;
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
            document = JsonSerializer.Deserialize<EndingTextDocument>(source,
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
            document.JapaneseSubtitle.Select(static cell => cell.Raw).ToArray(),
            percentage, final, Convert.ToHexString(SHA256.HashData(source)));
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
