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
    /// <summary>Resolves the first sixteen copied words through Draygon's primary material paint definition.</summary>
    private readonly DraygonMaterialPaintDefinitions primary;

    /// <summary>Resolves the final eight copied words through the escape-door surface paint definition.</summary>
    private readonly CeresDoorEscapeSurfacePaintDefinitions surface;

    /// <summary>Optional independent override for the copied clear-target word at index sixteen.</summary>
    private readonly ushort? clearEdit;

    /// <summary>Builds the intro paint view while preserving its shared source identities and editable clear target.</summary>
    /// <param name="colors">The 25 CGRAM words copied by the Draygon body initializer, in source order.</param>
    internal DraygonIntroPaintDefinitions(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 25) throw new ArgumentException("Draygon intro requires twenty-five colors.", nameof(colors));
        primary = new(colors[..16]);
        surface = new(colors[17..]);
        clearEdit = colors[16] == DraygonMaterialPaintDefinitions.ClearTarget ? null : colors[16];
    }

    /// <summary>Resolves one copied paint word from its primary, clear-target, or escape-surface source.</summary>
    /// <param name="index">Zero-based index within the 25-word intro paint block.</param>
    /// <returns>The effective color word, including any installed clear-target edit.</returns>
    internal ushort Color(int index)
    {
        if ((uint)index >= 25) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 16) return primary.Color(index);
        if (index == 16) return clearEdit ?? DraygonMaterialPaintDefinitions.ClearTarget;
        return surface.ColorAt(index - 17);
    }
}
