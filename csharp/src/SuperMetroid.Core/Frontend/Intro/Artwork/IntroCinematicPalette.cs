using SuperMetroid.Core.Rendering;
using System.Buffers.Binary;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>The opening narration's full, editable 256 screen colours.</summary>
public sealed class IntroCinematicPalette
{
    /// <summary>Decoded sixteen-color rows; empty when the installed bytes match compiled stock paint.</summary>
    internal readonly PaletteRow[] rows;

    /// <summary>Keeps the opening's 256 selected colours, recognising stock, neutral-cycle and shared cross-fade rows.</summary>
    /// <param name="colours">The selected colours in screen-palette order.</param>
    internal static IntroCinematicPalette FromColours(ReadOnlySpan<Bgr555> colours)
    {
        var transfer = new byte[colours.Length * Bgr555.ByteCount];
        for (int index = 0; index < colours.Length; index++)
            BinaryPrimitives.WriteUInt16LittleEndian(transfer.AsSpan(index * Bgr555.ByteCount), colours[index].ToWord());
        return new IntroCinematicPalette(transfer);
    }

    /// <summary>Retains exact supplied colors while recognizing stock, neutral-cycle, and shared cross-fade rows.</summary>
    /// <param name="transfer">The 256 selected colours as the palette transfer packs them.</param>
    private IntroCinematicPalette(byte[] transfer)
    {
        if (IntroCinematicPaintDefinitions.Matches(transfer)) { rows = []; return; }
        rows = new PaletteRow[ScreenPalette.ColorCount / IntroCinematicPaletteFormat.ColorsPerRow];
        for (int row = 0; row < rows.Length; row++)
        {
            var colors = new Bgr555[IntroCinematicPaletteFormat.ColorsPerRow];
            for (int color = 0; color < colors.Length; color++)
                colors[color] = Bgr555.FromWord(BinaryPrimitives.ReadUInt16LittleEndian(transfer.AsSpan(2 * (row * colors.Length + color))));
            bool neutralCycle = row == IntroCinematicPaletteFormat.NeutralCycleRow;
            PaletteRow? crossFadeSource = row == IntroCinematicPaletteFormat.CrossFadeRow
                ? rows[IntroCinematicPaletteFormat.SharedCrossFadeSourceRow] : null;
            rows[row] = new PaletteRow(colors, neutralCycle, crossFadeSource);
        }
    }

    /// <summary>The installed colours, in screen order.</summary>
    public Bgr555[] Colors
    {
        get
        {
            var output = new Bgr555[ScreenPalette.ColorCount];
            for (int row = 0; row < ScreenPalette.ColorCount / IntroCinematicPaletteFormat.ColorsPerRow; row++)
            for (int color = 0; color < IntroCinematicPaletteFormat.ColorsPerRow; color++)
                output[row * IntroCinematicPaletteFormat.ColorsPerRow + color] = rows.Length == 0 ? IntroCinematicPaintDefinitions.Color(row, color) : rows[row].Resolve(color);
            return output;
        }
    }

    /// <summary>The installed colours as little-endian words, for the content identity.</summary>
    public ReadOnlyMemory<byte> ColorWords
    {
        get
        {
            Bgr555[] colors = Colors;
            var output = new byte[colors.Length * Bgr555.ByteCount];
            for (int color = 0; color < colors.Length; color++)
                BinaryPrimitives.WriteUInt16LittleEndian(output.AsSpan(color * Bgr555.ByteCount), colors[color].ToWord());
            return output;
        }
    }

    /// <summary>Replaces every colour of the screen with the installed opening palette, corresponding to $8C:E3E9-E5E8; does not advance or reset cinematic fade timers.</summary>
    /// <param name="colours">The screen's colours, every one of which the palette replaces.</param>
    public void LoadTo(ScreenPalette colours) => Ensure.NotNull(colours).Load(Colors);

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
}
