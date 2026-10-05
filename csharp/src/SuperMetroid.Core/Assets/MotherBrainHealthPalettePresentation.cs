using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable damage-state colors for Mother Brain's final-phase body and rear legs.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    private readonly TintPalette body;
    private readonly TintPalette backLegs;

    private MotherBrainHealthPalettePresentation(ushort[][] body, ushort[][] backLegs)
    {
        this.body = new TintPalette(body, backLeg: false);
        this.backLegs = new TintPalette(backLegs, backLeg: true, this.body);
    }

    /// <summary>
    /// $A9:9474/9494 normal restoration repeats the $AD:E6AC/E74C health-state-zero bases.
    /// Independent installed documents use this only when their own supplied colors match.
    /// </summary>
    internal static ushort StockBaseColor(bool backLeg, int color) =>
        (backLeg ? BasePalette.StockRear : BasePalette.StockBody).Color(color);

    /// <summary>$AD:EA0A/EA26 and F119 use health state three as their death-fade starting point.</summary>
    internal static ushort StockDeathStartColor(bool backLeg, int color) =>
        TintColor(StockBaseColor(backLeg, color), 3, backLeg);

    private static ushort TintColor(ushort initial, int state, bool backLeg)
    {
        int amount = state * (state + 1) + (backLeg && state != 0 ? 2 : 0);
        int result = 0;
        for (int component = 0; component < 3; component++)
        {
            int rgb8 = (((initial >> (component * 5)) & 31) * 8) + 1;
            int target = component == 0 ? 31 * 8 : 0;
            int value = (rgb8 * (30 - amount) + target * amount) / (30 * 8);
            result |= value << (component * 5);
        }
        return (ushort)result;
    }
    /// <summary>Calculates one damage-tinted color pair to the three native CGRAM destinations.</summary>
    public void Apply(SnesCgram cgram, int damageState)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if ((uint)damageState >= MotherBrainHealthPaletteFormat.StateCount)
            throw new ArgumentOutOfRangeException(nameof(damageState));
        for (int color = 0; color < MotherBrainRainbowPaletteRomData.ColorCount; color++)
        {
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + color, body.Color(damageState, color));
            cgram.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + color, body.Color(damageState, color));
            cgram.SetColor(MotherBrainRainbowPaletteRomData.SecondaryColor + color, backLegs.Color(damageState, color));
        }
    }

    /// <summary>
    /// RGB8 red tint with a shared one-unit quantization bias. Body strength is
    /// state*(state+1)/30; rear legs add 2/30 after state zero.
    /// </summary>
    private sealed class TintPalette
    {
        private readonly BasePalette basis;
        private readonly bool backLeg;
        private readonly ushort[][]? supplied;

        public TintPalette(ushort[][] rows, bool backLeg, TintPalette? front = null)
        {
            this.backLeg = backLeg;
            basis = BasePalette.Create(rows[0], backLeg, front?.basis);
            for (int state = 0; state < rows.Length; state++)
                for (int color = 0; color < rows[state].Length; color++)
                    if (Calculate(state, color) != rows[state][color])
                    { supplied = rows; return; }
        }

        public ushort Color(int state, int color) => supplied is null ? Calculate(state, color) : supplied[state][color];

        private ushort Calculate(int state, int color) => TintColor(basis.Color(color), state, backLeg);
    }
    /// <summary>Independent paint colors plus calculated quarter/fifth shade ramps.</summary>
    private sealed class BasePalette
    {
        private readonly ushort highlight;
        private readonly ushort midtone;
        private readonly ushort shadow;
        private readonly ushort outline;
        private readonly ushort gray;
        private readonly ushort brown;
        private readonly bool backLeg;
        private readonly ushort[]? supplied;
        private readonly BasePalette? front;

        internal static BasePalette StockBody { get; } = new(false);
        internal static BasePalette StockRear { get; } = new(true);

        private BasePalette(bool backLeg)
        {
            this.backLeg = backLeg;
            front = backLeg ? StockBody : null;
            if (backLeg) return;
            highlight = MotherBrainHealthPaintDefinitions.CortexHighlight;
            midtone = MotherBrainHealthPaintDefinitions.CortexMidtone;
            shadow = MotherBrainHealthPaintDefinitions.CortexShadow;
            outline = MotherBrainHealthPaintDefinitions.Outline;
            gray = MotherBrainHealthPaintDefinitions.PlateHighlight;
            brown = MotherBrainHealthPaintDefinitions.TissueHighlight;
        }

        internal static BasePalette Create(ushort[] colors, bool backLeg, BasePalette? front)
        {
            BasePalette stock = backLeg ? StockRear : StockBody;
            bool matches = !backLeg || ReferenceEquals(front, StockBody);
            for (int color = 0; matches && color < colors.Length; color++)
                matches = colors[color] == stock.Color(color);
            return matches ? stock : new BasePalette(colors, backLeg, front);
        }

        public BasePalette(ushort[] colors, bool backLeg, BasePalette? front)
        {
            this.backLeg = backLeg;
            this.front = front;
            highlight = backLeg ? (ushort)0 : colors[0];
            midtone = backLeg ? (ushort)0 : colors[1];
            shadow = backLeg ? (ushort)0 : colors[2];
            outline = front is null ? colors[3] : (ushort)0;
            gray = front is null ? colors[4] : (ushort)0;
            brown = backLeg ? (ushort)0 : colors[8];
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color])
                { supplied = colors; return; }
        }

        public ushort Color(int color) => supplied is null ? Calculate(color) : supplied[color];

        private ushort Calculate(int color) => color switch
        {
            0 => backLeg ? (ushort)0 : highlight,
            1 => backLeg ? (ushort)0 : midtone,
            2 => backLeg ? (ushort)0 : shadow,
            3 => Outline,
            >= 4 and <= 7 => Shade(Gray, 8 - color, 4),
            >= 8 and <= 12 => backLeg ? (ushort)0 : Shade(brown, 13 - color, 5),
            13 => backLeg ? Gray : (ushort)((31 << 10) | (31 << 5) | 31),
            _ => 0,
        };

        // The rear outline keeps the front red/blue and adds one RGB5 green
        // step. This names the chosen rear tint; it is not a universal lighting law.
        private ushort Outline
        {
            get
            {
                if (front is null) return outline;
                ushort lit = front.Color(3);
                int green = Math.Min(31, (lit >> 5 & 31) + 1);
                return (ushort)((lit & ~(31 << 5)) | green << 5);
            }
        }
        // Rear illumination halves the front gray endpoint, rounding R/G upward
        // and B downward before generating its own four shade levels.
        private ushort Gray
        {
            get
            {
                if (front is null) return gray;
                ushort lit = front.Color(4);
                return (ushort)(((lit & 31) + 1) / 2 |
                    (((lit >> 5 & 31) + 1) / 2) << 5 | ((lit >> 10 & 31) / 2) << 10);
            }
        }
        private static ushort Shade(ushort source, int numerator, int denominator)
        {
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= ((((source >> shift) & 31) * numerator + denominator / 2) / denominator) << shift;
            return (ushort)result;
        }
    }
    public static MotherBrainHealthPalettePresentation Load(Stream json)
    {
        MotherBrainHealthPaletteDocument document = JsonAssetDocument.Read<MotherBrainHealthPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "Mother Brain health palette");
        if (document.Version != MotherBrainHealthPaletteFormat.Version)
            throw new InvalidDataException("Unsupported Mother Brain health palette version.");
        return new(Convert(document.Body, nameof(document.Body)),
            Convert(document.BackLegs, nameof(document.BackLegs)));

        static ushort[][] Convert(PaletteRgb5[][]? frames, string name)
        {
            if (frames is null || frames.Length != MotherBrainHealthPaletteFormat.StateCount)
                throw new InvalidDataException($"Mother Brain {name} requires four damage states.");
            var values = new ushort[frames.Length][];
            for (int state = 0; state < frames.Length; state++)
            {
                PaletteRgb5[]? colors = frames[state];
                if (colors is null || colors.Length != MotherBrainRainbowPaletteRomData.ColorCount)
                    throw new InvalidDataException($"Mother Brain {name} state {state} requires fifteen colors.");
                values[state] = new ushort[colors.Length];
                for (int color = 0; color < colors.Length; color++)
                {
                    PaletteRgb5? rgb = colors[color];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                        throw new InvalidDataException($"Mother Brain {name} state {state} color {color} requires RGB5 components.");
                    values[state][color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
                }
            }
            return values;
        }
    }

    public static void Write(Stream json, MotherBrainHealthPaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

public sealed record MotherBrainHealthPaletteDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[][] Body { get; init; }
    public required PaletteRgb5[][] BackLegs { get; init; }
}

public static class MotherBrainHealthPaletteFormat
{
    public const string FileName = "mother-brain-health-palette.json";
    public const int Version = 1;
    public const int StateCount = 4;
}
