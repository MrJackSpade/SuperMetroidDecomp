using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable RGB5 colors for Power Bomb, Crystal Flash and Ceres explosions.</summary>
public sealed class PowerBombFixedColorCatalog
{
    /// <summary>Authored pre-explosion colors that differ from the calculated sixteen-entry stock sequence.</summary>
    internal readonly Dictionary<int, (byte Red, byte Green, byte Blue)> preExplosion;
    /// <summary>Authored explosion colors, including later entries without a calculated stock definition.</summary>
    internal readonly Dictionary<int, (byte Red, byte Green, byte Blue)> explosion;

    /// <summary>Retains validated component edits separately for the two radius-selected sequences.</summary>
    private PowerBombFixedColorCatalog(
        Dictionary<int, (byte Red, byte Green, byte Blue)> preExplosion,
        Dictionary<int, (byte Red, byte Green, byte Blue)> explosion)
    {
        this.preExplosion = preExplosion;
        this.explosion = explosion;
    }

    /// <summary>Writes camel-case resource JSON while rejecting unmapped members during load.</summary>
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
        Ensure.NotNull(json);
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

    /// <summary>Checks RGB5 channels and keeps entries that differ from the calculated stock sequence.</summary>
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

    /// <summary>Rejects duplicate JSON properties before deserialization can hide a supplied color.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Power Bomb fixed-color property {name}."));
}
