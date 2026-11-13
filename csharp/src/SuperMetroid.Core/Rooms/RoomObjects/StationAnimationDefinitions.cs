namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The stations' animations (#1554): each animation's images in order, with the updates each shows. The map and resource
/// cycles show three images six updates each, two once the map is held; the save station's idle image shows once, and
/// each of its two active images four updates.
/// </summary>
internal static class StationAnimationDefinitions
{
    /// <summary>Pairs a station frame's hold duration with the draw shown while it is held.</summary>
    /// <param name="Duration">Number of station animation updates before advancing to the next frame.</param>
    /// <param name="Draw">The blocks displayed for this frame.</param>
    internal readonly record struct Frame(int Duration, RoomObjectDraw Draw);

    /// <summary>Each animation's images, in the order they show.</summary>
    internal static IReadOnlyDictionary<StationAnimationList, IReadOnlyList<Frame>> Frames { get; } =
        new Dictionary<StationAnimationList, IReadOnlyList<Frame>>
    {
        [StationAnimationList.MapIdle] =
        [
            new(6, RoomObjectDraw.Of(StationDraw.MapFrame0)),
            new(6, RoomObjectDraw.Of(StationDraw.MapFrame1)),
            new(6, RoomObjectDraw.Of(StationDraw.MapFrame2)),
        ],
        [StationAnimationList.MapAcquired] =
        [
            new(2, RoomObjectDraw.Of(StationDraw.MapFrame0)),
            new(2, RoomObjectDraw.Of(StationDraw.MapFrame1)),
            new(2, RoomObjectDraw.Of(StationDraw.MapFrame2)),
        ],
        [StationAnimationList.Energy] =
        [
            new(6, RoomObjectDraw.Of(StationDraw.EnergyFrame0)),
            new(6, RoomObjectDraw.Of(StationDraw.EnergyFrame1)),
            new(6, RoomObjectDraw.Of(StationDraw.EnergyFrame2)),
        ],
        [StationAnimationList.Missile] =
        [
            new(6, RoomObjectDraw.Of(StationDraw.MissileFrame0)),
            new(6, RoomObjectDraw.Of(StationDraw.MissileFrame1)),
            new(6, RoomObjectDraw.Of(StationDraw.MissileFrame2)),
        ],
        [StationAnimationList.SaveIdle] = [new(1, RoomObjectDraw.Of(StationDraw.SaveIdle))],
        [StationAnimationList.SaveFirstFrame] = [new(4, RoomObjectDraw.Of(StationDraw.SaveActive))],
        [StationAnimationList.SaveSecondFrame] = [new(4, RoomObjectDraw.Of(StationDraw.SaveAlternate))],
    };
}
