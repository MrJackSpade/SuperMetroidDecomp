using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Spawns the room object that clears or crumbles the Tourian access floor (<see cref="TourianAccessObjectDefinitions"/>).</summary>
internal static class TourianAccessFloors
{
    /// <summary>Returns the change the room object for the requested floor state makes.</summary>
    public static TerrainChange ForState(TourianAccessFloor floor) =>
        TourianAccessObjectDefinitions.Floors.TryGetValue(floor, out TerrainChange? change)
            ? change : throw new InvalidOperationException($"Undefined {nameof(TourianAccessFloor)} {floor}.");
}
