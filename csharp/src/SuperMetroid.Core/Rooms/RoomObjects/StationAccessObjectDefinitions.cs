namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What the access arm draws at each of the six station accesses, one per station and side (#2484). Energy and missile
/// stations share access artwork on each side. The save station's floor moves no arm and has no access draws.
/// </summary>
internal static partial class StationAccessObjectDefinitions
{
    /// <summary>The arm's retracted and extended draws at each station access.</summary>
    internal static IReadOnlyDictionary<StationAccess, StationAccessObjectDefinition> Arms { get; } =
        new Dictionary<StationAccess, StationAccessObjectDefinition>
        {
            [new(StationKind.Map, HorizontalDirection.Right)] = new(StationDraw.MapRightRetracted, StationDraw.MapRightExtended),
            [new(StationKind.Map, HorizontalDirection.Left)] = new(StationDraw.MapLeftRetracted, StationDraw.MapLeftExtended),
            [new(StationKind.Energy, HorizontalDirection.Right)] = new(StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended),
            [new(StationKind.Energy, HorizontalDirection.Left)] = new(StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended),
            [new(StationKind.Missile, HorizontalDirection.Right)] = new(StationDraw.ResourceRightRetracted, StationDraw.ResourceRightExtended),
            [new(StationKind.Missile, HorizontalDirection.Left)] = new(StationDraw.ResourceLeftRetracted, StationDraw.ResourceLeftExtended),
        };
}
