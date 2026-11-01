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
        CrateriaDoorDefinitions.WestOcean1,
        CrateriaDoorDefinitions.WestOcean3,
        CrateriaDoorDefinitions.WestOcean4,
        CrateriaDoorDefinitions.WestOcean5,
        CrateriaDoorDefinitions.ForgottenHighwayElev1,
        CrateriaDoorDefinitions.EastOcean0,
        CrateriaDoorDefinitions.RedBinstarElev1,
        CrateriaDoorDefinitions.PreBowling1,
        CrateriaDoorDefinitions.ElevToBlueBrinstar1,
        CrateriaDoorDefinitions.GreenBrinstarElev1,
        CrateriaDoorDefinitions.Statues1,
        BrinstarDoorDefinitions.GreenBrinstarMainShaft0,
        BrinstarDoorDefinitions.MorphBall2,
        BrinstarDoorDefinitions.Caterpillar3,
        BrinstarDoorDefinitions.Caterpillar4,
        BrinstarDoorDefinitions.BelowSpazer1,
        BrinstarDoorDefinitions.WarehouseEntrance0,
        BrinstarDoorDefinitions.WarehouseEntrance2,
        NorfairDoorDefinitions.BusinessCenter4,
        WreckedShipDoorDefinitions.BowlingAlley0,
        WreckedShipDoorDefinitions.BowlingAlley1,
        WreckedShipDoorDefinitions.WreckedShipEntrance0,
        WreckedShipDoorDefinitions.Attic2,
        WreckedShipDoorDefinitions.ElectricDeath0,
        WreckedShipDoorDefinitions.GravitySuit0,
        MaridiaDoorDefinitions.WestTunnel1,
        MaridiaDoorDefinitions.EastTunnel1,
        MaridiaDoorDefinitions.RedFish1,
        MaridiaDoorDefinitions.MaridiaElev2,
        TourianDoorDefinitions.TourianFirst1,
        TourianDoorDefinitions.TourianEscape41,
    }.ToFrozenSet();
}
