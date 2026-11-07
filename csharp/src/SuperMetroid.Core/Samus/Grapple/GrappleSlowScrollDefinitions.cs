namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>
/// Constants of the camera's fast-swing branch, taken while the grapple's
/// slow-scrolling flag is set.
/// </summary>
internal static class GrappleSlowScrollDefinitions
{
    /// <summary>Pixels the camera moves per frame toward the dead zone.</summary>
    public const int Step = 3;

    /// <summary>Samus screen X from which the camera scrolls right.</summary>
    public const int RightEdgeX = 160;

    /// <summary>Samus screen X below which the camera scrolls left.</summary>
    public const int LeftEdgeX = 96;

    /// <summary>Samus screen Y from which the camera scrolls down.</summary>
    public const int BottomEdgeY = 144;

    /// <summary>Samus screen Y below which the camera scrolls up.</summary>
    public const int TopEdgeY = 112;

    /// <summary>
    /// Swing angular speed from which the swing sets the flag; slower
    /// swings clear it.
    /// </summary>
    public const int FastSwingAngularSpeed = 64;
}
