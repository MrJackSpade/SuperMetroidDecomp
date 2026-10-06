namespace SuperMetroid.Core.Assets;

/// <summary>
/// Shared primary Draygon paint at $A5:A1F7-A236 and $A5:A277-A296.
/// Highlights, outlines and clipped green-carapace ramps are selected material
/// content; the four gold belly shades use their existing health-paint owner.
/// Replacing the selected highlight/outline/eye levels or these precise material
/// groupings would invent different categorical paint. Copied clear/green targets
/// are bounded source metadata, not values inferred from transparency or absence.
/// Each installed occurrence preserves independently supplied channel values.
/// </summary>
internal sealed class DraygonMaterialPaintDefinitions
{
    /// <summary>$A5:A277: uniformly copied clear-slot target blue14, not derived from transparency.</summary>
    private const int ClearBlue = 14;
    /// <summary>$A5:A279-A27C: categorical light/secondary carapace highlight channels.</summary>
    private const int HighlightRed = 23, HighlightGreen = 26, HighlightBlue = 15,
        SecondaryRed = 13, SecondaryBlue = 11;
    /// <summary>$A5:A27D-A280: selected dark-green contour/outline colors; blue is zero.</summary>
    private const int ContourRed = 2, OutlineGreen = 3;
    /// <summary>
    /// $A5:A281-A288: four carapace midtones. Red declines by5 to floor4;
    /// green shares red plus5, also used by the secondary highlight and contour.
    /// Blue halves from the shared intensity16, clipped
    /// to selected highlight14. These exact channel dependencies calculate.
    /// </summary>
    private const int MiddlePeak = 16, MiddleRedFloor = 4,
        ChannelBias = 5, MiddleBlueCeiling = 14;
    /// <summary>
    /// $A5:A291-A296: magenta/red eye paint (native frame $A5:A37F, stream
    /// $A5:B42C). Magenta red shares blue plus the five-unit channel separation.
    /// The final green target shares that bias but is separately copied metadata,
    /// absent from the bounded Draygon BG2/sprite pixel domain. Its assignment
    /// remains observable through the uniform CGRAM copy.
    /// </summary>
    private const int MagentaBlue = 22,
        RedTargetRed = 18, RedTargetGreen = 4, RedTargetBlue = 6;
    private readonly Dictionary<int, ushort> edits = [];

    internal DraygonMaterialPaintDefinitions(ReadOnlySpan<ushort> colors)
    {
        if (colors.Length != 16) throw new ArgumentException("Draygon material requires sixteen colors.", nameof(colors));
        for (int color = 0; color < colors.Length; color++)
            if (colors[color] != StockColor(color)) edits.Add(color, colors[color]);
    }

    internal ushort Color(int index)
    {
        if ((uint)index >= 16) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(index, out ushort edited) ? edited : StockColor(index);
    }

    internal static ushort ClearTarget => Pack(0, 0, ClearBlue);

    private static ushort StockColor(int color)
    {
        if (color is >= 5 and <= 8)
        {
            int shade = color - 5;
            int red = Math.Max(MiddleRedFloor, MiddlePeak - ChannelBias * shade);
            return Pack(red, red + ChannelBias, Math.Min(MiddleBlueCeiling, MiddlePeak >> shade));
        }
        if (color is >= 9 and <= 12) return DraygonHealthPaintDefinitions.HealthyShade(color - 9);
        return color switch
        {
            0 => ClearTarget,
            1 => Pack(HighlightRed, HighlightGreen, HighlightBlue),
            2 => Pack(SecondaryRed, SecondaryRed + ChannelBias, SecondaryBlue),
            3 => Pack(ContourRed, ContourRed + ChannelBias, 0),
            4 => Pack(0, OutlineGreen, 0),
            13 => Pack(MagentaBlue + ChannelBias, 0, MagentaBlue),
            14 => Pack(RedTargetRed, RedTargetGreen, RedTargetBlue),
            15 => Pack(ChannelBias, ChannelBias * 2, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    private static ushort Pack(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}
