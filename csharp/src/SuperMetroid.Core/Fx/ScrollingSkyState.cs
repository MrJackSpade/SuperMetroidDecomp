using System.Collections.Immutable;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky's 23 parallax bands and the background vertical scroll (#1347).
/// </summary>
/// <remarks>
/// <see cref="ProcessFrame"/> advances the bands and the sky's vertical scroll. The sky's rows are
/// drawn from its installed picture (<see cref="SuperMetroid.Core.Rooms.Backgrounds.SkyPictures"/>), so the game's
/// circular row streaming is not modeled; the bands are published with <see cref="Publish"/>.
/// </remarks>
/// <param name="variant">The land or ocean chunk table the room main selects.</param>
/// <param name="hasBands">Whether the sky FX or setup spawned the band per-line effect object.</param>
[RoomLifetime]
public sealed class ScrollingSkyState(ScrollingSkyChunkTable variant, bool hasBands = true)
{
    /// <summary>Only the sky FX/setup spawns the horizontal per-line effect object; the room-main callback alone does not.</summary>
    public bool HasBands { get; } = hasBands;

    /// <summary>The land or ocean chunk table the sky's rows follow.</summary>
    public ScrollingSkyChunkTable Variant { get; } = variant;

    /// <summary>23 16.16 per-line data slots; the last slot is stationary.</summary>
    private readonly FixedCoordinate[] _fixedHorizontalScrolls =
        new FixedCoordinate[ScrollingSkyBandDefinitions.DataSlotCount];

    /// <summary>
    /// Returns whether a room-state callback dispatches to the land-sky
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
    /// Whether a room state's setup callback spawns the sky's bands, whichever door the room is entered through; a room's
    /// effect can spawn them too (<see cref="SpawnsBands"/>).
    /// </summary>
    public static bool SetupSpawnsBands(RoomSetupCallback setup) =>
        setup is RoomSetupCallback.ScrollingSkyLand or RoomSetupCallback.ScrollingSkyOcean
            or RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape;

    /// <summary>
    /// Whether a room state spawns the sky's bands when entered with <paramref name="selectedFx"/> as its effect: the sky
    /// effect or the state's setup callback spawns them; the room-main callback alone does not.
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

    /// <summary>Whether the indirect table has not been terminated by a frozen frame.</summary>
    public bool BandsShown { get; private set; } = hasBands;

    /// <summary>The whole-pixel horizontal offset of band <paramref name="slot"/>, one of the sky's 23 bands.</summary>
    public int GetDataSlotPosition(int slot)
    {
        if ((uint)slot >= _fixedHorizontalScrolls.Length)
            throw new ArgumentOutOfRangeException(nameof(slot));
        return _fixedHorizontalScrolls[slot].Whole;
    }

    /// <summary>Advances the fixed-point sky bands and copies the camera Y into the background vertical scroll.</summary>
    public void ProcessFrame(int cameraY, bool timeIsFrozen)
    {
        if (timeIsFrozen)
        {
            // The sky's setup writes a zero first entry, terminating the table. The
            // per-frame step likewise skips fixed-point advancement.
            BandsShown = false;
            return;
        }

        BandsShown = HasBands;
        if (HasBands) AdvanceHorizontalScrolls();
        BackgroundScroll = new Position16<BackgroundPixels>(0, cameraY);
    }

    /// <summary>
    /// The sky layer the frame shows, positioned from <paramref name="cameraY"/>; null when the room spawned no
    /// band per-line effect object, whose background is the room's own background.
    /// </summary>
    /// <param name="cameraY">The camera Y the bands are positioned from.</param>
    public ScrollingSkyLayer? Publish(int cameraY)
    {
        if (!HasBands)
            return null;
        var offsets = ImmutableArray.CreateBuilder<int>(_fixedHorizontalScrolls.Length);
        for (int slot = 0; slot < _fixedHorizontalScrolls.Length; slot++)
            offsets.Add(GetDataSlotPosition(slot));
        return new ScrollingSkyLayer(cameraY, offsets.MoveToImmutable(), BandsShown);
    }

    /// <summary>Adds each strip's 16.16 speed and resets the stationary bottom strip.</summary>
    private void AdvanceHorizontalScrolls()
    {
        for (int index = 0; index < ScrollingSkyBandDefinitions.SectionCount; index++)
        {
            SkyScrollSection section = ScrollingSkyBands.GetSection(index);
            _fixedHorizontalScrolls[section.DataSlot] = _fixedHorizontalScrolls[section.DataSlot].MovedBy(section.Velocity);
        }

        // The sky clears the final fraction and whole pixels after all 23
        // additions, keeping the bottommost section stationary.
        _fixedHorizontalScrolls[ScrollingSkyBandDefinitions.DataSlotCount - 1] = default;
    }
}
