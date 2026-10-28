namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>Draygon's four belly-material shades across eight health bands at $A5:96AF-96EE: the selected gold and critical-red channels <see cref="DraygonHealthPaint"/> calculates them from.</summary>
internal static class DraygonHealthPaintDefinitions
{
    /// <summary>Four belly shades are copied in each health band.</summary>
    internal const int Shades = 4;
    /// <summary>Eight health-band rows span healthy gold through critical red.</summary>
    internal const int Bands = 8;
    /// <summary>Five-bit red channel ceiling used by the critical endpoint.</summary>
    internal const int Maximum = (1 << 5) - 1;
    /// <summary>
    /// $A5:96AF-96B6 (also normal palette slots9..12 at $A5:A289-A290):
    /// selected gold material steps red5/green6. Red leads green by one shade,
    /// producing red25..10 and green24..6; blue is zero. These material choices
    /// describe categorical belly paint. Replacing these gold-channel choices,
    /// the red-leading shade assignment or critical pure-red composition would
    /// invent different health-indicator artwork; the uniform copy supplies no
    /// rule selecting alternate paint. Pixels and health thresholds are separate.
    /// </summary>
    internal const int HealthyRedStep = 5;
    /// <summary>Healthy gold shades step down green by six per shade.</summary>
    internal const int HealthyGreenStep = 6;
}
