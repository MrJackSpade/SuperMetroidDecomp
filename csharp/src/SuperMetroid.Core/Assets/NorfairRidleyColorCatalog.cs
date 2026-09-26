using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable initial OBJ colors and fifteen arena-reveal BG colors for Lower Norfair Ridley.</summary>
public sealed class NorfairRidleyColorCatalog
{
    private readonly ushort[] initial;
    private readonly ushort[][] reveal;

    private NorfairRidleyColorCatalog(ushort[] initial, ushort[][] reveal)
    {
        this.initial = initial;
        this.reveal = reveal;
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveInitial(int color) => initial[color];
    public ushort ResolveReveal(int row, int color) => reveal[row][color];

    public void ApplyInitial(SnesCgram cgram) =>
        Apply(cgram, initial, NorfairRidleyPaletteRomData.InitialCgramIndex);

    public void ApplyReveal(SnesCgram cgram, int row) =>
        Apply(cgram, reveal[row], NorfairRidleyPaletteRomData.RevealCgramIndex);

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

    private static void Apply(SnesCgram cgram, ushort[] colors, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < colors.Length; color++)
            cgram.SetColor(destination + color, colors[color]);
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException("Duplicate Norfair Ridley color property.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
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
