using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The opening narration's full, editable native-precision CGRAM image.</summary>
public sealed class IntroCinematicPalette
{
    private readonly PaletteRow[] rows;

    private IntroCinematicPalette(byte[] nativeBytes)
    {
        rows = new PaletteRow[SnesCgram.ColorCount / IntroCinematicPaletteFormat.ColorsPerRow];
        for (int row = 0; row < rows.Length; row++)
        {
            var colors = new ushort[IntroCinematicPaletteFormat.ColorsPerRow];
            for (int color = 0; color < colors.Length; color++)
                colors[color] = BinaryPrimitives.ReadUInt16LittleEndian(nativeBytes.AsSpan(2 * (row * colors.Length + color)));
            rows[row] = new PaletteRow(colors, row == IntroCinematicPaletteFormat.NeutralCycleRow,
                row == IntroCinematicPaletteFormat.CrossFadeRow ? rows[IntroCinematicPaletteFormat.SharedCrossFadeSourceRow] : null);
        }
    }

    /// <summary>Exact BGR555 bytes copied by the cartridge's opening setup.</summary>
    public ReadOnlyMemory<byte> Transfer
    {
        get
        {
            var output = new byte[SnesCgram.ByteCount];
            for (int row = 0; row < rows.Length; row++)
            for (int color = 0; color < IntroCinematicPaletteFormat.ColorsPerRow; color++)
                BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(2 * (row * IntroCinematicPaletteFormat.ColorsPerRow + color)), rows[row].Resolve(color));
            return output;
        }
    }

    public void LoadTo(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        cgram.LoadBytes(Transfer.Span);
    }

    private sealed class PaletteRow
    {
        private readonly ushort background, foreground;
        private readonly ushort[]? supplied;
        private readonly bool cycle;
        private readonly PaletteRow? sharedGradient;
        private readonly ushort[]? crossFadeInputs;
        internal PaletteRow(ushort[] colors, bool allowCycle, PaletteRow? crossFadeSource)
        {
            if (crossFadeSource is not null && MatchesCrossFade(colors, crossFadeSource))
            {
                sharedGradient = crossFadeSource;
                crossFadeInputs = [colors[0], colors[9], colors[13], colors[14], colors[15]];
                return;
            }
            background = colors[0];
            foreground = colors[1];
            bool flat = colors.AsSpan(1).IndexOfAnyExcept(foreground) < 0;
            if (flat) return;
            cycle = allowCycle;
            for (int color = 1; color < colors.Length; color++)
            {
                if (cycle && colors[color] == CycleColor(color)) continue;
                supplied = colors;
                background = foreground = 0;
                break;
            }
        }
        private static bool MatchesCrossFade(ushort[] colors, PaletteRow source)
        {
            for (int color = 1; color <= 8; color++)
                if (colors[color] != source.Resolve(color)) return false;
            int blue = colors[9] >> 10;
            if ((colors[9] & 0x3ff) != 0 || blue < 3 * IntroCinematicPaletteFormat.UnresolvedCrossFadeBlueStep) return false;
            for (int shade = 1; shade < 4; shade++)
                if (colors[9 + shade] != (blue - shade * IntroCinematicPaletteFormat.UnresolvedCrossFadeBlueStep) << 10) return false;
            return true;
        }
        internal ushort Resolve(int color)
        {
            if (crossFadeInputs is not null)
            {
                if (color == 0) return crossFadeInputs[0];
                if (color <= 8) return sharedGradient!.Resolve(color);
                if (color <= 12) return (ushort)(((crossFadeInputs[1] >> 10) -
                    (color - 9) * IntroCinematicPaletteFormat.UnresolvedCrossFadeBlueStep) << 10);
                return crossFadeInputs[color - 11];
            }
            return supplied is not null ? supplied[color] :
                color == 0 ? background : cycle ? CycleColor(color) : foreground;
        }
        private ushort CycleColor(int color)
        {
            int decrease = (color - 1) % IntroCinematicPaletteFormat.UnresolvedNeutralCycleLength *
                IntroCinematicPaletteFormat.UnresolvedNeutralCycleStep;
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= Math.Max(0, (foreground >> shift & 31) - decrease) << shift;
            return (ushort)result;
        }
    }
    public static IntroCinematicPalette Load(Stream json)
    {
        IntroCinematicPaletteDocument document = JsonAssetDocument.Read<IntroCinematicPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "opening palette");
        if (document.Version != IntroCinematicPaletteFormat.Version ||
            document.Colors is not { Length: SnesCgram.ColorCount })
            throw new InvalidDataException("Opening palette requires 256 RGB5 colors.");

        var native = new byte[SnesCgram.ByteCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Opening palette color {index} requires red, green and blue in 0..31.");
            BinaryPrimitives.WriteUInt16LittleEndian(native.AsSpan(index * sizeof(ushort)),
                (ushort)(color.Red | color.Green << 5 | color.Blue << 10));
        }
        return new IntroCinematicPalette(native);
    }

    public static void Write(Stream json, IntroCinematicPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record IntroCinematicPaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Resource identity and schema for the opening narration palette.</summary>
public static class IntroCinematicPaletteFormat
{
    public const int Version = 1;
    /// <summary>Native CGRAM palette-row width in RGB5 words.</summary>
    internal const int ColorsPerRow = 16;
    /// <summary>$8C:E4E9-E508: first object palette repeats a neutral ramp after its background slot.</summary>
    internal const int NeutralCycleRow = 8;
    /// <summary>$8C:E4EB-E4F2: selected four-shade cycle; period remains required.</summary>
    internal const int UnresolvedNeutralCycleLength = 4;
    /// <summary>$8C:E4EB-E508: selected unit decrement in every RGB5 channel; magnitude remains required.</summary>
    internal const int UnresolvedNeutralCycleStep = 1;
    /// <summary>$8C:E5A9-E5C8, Palettes_Intro_CrossFade; its first eight visible inks repeat palette2.</summary>
    internal const int CrossFadeRow = 14;
    /// <summary>$8C:E42B-E43A supplies the eight-color gradient also copied atE5AB-E5BA.</summary>
    internal const int SharedCrossFadeSourceRow = 2;
    /// <summary>$8C:E5BB-E5C2: selected blue levels31/22/13/4; decrement9 remains required.</summary>
    internal const int UnresolvedCrossFadeBlueStep = 9;
    public const string FileName = "intro-narration-palette.json";
}
