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
    private const int MaximumChannel = (1 << 5) - 1;
    private const int FadeStep = 5;
    private const int GoldBrightening = 3;
    private const int GoldHighlightGreenBrightening = 4;
    private readonly CeresDoorNormalPaintDefinitions normal;
    private readonly int firstSeedBlue;
    private readonly int dimMiddleAmberRed;
    private readonly Dictionary<int, ushort> seedEdits = [];
    private readonly Dictionary<int, ushort> edits = [];

    internal CeresDoorAnimationPaintDefinitions(ushort[][] rows, CeresDoorNormalPaintDefinitions normal)
    {
        Ensure.NotNull(rows); Ensure.NotNull(normal);
        if (rows.Length != 8 || rows.Any(row => row is null || row.Length != 6))
            throw new ArgumentException("Beacon paint requires eight six-color rows.", nameof(rows));
        this.normal = normal;
        firstSeedBlue = rows[1][0] >> 10;
        dimMiddleAmberRed = rows[3][4] & MaximumChannel;
        for (int color = 0; color < 6; color++)
            if (rows[1][color] != SharedSeed(color)) seedEdits.Add(color, rows[1][color]);
        for (int row = 0; row < 8; row++)
            for (int color = 0; color < 6; color++)
                if (rows[row][color] != Calculate(row, color)) edits.Add(row * 6 + color, rows[row][color]);
    }

    private ushort SharedSeed(int color) => color == 0
        ? (ushort)((normal.ColorAt(8) & 0x3ff) | firstSeedBlue << 10)
        : normal.ColorAt(8 + color);

    internal ushort ColorAt(int row, int color)
    {
        if ((uint)row >= 8 || (uint)color >= 6) throw new IndexOutOfRangeException();
        return edits.TryGetValue(row * 6 + color, out ushort edited) ? edited : Calculate(row, color);
    }

    private ushort Calculate(int row, int color)
    {
        int phase = Math.Min(row, 7 - row);
        ushort seed = seedEdits.TryGetValue(color, out ushort edited) ? edited : SharedSeed(color);
        int red = seed & MaximumChannel, green = seed >> 5 & MaximumChannel, blue = seed >> 10;
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
        return (ushort)(red | green << 5 | blue << 10);
    }
}
