namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The access layouts of the map, energy and missile stations.</summary>
internal static class StationAccessLayouts
{
    /// <summary>The map station: access blocks one to the right and two to the left, and an acquired animation once the area map is held.</summary>
    internal static StationAccessLayout Map { get; } = new(StationKind.Map, 1, -2, StationAccessBehavior.MapRight, StationAccessBehavior.MapLeft,
        StationAnimationList.MapIdle, StationAnimationList.MapAcquired);

    /// <summary>The energy station: access blocks either side.</summary>
    internal static StationAccessLayout Energy { get; } = new(StationKind.Energy, 1, -1, StationAccessBehavior.EnergyRight, StationAccessBehavior.EnergyLeft,
        StationAnimationList.Energy, AcquiredAnimation: null);

    /// <summary>The missile station: access blocks either side.</summary>
    internal static StationAccessLayout Missile { get; } = new(StationKind.Missile, 1, -1, StationAccessBehavior.MissileRight, StationAccessBehavior.MissileLeft,
        StationAnimationList.Missile, AcquiredAnimation: null);
}
