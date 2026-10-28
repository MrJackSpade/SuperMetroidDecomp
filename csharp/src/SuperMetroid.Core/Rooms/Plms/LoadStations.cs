using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Rooms.Plms.LoadStationDefinitions;

using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The placement each area's load station selects.</summary>
public static class LoadStations
{
    /// <summary>Native list length of <paramref name="area"/>, including inert placeholders.</summary>
    public static int Count(AreaId area) =>
        Counts.TryGetValue(area, out int count) ? count : throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown retail area ID.");

    /// <summary>Constructs one exact native placement after validating area and station bounds.</summary>
    public static LoadStationEntry Get(AreaId areaIndex, int stationIndex)
    {
        int area = AreaIds.ToIndex(areaIndex);
        int count = Count(areaIndex);
        if ((uint)stationIndex >= count)
            throw new ArgumentOutOfRangeException(nameof(stationIndex), stationIndex,
                $"{areaIndex} has {count} load-station slots.");
        int preceding = 0;
        for (int index = 0; index < area; index++) preceding += Count(AreaIds.At(index));
        LoadStationDefinition entry = areaIndex == AreaId.Ceres
            ? new(RoomHeaderDefinitions.CeresElev, CeresDoorDefinitions.FallingTile0, Position16<RoomPixels>.Zero, new(0, (short)(stationIndex == 0 ? 72 : 64)))
            : Placement(areaIndex, stationIndex);
        return new LoadStationEntry(entry.Room, entry.Door, entry.CameraPosition, entry.SamusOffset);
    }

    /// <summary>Selects a validated non-Ceres station; unused slots receive the native inert record.</summary>
    private static LoadStationDefinition Placement(AreaId area, int station) =>
        Placements.GetValueOrDefault(new LoadStationKey(area, station), Placeholder);
}
