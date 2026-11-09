namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The station each station room object a room places is.</summary>
internal static class StationKindDefinitions
{
    /// <summary>Each station room object's kind.</summary>
    internal static readonly IReadOnlyDictionary<RoomObjectHeaderId, StationKind> Kinds = new Dictionary<RoomObjectHeaderId, StationKind>
    {
        [RoomObjectHeaderId.MapStation] = StationKind.Map,
        [RoomObjectHeaderId.EnergyStation] = StationKind.Energy,
        [RoomObjectHeaderId.MissileStation] = StationKind.Missile,
        [RoomObjectHeaderId.SaveStation] = StationKind.Save,
    };
}
