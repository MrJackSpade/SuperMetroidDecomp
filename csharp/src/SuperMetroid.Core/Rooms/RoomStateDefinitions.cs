namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Selects the initialization settings for each of the 323 retail room states.
/// The bank-$8F identities dispatch directly to level, graphics/music, population,
/// scroll, background and callback selections. Each field preserves its native
/// byte/word width from LoadStateHeader ($82:DEF2); no packed payload, lookup array
/// or generated cache is kept. Interior and unknown addresses are rejected.
/// </summary>
public static class RoomStateDefinitions
{

    /// <summary>Enumerates every selected state in ascending native identity order.</summary>
    public static IEnumerable<CartridgeRoomState> All
    {
        get
        {
            for (int pointer = 0x8000; pointer <= ushort.MaxValue; pointer++)
                if (Select((ushort)pointer) is { } state)
                    yield return state;
        }
    }

    /// <summary>Returns the compiled initialization settings for a selected retail state.</summary>
    public static CartridgeRoomState Get(ushort statePointer) => Select(statePointer) ??
        throw new ArgumentOutOfRangeException(nameof(statePointer), statePointer,
            "Pointer is not one of the 323 selected retail room states.");

    /// <summary>Maps a bank-$8F state pointer to its compiled retail settings, leaving unrecognized pointers unselected.</summary>
    private static CartridgeRoomState? Select(ushort statePointer) => statePointer switch
    {
        0x9213 =>
            new(0x9213, 0xC2C2BB, 0x00, 0x06, 0x05, 0x80C0, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.LandingSite, 0x0000, RoomMainCallback.ScrollingSkyLand, 0x8000, 0xB76A, RoomSetupCallback.ScrollingSkyLand),
        0x922D =>
            new(0x922D, 0xC2C2BB, 0x00, 0x06, 0x06, 0x80C0, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.LandingSite, 0x0000, RoomMainCallback.ScrollingSkyLand, 0x8000, 0xB76A, RoomSetupCallback.ScrollingSkyLand),
        0x9247 =>
            new(0x9247, 0xC2C2BB, 0x00, 0x0C, 0x05, 0x80D0, Enemies.RoomEnemyPopulationDefinitions.LandingSite012, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite012, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.LandingSite, 0x0000, RoomMainCallback.ScrollingSkyLand, 0x8000, 0xB76A, RoomSetupCallback.ScrollingSkyLand),
        0x9261 =>
            new(0x9261, 0xC2C2BB, 0x00, 0x00, 0x00, 0x8000, Enemies.RoomEnemyPopulationDefinitions.LandingSite3, Enemies.RoomEnemyGraphicsSetDefinitions.LandingSite3, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.LandingSite, 0x0000, RoomMainCallback.ScrollingSkyLandZebesTimebombSet, 0x8026, 0xB76A, RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape),
        0x92C5 =>
            new(0x92C5, 0xC2D6E8, 0x00, 0x00, 0x00, 0x80E0, Enemies.RoomEnemyPopulationDefinitions.GauntletEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletEntrance, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8058, 0xB899, RoomSetupCallback.Return),
        0x92DF =>
            new(0x92DF, 0xC2D6E8, 0x00, 0x09, 0x05, 0x80E0, Enemies.RoomEnemyPopulationDefinitions.GauntletEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletEntrance, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8058, 0xB899, RoomSetupCallback.Return),
        0x9314 =>
            new(0x9314, 0xC2DBC4, 0x00, 0x00, 0x00, 0x80F0, Enemies.RoomEnemyPopulationDefinitions.Parlor0, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Parlor, 0x0000, RoomMainCallback.None, 0x805A, 0xB8B4, RoomSetupCallback.Return),
        0x932E =>
            new(0x932E, 0xC2DBC4, 0x00, 0x09, 0x05, 0x8050, Enemies.RoomEnemyPopulationDefinitions.Parlor1, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Parlor, 0x0000, RoomMainCallback.None, 0x805A, 0xB8B4, RoomSetupCallback.ReturnBeforeEscapeSkySetup),
        0x9348 =>
            new(0x9348, 0xC2DBC4, 0x00, 0x00, 0x00, 0x8010, Enemies.RoomEnemyPopulationDefinitions.Parlor2, Enemies.RoomEnemyGraphicsSetDefinitions.Parlor2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Parlor, 0x0000, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, 0x8104, 0xB8B4, RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen),
        0x93B7 =>
            new(0x93B7, 0xC2E977, 0x00, 0x00, 0x00, 0x8100, Enemies.RoomEnemyPopulationDefinitions.CrateriaPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaPowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrateriaPowerBomb, 0x0000, RoomMainCallback.ScrollingSkyLand, 0x81CC, 0xB8B4, RoomSetupCallback.Return),
        0x93E2 =>
            new(0x93E2, 0xCE92CB, 0x19, 0x09, 0x05, 0x8110, Enemies.RoomEnemyPopulationDefinitions.CrateriaSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x81D4, 0x0000, RoomSetupCallback.Return),
        0x940B =>
            new(0x940B, 0xC2EB45, 0x00, 0x0C, 0x05, 0x8112, Enemies.RoomEnemyPopulationDefinitions.WestOcean, Enemies.RoomEnemyGraphicsSetDefinitions.WestOcean, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.WestOcean, 0x0000, RoomMainCallback.ScrollingSkyOcean, 0x81DC, 0xB7AE, RoomSetupCallback.ScrollingSkyOcean),
        0x946E =>
            new(0x946E, 0xC38E1F, 0x00, 0x00, 0x00, 0x8122, Enemies.RoomEnemyPopulationDefinitions.BowlingAlleyPath, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlleyPath, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x81FC, 0xB899, RoomSetupCallback.Return),
        0x9499 =>
            new(0x9499, 0xC38FFE, 0x00, 0x00, 0x05, 0x8132, Enemies.RoomEnemyPopulationDefinitions.CrateriaKihunter, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaKihunter, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrateriaKihunter, 0x0000, RoomMainCallback.None, 0x81FE, 0xB8B4, RoomSetupCallback.Return),
        0x94D9 =>
            new(0x94D9, 0xC39BFC, 0x02, 0x09, 0x03, 0x8142, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayElev, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElev, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ForgottenHighwayElev, 0x0000, RoomMainCallback.None, 0x8230, 0xB93B, RoomSetupCallback.Return),
        0x950A =>
            new(0x950A, 0xC39DB8, 0x00, 0x0C, 0x05, 0x8144, Enemies.RoomEnemyPopulationDefinitions.EastOcean, Enemies.RoomEnemyGraphicsSetDefinitions.EastOcean, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.EastOcean, 0x0000, RoomMainCallback.ScrollingSkyOcean, 0x8238, 0xB7F2, RoomSetupCallback.ScrollingSkyOcean),
        0x955F =>
            new(0x955F, 0xC3ACDB, 0x00, 0x00, 0x00, 0x8154, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayKagos, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayKagos, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AllGreen, 0x0000, RoomMainCallback.None, 0x823A, 0xB87E, RoomSetupCallback.Return),
        0x958A =>
            new(0x958A, 0xC3B1BA, 0x00, 0x00, 0x00, 0x8156, Enemies.RoomEnemyPopulationDefinitions.CrabMaze, Enemies.RoomEnemyGraphicsSetDefinitions.CrabMaze, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AllGreen, 0x0000, RoomMainCallback.None, 0x823C, 0xB8B4, RoomSetupCallback.Return),
        0x95B5 =>
            new(0x95B5, 0xC3BB9B, 0x00, 0x00, 0x05, 0x8166, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwayElbow, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwayElbow, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ForgottenHighwayElbow, 0x0000, RoomMainCallback.None, 0x823E, 0xB87E, RoomSetupCallback.Return),
        0x95E1 =>
            new(0x95E1, 0xC3BCD2, 0x00, 0x00, 0x00, 0x8168, Enemies.RoomEnemyPopulationDefinitions.CrateriaTube, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaTube, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8246, 0x0000, RoomSetupCallback.Return),
        0x960C =>
            new(0x960C, 0xC3BD6D, 0x00, 0x00, 0x00, 0x816A, Enemies.RoomEnemyPopulationDefinitions.Moat, Enemies.RoomEnemyGraphicsSetDefinitions.Moat, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8248, 0xB8B4, RoomSetupCallback.Return),
        0x9637 =>
            new(0x9637, 0xC3C145, 0x02, 0x00, 0x03, 0x817A, Enemies.RoomEnemyPopulationDefinitions.RedBinstarElev, Enemies.RoomEnemyGraphicsSetDefinitions.RedBinstarElev, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RedBinstarElev, 0x0000, RoomMainCallback.None, 0x8250, 0xB93B, RoomSetupCallback.Return),
        0x9668 =>
            new(0x9668, 0xC3C301, 0x00, 0x00, 0x00, 0x817C, Enemies.RoomEnemyPopulationDefinitions.GauntletETank, Enemies.RoomEnemyGraphicsSetDefinitions.GauntletETank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GauntletETank, 0x0000, RoomMainCallback.None, 0x825E, 0xB899, RoomSetupCallback.Return),
        0x969C =>
            new(0x969C, 0xC3C80C, 0x00, 0x0C, 0x05, 0x818C, Enemies.RoomEnemyPopulationDefinitions.PreBowling, Enemies.RoomEnemyGraphicsSetDefinitions.PreBowling, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.ScrollingSkyOcean, 0x826C, 0xB80A, RoomSetupCallback.ScrollingSkyOcean),
        0x96D1 =>
            new(0x96D1, 0xC3C998, 0x03, 0x00, 0x00, 0x818E, Enemies.RoomEnemyPopulationDefinitions.Climb0, Enemies.RoomEnemyGraphicsSetDefinitions.Climb0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Climb, 0x0000, RoomMainCallback.None, 0x826E, 0xB905, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x96EB =>
            new(0x96EB, 0xC3C998, 0x02, 0x09, 0x05, 0x8060, Enemies.RoomEnemyPopulationDefinitions.Climb1, Enemies.RoomEnemyGraphicsSetDefinitions.Climb1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Climb, 0x0000, RoomMainCallback.None, 0x826E, 0xB905, RoomSetupCallback.ReturnBeforeEscapeSkySetup),
        0x9705 =>
            new(0x9705, 0xC3C998, 0x02, 0x24, 0x07, 0x8020, Enemies.RoomEnemyPopulationDefinitions.Climb2, Enemies.RoomEnemyGraphicsSetDefinitions.Climb2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Climb, 0x0000, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, 0x830C, 0xB905, RoomSetupCallback.AutoDestroyWallDuringEscape),
        0x976D =>
            new(0x976D, 0xC3D9F7, 0x03, 0x00, 0x05, 0x819E, Enemies.RoomEnemyPopulationDefinitions.Pit0, Enemies.RoomEnemyGraphicsSetDefinitions.Pit0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Pit, 0x0000, RoomMainCallback.None, 0x83B6, 0xB905, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9787 =>
            new(0x9787, 0xC3D9F7, 0x02, 0x09, 0x05, 0x8080, Enemies.RoomEnemyPopulationDefinitions.Pit1, Enemies.RoomEnemyGraphicsSetDefinitions.Pit1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Pit, 0x0000, RoomMainCallback.None, 0x83D0, 0xB905, RoomSetupCallback.ReturnBeforeEscapeSkySetup),
        0x97C6 =>
            new(0x97C6, 0xC3DF23, 0x02, 0x06, 0x05, 0x81AE, Enemies.RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, Enemies.RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, 0x0000, RoomMainCallback.None, 0x83F6, 0x0000, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x97E0 =>
            new(0x97E0, 0xC3DF23, 0x02, 0x00, 0x03, 0x8090, Enemies.RoomEnemyPopulationDefinitions.ElevToBlueBrinstar, Enemies.RoomEnemyGraphicsSetDefinitions.ElevToBlueBrinstar, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.ElevToBlueBrinstar, 0x0000, RoomMainCallback.None, 0x83F6, 0x0000, RoomSetupCallback.ReturnBeforeEscapeSkySetup),
        0x981B =>
            new(0x981B, 0xC3E0D0, 0x02, 0x24, 0x03, 0x81BE, Enemies.RoomEnemyPopulationDefinitions.BombTorizo01, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x83FE, 0xB905, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9835 =>
            new(0x9835, 0xC3E0D0, 0x02, 0x00, 0x03, 0x81BE, Enemies.RoomEnemyPopulationDefinitions.BombTorizo01, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x83FE, 0xB905, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x984F =>
            new(0x984F, 0xC3E0D0, 0x02, 0x00, 0x00, 0x8030, Enemies.RoomEnemyPopulationDefinitions.BombTorizo2, Enemies.RoomEnemyGraphicsSetDefinitions.BombTorizo2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x986B, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, 0x8412, 0xB905, RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape),
        0x9890 =>
            new(0x9890, 0xC3E16E, 0x02, 0x09, 0x05, 0x81C0, Enemies.RoomEnemyPopulationDefinitions.Flyway01, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8420, 0xB8EA, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x98AA =>
            new(0x98AA, 0xC3E16E, 0x02, 0x09, 0x05, 0x81C0, Enemies.RoomEnemyPopulationDefinitions.Flyway01, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8420, 0xB8EA, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x98C4 =>
            new(0x98C4, 0xC3E16E, 0x02, 0x24, 0x07, 0x8040, Enemies.RoomEnemyPopulationDefinitions.Flyway2, Enemies.RoomEnemyGraphicsSetDefinitions.Flyway2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions, 0x8428, 0xB8EA, RoomSetupCallback.ReturnAfterEscapeWallSetup),
        0x98EF =>
            new(0x98EF, 0xC3E232, 0x02, 0x00, 0x00, 0x81C2, Enemies.RoomEnemyPopulationDefinitions.PreMapFlyway, Enemies.RoomEnemyGraphicsSetDefinitions.PreMapFlyway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8430, 0xB920, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x991A =>
            new(0x991A, 0xC3E2FC, 0x02, 0x00, 0x00, 0x81C2, Enemies.RoomEnemyPopulationDefinitions.Terminator, Enemies.RoomEnemyGraphicsSetDefinitions.Terminator, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8432, 0xB8CF, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9945 =>
            new(0x9945, 0xC3E985, 0x02, 0x09, 0x03, 0x81C2, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarElev, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarElev, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenBrinstarElev, 0x0000, RoomMainCallback.None, 0x843A, 0x0000, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9976 =>
            new(0x9976, 0xC3EB35, 0x02, 0x00, 0x05, 0x81C2, Enemies.RoomEnemyPopulationDefinitions.LowerMushrooms, Enemies.RoomEnemyGraphicsSetDefinitions.LowerMushrooms, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8442, 0xB8CF, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x99A1 =>
            new(0x99A1, 0xCE86BD, 0x15, 0x00, 0x00, 0x81C2, Enemies.RoomEnemyPopulationDefinitions.CrateriaMap, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8444, 0x0000, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x99CA =>
            new(0x99CA, 0xC3EE60, 0x02, 0x00, 0x05, 0x81C2, Enemies.RoomEnemyPopulationDefinitions.GreenPiratesShaft, Enemies.RoomEnemyGraphicsSetDefinitions.GreenPiratesShaft, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.GreenPiratesShaft, 0x0000, RoomMainCallback.None, 0x844C, 0xB956, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9A06 =>
            new(0x9A06, 0xC3F4D3, 0x02, 0x00, 0x00, 0x81C4, Enemies.RoomEnemyPopulationDefinitions.CrateriaSuper, Enemies.RoomEnemyGraphicsSetDefinitions.CrateriaSuper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrateriaSuper, 0x0000, RoomMainCallback.None, 0x8478, 0xB905, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9A56 =>
            new(0x9A56, 0xC4811E, 0x02, 0x00, 0x00, 0x81D4, Enemies.RoomEnemyPopulationDefinitions.FinalMissileBombway0, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissileBombway0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FinalMissileBombway, 0x0000, RoomMainCallback.None, 0x8480, 0xB956, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9A70 =>
            new(0x9A70, 0xC4811E, 0x02, 0x00, 0x00, 0x80A0, Enemies.RoomEnemyPopulationDefinitions.FinalMissileBombway1, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissileBombway1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FinalMissileBombway, 0x0000, RoomMainCallback.None, 0x8482, 0xB956, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9AA2 =>
            new(0x9AA2, 0xC48232, 0x02, 0x00, 0x00, 0x81E4, Enemies.RoomEnemyPopulationDefinitions.FinalMissile0, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissile0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FinalMissile, 0x0000, RoomMainCallback.None, 0x8484, 0xB905, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9ABC =>
            new(0x9ABC, 0xC48232, 0x02, 0x00, 0x00, 0x80B0, Enemies.RoomEnemyPopulationDefinitions.FinalMissile1, Enemies.RoomEnemyGraphicsSetDefinitions.FinalMissile1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FinalMissile, 0x0000, RoomMainCallback.None, 0x8486, 0xB905, RoomSetupCallback.ReturnAfterOceanSkySetup),
        0x9AE6 =>
            new(0x9AE6, 0xC4F1CE, 0x06, 0x0F, 0x05, 0x8204, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMainShaft, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMainShaft, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.GreenBrinstarMainShaft, 0x0000, RoomMainCallback.None, 0x848E, 0xBA37, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9B68 =>
            new(0x9B68, 0xC58BD5, 0x06, 0x00, 0x03, 0x8214, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnSuper, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnSuper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SporeSpawnSuper, 0x0000, RoomMainCallback.None, 0x84D8, 0xBABE, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9BAA =>
            new(0x9BAA, 0xC59642, 0x06, 0x00, 0x00, 0x8224, Enemies.RoomEnemyPopulationDefinitions.BrinstarPreMap, Enemies.RoomEnemyGraphicsSetDefinitions.BrinstarPreMap, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x84EC, 0xBA88, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9BD5 =>
            new(0x9BD5, 0xC59755, 0x06, 0x00, 0x05, 0x8226, Enemies.RoomEnemyPopulationDefinitions.EarlySupers, Enemies.RoomEnemyGraphicsSetDefinitions.EarlySupers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.EarlySupers, 0x0000, RoomMainCallback.None, 0x84F4, 0xBAA3, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9C14 =>
            new(0x9C14, 0xC59B00, 0x06, 0x00, 0x03, 0x8228, Enemies.RoomEnemyPopulationDefinitions.BrinstarReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.BrinstarReserveTank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BrinstarReserveTank, 0x0000, RoomMainCallback.None, 0x8526, 0xBA6D, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9C42 =>
            new(0x9C42, 0xCE83C3, 0x15, 0x00, 0x00, 0x822A, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMap, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8540, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9C6B =>
            new(0x9C6B, 0xC59CAC, 0x06, 0x00, 0x00, 0x822C, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarFirefleas, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarFirefleas, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8548, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9C96 =>
            new(0x9C96, 0xCE8CB3, 0x18, 0x00, 0x00, 0x823C, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarMissileRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarMissileRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenBrinstarMissileRefill, 0x0000, RoomMainCallback.None, 0x8550, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9CC0 =>
            new(0x9CC0, 0xC5A15F, 0x06, 0x00, 0x00, 0x823E, Enemies.RoomEnemyPopulationDefinitions.Dachora, Enemies.RoomEnemyGraphicsSetDefinitions.Dachora, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Dachora, 0x0000, RoomMainCallback.ScrollScreenRightInDachoraRoom, 0x8558, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9D26 =>
            new(0x9D26, 0xC5B54D, 0x06, 0x0F, 0x05, 0x824E, Enemies.RoomEnemyPopulationDefinitions.BigPink, Enemies.RoomEnemyGraphicsSetDefinitions.BigPink, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigPink, 0x0000, RoomMainCallback.None, 0x85E4, 0xBB45, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9DA9 =>
            new(0x9DA9, 0xC5CBA7, 0x06, 0x0F, 0x05, 0x825E, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnKihunters, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnKihunters, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8634, 0xBA52, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9DD9 =>
            new(0x9DD9, 0xC5CE34, 0x06, 0x2A, 0x05, 0x826E, Enemies.RoomEnemyPopulationDefinitions.SporeSpawn, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawn, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AllGreen, 0x0000, RoomMainCallback.None, 0x8642, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9DF3 =>
            new(0x9DF3, 0xC5CE34, 0x06, 0x00, 0x03, 0x826E, Enemies.RoomEnemyPopulationDefinitions.SporeSpawn, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawn, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AllGreen, 0x0000, RoomMainCallback.None, 0x8642, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9E1E =>
            new(0x9E1E, 0xC5D18F, 0x06, 0x00, 0x00, 0x827E, Enemies.RoomEnemyPopulationDefinitions.PinkBrinstarPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PinkBrinstarPowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PinkBrinstarPowerBombs, 0x0000, RoomMainCallback.None, 0x864A, 0xBAA3, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9E5F =>
            new(0x9E5F, 0xC5D559, 0x06, 0x0F, 0x05, 0x828E, Enemies.RoomEnemyPopulationDefinitions.GreenHillZone, Enemies.RoomEnemyGraphicsSetDefinitions.GreenHillZone, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenHillZone, 0x0000, RoomMainCallback.None, 0x8664, 0xBAD9, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9EB1 =>
            new(0x9EB1, 0xC5DED5, 0x06, 0x06, 0x07, 0x8290, Enemies.RoomEnemyPopulationDefinitions.MorphBall0, Enemies.RoomEnemyGraphicsSetDefinitions.MorphBall0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MorphBall, 0x0000, RoomMainCallback.None, 0x867E, 0xBAF4, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9ECB =>
            new(0x9ECB, 0xC5DED5, 0x06, 0x09, 0x05, 0x81F4, Enemies.RoomEnemyPopulationDefinitions.MorphBall1, Enemies.RoomEnemyGraphicsSetDefinitions.MorphBall1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MorphBall, 0x0000, RoomMainCallback.None, 0x86E6, 0xBAF4, RoomSetupCallback.ReturnBeforeEscapeSkySetup),
        0x9F23 =>
            new(0x9F23, 0xC5E63A, 0x06, 0x00, 0x00, 0x82A0, Enemies.RoomEnemyPopulationDefinitions.ConstructionZone0, Enemies.RoomEnemyGraphicsSetDefinitions.ConstructionZone0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ConstructionZone, 0x0000, RoomMainCallback.None, 0x8754, 0xBABE, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9F3D =>
            new(0x9F3D, 0xC5E63A, 0x06, 0x00, 0x00, 0x81F4, Enemies.RoomEnemyPopulationDefinitions.ConstructionZone1, Enemies.RoomEnemyGraphicsSetDefinitions.ConstructionZone1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ConstructionZone, 0x0000, RoomMainCallback.None, 0x8754, 0xBABE, RoomSetupCallback.ReturnBeforeEscapeSkySetup),
        0x9F76 =>
            new(0x9F76, 0xC5E86F, 0x06, 0x00, 0x00, 0x82B0, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarETank0, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueBrinstarETank, 0x0000, RoomMainCallback.None, 0x878C, 0xBAF4, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9F90 =>
            new(0x9F90, 0xC5E86F, 0x06, 0x00, 0x00, 0x81F4, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarETank1, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarETank1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueBrinstarETank, 0x0000, RoomMainCallback.None, 0x878C, 0xBAF4, RoomSetupCallback.ReturnBeforeEscapeSkySetup),
        0x9FC7 =>
            new(0x9FC7, 0xC5ECAE, 0x06, 0x0F, 0x05, 0x82C0, Enemies.RoomEnemyPopulationDefinitions.NoobBridge, Enemies.RoomEnemyGraphicsSetDefinitions.NoobBridge, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x87A6, 0xBA52, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0x9FF2 =>
            new(0x9FF2, 0xC5EF71, 0x06, 0x00, 0x00, 0x82D0, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarBeetoms, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarBeetoms, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenBrinstarBeetoms, 0x0000, RoomMainCallback.None, 0x87AE, 0xBA52, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA01E =>
            new(0xA01E, 0xC5F057, 0x06, 0x00, 0x00, 0x82D2, Enemies.RoomEnemyPopulationDefinitions.EtecoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonETank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.EtecoonETank, 0x0000, RoomMainCallback.None, 0x87B0, 0xBA52, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA05E =>
            new(0xA05E, 0xC5F43E, 0x06, 0x00, 0x00, 0x82E2, Enemies.RoomEnemyPopulationDefinitions.EtecoonSuper, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonSuper, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.EtecoonSuper, 0x0000, RoomMainCallback.None, 0x87D0, 0xBA52, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA088 =>
            new(0xA088, 0xCE89B6, 0x17, 0x00, 0x00, 0x82F2, Enemies.RoomEnemyPopulationDefinitions.DachoraEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.DachoraEnergyRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x87D8, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA0B1 =>
            new(0xA0B1, 0xC5F4C9, 0x06, 0x0F, 0x05, 0x82F4, Enemies.RoomEnemyPopulationDefinitions.SporeSpawnFarming, Enemies.RoomEnemyGraphicsSetDefinitions.SporeSpawnFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.SporeSpawnFarming, 0x0000, RoomMainCallback.None, 0x87E0, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA0DF =>
            new(0xA0DF, 0xC5F778, 0x06, 0x00, 0x00, 0x8304, Enemies.RoomEnemyPopulationDefinitions.WaterwayETank, Enemies.RoomEnemyGraphicsSetDefinitions.WaterwayETank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.WaterwayETank, 0x0000, RoomMainCallback.None, 0x87E8, 0xBAA3, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA114 =>
            new(0xA114, 0xC5FD50, 0x06, 0x00, 0x00, 0x8314, Enemies.RoomEnemyPopulationDefinitions.FirstMissile, Enemies.RoomEnemyGraphicsSetDefinitions.FirstMissile, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8802, 0xBABE, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA13D =>
            new(0xA13D, 0xC5FE1B, 0x06, 0x00, 0x00, 0x8324, Enemies.RoomEnemyPopulationDefinitions.PinkBrinstarHoppers, Enemies.RoomEnemyGraphicsSetDefinitions.PinkBrinstarHoppers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x880A, 0xBAA3, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA168 =>
            new(0xA168, 0xC681C2, 0x06, 0x00, 0x00, 0x8334, Enemies.RoomEnemyPopulationDefinitions.HopperETank, Enemies.RoomEnemyGraphicsSetDefinitions.HopperETank, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8824, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA191 =>
            new(0xA191, 0xCE92CB, 0x19, 0x0F, 0x05, 0x8336, Enemies.RoomEnemyPopulationDefinitions.BigPinkSaveRoom, Enemies.RoomEnemyGraphicsSetDefinitions.BigPinkSaveRoom, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x882C, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA1BA =>
            new(0xA1BA, 0xC68318, 0x06, 0x00, 0x00, 0x8338, Enemies.RoomEnemyPopulationDefinitions.BlueBrinstarBoulders, Enemies.RoomEnemyGraphicsSetDefinitions.BlueBrinstarBoulders, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8834, 0xBAF4, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA1E5 =>
            new(0xA1E5, 0xC68437, 0x06, 0x00, 0x00, 0x8348, Enemies.RoomEnemyPopulationDefinitions.BillyMays, Enemies.RoomEnemyGraphicsSetDefinitions.BillyMays, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8836, 0xBAF4, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA20E =>
            new(0xA20E, 0xCE92CB, 0x19, 0x0F, 0x05, 0x834A, Enemies.RoomEnemyPopulationDefinitions.GreenBrinstarSave, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBrinstarSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8844, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA237 =>
            new(0xA237, 0xCE92CB, 0x19, 0x0F, 0x05, 0x834C, Enemies.RoomEnemyPopulationDefinitions.EtecoonSave, Enemies.RoomEnemyGraphicsSetDefinitions.EtecoonSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x884C, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupA),
        0xA260 =>
            new(0xA260, 0xC684EE, 0x07, 0x12, 0x05, 0x834E, Enemies.RoomEnemyPopulationDefinitions.RedTower, Enemies.RoomEnemyGraphicsSetDefinitions.RedTower, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RedTower, 0x0000, RoomMainCallback.None, 0x8854, 0xBB7B, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA2A0 =>
            new(0xA2A0, 0xC691E3, 0x07, 0x00, 0x05, 0x835E, Enemies.RoomEnemyPopulationDefinitions.RedBrinstarFirefleas, Enemies.RoomEnemyGraphicsSetDefinitions.RedBrinstarFirefleas, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.RedBrinstarFirefleas, 0x0000, RoomMainCallback.None, 0x886E, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA2DB =>
            new(0xA2DB, 0xC69BF9, 0x07, 0x00, 0x03, 0x836E, Enemies.RoomEnemyPopulationDefinitions.XrayScope, Enemies.RoomEnemyGraphicsSetDefinitions.XrayScope, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8876, 0xBC38, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA304 =>
            new(0xA304, 0xC69D70, 0x07, 0x00, 0x00, 0x8370, Enemies.RoomEnemyPopulationDefinitions.Hellway, Enemies.RoomEnemyGraphicsSetDefinitions.Hellway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x887E, 0xBBCC, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA32F =>
            new(0xA32F, 0xC69F4B, 0x07, 0x12, 0x05, 0x8380, Enemies.RoomEnemyPopulationDefinitions.Caterpillar, Enemies.RoomEnemyGraphicsSetDefinitions.Caterpillar, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Caterpillar, 0x0000, RoomMainCallback.None, 0x8880, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA389 =>
            new(0xA389, 0xC6B31F, 0x07, 0x00, 0x00, 0x8390, Enemies.RoomEnemyPopulationDefinitions.BetaPowerBomb, Enemies.RoomEnemyGraphicsSetDefinitions.BetaPowerBomb, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BetaPowerBomb, 0x0000, RoomMainCallback.None, 0x88BE, 0xBBE7, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA3BB =>
            new(0xA3BB, 0xC6B58C, 0x07, 0x00, 0x03, 0x83A0, Enemies.RoomEnemyPopulationDefinitions.AlphaPowerBomb, Enemies.RoomEnemyGraphicsSetDefinitions.AlphaPowerBomb, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AlphaPowerBomb, 0x0000, RoomMainCallback.None, 0x88D8, 0xBCEC, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA3EA =>
            new(0xA3EA, 0xC6B739, 0x07, 0x00, 0x00, 0x83B0, Enemies.RoomEnemyPopulationDefinitions.SkreeBoost, Enemies.RoomEnemyGraphicsSetDefinitions.SkreeBoost, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x891C, 0xBBE7, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA415 =>
            new(0xA415, 0xC6B91C, 0x07, 0x00, 0x05, 0x83C0, Enemies.RoomEnemyPopulationDefinitions.BelowSpazer, Enemies.RoomEnemyGraphicsSetDefinitions.BelowSpazer, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BelowSpazer, 0x0000, RoomMainCallback.None, 0x891E, 0xBC02, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA454 =>
            new(0xA454, 0xC6BCC7, 0x07, 0x00, 0x03, 0x83D0, Enemies.RoomEnemyPopulationDefinitions.Spazer, Enemies.RoomEnemyGraphicsSetDefinitions.Spazer, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Spazer, 0x0000, RoomMainCallback.None, 0x896E, 0xBCEC, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA47E =>
            new(0xA47E, 0xC6BD83, 0x07, 0x00, 0x05, 0x83D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseZeela, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseZeela, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WarehouseZeela, 0x0000, RoomMainCallback.None, 0x8976, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA4BE =>
            new(0xA4BE, 0xC6C469, 0x07, 0x00, 0x00, 0x83D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseETank, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseETank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8996, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA4E7 =>
            new(0xA4E7, 0xC6C630, 0x07, 0x12, 0x05, 0x83D2, Enemies.RoomEnemyPopulationDefinitions.WarehouseKihunter, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseKihunter, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WarehouseKihunter, 0x0000, RoomMainCallback.None, 0x89A4, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA533 =>
            new(0xA533, 0xC6CDB9, 0x07, 0x27, 0x06, 0x83E2, Enemies.RoomEnemyPopulationDefinitions.MiniKraid, Enemies.RoomEnemyGraphicsSetDefinitions.MiniKraid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x89F4, 0xBC6E, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA54D =>
            new(0xA54D, 0xC6CDB9, 0x07, 0x27, 0x03, 0x83E2, Enemies.RoomEnemyPopulationDefinitions.MiniKraid, Enemies.RoomEnemyGraphicsSetDefinitions.MiniKraid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x89F4, 0xBC6E, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA578 =>
            new(0xA578, 0xC6D2CB, 0x07, 0x00, 0x00, 0x83F2, Enemies.RoomEnemyPopulationDefinitions.KraidEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.KraidEyeDoor, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.KraidEyeDoor, 0x0000, RoomMainCallback.None, 0x8A02, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA5B1 =>
            new(0xA5B1, 0xC6D620, 0x1A, 0x27, 0x06, 0x83F4, Enemies.RoomEnemyPopulationDefinitions.Kraid, Enemies.RoomEnemyGraphicsSetDefinitions.Kraid, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Kraid, 0x0000, RoomMainCallback.None, 0x8A2E, 0xB815, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA5CB =>
            new(0xA5CB, 0xC6D620, 0x1A, 0x00, 0x00, 0x83F4, Enemies.RoomEnemyPopulationDefinitions.Kraid, Enemies.RoomEnemyGraphicsSetDefinitions.Kraid, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Kraid, 0x0000, RoomMainCallback.None, 0x8A2E, 0xB840, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA5FA =>
            new(0xA5FA, 0xC6D88D, 0x08, 0x00, 0x04, 0x83F6, Enemies.RoomEnemyPopulationDefinitions.StatuesHallway, Enemies.RoomEnemyGraphicsSetDefinitions.StatuesHallway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8A3C, 0xBCA4, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA625 =>
            new(0xA625, 0xCE89B6, 0x17, 0x00, 0x00, 0x8406, Enemies.RoomEnemyPopulationDefinitions.RedTowerRefill, Enemies.RoomEnemyGraphicsSetDefinitions.RedTowerRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8A3E, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA64E =>
            new(0xA64E, 0xCEA201, 0x18, 0x00, 0x00, 0x8408, Enemies.RoomEnemyPopulationDefinitions.KraidRefill, Enemies.RoomEnemyGraphicsSetDefinitions.KraidRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8A46, 0x0000, RoomSetupCallback.ReturnBeforeStatueSetupB),
        0xA677 =>
            new(0xA677, 0xCE9BE9, 0x15, 0x09, 0x06, 0x840A, Enemies.RoomEnemyPopulationDefinitions.Statues, Enemies.RoomEnemyGraphicsSetDefinitions.Statues, Scrolling.Layer2ScrollModes.FromHeader(0x81, 0x01), Scrolling.RoomScrollDefinitions.Statues, 0x0000, RoomMainCallback.None, 0x8A54, 0xBB60, RoomSetupCallback.RunStatueUnlockingAnimations),
        0xA6AE =>
            new(0xA6AE, 0xC6DEE0, 0x07, 0x12, 0x03, 0x841A, Enemies.RoomEnemyPopulationDefinitions.WarehouseEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseEntrance, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WarehouseEntrance, 0x0000, RoomMainCallback.None, 0x8A5C, 0xBC53, RoomSetupCallback.SharedReturn),
        0xA6EF =>
            new(0xA6EF, 0xC6E355, 0x07, 0x00, 0x03, 0x842A, Enemies.RoomEnemyPopulationDefinitions.VariaSuit, Enemies.RoomEnemyGraphicsSetDefinitions.VariaSuit, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8ACA, 0x0000, RoomSetupCallback.SharedReturn),
        0xA718 =>
            new(0xA718, 0xCE95C2, 0x19, 0x12, 0x05, 0x842C, Enemies.RoomEnemyPopulationDefinitions.WarehouseSave, Enemies.RoomEnemyGraphicsSetDefinitions.WarehouseSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8AD2, 0x0000, RoomSetupCallback.SharedReturn),
        0xA741 =>
            new(0xA741, 0xCE95C2, 0x19, 0x12, 0x05, 0x842E, Enemies.RoomEnemyPopulationDefinitions.RedBrinstarSave, Enemies.RoomEnemyGraphicsSetDefinitions.RedBrinstarSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8ADA, 0x0000, RoomSetupCallback.SharedReturn),
        0xA76A =>
            new(0xA76A, 0xC6E4A4, 0x09, 0x00, 0x00, 0x8430, Enemies.RoomEnemyPopulationDefinitions.IceBeamAcid, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamAcid, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8AE2, 0xBE3F, RoomSetupCallback.SharedReturnB),
        0xA795 =>
            new(0xA795, 0xC6E5F5, 0x09, 0x00, 0x00, 0x8440, Enemies.RoomEnemyPopulationDefinitions.Cathedral, Enemies.RoomEnemyGraphicsSetDefinitions.Cathedral, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8AE4, 0x0000, RoomSetupCallback.SharedReturnB),
        0xA7C0 =>
            new(0xA7C0, 0xC6ECB9, 0x09, 0x00, 0x00, 0x8450, Enemies.RoomEnemyPopulationDefinitions.CathedralEntrance, Enemies.RoomEnemyGraphicsSetDefinitions.CathedralEntrance, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8AF2, 0x0000, RoomSetupCallback.SharedReturnB),
        0xA7EB =>
            new(0xA7EB, 0xC6F2E1, 0x0A, 0x15, 0x05, 0x8460, Enemies.RoomEnemyPopulationDefinitions.BusinessCenter, Enemies.RoomEnemyGraphicsSetDefinitions.BusinessCenter, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8AFA, 0xBEE1, RoomSetupCallback.SharedReturnB),
        0xA822 =>
            new(0xA822, 0xC6F8C1, 0x0A, 0x00, 0x00, 0x8470, Enemies.RoomEnemyPopulationDefinitions.IceBeamGate, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamGate, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.IceBeamGate, 0x0000, RoomMainCallback.None, 0x8B14, 0xBE5A, RoomSetupCallback.SharedReturnB),
        0xA872 =>
            new(0xA872, 0xC782ED, 0x09, 0x00, 0x00, 0x8480, Enemies.RoomEnemyPopulationDefinitions.IceBeamTutorial, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamTutorial, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8B22, 0xBE3F, RoomSetupCallback.SharedReturnB),
        0xA89D =>
            new(0xA89D, 0xC784A3, 0x0A, 0x00, 0x03, 0x8490, Enemies.RoomEnemyPopulationDefinitions.IceBeam, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeam, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8B24, 0xBE5A, RoomSetupCallback.SharedReturnB),
        0xA8C6 =>
            new(0xA8C6, 0xC785D6, 0x09, 0x00, 0x05, 0x84A0, Enemies.RoomEnemyPopulationDefinitions.IceBeamSnake, Enemies.RoomEnemyGraphicsSetDefinitions.IceBeamSnake, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.IceBeamSnake, 0x0000, RoomMainCallback.None, 0x8B2C, 0xBE5A, RoomSetupCallback.SharedReturnB),
        0xA905 =>
            new(0xA905, 0xC78A47, 0x09, 0x00, 0x00, 0x84B0, Enemies.RoomEnemyPopulationDefinitions.CrumbleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.CrumbleShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8B46, 0xBEE1, RoomSetupCallback.SharedReturnB),
        0xA930 =>
            new(0xA930, 0xC78CFA, 0x09, 0x15, 0x05, 0x84C0, Enemies.RoomEnemyPopulationDefinitions.CrocomireSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSpeedway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrocomireSpeedway, 0x0000, RoomMainCallback.None, 0x8B4E, 0xBE5A, RoomSetupCallback.SharedReturnB),
        0xA99F =>
            new(0xA99F, 0xC79D71, 0x1B, 0x27, 0x05, 0x84D0, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Crocomire, 0x0000, RoomMainCallback.CrocomireRoomShaking, 0x8B9E, 0xB84D, RoomSetupCallback.SharedReturnC),
        0xA9B9 =>
            new(0xA9B9, 0xC79D71, 0x1B, 0x00, 0x00, 0x84D0, Enemies.RoomEnemyPopulationDefinitions.Crocomire, Enemies.RoomEnemyGraphicsSetDefinitions.Crocomire, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Crocomire, 0x0000, RoomMainCallback.CrocomireRoomShaking, 0x8B9E, 0xB858, RoomSetupCallback.SharedReturnC),
        0xA9F2 =>
            new(0xA9F2, 0xC7A036, 0x0A, 0x00, 0x03, 0x84E0, Enemies.RoomEnemyPopulationDefinitions.HiJumpBoots, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpBoots, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8BAC, 0xBE3F, RoomSetupCallback.SharedReturnC),
        0xAA1B =>
            new(0xAA1B, 0xC7A18D, 0x09, 0x00, 0x00, 0x84F0, Enemies.RoomEnemyPopulationDefinitions.CrocomireEscape, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireEscape, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.CrocomireEscape, 0x0000, RoomMainCallback.None, 0x8BB4, 0x0000, RoomSetupCallback.SharedReturnC),
        0xAA4E =>
            new(0xAA4E, 0xC7AA70, 0x0A, 0x00, 0x05, 0x8500, Enemies.RoomEnemyPopulationDefinitions.HiJumpETank, Enemies.RoomEnemyGraphicsSetDefinitions.HiJumpETank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.HiJumpETank, 0x0000, RoomMainCallback.None, 0x8BC8, 0x0000, RoomSetupCallback.SharedReturnC),
        0xAA8F =>
            new(0xAA8F, 0xC7AEB3, 0x09, 0x15, 0x05, 0x8510, Enemies.RoomEnemyPopulationDefinitions.PostCrocFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocFarming, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.PostCrocFarming, 0x0000, RoomMainCallback.None, 0x8BF4, 0x0000, RoomSetupCallback.SharedReturnC),
        0xAAC2 =>
            new(0xAAC2, 0xCE95C2, 0x19, 0x15, 0x05, 0x8520, Enemies.RoomEnemyPopulationDefinitions.PostCrocSave, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8BFC, 0x0000, RoomSetupCallback.SharedReturnC),
        0xAAEB =>
            new(0xAAEB, 0xC7B28B, 0x09, 0x00, 0x00, 0x8530, Enemies.RoomEnemyPopulationDefinitions.PostCrocPowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocPowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8C04, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xAB14 =>
            new(0xAB14, 0xC7B3E7, 0x0A, 0x00, 0x00, 0x8540, Enemies.RoomEnemyPopulationDefinitions.PostCrocShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PostCrocShaft, 0x0000, RoomMainCallback.None, 0x8C0C, 0xBEE1, RoomSetupCallback.SharedReturnC),
        0xAB48 =>
            new(0xAB48, 0xC7B780, 0x0A, 0x00, 0x00, 0x8550, Enemies.RoomEnemyPopulationDefinitions.PostCrocMissile, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocMissile, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8C14, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xAB71 =>
            new(0xAB71, 0xC7BB6B, 0x0A, 0x00, 0x00, 0x8560, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial3, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial3, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8C1C, 0xBE5A, RoomSetupCallback.SharedReturnC),
        0xAB9C =>
            new(0xAB9C, 0xC7BECB, 0x0A, 0x00, 0x05, 0x8570, Enemies.RoomEnemyPopulationDefinitions.PostCrocJump, Enemies.RoomEnemyGraphicsSetDefinitions.PostCrocJump, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.PostCrocJump, 0x0000, RoomMainCallback.None, 0x8C2A, 0x0000, RoomSetupCallback.SharedReturnC),
        0xABDF =>
            new(0xABDF, 0xC7CD91, 0x0A, 0x00, 0x00, 0x8580, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial2, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GrappleTutorial2, 0x0000, RoomMainCallback.None, 0x8C32, 0xBEE1, RoomSetupCallback.SharedReturnC),
        0xAC0D =>
            new(0xAC0D, 0xC7CFCD, 0x0A, 0x00, 0x05, 0x8590, Enemies.RoomEnemyPopulationDefinitions.GrappleTutorial1, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleTutorial1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8C34, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xAC38 =>
            new(0xAC38, 0xC7D13C, 0x0A, 0x00, 0x03, 0x85A0, Enemies.RoomEnemyPopulationDefinitions.GrappleBeam, Enemies.RoomEnemyGraphicsSetDefinitions.GrappleBeam, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GrappleBeam, 0x0000, RoomMainCallback.None, 0x8C36, 0xBEE1, RoomSetupCallback.SharedReturnC),
        0xAC67 =>
            new(0xAC67, 0xC7D4FE, 0x09, 0x00, 0x00, 0x85B0, Enemies.RoomEnemyPopulationDefinitions.NorfairReserveTank, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairReserveTank, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8C3E, 0xBF68, RoomSetupCallback.SharedReturnC),
        0xAC90 =>
            new(0xAC90, 0xC7D66F, 0x09, 0x00, 0x00, 0x85C0, Enemies.RoomEnemyPopulationDefinitions.GreenBubblesMissiles, Enemies.RoomEnemyGraphicsSetDefinitions.GreenBubblesMissiles, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenBubblesMissiles, 0x0000, RoomMainCallback.None, 0x8C4C, 0xBF83, RoomSetupCallback.SharedReturnC),
        0xACC0 =>
            new(0xACC0, 0xC7D895, 0x09, 0x00, 0x00, 0x85D0, Enemies.RoomEnemyPopulationDefinitions.BubbleMountain, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountain, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BubbleMountain, 0x0000, RoomMainCallback.None, 0x8C5A, 0xBF83, RoomSetupCallback.SharedReturnC),
        0xACFD =>
            new(0xACFD, 0xC7E08C, 0x09, 0x00, 0x05, 0x85E0, Enemies.RoomEnemyPopulationDefinitions.SpeedBoosterHall, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBoosterHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8C6E, 0xBE5A, RoomSetupCallback.SharedReturnC),
        0xAD28 =>
            new(0xAD28, 0xC7EAA8, 0x09, 0x00, 0x03, 0x8600, Enemies.RoomEnemyPopulationDefinitions.SpeedBooster, Enemies.RoomEnemyGraphicsSetDefinitions.SpeedBooster, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8C82, 0xBE3F, RoomSetupCallback.SharedReturnC),
        0xAD6B =>
            new(0xAD6B, 0xC7EC03, 0x09, 0x15, 0x05, 0x8610, Enemies.RoomEnemyPopulationDefinitions.SingleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.SingleChamber, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.SingleChamber, 0x0000, RoomMainCallback.None, 0x8C8A, 0x0000, RoomSetupCallback.SharedReturnC),
        0xADBA =>
            new(0xADBA, 0xC7FF02, 0x09, 0x00, 0x05, 0x8620, Enemies.RoomEnemyPopulationDefinitions.DoubleChamber, Enemies.RoomEnemyGraphicsSetDefinitions.DoubleChamber, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.DoubleChamber, 0x0000, RoomMainCallback.None, 0x8CB0, 0x0000, RoomSetupCallback.SharedReturnC),
        0xADEB =>
            new(0xADEB, 0xC88532, 0x09, 0x00, 0x03, 0x8630, Enemies.RoomEnemyPopulationDefinitions.WaveBeam, Enemies.RoomEnemyGraphicsSetDefinitions.WaveBeam, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8CCA, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xAE14 =>
            new(0xAE14, 0xC8865C, 0x09, 0x00, 0x00, 0x8640, Enemies.RoomEnemyPopulationDefinitions.SpikyPlatformsTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyPlatformsTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8CD2, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xAE3F =>
            new(0xAE3F, 0xC88953, 0x09, 0x00, 0x00, 0x8650, Enemies.RoomEnemyPopulationDefinitions.Volcano, Enemies.RoomEnemyGraphicsSetDefinitions.Volcano, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Volcano, 0x0000, RoomMainCallback.None, 0x8CD4, 0x0000, RoomSetupCallback.SharedReturnC),
        0xAE81 =>
            new(0xAE81, 0xC8943A, 0x09, 0x00, 0x00, 0x8670, Enemies.RoomEnemyPopulationDefinitions.KronicBoost, Enemies.RoomEnemyGraphicsSetDefinitions.KronicBoost, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.KronicBoost, 0x0000, RoomMainCallback.None, 0x8D1E, 0x0000, RoomSetupCallback.SharedReturnC),
        0xAEC1 =>
            new(0xAEC1, 0xC89D5D, 0x09, 0x00, 0x00, 0x8680, Enemies.RoomEnemyPopulationDefinitions.MagdolliteTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.MagdolliteTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8D56, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xAEEC =>
            new(0xAEEC, 0xC89FE3, 0x09, 0x00, 0x00, 0x8690, Enemies.RoomEnemyPopulationDefinitions.PurpleShaft, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.PurpleShaft, 0x0000, RoomMainCallback.None, 0x8D58, 0xBF68, RoomSetupCallback.SharedReturnC),
        0xAF21 =>
            new(0xAF21, 0xC8A2CF, 0x09, 0x00, 0x05, 0x86A0, Enemies.RoomEnemyPopulationDefinitions.LavaDive, Enemies.RoomEnemyGraphicsSetDefinitions.LavaDive, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8D7E, 0xBE5A, RoomSetupCallback.SharedReturnC),
        0xAF4C =>
            new(0xAF4C, 0xC8AA89, 0x09, 0x15, 0x05, 0x86B0, Enemies.RoomEnemyPopulationDefinitions.LowerNorfairElev, Enemies.RoomEnemyGraphicsSetDefinitions.LowerNorfairElev, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.LowerNorfairElev, 0x0000, RoomMainCallback.None, 0x8D80, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xAF7F =>
            new(0xAF7F, 0xC8ABED, 0x09, 0x00, 0x00, 0x86C0, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairFarming, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.UpperNorfairFarming, 0x0000, RoomMainCallback.None, 0x8D88, 0x0000, RoomSetupCallback.SharedReturnC),
        0xAFB0 =>
            new(0xAFB0, 0xC8B15B, 0x09, 0x00, 0x00, 0x86D0, Enemies.RoomEnemyPopulationDefinitions.RisingTide, Enemies.RoomEnemyGraphicsSetDefinitions.RisingTide, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8D96, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xAFDB =>
            new(0xAFDB, 0xC8B4F9, 0x09, 0x00, 0x00, 0x86F0, Enemies.RoomEnemyPopulationDefinitions.AcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.AcidSnakesTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8D98, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xB008 =>
            new(0xB008, 0xC8B853, 0x09, 0x00, 0x00, 0x8700, Enemies.RoomEnemyPopulationDefinitions.SpikyAcidSnakesTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyAcidSnakesTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8D9A, 0xBEC6, RoomSetupCallback.SharedReturnC),
        0xB033 =>
            new(0xB033, 0xCE8FA6, 0x17, 0x00, 0x00, 0x8710, Enemies.RoomEnemyPopulationDefinitions.UpperNorfairRefill, Enemies.RoomEnemyGraphicsSetDefinitions.UpperNorfairRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8D9C, 0x0000, RoomSetupCallback.SharedReturnC),
        0xB05E =>
            new(0xB05E, 0xC8B9CF, 0x09, 0x00, 0x00, 0x8720, Enemies.RoomEnemyPopulationDefinitions.PurpleFarming, Enemies.RoomEnemyGraphicsSetDefinitions.PurpleFarming, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8DA4, 0xBF68, RoomSetupCallback.SharedReturnC),
        0xB087 =>
            new(0xB087, 0xC8BB21, 0x09, 0x00, 0x05, 0x8730, Enemies.RoomEnemyPopulationDefinitions.BatCave, Enemies.RoomEnemyGraphicsSetDefinitions.BatCave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BatCave, 0x0000, RoomMainCallback.None, 0x8DA6, 0x0000, RoomSetupCallback.SharedReturnC),
        0xB0C1 =>
            new(0xB0C1, 0xCE83C3, 0x15, 0x00, 0x00, 0x8740, Enemies.RoomEnemyPopulationDefinitions.NorfairMap, Enemies.RoomEnemyGraphicsSetDefinitions.NorfairMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8DD8, 0x0000, RoomSetupCallback.SharedReturnC),
        0xB0EA =>
            new(0xB0EA, 0xCE92CB, 0x19, 0x15, 0x05, 0x8750, Enemies.RoomEnemyPopulationDefinitions.BubbleMountainSave, Enemies.RoomEnemyGraphicsSetDefinitions.BubbleMountainSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8DE0, 0x0000, RoomSetupCallback.SharedReturnC),
        0xB113 =>
            new(0xB113, 0xC8BDE8, 0x0A, 0x00, 0x00, 0x8760, Enemies.RoomEnemyPopulationDefinitions.FrogSpeedway, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSpeedway, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.FrogSpeedway, 0x0000, RoomMainCallback.None, 0x8DE8, 0xBE3F, RoomSetupCallback.SharedReturnC),
        0xB146 =>
            new(0xB146, 0xC8C165, 0x09, 0x00, 0x00, 0x8762, Enemies.RoomEnemyPopulationDefinitions.RedPirateShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedPirateShaft, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.RedPirateShaft, 0x0000, RoomMainCallback.None, 0x8DEA, 0xBEE1, RoomSetupCallback.SharedReturnC),
        0xB174 =>
            new(0xB174, 0xCE9EF6, 0x19, 0x15, 0x05, 0x8764, Enemies.RoomEnemyPopulationDefinitions.FrogSave, Enemies.RoomEnemyGraphicsSetDefinitions.FrogSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8DEC, 0x0000, RoomSetupCallback.SharedReturnC),
        0xB19F =>
            new(0xB19F, 0xCE95C2, 0x19, 0x15, 0x05, 0x8766, Enemies.RoomEnemyPopulationDefinitions.CrocomireSave, Enemies.RoomEnemyGraphicsSetDefinitions.CrocomireSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8DF4, 0x0000, RoomSetupCallback.SharedReturnC),
        0xB1C8 =>
            new(0xB1C8, 0xCE92CB, 0x19, 0x15, 0x05, 0x8768, Enemies.RoomEnemyPopulationDefinitions.LNElevSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNElevSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.LNElevSave, 0x0000, RoomMainCallback.None, 0x8DFC, 0x0000, RoomSetupCallback.None),
        0xB1F2 =>
            new(0xB1F2, 0xC8C4D3, 0x09, 0x18, 0x05, 0x876A, Enemies.RoomEnemyPopulationDefinitions.AcidStatue, Enemies.RoomEnemyGraphicsSetDefinitions.AcidStatue, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.AcidStatue, 0x0000, RoomMainCallback.None, 0x8E04, 0xBF32, RoomSetupCallback.SharedReturnD),
        0xB243 =>
            new(0xB243, 0xC8D59C, 0x09, 0x18, 0x05, 0x878A, Enemies.RoomEnemyPopulationDefinitions.MainHall, Enemies.RoomEnemyGraphicsSetDefinitions.MainHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.MainHall, 0x0000, RoomMainCallback.None, 0x8E12, 0xBF17, RoomSetupCallback.SharedReturnD),
        0xB295 =>
            new(0xB295, 0xC8E09D, 0x09, 0x24, 0x03, 0x879A, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GoldenTorizo, 0x0000, RoomMainCallback.None, 0x8E3E, 0x0000, RoomSetupCallback.SharedReturnD),
        0xB2AF =>
            new(0xB2AF, 0xC8E09D, 0x09, 0x00, 0x03, 0x879A, Enemies.RoomEnemyPopulationDefinitions.GoldenTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.GoldenTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GoldenTorizo, 0x0000, RoomMainCallback.None, 0x8E3E, 0x0000, RoomSetupCallback.SharedReturnD),
        0xB2E7 =>
            new(0xB2E7, 0xC8E900, 0x09, 0x18, 0x05, 0x87AA, Enemies.RoomEnemyPopulationDefinitions.FastRipper, Enemies.RoomEnemyGraphicsSetDefinitions.FastRipper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8E82, 0xBF17, RoomSetupCallback.SharedReturnD),
        0xB312 =>
            new(0xB312, 0xCE98DC, 0x17, 0x00, 0x03, 0x87BA, Enemies.RoomEnemyPopulationDefinitions.GTEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.GTEnergyRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8E90, 0x0000, RoomSetupCallback.SharedReturnD),
        0xB340 =>
            new(0xB340, 0xC8EBFD, 0x09, 0x24, 0x04, 0x87BC, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.Ridley, 0x0000, RoomMainCallback.RidleyRoomShaking, 0x8E98, 0xBF32, RoomSetupCallback.SharedReturnD),
        0xB35A =>
            new(0xB35A, 0xC8EBFD, 0x09, 0x00, 0x03, 0x87BC, Enemies.RoomEnemyPopulationDefinitions.Ridley, Enemies.RoomEnemyGraphicsSetDefinitions.Ridley, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.Ridley, 0x0000, RoomMainCallback.RidleyRoomShaking, 0x8E98, 0xBF32, RoomSetupCallback.SharedReturnD),
        0xB387 =>
            new(0xB387, 0xC8EDCE, 0x09, 0x18, 0x05, 0x87CC, Enemies.RoomEnemyPopulationDefinitions.LNFarming, Enemies.RoomEnemyGraphicsSetDefinitions.LNFarming, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8EA6, 0xBF4D, RoomSetupCallback.SharedReturnD),
        0xB3B2 =>
            new(0xB3B2, 0xC8F01F, 0x09, 0x00, 0x00, 0x87DC, Enemies.RoomEnemyPopulationDefinitions.FastPillarsSetup, Enemies.RoomEnemyGraphicsSetDefinitions.FastPillarsSetup, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.FastPillarsSetup, 0x0000, RoomMainCallback.None, 0x8EBA, 0xBEAB, RoomSetupCallback.SharedReturnD),
        0xB3EE =>
            new(0xB3EE, 0xC8F40B, 0x09, 0x00, 0x00, 0x87EC, Enemies.RoomEnemyPopulationDefinitions.UnusedRoom8FB3EEA1AD06, Enemies.RoomEnemyGraphicsSetDefinitions.UnusedState8FB3EEB48899, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8ED4, 0xBE90, RoomSetupCallback.SharedReturnD),
        0xB417 =>
            new(0xB417, 0xC8F58B, 0x09, 0x00, 0x00, 0x87FC, Enemies.RoomEnemyPopulationDefinitions.MickeyMouse, Enemies.RoomEnemyGraphicsSetDefinitions.MickeyMouse, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MickeyMouse, 0x0000, RoomMainCallback.None, 0x8ED6, 0xBF68, RoomSetupCallback.SharedReturnD),
        0xB464 =>
            new(0xB464, 0xC8FCC5, 0x09, 0x00, 0x00, 0x880C, Enemies.RoomEnemyPopulationDefinitions.Pillar, Enemies.RoomEnemyGraphicsSetDefinitions.Pillar, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8F38, 0xBF17, RoomSetupCallback.SharedReturnD),
        0xB48F =>
            new(0xB48F, 0xC98222, 0x09, 0x00, 0x00, 0x881C, Enemies.RoomEnemyPopulationDefinitions.Plowerhouse, Enemies.RoomEnemyGraphicsSetDefinitions.Plowerhouse, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8F3A, 0xBF4D, RoomSetupCallback.SharedReturnD),
        0xB4BA =>
            new(0xB4BA, 0xC984D3, 0x09, 0x00, 0x00, 0x882C, Enemies.RoomEnemyPopulationDefinitions.WorstRoomInTheGame, Enemies.RoomEnemyGraphicsSetDefinitions.WorstRoomInTheGame, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WorstRoomInTheGame, 0x0000, RoomMainCallback.None, 0x8F3C, 0xBEFC, RoomSetupCallback.SharedReturnD),
        0xB4F2 =>
            new(0xB4F2, 0xC9899F, 0x09, 0x00, 0x00, 0x883C, Enemies.RoomEnemyPopulationDefinitions.Amphitheatre, Enemies.RoomEnemyGraphicsSetDefinitions.Amphitheatre, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8F7A, 0xBE5A, RoomSetupCallback.SharedReturnD),
        0xB51D =>
            new(0xB51D, 0xC994BA, 0x09, 0x00, 0x05, 0x885C, Enemies.RoomEnemyPopulationDefinitions.LNSpringBallMaze, Enemies.RoomEnemyGraphicsSetDefinitions.LNSpringBallMaze, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.LNSpringBallMaze, 0x0000, RoomMainCallback.None, 0x8F7C, 0xBF68, RoomSetupCallback.SharedReturnD),
        0xB567 =>
            new(0xB567, 0xC99CE2, 0x09, 0x00, 0x00, 0x886C, Enemies.RoomEnemyPopulationDefinitions.LNEscapePowerBombs, Enemies.RoomEnemyGraphicsSetDefinitions.LNEscapePowerBombs, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x8FD2, 0xBF32, RoomSetupCallback.SharedReturnD),
        0xB592 =>
            new(0xB592, 0xC99E7B, 0x09, 0x00, 0x00, 0x887C, Enemies.RoomEnemyPopulationDefinitions.RedKihunterShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RedKihunterShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RedKihunterShaft, 0x0000, RoomMainCallback.None, 0x8FDA, 0xBE5A, RoomSetupCallback.SharedReturnD),
        0xB5E2 =>
            new(0xB5E2, 0xC9A88C, 0x09, 0x00, 0x00, 0x888C, Enemies.RoomEnemyPopulationDefinitions.Wasteland, Enemies.RoomEnemyGraphicsSetDefinitions.Wasteland, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Wasteland, 0x0000, RoomMainCallback.None, 0x9036, 0xBF32, RoomSetupCallback.SharedReturnD),
        0xB638 =>
            new(0xB638, 0xC9B1C7, 0x09, 0x00, 0x00, 0x889C, Enemies.RoomEnemyPopulationDefinitions.MetalPirates, Enemies.RoomEnemyGraphicsSetDefinitions.MetalPirates, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x90C8, 0xBF4D, RoomSetupCallback.SharedReturnD),
        0xB663 =>
            new(0xB663, 0xC9B4AB, 0x09, 0x18, 0x05, 0x88AC, Enemies.RoomEnemyPopulationDefinitions.ThreeMusketeers, Enemies.RoomEnemyGraphicsSetDefinitions.ThreeMusketeers, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ThreeMusketeers, 0x0000, RoomMainCallback.None, 0x90D0, 0xBF68, RoomSetupCallback.SharedReturnD),
        0xB6A5 =>
            new(0xB6A5, 0xC9C30D, 0x09, 0x00, 0x00, 0x88BC, Enemies.RoomEnemyPopulationDefinitions.RidleyETank, Enemies.RoomEnemyGraphicsSetDefinitions.RidleyETank, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x9108, 0xBF32, RoomSetupCallback.SharedReturnD),
        0xB6CE =>
            new(0xB6CE, 0xC9C428, 0x09, 0x00, 0x03, 0x88CC, Enemies.RoomEnemyPopulationDefinitions.ScrewAttack, Enemies.RoomEnemyGraphicsSetDefinitions.ScrewAttack, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x9110, 0xBF32, RoomSetupCallback.SharedReturnD),
        0xB6FB =>
            new(0xB6FB, 0xC9C706, 0x0A, 0x18, 0x05, 0x88DC, Enemies.RoomEnemyPopulationDefinitions.LNFireflea, Enemies.RoomEnemyGraphicsSetDefinitions.LNFireflea, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.LNFireflea, 0x0000, RoomMainCallback.None, 0x9118, 0x0000, RoomSetupCallback.None),
        0xB74E =>
            new(0xB74E, 0xCE95C2, 0x19, 0x18, 0x05, 0x88EC, Enemies.RoomEnemyPopulationDefinitions.LNSave, Enemies.RoomEnemyGraphicsSetDefinitions.LNSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0x918C, 0x0000, RoomSetupCallback.None),
        0xC9A0 =>
            new(0xC9A0, 0xC48322, 0x05, 0x30, 0x03, 0x9BA4, Enemies.RoomEnemyPopulationDefinitions.BowlingAlley0, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlley0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BowlingAlley, 0x0000, RoomMainCallback.None, 0xC215, 0x0000, RoomSetupCallback.OrdinaryReturn),
        0xC9BA =>
            new(0xC9BA, 0xC48322, 0x04, 0x30, 0x03, 0x9AC2, Enemies.RoomEnemyPopulationDefinitions.BowlingAlley1, Enemies.RoomEnemyGraphicsSetDefinitions.BowlingAlley1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BowlingAlley, 0x0000, RoomMainCallback.None, 0xC2D1, 0x0000, RoomSetupCallback.OrdinaryReturn),
        0xCA1A =>
            new(0xCA1A, 0xC49D2E, 0x05, 0x30, 0x05, 0x9BB4, Enemies.RoomEnemyPopulationDefinitions.WreckedShipEntrance0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipEntrance0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC22F, 0xE117, RoomSetupCallback.OrdinaryReturn),
        0xCA34 =>
            new(0xCA34, 0xC49D2E, 0x04, 0x30, 0x06, 0x9AD2, Enemies.RoomEnemyPopulationDefinitions.WreckedShipEntrance1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipEntrance1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC2FD, 0xE117, RoomSetupCallback.OrdinaryReturn),
        0xCA64 =>
            new(0xCA64, 0xC49EAE, 0x05, 0x30, 0x05, 0x9BC4, Enemies.RoomEnemyPopulationDefinitions.Attic0, Enemies.RoomEnemyGraphicsSetDefinitions.Attic0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xFF), Scrolling.RoomScrollDefinitions.Attic, 0x0000, RoomMainCallback.None, 0xC231, 0xE168, RoomSetupCallback.OrdinaryReturn),
        0xCA7E =>
            new(0xCA7E, 0xC4A2E7, 0x04, 0x30, 0x06, 0x9AE2, Enemies.RoomEnemyPopulationDefinitions.Attic1, Enemies.RoomEnemyGraphicsSetDefinitions.Attic1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xFF), Scrolling.RoomScrollDefinitions.Attic, 0x0000, RoomMainCallback.None, 0xC2FF, 0xE168, RoomSetupCallback.OrdinaryReturn),
        0xCAC0 =>
            new(0xCAC0, 0xC4A720, 0x05, 0x00, 0x00, 0x9BD4, Enemies.RoomEnemyPopulationDefinitions.AssemblyLine0, Enemies.RoomEnemyGraphicsSetDefinitions.AssemblyLine0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC245, 0xE1B9, RoomSetupCallback.OrdinaryReturn),
        0xCADA =>
            new(0xCADA, 0xC4A720, 0x04, 0x00, 0x00, 0x9AF2, Enemies.RoomEnemyPopulationDefinitions.AssemblyLine1, Enemies.RoomEnemyGraphicsSetDefinitions.AssemblyLine1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC319, 0xE1B9, RoomSetupCallback.OrdinaryReturn),
        0xCB08 =>
            new(0xCB08, 0xC4A9AC, 0x05, 0x30, 0x05, 0x9BE4, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMainShaft0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaft0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, 0x0000, RoomMainCallback.None, 0xC247, 0xE19E, RoomSetupCallback.OrdinaryReturn),
        0xCB22 =>
            new(0xCB22, 0xC4BDC0, 0x04, 0x30, 0x06, 0x9B02, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMainShaft1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMainShaft1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WreckedShipMainShaft, 0x0000, RoomMainCallback.None, 0xC247, 0xE19E, RoomSetupCallback.OrdinaryReturn),
        0xCB9D =>
            new(0xCB9D, 0xC4D187, 0x05, 0x00, 0x00, 0x9BF4, Enemies.RoomEnemyPopulationDefinitions.SpikyDeath0, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyDeath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC27F, 0xE14D, RoomSetupCallback.OrdinaryReturn),
        0xCBB7 =>
            new(0xCBB7, 0xC4D187, 0x04, 0x00, 0x00, 0x9B12, Enemies.RoomEnemyPopulationDefinitions.SpikyDeath1, Enemies.RoomEnemyGraphicsSetDefinitions.SpikyDeath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC321, 0xE14D, RoomSetupCallback.OrdinaryReturn),
        0xCBE7 =>
            new(0xCBE7, 0xC4D3EE, 0x05, 0x30, 0x05, 0x9C04, Enemies.RoomEnemyPopulationDefinitions.ElectricDeath0, Enemies.RoomEnemyGraphicsSetDefinitions.ElectricDeath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ElectricDeath, 0x0000, RoomMainCallback.None, 0xC281, 0xE19E, RoomSetupCallback.OrdinaryReturn),
        0xCC01 =>
            new(0xCC01, 0xC4D3EE, 0x04, 0x30, 0x06, 0x9B22, Enemies.RoomEnemyPopulationDefinitions.ElectricDeath1, Enemies.RoomEnemyGraphicsSetDefinitions.ElectricDeath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.ElectricDeath, 0x0000, RoomMainCallback.None, 0xC323, 0xE19E, RoomSetupCallback.OrdinaryReturn),
        0xCC39 =>
            new(0xCC39, 0xC4D883, 0x05, 0x00, 0x03, 0x9C14, Enemies.RoomEnemyPopulationDefinitions.WreckedShipETank0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipETank0, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC28F, 0x0000, RoomSetupCallback.OrdinaryReturn),
        0xCC53 =>
            new(0xCC53, 0xC4D883, 0x04, 0x00, 0x03, 0x9B32, Enemies.RoomEnemyPopulationDefinitions.WreckedShipETank1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipETank1, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC337, 0x0000, RoomSetupCallback.OrdinaryReturn),
        0xCC81 =>
            new(0xCC81, 0xC4E14E, 0x05, 0x30, 0x05, 0x9C24, Enemies.RoomEnemyPopulationDefinitions.Basement0, Enemies.RoomEnemyGraphicsSetDefinitions.Basement0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Basement, 0x0000, RoomMainCallback.None, 0xC291, 0xE14D, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile),
        0xCC9B =>
            new(0xCC9B, 0xC4E14E, 0x04, 0x30, 0x06, 0x9B42, Enemies.RoomEnemyPopulationDefinitions.Basement1, Enemies.RoomEnemyGraphicsSetDefinitions.Basement1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.Basement, 0x0000, RoomMainCallback.None, 0xC33F, 0xE14D, RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile),
        0xCCDD =>
            new(0xCCDD, 0xCE83C3, 0x16, 0x00, 0x00, 0x9C34, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMap0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMap0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC2B1, 0x0000, RoomSetupCallback.BossRoomReturn),
        0xCCF7 =>
            new(0xCCF7, 0xCE83C3, 0x15, 0x00, 0x00, 0x9B52, Enemies.RoomEnemyPopulationDefinitions.WreckedShipMap1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipMap1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC34D, 0x0000, RoomSetupCallback.BossRoomReturn),
        0xCD25 =>
            new(0xCD25, 0xC4E58C, 0x05, 0x27, 0x06, 0x9C44, Enemies.RoomEnemyPopulationDefinitions.Phantoon0, Enemies.RoomEnemyGraphicsSetDefinitions.Phantoon0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Phantoon, 0x0000, RoomMainCallback.None, 0xC2B3, 0xE0FD, RoomSetupCallback.BossRoomReturn),
        0xCD3F =>
            new(0xCD3F, 0xC4E58C, 0x04, 0x00, 0x03, 0x9B62, Enemies.RoomEnemyPopulationDefinitions.Phantoon1, Enemies.RoomEnemyGraphicsSetDefinitions.Phantoon1, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Phantoon, 0x0000, RoomMainCallback.None, 0xC2B3, 0xE113, RoomSetupCallback.BossRoomReturn),
        0xCD6E =>
            new(0xCD6E, 0xC4E6A5, 0x05, 0x00, 0x00, 0x9C54, Enemies.RoomEnemyPopulationDefinitions.SpongeBath0, Enemies.RoomEnemyGraphicsSetDefinitions.SpongeBath0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SpongeBath, 0x0000, RoomMainCallback.None, 0xC2BB, 0xE183, RoomSetupCallback.BossRoomReturn),
        0xCD88 =>
            new(0xCD88, 0xC4E6A5, 0x04, 0x00, 0x00, 0x9B64, Enemies.RoomEnemyPopulationDefinitions.SpongeBath1, Enemies.RoomEnemyGraphicsSetDefinitions.SpongeBath1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.SpongeBath, 0x0000, RoomMainCallback.None, 0xC355, 0xE183, RoomSetupCallback.BossRoomReturn),
        0xCDBA =>
            new(0xCDBA, 0xC4E94E, 0x05, 0x00, 0x00, 0x9C64, Enemies.RoomEnemyPopulationDefinitions.WSWestSuper0, Enemies.RoomEnemyGraphicsSetDefinitions.WSWestSuper0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSWestSuper, 0x0000, RoomMainCallback.None, 0xC2BD, 0xE183, RoomSetupCallback.BossRoomReturn),
        0xCDD4 =>
            new(0xCDD4, 0xC4E94E, 0x04, 0x00, 0x00, 0x9B74, Enemies.RoomEnemyPopulationDefinitions.WSWestSuper1, Enemies.RoomEnemyGraphicsSetDefinitions.WSWestSuper1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSWestSuper, 0x0000, RoomMainCallback.None, 0xC357, 0xE183, RoomSetupCallback.BossRoomReturn),
        0xCE03 =>
            new(0xCE03, 0xC4EA8F, 0x05, 0x00, 0x00, 0x9C74, Enemies.RoomEnemyPopulationDefinitions.WSEastSuper0, Enemies.RoomEnemyGraphicsSetDefinitions.WSEastSuper0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSEastSuper, 0x0000, RoomMainCallback.None, 0xC2BF, 0xE19E, RoomSetupCallback.BossRoomReturn),
        0xCE1D =>
            new(0xCE1D, 0xC4ED98, 0x04, 0x00, 0x00, 0x9B84, Enemies.RoomEnemyPopulationDefinitions.WSEastSuper1, Enemies.RoomEnemyGraphicsSetDefinitions.WSEastSuper1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.WSEastSuper, 0x0000, RoomMainCallback.None, 0xC35F, 0xE183, RoomSetupCallback.BossRoomReturn),
        0xCE52 =>
            new(0xCE52, 0xC4F0A1, 0x05, 0x00, 0x00, 0x9C74, Enemies.RoomEnemyPopulationDefinitions.GravitySuit0, Enemies.RoomEnemyGraphicsSetDefinitions.GravitySuit0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC2C7, 0xE183, RoomSetupCallback.BossRoomReturn),
        0xCE6C =>
            new(0xCE6C, 0xC4F0A1, 0x04, 0x00, 0x00, 0x9B94, Enemies.RoomEnemyPopulationDefinitions.GravitySuit1, Enemies.RoomEnemyGraphicsSetDefinitions.GravitySuit1, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC36D, 0xE183, RoomSetupCallback.BossRoomReturn),
        0xCE9C =>
            new(0xCE9C, 0xCE95C2, 0x16, 0x30, 0x05, 0x9C74, Enemies.RoomEnemyPopulationDefinitions.WreckedShipSave0, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipSave0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC2C7, 0x0000, RoomSetupCallback.None),
        0xCEB6 =>
            new(0xCEB6, 0xCE95C2, 0x15, 0x30, 0x06, 0x9B94, Enemies.RoomEnemyPopulationDefinitions.WreckedShipSave1, Enemies.RoomEnemyGraphicsSetDefinitions.WreckedShipSave1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC2C9, 0x0000, RoomSetupCallback.BossRoomReturn),
        0xCEDF =>
            new(0xCEDF, 0xCE95C2, 0x19, 0x12, 0x05, 0x9C84, Enemies.RoomEnemyPopulationDefinitions.GlassTunnelSave, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnelSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC375, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xCF0D =>
            new(0xCF0D, 0xC9DB52, 0x0B, 0x12, 0x05, 0x9C94, Enemies.RoomEnemyPopulationDefinitions.GlassTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnel, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.GlassTunnel, 0x0000, RoomMainCallback.None, 0xC37D, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xCF27 =>
            new(0xCF27, 0xC9E129, 0x0B, 0x12, 0x05, 0x9C94, Enemies.RoomEnemyPopulationDefinitions.GlassTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.GlassTunnel, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.GlassTunnel, 0x0000, RoomMainCallback.None, 0xC37D, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xCF61 =>
            new(0xCF61, 0xC9E6AE, 0x0B, 0x00, 0x00, 0x9CA4, Enemies.RoomEnemyPopulationDefinitions.WestTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.WestTunnel, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.WestTunnel, 0x0000, RoomMainCallback.None, 0xC3DF, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xCF8D =>
            new(0xCF8D, 0xC9E809, 0x0B, 0x12, 0x05, 0x9CB4, Enemies.RoomEnemyPopulationDefinitions.EastTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.EastTunnel, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.EastTunnel, 0x0000, RoomMainCallback.None, 0xC3E1, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xCFD6 =>
            new(0xCFD6, 0xC9F225, 0x0B, 0x1B, 0x06, 0x9CC4, Enemies.RoomEnemyPopulationDefinitions.MainStreet, Enemies.RoomEnemyGraphicsSetDefinitions.MainStreet, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.MainStreet, 0x0000, RoomMainCallback.None, 0xC42B, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD024 =>
            new(0xD024, 0xCA8EFF, 0x0B, 0x00, 0x00, 0x9CD4, Enemies.RoomEnemyPopulationDefinitions.FishTank, Enemies.RoomEnemyGraphicsSetDefinitions.FishTank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.FishTank, 0x0000, RoomMainCallback.None, 0xC445, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD062 =>
            new(0xD062, 0xCAA113, 0x0B, 0x00, 0x00, 0x9CE4, Enemies.RoomEnemyPopulationDefinitions.MamaTurtle, Enemies.RoomEnemyGraphicsSetDefinitions.MamaTurtle, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.MamaTurtle, 0x0000, RoomMainCallback.None, 0xC47D, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD097 =>
            new(0xD097, 0xCAAF99, 0x0B, 0x1B, 0x06, 0x9CF4, Enemies.RoomEnemyPopulationDefinitions.CrabTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.CrabTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.CrabTunnel, 0x0000, RoomMainCallback.None, 0xC48B, 0xE248, RoomSetupCallback.BossRoomReturnB),
        0xD0C6 =>
            new(0xD0C6, 0xCAB24F, 0x0B, 0x00, 0x00, 0x9D04, Enemies.RoomEnemyPopulationDefinitions.MtEverest, Enemies.RoomEnemyGraphicsSetDefinitions.MtEverest, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0xC0), Scrolling.RoomScrollDefinitions.MtEverest, 0x0000, RoomMainCallback.None, 0xC499, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD111 =>
            new(0xD111, 0xCACE42, 0x0B, 0x1B, 0x06, 0x9D14, Enemies.RoomEnemyPopulationDefinitions.RedFish, Enemies.RoomEnemyGraphicsSetDefinitions.RedFish, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.RedFish, 0x0000, RoomMainCallback.None, 0xC49B, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD148 =>
            new(0xD148, 0xCAD474, 0x0B, 0x00, 0x00, 0x9D24, Enemies.RoomEnemyPopulationDefinitions.WateringHole, Enemies.RoomEnemyGraphicsSetDefinitions.WateringHole, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WateringHole, 0x0000, RoomMainCallback.None, 0xC4A9, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD17A =>
            new(0xD17A, 0xCADBC8, 0x0B, 0x00, 0x00, 0x9D34, Enemies.RoomEnemyPopulationDefinitions.NWestMaridiaBug, Enemies.RoomEnemyGraphicsSetDefinitions.NWestMaridiaBug, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.NWestMaridiaBug, 0x0000, RoomMainCallback.None, 0xC4BD, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD1B0 =>
            new(0xD1B0, 0xCAE458, 0x0B, 0x1B, 0x06, 0x9D44, Enemies.RoomEnemyPopulationDefinitions.CrabShaft, Enemies.RoomEnemyGraphicsSetDefinitions.CrabShaft, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.CrabShaft, 0x0000, RoomMainCallback.None, 0xC4EF, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD1EA =>
            new(0xD1EA, 0xCAF59C, 0x0B, 0x00, 0x00, 0x9D54, Enemies.RoomEnemyPopulationDefinitions.PseudoPlasmaSpark, Enemies.RoomEnemyGraphicsSetDefinitions.PseudoPlasmaSpark, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.PseudoPlasmaSpark, 0x0000, RoomMainCallback.None, 0xC503, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD229 =>
            new(0xD229, 0xCB83DB, 0x0B, 0x1B, 0x05, 0x9D64, Enemies.RoomEnemyPopulationDefinitions.CrabHole, Enemies.RoomEnemyGraphicsSetDefinitions.CrabHole, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.CrabHole, 0x0000, RoomMainCallback.None, 0xC53B, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD25F =>
            new(0xD25F, 0xCB883A, 0x0B, 0x00, 0x00, 0x9D74, Enemies.RoomEnemyPopulationDefinitions.WestSandHallTunnel, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHallTunnel, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.WestSandHallTunnel, 0x0000, RoomMainCallback.None, 0xC54F, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD28B =>
            new(0xD28B, 0xCB89E0, 0x0B, 0x00, 0x00, 0x9D84, Enemies.RoomEnemyPopulationDefinitions.PlasmaTutorial, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaTutorial, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.PlasmaTutorial, 0x0000, RoomMainCallback.None, 0xC551, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD2B7 =>
            new(0xD2B7, 0xCB8BD4, 0x0B, 0x00, 0x00, 0x9D94, Enemies.RoomEnemyPopulationDefinitions.Plasma, Enemies.RoomEnemyGraphicsSetDefinitions.Plasma, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Plasma, 0x0000, RoomMainCallback.None, 0xC553, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD2E6 =>
            new(0xD2E6, 0xCB9792, 0x0B, 0x00, 0x00, 0x9DA4, Enemies.RoomEnemyPopulationDefinitions.ThreadTheNeedle, Enemies.RoomEnemyGraphicsSetDefinitions.ThreadTheNeedle, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.ThreadTheNeedle, 0x0000, RoomMainCallback.None, 0xC561, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD318 =>
            new(0xD318, 0xCBA0D0, 0x0B, 0x1B, 0x05, 0x9DB4, Enemies.RoomEnemyPopulationDefinitions.MaridiaElev, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaElev, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.MaridiaElev, 0x0000, RoomMainCallback.None, 0xC563, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD34D =>
            new(0xD34D, 0xCBA878, 0x0B, 0x00, 0x05, 0x9DC4, Enemies.RoomEnemyPopulationDefinitions.PlasmaSpark, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaSpark, Scrolling.Layer2ScrollModes.FromHeader(0xE0, 0x00), Scrolling.RoomScrollDefinitions.PlasmaSpark, 0x0000, RoomMainCallback.None, 0xC571, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD394 =>
            new(0xD394, 0xCBC64F, 0x0B, 0x00, 0x05, 0x9DD4, Enemies.RoomEnemyPopulationDefinitions.Kassiuz, Enemies.RoomEnemyGraphicsSetDefinitions.Kassiuz, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.Kassiuz, 0x0000, RoomMainCallback.None, 0xC57F, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD3C3 =>
            new(0xD3C3, 0xCE86BD, 0x15, 0x00, 0x00, 0x9DE4, Enemies.RoomEnemyPopulationDefinitions.MaridiaMap, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaMap, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC581, 0x0000, RoomSetupCallback.BossRoomReturnB),
        0xD3EC =>
            new(0xD3EC, 0xCE95C2, 0x19, 0x1B, 0x05, 0x9DF4, Enemies.RoomEnemyPopulationDefinitions.ForgottenHighwaySave, Enemies.RoomEnemyGraphicsSetDefinitions.ForgottenHighwaySave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC589, 0x0000, RoomSetupCallback.None),
        0xD415 =>
            new(0xD415, 0xCBCD9F, 0x0C, 0x00, 0x00, 0x9E04, Enemies.RoomEnemyPopulationDefinitions.Toilet, Enemies.RoomEnemyGraphicsSetDefinitions.Toilet, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AllGreen, 0x0000, RoomMainCallback.MaridiaElevatube, 0xC591, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD440 =>
            new(0xD440, 0xCBDCF3, 0x0C, 0x00, 0x00, 0x9E34, Enemies.RoomEnemyPopulationDefinitions.BugSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.BugSandHole, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BugSandHole, 0x0000, RoomMainCallback.None, 0xC593, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD46E =>
            new(0xD46E, 0xCBDE8F, 0x0C, 0x00, 0x00, 0x9E44, Enemies.RoomEnemyPopulationDefinitions.WestSandHall, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC595, 0xE25A, RoomSetupCallback.BossRoomReturnC),
        0xD49B =>
            new(0xD49B, 0xCBE472, 0x0C, 0x00, 0x00, 0x9E54, Enemies.RoomEnemyPopulationDefinitions.Oasis, Enemies.RoomEnemyGraphicsSetDefinitions.Oasis, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.Oasis, 0x0000, RoomMainCallback.None, 0xC597, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD4CF =>
            new(0xD4CF, 0xCBE899, 0x0C, 0x00, 0x00, 0x9E64, Enemies.RoomEnemyPopulationDefinitions.EastSandHall, Enemies.RoomEnemyGraphicsSetDefinitions.EastSandHall, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC5DB, 0xE25A, RoomSetupCallback.BossRoomReturnC),
        0xD4FC =>
            new(0xD4FC, 0xCBEC32, 0x0C, 0x00, 0x00, 0x9E74, Enemies.RoomEnemyPopulationDefinitions.WestSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.WestSandHole, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.WestSandHole, 0x0000, RoomMainCallback.None, 0xC5DD, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD52B =>
            new(0xD52B, 0xCBF580, 0x0C, 0x00, 0x00, 0x9E84, Enemies.RoomEnemyPopulationDefinitions.EastSandHole, Enemies.RoomEnemyGraphicsSetDefinitions.EastSandHole, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.EastSandHole, 0x0000, RoomMainCallback.None, 0xC5EB, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD55A =>
            new(0xD55A, 0xCBFEC8, 0x0C, 0x00, 0x00, 0x9E94, Enemies.RoomEnemyPopulationDefinitions.WestAqueductQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.WestAqueductQuicksand, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.WestAqueductQuicksand, 0x0000, RoomMainCallback.None, 0xC5F9, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD587 =>
            new(0xD587, 0xCC80B8, 0x0C, 0x00, 0x00, 0x9E94, Enemies.RoomEnemyPopulationDefinitions.EastAqueductQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.EastAqueductQuicksand, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.EastAqueductQuicksand, 0x0000, RoomMainCallback.None, 0xC5FB, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD5B4 =>
            new(0xD5B4, 0xCC82A8, 0x0C, 0x1B, 0x05, 0x9EA4, Enemies.RoomEnemyPopulationDefinitions.Aqueduct, Enemies.RoomEnemyGraphicsSetDefinitions.Aqueduct, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Aqueduct, 0x0000, RoomMainCallback.None, 0xC5FD, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD5F9 =>
            new(0xD5F9, 0xCCA13B, 0x0C, 0x00, 0x00, 0x9EB4, Enemies.RoomEnemyPopulationDefinitions.Butterfly, Enemies.RoomEnemyGraphicsSetDefinitions.Butterfly, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC611, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD624 =>
            new(0xD624, 0xCCA34A, 0x0C, 0x1B, 0x05, 0x9EC4, Enemies.RoomEnemyPopulationDefinitions.BotwoonHallway, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonHallway, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.BotwoonHallway, 0x0000, RoomMainCallback.None, 0xC619, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD653 =>
            new(0xD653, 0xCCAC48, 0x0C, 0x00, 0x00, 0x9ED4, Enemies.RoomEnemyPopulationDefinitions.Pants, Enemies.RoomEnemyGraphicsSetDefinitions.Pants, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.Pants, 0x0000, RoomMainCallback.None, 0xC61B, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD6A7 =>
            new(0xD6A7, 0xCCB843, 0x0C, 0x00, 0x00, 0x9EE4, Enemies.RoomEnemyPopulationDefinitions.EastPants, Enemies.RoomEnemyGraphicsSetDefinitions.EastPants, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.EastPants, 0x0000, RoomMainCallback.None, 0xC6AD, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD6DD =>
            new(0xD6DD, 0xCCBD31, 0x0C, 0x00, 0x00, 0x9EF4, Enemies.RoomEnemyPopulationDefinitions.Springball, Enemies.RoomEnemyGraphicsSetDefinitions.Springball, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Springball, 0x0000, RoomMainCallback.None, 0xC6E5, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD70A =>
            new(0xD70A, 0xCCC22F, 0x0C, 0x00, 0x00, 0x9F04, Enemies.RoomEnemyPopulationDefinitions.BelowBotwoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.BelowBotwoonETank, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BelowBotwoonETank, 0x0000, RoomMainCallback.None, 0xC6ED, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD737 =>
            new(0xD737, 0xCCC9F1, 0x0C, 0x00, 0x00, 0x9F14, Enemies.RoomEnemyPopulationDefinitions.Colosseum, Enemies.RoomEnemyGraphicsSetDefinitions.Colosseum, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Colosseum, 0x0000, RoomMainCallback.None, 0xC6EF, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD772 =>
            new(0xD772, 0xCE92CB, 0x19, 0x1B, 0x05, 0x9F24, Enemies.RoomEnemyPopulationDefinitions.AqueductSave, Enemies.RoomEnemyGraphicsSetDefinitions.AqueductSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.AqueductSave, 0x0000, RoomMainCallback.None, 0xC703, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD7A1 =>
            new(0xD7A1, 0xCCE5B1, 0x0C, 0x00, 0x00, 0x9F34, Enemies.RoomEnemyPopulationDefinitions.ThePrecious, Enemies.RoomEnemyGraphicsSetDefinitions.ThePrecious, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.ThePrecious, 0x0000, RoomMainCallback.None, 0xC70B, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD7BB =>
            new(0xD7BB, 0xCCE5B1, 0x0C, 0x1B, 0x05, 0x9F34, Enemies.RoomEnemyPopulationDefinitions.ThePrecious, Enemies.RoomEnemyGraphicsSetDefinitions.ThePrecious, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.ThePrecious, 0x0000, RoomMainCallback.None, 0xC70B, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD7F1 =>
            new(0xD7F1, 0xCCEE0C, 0x0C, 0x1B, 0x05, 0x9F44, Enemies.RoomEnemyPopulationDefinitions.BotwoonETank, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonETank, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.BotwoonETank, 0x0000, RoomMainCallback.None, 0xC755, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD827 =>
            new(0xD827, 0xCE9EF6, 0x19, 0x1B, 0x05, 0x9F54, Enemies.RoomEnemyPopulationDefinitions.DraygonSave, Enemies.RoomEnemyGraphicsSetDefinitions.DraygonSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC75D, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD852 =>
            new(0xD852, 0xCEAB31, 0x17, 0x00, 0x00, 0x9F64, Enemies.RoomEnemyPopulationDefinitions.MaridiaMissileRefill, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaMissileRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.None, 0xC765, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD87B =>
            new(0xD87B, 0xCCFA8D, 0x0C, 0x00, 0x00, 0x9F74, Enemies.RoomEnemyPopulationDefinitions.PlasmaBeachQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.PlasmaBeachQuicksand, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.PlasmaBeachQuicksand, 0x0000, RoomMainCallback.None, 0xC76D, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD8A5 =>
            new(0xD8A5, 0xCCFB88, 0x0C, 0x00, 0x00, 0x9F74, Enemies.RoomEnemyPopulationDefinitions.BotwoonQuicksand, Enemies.RoomEnemyGraphicsSetDefinitions.BotwoonQuicksand, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.BotwoonQuicksand, 0x0000, RoomMainCallback.None, 0xC76F, 0x0000, RoomSetupCallback.BossRoomReturnC),
        0xD8D7 =>
            new(0xD8D7, 0xCCFD75, 0x0C, 0x00, 0x00, 0x9F84, Enemies.RoomEnemyPopulationDefinitions.Shaktool, Enemies.RoomEnemyGraphicsSetDefinitions.Shaktool, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Shaktool, 0x0000, RoomMainCallback.None, 0xC771, 0x0000, RoomSetupCallback.SetupShaktoolRoomPlm),
        0xD8F1 =>
            new(0xD8F1, 0xCD8404, 0x0C, 0x00, 0x00, 0x9F84, Enemies.RoomEnemyPopulationDefinitions.Shaktool, Enemies.RoomEnemyGraphicsSetDefinitions.Shaktool, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.Shaktool, 0x0000, RoomMainCallback.None, 0xC771, 0x0000, RoomSetupCallback.ReturnBeforeDraygonSetup),
        0xD920 =>
            new(0xD920, 0xCD8A37, 0x0C, 0x00, 0x00, 0x9F94, Enemies.RoomEnemyPopulationDefinitions.HalfieClimb, Enemies.RoomEnemyGraphicsSetDefinitions.HalfieClimb, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.HalfieClimb, 0x0000, RoomMainCallback.None, 0xC773, 0x0000, RoomSetupCallback.ReturnBeforeDraygonSetup),
        0xD970 =>
            new(0xD970, 0xCD950E, 0x0C, 0x2A, 0x05, 0x9FA4, Enemies.RoomEnemyPopulationDefinitions.Botwoon, Enemies.RoomEnemyGraphicsSetDefinitions.Botwoon, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Botwoon, 0x0000, RoomMainCallback.None, 0xC79F, 0x0000, RoomSetupCallback.ReturnBeforeDraygonSetup),
        0xD98A =>
            new(0xD98A, 0xCD950E, 0x0C, 0x00, 0x03, 0x9FA4, Enemies.RoomEnemyPopulationDefinitions.Botwoon, Enemies.RoomEnemyGraphicsSetDefinitions.Botwoon, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.Botwoon, 0x0000, RoomMainCallback.None, 0xC79F, 0x0000, RoomSetupCallback.ReturnBeforeDraygonSetup),
        0xD9B7 =>
            new(0xD9B7, 0xCD991E, 0x0C, 0x00, 0x03, 0x9FB4, Enemies.RoomEnemyPopulationDefinitions.SpaceJump, Enemies.RoomEnemyGraphicsSetDefinitions.SpaceJump, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.SpaceJump, 0x0000, RoomMainCallback.None, 0xC7A7, 0x0000, RoomSetupCallback.ReturnBeforeDraygonSetup),
        0xD9E1 =>
            new(0xD9E1, 0xCE98DC, 0x17, 0x00, 0x00, 0x9FC4, Enemies.RoomEnemyPopulationDefinitions.MaridiaEnergyRefill, Enemies.RoomEnemyGraphicsSetDefinitions.MaridiaEnergyRefill, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.MaridiaEnergyRefill, 0x0000, RoomMainCallback.None, 0xC7AF, 0x0000, RoomSetupCallback.None),
        0xDA0B =>
            new(0xDA0B, 0xCD9B28, 0x0C, 0x00, 0x00, 0x9FD4, Enemies.RoomEnemyPopulationDefinitions.WestCactusAlley, Enemies.RoomEnemyGraphicsSetDefinitions.WestCactusAlley, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0xC0), Scrolling.RoomScrollDefinitions.WestCactusAlley, 0x0000, RoomMainCallback.None, 0xC7B7, 0x0000, RoomSetupCallback.None),
        0xDA38 =>
            new(0xDA38, 0xCDA00D, 0x0C, 0x00, 0x00, 0x9FE4, Enemies.RoomEnemyPopulationDefinitions.EastCactusAlley, Enemies.RoomEnemyGraphicsSetDefinitions.EastCactusAlley, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.EastCactusAlley, 0x0000, RoomMainCallback.None, 0xC7B9, 0x0000, RoomSetupCallback.None),
        0xDA72 =>
            new(0xDA72, 0xCDB19D, 0x1C, 0x24, 0x05, 0x9FF4, Enemies.RoomEnemyPopulationDefinitions.Draygon0, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Draygon, 0x0000, RoomMainCallback.None, 0xC7BB, 0xE108, RoomSetupCallback.SetPausingCodeForDraygon),
        0xDA8C =>
            new(0xDA8C, 0xCDB19D, 0x1C, 0x00, 0x03, 0x9FF4, Enemies.RoomEnemyPopulationDefinitions.DraygonState1MotherBrainState2, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x01), Scrolling.RoomScrollDefinitions.Draygon, 0x0000, RoomMainCallback.None, 0xC7BB, 0xE113, RoomSetupCallback.SetPausingCodeForDraygon),
        0xDABB =>
            new(0xDABB, 0xCDC4FE, 0x0D, 0x1E, 0x05, 0xA004, Enemies.RoomEnemyPopulationDefinitions.TourianFirst, Enemies.RoomEnemyGraphicsSetDefinitions.TourianFirst, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.TourianFirst, 0x0000, RoomMainCallback.None, 0xC7E1, 0xE3E8, RoomSetupCallback.SetCollectedMap),
        0xDAF3 =>
            new(0xDAF3, 0xCDC8DC, 0x0D, 0x00, 0x00, 0xA014, Enemies.RoomEnemyPopulationDefinitions.Metroids10, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids10, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids1, 0x0000, RoomMainCallback.None, 0xC7E9, 0xE403, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDB0D =>
            new(0xDB0D, 0xCDC8DC, 0x0D, 0x00, 0x00, 0xA014, Enemies.RoomEnemyPopulationDefinitions.Metroids11, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids11, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids1, 0x0000, RoomMainCallback.None, 0xC7E9, 0xE403, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDB43 =>
            new(0xDB43, 0xCDCDA0, 0x0D, 0x00, 0x00, 0xA024, Enemies.RoomEnemyPopulationDefinitions.Metroids20, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids20, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids2, 0x0000, RoomMainCallback.None, 0xC7F7, 0xE3E8, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDB5D =>
            new(0xDB5D, 0xCDCDA0, 0x0D, 0x00, 0x00, 0xA024, Enemies.RoomEnemyPopulationDefinitions.Metroids21, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids21, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids2, 0x0000, RoomMainCallback.None, 0xC7F7, 0xE3E8, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDB8F =>
            new(0xDB8F, 0xCDD02D, 0x0D, 0x00, 0x00, 0xA034, Enemies.RoomEnemyPopulationDefinitions.Metroids30, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids30, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids3, 0x0000, RoomMainCallback.None, 0xC805, 0xE403, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDBA9 =>
            new(0xDBA9, 0xCDD02D, 0x0D, 0x00, 0x00, 0xA034, Enemies.RoomEnemyPopulationDefinitions.Metroids31, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids31, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids3, 0x0000, RoomMainCallback.None, 0xC805, 0xE403, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDBDF =>
            new(0xDBDF, 0xCDD3E5, 0x0D, 0x1E, 0x05, 0xA044, Enemies.RoomEnemyPopulationDefinitions.Metroids40, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids40, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids4, 0x0000, RoomMainCallback.None, 0xC813, 0xE3E8, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDBF9 =>
            new(0xDBF9, 0xCDD3E5, 0x0D, 0x1E, 0x05, 0xA044, Enemies.RoomEnemyPopulationDefinitions.Metroids41, Enemies.RoomEnemyGraphicsSetDefinitions.Metroids41, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Metroids4, 0x0000, RoomMainCallback.None, 0xC813, 0xE3E8, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDC2B =>
            new(0xDC2B, 0xCDD5EB, 0x0D, 0x45, 0x06, 0xA054, Enemies.RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueHopper, 0x0000, RoomMainCallback.None, 0xC821, 0xE41E, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDC45 =>
            new(0xDC45, 0xCDD5EB, 0x0D, 0x00, 0x00, 0xA054, Enemies.RoomEnemyPopulationDefinitions.BlueHopper, Enemies.RoomEnemyGraphicsSetDefinitions.BlueHopper, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BlueHopper, 0x0000, RoomMainCallback.None, 0xC821, 0xE41E, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDC77 =>
            new(0xDC77, 0xCDD7C4, 0x0D, 0x45, 0x06, 0xA064, Enemies.RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.DustTorizo, 0x0000, RoomMainCallback.None, 0xC823, 0xE41E, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDC91 =>
            new(0xDC91, 0xCDD7C4, 0x0D, 0x1E, 0x05, 0xA064, Enemies.RoomEnemyPopulationDefinitions.DustTorizo, Enemies.RoomEnemyGraphicsSetDefinitions.DustTorizo, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.DustTorizo, 0x0000, RoomMainCallback.None, 0xC823, 0xE41E, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDCC3 =>
            new(0xDCC3, 0xCDD930, 0x0D, 0x00, 0x00, 0xA074, Enemies.RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigBoy, 0x0000, RoomMainCallback.None, 0xC831, 0xE41E, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDCDD =>
            new(0xDCDD, 0xCDD930, 0x0D, 0x00, 0x00, 0xA074, Enemies.RoomEnemyPopulationDefinitions.BigBoy, Enemies.RoomEnemyGraphicsSetDefinitions.BigBoy, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.BigBoy, 0x0000, RoomMainCallback.None, 0xC831, 0xE41E, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDD0C =>
            new(0xDD0C, 0xCDDBF8, 0x0D, 0x1E, 0x05, 0xA084, Enemies.RoomEnemyPopulationDefinitions.Seaweed, Enemies.RoomEnemyGraphicsSetDefinitions.Seaweed, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.Seaweed, 0x0000, RoomMainCallback.None, 0xC839, 0xE41E, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDD3B =>
            new(0xDD3B, 0xCEA823, 0x18, 0x00, 0x00, 0xA094, Enemies.RoomEnemyPopulationDefinitions.TourianRecharge, Enemies.RoomEnemyGraphicsSetDefinitions.TourianRecharge, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.TourianRecharge, 0x0000, RoomMainCallback.None, 0xC841, 0x0000, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDD6E =>
            new(0xDD6E, 0xCDDEDE, 0x0E, 0x00, 0x00, 0xA0A4, Enemies.RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, 0x0000, RoomMainCallback.None, 0xC84F, 0xE48A, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDD88 =>
            new(0xDD88, 0xCDDEDE, 0x0E, 0x00, 0x00, 0xA0A4, Enemies.RoomEnemyPopulationDefinitions.MotherBrain01, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrain01, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, 0x0000, RoomMainCallback.None, 0xC84F, 0xE48A, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDDA2 =>
            new(0xDDA2, 0xCDDEDE, 0x0E, 0x00, 0x03, 0xA188, Enemies.RoomEnemyPopulationDefinitions.DraygonState1MotherBrainState2, Enemies.RoomEnemyGraphicsSetDefinitions.Draygon1MotherBrain2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.MotherBrain, 0x0000, RoomMainCallback.None, 0xC84F, 0xE48A, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDDD1 =>
            new(0xDDD1, 0xCDE20F, 0x0D, 0x00, 0x00, 0xA0D4, Enemies.RoomEnemyPopulationDefinitions.TourianEyeDoor, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEyeDoor, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEyeDoor, 0x0000, RoomMainCallback.None, 0xC857, 0xE41E, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDE00 =>
            new(0xDE00, 0xCDE518, 0x0D, 0x1E, 0x05, 0xA0E4, Enemies.RoomEnemyPopulationDefinitions.RinkaShaft, Enemies.RoomEnemyGraphicsSetDefinitions.RinkaShaft, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.RinkaShaft, 0x0000, RoomMainCallback.None, 0xC86B, 0xE454, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDE30 =>
            new(0xDE30, 0xCE92CB, 0x19, 0x1E, 0x05, 0xA0F4, Enemies.RoomEnemyPopulationDefinitions.MotherBrainSave, Enemies.RoomEnemyGraphicsSetDefinitions.MotherBrainSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.MotherBrainSave, 0x0000, RoomMainCallback.None, 0xC873, 0x0000, RoomSetupCallback.ReturnBeforeZebesTimebombSetup),
        0xDE5A =>
            new(0xDE5A, 0xCDE914, 0x0D, 0x24, 0x07, 0xA104, Enemies.RoomEnemyPopulationDefinitions.TourianEscape1, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape1, 0x0000, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, 0xC87B, 0xE439, RoomSetupCallback.SetZebesTimebombEventAndLightHorizontalShaking),
        0xDE87 =>
            new(0xDE87, 0xCDEB5B, 0x0D, 0x00, 0x00, 0xA114, Enemies.RoomEnemyPopulationDefinitions.TourianEscape2, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape2, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape2, 0x0000, RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal, 0xC889, 0xE454, RoomSetupCallback.SetLightHorizontalRoomShaking),
        0xDEB4 =>
            new(0xDEB4, 0xCDED7A, 0x0D, 0x00, 0x00, 0xA124, Enemies.RoomEnemyPopulationDefinitions.TourianEscape3, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape3, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape3, 0x0000, RoomMainCallback.GenerateRandomExplosionEveryFourthFrame, 0xC897, 0xE46F, RoomSetupCallback.SetMediumHorizontalRoomShaking),
        0xDEEB =>
            new(0xDEEB, 0xCDF534, 0x0D, 0x00, 0x00, 0xA134, Enemies.RoomEnemyPopulationDefinitions.TourianEscape4, Enemies.RoomEnemyGraphicsSetDefinitions.TourianEscape4, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0xC1), Scrolling.RoomScrollDefinitions.TourianEscape4, 0x0000, RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal, 0xC8A5, 0xE46F, RoomSetupCallback.SetupEscapeRoom4PlmAndMediumHorizontalShaking),
        0xDF28 =>
            new(0xDF28, 0xCE95C2, 0x19, 0x1E, 0x05, 0xA144, Enemies.RoomEnemyPopulationDefinitions.UpperTourianSave, Enemies.RoomEnemyGraphicsSetDefinitions.UpperTourianSave, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.UpperTourianSave, 0x0000, RoomMainCallback.None, 0xC8B3, 0x0000, RoomSetupCallback.None),
        0xDF57 =>
            new(0xDF57, 0xCDB846, 0x11, 0x2D, 0x06, 0xA146, Enemies.RoomEnemyPopulationDefinitions.CeresElev0, Enemies.RoomEnemyGraphicsSetDefinitions.CeresElev0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.CeresElevatorShaft, 0xC8BB, 0xE1D4, RoomSetupCallback.SpawnCeresHaze),
        0xDF71 =>
            new(0xDF71, 0xCDB846, 0x12, 0x00, 0x00, 0xA146, Enemies.RoomEnemyPopulationDefinitions.CeresElev1, Enemies.RoomEnemyGraphicsSetDefinitions.CeresElev1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.CeresElevatorShaft, 0xC8BB, 0xE1D4, RoomSetupCallback.TurnCeresDoorToSolidBlocksAndSpawnHaze),
        0xDF9F =>
            new(0xDF9F, 0xCDBBFE, 0x0F, 0x00, 0x00, 0xA156, Enemies.RoomEnemyPopulationDefinitions.FallingTile0, Enemies.RoomEnemyGraphicsSetDefinitions.FallingTile0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, 0xC8BD, 0xE4A5, RoomSetupCallback.SpawnCeresHaze),
        0xDFB9 =>
            new(0xDFB9, 0xCDBBFE, 0x10, 0x00, 0x00, 0xA17E, Enemies.RoomEnemyPopulationDefinitions.FallingTile1, Enemies.RoomEnemyGraphicsSetDefinitions.FallingTile1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.SpawnCeresPreElevatorHallFallingDebris, 0xC8BD, 0xE4A5, RoomSetupCallback.SpawnCeresHaze),
        0xDFE9 =>
            new(0xDFE9, 0xCDBD78, 0x0F, 0x00, 0x00, 0xA158, Enemies.RoomEnemyPopulationDefinitions.MagnetStairs0, Enemies.RoomEnemyGraphicsSetDefinitions.MagnetStairs0, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0x41), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.Return, 0xC8BF, 0xE4A5, RoomSetupCallback.SpawnCeresHaze),
        0xE003 =>
            new(0xE003, 0xCDBD78, 0x10, 0x00, 0x00, 0xA180, Enemies.RoomEnemyPopulationDefinitions.MagnetStairs1, Enemies.RoomEnemyGraphicsSetDefinitions.MagnetStairs1, Scrolling.Layer2ScrollModes.FromHeader(0x01, 0xC1), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.Return, 0xC8BF, 0xE4A5, RoomSetupCallback.SpawnCeresHaze),
        0xE033 =>
            new(0xE033, 0xCDBFC9, 0x0F, 0x00, 0x00, 0xA15A, Enemies.RoomEnemyPopulationDefinitions.DeadScientist0, Enemies.RoomEnemyGraphicsSetDefinitions.DeadScientist0, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.Return, 0xC8C1, 0x0000, RoomSetupCallback.SpawnCeresHaze),
        0xE04D =>
            new(0xE04D, 0xCDBFC9, 0x10, 0x00, 0x00, 0xA182, Enemies.RoomEnemyPopulationDefinitions.DeadScientist1, Enemies.RoomEnemyGraphicsSetDefinitions.DeadScientist1, Scrolling.Layer2ScrollModes.FromHeader(0xC0, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.Return, 0xC8C1, 0x0000, RoomSetupCallback.SpawnCeresHaze),
        0xE07D =>
            new(0xE07D, 0xCDC330, 0x0F, 0x00, 0x00, 0xA15C, Enemies.RoomEnemyPopulationDefinitions.Room58Escape0, Enemies.RoomEnemyGraphicsSetDefinitions.Room58Escape0, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.Return, 0xC8C3, 0xE4A5, RoomSetupCallback.SpawnCeresHaze),
        0xE097 =>
            new(0xE097, 0xCDC330, 0x10, 0x00, 0x00, 0xA184, Enemies.RoomEnemyPopulationDefinitions.Room58Escape1, Enemies.RoomEnemyGraphicsSetDefinitions.Room58Escape1, Scrolling.Layer2ScrollModes.FromHeader(0xC1, 0x01), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.Return, 0xC8C3, 0xE4A5, RoomSetupCallback.SpawnCeresHaze),
        0xE0C7 =>
            new(0xE0C7, 0xCDC43F, 0x13, 0x24, 0x80, 0xA15E, Enemies.RoomEnemyPopulationDefinitions.CeresRidley0, Enemies.RoomEnemyGraphicsSetDefinitions.CeresRidley0, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.HandleCeresRidleyGetawayCutscene, 0xC8C5, 0x0000, RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze),
        0xE0E1 =>
            new(0xE0E1, 0xCDC43F, 0x14, 0x00, 0x00, 0xA186, Enemies.RoomEnemyPopulationDefinitions.CeresRidley1, Enemies.RoomEnemyGraphicsSetDefinitions.CeresRidley1, Scrolling.Layer2ScrollModes.FromHeader(0x00, 0x00), Scrolling.RoomScrollDefinitions.GreenWithBlueBottomRow, 0x0000, RoomMainCallback.HandleCeresRidleyGetawayCutscene, 0xC8C5, 0x0000, RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze),
        _ => null,
    };
}
