using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Two mutually exclusive cartridge fixed-color sequences.</summary>
public enum PowerBombFixedColorSequence
{
    /// <summary>$88:9079, PowerBomb_PreExplosion_Colors: sixteen fixed-color RGB5 triplets selected by the pre-explosion radius through its white and yellow phases.</summary>
    PreExplosion,
    /// <summary>$88:8D85, PowerBombExplosion_Colors: thirty-two radius-selected fixed-color RGB5 triplets shared by Power Bomb, Crystal Flash, and the Ceres station explosion.</summary>
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

    /// <summary>Validates the installed fixed-color JSON and compiles independent edits to the two native RGB5 sequences without changing phase or radius mechanics.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled component triplets detached from the document arrays, with stock colors resolved from reviewed definitions where available.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, incorrect sequence lengths, null colors, or channels outside 0..31.</exception>
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

    /// <summary>Serializes the editable sequences to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">The two complete RGB5 color sequences to serialize; the writer does not retain their collections.</param>
    /// <returns>Validated JSON bytes for <see cref="PowerBombFixedColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, sequence-length, or RGB5-channel validation.</exception>
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

/// <summary>Editable five-bit fixed-color component sequences for bank-$88 explosion color math; entries are not CGRAM palette words or COLDATA command bytes.</summary>
public sealed record PowerBombFixedColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="PowerBombFixedColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Sixteen nonnull RGB5 triplets corresponding to $88:9079, indexed by the 16-bit pre-explosion radius shifted right eleven bits and masked to 0..15.</summary>
    public required PaletteRgb5[] PreExplosion { get; init; }
    /// <summary>Thirty-two nonnull RGB5 triplets corresponding to $88:8D85, indexed by the 16-bit explosion radius shifted right eleven bits, not by elapsed gameplay updates.</summary>
    public required PaletteRgb5[] Explosion { get; init; }
}

/// <summary>Installed-resource identity, schema revision, and native sequence dimensions for explosion fixed-color artwork.</summary>
public static class PowerBombFixedColorFormat
{
    /// <summary>JSON resource filename containing the pre-explosion and explosion RGB5 sequences.</summary>
    public const string FileName = "power-bomb-fixed-colors.json";
    /// <summary>Supported schema revision, one, fixing the sixteen- and thirty-two-entry sequence dimensions.</summary>
    public const int Version = 1;

    /// <summary>Native bank-$88 source address for one authored RGB5 sequence.</summary>
    public static int SourceAddress(PowerBombFixedColorSequence sequence) => sequence switch
    {
        PowerBombFixedColorSequence.PreExplosion => SamusPaletteRomData.PowerBomb.PreExplosionColors,
        PowerBombFixedColorSequence.Explosion => SamusPaletteRomData.PowerBomb.ExplosionColors,
        _ => throw new ArgumentOutOfRangeException(nameof(sequence)),
    };

    /// <summary>Returns the number of radius-addressable RGB5 triplets in the selected native sequence.</summary>
    /// <param name="sequence">Pre-explosion or explosion artwork domain; this is not an animation phase or composable bit mask.</param>
    /// <returns>Sixteen for pre-explosion or thirty-two for explosion, defining the exclusive upper index bound.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The sequence value is not defined.</exception>
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
