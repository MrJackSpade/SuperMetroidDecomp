using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Editable damage-state colors for Mother Brain's final-phase body and rear legs.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    /// <summary>Selected body and brain colors across four health bands.</summary>
    internal readonly TintPalette body;
    /// <summary>Rear-leg colors whose stock outline and gray basis depend on the selected body palette.</summary>
    internal readonly TintPalette backLegs;

    /// <summary>Compiles body and rear-leg tint rows, retaining independently supplied colors when they differ from native shading.</summary>
    internal MotherBrainHealthPalettePresentation(RgbColor[][] body, RgbColor[][] backLegs)
    {
        this.body = new TintPalette(body, MotherBrainPalettePart.Body);
        this.backLegs = new TintPalette(backLegs, MotherBrainPalettePart.BackLeg, this.body);
    }

    /// <summary>
    /// $A9:9474/9494 normal restoration repeats the $AD:E6AC/E74C health-state-zero bases.
    /// Independent installed documents use this only when their own supplied colors match.
    /// </summary>
    internal static RgbColor StockBaseColor(MotherBrainPalettePart part, int color) =>
        (part == MotherBrainPalettePart.BackLeg ? BasePalette.StockRear : BasePalette.StockBody).Color(color);

    /// <summary>$AD:EA0A/EA26 and F119 use health state three as their death-fade starting point.</summary>
    internal static RgbColor StockDeathStartColor(MotherBrainPalettePart part, int color) =>
        TintColor(StockBaseColor(part, color), 3, part);

    /// <summary>Applies the native red tint in biased RGB8 space, with an extra rear-leg increment beyond state zero.</summary>
    private static RgbColor TintColor(RgbColor initial, int state, MotherBrainPalettePart part)
    {
        int amount = state * (state + 1) + (part == MotherBrainPalettePart.BackLeg && state != 0 ? 2 : 0);
        return initial.Map((channel, value) =>
        {
            int rgb8 = value * 8 + 1;
            int target = channel == ColorChannel.Red ? 31 * 8 : 0;
            return (rgb8 * (30 - amount) + target * amount) / (30 * 8);
        });
    }
    /// <summary>Calculates one damage-tinted color pair to the three native CGRAM destinations.</summary>
    /// <param name="gameplayColours">Mutable destination; body colors replace indices 65..79 and 145..159, while rear-leg colors replace indices 177..191. Transparent palette slots remain unchanged.</param>
    /// <param name="damageState">Zero-based health-band selector: zero for health at least 9000, one for 5400..8999, two for 1800..5399, and three below 1800; health selection itself remains in gameplay code.</param>
    /// <exception cref="ArgumentNullException"><paramref name="gameplayColours"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="damageState"/> is outside zero through three.</exception>
    public void Apply(GameplayColours gameplayColours, int damageState)
    {
        Ensure.NotNull(gameplayColours);
        if ((uint)damageState >= MotherBrainHealthPaletteFormat.StateCount)
            throw new ArgumentOutOfRangeException(nameof(damageState));
        for (int color = 0; color < MotherBrainRainbowPaletteDefinitions.ColorCount; color++)
        {
            new MotherBrainColours(gameplayColours).Body.Write(color, body.Color(damageState, color));
            new MotherBrainColours(gameplayColours).Brain.Write(color, body.Color(damageState, color));
            new MotherBrainColours(gameplayColours).RearLegs.Write(color, backLegs.Color(damageState, color));
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
        private readonly MotherBrainPalettePart part;
        /// <summary>Complete edited rows retained if any sample differs from the basis-derived tint.</summary>
        internal readonly RgbColor[][]? supplied;

        /// <summary>Compares all four supplied rows with the calculated tint before deciding whether to retain them.</summary>
        public TintPalette(RgbColor[][] rows, MotherBrainPalettePart part, TintPalette? front = null)
        {
            this.part = part;
            basis = BasePalette.Create(rows[0], part, front?.basis);
            for (int state = 0; state < rows.Length; state++)
                for (int color = 0; color < rows[state].Length; color++)
                    if (Calculate(state, color) != rows[state][color])
                    { supplied = rows; return; }
        }

        /// <summary>Selects an edited sample or the native tint of its state-zero basis color.</summary>
        public RgbColor Color(int state, int color) => supplied is null ? Calculate(state, color) : supplied[state][color];

        /// <summary>Tints one selected basis ink for a health band.</summary>
        private RgbColor Calculate(int state, int color) => TintColor(basis.Color(color), state, part);
    }
    /// <summary>Independent paint colors plus calculated quarter/fifth shade ramps.</summary>
    internal sealed class BasePalette
    {
        /// <summary>Body cortex highlight used as the first stock basis color.</summary>
        internal readonly RgbColor highlight;
        /// <summary>Body cortex midtone used as the second stock basis color.</summary>
        internal readonly RgbColor midtone;
        /// <summary>Body cortex shadow used as the third stock basis color.</summary>
        internal readonly RgbColor shadow;
        /// <summary>Body tissue endpoint from which the fifth-based shade ramp is calculated.</summary>
        internal readonly RgbColor brown;
        /// <summary>Selects the rear-leg-specific relationship to the body outline and gray endpoint.</summary>
        private readonly MotherBrainPalettePart part;
        /// <summary>Independent state-zero colors retained when an edited image breaks a calculated relationship.</summary>
        internal readonly RgbColor[]? supplied;
        /// <summary>Selected body basis from which stock rear-leg outline and gray are derived.</summary>
        private readonly BasePalette? front;

        /// <summary>Native state-zero body basis from reviewed paint colors.</summary>
        internal static BasePalette StockBody { get; } = new(MotherBrainPalettePart.Body);
        /// <summary>Native rear-leg basis derived from the stock body basis.</summary>
        internal static BasePalette StockRear { get; } = new(MotherBrainPalettePart.BackLeg);

        /// <summary>Initializes the reviewed stock body or rear-leg basis without storing a color row.</summary>
        private BasePalette(MotherBrainPalettePart part)
        {
            this.part = part;
            front = part == MotherBrainPalettePart.BackLeg ? StockBody : null;
            if (part == MotherBrainPalettePart.BackLeg) return;
            highlight = MotherBrainHealthPaintDefinitions.CortexHighlight;
            midtone = MotherBrainHealthPaintDefinitions.CortexMidtone;
            shadow = MotherBrainHealthPaintDefinitions.CortexShadow;
            Outline = MotherBrainHealthPaintDefinitions.Outline;
            Gray = MotherBrainHealthPaintDefinitions.PlateHighlight;
            brown = MotherBrainHealthPaintDefinitions.TissueHighlight;
        }

        /// <summary>Shares a stock basis only when every supplied color and its dependent front palette agree.</summary>
        internal static BasePalette Create(RgbColor[] colors, MotherBrainPalettePart part, BasePalette? front)
        {
            BasePalette stock = part == MotherBrainPalettePart.BackLeg ? StockRear : StockBody;
            bool matches = part != MotherBrainPalettePart.BackLeg || ReferenceEquals(front, StockBody);
            for (int color = 0; matches && color < colors.Length; color++)
                matches = colors[color] == stock.Color(color);
            return matches ? stock : new BasePalette(colors, part, front);
        }

        /// <summary>Selects independent endpoint inks and retains the full row if native shade relationships do not reproduce it.</summary>
        public BasePalette(RgbColor[] colors, MotherBrainPalettePart part, BasePalette? front)
        {
            this.part = part;
            this.front = front;
            highlight = part == MotherBrainPalettePart.BackLeg ? RgbColor.Black : colors[0];
            midtone = part == MotherBrainPalettePart.BackLeg ? RgbColor.Black : colors[1];
            shadow = part == MotherBrainPalettePart.BackLeg ? RgbColor.Black : colors[2];
            Outline = front is null ? colors[3] : RgbColor.Black;
            Gray = front is null ? colors[4] : RgbColor.Black;
            brown = part == MotherBrainPalettePart.BackLeg ? RgbColor.Black : colors[8];
            for (int color = 0; color < colors.Length; color++)
                if (Calculate(color) != colors[color])
                { supplied = colors; return; }
        }

        /// <summary>Selects one edited basis ink or its calculated stock shade.</summary>
        public RgbColor Color(int color) => supplied is null ? Calculate(color) : supplied[color];

        /// <summary>Builds cortex, outline, gray, and tissue inks using the native quarter and fifth shade ramps.</summary>
        private RgbColor Calculate(int color) => color switch
        {
            0 => part == MotherBrainPalettePart.BackLeg ? RgbColor.Black : highlight,
            1 => part == MotherBrainPalettePart.BackLeg ? RgbColor.Black : midtone,
            2 => part == MotherBrainPalettePart.BackLeg ? RgbColor.Black : shadow,
            3 => Outline,
            >= 4 and <= 7 => Shade(Gray, 8 - color, 4),
            >= 8 and <= 12 => part == MotherBrainPalettePart.BackLeg ? RgbColor.Black : Shade(brown, 13 - color, 5),
            13 => part == MotherBrainPalettePart.BackLeg ? Gray : RgbColor.White,
            _ => RgbColor.Black,
        };

        /// <summary>Rear outline keeps the front red and blue with one more RGB5 green step, a chosen tint rather than a general lighting rule.</summary>
        private RgbColor Outline
        {
            get
            {
                if (front is null) return field;
                RgbColor lit = front.Color(3);
                return lit.WithGreen(Math.Min(31, lit.Green + 1));
            }
        }
        /// <summary>Rear gray halves the front endpoint, rounding red and green upward and blue downward before its shade ramp.</summary>
        private RgbColor Gray
        {
            get
            {
                if (front is null) return field;
                RgbColor lit = front.Color(4);
                return new((lit.Red + 1) / 2, (lit.Green + 1) / 2, lit.Blue / 2);
            }
        }
        /// <summary>Scales each RGB5 channel with nearest-integer rounding for a quarter or fifth ramp.</summary>
        private static RgbColor Shade(RgbColor source, int numerator, int denominator)
        {
            return source.Map((_, a) => ((a * numerator + denominator / 2) / denominator));
        }
    }
}
