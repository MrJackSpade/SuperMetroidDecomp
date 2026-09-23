using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable RGB5 visor colors shared by X-ray and room palette cycling.</summary>
public sealed class SamusVisorColorCatalog
{
    private readonly ushort[] colors;

    private SamusVisorColorCatalog(ushort[] colors) => this.colors = colors;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public static SamusVisorColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        SamusVisorColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<SamusVisorColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Samus visor color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Samus visor color JSON.", error);
        }
        if (document.Version != SamusVisorColorFormat.Version ||
            document.Colors is null || document.Colors.Length != SamusVisorColorFormat.ColorCount)
            throw new InvalidDataException("Samus visor colors require the supported version and six RGB5 colors.");

        var compiled = new ushort[document.Colors.Length];
        for (int index = 0; index < compiled.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus visor color {index} requires RGB components from zero through 31.");
            compiled[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return new(compiled);
    }

    public static byte[] Write(SamusVisorColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>
    /// Resolves only the six authored even offsets. A corrupted native timer/index may
    /// address adjacent cartridge bytes; callers retain their bus path for that case.
    /// </summary>
    public bool TryResolveByteOffset(int byteOffset, out ushort color)
    {
        if ((byteOffset & 1) == 0 && (uint)(byteOffset >> 1) < colors.Length)
        {
            color = colors[byteOffset >> 1];
            return true;
        }
        color = 0;
        return false;
    }

    public ushort Resolve(int index) => (uint)index < colors.Length
        ? colors[index] : throw new ArgumentOutOfRangeException(nameof(index));

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Samus visor color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record SamusVisorColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}

public static class SamusVisorColorFormat
{
    public const string FileName = "samus-visor-colors.json";
    public const int Version = 1;
    /// <summary>The six authored BGR555 words at $9B:A3C0, including X-ray widening and room-cycle colors.</summary>
    public const int SourceAddress = SamusPaletteRomData.Visor.Colors;
    public const int ColorCount = 6;
}
