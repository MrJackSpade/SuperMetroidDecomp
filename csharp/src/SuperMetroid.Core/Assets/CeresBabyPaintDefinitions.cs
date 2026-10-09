namespace SuperMetroid.Core.Assets;

/// <summary>
/// Ceres Baby/container palettes $A6:E1F1-E268. $A6:BF5D-BFBF selects the
/// horizontal/round/vertical brightness with the matching OAM shape; $A6:BFE1
/// copies fifteen colors uniformly. Shared organ and fang source paints calculate
/// from existing owners. Eight selected glass/dome/outline inputs and the exact
/// clipped three-unit pose brightening specify this reviewed material composition,
/// not pixels or frame timing. Changing these choices invents different paint.
/// Supplied source and output edits remain independently observable.
/// </summary>
internal sealed class CeresBabyPaintDefinitions
{
    /// <summary>Maximum value of a five-bit SNES color channel.</summary>
    private const int Maximum = (1 << 5) - 1;
    /// <summary>Per-pose channel increase used by the Baby/container material's brightening ramp.</summary>
    private const int BrightnessStep = 3;
    /// <summary>Optional source palette for organ and body colors shared with the active Ridley material.</summary>
    private readonly CeresRidleyFadeColorDefinitions? body;
    /// <summary>Optional standalone body-color source used when this paint is resolved independently.</summary>
    private readonly CeresRidleyBodyPaintDefinitions? standaloneBody;
    /// <summary>Number of pose rows accepted by <see cref="Resolve"/> for this instance.</summary>
    private readonly int rowCount;
    /// <summary>Selected red and blue channels for the glass highlights and their middle row.</summary>
    private readonly int glassRed, glassBlue, glassMiddleRed, glassGreenStep;
    /// <summary>Selected dome tint and shading ramp, plus the outline's red channel.</summary>
    private readonly int domeTint, domeGreen, domeShadeStep, outlineRed;
    /// <summary>Explicit source-palette overrides keyed by flattened row and color index.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Builds a multi-pose paint model while preserving supplied colors that differ from its calculated ramp.</summary>
    /// <param name="rows">Palette rows authored for the poses; each row contains fifteen SNES colors.</param>
    /// <param name="body">Shared Ridley paint used to resolve the baby body's underlying colors.</param>
    internal CeresBabyPaintDefinitions(ushort[][] rows, CeresRidleyFadeColorDefinitions body)
        : this(rows[0], 4)
    {
        this.body = body;
        for (int row = 0; row < rowCount; row++)
            for (int color = 0; color < 15; color++)
                if (Calculate(row, color) != rows[row][color]) edits.Add(row * 15 + color, rows[row][color]);
    }

    /// <summary>Builds a single-pose paint model from the Norfair Ridley material at <c>$A6:E1F1-E20E</c>, retaining its supplied palette values.</summary>
    /// <param name="initial">The fifteen initial SNES colors from the material's palette.</param>
    internal CeresBabyPaintDefinitions(ReadOnlySpan<ushort> initial)
        : this(initial, 1)
    {
        standaloneBody = new(initial[4], initial[5], initial[6]);
        for (int color = 0; color < 15; color++)
            if (Calculate(0, color) != initial[color]) edits.Add(color, initial[color]);
    }

    /// <summary>Extracts the material channels that define the calculated palette ramp.</summary>
    /// <param name="initial">The first pose's fifteen SNES colors.</param>
    /// <param name="rowCount">Number of pose rows represented by the palette source.</param>
    private CeresBabyPaintDefinitions(ReadOnlySpan<ushort> initial, int rowCount)
    {
        if (initial.Length != 15) throw new ArgumentException("Baby paint requires fifteen colors.", nameof(initial));
        this.rowCount = rowCount;
        glassRed = Channel(initial[0], 0); glassBlue = Channel(initial[0], 2);
        glassMiddleRed = Channel(initial[12], 0); glassGreenStep = Maximum - Channel(initial[12], 1);
        domeTint = Channel(initial[1], 0); domeGreen = Channel(initial[1], 1);
        domeShadeStep = domeGreen - Channel(initial[2], 1); outlineRed = Channel(initial[8], 0);
    }

