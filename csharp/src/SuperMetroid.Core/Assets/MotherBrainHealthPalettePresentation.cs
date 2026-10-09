using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable damage-state colors for Mother Brain's final-phase body and rear legs.</summary>
public sealed class MotherBrainHealthPalettePresentation
{
    /// <summary>Damage-state color rows used for the body and brain OBJ palettes.</summary>
    private readonly TintPalette body;
    /// <summary>Damage-state color rows used for Mother Brain's rear-leg palette.</summary>
    private readonly TintPalette backLegs;

    /// <summary>Builds lookup palettes from the validated RGB5 rows loaded from the installed asset.</summary>
    /// <param name="body">Four body/brain rows, ordered from healthiest to most damaged.</param>
    /// <param name="backLegs">Four rear-leg rows aligned with <paramref name="body"/> health bands.</param>
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

    /// <summary>Interpolates a base RGB5 color toward red using the native damage-state tint strength and rear-leg adjustment.</summary>
    /// <param name="initial">Unmodified RGB5 source color.</param>
    /// <param name="state">Damage band from zero through three.</param>
    /// <param name="backLeg">Whether to apply the rear-leg tint increment used for nonzero damage bands.</param>
    /// <returns>The quantized RGB5 tint, with the shared one-unit RGB8 conversion bias.</returns>
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
        /// <summary>Base colors used to calculate tint values when the installed rows follow the native ramp.</summary>
        private readonly BasePalette basis;
        /// <summary>Controls the extra tint strength applied to nonzero rear-leg damage states.</summary>
        private readonly bool backLeg;
        /// <summary>Original rows retained when any installed entry differs from the calculated tint ramp.</summary>
        private readonly ushort[][]? supplied;

        /// <summary>Chooses native tint calculation or preserves the supplied rows when they contain custom colors.</summary>
        /// <param name="rows">Damage-state rows to compare with colors calculated from the first row.</param>
        /// <param name="backLeg">Selects rear-leg tint strength and palette bases.</param>
        /// <param name="front">Body palette used as the source of rear-leg base colors.</param>
        public TintPalette(ushort[][] rows, bool backLeg, TintPalette? front = null)
        {
            this.backLeg = backLeg;
            basis = BasePalette.Create(rows[0], backLeg, front?.basis);
            for (int state = 0; state < rows.Length; state++)
                for (int color = 0; color < rows[state].Length; color++)
                    if (Calculate(state, color) != rows[state][color])
                    { supplied = rows; return; }
        }

        /// <summary>Returns an installed color verbatim when custom rows are needed, otherwise calculates its tint.</summary>
        /// <param name="state">Damage-state row index.</param>
        /// <param name="color">Color index within that row.</param>
        public ushort Color(int state, int color) => supplied is null ? Calculate(state, color) : supplied[state][color];

        /// <summary>Calculates a tint from the selected base color and this palette's rear-leg mode.</summary>
        /// <param name="state">Damage band controlling the interpolation amount.</param>
        /// <param name="color">Base-palette color index.</param>
        /// <returns>The RGB5 color for the requested damage band.</returns>
        private ushort Calculate(int state, int color) => TintColor(basis.Color(color), state, backLeg);
    }
    /// <summary>Independent paint colors plus calculated quarter/fifth shade ramps.</summary>
    private sealed class BasePalette
    {
        /// <summary>Body highlight used at palette index zero.</summary>
        private readonly ushort highlight;
        /// <summary>Body midtone used at palette index one.</summary>
        private readonly ushort midtone;
        /// <summary>Body shadow used at palette index two.</summary>
        private readonly ushort shadow;
        /// <summary>Outline endpoint; rear palettes derive theirs from the front palette.</summary>
        private readonly ushort outline;
        /// <summary>Gray endpoint from which the four plate shades are calculated.</summary>
        private readonly ushort gray;
        /// <summary>Tissue endpoint from which the five brown shades are calculated.</summary>
        private readonly ushort brown;
        /// <summary>Selects rear-leg palette derivations and zero-filled palette positions.</summary>
        private readonly bool backLeg;
        /// <summary>Installed colors retained if native shade calculations do not reproduce every entry.</summary>
        private readonly ushort[]? supplied;
        /// <summary>Front body palette supplying the rear palette's related outline and gray endpoints.</summary>
        private readonly BasePalette? front;

        /// <summary>Canonical palette bases derived from the native body and rear-leg artwork.</summary>
        internal static BasePalette StockBody { get; } = new(false);
        /// <summary>Canonical rear-leg palette, whose outline and gray shades derive from <see cref="StockBody"/>.</summary>
        internal static BasePalette StockRear { get; } = new(true);

        /// <summary>Initializes canonical paint endpoints, linking the rear palette to the stock body palette.</summary>
        /// <param name="backLeg">Selects the rear-leg palette variant; false initializes the body endpoints.</param>
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

        /// <summary>Reuses a stock palette when its entries match the supplied base row; otherwise compiles the installed row.</summary>
        /// <param name="colors">RGB5 base-row values to compare against the selected stock palette.</param>
        /// <param name="backLeg">Selects the body or rear-leg stock palette.</param>
        /// <param name="front">Body palette required to derive custom rear-leg endpoints.</param>
        /// <returns>The matching stock instance or a palette that preserves custom colors.</returns>
        internal static BasePalette Create(ushort[] colors, bool backLeg, BasePalette? front)
        {
            BasePalette stock = backLeg ? StockRear : StockBody;
            bool matches = !backLeg || ReferenceEquals(front, StockBody);
            for (int color = 0; matches && color < colors.Length; color++)
                matches = colors[color] == stock.Color(color);
            return matches ? stock : new BasePalette(colors, backLeg, front);
        }

        /// <summary>Compiles paint endpoints and retains the row if calculated shade ramps would alter installed colors.</summary>
        /// <param name="colors">One base palette row in native fifteen-color order.</param>
        /// <param name="backLeg">Selects rear-leg slots and endpoint derivation.</param>
        /// <param name="front">Body palette used to derive a rear palette's outline and gray shades.</param>
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

        /// <summary>Returns the exact installed entry when required, or the corresponding native shade calculation.</summary>
        /// <param name="color">Index in the fifteen-color base palette.</param>
        public ushort Color(int color) => supplied is null ? Calculate(color) : supplied[color];

        /// <summary>Maps a native palette slot to its endpoint, derived shade, or transparent zero.</summary>
        /// <param name="color">Palette index whose RGB5 value is requested.</param>
        /// <returns>The calculated color for that slot.</returns>
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
        /// <summary>Gets the stored body outline or the rear outline derived by increasing the front green channel.</summary>
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
        /// <summary>Gets the stored body gray endpoint or the half-lit rear endpoint used for rear plate shades.</summary>
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
        /// <summary>Scales each RGB5 channel by a rational shade factor with nearest-integer rounding.</summary>
        /// <param name="source">RGB5 endpoint to scale.</param>
        /// <param name="numerator">Shade multiplier applied independently to each channel.</param>
        /// <param name="denominator">Divisor for the channel scaling factor.</param>
        /// <returns>The independently scaled RGB5 color.</returns>
        private static ushort Shade(ushort source, int numerator, int denominator)
        {
            int result = 0;
            for (int shift = 0; shift < 15; shift += 5)
                result |= ((((source >> shift) & 31) * numerator + denominator / 2) / denominator) << shift;
            return (ushort)result;
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
