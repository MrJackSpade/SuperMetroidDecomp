using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable damage-state colors for Mother Brain's final-phase body and rear legs.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    /// <summary>Selected body and brain colors across four health bands.</summary>
    internal readonly TintPalette body;
    /// <summary>Rear-leg colors whose stock outline and gray basis depend on the selected body palette.</summary>
    internal readonly TintPalette backLegs;

    /// <summary>Compiles body and rear-leg tint rows, retaining independently supplied colors when they differ from native shading.</summary>
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

    /// <summary>Applies the native red tint in biased RGB8 space, with an extra rear-leg increment beyond state zero.</summary>
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
        Ensure.NotNull(cgram);
        if ((uint)damageState >= MotherBrainHealthPaletteFormat.StateCount)
            throw new ArgumentOutOfRangeException(nameof(damageState));
        for (int color = 0; color < MotherBrainRainbowPaletteRomData.ColorCount; color++)
        {
            new MotherBrainPalettes(cgram).Body.Write(color, body.Color(damageState, color));
            new MotherBrainPalettes(cgram).Brain.Write(color, body.Color(damageState, color));
            new MotherBrainPalettes(cgram).RearLegs.Write(color, backLegs.Color(damageState, color));
        }
    }

    /// <summary>
    /// RGB8 red tint with a shared one-unit quantization bias. Body strength is
    /// state*(state+1)/30; rear legs add 2/30 after state zero.
    /// </summary>
    internal sealed class TintPalette
    {
        /// <summary>State-zero colors used to derive the native damage tint curve.</summary>
        internal readonly BasePalette basis;
        /// <summary>Selects the extra rear-leg tint strength after the undamaged state.</summary>
        private readonly bool backLeg;
        /// <summary>Complete edited rows retained if any sample differs from the basis-derived tint.</summary>
        internal readonly Bgr555[][]? supplied;

        /// <summary>Compares all four supplied rows with the calculated tint before deciding whether to retain them.</summary>
        public TintPalette(Bgr555[][] rows, bool backLeg, TintPalette? front = null)
        {
            this.backLeg = backLeg;
            basis = BasePalette.Create(rows[0], backLeg, front?.basis);
            for (int state = 0; state < rows.Length; state++)
                for (int color = 0; color < rows[state].Length; color++)
                    if (Calculate(state, color) != rows[state][color])
                    { supplied = rows; return; }
        }

        /// <summary>Selects an edited sample or the native tint of its state-zero basis color.</summary>
        public Bgr555 Color(int state, int color) => supplied is null ? Calculate(state, color) : supplied[state][color];

        /// <summary>Tints one selected basis ink for a health band.</summary>
        private Bgr555 Calculate(int state, int color) => TintColor(basis.Color(color), state, backLeg);
    }
    /// <summary>Independent paint colors plus calculated quarter/fifth shade ramps.</summary>
    internal sealed class BasePalette
    {
        /// <summary>Body cortex highlight used as the first stock basis color.</summary>
        internal readonly Bgr555 highlight;
        /// <summary>Body cortex midtone used as the second stock basis color.</summary>
        internal readonly Bgr555 midtone;
        /// <summary>Body cortex shadow used as the third stock basis color.</summary>
        internal readonly Bgr555 shadow;
        /// <summary>Body tissue endpoint from which the fifth-based shade ramp is calculated.</summary>
        internal readonly Bgr555 brown;
        /// <summary>Selects the rear-leg-specific relationship to the body outline and gray endpoint.</summary>
        private readonly bool backLeg;
        /// <summary>Independent state-zero colors retained when an edited image breaks a calculated relationship.</summary>
        internal readonly Bgr555[]? supplied;
        /// <summary>Selected body basis from which stock rear-leg outline and gray are derived.</summary>
        private readonly BasePalette? front;

        /// <summary>Native state-zero body basis from reviewed paint colors.</summary>
        internal static BasePalette StockBody { get; } = new(false);
        /// <summary>Native rear-leg basis derived from the stock body basis.</summary>
        internal static BasePalette StockRear { get; } = new(true);

        /// <summary>Initializes the reviewed stock body or rear-leg basis without storing a color row.</summary>
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

        /// <summary>Shares a stock basis only when every supplied color and its dependent front palette agree.</summary>
        internal static BasePalette Create(Bgr555[] colors, bool backLeg, BasePalette? front)
        {
            BasePalette stock = backLeg ? StockRear : StockBody;
            bool matches = !backLeg || ReferenceEquals(front, StockBody);
            for (int color = 0; matches && color < colors.Length; color++)
                matches = colors[color] == stock.Color(color);
            return matches ? stock : new BasePalette(colors, backLeg, front);
        }

        /// <summary>Selects independent endpoint inks and retains the full row if native shade relationships do not reproduce it.</summary>
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

        /// <summary>Selects one edited basis ink or its calculated stock shade.</summary>
        public Bgr555 Color(int color) => supplied is null ? Calculate(color) : supplied[color];

        /// <summary>Builds cortex, outline, gray, and tissue inks using the native quarter and fifth shade ramps.</summary>
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

        /// <summary>Rear outline keeps the front red and blue with one more RGB5 green step, a chosen tint rather than a general lighting rule.</summary>
        private Bgr555 Outline
        {
            get
            {
                if (front is null) return field;
                Bgr555 lit = front.Color(3);
                return lit.WithGreen(Math.Min(31, lit.Green + 1));
            }
        }
        /// <summary>Rear gray halves the front endpoint, rounding red and green upward and blue downward before its shade ramp.</summary>
        private Bgr555 Gray
        {
            get
            {
                if (front is null) return field;
                Bgr555 lit = front.Color(4);
                return new((lit.Red + 1) / 2, (lit.Green + 1) / 2, lit.Blue / 2);
            }
        }
        /// <summary>Scales each RGB5 channel with nearest-integer rounding for a quarter or fifth ramp.</summary>
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
