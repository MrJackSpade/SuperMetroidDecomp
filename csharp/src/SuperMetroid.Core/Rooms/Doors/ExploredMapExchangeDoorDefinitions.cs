using System.Collections.Frozen;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The doors whose passage exchanges explored maps (#1500): the live explored map becomes the map of the area
/// Samus leaves, then loads the live map from the stored map of the area she enters. They are the doors between areas,
/// elevators among them, except the escape's door from the Climb down into Tourian's fourth escape room, which leaves the
/// live map as it is.
/// </summary>
internal static class ExploredMapExchangeDoorDefinitions
{
    /// <summary>Every door that exchanges explored maps.</summary>
    internal static FrozenSet<DoorDefinition> Doors { get; } = new[]
    {
        CrateriaDoorDefinitions.WestOceanToWreckedShipEntrance,
        CrateriaDoorDefinitions.WestOceanToAttic,
        CrateriaDoorDefinitions.WestOceanToBowlingAlley,
        CrateriaDoorDefinitions.WestOceanToGravitySuit,
        CrateriaDoorDefinitions.ForgottenHighwayElevatorToMaridiaElevator,
        CrateriaDoorDefinitions.EastOceanToElectricDeath,
        CrateriaDoorDefinitions.RedBrinstarElevatorToCaterpillar,
        CrateriaDoorDefinitions.PreBowlingToBowlingAlley,
        CrateriaDoorDefinitions.ElevatorToBlueBrinstarToMorphBall,
        CrateriaDoorDefinitions.GreenBrinstarElevatorToGreenBrinstarMainShaft,
        CrateriaDoorDefinitions.StatuesToTourianFirst,
        BrinstarDoorDefinitions.GreenBrinstarMainShaftToGreenBrinstarElevator,
        BrinstarDoorDefinitions.MorphBallToElevatorToBlueBrinstar,
        BrinstarDoorDefinitions.CaterpillarToRedBrinstarElevator,
        BrinstarDoorDefinitions.CaterpillarToRedFish,
        BrinstarDoorDefinitions.BelowSpazerToWestTunnel,
        BrinstarDoorDefinitions.WarehouseEntranceToEastTunnel,
        BrinstarDoorDefinitions.WarehouseEntranceToBusinessCenter,
        NorfairDoorDefinitions.BusinessCenterToWarehouseEntrance,
        WreckedShipDoorDefinitions.BowlingAlleyToWestOcean,
        WreckedShipDoorDefinitions.BowlingAlleyToPreBowling,
        WreckedShipDoorDefinitions.WreckedShipEntranceToWestOcean,
        WreckedShipDoorDefinitions.AtticToWestOcean,
        WreckedShipDoorDefinitions.ElectricDeathToEastOcean,
        WreckedShipDoorDefinitions.GravitySuitToWestOcean,
        MaridiaDoorDefinitions.WestTunnelToBelowSpazer,
        MaridiaDoorDefinitions.EastTunnelToWarehouseEntrance,
        MaridiaDoorDefinitions.RedFishToCaterpillar,
        MaridiaDoorDefinitions.MaridiaElevatorToForgottenHighwayElevator,
        TourianDoorDefinitions.TourianFirstToStatues,
        TourianDoorDefinitions.TourianEscape4ToClimb,
    }.ToFrozenSet();
}
