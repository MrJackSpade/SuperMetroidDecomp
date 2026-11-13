using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The straight-line door callbacks that only rewrite room scroll cells (#641). Each callback is
/// its ordered list of writes; a scroll index is the row-major cell number in the room's scroll grid.
/// Each setup is named for the room its doors lead into and the screens of that room its writes change.
/// </summary>
internal static class DoorScrollChangeDefinitions
{
    /// <summary>The ordered scroll writes of each setup that only rewrites scroll cells.</summary>
    internal static IReadOnlyDictionary<DoorSetupCode, IReadOnlyList<DoorScrollWrite>> Writes { get; } =
        new Dictionary<DoorSetupCode, IReadOnlyList<DoorScrollWrite>>
        {
            [DoorSetupCode.ParlorOpensScreen1x1] = [new(6, RoomScrollState.Green)],
            [DoorSetupCode.ParlorOpensScreen0x0TopAligned] = [new(0, RoomScrollState.Blue)],
            [DoorSetupCode.LandingSiteOpensScreen1x2TopAligned] = [new(19, RoomScrollState.Blue)],
            [DoorSetupCode.ParlorClosesScreen4x0OpensScreen3x1] = [new(4, RoomScrollState.RedBoundary), new(8, RoomScrollState.Green)],
            [DoorSetupCode.BowlingAlleyClosesScreens2x1And3x1And4x1And5x1] = [new(8, RoomScrollState.RedBoundary), new(9, RoomScrollState.RedBoundary), new(10, RoomScrollState.RedBoundary), new(11, RoomScrollState.RedBoundary)],
            [DoorSetupCode.BowlingAlleyClosesScreens2x0And3x0And4x0And5x0And5x1And0x2And1x2And5x2] = [new(2, RoomScrollState.RedBoundary), new(3, RoomScrollState.RedBoundary), new(4, RoomScrollState.RedBoundary), new(5, RoomScrollState.RedBoundary), new(11, RoomScrollState.RedBoundary), new(12, RoomScrollState.RedBoundary), new(13, RoomScrollState.RedBoundary), new(17, RoomScrollState.RedBoundary)],
            [DoorSetupCode.CrateriaKihunterOpensScreens1x0And1x1] = [new(1, RoomScrollState.Green), new(4, RoomScrollState.Green)],
            [DoorSetupCode.ClimbOpensScreen2x0TopAligned] = [new(2, RoomScrollState.Blue)],
            [DoorSetupCode.ClimbOpensScreen2x7TopAligned] = [new(23, RoomScrollState.Blue)],
            [DoorSetupCode.GauntletETankOpensScreen4x0TopAligned] = [new(4, RoomScrollState.Blue)],
            [DoorSetupCode.CaterpillarFromRedBinstarElevOpensScreen0x2] = [new(6, RoomScrollState.Green)],
            [DoorSetupCode.GreenPiratesShaftOpensScreen0x3] = [new(3, RoomScrollState.Green)],
            [DoorSetupCode.GreenBrinstarMainShaftOpensScreens0x6And0x7] = [new(24, RoomScrollState.Green), new(28, RoomScrollState.Green)],
            [DoorSetupCode.EtecoonETankOpensScreens0x1And1x1TopAligned] = [new(5, RoomScrollState.Blue), new(6, RoomScrollState.Blue)],
            [DoorSetupCode.GreenBrinstarMainShaftOpensScreen1x7TopAligned] = [new(29, RoomScrollState.Blue)],
            [DoorSetupCode.PinkBrinstarPowerBombsOpensScreens0x1And1x1] = [new(2, RoomScrollState.Green), new(3, RoomScrollState.Green)],
            [DoorSetupCode.WarehouseEntranceClosesScreen0x0OpensScreen1x0] = [new(0, RoomScrollState.RedBoundary), new(1, RoomScrollState.Green)],
            [DoorSetupCode.DachoraOpensScreen4x1] = [new(11, RoomScrollState.Green)],
            [DoorSetupCode.BigPinkClosesScreen3x5OpensScreen4x5TopAligned] = [new(28, RoomScrollState.RedBoundary), new(29, RoomScrollState.Blue)],
            [DoorSetupCode.DachoraClosesScreen4x0] = [new(4, RoomScrollState.RedBoundary)],
            [DoorSetupCode.BigPinkOpensScreens2x6And1x7And2x7] = [new(32, RoomScrollState.Green), new(36, RoomScrollState.Green), new(37, RoomScrollState.Green)],
            [DoorSetupCode.BlueBrinstarETankOpensScreen2x0TopAligned] = [new(2, RoomScrollState.Blue)],
            [DoorSetupCode.KraidEyeDoorOpensScreen0x0] = [new(0, RoomScrollState.Green)],
            [DoorSetupCode.RedTowerOpensScreens0x6And0x7] = [new(6, RoomScrollState.Green), new(7, RoomScrollState.Green)],
            [DoorSetupCode.RedFishClosesScreen2x0OpensScreen1x0TopAligned] = [new(1, RoomScrollState.Blue), new(2, RoomScrollState.RedBoundary)],
            [DoorSetupCode.SporeSpawnSuperClosesScreen1x1OpensScreen1x0TopAligned] = [new(1, RoomScrollState.Blue), new(3, RoomScrollState.RedBoundary)],
            [DoorSetupCode.EastTunnelFromWarehouseEntranceClosesScreen0x0OpensScreen0x1TopAligned] = [new(0, RoomScrollState.RedBoundary), new(4, RoomScrollState.Blue)],
            [DoorSetupCode.WarehouseKihunterOpensScreens2x0And3x0TopAligned] = [new(2, RoomScrollState.Blue), new(3, RoomScrollState.Blue)],
            [DoorSetupCode.BelowSpazerOpensScreens0x0And1x0] = [new(0, RoomScrollState.Green), new(1, RoomScrollState.Green)],
            [DoorSetupCode.BigPinkOpensScreen1x0] = [new(1, RoomScrollState.Green)],
            [DoorSetupCode.CaterpillarOpensScreens0x5And0x6] = [new(15, RoomScrollState.Green), new(18, RoomScrollState.Green)],
            [DoorSetupCode.CaterpillarFromHellwayOpensScreen0x2] = [new(6, RoomScrollState.Green)],
            [DoorSetupCode.ConstructionZoneOpensScreen0x0OpensScreen0x1TopAligned] = [new(0, RoomScrollState.Green), new(1, RoomScrollState.Blue)],
            [DoorSetupCode.GreenBrinstarElevOpensScreen0x2BelowTheRoom] = [new(2, RoomScrollState.Green)],
            [DoorSetupCode.AcidStatueClosesScreens0x1And1x1OpensScreens0x2And1x2And2x2TopAligned] = [new(3, RoomScrollState.RedBoundary), new(4, RoomScrollState.RedBoundary), new(6, RoomScrollState.Blue), new(7, RoomScrollState.Blue), new(8, RoomScrollState.Blue)],
            [DoorSetupCode.LNSpringBallMazeClosesScreen1x1OpensScreen4x0OpensScreens1x0And2x0And3x0TopAligned] = [new(1, RoomScrollState.Blue), new(2, RoomScrollState.Blue), new(3, RoomScrollState.Blue), new(4, RoomScrollState.Green), new(6, RoomScrollState.RedBoundary)],
            [DoorSetupCode.CrocomireOpensScreens0x0And1x0TopAligned] = [new(0, RoomScrollState.Blue), new(1, RoomScrollState.Blue)],
            [DoorSetupCode.BatCaveClosesScreen0x1OpensScreen0x0TopAligned] = [new(0, RoomScrollState.Blue), new(1, RoomScrollState.RedBoundary)],
            [DoorSetupCode.IceBeamGateOpensScreen3x1] = [new(10, RoomScrollState.Green)],
            [DoorSetupCode.HiJumpETankOpensScreens0x0And0x1] = [new(0, RoomScrollState.Green), new(2, RoomScrollState.Green)],
            [DoorSetupCode.VolcanoClosesScreen2x2OpensScreens0x2And1x2TopAligned] = [new(6, RoomScrollState.Blue), new(7, RoomScrollState.Blue), new(8, RoomScrollState.RedBoundary)],
            [DoorSetupCode.IceBeamSnakeClosesScreen0x1OpensScreen1x1TopAligned] = [new(2, RoomScrollState.RedBoundary), new(3, RoomScrollState.Blue)],
            [DoorSetupCode.WastelandOpensScreen1x1] = [new(7, RoomScrollState.Green)],
            [DoorSetupCode.FastPillarsSetupClosesScreen0x1OpensScreen0x2TopAligned] = [new(1, RoomScrollState.RedBoundary), new(2, RoomScrollState.Blue)],
            [DoorSetupCode.RedKihunterShaftFromAmphitheatreClosesScreen0x1OpensScreen0x0TopAligned] = [new(0, RoomScrollState.Blue), new(3, RoomScrollState.RedBoundary)],
            [DoorSetupCode.LNFirefleaClosesScreen1x1OpensScreen1x0TopAligned] = [new(1, RoomScrollState.Blue), new(4, RoomScrollState.RedBoundary)],
            [DoorSetupCode.GoldenTorizoClosesScreens1x0And0x1And1x1OpensScreen0x0TopAligned] = [new(0, RoomScrollState.Blue), new(1, RoomScrollState.RedBoundary), new(2, RoomScrollState.RedBoundary), new(3, RoomScrollState.RedBoundary)],
            [DoorSetupCode.WorstRoomInTheGameOpensScreen0x0] = [new(0, RoomScrollState.Green)],
            [DoorSetupCode.LNFirefleaClosesScreen1x1OpensScreens0x0And1x0TopAligned] = [new(0, RoomScrollState.Blue), new(1, RoomScrollState.Blue), new(4, RoomScrollState.RedBoundary)],
            [DoorSetupCode.RedKihunterShaftFromLNFirefleaClosesScreen0x1OpensScreen0x0TopAligned] = [new(0, RoomScrollState.Blue), new(3, RoomScrollState.RedBoundary)],
            [DoorSetupCode.GreenBubblesMissilesOpensScreen0x0TopAligned] = [new(0, RoomScrollState.Blue)],
            [DoorSetupCode.PurpleShaftClosesScreen0x1OpensScreen0x0TopAligned] = [new(0, RoomScrollState.Blue), new(1, RoomScrollState.RedBoundary)],
            [DoorSetupCode.IceBeamGateOpensScreen3x3TopAligned] = [new(24, RoomScrollState.Blue)],
            [DoorSetupCode.KronicBoostClosesScreen1x1OpensScreen0x1TopAligned] = [new(2, RoomScrollState.Blue), new(3, RoomScrollState.RedBoundary)],
            [DoorSetupCode.MickeyMouseClosesScreen2x3] = [new(14, RoomScrollState.RedBoundary)],
            [DoorSetupCode.SpongeBathOpensScreen1x0TopAligned] = [new(1, RoomScrollState.Blue)],
            [DoorSetupCode.ElectricDeathOpensScreen0x0] = [new(0, RoomScrollState.Green)],
            [DoorSetupCode.BasementClosesScreen3x0OpensScreen4x0TopAligned] = [new(3, RoomScrollState.RedBoundary), new(4, RoomScrollState.Blue)],
            [DoorSetupCode.WreckedShipMainShaftOpensScreen5x6TopAligned] = [new(41, RoomScrollState.Blue)],
            [DoorSetupCode.WreckedShipMainShaftOpensScreens4x6And4x7] = [new(40, RoomScrollState.Green), new(46, RoomScrollState.Green)],
            [DoorSetupCode.BowlingAlleyClosesScreens0x1And1x1And2x1And3x1And4x1And5x1] = [new(6, RoomScrollState.RedBoundary), new(7, RoomScrollState.RedBoundary), new(8, RoomScrollState.RedBoundary), new(9, RoomScrollState.RedBoundary), new(10, RoomScrollState.RedBoundary), new(11, RoomScrollState.RedBoundary)],
            [DoorSetupCode.HalfieClimbClosesScreen0x2OpensScreen1x2TopAligned] = [new(10, RoomScrollState.RedBoundary), new(11, RoomScrollState.Blue)],
            [DoorSetupCode.EastTunnelFromGlassTunnelClosesScreen0x0OpensScreen0x1TopAligned] = [new(0, RoomScrollState.RedBoundary), new(4, RoomScrollState.Blue)],
            [DoorSetupCode.CrabHoleClosesScreen0x0OpensScreen0x1TopAligned] = [new(0, RoomScrollState.RedBoundary), new(1, RoomScrollState.Blue)],
            [DoorSetupCode.CaterpillarClosesScreen0x3OpensScreen1x3TopAligned] = [new(9, RoomScrollState.RedBoundary), new(10, RoomScrollState.Blue)],
            [DoorSetupCode.GlassTunnelClosesScreens0x0And0x2OpensScreen0x1TopAligned] = [new(0, RoomScrollState.RedBoundary), new(2, RoomScrollState.RedBoundary), new(1, RoomScrollState.Blue)],
            [DoorSetupCode.BotwoonOpensScreen1x0TopAligned] = [new(1, RoomScrollState.Blue)],
            [DoorSetupCode.CrabShaftOpensScreen0x3TopAligned] = [new(6, RoomScrollState.Blue)],
            [DoorSetupCode.PantsClosesScreen0x2] = [new(4, RoomScrollState.RedBoundary)],
            [DoorSetupCode.PantsClosesScreens0x2And1x3] = [new(4, RoomScrollState.RedBoundary), new(7, RoomScrollState.RedBoundary)],
            [DoorSetupCode.EastPantsClosesScreen0x2OpensScreen0x1TopAligned] = [new(1, RoomScrollState.Blue), new(2, RoomScrollState.RedBoundary)],
            [DoorSetupCode.ThePreciousOpensScreens0x0And0x1] = [new(0, RoomScrollState.Green), new(2, RoomScrollState.Green)],
            [DoorSetupCode.StatuesOpensScreens0x0And0x1] = [new(0, RoomScrollState.Green), new(1, RoomScrollState.Green)],
            [DoorSetupCode.ClimbClosesScreen1x8OpensScreen0x8TopAligned] = [new(24, RoomScrollState.Blue), new(25, RoomScrollState.RedBoundary)],
        };

    /// <summary>The setups that write no scroll cells.</summary>
    internal static IReadOnlySet<DoorSetupCode> WithoutScrollWrites { get; } = new HashSet<DoorSetupCode>
    {
        DoorSetupCode.None,
        DoorSetupCode.WreckedShipEntranceStartsTreadmillRightwards,
        DoorSetupCode.WreckedShipEntranceStartsTreadmillLeftwards,
        DoorSetupCode.ToiletStartsElevatubeRideFromSouth,
        DoorSetupCode.ToiletStartsElevatubeRideFromNorth,
        DoorSetupCode.PlasmaSparkEndsElevatubeRide,
        DoorSetupCode.OasisEndsElevatubeRideOpensScreens0x0And0x1,
        DoorSetupCode.CeresElevShowsTurningFloor,
        DoorSetupCode.FallingTileEndsTurningFloor,
    };
}
