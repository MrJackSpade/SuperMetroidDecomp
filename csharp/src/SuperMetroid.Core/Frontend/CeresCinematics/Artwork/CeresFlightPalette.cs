using SuperMetroid.Core.Rendering;
using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>The Ceres approach's full, editable 256 screen colours.</summary>
public sealed class CeresFlightPalette
{
    /// <summary>Independent painted colors; only repeated sections and endpoint-defined ramps are calculated.</summary>
    internal readonly Dictionary<int, Bgr555> colors = [];
    /// <summary>Supplied slots that differ from those calculated relationships remain independent.</summary>
    internal readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Separates the supplied CGRAM image into independent colors and edited exceptions.</summary>
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
        if ((uint)index >= ScreenPalette.ColorCount) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(index, out Bgr555 edited) ? edited : Calculate(index);
    }

    /// <summary>Writes the colours as little-endian words for the content identity, without retaining them.</summary>
    public void CopyColorWordsTo(Span<byte> destination)
    {
        if (destination.Length < ColorWordBytes) throw new ArgumentException("Palette output requires 512 bytes.", nameof(destination));
        for (int index = 0; index < ScreenPalette.ColorCount; index++)
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
    private static bool IsRampInterior(int index) => index is > RampStart and < RampEnd;

    /// <summary>The CGRAM slot of the ramp's first endpoint, palette 6 color 1.</summary>
    private const int RampStart = 6 * 16 + 1;

    /// <summary>The CGRAM slot of the ramp's last endpoint, palette 6 color 8.</summary>
    private const int RampEnd = 6 * 16 + 8;

    /// <summary>Calculates repeated palette slots or interpolated ramp colors from independent inputs.</summary>
    private Bgr555 Calculate(int index)
    {
        int source = SharedIndex(index);
        if (!IsRampInterior(source)) return colors[source];
        int step = source - RampStart;
        Bgr555 first = colors[RampStart], last = colors[RampEnd];
        return first.Zip(last, (_, start, end) => (start * (7 - step) + end * step + 3) / 7);
    }

    /// <summary>Loads the supported palette schema with exactly 256 non-null RGB5 colors, rejecting malformed or duplicate JSON properties and channels outside 0..31; supplied edits remain independent of calculated stock repeats and ramps.</summary>
    /// <param name="json">Caller-owned UTF-8 JSON stream containing the complete Ceres approach palette in CGRAM order.</param>
    /// <returns>The immutable selected palette corresponding to compiled <c>Palettes_SpaceGunshipCeres</c> at <c>$8C:E5E9</c>.</returns>
    public static CeresFlightPalette Load(Stream json)
    {
        CeresFlightPaletteDocument document = JsonAssetDocument.Read<CeresFlightPaletteDocument>(
            json, ArtworkJson.Options, "Ceres flight palette");
        if (document.Version != CeresFlightPaletteFormat.Version ||
            document.Colors is not { Length: ScreenPalette.ColorCount })
            throw new InvalidDataException("Ceres flight palette requires 256 RGB5 colors.");

        var compiled = new Bgr555[ScreenPalette.ColorCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ceres flight palette color {index} requires red, green and blue in 0..31.");
            compiled[index] = color.ToBgr555();
        }
        return new CeresFlightPalette(compiled);
    }

    /// <summary>Serializes a palette document with the presentation JSON options and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream for the validated UTF-8 JSON.</param>
    /// <param name="document">Document containing the supported version and all 256 RGB5 colors.</param>
    public static void Write(Stream json, CeresFlightPaletteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }

    /// <summary>The bytes of the colours as little-endian words.</summary>
    public const int ColorWordBytes = ScreenPalette.ColorCount * Bgr555.ByteCount;

    /// <summary>Replaces every colour of the screen with the selected approach palette; cinematic timing, brightness, and fades remain the caller's responsibility.</summary>
    /// <param name="colours">The screen's colours.</param>
    public void LoadTo(ScreenPalette colours)
    {
        Ensure.NotNull(colours);
        Span<Bgr555> image = stackalloc Bgr555[ScreenPalette.ColorCount];
        for (int index = 0; index < image.Length; index++)
            image[index] = ColorAt(index);
        colours.Load(image);
    }
}
