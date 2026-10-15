namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// The eight directions Botwoon's head and body segments face, clockwise from up: the order of
/// the cartridge's 32-unit angle octants, so octant <c>n</c> faces <c>(BotwoonFacing)n</c>.
/// </summary>
internal enum BotwoonFacing
{
    /// <summary>Octant 0.</summary>
    Up,
    /// <summary>Octant 1.</summary>
    UpRight,
    /// <summary>Octant 2.</summary>
    Right,
    /// <summary>Octant 3.</summary>
    DownRight,
    /// <summary>Octant 4.</summary>
    Down,
    /// <summary>Octant 5.</summary>
    DownLeft,
    /// <summary>Octant 6.</summary>
    Left,
    /// <summary>Octant 7.</summary>
    UpLeft,
}
