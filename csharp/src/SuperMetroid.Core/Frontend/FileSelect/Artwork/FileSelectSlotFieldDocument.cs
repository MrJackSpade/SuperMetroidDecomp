namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>Where one save slot's field is shown: each anchor is the page pixel a picture's origin lands on.</summary>
public sealed record FileSelectSlotFieldDocument
{
    /// <summary>Where the ENERGY label is shown; the slot's energy tanks are shown from it.</summary>
    public required Position16<PlanePixels> EnergyAnchor { get; init; }
    /// <summary>Where the first of the two health digits is shown; the second follows one digit's width right.</summary>
    public required Position16<PlanePixels> HealthAnchor { get; init; }
    /// <summary>Where NO DATA is shown, when the slot holds no save.</summary>
    public required Position16<PlanePixels> NoDataAnchor { get; init; }
    /// <summary>Where the HH:MM time's first hour digit is shown; the colon and minutes follow, one glyph's width apart.</summary>
    public required Position16<PlanePixels> TimeValueAnchor { get; init; }
}
