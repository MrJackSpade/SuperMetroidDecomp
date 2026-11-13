namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The station each station room object a room places is.</summary>
internal static class StationKindDefinitions
{
    /// <summary>Each station room object's kind.</summary>
    internal static readonly IReadOnlyDictionary<RoomObjectKind, StationKind> Kinds = new Dictionary<RoomObjectKind, StationKind>
    {
        [RoomObjectKind.MapStation] = StationKind.Map,
        [RoomObjectKind.EnergyStation] = StationKind.Energy,
        [RoomObjectKind.MissileStation] = StationKind.Missile,
        [RoomObjectKind.SaveStation] = StationKind.Save,
    };
}
