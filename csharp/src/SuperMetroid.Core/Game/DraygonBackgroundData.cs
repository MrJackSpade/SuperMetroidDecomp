namespace SuperMetroid.Core.Game;

/// <summary>BG2 artwork origin operands used by Draygon's graphics-drawn hook.</summary>
internal static class DraygonBackgroundData
{
    /// <summary>$A5:9342, Draygon_Func_36: horizontal artwork origin subtracted after camera minus body X.</summary>
    public const int HorizontalOrigin = 450;

    /// <summary>$A5:9342, Draygon_Func_36: vertical artwork origin subtracted after camera minus body Y.</summary>
    public const int VerticalOrigin = 192;
}
