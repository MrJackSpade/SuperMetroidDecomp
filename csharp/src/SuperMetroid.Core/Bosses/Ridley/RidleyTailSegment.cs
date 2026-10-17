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
    /// <summary>
    /// Gets the accumulated angular stagger, in tail-angle units, before this segment activates the next segment. It counts
    /// up from zero (seventeen at initialization) by the tail's angular step until it reaches the ideal inter-segment angle,
    /// then holds <see cref="RidleyTailDefinitions.StaggerPropagated"/> once the next segment has been activated.
    /// </summary>
    public int StaggerAngle { get; internal set; }
    /// <summary>Gets the native direction word; bit $8000 selects clockwise/decreasing-angle motion.</summary>
    public ushort MovementDirection { get; internal set; }
    /// <summary>
    /// Gets the live 8.8 distance (1/256 pixels) from the preceding tail anchor, between half a pixel under its rest length
    /// and its whip maximum; its whole part supplies the pixels used for offset composition.
    /// </summary>
    public int Distance { get; internal set; }

    /// <summary>
    /// Gets the 8.8 distance this segment is extending toward for a whip, stab or bounce, or zero while it is not extending.
    /// It is stored separately from the live distance and cleared once the live distance passes it.
    /// </summary>
    public int TargetDistance { get; internal set; }

    /// <summary>Gets the wrapped native tail angle; the low byte indexes the sine table and the full word retains winding and clamp state.</summary>
    public ushort Angle { get; internal set; }
    /// <summary>Gets the signed whole-pixel displacement from the preceding anchor.</summary>
    public Offset16<RoomPixels> Offset { get; internal set; }
    /// <summary>Gets the composed sixteen-bit room position used for drawing and tail contact.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
}
