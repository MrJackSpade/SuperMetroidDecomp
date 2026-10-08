using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable initial OBJ colors and fifteen arena-reveal BG colors for Lower Norfair Ridley.</summary>
public sealed class NorfairRidleyColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("NorfairRidleyColorCatalog-v1", content =>
        {
            Span<ushort> initialWords = stackalloc ushort[NorfairRidleyPaletteRomData.InitialColorCount];
            for (int color = 0; color < initialWords.Length; color++) initialWords[color] = initial.ColorAt(color);
            content.AppendWords("initial", initialWords);
            ushort[][] revealWords = new ushort[NorfairRidleyPaletteRomData.RevealRowCount][];
            for (int row = 0; row < revealWords.Length; row++)
            {
                revealWords[row] = new ushort[NorfairRidleyPaletteRomData.RevealColorCount];
                for (int color = 0; color < revealWords[row].Length; color++) revealWords[row][color] = reveal.ColorAt(row, color);
            }
            content.AppendWordFrames("reveal", revealWords);
        });

    private readonly NorfairRidleyInitialPaintDefinitions initial;
    private readonly NorfairRidleyRevealPaintDefinitions reveal;

    private NorfairRidleyColorCatalog(ushort[] initial, ushort[][] reveal)
    {
        this.initial = new(initial);
        this.reveal = new(reveal);
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public void ApplyInitial(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < NorfairRidleyPaletteRomData.InitialColorCount; color++)
            cgram.SetColor(NorfairRidleyPaletteRomData.InitialCgramIndex + color, initial.ColorAt(color));
    }

    public void ApplyReveal(SnesCgram cgram, int row)
    {
        NorfairRidleyRevealPaintDefinitions.ValidateRow(row);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < NorfairRidleyPaletteRomData.RevealColorCount; color++)
            cgram.SetColor(NorfairRidleyPaletteRomData.RevealCgramIndex + color, reveal.ColorAt(row, color));
    }

    public static NorfairRidleyColorCatalog Load(Stream json)
    {
        NorfairRidleyColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<NorfairRidleyColorDocument>(Options)
                ?? throw new InvalidDataException("Norfair Ridley colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Norfair Ridley color JSON.", error);
        }

        if (document.Version != NorfairRidleyColorFormat.Version ||
            document.Initial is null ||
            document.Initial.Length != NorfairRidleyPaletteRomData.InitialColorCount ||
            document.Reveal is null ||
            document.Reveal.Length != NorfairRidleyPaletteRomData.RevealRowCount)
            throw new InvalidDataException("Norfair Ridley colors require the initial palette and all reveal rows.");

        ushort[] initial = Compile(document.Initial, "initial");
        var reveal = new ushort[document.Reveal.Length][];
        for (int row = 0; row < reveal.Length; row++)
        {
            if (document.Reveal[row] is null ||
                document.Reveal[row].Length != NorfairRidleyPaletteRomData.RevealColorCount)
                throw new InvalidDataException($"Norfair Ridley reveal row {row} requires fourteen colors.");
            reveal[row] = Compile(document.Reveal[row], $"reveal row {row}");
        }
        return new(initial, reveal);
    }

    public static byte[] Write(NorfairRidleyColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[] colors, string label)
    {
        var words = new ushort[colors.Length];
        for (int color = 0; color < words.Length; color++)
        {
            PaletteRgb5? rgb = colors[color];
            if (rgb is null || (uint)rgb.Red > 31 ||
                (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                throw new InvalidDataException($"Norfair Ridley {label} color {color} requires RGB5 channels.");
            words[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return words;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Norfair Ridley color property."));
}

public sealed record NorfairRidleyColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Initial { get; init; }
    public required PaletteRgb5[][] Reveal { get; init; }
}

/// <summary>Editable visual rows; the reveal's cadence and zero terminator stay in code.</summary>
public static class NorfairRidleyColorFormat
{
    public const string FileName = "norfair-ridley-colors.json";
    public const int Version = 1;
}
