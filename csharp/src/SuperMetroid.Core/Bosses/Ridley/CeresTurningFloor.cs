using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The turning floor Ceres Ridley's getaway flies over (#2228): whether it shows, the update of the getaway the next update
/// flies, how far it has turned and zoomed, the point it turns about and the capsule's and wings' pictures on it.
/// </summary>
public sealed class CeresTurningFloor
{
    /// <summary>Gets whether the getaway's turning floor currently owns the Ridley and Baby presentation.</summary>
    public bool Active { get; internal set; }
    /// <summary>Gets the update of the getaway the next update flies, from zero, advanced by one each update.</summary>
    public int GetawayUpdate { get; internal set; }
    /// <summary>Gets how far the getaway has turned its floor clockwise, wrapping at a full turn.</summary>
    public RotationAngle Angle { get; internal set; }
    /// <summary>Gets the floor point the getaway places at the screen's top-left before turning and zooming the floor.</summary>
    public Offset16<FloorPixels> Offset { get; internal set; }
    /// <summary>
    /// Gets the getaway's zoom: eight floor pixels per screen pixel when the getaway starts, then its zoom curve's zoom
    /// for each update. It also selects the floor's shade.
    /// </summary>
    public FloorZoom Zoom { get; internal set; }
    /// <summary>
    /// Gets the floor point the getaway turns and zooms its floor about, from the floor's top-left (#2471): (64, 64) while it
    /// runs and cleared on completion.
    /// </summary>
    public Offset16<FloorPixels> Center { get; internal set; }
    /// <summary>Gets the picture of the Baby capsule's cycle the floor shows (#2329).</summary>
    public ListStep<CeresFloorActorPicture> Capsule { get; internal set; } = FirstCapsulePicture;
    /// <summary>Gets the picture of the wings' two-beat cycle the floor shows (#2329).</summary>
    public ListStep<CeresFloorActorPicture> Wings { get; internal set; } = FirstWingPicture;

    /// <summary>The capsule's picture as a getaway starts: the first of its cycle.</summary>
    internal static ListStep<CeresFloorActorPicture> FirstCapsulePicture => ListStep<CeresFloorActorPicture>.FirstOf(CeresFloorAnimationDefinitions.CapsuleCycle);

    /// <summary>The wings' picture as a getaway starts: the first of their cycle.</summary>
    internal static ListStep<CeresFloorActorPicture> FirstWingPicture => ListStep<CeresFloorActorPicture>.FirstOf(CeresFloorAnimationDefinitions.WingCycle);
}
