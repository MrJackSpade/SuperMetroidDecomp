namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// Sbug's eight directional animation loops (#641).
/// Their steps are <see cref="SbugAnimations"/>.
/// </summary>
internal enum SbugSegment
{
    /// <summary>Facing right, its wings beating up, level, up and down, repeating.</summary>
    Right,
    /// <summary>Facing up and right, its wings beating up, level, up and down, repeating.</summary>
    UpRight,
    /// <summary>Facing up, its wings beating up, level, up and down, repeating.</summary>
    Up,
    /// <summary>Facing up and left, its wings beating up, level, up and down, repeating.</summary>
    UpLeft,
    /// <summary>Facing left, its wings beating up, level, up and down, repeating.</summary>
    Left,
    /// <summary>Facing down and left, its wings beating up, level, up and down, repeating.</summary>
    DownLeft,
    /// <summary>Facing down, its wings beating up, level, up and down, repeating.</summary>
    Down,
    /// <summary>Facing down and right, its wings beating up, level, up and down, repeating.</summary>
    DownRight,
}
