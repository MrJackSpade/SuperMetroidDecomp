namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>The shape of the Tourian statue's installed colours (#1415).</summary>
public static class TourianStatueColorDefinitions
{
    /// <summary>The base decoration's sixteen colours, the whole sprite palette, colour zero included.</summary>
    public const int BaseColorCount = 16;

    /// <summary>The entrance statue's sixteen colours, installed as its whole sprite palette, colour zero included.</summary>
    public const int StatueColorCount = 16;

    /// <summary>The four boss-specific eye glows, ordered Phantoon, Ridley, Draygon, Kraid.</summary>
    public const int EyeRowCount = 4;

    /// <summary>The colours of one eye glow.</summary>
    public const int EyeColorCount = 4;

    /// <summary>The eight grey colours a released statue turns; its animated tiles choose which colours they replace.</summary>
    public const int GreyColorCount = 8;
}
