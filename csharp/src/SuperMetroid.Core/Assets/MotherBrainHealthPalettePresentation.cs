using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable damage-state colors for Mother Brain's final-phase body and rear legs.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    private readonly TintPalette body;
    private readonly TintPalette backLegs;

    private MotherBrainHealthPalettePresentation(Bgr555[][] body, Bgr555[][] backLegs)
    {
        this.body = new TintPalette(body, backLeg: false);
        this.backLegs = new TintPalette(backLegs, backLeg: true, this.body);
    }

    /// <summary>
    /// $A9:9474/9494 normal restoration repeats the $AD:E6AC/E74C health-state-zero bases.
    /// Independent installed documents use this only when their own supplied colors match.
    /// </summary>
    internal static Bgr555 StockBaseColor(bool backLeg, int color) =>
        (backLeg ? BasePalette.StockRear : BasePalette.StockBody).Color(color);

    /// <summary>$AD:EA0A/EA26 and F119 use health state three as their death-fade starting point.</summary>
    internal static Bgr555 StockDeathStartColor(bool backLeg, int color) =>
        TintColor(StockBaseColor(backLeg, color), 3, backLeg);

    private static Bgr555 TintColor(Bgr555 initial, int state, bool backLeg)
    {
        int amount = state * (state + 1) + (backLeg && state != 0 ? 2 : 0);
        return initial.Map((channel, value) =>
        {
            int rgb8 = value * 8 + 1;
            int target = channel == ColorChannel.Red ? 31 * 8 : 0;
            return (rgb8 * (30 - amount) + target * amount) / (30 * 8);
        });
    }
    /// <summary>Calculates one damage-tinted color pair to the three native CGRAM destinations.</summary>
    /// <param name="cgram">Mutable destination; body colors replace indices 65..79 and 145..159, while rear-leg colors replace indices 177..191. Transparent palette slots remain unchanged.</param>
    /// <param name="damageState">Zero-based health-band selector: zero for health at least 9000, one for 5400..8999, two for 1800..5399, and three below 1800; health selection itself remains in gameplay code.</param>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="damageState"/> is outside zero through three.</exception>
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
        private readonly Bgr555[][]? supplied;

        public TintPalette(Bgr555[][] rows, bool backLeg, TintPalette? front = null)
        {
            this.backLeg = backLeg;
            basis = BasePalette.Create(rows[0], backLeg, front?.basis);
            for (int state = 0; state < rows.Length; state++)
                for (int color = 0; color < rows[state].Length; color++)
                    if (Calculate(state, color) != rows[state][color])
                    { supplied = rows; return; }
        }

        public Bgr555 Color(int state, int color) => supplied is null ? Calculate(state, color) : supplied[state][color];

        private Bgr555 Calculate(int state, int color) => TintColor(basis.Color(color), state, backLeg);
    }
    /// <summary>Independent paint colors plus calculated quarter/fifth shade ramps.</summary>
    private sealed class BasePalette
    {
        private readonly Bgr555 highlight;
        private readonly Bgr555 midtone;
        private readonly Bgr555 shadow;
        private readonly Bgr555 brown;
        private readonly bool backLeg;
        private readonly Bgr555[]? supplied;
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
            Outline = MotherBrainHealthPaintDefinitions.Outline;
            Gray = MotherBrainHealthPaintDefinitions.PlateHighlight;
            brown = MotherBrainHealthPaintDefinitions.TissueHighlight;
        }

        internal static BasePalette Create(Bgr555[] colors, bool backLeg, BasePalette? front)
        {
            BasePalette stock = backLeg ? StockRear : StockBody;
            bool matches = !backLeg || ReferenceEquals(front, StockBody);
            for (int color = 0; matches && color < colors.Length; color++)
                matches = colors[color] == stock.Color(color);
            return matches ? stock : new BasePalette(colors, backLeg, front);
        }

        public BasePalette(Bgr555[] colors, bool backLeg, BasePalette? front)
        {
            this.backLeg = backLeg;
            this.front = front;
            highlight = backLeg ? Bgr555.Black : colors[0];
            midtone = backLeg ? Bgr555.Black : colors[1];
            shadow = backLeg ? Bgr555.Black : colors[2];
            Outline = front is null ? colors[3] : Bgr555.Black;
            Gray = front is null ? colors[4] : Bgr555.Black;
            brown = backLeg ? Bgr555.Black : colors[8];
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color])
                { supplied = colors; return; }
        }

        public Bgr555 Color(int color) => supplied is null ? Calculate(color) : supplied[color];

        private Bgr555 Calculate(int color) => color switch
        {
            0 => backLeg ? Bgr555.Black : highlight,
            1 => backLeg ? Bgr555.Black : midtone,
            2 => backLeg ? Bgr555.Black : shadow,
            3 => Outline,
            >= 4 and <= 7 => Shade(Gray, 8 - color, 4),
            >= 8 and <= 12 => backLeg ? Bgr555.Black : Shade(brown, 13 - color, 5),
            13 => backLeg ? Gray : Bgr555.White,
            _ => Bgr555.Black,
        };

        // The rear outline keeps the front red/blue and adds one RGB5 green
        // step. This names the chosen rear tint; it is not a universal lighting law.
        private Bgr555 Outline
        {
            get
            {
                if (front is null) return field;
                Bgr555 lit = front.Color(3);
                return lit.WithGreen(Math.Min(31, lit.Green + 1));
            }
        }
        // Rear illumination halves the front gray endpoint, rounding R/G upward
        // and B downward before generating its own four shade levels.
        private Bgr555 Gray
        {
            get
            {
                if (front is null) return field;
                Bgr555 lit = front.Color(4);
                return new((lit.Red + 1) / 2, (lit.Green + 1) / 2, lit.Blue / 2);
            }
        }
        private static Bgr555 Shade(Bgr555 source, int numerator, int denominator)
        {
            return source.Map((_, a) => ((a * numerator + denominator / 2) / denominator));
        }
    }
    /// <summary>Compiles installed RGB5 health-state artwork, retaining supplied edits independently of the native shade ramps and red-tint calculations.</summary>
    /// <param name="json">UTF-8 JSON input consumed from its current position to the end; the stream remains open.</param>
    /// <returns>Compiled body and rear-leg palette state independent of the source document's mutable arrays.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON is malformed, null, has duplicate properties, uses an unsupported version, or lacks four fifteen-color rows in each palette with RGB5 channels from zero through 31.</exception>
    public static MotherBrainHealthPalettePresentation Load(Stream json)
    {
        MotherBrainHealthPaletteDocument document = JsonAssetDocument.Read<MotherBrainHealthPaletteDocument>(
            json, MapPresentationFormat.JsonOptions, "Mother Brain health palette");
        if (document.Version != MotherBrainHealthPaletteFormat.Version)
            throw new InvalidDataException("Unsupported Mother Brain health palette version.");
        return new(Convert(document.Body, nameof(document.Body)),
            Convert(document.BackLegs, nameof(document.BackLegs)));

        static Bgr555[][] Convert(PaletteRgb5[][]? frames, string name)
        {
            if (frames is null || frames.Length != MotherBrainHealthPaletteFormat.StateCount)
                throw new InvalidDataException($"Mother Brain {name} requires four damage states.");
            var values = new Bgr555[frames.Length][];
            for (int state = 0; state < frames.Length; state++)
            {
                PaletteRgb5[]? colors = frames[state];
                if (colors is null || colors.Length != MotherBrainRainbowPaletteRomData.ColorCount)
                    throw new InvalidDataException($"Mother Brain {name} state {state} requires fifteen colors.");
                values[state] = new Bgr555[colors.Length];
                for (int color = 0; color < colors.Length; color++)
                {
                    PaletteRgb5? rgb = colors[color];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                        throw new InvalidDataException($"Mother Brain {name} state {state} color {color} requires RGB5 components.");
                    values[state][color] = rgb.ToBgr555();
                }
            }
            return values;
        }
    }

    /// <summary>Serializes artwork as UTF-8 JSON, validates it through <see cref="Load"/>, then writes the validated bytes without retaining the document.</summary>
    /// <param name="json">Output written at its current position and left open; existing trailing bytes are not truncated.</param>
    /// <param name="document">Four health-band images for body/brain and rear legs; supplied arrays are not modified.</param>
    /// <exception cref="InvalidDataException">The serialized document is null or fails schema, palette dimensions, or RGB5 channel validation.</exception>
    public static void Write(Stream json, MotherBrainHealthPaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}

