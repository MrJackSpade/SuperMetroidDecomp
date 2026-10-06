namespace SuperMetroid.Core.Assets;

/// <summary>
/// Ridley health colors $A6:E46A-E4BD, uniformly copied by $A6:D495-D4B4.
/// The selected armor/eye color composition tints toward the existing middle
/// membrane paint at $A6:E466, with strengths2/20,3/20,4/20. Recessed armor green
/// holds its healthy shade in the first band; middle plate blue holds the preceding
/// shade in the second band. Those two chosen material/phase memberships preserve
/// the painted shading, with channel magnitudes derived from their previous shade.
/// No new paint magnitudes, health thresholds, timing or pixels are retained here.
/// </summary>
internal sealed class CeresRidleyHealthPaintDefinitions
{
    private readonly CeresRidleyFadeColorDefinitions body, eyes;
    private readonly Dictionary<int, ushort> edits = [];

    internal CeresRidleyHealthPaintDefinitions(ushort[][] rows,
        CeresRidleyFadeColorDefinitions body, CeresRidleyFadeColorDefinitions eyes)
    {
        this.body = body; this.eyes = eyes;
        for (int row = 0; row < 3; row++) for (int color = 0; color < 14; color++)
            if (Calculate(row, color) != rows[row][color]) edits.Add(row * 14 + color, rows[row][color]);
    }

    internal ushort Resolve(int row, int color)
    {
        if ((uint)row >= 3) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= 14) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(row * 14 + color, out ushort edited) ? edited : Calculate(row, color);
    }

    private ushort Calculate(int row, int color)
    {
        ushort healthy = color < 11 ? body.Resolve(15, color) : eyes.Resolve(0, color - 11);
        ushort target = body.Resolve(15, 9);
        int strength = 2 + row, result = 0;
        for (int channel = 0; channel < 3; channel++)
        {
            int shift = channel * 5;
            int original = healthy >> shift & 31, toward = target >> shift & 31;
            int value = (original * (20 - strength) + toward * strength) / 20;
            if (row == 0 && color == 2 && channel == 1) value = original;
            if (row == 1 && color == 5 && channel == 2) value = Calculate(0, color) >> shift & 31;
            result |= value << shift;
        }
        return (ushort)result;
    }
}
