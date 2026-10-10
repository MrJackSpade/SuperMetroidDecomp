using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The Ceres approach's full, editable 256-color CGRAM image.</summary>
public sealed class CeresFlightPalette
{
    // Independent painted colors remain unresolved inputs. Only repeated sections
    // and the endpoint-defined ramp are calculated; edited slots stay independent.
    private readonly Dictionary<int, Bgr555> colors = [];
    private readonly Dictionary<int, Bgr555> edits = [];

    private CeresFlightPalette(Bgr555[] supplied)
    {
        for (int index = 0; index < supplied.Length; index++)
        {
            int source = SharedIndex(index);
            if (!IsRampInterior(source)) colors.TryAdd(source, supplied[source]);
        }
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != Calculate(index)) edits.Add(index, supplied[index]);
    }

    /// <summary>Selected BGR555 color, including independent resource edits.</summary>
    public Bgr555 ColorAt(int index)
    {
        if ((uint)index >= SnesCgram.ColorCount) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(index, out Bgr555 edited) ? edited : Calculate(index);
    }

    /// <summary>Writes serialization/hash bytes without retaining a runtime palette image.</summary>
    public void CopyTransferTo(Span<byte> destination)
    {
        if (destination.Length < SnesCgram.ByteCount) throw new ArgumentException("Palette output requires 512 bytes.", nameof(destination));
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice(index * Bgr555.ByteCount), ColorAt(index).ToWord());
    }

    /// <summary>
    /// $8C:E5E9: lower/upper palette halves share all non-backdrop slots except
    /// row D and row F's five hue colors. Rows 1..3 repeat the neutral fill.
    /// These are palette-slot relationships, not exemptions for their painted inputs.
    /// </summary>
    private static int SharedIndex(int index)
    {
        int column = index % 16;
        if (column == 0) return index == 0 ? 0 : index < 128 ? 16 : 128;
        if (index >= 128 && index / 16 != 13 && !(index / 16 == 15 && column is >= 9 and <= 13))
            index -= 128;
        if (index < 64 && (index >= 16 || column >= 4)) return 2;
        return index;
    }

    /// <summary>$8C:E6AB-E6BA and E7AB-E7BA: eight colors interpolate RGB5 endpoints with nearest-integer rounding.</summary>
    private static bool IsRampInterior(int index) => index is > 0x61 and < 0x68; // magic-number-audit: allow(SramOffset) - CGRAM interpolation slots, not SRAM offsets.

    private Bgr555 Calculate(int index)
    {
        int source = SharedIndex(index);
        if (!IsRampInterior(source)) return colors[source];
        int step = source - 0x61;
        Bgr555 first = colors[0x61], last = colors[0x68];
        return first.Zip(last, (_, start, end) => (start * (7 - step) + end * step + 3) / 7);
    }

    /// <summary>Loads the supported palette schema with exactly 256 non-null RGB5 colors, rejecting malformed or duplicate JSON properties and channels outside 0..31; supplied edits remain independent of calculated stock repeats and ramps.</summary>
    /// <param name="json">Caller-owned UTF-8 JSON stream containing the complete Ceres approach palette in CGRAM order.</param>
    /// <returns>The immutable selected palette corresponding to native <c>Palettes_SpaceGunshipCeres</c> at <c>$8C:E5E9</c>.</returns>
    public static CeresFlightPalette Load(Stream json)
    {
        CeresFlightPaletteDocument document = JsonAssetDocument.Read<CeresFlightPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "Ceres flight palette");
        if (document.Version != CeresFlightPaletteFormat.Version ||
            document.Colors is not { Length: SnesCgram.ColorCount })
            throw new InvalidDataException("Ceres flight palette requires 256 RGB5 colors.");

        var native = new Bgr555[SnesCgram.ColorCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ceres flight palette color {index} requires red, green and blue in 0..31.");
            native[index] = color.ToBgr555();
        }
        return new CeresFlightPalette(native);
    }

    /// <summary>Serializes a palette document with the presentation JSON options and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream for the validated UTF-8 JSON.</param>
    /// <param name="document">Document containing the supported version and all 256 RGB5 colors.</param>
    public static void Write(Stream json, CeresFlightPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>Replaces all 256 current CGRAM entries with the selected approach palette, including backdrop and OBJ colors; cinematic timing, brightness, and fades remain the caller's responsibility.</summary>
    /// <param name="cgram">Destination color memory to overwrite in color-entry order.</param>
    public void LoadTo(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < SnesCgram.ColorCount; index++) cgram.SetColor(index, ColorAt(index));
    }
}

/// <summary>Editable JSON schema for the complete Ceres approach palette, also reused by the destruction scene; PNG transport palettes do not supply these rendered colors.</summary>
public sealed record CeresFlightPaletteDocument
{
    /// <summary>Schema revision, which must equal <see cref="CeresFlightPaletteFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 non-null colors with channels in 0..31, ordered by CGRAM index and matching native source words at <c>$8C:E5E9 + 2 * index</c>.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Resource identity and schema for the native Ceres approach palette.</summary>
public static class CeresFlightPaletteFormat
{
    /// <summary>Supported revision of the full 256-color RGB5 palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Asset filename for the independently editable Ceres approach color image.</summary>
    public const string FileName = "ceres-flight-palette.json";
}
