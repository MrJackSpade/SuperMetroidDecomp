using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The opening narration's full, editable native-precision CGRAM image.</summary>
public sealed class IntroCinematicPalette
{
    /// <summary>Decoded sixteen-color rows; empty when the installed bytes match compiled stock paint.</summary>
    internal readonly PaletteRow[] rows;

    /// <summary>Retains exact supplied colors while recognizing stock, neutral-cycle, and shared cross-fade rows.</summary>
    [PpuLayoutBoundary("codec: native 512-byte CGRAM transfer image of the opening palette")]
    private IntroCinematicPalette(byte[] transfer)
    {
        if (IntroCinematicPaintDefinitions.Matches(transfer)) { rows = []; return; }
        rows = new PaletteRow[CgramPalettes.ColorCount / IntroCinematicPaletteFormat.ColorsPerRow];
        for (int row = 0; row < rows.Length; row++)
        {
            var colors = new Bgr555[IntroCinematicPaletteFormat.ColorsPerRow];
            for (int color = 0; color < colors.Length; color++)
                colors[color] = Bgr555.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(transfer.AsSpan(2 * (row * colors.Length + color))));
            bool neutralCycle = false;
            PaletteRow? crossFadeSource = null;
            if (Enum.IsDefined((IntroCinematicAnimatedRow)row))
            {
                switch ((IntroCinematicAnimatedRow)row)
                {
                    case IntroCinematicAnimatedRow.NeutralCycle:
                        neutralCycle = true;
                        break;
                    case IntroCinematicAnimatedRow.CrossFade:
                        crossFadeSource = rows[IntroCinematicPaletteFormat.SharedCrossFadeSourceRow];
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(IntroCinematicAnimatedRow)} {row}.");
                }
            }
            rows[row] = new PaletteRow(colors, neutralCycle, crossFadeSource);
        }
    }

    /// <summary>Exact BGR555 bytes copied by the cartridge's opening setup.</summary>
    [PpuLayoutBoundary("codec: native 512-byte CGRAM transfer image of the opening palette")]
    public ReadOnlyMemory<byte> Transfer
    {
        get
        {
            var output = new byte[CgramPalettes.ByteCount];
            for (int row = 0; row < CgramPalettes.ColorCount / IntroCinematicPaletteFormat.ColorsPerRow; row++)
            for (int color = 0; color < IntroCinematicPaletteFormat.ColorsPerRow; color++)
                BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(2 * (row * IntroCinematicPaletteFormat.ColorsPerRow + color)), (rows.Length == 0 ? IntroCinematicPaintDefinitions.Color(row, color) : rows[row].Resolve(color)).ToWord());
            return output;
        }
    }

    /// <summary>Replaces all 256 CGRAM colors with the installed opening palette, corresponding to native $8C:E3E9-E5E8; does not advance or reset cinematic fade timers.</summary>
    /// <param name="colours">The screen's colours, every one of which the palette replaces.</param>
    [PpuLayoutBoundary("codec: native 512-byte CGRAM transfer image of the opening palette")]
    public void LoadTo(IPaletteColors colours)
    {
        Ensure.NotNull(colours);
        new CgramPalettes(colours).All.LoadPortBytes(Transfer.Span);
    }

    /// <summary>One CGRAM row represented by exact colors or an equivalent authored color relationship.</summary>
    internal sealed class PaletteRow
    {
        /// <summary>Background and common foreground inks for flat or derived neutral-cycle rows.</summary>
        private readonly Bgr555 background, foreground;
        /// <summary>Original row colors when they cannot be represented by a stock shade relationship.</summary>
        private readonly Bgr555[]? supplied;
        /// <summary>Whether visible inks follow the four-shade neutral material cycle.</summary>
        private readonly bool cycle;
        /// <summary>The earlier palette row reused by the cross-fade row's first eight visible inks.</summary>
        private readonly PaletteRow? sharedGradient;
        /// <summary>Independent cross-fade endpoints and trailing inks retained alongside the shared gradient.</summary>
        private readonly Bgr555[]? crossFadeInputs;
        /// <summary>Recognizes exact cross-fade or neutral-cycle relationships before retaining unmatched colors verbatim.</summary>
        internal PaletteRow(Bgr555[] colors, bool allowCycle, PaletteRow? crossFadeSource)
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
                background = foreground = Bgr555.Black;
                break;
            }
        }
        /// <summary>Checks that the shared gradient and four descending blue inks reproduce the supplied row.</summary>
        private static bool MatchesCrossFade(Bgr555[] colors, PaletteRow source)
        {
            for (int color = 1; color <= 8; color++)
                if (colors[color] != source.Resolve(color)) return false;
            int blue = colors[9].Blue;
            if (colors[9].Red != 0 || colors[9].Green != 0 || blue < 3 * IntroCinematicPaletteFormat.CrossFadeBlueStep) return false;
            for (int shade = 1; shade < 4; shade++)
                if (colors[9 + shade] != new Bgr555(0, 0, blue - shade * IntroCinematicPaletteFormat.CrossFadeBlueStep)) return false;
            return true;
        }
        /// <summary>Reconstructs a color from retained source inks or its proven cycle/cross-fade relationship.</summary>
        internal Bgr555 Resolve(int color)
        {
            if (crossFadeInputs is not null)
            {
                if (color == 0) return crossFadeInputs[0];
                if (color <= 8)
                {
                    return (sharedGradient ?? throw new InvalidOperationException(
                        "A cross-faded intro palette requires its shared gradient.")).Resolve(color);
                }
                if (color <= 12) return new Bgr555(0, 0, crossFadeInputs[1].Blue -
                    (color - 9) * IntroCinematicPaletteFormat.CrossFadeBlueStep);
                return crossFadeInputs[color - 11];
            }
            return supplied is not null ? supplied[color] :
                color == 0 ? background : cycle ? CycleColor(color) : foreground;
        }
        /// <summary>Decreases each foreground RGB5 channel for its position in the repeating neutral ramp.</summary>
        private Bgr555 CycleColor(int color)
        {
            int decrease = (color - 1) % IntroCinematicPaletteFormat.NeutralCycleLength *
                IntroCinematicPaletteFormat.NeutralCycleStep;
            return foreground.Map((_, a) => Math.Max(0, a - decrease));
        }
    }
    /// <summary>Validates 256 ordered RGB5 colors and compiles their little-endian BGR555 image, calculating stock/shared shade relationships only when supplied colors match them.</summary>
    /// <param name="json">Caller-owned stream containing the supported editable opening-palette document.</param>
    /// <returns>The installed palette used for opening setup and subsequent cinematic fade targets, without changing fade scheduling.</returns>
    /// <exception cref="InvalidDataException">The schema version, complete color count, or RGB components are invalid.</exception>
    [PpuLayoutBoundary("codec: native 512-byte CGRAM transfer image of the opening palette")]
    public static IntroCinematicPalette Load(Stream json)
    {
        IntroCinematicPaletteDocument document = JsonAssetDocument.Read<IntroCinematicPaletteDocument>(
            json, ArtworkJson.Options, "opening palette");
        if (document.Version != IntroCinematicPaletteFormat.Version ||
            document.Colors is not { Length: CgramPalettes.ColorCount })
            throw new InvalidDataException("Opening palette requires 256 RGB5 colors.");

        var image = new byte[CgramPalettes.ByteCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Opening palette color {index} requires red, green and blue in 0..31.");
            BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(index * Bgr555.ByteCount),
                color.ToBgr555().ToWord());
        }
        return new IntroCinematicPalette(image);
    }

    /// <summary>Serializes the complete opening palette as UTF-8 JSON and validates it through <see cref="Load"/> before writing any destination bytes.</summary>
    /// <param name="json">Caller-owned destination stream.</param>
    /// <param name="document">Complete ordered RGB5 palette to validate and serialize.</param>
    public static void Write(Stream json, IntroCinematicPaletteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
