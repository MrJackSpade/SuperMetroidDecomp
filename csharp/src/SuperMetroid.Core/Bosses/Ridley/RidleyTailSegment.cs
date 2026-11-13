namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// One of the seven logical tail entries initialized by Ridley's tail initialization. The tail
/// is shared by Ceres Ridley and Lower Norfair Ridley; encounter-specific controllers only
/// change these common targets and activation flags.
/// </summary>
public sealed class RidleyTailSegment
{
    /// <summary>Gets whether this tail segment is participating in the current wave or whip; deactivation resets its stagger and reverses its movement direction.</summary>
    public bool Active { get; internal set; }
    /// <summary>
    /// Gets the accumulated angular stagger, in tail-angle units, before this segment activates the next segment. It counts
    /// up from zero (seventeen at initialization) by the tail's angular step until it reaches the ideal inter-segment angle,
    /// then has none (null) once the next segment has been activated: no stagger compares below the ideal angle, as the
    /// minus-one value does not (#1413).
    /// </summary>
    public int? StaggerAngle { get; internal set; }
    /// <summary>Gets whether this segment moves clockwise, toward decreasing angles; otherwise it moves counterclockwise.</summary>
    public bool Clockwise { get; internal set; }
    /// <summary>
    /// Gets the live distance from the preceding tail anchor, between half a pixel under its rest length and its whip
    /// maximum; its whole pixels supply the offset composition.
    /// </summary>
    public SignedEightEight Distance { get; internal set; }

    /// <summary>
    /// Gets the distance this segment is extending toward for a whip, stab or bounce, or null while it is not extending.
    /// It is stored separately from the live distance and cleared once the live distance passes it.
    /// </summary>
    public SignedEightEight? TargetDistance { get; internal set; }

    /// <summary>
    /// Gets the link's angle, counted from straight down without wrapping: its direction within the turn aims the link,
    /// and the whole count orders angles across the turn boundary for the clamp and whip-target comparisons.
    /// </summary>
    public RidleyTailAngle Angle { get; internal set; }
    /// <summary>Gets the signed whole-pixel displacement from the preceding anchor.</summary>
    public Offset16<RoomPixels> Offset { get; internal set; }
    /// <summary>Gets the composed room position used for drawing and tail contact.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
}
