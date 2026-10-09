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
    /// <summary>Base fade definitions that supply Ridley's body and eye material colors.</summary>
    private readonly CeresRidleyFadeColorDefinitions body, eyes;

    /// <summary>Authored health-row words retained where the material fade calculation does not reproduce the data.</summary>
    private readonly Dictionary<int, ushort> edits = [];

    /// <summary>Builds compact health-palette rows from material fades plus any source-specific word overrides.</summary>
    /// <param name="rows">Three supplied rows of fourteen packed colors, used to retain values not reproduced by the formula.</param>
    /// <param name="body">Fade palette used for Ridley's body materials and the shared membrane target.</param>
    /// <param name="eyes">Fade palette supplying the eye colors appended after the eleven body colors.</param>
    internal CeresRidleyHealthPaintDefinitions(ushort[][] rows,
        CeresRidleyFadeColorDefinitions body, CeresRidleyFadeColorDefinitions eyes)
    {
        this.body = body; this.eyes = eyes;
        for (int row = 0; row < 3; row++) for (int color = 0; color < 14; color++)
            if (Calculate(row, color) != rows[row][color]) edits.Add(row * 14 + color, rows[row][color]);
    }

    /// <summary>Gets a packed health-palette word, preferring an authored override over the derived material value.</summary>
    /// <param name="row">Zero-based health band from zero through two.</param>
    /// <param name="color">Zero-based color slot from zero through thirteen.</param>
    /// <returns>The stored source word or the calculated color for that band and slot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The row or color index is outside the three-by-fourteen palette.</exception>
    internal ushort Resolve(int row, int color)
    {
        if ((uint)row >= 3) throw new ArgumentOutOfRangeException(nameof(row));
        if ((uint)color >= 14) throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(row * 14 + color, out ushort edited) ? edited : Calculate(row, color);
    }

    /// <summary>Blends the selected body or eye color toward the membrane paint using the row's fixed strength.</summary>
    /// <param name="row">Health band selecting the blend strength.</param>
    /// <param name="color">Palette slot selecting a body material or eye color.</param>
    /// <returns>The derived packed color before source-specific overrides are applied.</returns>
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
