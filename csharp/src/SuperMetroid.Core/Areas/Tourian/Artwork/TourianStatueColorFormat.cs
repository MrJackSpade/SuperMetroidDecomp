namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Versioned, editable Tourian statue visual palette resource.</summary>
public static class TourianStatueColorFormat
{
    /// <summary>Installed editable JSON filename for entrance base/statue bands, boss-eye glows, and grey-transition colors.</summary>
    public const string FileName = "tourian-statue-colors.json";
    /// <summary>Supported schema revision, requiring the exact native palette-band and eye-row geometry.</summary>
    public const int Version = 1;
}
