using static SuperMetroid.Core.Rooms.RoomObjects.StationAccessObjectDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Which station an access PLM resolves to.</summary>
internal static class StationAccessObjects
{
    /// <summary>Resolves the access actor selected by one station-special-block behaviour.</summary>
    public static StationAccessObjectDefinition Resolve(StationAccessBehavior behavior) => behavior switch
    {
        StationAccessBehavior.MapRight => MapRight,
        StationAccessBehavior.MapLeft => MapLeft,
        StationAccessBehavior.EnergyRight => EnergyRight,
        StationAccessBehavior.EnergyLeft => EnergyLeft,
        StationAccessBehavior.MissileRight => MissileRight,
        StationAccessBehavior.MissileLeft => MissileLeft,
        StationAccessBehavior.SaveFloor => throw new InvalidDataException($"Station access has invalid behavior {behavior}."),
        _ => throw new InvalidDataException($"Station access has invalid behavior {behavior}.")
    };
}
