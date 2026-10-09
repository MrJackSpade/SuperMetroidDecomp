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
    /// <summary>Maximum intensity of one five-bit SNES RGB channel.</summary>
    private const int MaximumChannel = (1 << 5) - 1;
    /// <summary>Per-phase channel change used by the mirrored five-level beacon fade.</summary>
    private const int FadeStep = 5;
    /// <summary>Red and ordinary green increase applied to the brightest gold facets.</summary>
    private const int GoldBrightening = 3;
    /// <summary>Additional green increase specific to the gold highlight facet.</summary>
    private const int GoldHighlightGreenBrightening = 4;
    /// <summary>Normal door paint channels used as the shared seed for beacon animation rows.</summary>
    private readonly CeresDoorNormalPaintDefinitions normal;
    /// <summary>Blue channel retained from the first authored animation seed.</summary>
    private readonly int firstSeedBlue;
    /// <summary>Independent red channel used by the dim middle amber animation phase.</summary>
    private readonly int dimMiddleAmberRed;
    /// <summary>First-row colors that differ from the seed derived from normal target paint.</summary>
    private readonly Dictionary<int, ushort> seedEdits = [];
    /// <summary>Animation-row colors that differ from the shared fade and facet brightening rules.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Builds the eight-row beacon palette as shared normal-paint channels plus authored exceptions.</summary>
    /// <param name="rows">Eight ordered animation phases, each containing six RGB555 colors.</param>
    /// <param name="normal">Normal target paint whose selected colors seed the animation.</param>
    /// <exception cref="ArgumentException">The animation does not contain exactly eight rows of six colors.</exception>
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

    /// <summary>Returns one base animation color, applying the independently authored first-seed blue when needed.</summary>
    /// <param name="color">Palette position from zero through five.</param>
    /// <returns>The seed RGB555 color used by calculated animation phases.</returns>
    private ushort SharedSeed(int color) => color == 0
        ? (ushort)((normal.ColorAt(8) & 0x3ff) | firstSeedBlue << 10)
        : normal.ColorAt(8 + color);

    /// <summary>Returns an authored row override or the calculated beacon color for one phase and palette position.</summary>
    /// <param name="row">Animation phase index from zero through seven.</param>
    /// <param name="color">Palette position within the six-color row.</param>
    /// <returns>The selected RGB555 color word.</returns>
    /// <exception cref="IndexOutOfRangeException">Either index is outside the authored animation dimensions.</exception>
    internal ushort ColorAt(int row, int color)
    {
        if ((uint)row >= 8 || (uint)color >= 6) throw new IndexOutOfRangeException();
        return edits.TryGetValue(row * 6 + color, out ushort edited) ? edited : Calculate(row, color);
    }

    /// <summary>Mirrors the row phase, applies the five-level fade, and brightens the gold facets at the highlight phase.</summary>
    /// <param name="row">Animation phase index from zero through seven.</param>
    /// <param name="color">Palette position identifying the affected facet.</param>
    /// <returns>The calculated RGB555 color word before any per-cell override.</returns>
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
