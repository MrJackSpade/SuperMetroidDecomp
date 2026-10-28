namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>The selected channels of Draygon's primary material at $A5:A1F7-A236 and $A5:A277-A296; <see cref="DraygonMaterialPaint"/> calculates the material's ramps from them.</summary>
internal static class DraygonMaterialPaintDefinitions
{
    /// <summary>$A5:A277: uniformly copied clear-slot target blue14, not derived from transparency.</summary>
    internal const int ClearBlue = 14;
    /// <summary>$A5:A279-A27C: categorical light/secondary carapace highlight channels.</summary>
    internal const int HighlightRed = 23, HighlightGreen = 26, HighlightBlue = 15,
        SecondaryRed = 13, SecondaryBlue = 11;
    /// <summary>$A5:A27D-A280: selected dark-green contour/outline colors; blue is zero.</summary>
    internal const int ContourRed = 2, OutlineGreen = 3;
    /// <summary>
    /// $A5:A281-A288: four carapace midtones. Red declines by5 to floor4;
    /// green shares red plus5, also used by the secondary highlight and contour.
    /// Blue halves from the shared intensity16, clipped
    /// to selected highlight14. These exact channel dependencies calculate.
    /// </summary>
    internal const int MiddlePeak = 16, MiddleRedFloor = 4,
        ChannelBias = 5, MiddleBlueCeiling = 14;
    /// <summary>
    /// $A5:A291-A296: magenta/red eye paint (native frame $A5:A37F, stream
    /// $A5:B42C). Magenta red shares blue plus the five-unit channel separation.
    /// The final green target shares that bias but is separately copied metadata,
    /// absent from the bounded Draygon BG2/sprite pixel domain. Its assignment
    /// remains observable through the uniform CGRAM copy.
    /// </summary>
    internal const int MagentaBlue = 22,
        RedTargetRed = 18, RedTargetGreen = 4, RedTargetBlue = 6;
}
