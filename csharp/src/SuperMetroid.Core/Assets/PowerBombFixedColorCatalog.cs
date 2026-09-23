using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Two mutually exclusive cartridge fixed-color sequences.</summary>
public enum PowerBombFixedColorSequence
{
    PreExplosion,
    Explosion,
}

/// <summary>Editable RGB5 colors for Power Bomb, Crystal Flash and Ceres explosions.</summary>
public sealed class PowerBombFixedColorCatalog
{
    private readonly (byte Red, byte Green, byte Blue)[] preExplosion;
    private readonly (byte Red, byte Green, byte Blue)[] explosion;

    private PowerBombFixedColorCatalog(
        (byte Red, byte Green, byte Blue)[] preExplosion,
        (byte Red, byte Green, byte Blue)[] explosion)
    {
        this.preExplosion = preExplosion;
        this.explosion = explosion;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public static PowerBombFixedColorCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        PowerBombFixedColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<PowerBombFixedColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Power Bomb fixed-color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Power Bomb fixed-color JSON.", error);
        }
        if (document.Version != PowerBombFixedColorFormat.Version)
            throw new InvalidDataException("Power Bomb fixed colors require the supported version.");
        return new(Compile(document.PreExplosion,
                SamusPaletteRomData.PowerBomb.PreExplosionColorCount, "pre-explosion"),
            Compile(document.Explosion,
                SamusPaletteRomData.PowerBomb.ExplosionColorCount, "explosion"));
    }

    public static byte[] Write(PowerBombFixedColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Returns authored display color; phase and radius indexing remain engine-owned.</summary>
    public (byte Red, byte Green, byte Blue) Resolve(PowerBombFixedColorSequence sequence, int index)
    {
        (byte Red, byte Green, byte Blue)[] colors = sequence switch
        {
            PowerBombFixedColorSequence.PreExplosion => preExplosion,
            PowerBombFixedColorSequence.Explosion => explosion,
            _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
        };
        if ((uint)index >= colors.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors[index];
    }

    private static (byte Red, byte Green, byte Blue)[] Compile(
        PaletteRgb5[]? source, int count, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Power Bomb {name} requires {count} RGB5 colors.");
        var colors = new (byte Red, byte Green, byte Blue)[count];
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Power Bomb {name} color {index} requires components from zero through 31.");
            colors[index] = ((byte)color.Red, (byte)color.Green, (byte)color.Blue);
        }
        return colors;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Power Bomb fixed-color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record PowerBombFixedColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] PreExplosion { get; init; }
    public required PaletteRgb5[] Explosion { get; init; }
}

public static class PowerBombFixedColorFormat
{
    public const string FileName = "power-bomb-fixed-colors.json";
    public const int Version = 1;

    /// <summary>Native bank-$88 source address for one authored RGB5 sequence.</summary>
    public static int SourceAddress(PowerBombFixedColorSequence sequence) => sequence switch
    {
        PowerBombFixedColorSequence.PreExplosion => SamusPaletteRomData.PowerBomb.PreExplosionColors,
        PowerBombFixedColorSequence.Explosion => SamusPaletteRomData.PowerBomb.ExplosionColors,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
    };

    public static int Count(PowerBombFixedColorSequence sequence) => sequence switch
    {
        PowerBombFixedColorSequence.PreExplosion => SamusPaletteRomData.PowerBomb.PreExplosionColorCount,
        PowerBombFixedColorSequence.Explosion => SamusPaletteRomData.PowerBomb.ExplosionColorCount,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
    };
}
