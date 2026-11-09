namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// Two-entry horizontal dispatcher used by Tripper and Kamer (the suspensor
/// platform). These values are indexes into the dispatcher's two functions, not the functions themselves.
/// </summary>
public enum PlatformHorizontalMovement
{
    /// <summary>Dispatcher index zero selects negative/left horizontal motion; a wall collision switches to index one and installs right-facing artwork.</summary>
    Left = 0,
    /// <summary>Dispatcher index one selects positive/right horizontal motion; a wall collision switches to index zero and installs left-facing artwork.</summary>
    Right = 1,
}
