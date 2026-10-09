using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The opening narration's full, editable native-precision CGRAM image.</summary>
public sealed class IntroCinematicPalette
{
    /// <summary>Compiled row resolvers; empty when the input matches the canonical stock palette.</summary>
    private readonly PaletteRow[] rows;

    /// <summary>Creates row resolvers from the complete little-endian BGR555 CGRAM image.</summary>
    /// <param name="nativeBytes">The validated 512-byte palette transfer image.</param>
    private IntroCinematicPalette(byte[] nativeBytes)
    {
        if (IntroCinematicPaintDefinitions.Matches(nativeBytes)) { rows = []; return; }
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
            for (int row = 0; row < SnesCgram.ColorCount / IntroCinematicPaletteFormat.ColorsPerRow; row++)
            for (int color = 0; color < IntroCinematicPaletteFormat.ColorsPerRow; color++)
                BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(2 * (row * IntroCinematicPaletteFormat.ColorsPerRow + color)), rows.Length == 0 ? IntroCinematicPaintDefinitions.Color(row, color) : rows[row].Resolve(color));
            return output;
        }
    }

    /// <summary>Replaces all 256 CGRAM colors with the installed opening palette, corresponding to native $8C:E3E9-E5E8; does not advance or reset cinematic fade timers.</summary>
    /// <param name="cgram">Destination CGRAM image, receiving the complete $0200-byte BGR555 transfer.</param>
    public void LoadTo(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        cgram.LoadBytes(Transfer.Span);
    }

    /// <summary>Compact resolver for one sixteen-color row, retaining explicit values when no reviewed relationship matches.</summary>
    private sealed class PaletteRow
    {
        /// <summary>Base colors from slots zero and one: the background and the foreground repeated by flat rows.</summary>
        private readonly ushort background, foreground;
        /// <summary>Full authored row retained when its values cannot be represented by a recognized palette rule.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Whether visible entries follow the row's repeating neutral-shade cycle.</summary>
        private readonly bool cycle;
        /// <summary>Source row used to share the cross-fade's initial visible gradient.</summary>
        private readonly PaletteRow? sharedGradient;
        /// <summary>Cross-fade slot-zero and endpoint words retained while intermediate blue shades are derived.</summary>
        private readonly ushort[]? crossFadeInputs;

        /// <summary>Chooses a compact stock/shared representation or preserves all supplied row words verbatim.</summary>
        /// <param name="colors">The sixteen BGR555 entries in this CGRAM row.</param>
        /// <param name="allowCycle"><see langword="true"/> when this row may use the reviewed neutral cycle rule.</param>
        /// <param name="crossFadeSource">Optional row whose visible gradient may be shared by a matching cross-fade row.</param>
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
        /// <summary>Checks whether the row's first gradient and blue fade follow the selected shared cross-fade rules.</summary>
        /// <param name="colors">Candidate row's sixteen BGR555 entries.</param>
        /// <param name="source">Palette row expected to supply visible entries one through eight.</param>
        /// <returns><see langword="true"/> when the checked entries match the shared gradient and stepped blue sequence.</returns>
        private static bool MatchesCrossFade(ushort[] colors, PaletteRow source)
        {
            for (int color = 1; color <= 8; color++)
                if (colors[color] != source.Resolve(color)) return false;
            int blue = colors[9] >> 10;
            if ((colors[9] & 0x3ff) != 0 || blue < 3 * IntroCinematicPaletteFormat.CrossFadeBlueStep) return false;
            for (int shade = 1; shade < 4; shade++)
                if (colors[9 + shade] != (blue - shade * IntroCinematicPaletteFormat.CrossFadeBlueStep) << 10) return false;
            return true;
        }
        /// <summary>Resolves one row entry from its retained source values or the row's recognized compact rule.</summary>
        /// <param name="color">CGRAM slot within this row.</param>
        /// <returns>The selected BGR555 word.</returns>
        internal ushort Resolve(int color)
        {
            if (crossFadeInputs is not null)
            {
                if (color == 0) return crossFadeInputs[0];
                if (color <= 8) return sharedGradient!.Resolve(color);
                if (color <= 12) return (ushort)(((crossFadeInputs[1] >> 10) -
                    (color - 9) * IntroCinematicPaletteFormat.CrossFadeBlueStep) << 10);
                return crossFadeInputs[color - 11];
            }
            return supplied is not null ? supplied[color] :
                color == 0 ? background : cycle ? CycleColor(color) : foreground;
        }
        /// <summary>Applies the repeating per-channel decrement used by the neutral material cycle.</summary>
        /// <param name="color">Visible slot whose cycle phase determines the decrement.</param>
        /// <returns>The cycled BGR555 color, with each channel clamped at zero.</returns>
        private ushort CycleColor(int color)
        {
            int decrease = (color - 1) % IntroCinematicPaletteFormat.NeutralCycleLength *
                IntroCinematicPaletteFormat.NeutralCycleStep;
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= Math.Max(0, (foreground >> shift & 31) - decrease) << shift;
            return (ushort)result;
        }
    }
    /// <summary>Validates 256 ordered RGB5 colors and compiles their little-endian BGR555 image, calculating stock/shared shade relationships only when supplied colors match them.</summary>
    /// <param name="json">Caller-owned stream containing the supported editable opening-palette document.</param>
    /// <returns>The installed palette used for opening setup and subsequent cinematic fade targets, without changing fade scheduling.</returns>
    /// <exception cref="InvalidDataException">The schema version, complete color count, or RGB components are invalid.</exception>
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

    /// <summary>Serializes the complete opening palette as UTF-8 JSON and validates it through <see cref="Load"/> before writing any destination bytes.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete ordered RGB5 palette to validate and serialize.</param>
    public static void Write(Stream json, IntroCinematicPaletteDocument document)
    {
        ArgumentNullException.ThrowIfNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable full-CGRAM color schema for opening narration, portrait, object, and cross-fade inks; scene phases and fade timing are not document content.</summary>
public sealed record IntroCinematicPaletteDocument
{
    /// <summary>Schema revision required to equal <see cref="IntroCinematicPaletteFormat.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly 256 non-null RGB5 colors in CGRAM index order, including every sixteen-color row's slot zero; each red, green, and blue channel is 0..31.</summary>
    public required PaletteRgb5[] Colors { get; init; }
}

/// <summary>Resource identity and schema for the opening narration palette.</summary>
public static class IntroCinematicPaletteFormat
{
    /// <summary>Supported revision of the complete 256-color RGB5 opening-palette schema.</summary>
    public const int Version = 1;
    /// <summary>Native CGRAM palette-row width in RGB5 words.</summary>
    internal const int ColorsPerRow = 16;
    /// <summary>$8C:E4E9-E508: first object palette repeats a neutral ramp after its background slot.</summary>
    internal const int NeutralCycleRow = 8;
    /// <summary>$8C:E4EB-E4F2: reviewed four-shade neutral material cycle.</summary>
    internal const int NeutralCycleLength = IntroCinematicPaintDefinitions.WhiteCycleLength;
    /// <summary>$8C:E4EB-E508: reviewed unit decrement in every RGB5 channel.</summary>
    internal const int NeutralCycleStep = IntroCinematicPaintDefinitions.WhiteCycleStep;
    /// <summary>$8C:E5A9-E5C8, Palettes_Intro_CrossFade; its first eight visible inks repeat palette2.</summary>
    internal const int CrossFadeRow = 14;
    /// <summary>$8C:E42B-E43A supplies the eight-color gradient also copied atE5AB-E5BA.</summary>
    internal const int SharedCrossFadeSourceRow = 2;
    /// <summary>$8C:E5BB-E5C2: reviewed blue endpoints31/4 derive their equal three-interval decrement.</summary>
    internal const int CrossFadeBlueStep = IntroCinematicPaintDefinitions.CrossfadeBlueStep;
    /// <summary>Editable JSON filename for the opening narration's complete native-precision CGRAM palette.</summary>
    public const string FileName = "intro-narration-palette.json";
}
