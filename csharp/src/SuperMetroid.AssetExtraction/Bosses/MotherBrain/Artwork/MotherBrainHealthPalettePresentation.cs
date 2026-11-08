using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Editable damage-state colors for Mother Brain's final-phase body and rear legs.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    /// <summary>Selected body and brain colors across four health bands.</summary>
    internal readonly TintPalette body;
    /// <summary>Rear-leg colors whose stock outline and gray basis depend on the selected body palette.</summary>
    internal readonly TintPalette backLegs;

    /// <summary>Keeps the recognized body and rear-leg health-band tints.</summary>
    /// <param name="body">The body and brain tints.</param>
    /// <param name="backLegs">The rear-leg tints, whose stock basis depends on the body's.</param>
    internal MotherBrainHealthPalettePresentation(TintPalette body, TintPalette backLegs)
    {
        this.body = body;
        this.backLegs = backLegs;
    }

    /// <summary>
    /// The normal restoration repeats the health-state-zero bases.
    /// Independent installed documents use this only when their own supplied colors match.
    /// </summary>
    internal static RgbColor StockBaseColor(MotherBrainPalettePart part, int color) =>
        (part == MotherBrainPalettePart.BackLeg ? BasePalette.StockRear : BasePalette.StockBody).Color(color);

    /// <summary>The death fades use health state three as their starting point.</summary>
    internal static RgbColor StockDeathStartColor(MotherBrainPalettePart part, int color) =>
        TintColor(StockBaseColor(part, color), 3, part);

    /// <summary>Applies the red tint in biased RGB8 space, with an extra rear-leg increment beyond state zero.</summary>
    private static RgbColor TintColor(RgbColor initial, int state, MotherBrainPalettePart part)
    {
        int amount = state * (state + 1) + (part == MotherBrainPalettePart.BackLeg && state != 0 ? 2 : 0);
        return initial.FadeLevels.Map((channel, value) =>
        {
            int rgb8 = value * 8 + 1;
            int target = channel == ColorChannel.Red ? 31 * 8 : 0;
            return (rgb8 * (30 - amount) + target * amount) / (30 * 8);
        }).ToColour();
    }
    /// <summary>
    /// RGB8 red tint with a shared one-unit quantization bias. Body strength is
    /// state*(state+1)/30; rear legs add 2/30 after state zero.
    /// </summary>
    internal sealed class TintPalette
    {
        /// <summary>State-zero colors used to derive the damage tint curve.</summary>
        internal readonly BasePalette basis;
        /// <summary>Selects the extra rear-leg tint strength after the undamaged state.</summary>
        private readonly MotherBrainPalettePart part;
        /// <summary>Complete edited health bands retained if any sample differs from the basis-derived tint.</summary>
        internal readonly RgbColor[][]? supplied;

        /// <summary>Keeps the state-zero basis and, when one sample differs from its tint, every supplied health band.</summary>
        /// <param name="basis">The state-zero colors the tint curve derives from.</param>
        /// <param name="part">The palette part, which selects the rear-leg tint strength.</param>
        /// <param name="supplied">Every supplied health band when one sample differs, otherwise null.</param>
        internal TintPalette(BasePalette basis, MotherBrainPalettePart part, RgbColor[][]? supplied)
        {
            this.basis = basis;
            this.part = part;
            this.supplied = supplied;
        }

        /// <summary>Selects an edited sample or the tint of its state-zero basis color.</summary>
        public RgbColor Color(int state, int color) => supplied is null ? Calculate(state, color) : supplied[state][color];

        /// <summary>Tints one selected basis ink for a health band.</summary>
        private RgbColor Calculate(int state, int color) => TintColor(basis.Color(color), state, part);
    }
    /// <summary>The independent endpoint inks a state-zero basis calculates its shade ramps from.</summary>
    /// <param name="Highlight">Body cortex highlight.</param>
    /// <param name="Midtone">Body cortex midtone.</param>
    /// <param name="Shadow">Body cortex shadow.</param>
    /// <param name="Outline">Body outline; a rear-leg basis derives its own from the body.</param>
    /// <param name="Gray">Body plate gray endpoint; a rear-leg basis derives its own from the body.</param>
    /// <param name="Brown">Body tissue endpoint.</param>
    internal readonly record struct BasePaletteAnchors(RgbColor Highlight, RgbColor Midtone, RgbColor Shadow, RgbColor Outline,
        RgbColor Gray, RgbColor Brown);

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

        /// <summary>State-zero body basis from reviewed paint colors.</summary>
        internal static BasePalette StockBody { get; } = new(MotherBrainPalettePart.Body);
        /// <summary>Rear-leg basis derived from the stock body basis.</summary>
        internal static BasePalette StockRear { get; } = new(MotherBrainPalettePart.BackLeg);

        /// <summary>Initializes the reviewed stock body or rear-leg basis without storing its colors.</summary>
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

        /// <summary>Keeps independent endpoint inks and, when the shade relationships do not reproduce them, every supplied color.</summary>
        /// <param name="part">The palette part.</param>
        /// <param name="front">The body basis a rear-leg basis derives its outline and gray from, or null.</param>
        /// <param name="anchors">The independent endpoint inks.</param>
        /// <param name="supplied">Every supplied color when the shade relationships do not reproduce them, otherwise null.</param>
        internal BasePalette(MotherBrainPalettePart part, BasePalette? front, BasePaletteAnchors anchors, RgbColor[]? supplied)
        {
            this.part = part;
            this.front = front;
            highlight = anchors.Highlight;
            midtone = anchors.Midtone;
            shadow = anchors.Shadow;
            Outline = anchors.Outline;
            Gray = anchors.Gray;
            brown = anchors.Brown;
            this.supplied = supplied;
        }

        /// <summary>Selects one edited basis ink or its calculated stock shade.</summary>
        public RgbColor Color(int color) => supplied is null ? Calculate(color) : supplied[color];

        /// <summary>Builds cortex, outline, gray, and tissue inks using the quarter and fifth shade ramps.</summary>
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
                return lit.FadeLevels.WithGreen(Math.Min(31, lit.FadeLevels.Green + 1)).ToColour();
            }
        }
        /// <summary>Rear gray halves the front endpoint, rounding red and green upward and blue downward before its shade ramp.</summary>
        private RgbColor Gray
        {
            get
            {
                if (front is null) return field;
                RgbColor lit = front.Color(4);
                return new FadeLevelColour((lit.FadeLevels.Red + 1) / 2, (lit.FadeLevels.Green + 1) / 2, lit.FadeLevels.Blue / 2).ToColour();
            }
        }
        /// <summary>Scales each RGB5 channel with nearest-integer rounding for a quarter or fifth ramp.</summary>
        private static RgbColor Shade(RgbColor source, int numerator, int denominator)
        {
            return source.FadeLevels.Map((_, a) => ((a * numerator + denominator / 2) / denominator)).ToColour();
        }
    }
}
