namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The access rules of the map, energy and missile stations.</summary>
internal static partial class StationAccessRuleDefinitions
{
    /// <summary>The map station: access blocks one to the right and two to the left, and an acquired animation once the area map is held.</summary>
    internal static StationAccessRule Map { get; } = new(StationKind.Map, 1, -2,
        StationAnimation.MapIdle, StationAnimation.MapAcquired, GameplayMessageId.MapDataAccessCompleted, StationReward.AreaMap);

    /// <summary>The energy station: access blocks either side, and Samus's energy refilled.</summary>
    internal static StationAccessRule Energy { get; } = new(StationKind.Energy, 1, -1,
        StationAnimation.Energy, AcquiredAnimation: null, GameplayMessageId.EnergyRechargeCompleted, new StationReward.Refills(StationRefill.Energy));

    /// <summary>The missile station: access blocks either side, and Samus's Missiles refilled.</summary>
    internal static StationAccessRule Missile { get; } = new(StationKind.Missile, 1, -1,
        StationAnimation.Missile, AcquiredAnimation: null, GameplayMessageId.MissileRechargeCompleted, new StationReward.Refills(StationRefill.Missiles));

    /// <summary>Each station's access rule.</summary>
    internal static IReadOnlyDictionary<StationKind, StationAccessRule> Rules { get; } = new Dictionary<StationKind, StationAccessRule>
    {
        [StationKind.Map] = Map,
        [StationKind.Energy] = Energy,
        [StationKind.Missile] = Missile,
    };
}
