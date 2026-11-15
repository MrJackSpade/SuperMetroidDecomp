using System.Collections.Frozen;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The doors whose passage exchanges explored maps (#1500): the game stores its live explored map as the map of the area
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
        CrateriaDoorDefinitions.ForgottenHighwayElevToMaridiaElev,
        CrateriaDoorDefinitions.EastOceanToElectricDeath,
        CrateriaDoorDefinitions.RedBinstarElevToCaterpillar,
        CrateriaDoorDefinitions.PreBowlingToBowlingAlley,
        CrateriaDoorDefinitions.ElevToBlueBrinstarToMorphBall,
        CrateriaDoorDefinitions.GreenBrinstarElevToGreenBrinstarMainShaft,
        CrateriaDoorDefinitions.StatuesToTourianFirst,
        BrinstarDoorDefinitions.GreenBrinstarMainShaftToGreenBrinstarElev,
        BrinstarDoorDefinitions.MorphBallToElevToBlueBrinstar,
        BrinstarDoorDefinitions.CaterpillarToRedBinstarElev,
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
        MaridiaDoorDefinitions.MaridiaElevToForgottenHighwayElev,
        TourianDoorDefinitions.TourianFirstToStatues,
        TourianDoorDefinitions.TourianEscape4ToClimb,
    }.ToFrozenSet();
}
