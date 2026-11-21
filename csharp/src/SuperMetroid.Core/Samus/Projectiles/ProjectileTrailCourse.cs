namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The travel a trail family keeps one placement list for (#2565): every direction, an axis of travel, or one
/// direction, as the family's lists divide the shot's directions.
/// </summary>
internal enum ProjectileTrailCourse
{
    /// <summary>Every direction the shot travels in.</summary>
    AnyDirection,

    /// <summary>Upward and downward travel.</summary>
    Vertical,

    /// <summary>Leftward and rightward travel.</summary>
    Horizontal,

    /// <summary>Up-right and down-left travel.</summary>
    RisingDiagonal,

    /// <summary>Down-right and up-left travel.</summary>
    FallingDiagonal,

    /// <summary>Upward travel.</summary>
    Up,

    /// <summary>Up-right travel.</summary>
    UpRight,

    /// <summary>Rightward travel.</summary>
    Right,

    /// <summary>Down-right travel.</summary>
    DownRight,

    /// <summary>Downward travel.</summary>
    Down,

    /// <summary>Down-left travel.</summary>
    DownLeft,

    /// <summary>Leftward travel.</summary>
    Left,

    /// <summary>Up-left travel.</summary>
    UpLeft,
}
