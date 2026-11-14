namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The horizontal movement of Tripper and Kamer (the suspensor
/// platform): left or right.
/// </summary>
public enum PlatformHorizontalMovement
{
    /// <summary>Moves left (negative X); a wall collision switches to right and installs right-facing artwork.</summary>
    Left,
    /// <summary>Moves right (positive X); a wall collision switches to left and installs left-facing artwork.</summary>
    Right,
}
