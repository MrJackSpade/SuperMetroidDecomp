namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The turning floor Ceres Ridley's getaway flies over (#2228): whether it shows, the frame of its motion the next update
/// shows, how far it has turned and zoomed, the point it turns about and the capsule's and wings' steps on it.
/// </summary>
public sealed class CeresTurningFloor
{
    /// <summary>Gets whether the getaway's turning floor currently owns the Ridley and Baby presentation.</summary>
    public bool Active { get; internal set; }
    /// <summary>Gets whether the getaway's motion has ended, its curve having passed its last frame, and released the floor.</summary>
    public bool Finished { get; internal set; }
    /// <summary>Gets the frame of the getaway's motion the next update shows, advanced by one each update.</summary>
    public int FrameIndex { get; internal set; }
    /// <summary>Gets how far the getaway has turned its floor clockwise, wrapping at a full turn.</summary>
    public RotationAngle Angle { get; internal set; }
    /// <summary>Gets the floor point the getaway places at the screen's top-left before turning and zooming the floor.</summary>
    public Offset16<FloorPixels> Offset { get; internal set; }
    /// <summary>
    /// Gets the getaway's zoom: eight floor pixels per screen pixel when the getaway starts, then each table frame's
    /// authored zoom. It also selects the floor's shade.
    /// </summary>
    public FloorZoom Zoom { get; internal set; }
    /// <summary>Gets the floor point the getaway turns and zooms its floor about, (64, 64) while it runs and cleared on completion.</summary>
    public Position16<FloorPixels> Center { get; internal set; }
    /// <summary>Gets the step of the Baby capsule's cycle the floor shows.</summary>
    public int CapsuleStep { get; internal set; }
    /// <summary>Gets the step of the wings' two-beat cycle the floor shows.</summary>
    public int WingBeat { get; internal set; }
}
