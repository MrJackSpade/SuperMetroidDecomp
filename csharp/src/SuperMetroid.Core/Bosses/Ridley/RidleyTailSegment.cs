namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of the seven twenty-byte logical tail entries initialized by $A6:D2D6. The tail
/// is shared by Ceres Ridley and Lower Norfair Ridley; encounter-specific controllers only
/// change these common targets and activation words.
/// </summary>
public sealed class RidleyTailSegment
{
    /// <summary>Gets whether this tail segment is participating in the current wave or whip; deactivation resets its stagger and reverses its movement direction.</summary>
    public bool Active { get; internal set; }
    /// <summary>Gets the accumulated angular stagger before this segment activates the next segment; $FFFF marks propagation already performed.</summary>
    public ushort StaggerAngle { get; internal set; }
    /// <summary>Gets the native direction word; bit $8000 selects clockwise/decreasing-angle motion.</summary>
    public ushort MovementDirection { get; internal set; }
    /// <summary>Gets the live unsigned 8.8 distance from the preceding tail anchor; its high byte supplies whole pixels for offset composition.</summary>
    public ushort Distance { get; internal set; }

    /// <summary>
    /// Nonzero while this segment is extending for a whip. Native stores this separately
    /// from the live radius and clears it after the live distance passes the request.
    /// </summary>
    public ushort TargetDistance { get; internal set; }

    /// <summary>Gets the wrapped native tail angle; the low byte indexes the sine table and the full word retains winding and clamp state.</summary>
    public ushort Angle { get; internal set; }
    /// <summary>Gets the signed whole-pixel displacement from the preceding anchor.</summary>
    public Offset16<RoomPixels> Offset { get; internal set; }
    /// <summary>Gets the composed sixteen-bit room position used for drawing and tail contact.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
}
