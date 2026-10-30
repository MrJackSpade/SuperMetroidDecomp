using static SuperMetroid.Core.Rooms.Plms.StationAccessPlmDefinitions;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Which station an access PLM resolves to.</summary>
internal static class StationAccessPlms
{
    /// <summary>Resolves the access actor selected by one station-special-block behaviour.</summary>
    public static StationAccessPlmDefinition Resolve(StationAccessBehavior behavior) => behavior switch
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
