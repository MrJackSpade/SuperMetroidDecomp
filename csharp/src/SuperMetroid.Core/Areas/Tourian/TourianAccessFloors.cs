namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>Spawns the PLM that clears or crumbles the Tourian access floor (<see cref="TourianAccessPlmDefinitions"/>).</summary>
internal static class TourianAccessFloors
{
    /// <summary>Returns the complete native spawn identity for the requested floor state.</summary>
    public static TourianAccessPlmDefinition ForState(TourianAccessFloor floor) =>
        TourianAccessPlmDefinitions.Floors.TryGetValue(floor, out TourianAccessPlmDefinition definition)
            ? definition : throw new InvalidOperationException($"Undefined {nameof(TourianAccessFloor)} {floor}.");
}
