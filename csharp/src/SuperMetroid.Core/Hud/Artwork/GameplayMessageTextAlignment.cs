namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>Where a gameplay notice's text sits within its region, stored in documents by its name.</summary>
public enum GameplayMessageTextAlignment
{
    /// <summary>The text starts at the region's first cell.</summary>
    Left,
    /// <summary>The text is centered, with integer division of the remaining cells.</summary>
    Center,
}