    /// <summary>Returns an authored override or calculates the color at a pose row and palette index.</summary>
    /// <param name="row">Zero-based pose row within this paint model.</param>
    /// <param name="color">Zero-based color index from the fifteen-color palette.</param>
    /// <returns>The resolved packed SNES color.</returns>
    internal ushort Resolve(int row, int color)
    {
        if ((uint)row >= rowCount) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= 15) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(row * 15 + color, out ushort edited) ? edited : Calculate(row, color);
    }

    /// <summary>Applies the pose brightening ramp, including the delayed tooth-red pulse and capped cyan glass.</summary>
    /// <param name="row">Zero-based pose row used to derive the brightness phase.</param>
    /// <param name="color">Palette index whose material channels are calculated.</param>
    /// <returns>The calculated packed SNES color before any explicit row override.</returns>
    private ushort Calculate(int row, int color)
    {
        ushort basis = Initial(color);
        int phase = Math.Max(0, row - 1);
        int addition = BrightnessStep * phase;
        // The selected tooth pulse delays red by one pose step while G/B brighten.
        int redAddition = color is >= 9 and <= 11 ? BrightnessStep * Math.Max(0, phase - 1) : addition;
        int green = Channel(basis, 1) + addition, blue = Channel(basis, 2) + addition;
        // Cyan glass stays one channel unit below the white glints at its peak.
        if (color == 12)
        {
            green = Math.Min(Maximum - 1, green);
            blue = Math.Min(Maximum - 1, blue);
        }
        return Pack(Channel(basis, 0) + redAddition, green, blue);
    }

    /// <summary>Constructs the base material color for each glass, dome, body, fang, or highlight palette index.</summary>
    /// <param name="color">Zero-based index in the fifteen-color source palette.</param>
    /// <returns>The base packed SNES color before pose brightening.</returns>
    private ushort Initial(int color) => color switch
    {
        0 => Pack(glassRed, Maximum, glassBlue),
        >= 1 and <= 3 => Pack(domeTint, domeGreen - domeShadeStep * (color - 1), domeTint),
        >= 4 and <= 6 => BodyColor(color + 4),
        7 => Pack(Channel(BodyColor(10), 0) / 2, 0, Channel(BodyColor(10), 2) / 2),
        8 => Pack(outlineRed, 0, Channel(BodyColor(10), 2) / 4),
        9 => BabyMetroidInitialPaintDefinitions.FangLight,
        10 => Pack((Channel(BabyMetroidInitialPaintDefinitions.FangLight, 0) + Channel(BabyMetroidInitialPaintDefinitions.FangDark, 0)) / 2,
            (Channel(BabyMetroidInitialPaintDefinitions.FangLight, 1) + Channel(BabyMetroidInitialPaintDefinitions.FangDark, 1)) / 2,
            (Channel(BabyMetroidInitialPaintDefinitions.FangLight, 2) + Channel(BabyMetroidInitialPaintDefinitions.FangDark, 2)) / 2),
        11 => BabyMetroidInitialPaintDefinitions.FangDark,
        12 => Pack(glassMiddleRed, Maximum - glassGreenStep, Maximum - glassGreenStep),
        13 => Pack(Maximum, Maximum, Maximum),
        _ => Pack(0, Maximum - 2 * glassGreenStep, Maximum - 2 * glassGreenStep),
    };

    /// <summary>Resolves a body palette color through the shared Ridley source selected for this instance.</summary>
    /// <param name="color">Color index in the source body's palette.</param>
    /// <returns>The packed SNES body color.</returns>
    private ushort BodyColor(int color) => body is not null ? body.Resolve(15, color) : standaloneBody!.Color(color);

    /// <summary>Extracts one five-bit SNES color component from a packed word.</summary>
    /// <param name="color">Packed SNES color in BGR component order.</param>
    /// <param name="channel">Component index: zero for red, one for green, or two for blue.</param>
    /// <returns>The unsigned five-bit component value.</returns>
    private static int Channel(ushort color, int channel) => color >> (channel * 5) & Maximum;

    /// <summary>Clamps and combines five-bit red, green, and blue components into a SNES color word.</summary>
    /// <param name="red">Red component, clamped to the hardware's five-bit range.</param>
    /// <param name="green">Green component, clamped to the hardware's five-bit range.</param>
    /// <param name="blue">Blue component, clamped to the hardware's five-bit range.</param>
    /// <returns>The packed SNES color.</returns>
    private static ushort Pack(int red, int green, int blue) => (ushort)(Math.Clamp(red, 0, Maximum)
        | Math.Clamp(green, 0, Maximum) << 5 | Math.Clamp(blue, 0, Maximum) << 10);
}
