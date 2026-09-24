using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Three editable, full-body normal suit palettes; suit selection remains gameplay code.</summary>
public sealed class SamusSuitColorCatalog
{
    private readonly ushort[][] colors;

    private SamusSuitColorCatalog(ushort[][] colors) => this.colors = colors;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort Resolve(ushort suitTableOffset, int colorIndex)
    {
        int suit = suitTableOffset switch
        {
            0 => 0,
            2 => 1,
            4 => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(suitTableOffset)),
        };
        if ((uint)colorIndex >= SamusSuitColorFormat.ColorsPerSuit)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return colors[suit][colorIndex];
    }

    /// <summary>Writes only Samus's sixteen OBJ colors; no equipment or phase state changes.</summary>
    public void Apply(SnesCgram cgram, ushort suitTableOffset)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < SamusSuitColorFormat.ColorsPerSuit; index++)
            cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + index,
                Resolve(suitTableOffset, index));
    }

    public static SamusSuitColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        SamusSuitColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusSuitColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Samus suit color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Samus suit color JSON.", error);
        }
        if (document.Version != SamusSuitColorFormat.Version)
            throw new InvalidDataException("Samus suit colors require the supported version.");
        return new([Compile(document.Power, "Power"), Compile(document.Varia, "Varia"),
            Compile(document.Gravity, "Gravity")]);
    }

    public static byte[] Write(SamusSuitColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort[] Compile(PaletteRgb5[]? source, string name)
    {
        if (source is null || source.Length != SamusSuitColorFormat.ColorsPerSuit)
            throw new InvalidDataException($"Samus {name} suit requires sixteen RGB5 colors.");
        var result = new ushort[source.Length];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus {name} color {index} requires RGB components 0..31.");
            result[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return result;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Samus suit color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record SamusSuitColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Power { get; init; }
    public required PaletteRgb5[] Varia { get; init; }
    public required PaletteRgb5[] Gravity { get; init; }
}

public static class SamusSuitColorFormat
{
    public const string FileName = "samus-suit-colors.json";
    public const int Version = 1;
    public const int ColorsPerSuit = SamusPaletteRomData.Common.ColorsPerObjPalette;
}
