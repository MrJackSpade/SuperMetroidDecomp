namespace SuperMetroid.AssetExtraction.Areas.Tourian.Artwork;

/// <summary>Versioned, editable Tourian statue visual palette resource.</summary>
public static class TourianStatueColorFormat
{
    /// <summary>Installed editable JSON filename for entrance base/statue bands, boss-eye glows, and grey-transition colors.</summary>
    public const string FileName = "tourian-statue-colors.json";
    /// <summary>Supported schema revision, requiring the exact palette-band and eye-row geometry.</summary>
    public const int Version = 1;
    /// <summary>The base decoration's sixteen colours, the whole sprite palette, colour zero included.</summary>
    public const int BaseColorCount = 16;
    /// <summary>The entrance statue's sixteen colours, installed as its whole sprite palette, colour zero included.</summary>
    public const int StatueColorCount = 16;
}
