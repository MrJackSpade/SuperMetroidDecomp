using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable RGB5 visor colors shared by X-ray and room palette cycling.</summary>
public sealed class SamusVisorColorCatalog
{
    /// <summary>Only color ordinals whose installed RGB5 value differs from the calculated stock value.</summary>
    private readonly Dictionary<int, ushort> colors;

    /// <summary>Stores the compiled independent color edits used by visor palette resolution.</summary>
    /// <param name="colors">Changed color ordinals mapped to packed BGR555 values.</param>
    private SamusVisorColorCatalog(Dictionary<int, ushort> colors) => this.colors = colors;

    /// <summary>Strict camel-case JSON settings shared by visor-color deserialization and serialization.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Validates the six RGB5 visor colors and compiles independent edits relative to the reviewed $9B:A3C0 widening/cycling definitions.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled visor colors detached from the document array, without changing X-ray or room-cycle timing.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, a length other than six, null colors, or RGB5 channels outside 0..31.</exception>
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

        var compiled = new Dictionary<int, ushort>();
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 ||
                (uint)color.Green > 31 || (uint)color.Blue > 31)
                throw new InvalidDataException($"Samus visor color {index} requires RGB components from zero through 31.");
            ushort packed = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
            if (packed != SamusVisorColorDefinitions.Color(index)) compiled.Add(index, packed);
        }
        return new(compiled);
    }

    /// <summary>Serializes the editable visor colors to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">All six RGB5 visor colors to serialize; the array is not retained.</param>
    /// <returns>Validated JSON bytes for <see cref="SamusVisorColorFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, color-count, or RGB5-channel validation.</exception>
    public static byte[] Write(SamusVisorColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>
    /// Resolves only the six installed even offsets. Corrupted or adjacent offsets
    /// return false so the caller can enforce its bounded palette contract.
    /// </summary>
    public bool TryResolveByteOffset(int byteOffset, out ushort color)
    {
        if ((byteOffset & 1) == 0 && (uint)(byteOffset >> 1) < SamusVisorColorFormat.ColorCount)
        {
            color = Resolve(byteOffset >> 1);
            return true;
        }
        color = 0;
        return false;
    }

    /// <summary>Returns a selected visor color for the native owners that write Samus OBJ palette-four color four, CGRAM entry 196.</summary>
    /// <param name="index">Color ordinal 0..5, not a byte offset: 0..2 are X-ray widening colors and 3..5 are the steady X-ray/room-backdrop cycle.</param>
    /// <returns>Packed SNES BGR555 word from the independent edit or calculated stock definition.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The color ordinal is outside 0..5.</exception>
    public ushort Resolve(int index)
    {
        if ((uint)index >= SamusVisorColorFormat.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(index));
        return colors.TryGetValue(index, out ushort color) ? color : SamusVisorColorDefinitions.Color(index);
    }

    /// <summary>Rejects duplicate object properties before deserialization could silently discard them.</summary>
    /// <param name="value">Parsed JSON root to check for repeated property names.</param>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Samus visor color property {name}."));
}

/// <summary>Editable RGB5 schema for the six shared X-ray and animated-room visor colors, excluding their native timing and byte-offset state.</summary>
public sealed record SamusVisorColorDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="SamusVisorColorFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Six nonnull RGB5 colors with channels 0..31 in $9B:A3C0 word order: three widening colors followed by three full-X-ray/room-cycle colors.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Installed-resource identity, schema revision, and native six-word extent for editable Samus visor colors.</summary>
public static class SamusVisorColorFormat
{
    /// <summary>JSON resource filename containing the six editable visor RGB5 colors.</summary>
    public const string FileName = "samus-visor-colors.json";
    /// <summary>Supported schema revision, one, requiring exactly the six authored visor words.</summary>
    public const int Version = 1;
    /// <summary>The six authored BGR555 words at $9B:A3C0, including X-ray widening and room-cycle colors.</summary>
    public const int SourceAddress = SamusPaletteRomData.Visor.Colors;
    /// <summary>Six selected color words, corresponding to valid native even byte offsets 0, 2, 4, 6, 8, and 10.</summary>
    public const int ColorCount = 6;
}
