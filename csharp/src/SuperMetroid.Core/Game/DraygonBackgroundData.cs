namespace SuperMetroid.Core.Game;

/// <summary>BG2 artwork origin operands used by Draygon's graphics-drawn hook.</summary>
internal static class DraygonBackgroundData
{
    /// <summary>$88:DF94 rejects body screen X below -$40 or at/above $140.</summary>
    public const int MinimumScreenX = -64, MaximumScreenX = 320;
    /// <summary>$88:DF94 rejects body screen Y below -$10 or at/above $130.</summary>
    public const int MinimumScreenY = -16, MaximumScreenY = 304;
    /// <summary>$88:DFC7/$DFCC split top, middle and bottom at body Y $28/$C0.</summary>
    public const int MiddleScreenY = 40, BottomScreenY = 192;
    /// <summary>$88:E013: after the HUD, enable BG2 for $40 physical lines.</summary>
    public const int TopBandEnd = 96;
    /// <summary>$88:E00C: after the HUD, disable BG2 for $60 physical lines.</summary>
    public const int BottomBandStart = 128;
    /// <summary>$A5:9342, Draygon_Func_36: horizontal artwork origin subtracted after camera minus body X.</summary>
    public const int HorizontalOrigin = 450;

    /// <summary>$A5:9342, Draygon_Func_36: vertical artwork origin subtracted after camera minus body Y.</summary>
    public const int VerticalOrigin = 192;
}
