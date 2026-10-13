using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// $A5:A217-A248, copied uniformly to CGRAM144..168 by InitAI_DraygonBody
/// ($A5:8687). First16 words alias the primary material, the next is its clear
/// target, and the final8 alias escape-door surface paint ($A6:F50E-F51D).
/// These existing source identities introduce no new independent paint inputs.
/// Installed occurrences retain independent edits, including the copied clear target.
/// </summary>
internal sealed class DraygonIntroPaintDefinitions
{
    /// <summary>First sixteen installed colors, shared in identity with Draygon's primary material.</summary>
    internal readonly DraygonMaterialPaintDefinitions primary;
    /// <summary>Final eight colors, aliased to the Ceres escape-door surface material.</summary>
    internal readonly CeresDoorEscapeSurfacePaintDefinitions surface;
    /// <summary>Independent edit of the copied clear target, or null when it retains the native value.</summary>
    internal readonly Bgr555? clearEdit;

    /// <summary>Splits the 25 copied CGRAM colors into primary, clear-target, and door-surface aliases.</summary>
    internal DraygonIntroPaintDefinitions(ReadOnlySpan<Bgr555> colors)
    {
        Ensure.LengthEqual(colors, 25);
        primary = new(colors[..16]);
        surface = new(colors[17..]);
        clearEdit = colors[16] == DraygonMaterialPaintDefinitions.ClearTarget ? null : colors[16];
    }

    /// <summary>Resolves a copied color through its owning material while preserving an edited clear target.</summary>
    internal Bgr555 Color(int index)
    {
        if ((uint)index >= 25) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 16) return primary.Color(index);
        if (index == 16) return clearEdit ?? DraygonMaterialPaintDefinitions.ClearTarget;
        return surface.ColorAt(index - 17);
    }
}
