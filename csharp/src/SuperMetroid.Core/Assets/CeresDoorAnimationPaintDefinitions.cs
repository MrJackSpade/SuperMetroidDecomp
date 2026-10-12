using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Elevator beacon colors at $A6:F871-F8EC, copied to CGRAM41..46 by $A6:F850-F86F.
/// White/rust/red surfaces and three gold facets share normal target paint identities.
/// Mirrored phases use a five-level fade; the brightest gold facets preserve blue
/// while brightening red/green by three, with a four-level green highlight.
/// Only first-seed blue17 and the dimmest middle-amber red14 remain independent
/// paint channels. These and the named brightening/phase policies are selected
/// beacon paint composition, not a claim about pixel art or actor cadence.
/// </summary>
internal sealed class CeresDoorAnimationPaintDefinitions
{
    /// <summary>Stores maximum channel.</summary>
    private const int MaximumChannel = (1 << 5) - 1;
    /// <summary>Stores fade step.</summary>
    private const int FadeStep = 5;
    /// <summary>Stores gold brightening.</summary>
    private const int GoldBrightening = 3;
    /// <summary>Stores gold highlight green brightening.</summary>
    private const int GoldHighlightGreenBrightening = 4;
    /// <summary>Stores normal.</summary>
    private readonly CeresDoorNormalPaintDefinitions normal;
    /// <summary>Stores first seed blue.</summary>
    private readonly int firstSeedBlue;
    /// <summary>Stores dim middle amber red.</summary>
    private readonly int dimMiddleAmberRed;
    /// <summary>Stores seed edits.</summary>
    private readonly Dictionary<int, Bgr555> seedEdits = [];
    /// <summary>Stores edits.</summary>
    private readonly Dictionary<int, Bgr555> edits = [];

    /// <summary>Initializes a new ceres door animation paint definitions instance.</summary>
    internal CeresDoorAnimationPaintDefinitions(Bgr555[][] rows, CeresDoorNormalPaintDefinitions normal)
    {
        Ensure.NotNull(rows); Ensure.NotNull(normal);
        if (rows.Length != 8 || rows.Any(row => row is null || row.Length != 6))
            throw new ArgumentException("Beacon paint requires eight six-color rows.", nameof(rows));
        this.normal = normal;
        firstSeedBlue = rows[1][0].Blue;
        dimMiddleAmberRed = rows[3][4].Red;
        for (int color = 0; color < 6; color++)
            if (rows[1][color] != SharedSeed(color)) seedEdits.Add(color, rows[1][color]);
        for (int row = 0; row < 8; row++)
            for (int color = 0; color < 6; color++)
                if (rows[row][color] != Calculate(row, color)) edits.Add(row * 6 + color, rows[row][color]);
    }

    /// <summary>Shared seed.</summary>
    private Bgr555 SharedSeed(int color)
    {
        if (color != 0) return normal.ColorAt(8 + color);
        Bgr555 normalSeed = normal.ColorAt(8);
        return new(normalSeed.Red, normalSeed.Green, firstSeedBlue);
    }

    /// <summary>Color at.</summary>
    internal Bgr555 ColorAt(int row, int color)
    {
        if ((uint)row >= 8 || (uint)color >= 6) throw new IndexOutOfRangeException();
        return edits.TryGetValue(row * 6 + color, out Bgr555 edited) ? edited : Calculate(row, color);
    }

    /// <summary>Calculates.</summary>
    private Bgr555 Calculate(int row, int color)
    {
        int phase = Math.Min(row, 7 - row);
        Bgr555 seed = seedEdits.TryGetValue(color, out Bgr555 edited) ? edited : SharedSeed(color);
        int red = seed.Red, green = seed.Green, blue = seed.Blue;
        if (phase == 0 && color >= 3)
        {
            red = Math.Min(MaximumChannel, red + GoldBrightening);
            green = Math.Min(MaximumChannel, green + (color == 3 ? GoldHighlightGreenBrightening : GoldBrightening));
        }
        else
        {
            int delta = FadeStep * (1 - phase);
            red = Math.Clamp(red + delta, 0, MaximumChannel);
            green = Math.Clamp(green + delta, 0, MaximumChannel);
            blue = Math.Clamp(blue + delta, 0, MaximumChannel);
            if (phase == 3 && color == 4) red = dimMiddleAmberRed;
        }
        return new(red, green, blue);
    }
}
