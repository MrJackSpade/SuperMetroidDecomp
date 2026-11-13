using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Scroll-cell portion of door setup programs.</summary>
internal static class DoorSetupCodeInterpreter
{
    /// <summary>Applies only the scroll-cell effects of a door setup program; non-scroll setup remains with the runtime.</summary>
    public static void ApplyScrollWrites(
        DoorSetupCode setupCode,
        string doorName,
        RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        switch (setupCode)
        {
            case DoorSetupCode.None:
            case DoorSetupCode.CeresElevShowsTurningFloor:
            case DoorSetupCode.FallingTileEndsTurningFloor:
            case DoorSetupCode.WreckedShipEntranceStartsTreadmillRightwards:
            case DoorSetupCode.WreckedShipEntranceStartsTreadmillLeftwards:
            case DoorSetupCode.ToiletStartsElevatubeRideFromSouth:
            case DoorSetupCode.ToiletStartsElevatubeRideFromNorth:
            case DoorSetupCode.PlasmaSparkEndsElevatubeRide:
                // The two Ceres setups change the rotating-room view, not room scroll cells.
                // The treadmill and first three elevatube setups likewise own only
                // object/Samus state. Their non-scroll effects are applied by the runtime.
                return;

            case DoorSetupCode.OasisEndsElevatubeRideOpensScreens0x0And0x1:
                // The setup sets the first two cells together; the grid
                // takes both as green before Samus is unlocked.
                scrolls.SetCell(0, RoomScrollState.Green);
                scrolls.SetCell(1, RoomScrollState.Green);
                return;

            case DoorSetupCode.ParlorOpensScreen1x1:
            case DoorSetupCode.ParlorOpensScreen0x0TopAligned:
            case DoorSetupCode.LandingSiteOpensScreen1x2TopAligned:
            case DoorSetupCode.ParlorClosesScreen4x0OpensScreen3x1:
            case DoorSetupCode.BowlingAlleyClosesScreens2x1And3x1And4x1And5x1:
            case DoorSetupCode.BowlingAlleyClosesScreens2x0And3x0And4x0And5x0And5x1And0x2And1x2And5x2:
            case DoorSetupCode.CrateriaKihunterOpensScreens1x0And1x1:
            case DoorSetupCode.ClimbOpensScreen2x0TopAligned:
            case DoorSetupCode.ClimbOpensScreen2x7TopAligned:
            case DoorSetupCode.GauntletETankOpensScreen4x0TopAligned:
            case DoorSetupCode.CaterpillarFromRedBinstarElevOpensScreen0x2:
            case DoorSetupCode.GreenPiratesShaftOpensScreen0x3:
            case DoorSetupCode.GreenBrinstarMainShaftOpensScreens0x6And0x7:
            case DoorSetupCode.EtecoonETankOpensScreens0x1And1x1TopAligned:
            case DoorSetupCode.GreenBrinstarMainShaftOpensScreen1x7TopAligned:
            case DoorSetupCode.PinkBrinstarPowerBombsOpensScreens0x1And1x1:
            case DoorSetupCode.WarehouseEntranceClosesScreen0x0OpensScreen1x0:
            case DoorSetupCode.DachoraOpensScreen4x1:
            case DoorSetupCode.BigPinkClosesScreen3x5OpensScreen4x5TopAligned:
            case DoorSetupCode.DachoraClosesScreen4x0:
            case DoorSetupCode.BigPinkOpensScreens2x6And1x7And2x7:
            case DoorSetupCode.BlueBrinstarETankOpensScreen2x0TopAligned:
            case DoorSetupCode.KraidEyeDoorOpensScreen0x0:
            case DoorSetupCode.RedTowerOpensScreens0x6And0x7:
            case DoorSetupCode.RedFishClosesScreen2x0OpensScreen1x0TopAligned:
            case DoorSetupCode.SporeSpawnSuperClosesScreen1x1OpensScreen1x0TopAligned:
            case DoorSetupCode.EastTunnelFromWarehouseEntranceClosesScreen0x0OpensScreen0x1TopAligned:
            case DoorSetupCode.WarehouseKihunterOpensScreens2x0And3x0TopAligned:
            case DoorSetupCode.BelowSpazerOpensScreens0x0And1x0:
            case DoorSetupCode.BigPinkOpensScreen1x0:
            case DoorSetupCode.CaterpillarOpensScreens0x5And0x6:
            case DoorSetupCode.CaterpillarFromHellwayOpensScreen0x2:
            case DoorSetupCode.ConstructionZoneOpensScreen0x0OpensScreen0x1TopAligned:
            case DoorSetupCode.GreenBrinstarElevOpensScreen0x2BelowTheRoom:
            case DoorSetupCode.AcidStatueClosesScreens0x1And1x1OpensScreens0x2And1x2And2x2TopAligned:
            case DoorSetupCode.LNSpringBallMazeClosesScreen1x1OpensScreen4x0OpensScreens1x0And2x0And3x0TopAligned:
            case DoorSetupCode.CrocomireOpensScreens0x0And1x0TopAligned:
            case DoorSetupCode.BatCaveClosesScreen0x1OpensScreen0x0TopAligned:
            case DoorSetupCode.IceBeamGateOpensScreen3x1:
            case DoorSetupCode.HiJumpETankOpensScreens0x0And0x1:
            case DoorSetupCode.VolcanoClosesScreen2x2OpensScreens0x2And1x2TopAligned:
            case DoorSetupCode.IceBeamSnakeClosesScreen0x1OpensScreen1x1TopAligned:
            case DoorSetupCode.WastelandOpensScreen1x1:
            case DoorSetupCode.FastPillarsSetupClosesScreen0x1OpensScreen0x2TopAligned:
            case DoorSetupCode.RedKihunterShaftFromAmphitheatreClosesScreen0x1OpensScreen0x0TopAligned:
            case DoorSetupCode.LNFirefleaClosesScreen1x1OpensScreen1x0TopAligned:
            case DoorSetupCode.GoldenTorizoClosesScreens1x0And0x1And1x1OpensScreen0x0TopAligned:
            case DoorSetupCode.WorstRoomInTheGameOpensScreen0x0:
            case DoorSetupCode.LNFirefleaClosesScreen1x1OpensScreens0x0And1x0TopAligned:
            case DoorSetupCode.RedKihunterShaftFromLNFirefleaClosesScreen0x1OpensScreen0x0TopAligned:
            case DoorSetupCode.GreenBubblesMissilesOpensScreen0x0TopAligned:
            case DoorSetupCode.PurpleShaftClosesScreen0x1OpensScreen0x0TopAligned:
            case DoorSetupCode.IceBeamGateOpensScreen3x3TopAligned:
            case DoorSetupCode.KronicBoostClosesScreen1x1OpensScreen0x1TopAligned:
            case DoorSetupCode.MickeyMouseClosesScreen2x3:
            case DoorSetupCode.SpongeBathOpensScreen1x0TopAligned:
            case DoorSetupCode.ElectricDeathOpensScreen0x0:
            case DoorSetupCode.BasementClosesScreen3x0OpensScreen4x0TopAligned:
            case DoorSetupCode.WreckedShipMainShaftOpensScreen5x6TopAligned:
            case DoorSetupCode.WreckedShipMainShaftOpensScreens4x6And4x7:
            case DoorSetupCode.BowlingAlleyClosesScreens0x1And1x1And2x1And3x1And4x1And5x1:
            case DoorSetupCode.HalfieClimbClosesScreen0x2OpensScreen1x2TopAligned:
            case DoorSetupCode.EastTunnelFromGlassTunnelClosesScreen0x0OpensScreen0x1TopAligned:
            case DoorSetupCode.CrabHoleClosesScreen0x0OpensScreen0x1TopAligned:
            case DoorSetupCode.CaterpillarClosesScreen0x3OpensScreen1x3TopAligned:
            case DoorSetupCode.GlassTunnelClosesScreens0x0And0x2OpensScreen0x1TopAligned:
            case DoorSetupCode.BotwoonOpensScreen1x0TopAligned:
            case DoorSetupCode.CrabShaftOpensScreen0x3TopAligned:
            case DoorSetupCode.PantsClosesScreen0x2:
            case DoorSetupCode.PantsClosesScreens0x2And1x3:
            case DoorSetupCode.EastPantsClosesScreen0x2OpensScreen0x1TopAligned:
            case DoorSetupCode.ThePreciousOpensScreens0x0And0x1:
            case DoorSetupCode.StatuesOpensScreens0x0And0x1:
            case DoorSetupCode.ClimbClosesScreen1x8OpensScreen0x8TopAligned:
            default:
                if (DoorScrollChanges.TryApply(setupCode, scrolls))
                    return;
                throw new InvalidOperationException(
                    $"Undefined {nameof(DoorSetupCode)} {setupCode} in door {doorName}.");
        }
    }
}
