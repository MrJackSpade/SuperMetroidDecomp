namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>$9B:C122-$C198 origin-field indices, clockwise from up facing right
/// to up facing left, with distinct downward-facing hand poses at indices 4 and 5.</summary>
internal enum GrappleOriginDirection : byte
{
    /// <summary>Straight-up aim while Samus faces right.</summary>
    UpFacingRight = 0,
    /// <summary>Upward diagonal aim toward the right.</summary>
    UpRight = 1,
    /// <summary>Horizontal aim toward the right.</summary>
    Right = 2,
    /// <summary>Downward diagonal aim toward the right.</summary>
    DownRight = 3,
    /// <summary>Straight-down aim while Samus faces right.</summary>
    DownFacingRight = 4,
    /// <summary>Straight-down aim while Samus faces left, with its distinct hand X offset.</summary>
    DownFacingLeft = 5,
    /// <summary>Downward diagonal aim toward the left.</summary>
    DownLeft = 6,
    /// <summary>Horizontal aim toward the left.</summary>
    Left = 7,
    /// <summary>Upward diagonal aim toward the left.</summary>
    UpLeft = 8,
    /// <summary>Straight-up aim while Samus faces left.</summary>
    UpFacingLeft = 9,
}
