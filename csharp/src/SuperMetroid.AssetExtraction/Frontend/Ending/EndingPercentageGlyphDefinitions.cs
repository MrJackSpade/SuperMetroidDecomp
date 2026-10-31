using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>The map words of the item percentage's two-row digits and percent sign, which the import pictures (#1416, #1467).</summary>
internal static class EndingPercentageGlyphDefinitions
{
    /// <summary>The decimal digits the percentage shows.</summary>
    internal const int Digits = 10;

    /// <summary>The upper half of percentage digit zero; the next nine characters show digits one through nine.</summary>
    internal static BackgroundTile DigitTop { get; } = new(96, 6, highPriority: true);

    /// <summary>The lower half of percentage digit zero, one row below its upper half.</summary>
    internal static BackgroundTile DigitBottom { get; } = new(112, 6, highPriority: true);

    /// <summary>The upper half of the percent sign, after the three digits.</summary>
    internal static BackgroundTile PercentTop { get; } = new(106, 6, highPriority: true);

    /// <summary>The lower half of the percent sign, one row below its upper half.</summary>
    internal static BackgroundTile PercentBottom { get; } = new(122, 6, highPriority: true);
}
