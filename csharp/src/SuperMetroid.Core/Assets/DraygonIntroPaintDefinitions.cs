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
    /// <summary>Stores primary.</summary>
    private readonly DraygonMaterialPaintDefinitions primary;
    /// <summary>Stores surface.</summary>
    private readonly CeresDoorEscapeSurfacePaintDefinitions surface;
    /// <summary>Stores clear edit.</summary>
    private readonly Bgr555? clearEdit;

    /// <summary>Initializes a new draygon intro paint definitions instance.</summary>
    internal DraygonIntroPaintDefinitions(ReadOnlySpan<Bgr555> colors)
    {
        Ensure.LengthEqual(colors, 25);
        primary = new(colors[..16]);
        surface = new(colors[17..]);
        clearEdit = colors[16] == DraygonMaterialPaintDefinitions.ClearTarget ? null : colors[16];
    }

    /// <summary>color.</summary>
    internal Bgr555 Color(int index)
    {
        if ((uint)index >= 25) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 16) return primary.Color(index);
        if (index == 16) return clearEdit ?? DraygonMaterialPaintDefinitions.ClearTarget;
        return surface.ColorAt(index - 17);
    }
}