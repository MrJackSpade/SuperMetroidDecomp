using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The Ceres approach's full, editable 256-color CGRAM image.</summary>
public sealed class CeresFlightPalette
{
    // Independent painted colors remain unresolved inputs. Only repeated sections
    // and the endpoint-defined ramp are calculated; edited slots stay independent.
    private readonly Dictionary<int, ushort> colors = [];
    private readonly Dictionary<int, ushort> edits = [];

    private CeresFlightPalette(ushort[] supplied)
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
    public ushort ColorAt(int index)
    {
        if ((uint)index >= SnesCgram.ColorCount) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(index, out ushort edited) ? edited : Calculate(index);
    }

    /// <summary>Writes serialization/hash bytes without retaining a runtime palette image.</summary>
    public void CopyTransferTo(Span<byte> destination)
    {
        if (destination.Length < SnesCgram.ByteCount) throw new ArgumentException("Palette output requires 512 bytes.", nameof(destination));
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice(index * sizeof(ushort)), ColorAt(index));
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
    private static bool IsRampInterior(int index) => index is > 0x61 and < 0x68;

    private ushort Calculate(int index)
    {
        int source = SharedIndex(index);
        if (!IsRampInterior(source)) return colors[source];
        int step = source - 0x61;
        ushort first = colors[0x61], last = colors[0x68];
        int result = 0;
        for (int shift = 0; shift < 15; shift += 5)
        {
            int start = first >> shift & 31, end = last >> shift & 31;
            result |= ((start * (7 - step) + end * step + 3) / 7) << shift;
        }
        return (ushort)result;
    }

    public static CeresFlightPalette Load(Stream json)
    {
        CeresFlightPaletteDocument document = JsonAssetDocument.Read<CeresFlightPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "Ceres flight palette");
        if (document.Version != CeresFlightPaletteFormat.Version ||
            document.Colors is not { Length: SnesCgram.ColorCount })
            throw new InvalidDataException("Ceres flight palette requires 256 RGB5 colors.");

        var native = new ushort[SnesCgram.ColorCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ceres flight palette color {index} requires red, green and blue in 0..31.");
            native[index] = (ushort)(color.Red | color.Green << 5 | color.Blue << 10);
        }
        return new CeresFlightPalette(native);
    }

    public static void Write(Stream json, CeresFlightPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    public void LoadTo(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int index = 0; index < SnesCgram.ColorCount; index++) cgram.SetColor(index, ColorAt(index));
    }
}

public sealed record CeresFlightPaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Resource identity and schema for the native Ceres approach palette.</summary>
public static class CeresFlightPaletteFormat
{
    public const int Version = 1;
    public const string FileName = "ceres-flight-palette.json";
}
