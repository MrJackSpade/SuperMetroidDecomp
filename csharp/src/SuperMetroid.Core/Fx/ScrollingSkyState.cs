using System.Collections.Immutable;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky's drifting bands and the background vertical scroll (#1347, #1687).
/// </summary>
/// <remarks>
/// <see cref="ProcessFrame"/> moves each drift (<see cref="ScrollingSkyBandDefinitions"/>) and the sky's vertical
/// scroll. The sky's rows are drawn from its installed picture
/// (<see cref="SuperMetroid.Core.Rooms.Backgrounds.SkyPictures"/>); the bands are published with <see cref="Publish"/>.
/// </remarks>
/// <param name="sky">The land or ocean sky the room main selects.</param>
/// <param name="hasBands">Whether the sky's effect or setup made the sky's bands drift.</param>
[RoomLifetime]
public sealed partial class ScrollingSkyState(ScrollingSky sky, bool hasBands = true)
{
    /// <summary>Whether the sky's bands drift: only the sky's effect or setup starts them; the room-main behaviour alone does not.</summary>
    public bool HasBands { get; } = hasBands;

    /// <summary>The land or ocean sky the rows show.</summary>
    public ScrollingSky Sky { get; } = sky;

    /// <summary>How far each drift's bands have slid from where the room loaded them, to a fraction of a pixel.</summary>
    private readonly Dictionary<SkyDrift, FixedCoordinate> _phases =
        ScrollingSkyBandDefinitions.Drifts.ToDictionary(drift => drift, _ => default(FixedCoordinate));

    /// <summary>
    /// The room row at the camera when the sky last moved: the background shows the sky from that row down. A camera above
    /// the room's top is at a negative row. The sky never moves its whole background sideways; its bands shift each line instead.
    /// </summary>
    public int CameraRow { get; private set; }

    /// <summary>Whether the frame shows the bands drifted: false on a frozen frame, which shows every band unshifted.</summary>
    internal bool ShowsDrift { get; set; } = hasBands;

    /// <summary>How many whole pixels the bands moving with <paramref name="drift"/> have slid sideways.</summary>
    /// <param name="drift">One of the sky's drifts (<see cref="ScrollingSkyBandDefinitions.Drifts"/>).</param>
    /// <exception cref="KeyNotFoundException"><paramref name="drift"/> is not one of the sky's drifts.</exception>
    internal int OffsetOf(SkyDrift drift) => _phases[Ensure.NotNull(drift)].Whole;

    /// <summary>Moves each drift by its speed and the sky's background to <paramref name="camera"/>'s row; a frozen frame moves nothing.</summary>
    /// <param name="camera">The camera, the room pixel at the screen's top-left.</param>
    /// <param name="timeIsFrozen">Whether time is frozen this frame.</param>
    public void ProcessFrame(Position16<RoomPixels> camera, bool timeIsFrozen)
    {
        if (timeIsFrozen)
        {
            // A frozen frame shows the bands unshifted and leaves every drift where it stands.
            ShowsDrift = false;
            return;
        }

        ShowsDrift = HasBands;
        if (HasBands)
            foreach (SkyDrift drift in ScrollingSkyBandDefinitions.Drifts)
                _phases[drift] = _phases[drift].MovedBy(drift.Speed);
        CameraRow = (camera - Position16<RoomPixels>.Zero).Y;
    }

    /// <summary>
    /// The sky the frame shows, its bands placed against <paramref name="camera"/>; null when the sky's bands do not drift,
    /// whose background is the room's own background.
    /// </summary>
    /// <param name="camera">The camera, the room pixel at the screen's top-left; a camera above the room's top lies at a negative room row.</param>
    public ScrollingSkyLayer? Publish(Position16<RoomPixels> camera)
    {
        if (!HasBands)
            return null;
        ImmutableArray<SkyBand> bands = ScrollingSkyBandDefinitions.Bands;
        var shown = ImmutableArray.CreateBuilder<ShownSkyBand>(bands.Length);
        for (int index = 0; index < bands.Length; index++)
        {
            int endRow = index + 1 < bands.Length ? bands[index + 1].TopRow : ScrollingSkyBandDefinitions.EndRow;
            shown.Add(new ShownSkyBand(bands[index].TopRow, endRow, OffsetOf(bands[index].Drift)));
        }
        return new ScrollingSkyLayer((camera - Position16<RoomPixels>.Zero).Y, shown.MoveToImmutable(), ShowsDrift);
    }
}
