using SuperMetroid.Core.Hardware;

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
    /// <summary>Defines maximum.</summary>
    private const int Maximum = (1 << 5) - 1;
    /// <summary>Defines brightness step.</summary>
    private const int BrightnessStep = 3;
    /// <summary>Stores body.</summary>
    private readonly CeresRidleyFadeColorDefinitions? body;
    /// <summary>Stores standalone body.</summary>
    private readonly CeresRidleyBodyPaintDefinitions? standaloneBody;
    /// <summary>Stores row count.</summary>
    private readonly int rowCount;
    /// <summary>Stores glass red.</summary>
    private readonly int glassRed, glassBlue, glassMiddleRed, glassGreenStep;
    /// <summary>Stores dome tint.</summary>
    private readonly int domeTint, domeGreen, domeShadeStep, outlineRed;
    /// <summary>Stores edits.</summary>
    private readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Initializes a new ceres baby paint definitions instance.</summary>
    internal CeresBabyPaintDefinitions(Bgr555[][] rows, CeresRidleyFadeColorDefinitions body)
        : this(rows[0], 4)
    {
        this.body = body;
        for (int row = 0; row < rowCount; row++)
            for (int color = 0; color < 15; color++)
                if (Calculate(row, color) != rows[row][color]) edits.Add(row * 15 + color, rows[row][color]);
    }

    /// <summary>The same material at Norfair Ridley $A6:E1F1-E20E, with its own supplied values.</summary>
    internal CeresBabyPaintDefinitions(ReadOnlySpan<Bgr555> initial)
        : this(initial, 1)
    {
        standaloneBody = new(initial[4], initial[5], initial[6]);
        for (int color = 0; color < 15; color++)
            if (Calculate(0, color) != initial[color]) edits.Add(color, initial[color]);
    }

    /// <summary>Initializes a new ceres baby paint definitions instance.</summary>
    private CeresBabyPaintDefinitions(ReadOnlySpan<Bgr555> initial, int rowCount)
    {
        if (initial.Length != 15) throw new ArgumentException("Baby paint requires fifteen colors.", nameof(initial));
        this.rowCount = rowCount;
        glassRed = initial[0].Red; glassBlue = initial[0].Blue;
        glassMiddleRed = initial[12].Red; glassGreenStep = Maximum - initial[12].Green;
        domeTint = initial[1].Red; domeGreen = initial[1].Green;
        domeShadeStep = domeGreen - initial[2].Green; outlineRed = initial[8].Red;
    }

    /// <summary>Resolve.</summary>
    internal Bgr555 Resolve(int row, int color)
    {
        if ((uint)row >= rowCount) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= 15) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(row * 15 + color, out Bgr555 edited) ? edited : Calculate(row, color);
    }

    /// <summary>Calculate.</summary>
    private Bgr555 Calculate(int row, int color)
    {
        Bgr555 basis = Initial(color);
        int phase = Math.Max(0, row - 1);
        int addition = BrightnessStep * phase;
        // The selected tooth pulse delays red by one pose step while G/B brighten.
        int redAddition = color is >= 9 and <= 11 ? BrightnessStep * Math.Max(0, phase - 1) : addition;
        int green = basis.Green + addition, blue = basis.Blue + addition;
        // Cyan glass stays one channel unit below the white glints at its peak.
        if (color == 12)
        {
            green = Math.Min(Maximum - 1, green);
            blue = Math.Min(Maximum - 1, blue);
        }
        return Bgr555.Saturating(basis.Red + redAddition, green, blue);
    }

    /// <summary>Initial.</summary>
    private Bgr555 Initial(int color) => color switch
    {
        0 => Pack(glassRed, Maximum, glassBlue),
        >= 1 and <= 3 => Pack(domeTint, domeGreen - domeShadeStep * (color - 1), domeTint),
        >= 4 and <= 6 => BodyColor(color + 4),
        7 => Pack(BodyColor(10).Red / 2, 0, BodyColor(10).Blue / 2),
        8 => Pack(outlineRed, 0, BodyColor(10).Blue / 4),
        9 => BabyMetroidInitialPaintDefinitions.FangLight,
        10 => BabyMetroidInitialPaintDefinitions.FangLight.Zip(BabyMetroidInitialPaintDefinitions.FangDark,
            (_, light, dark) => (light + dark) / 2),
        11 => BabyMetroidInitialPaintDefinitions.FangDark,
        12 => Pack(glassMiddleRed, Maximum - glassGreenStep, Maximum - glassGreenStep),
        13 => Pack(Maximum, Maximum, Maximum),
        _ => Pack(0, Maximum - 2 * glassGreenStep, Maximum - 2 * glassGreenStep),
    };

    /// <summary>Body color.</summary>
    private Bgr555 BodyColor(int color) => body is not null ? body.Resolve(15, color) : standaloneBody!.Color(color);

    /// <summary>Pack.</summary>
    private static Bgr555 Pack(int red, int green, int blue) => Bgr555.Saturating(red, green, blue);
}