/// <summary>Editable health-band palette JSON, separate from rainbow cycling and death-fade artwork; array references, rows, and RGB5 entries remain caller-owned and mutable.</summary>
public sealed record MotherBrainHealthPaletteDocument
{
    /// <summary>Schema version required to match <see cref="MotherBrainHealthPaletteFormat.Version"/> during loading or writing.</summary>
    public required int Version { get; init; }
    /// <summary>Four rows of fifteen RGB5 colors in native <c>MotherBrainHealthBasedPalettes_BrainBody</c> order from <c>$AD:E6A2</c>; ascending row index denotes lower health, and each row supplies both body BG and brain/neck OBJ colors 1..15.</summary>
    public required PaletteRgb5[][] Body { get; init; }
    /// <summary>Four rows of fifteen RGB5 colors in native <c>MotherBrainHealthBasedPalettes_BackLeg</c> order from <c>$AD:E742</c>, aligned with the body health bands and supplying rear-leg OBJ palette colors 1..15.</summary>
    public required PaletteRgb5[][] BackLegs { get; init; }
}

/// <summary>Installed JSON identity and dimensions for the four final-phase health-dependent palettes selected by native <c>MotherBrainHealthBasedPaletteHandling</c> at <c>$AD:E3D5</c>.</summary>
public static class MotherBrainHealthPaletteFormat
{
    /// <summary>Installed resource filename loaded with area-map presentation assets for Mother Brain's body/brain and rear-leg damage colors.</summary>
    public const string FileName = "mother-brain-health-palette.json";
    /// <summary>Supported schema version requiring paired body and rear-leg arrays with four fifteen-color RGB5 rows each.</summary>
    public const int Version = 1;
    /// <summary>Four health-band selectors in least-to-most-damaged order; excludes the unused fifth entry present in the native pointer tables.</summary>
    public const int StateCount = 4;
}
