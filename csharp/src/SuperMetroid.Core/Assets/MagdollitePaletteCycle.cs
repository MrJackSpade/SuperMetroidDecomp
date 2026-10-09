using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Editable RGB5 colors for Magdollite's four-frame OBJ palette cycle.
/// The enemy hook owns frame order, eight-tick cadence, and destination selection.
/// </summary>
public sealed class MagdollitePaletteCycle
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("MagdollitePaletteCycle-v1", content =>
        {
            content.Append("frames", MagdollitePaletteRomData.FrameCount);
            Span<ushort> row = stackalloc ushort[MagdollitePaletteRomData.AnimatedColorCount];
            for (int frame = 0; frame < MagdollitePaletteRomData.FrameCount; frame++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = Resolve(frame, color);
                content.AppendWords("row", row);
            }
        });

    // The four independent glow colors remain required artwork under issue1165.
    /// <summary>Stores the first cycle row's four packed glow colors, from which unedited rows are rotated.</summary>
    private readonly ushort[] colors;
    /// <summary>Stores later-row cells that differ from the native left rotation of <see cref="colors"/>, keyed by flattened row and color index.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>
    /// Palette_Magdollite_Glow_0..3 at $A8:AC2E-AC34 rotates left one color per
    /// phase in the four native OBJ rows. Preserve arbitrary edited later rows
    /// as deviations from that rotation, without rebuilding the native table.
    /// </summary>
    private MagdollitePaletteCycle(ushort[][] frames)
    {
        colors = frames[0];
        for (int frame = 1; frame < frames.Length; frame++)
        for (int color = 0; color < colors.Length; color++)
            if (frames[frame][color] != colors[(color + frame) % colors.Length])
                edits.Add(frame * colors.Length + color, frames[frame][color]);
    }

    /// <summary>Applies the cycle document's camel-case naming, strict-member, and readable-output JSON conventions.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Returns one selected glow color, using the first row's native left rotation unless the requested later-row cell was independently edited.</summary>
    /// <param name="frame">Zero-based palette-cycle row 0..3, not an elapsed-update count.</param>
    /// <param name="color">Zero-based glow color 0..3, corresponding to OBJ palette colors nine through twelve.</param>
    /// <returns>Packed SNES BGR555 color from the selected row.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or color index is outside 0..3.</exception>
    public ushort Resolve(int frame, int color)
    {
        if ((uint)frame >= MagdollitePaletteRomData.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        if ((uint)color >= colors.Length)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(frame * colors.Length + color, out ushort edited)
            ? edited : colors[(color + frame) % colors.Length];
    }

    /// <summary>Writes the selected four-color row without advancing cycle timing or altering the other colors in Magdollite's OBJ palette.</summary>
    /// <param name="cgram">CGRAM receiving the selected packed colors.</param>
    /// <param name="frame">Palette-cycle row 0..3 chosen by the enemy's graphics-drawn hook.</param>
    /// <param name="destination">First CGRAM color-word index 0..252; the native hook supplies its selected OBJ palette base plus nine.</param>
    /// <exception cref="ArgumentNullException">The CGRAM target is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The row index or four-color destination range is invalid.</exception>
    public void ApplyFrame(SnesCgram cgram, int frame, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (destination < 0 ||
            destination + MagdollitePaletteRomData.AnimatedColorCount > SnesCgram.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int color = 0; color < MagdollitePaletteRomData.AnimatedColorCount; color++)
            cgram.SetColor(destination + color, Resolve(frame, color));
    }

    /// <summary>Validates and compiles the four RGB5 glow rows, preserving independent edits rather than requiring all later rows to rotate the first.</summary>
    /// <param name="json">UTF-8 JSON source consumed from its current position and left open.</param>
    /// <returns>Compiled palette-cycle colors detached from the mutable document arrays.</returns>
    /// <exception cref="ArgumentNullException">The source stream is null.</exception>
    /// <exception cref="InvalidDataException">The JSON contains duplicate or unknown properties, an unsupported version, dimensions other than four rows of four colors, null colors, or channels outside 0..31.</exception>
    public static MagdollitePaletteCycle Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        MagdollitePaletteCycleDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<MagdollitePaletteCycleDocument>(JsonOptions)
                ?? throw new InvalidDataException("Magdollite palette cycle JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Magdollite palette cycle JSON.", error);
        }

        if (document.Version != MagdollitePaletteCycleFormat.Version ||
            document.Frames is null ||
            document.Frames.Length != MagdollitePaletteRomData.FrameCount)
            throw new InvalidDataException("Magdollite palette cycle requires four version-one frames.");

        var compiled = new ushort[MagdollitePaletteRomData.FrameCount][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? source = document.Frames[frame];
            if (source is null || source.Length != MagdollitePaletteRomData.AnimatedColorCount)
                throw new InvalidDataException($"Magdollite palette frame {frame} requires four RGB5 colors.");
            compiled[frame] = new ushort[source.Length];
            for (int color = 0; color < source.Length; color++)
            {
                PaletteRgb5? rgb = source[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException(
                        $"Magdollite palette frame {frame} color {color} requires RGB5 channels 0..31.");
                compiled[frame][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
            }
        }
        return new MagdollitePaletteCycle(compiled);
    }

    /// <summary>Serializes the editable glow cycle to indented camel-case UTF-8 JSON and validates the resulting bytes through <see cref="Load"/>.</summary>
    /// <param name="document">Four complete RGB5 glow rows to serialize; their arrays are not retained.</param>
    /// <returns>Validated JSON bytes for <see cref="MagdollitePaletteCycleFormat.FileName"/>.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails schema, row-dimension, or RGB5-channel validation.</exception>
    public static byte[] Write(MagdollitePaletteCycleDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Rejects repeated property names before deserializing a cycle document.</summary>
    /// <param name="value">The parsed JSON value whose object properties are checked.</param>
    /// <exception cref="InvalidDataException">A property name occurs more than once in an object.</exception>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Magdollite palette property {name}."));
}

/// <summary>Editable four-row Magdollite glow schema, containing only animated OBJ colors rather than the complete sixteen-color native palette rows.</summary>
public sealed record MagdollitePaletteCycleDocument
{
    /// <summary>Schema revision; loading requires version one from <see cref="MagdollitePaletteCycleFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Four ordered rows of four nonnull RGB5 colors, each channel 0..31, corresponding to colors 9..12 of the native rows beginning at $A8:AC1C; stock rows rotate the $AC2E-$AC34 glow colors left.</summary>
    public required PaletteRgb5[][] Frames { get; init; }
}

/// <summary>Installed-resource identity and schema revision for Magdollite's four-color glow artwork.</summary>
public static class MagdollitePaletteCycleFormat
{
    /// <summary>JSON resource filename containing the four editable glow-cycle rows.</summary>
    public const string FileName = "magdollite-palette-cycle.json";
    /// <summary>Supported schema revision, one, fixing four rows of four RGB5 colors without encoding animation cadence.</summary>
    public const int Version = 1;
}
