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
    private const int Maximum = (1 << 5) - 1;
    private const int BrightnessStep = 3;
    private readonly CeresRidleyFadeColorDefinitions body;
    private readonly int glassRed, glassBlue, glassMiddleRed, glassGreenStep;
    private readonly int domeTint, domeGreen, domeShadeStep, outlineRed;
    private readonly Dictionary<int, ushort> edits = [];

    internal CeresBabyPaintDefinitions(ushort[][] rows, CeresRidleyFadeColorDefinitions body)
    {
        this.body = body;
        ushort[] initial = rows[0];
        glassRed = Channel(initial[0], 0); glassBlue = Channel(initial[0], 2);
        glassMiddleRed = Channel(initial[12], 0); glassGreenStep = Maximum - Channel(initial[12], 1);
        domeTint = Channel(initial[1], 0); domeGreen = Channel(initial[1], 1);
        domeShadeStep = domeGreen - Channel(initial[2], 1); outlineRed = Channel(initial[8], 0);
        for (int row = 0; row < 4; row++)
            for (int color = 0; color < 15; color++)
                if (Calculate(row, color) != rows[row][color]) edits.Add(row * 15 + color, rows[row][color]);
    }

    internal ushort Resolve(int row, int color)
    {
        if ((uint)row >= 4) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= 15) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(row * 15 + color, out ushort edited) ? edited : Calculate(row, color);
    }

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

    private ushort Initial(int color) => color switch
    {
        0 => Pack(glassRed, Maximum, glassBlue),
        >= 1 and <= 3 => Pack(domeTint, domeGreen - domeShadeStep * (color - 1), domeTint),
        >= 4 and <= 6 => body.Resolve(15, color + 4),
        7 => Pack(Channel(body.Resolve(15, 10), 0) / 2, 0, Channel(body.Resolve(15, 10), 2) / 2),
        8 => Pack(outlineRed, 0, Channel(body.Resolve(15, 10), 2) / 4),
        9 => BabyMetroidInitialPaintDefinitions.FangLight,
        10 => Pack((Channel(BabyMetroidInitialPaintDefinitions.FangLight, 0) + Channel(BabyMetroidInitialPaintDefinitions.FangDark, 0)) / 2,
            (Channel(BabyMetroidInitialPaintDefinitions.FangLight, 1) + Channel(BabyMetroidInitialPaintDefinitions.FangDark, 1)) / 2,
            (Channel(BabyMetroidInitialPaintDefinitions.FangLight, 2) + Channel(BabyMetroidInitialPaintDefinitions.FangDark, 2)) / 2),
        11 => BabyMetroidInitialPaintDefinitions.FangDark,
        12 => Pack(glassMiddleRed, Maximum - glassGreenStep, Maximum - glassGreenStep),
        13 => Pack(Maximum, Maximum, Maximum),
        _ => Pack(0, Maximum - 2 * glassGreenStep, Maximum - 2 * glassGreenStep),
    };

    private static int Channel(ushort color, int channel) => color >> (channel * 5) & Maximum;
    private static ushort Pack(int red, int green, int blue) => (ushort)(Math.Clamp(red, 0, Maximum)
        | Math.Clamp(green, 0, Maximum) << 5 | Math.Clamp(blue, 0, Maximum) << 10);
}
