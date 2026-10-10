using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>One editable sixteen-color RGB5 OBJ palette; selection and animation remain engine-owned.</summary>
public sealed class EnemyPaletteSheet
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("EnemyPaletteSheet-v1", content =>
        {
            content.AppendColors("colors", colors);
        });

    /// <summary>Sixteen RGB5 colors in a complete four-bit OBJ palette, including its color-zero entry.</summary>
    public const int ColorCount = 16;
    private readonly Bgr555[] colors;
    private EnemyPaletteSheet(Bgr555[] colors) => this.colors = colors;

    /// <summary>Loads a version-one palette document with exactly sixteen non-null RGB5 colors, rejecting duplicate or unknown properties and channels outside 0..31; JSON property names are matched case-insensitively.</summary>
    /// <param name="json">UTF-8 JSON stream containing the selected enemy palette in color-index order.</param>
    /// <returns>An immutable sheet owning the compiled sixteen RGB5 words.</returns>
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
        var compiled = new Bgr555[ColorCount];
        for (int i = 0; i < ColorCount; i++)
        {
            PaletteRgb5? color = document.Colors[i];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException($"Enemy palette color {i} requires RGB channels in 0..31.");
            compiled[i] = color.ToBgr555();
        }
        return new EnemyPaletteSheet(compiled);
    }

    /// <summary>Serializes a palette document as indented camel-case UTF-8 JSON and validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Version-one document containing sixteen valid RGB5 colors.</param>
    /// <returns>Validated enemy-palette JSON bytes.</returns>
    public static byte[] Write(EnemyPaletteSheetDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Copies all sixteen selected colors, including color zero, into consecutive current CGRAM entries; the room graphics-set owner selects the destination and controls palette animation.</summary>
    /// <param name="cgram">Destination color memory to update.</param>
    /// <param name="destinationColor">First CGRAM color-entry index, from 0 through 240, not a byte offset; no sixteen-color alignment is required by this transfer.</param>
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

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.OrdinalIgnoreCase,
            name => new InvalidDataException($"Duplicate enemy palette property {name}."));
}

/// <summary>Editable JSON schema for one enemy's sixteen-color palette; room selection, destination assignment, and animation are separate engine behavior.</summary>
public sealed record EnemyPaletteSheetDocument
{
    /// <summary>Schema revision, which must be 1 for this loader.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly sixteen non-null RGB5 colors in native palette order, with each red, green, and blue channel in 0..31.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}
