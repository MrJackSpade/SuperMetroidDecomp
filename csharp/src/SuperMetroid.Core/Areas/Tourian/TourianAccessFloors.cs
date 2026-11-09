namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Spawns the room object that clears or crumbles the Tourian access floor (<see cref="TourianAccessObjectDefinitions"/>).</summary>
internal static class TourianAccessFloors
{
    /// <summary>Returns the complete spawn identity for the requested floor state.</summary>
    public static TourianAccessObjectDefinition ForState(TourianAccessFloor floor) =>
        TourianAccessObjectDefinitions.Floors.TryGetValue(floor, out TourianAccessObjectDefinition definition)
            ? definition : throw new InvalidOperationException($"Undefined {nameof(TourianAccessFloor)} {floor}.");
}
