namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Selected two-pose startup groups EE12..F0C4 in native identity order. Their directional/art selections remain REQUIRED.</summary>
internal enum SpazerStartupShape
{
    /// <summary>Leftward seed pair.</summary>
    Left,
    /// <summary>Up-left seed pair.</summary>
    UpLeft,
    /// <summary>Upward seed pair.</summary>
    Up,
    /// <summary>Up-right seed pair.</summary>
    UpRight,
    /// <summary>Rightward seed pair.</summary>
    Right,
    /// <summary>Down-right seed pair.</summary>
    DownRight,
    /// <summary>Downward seed pair.</summary>
    Down,
    /// <summary>Alternate horizontal seed art.</summary>
    HorizontalAlternate,
    /// <summary>Alternate falling-diagonal seed art.</summary>
    FallingAlternate,
    /// <summary>Alternate vertical seed art.</summary>
    VerticalAlternate,
    /// <summary>Alternate rising-diagonal seed art.</summary>
    RisingAlternate,
}
