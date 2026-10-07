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
    private readonly Dictionary<int, (byte Red, byte Green, byte Blue)> preExplosion;
    private readonly Dictionary<int, (byte Red, byte Green, byte Blue)> explosion;

    private PowerBombFixedColorCatalog(
        Dictionary<int, (byte Red, byte Green, byte Blue)> preExplosion,
        Dictionary<int, (byte Red, byte Green, byte Blue)> explosion)
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
                SamusPaletteRomData.PowerBomb.PreExplosionColorCount, PowerBombFixedColorSequence.PreExplosion, "pre-explosion"),
            Compile(document.Explosion,
                SamusPaletteRomData.PowerBomb.ExplosionColorCount, PowerBombFixedColorSequence.Explosion, "explosion"));
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
        Dictionary<int, (byte Red, byte Green, byte Blue)> colors = sequence switch
        {
            PowerBombFixedColorSequence.PreExplosion => preExplosion,
            PowerBombFixedColorSequence.Explosion => explosion,
            _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
        };
        if ((uint)index >= PowerBombFixedColorFormat.Count(sequence))
            throw new ArgumentOutOfRangeException(nameof(index));
        if (colors.TryGetValue(index, out var supplied)) return supplied;
        if (PowerBombFixedColorFormat.TryCalculateStock(sequence, index, out var calculated)) return calculated;
        throw new InvalidDataException("Power Bomb color has neither supplied content nor a calculated definition.");
    }

    private static Dictionary<int, (byte Red, byte Green, byte Blue)> Compile(
        PaletteRgb5[]? source, int count, PowerBombFixedColorSequence sequence, string name)
    {
        if (source is null || source.Length != count)
            throw new InvalidDataException($"Power Bomb {name} requires {count} RGB5 colors.");
        var colors = new Dictionary<int, (byte Red, byte Green, byte Blue)>();
        for (int index = 0; index < source.Length; index++)
        {
            PaletteRgb5? color = source[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Power Bomb {name} color {index} requires components from zero through 31.");
            var supplied = ((byte)color.Red, (byte)color.Green, (byte)color.Blue);
            if (!PowerBombFixedColorFormat.TryCalculateStock(sequence, index, out var calculated) || supplied != calculated)
                colors.Add(index, supplied);
        }
        return colors;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Power Bomb fixed-color property {name}."));
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
    /// <summary>
    /// $88:9079 PowerBomb_PreExplosion_Colors: white-to-yellow rise and decline;
    /// $88:8D85 PowerBombExplosion_Colors: yellow opening and grayscale white crest.
    /// Calculates all sixteen pre-explosion colors and explosion colors zero through twenty.
    /// Later explosion colors remain independently supplied pending their own conversion.
    /// </summary>
    internal static bool TryCalculateStock(PowerBombFixedColorSequence sequence, int index,
        out (byte Red, byte Green, byte Blue) color)
    {
        color = default;
        if (sequence == PowerBombFixedColorSequence.PreExplosion && (uint)index < 16)
        {
            int red = index == 0 ? 16 : index <= 12 ? 2 * index + 2 : 50 - 2 * index;
            int blue = index <= 5 ? red : index is 6 or 12 ? 10 : index < 12 ? 8 : 34 - 2 * index;
            color = ((byte)red, (byte)red, (byte)blue);
            return true;
        }
        if (sequence != PowerBombFixedColorSequence.Explosion || (uint)index > 20) return false;
        if (index < 14)
        {
            byte yellow = (byte)Math.Min(14 + index, 26);
            color = (yellow, yellow, (byte)Math.Max(10 - index, 0));
        }
        else
        {
            byte white = (byte)(index <= 18 ? 26 + (index - 14) / 2 : 46 - index);
            color = (white, white, white);
        }
        return true;
    }
}
