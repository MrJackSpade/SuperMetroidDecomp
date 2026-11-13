namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Verification access to <see cref="DoorScrollChanges"/> members production does not use.</summary>
internal static class DoorScrollProgramsAccess
{
    /// <summary>Enumerates the door setup codes that own scroll-change programs.</summary>
    extension(DoorScrollChanges)
    {
        /// <summary>Original callback catalog order, generated without a stored registration table.</summary>
        internal static IEnumerable<DoorSetupCode> Pointers
        {
            get
            {
                yield return DoorSetupCode.ParlorOpensScreen1x1;
                yield return DoorSetupCode.ParlorOpensScreen0x0TopAligned;
                yield return DoorSetupCode.LandingSiteOpensScreen1x2TopAligned;
                yield return DoorSetupCode.ParlorClosesScreen4x0OpensScreen3x1;
                yield return DoorSetupCode.BowlingAlleyClosesScreens2x1And3x1And4x1And5x1;
                yield return DoorSetupCode.BowlingAlleyClosesScreens2x0And3x0And4x0And5x0And5x1And0x2And1x2And5x2;
                yield return DoorSetupCode.CrateriaKihunterOpensScreens1x0And1x1;
                yield return DoorSetupCode.ClimbOpensScreen2x0TopAligned;
                yield return DoorSetupCode.ClimbOpensScreen2x7TopAligned;
                yield return DoorSetupCode.GauntletETankOpensScreen4x0TopAligned;
                yield return DoorSetupCode.CaterpillarOpensScreen0x2;
                yield return DoorSetupCode.GreenPiratesShaftOpensScreen0x3;
                yield return DoorSetupCode.GreenBrinstarMainShaftOpensScreens0x6And0x7;
                yield return DoorSetupCode.EtecoonETankOpensScreens0x1And1x1TopAligned;
                yield return DoorSetupCode.GreenBrinstarMainShaftOpensScreen1x7TopAligned;
                yield return DoorSetupCode.PinkBrinstarPowerBombsOpensScreens0x1And1x1;
                yield return DoorSetupCode.WarehouseEntranceClosesScreen0x0OpensScreen1x0;
                yield return DoorSetupCode.DachoraOpensScreen4x1;
                yield return DoorSetupCode.BigPinkClosesScreen3x5OpensScreen4x5TopAligned;
                yield return DoorSetupCode.DachoraClosesScreen4x0;
                yield return DoorSetupCode.BigPinkOpensScreens2x6And1x7And2x7;
                yield return DoorSetupCode.BlueBrinstarETankOpensScreen2x0TopAligned;
                yield return DoorSetupCode.KraidEyeDoorOpensScreen0x0;
                yield return DoorSetupCode.RedTowerOpensScreens0x6And0x7;
                yield return DoorSetupCode.RedFishClosesScreen2x0OpensScreen1x0TopAligned;
                yield return DoorSetupCode.SporeSpawnSuperClosesScreen1x1OpensScreen1x0TopAligned;
                yield return DoorSetupCode.EastTunnelClosesScreen0x0OpensScreen0x1TopAligned;
                yield return DoorSetupCode.WarehouseKihunterOpensScreens2x0And3x0TopAligned;
                yield return DoorSetupCode.BelowSpazerOpensScreens0x0And1x0;
                yield return DoorSetupCode.BigPinkOpensScreen1x0;
                yield return DoorSetupCode.CaterpillarOpensScreens0x5And0x6;
                yield return DoorSetupCode.ConstructionZoneOpensScreen0x0OpensScreen0x1TopAligned;
                yield return DoorSetupCode.GreenBrinstarElevOpensScreen0x2BelowTheRoom;
                yield return DoorSetupCode.AcidStatueClosesScreens0x1And1x1OpensScreens0x2And1x2And2x2TopAligned;
                yield return DoorSetupCode.LNSpringBallMazeClosesScreen1x1OpensScreen4x0OpensScreens1x0And2x0And3x0TopAligned;
                yield return DoorSetupCode.CrocomireOpensScreens0x0And1x0TopAligned;
                yield return DoorSetupCode.BatCaveClosesScreen0x1OpensScreen0x0TopAligned;
                yield return DoorSetupCode.IceBeamGateOpensScreen3x1;
                yield return DoorSetupCode.HiJumpETankOpensScreens0x0And0x1;
                yield return DoorSetupCode.VolcanoClosesScreen2x2OpensScreens0x2And1x2TopAligned;
                yield return DoorSetupCode.IceBeamSnakeClosesScreen0x1OpensScreen1x1TopAligned;
                yield return DoorSetupCode.WastelandOpensScreen1x1;
                yield return DoorSetupCode.FastPillarsSetupClosesScreen0x1OpensScreen0x2TopAligned;
                yield return DoorSetupCode.RedKihunterShaftClosesScreen0x1OpensScreen0x0TopAligned;
                yield return DoorSetupCode.LNFirefleaClosesScreen1x1OpensScreen1x0TopAligned;
                yield return DoorSetupCode.GoldenTorizoClosesScreens1x0And0x1And1x1OpensScreen0x0TopAligned;
                yield return DoorSetupCode.WorstRoomInTheGameOpensScreen0x0;
                yield return DoorSetupCode.LNFirefleaClosesScreen1x1OpensScreens0x0And1x0TopAligned;
                yield return DoorSetupCode.GreenBubblesMissilesOpensScreen0x0TopAligned;
                yield return DoorSetupCode.PurpleShaftClosesScreen0x1OpensScreen0x0TopAligned;
                yield return DoorSetupCode.IceBeamGateOpensScreen3x3TopAligned;
                yield return DoorSetupCode.KronicBoostClosesScreen1x1OpensScreen0x1TopAligned;
                yield return DoorSetupCode.MickeyMouseClosesScreen2x3;
                yield return DoorSetupCode.SpongeBathOpensScreen1x0TopAligned;
                yield return DoorSetupCode.ElectricDeathOpensScreen0x0;
                yield return DoorSetupCode.BasementClosesScreen3x0OpensScreen4x0TopAligned;
                yield return DoorSetupCode.WreckedShipMainShaftOpensScreen5x6TopAligned;
                yield return DoorSetupCode.WreckedShipMainShaftOpensScreens4x6And4x7;
                yield return DoorSetupCode.BowlingAlleyClosesScreens0x1And1x1And2x1And3x1And4x1And5x1;
                yield return DoorSetupCode.HalfieClimbClosesScreen0x2OpensScreen1x2TopAligned;
                yield return DoorSetupCode.CrabHoleClosesScreen0x0OpensScreen0x1TopAligned;
                yield return DoorSetupCode.CaterpillarClosesScreen0x3OpensScreen1x3TopAligned;
                yield return DoorSetupCode.GlassTunnelClosesScreens0x0And0x2OpensScreen0x1TopAligned;
                yield return DoorSetupCode.BotwoonOpensScreen1x0TopAligned;
                yield return DoorSetupCode.CrabShaftOpensScreen0x3TopAligned;
                yield return DoorSetupCode.PantsClosesScreen0x2;
                yield return DoorSetupCode.PantsClosesScreens0x2And1x3;
                yield return DoorSetupCode.EastPantsClosesScreen0x2OpensScreen0x1TopAligned;
                yield return DoorSetupCode.ThePreciousOpensScreens0x0And0x1;
                yield return DoorSetupCode.StatuesOpensScreens0x0And0x1;
                yield return DoorSetupCode.ClimbClosesScreen1x8OpensScreen0x8TopAligned;
            }
        }
    }
}
