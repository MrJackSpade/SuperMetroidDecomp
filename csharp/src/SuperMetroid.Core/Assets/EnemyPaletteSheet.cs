using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One editable sixteen-color RGB5 OBJ palette; selection and animation remain engine-owned.</summary>
public sealed class EnemyPaletteSheet
{
    public const int ColorCount = 16;
    private readonly ushort[] colors;
    private EnemyPaletteSheet(ushort[] colors) => this.colors = colors;

    public static EnemyPaletteSheet Load(Stream json)
    {
        EnemyPaletteSheetDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<EnemyPaletteSheetDocument>(Options)
                ?? throw new InvalidDataException("Enemy palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid enemy palette JSON.", error);
        }
        if (document.Version != 1 || document.Colors is null || document.Colors.Length != ColorCount)
            throw new InvalidDataException("Enemy palette requires version 1 and exactly sixteen RGB5 colors.");
        var compiled = new ushort[ColorCount];
        for (int i = 0; i < ColorCount; i++)
        {
            PaletteRgb5? color = document.Colors[i];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Enemy palette color {i} requires RGB channels in 0..31.");
            compiled[i] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return new EnemyPaletteSheet(compiled);
    }

    public static byte[] Write(EnemyPaletteSheetDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    public void LoadTo(SnesCgram cgram, int destinationColor)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (destinationColor < 0 || destinationColor + ColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destinationColor));
        for (int i = 0; i < ColorCount; i++) cgram.SetColor(destinationColor + i, colors[i]);
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate enemy palette property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record EnemyPaletteSheetDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}
