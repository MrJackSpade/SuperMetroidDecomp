namespace SuperMetroid.Core.Game;

/// <summary>
/// Constants of the fast-swing camera branch of <c>Main_Scrolling_Routine</c>
/// ($90:94F2-$9555), taken while <c>GrappleBeam_SlowScrollingFlag</c> is set.
/// </summary>
internal static class GrappleSlowScrollDefinitions
{
    /// <summary>Pixels layer 1 moves per frame toward the dead zone ($90:950B/951C/9536/9547).</summary>
    public const ushort Step = 3;

    /// <summary>Samus screen X from which the camera scrolls right ($90:9502).</summary>
    public const ushort RightEdgeX = 0x00A0;

    /// <summary>Samus screen X below which the camera scrolls left ($90:9513).</summary>
    public const ushort LeftEdgeX = 0x0060;

    /// <summary>Samus screen Y from which the camera scrolls down ($90:952D).</summary>
    public const ushort BottomEdgeY = 0x0090;

    /// <summary>Samus screen Y below which the camera scrolls up ($90:953E).</summary>
    public const ushort TopEdgeY = 0x0070;

    /// <summary>
    /// Swing angular speed from which <c>$9B:BD95</c> sets the flag ($9B:BDA4); slower
    /// swings clear it.
    /// </summary>
    public const ushort FastSwingAngularSpeed = 0x0040;
}
