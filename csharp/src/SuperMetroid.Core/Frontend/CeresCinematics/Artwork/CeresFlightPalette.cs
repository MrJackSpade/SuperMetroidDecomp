using SuperMetroid.Core.Rendering;
using System.Buffers.Binary;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>The Ceres approach's full, editable 256 screen colours.</summary>
public sealed class CeresFlightPalette
{
    /// <summary>Independent painted colors; only repeated sections and endpoint-defined ramps are calculated.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> colors = [];
    /// <summary>Supplied slots that differ from those calculated relationships remain independent.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> edits = [];

    /// <summary>Separates the supplied CGRAM image into independent colors and edited exceptions.</summary>
    internal CeresFlightPalette(RgbColor[] supplied)
    {
        for (int index = 0; index < supplied.Length; index++)
        {
            int source = SharedIndex(index);
            if (!IsRampInterior(source)) colors.TryAdd(new ColourCell(0, source), supplied[source]);
        }
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != Calculate(index)) edits.Add(new ColourCell(0, index), supplied[index]);
    }

    /// <summary>Selected BGR555 color, including independent resource edits.</summary>
    public RgbColor ColorAt(int index)
    {
        if ((uint)index >= ScreenPalette.ColorCount) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new ColourCell(0, index), out RgbColor edited) ? edited : Calculate(index);
    }

    /// <summary>Writes the colours as little-endian words for the content identity, without retaining them.</summary>
    public void CopyColorWordsTo(Span<byte> destination)
    {
        if (destination.Length < ColorWordBytes) throw new ArgumentException("Palette output requires 512 bytes.", nameof(destination));
        for (int index = 0; index < ScreenPalette.ColorCount; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(destination.Slice(index * RgbColor.ByteCount), ColorAt(index).ToWord());
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
    private RgbColor Calculate(int index)
    {
        int source = SharedIndex(index);
        if (!IsRampInterior(source)) return colors[new ColourCell(0, source)];
        int step = source - RampStart;
        RgbColor first = colors[new ColourCell(0, RampStart)], last = colors[new ColourCell(0, RampEnd)];
        return first.Zip(last, (_, start, end) => (start * (7 - step) + end * step + 3) / 7);
    }

    /// <summary>The bytes of the colours as little-endian words.</summary>
    public const int ColorWordBytes = ScreenPalette.ColorCount * RgbColor.ByteCount;

    /// <summary>Replaces every colour of the screen with the selected approach palette; cinematic timing, brightness, and fades remain the caller's responsibility.</summary>
    /// <param name="colours">The screen's colours.</param>
    public void LoadTo(ScreenPalette colours)
    {
        Ensure.NotNull(colours);
        Span<RgbColor> image = stackalloc RgbColor[ScreenPalette.ColorCount];
        for (int index = 0; index < image.Length; index++)
            image[index] = ColorAt(index);
        colours.Load(image);
    }
}
