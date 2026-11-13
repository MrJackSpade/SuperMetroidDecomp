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
/// <param name="variant">The land or ocean chunk table the room main selects.</param>
/// <param name="hasBands">Whether the sky's effect or setup made the sky's bands drift.</param>
[RoomLifetime]
public sealed class ScrollingSkyState(ScrollingSkyChunkTable variant, bool hasBands = true)
{
    /// <summary>Whether the sky's bands drift: only the sky's effect or setup starts them; the room-main behaviour alone does not.</summary>
    public bool HasBands { get; } = hasBands;

    /// <summary>The land or ocean chunk table the sky's rows follow.</summary>
    public ScrollingSkyChunkTable Variant { get; } = variant;

    /// <summary>How far each drift's bands have slid, in pixels and 65536ths, from where the room loaded them.</summary>
    private readonly Dictionary<SkyDrift, FixedCoordinate> _phases =
        ScrollingSkyBandDefinitions.Drifts.ToDictionary(drift => drift, _ => default(FixedCoordinate));

    /// <summary>
    /// Returns whether a room state's main behaviour dispatches to the land-sky
    /// implementation. This is the same room-main dispatch decision the game makes;
    /// callers must not infer it from a room number, area, door, or camera position.
    /// </summary>
    public static bool IsLandRoomMain(RoomMainCallback callback) =>
        callback is RoomMainCallback.ScrollingSkyLand or
            RoomMainCallback.ScrollingSkyLandZebesTimebombSet;

    /// <summary>Identifies the scrolling-sky room mains; spawning the bands is a separate decision.</summary>
    public static bool IsScrollingSkyRoomMain(RoomMainCallback callback) =>
        IsLandRoomMain(callback) || callback == RoomMainCallback.ScrollingSkyOcean;

    /// <summary>
    /// Whether a room state's setup spawns the sky's bands, whichever door the room is entered through; a room's
    /// effect can spawn them too (<see cref="SpawnsBands"/>).
    /// </summary>
    public static bool SetupSpawnsBands(RoomSetupCallback setup) =>
        setup is RoomSetupCallback.ScrollingSkyLand or RoomSetupCallback.ScrollingSkyOcean
            or RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape;

    /// <summary>
    /// Whether a room state spawns the sky's bands when entered with <paramref name="selectedFx"/> as its effect: the sky
    /// effect or the state's setup spawns them; the room-main behaviour alone does not.
    /// </summary>
    public static bool SpawnsBands(RoomFxType? selectedFx, RoomSetupCallback setup) =>
        selectedFx == RoomFxType.ScrollingSky || SetupSpawnsBands(setup);

    /// <summary>The chunk table a scrolling-sky room main reads its rows through.</summary>
    public static ScrollingSkyChunkTable VariantOf(RoomMainCallback callback) =>
        IsLandRoomMain(callback) ? ScrollingSkyChunkTable.Land
            : callback == RoomMainCallback.ScrollingSkyOcean ? ScrollingSkyChunkTable.Ocean
            : throw new ArgumentOutOfRangeException(nameof(callback), callback, "The room main runs no scrolling sky.");

    /// <summary>
    /// The background scroll the sky publishes: the sky copies the camera Y, wrapping past row 65535, into Y.
    /// X stays zero because the sky never writes the uniform horizontal scroll; its bands shift each line instead.
    /// </summary>
    public Position16<BackgroundPixels> BackgroundScroll { get; private set; }

    /// <summary>Whether the frame shows the bands drifted: false on a frozen frame, which shows every band unshifted.</summary>
    public bool ShowsDrift { get; private set; } = hasBands;

    /// <summary>How many whole pixels the bands moving with <paramref name="drift"/> have slid sideways.</summary>
    /// <param name="drift">One of the sky's drifts (<see cref="ScrollingSkyBandDefinitions.Drifts"/>).</param>
    /// <exception cref="KeyNotFoundException"><paramref name="drift"/> is not one of the sky's drifts.</exception>
    public int OffsetOf(SkyDrift drift) => _phases[Ensure.NotNull(drift)].Whole;

    /// <summary>Moves each drift by its speed and copies the camera Y into the background vertical scroll; a frozen frame moves nothing.</summary>
    public void ProcessFrame(int cameraY, bool timeIsFrozen)
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
        BackgroundScroll = new Position16<BackgroundPixels>(0, cameraY);
    }

    /// <summary>
    /// The sky the frame shows, its bands placed against the camera at <paramref name="cameraY"/>; null when the sky's bands
    /// do not drift, whose background is the room's own background.
    /// </summary>
    /// <param name="cameraY">The camera's vertical position; a camera above the room's top reads as a negative room row.</param>
    public ScrollingSkyLayer? Publish(int cameraY)
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
        return new ScrollingSkyLayer(CoordinateAxis.AsSigned(cameraY), shown.MoveToImmutable(), ShowsDrift);
    }
}
