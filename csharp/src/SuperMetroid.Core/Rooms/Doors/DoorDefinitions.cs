using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The retail game's 597 physical doors.</summary>
/// <remarks>Each entry holds a door's destination room, orientation, PLM block, destination screen, Samus distance
/// and setup routine. Each door is named by the pinned disassembly's label for its record. The elevator pseudo-doors
/// are door-list entries (<see cref="RoomDoorListDefinitions"/>), not doors.</remarks>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class DoorDefinitions
{
    /// <summary>Retail doors in the cartridge's record order.</summary>
    internal static readonly CartridgeDoorHeader[] Definitions =
    [
        Door(nameof(LandingSiteLandingCutscene), RoomHeaderDefinitions.LandingSite, 0x03, new(0x00, 0x00), new(0x04, 0x00), 0x8000, 0x0000),
        Door(nameof(DemoSet3024), RoomHeaderDefinitions.LandingSite, 0x03, new(0x00, 0x00), new(0x04, 0x02), 0x8000, 0x0000),
        Door(nameof(LandingSite0), RoomHeaderDefinitions.Parlor, 0x05, new(0x4E, 0x06), new(0x04, 0x00), 0x8000, 0x0000),
        Door(nameof(LandingSite1), RoomHeaderDefinitions.CrateriaTube, 0x00, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(LandingSite2), RoomHeaderDefinitions.GauntletEntrance, 0x05, new(0x4E, 0x06), new(0x04, 0x00), 0x8000, 0x0000),
        Door(nameof(LandingSite3), RoomHeaderDefinitions.CrateriaPowerBombs, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GauntletEntrance0), RoomHeaderDefinitions.LandingSite, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0xB997),
        Door(nameof(GauntletEntrance1), RoomHeaderDefinitions.GauntletETank, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(Parlor0), RoomHeaderDefinitions.Terminator, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(Parlor1), RoomHeaderDefinitions.LandingSite, 0x04, new(0x01, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(Parlor2), RoomHeaderDefinitions.PreMapFlyway, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Parlor3), RoomHeaderDefinitions.Flyway, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Parlor4), RoomHeaderDefinitions.Climb, 0x06, new(0x16, 0x02), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(Parlor5), RoomHeaderDefinitions.CrateriaSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Parlor6), RoomHeaderDefinitions.FinalMissileBombway, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(CrateriaPowerBombs0), RoomHeaderDefinitions.LandingSite, 0x05, new(0x8E, 0x16), new(0x08, 0x01), 0x8000, 0x0000),
        Door(nameof(CrateriaSave0), RoomHeaderDefinitions.Parlor, 0x04, new(0x11, 0x26), new(0x01, 0x02), 0x8000, 0xB981),
        Door(nameof(WestOcean0), RoomHeaderDefinitions.Moat, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(WestOcean1), RoomHeaderDefinitions.WreckedShipEntrance, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xB971),
        Door(nameof(WestOcean2), RoomHeaderDefinitions.BowlingAlleyPath, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WestOcean3), RoomHeaderDefinitions.Attic, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WestOcean4), RoomHeaderDefinitions.BowlingAlley, 0x04, new(0x21, 0x06), new(0x02, 0x00), 0x8000, 0xB9B3),
        Door(nameof(WestOcean5), RoomHeaderDefinitions.GravitySuit, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BowlingAlleyPath0), RoomHeaderDefinitions.WestOcean, 0x05, new(0x2E, 0x26), new(0x02, 0x02), 0x8000, 0x0000),
        Door(nameof(BowlingAlleyPath1), RoomHeaderDefinitions.PreBowling, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrateriaKihunter0), RoomHeaderDefinitions.CrateriaTube, 0x01, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrateriaKihunter1), RoomHeaderDefinitions.Moat, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrateriaKihunter2), RoomHeaderDefinitions.RedBinstarElev, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ForgottenHighwayElev0), RoomHeaderDefinitions.ForgottenHighwayElbow, 0x07, new(0x06, 0x0D), new(0x00, 0x00), 0x01C0, 0x0000),
        Door(nameof(ForgottenHighwayElev1), RoomHeaderDefinitions.MaridiaElev, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0x0000),
        Door(nameof(EastOcean0), RoomHeaderDefinitions.ElectricDeath, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(EastOcean1), RoomHeaderDefinitions.ForgottenHighwayKagos, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ForgottenHighwayKagos0), RoomHeaderDefinitions.EastOcean, 0x05, new(0x6E, 0x46), new(0x06, 0x04), 0x8000, 0x0000),
        Door(nameof(ForgottenHighwayKagos1), RoomHeaderDefinitions.CrabMaze, 0x06, new(0x36, 0x02), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(CrabMaze0), RoomHeaderDefinitions.ForgottenHighwayKagos, 0x07, new(0x06, 0x3D), new(0x00, 0x03), 0x01C0, 0x0000),
        Door(nameof(ForgottenHighwayElbow0), RoomHeaderDefinitions.CrabMaze, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(CrabMaze1), RoomHeaderDefinitions.ForgottenHighwayElbow, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ForgottenHighwayElbow1), RoomHeaderDefinitions.ForgottenHighwayElev, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrateriaTube0), RoomHeaderDefinitions.LandingSite, 0x05, new(0x8E, 0x46), new(0x08, 0x04), 0x8000, 0x0000),
        Door(nameof(CrateriaTube1), RoomHeaderDefinitions.CrateriaKihunter, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Moat0), RoomHeaderDefinitions.CrateriaKihunter, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Moat1), RoomHeaderDefinitions.WestOcean, 0x04, new(0x01, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(RedBinstarElev0), RoomHeaderDefinitions.CrateriaKihunter, 0x07, new(0x16, 0x2D), new(0x01, 0x02), 0x01C0, 0xB9F1),
        Door(nameof(RedBinstarElev1), RoomHeaderDefinitions.Caterpillar, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0xBA21),
        Door(nameof(GauntletETank0), RoomHeaderDefinitions.GauntletEntrance, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GauntletETank1), RoomHeaderDefinitions.GreenPiratesShaft, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0xBA2C),
        Door(nameof(PreBowling0), RoomHeaderDefinitions.BowlingAlleyPath, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(PreBowling1), RoomHeaderDefinitions.BowlingAlley, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x00A0, 0xB9CA),
        Door(nameof(Climb0), RoomHeaderDefinitions.Parlor, 0x07, new(0x16, 0x4D), new(0x01, 0x04), 0x01C0, 0xB981),
        Door(nameof(Climb1), RoomHeaderDefinitions.CrateriaSuper, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Climb2), RoomHeaderDefinitions.CrateriaSuper, 0x04, new(0x01, 0x76), new(0x00, 0x07), 0x8000, 0x0000),
        Door(nameof(Climb3), RoomHeaderDefinitions.Pit, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Climb4), RoomHeaderDefinitions.TourianEscape4, 0x05, new(0x2E, 0x16), new(0x02, 0x01), 0x8000, 0x0000),
        Door(nameof(Pit0), RoomHeaderDefinitions.Climb, 0x05, new(0x1E, 0x86), new(0x01, 0x08), 0x8000, 0x0000),
        Door(nameof(Pit1), RoomHeaderDefinitions.ElevToBlueBrinstar, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ElevToBlueBrinstar0), RoomHeaderDefinitions.Pit, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(ElevToBlueBrinstar1), RoomHeaderDefinitions.MorphBall, 0x02, new(0x00, 0x00), new(0x05, 0x00), 0x0000, 0x0000),
        Door(nameof(BombTorizo0), RoomHeaderDefinitions.Flyway, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Flyway0), RoomHeaderDefinitions.Parlor, 0x05, new(0x3E, 0x26), new(0x03, 0x02), 0x8000, 0xB9A2),
        Door(nameof(Flyway1), RoomHeaderDefinitions.BombTorizo, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PreMapFlyway0), RoomHeaderDefinitions.Parlor, 0x05, new(0x1E, 0x36), new(0x01, 0x03), 0x8000, 0xB981),
        Door(nameof(PreMapFlyway1), RoomHeaderDefinitions.CrateriaMap, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Terminator0), RoomHeaderDefinitions.GreenPiratesShaft, 0x05, new(0x0E, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(Terminator1), RoomHeaderDefinitions.Parlor, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xB98C),
        Door(nameof(GreenBrinstarElev0), RoomHeaderDefinitions.LowerMushrooms, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarElev1), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0xBD25),
        Door(nameof(LowerMushrooms0), RoomHeaderDefinitions.GreenPiratesShaft, 0x04, new(0x01, 0x66), new(0x00, 0x06), 0x8000, 0x0000),
        Door(nameof(LowerMushrooms1), RoomHeaderDefinitions.GreenBrinstarElev, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrateriaMap0), RoomHeaderDefinitions.PreMapFlyway, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenPiratesShaft0), RoomHeaderDefinitions.Terminator, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(GreenPiratesShaft1), RoomHeaderDefinitions.LowerMushrooms, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenPiratesShaft2), RoomHeaderDefinitions.StatuesHallway, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenPiratesShaft3), RoomHeaderDefinitions.GauntletETank, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xBA16),
        Door(nameof(CrateriaSuper0), RoomHeaderDefinitions.Climb, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0xBA00),
        Door(nameof(CrateriaSuper1), RoomHeaderDefinitions.Climb, 0x05, new(0x2E, 0x76), new(0x02, 0x07), 0x8000, 0xBA0B),
        Door(nameof(FinalMissileBombway0), RoomHeaderDefinitions.FinalMissile, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(FinalMissileBombway1), RoomHeaderDefinitions.Parlor, 0x04, new(0x11, 0x36), new(0x01, 0x03), 0x8000, 0xB981),
        Door(nameof(FinalMissile0), RoomHeaderDefinitions.FinalMissileBombway, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarMainShaft0), RoomHeaderDefinitions.GreenBrinstarElev, 0x03, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0xBE36),
        Door(nameof(GreenBrinstarMainShaft1), RoomHeaderDefinitions.BrinstarPreMap, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarMainShaft2), RoomHeaderDefinitions.GreenBrinstarBeetoms, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarMainShaft3), RoomHeaderDefinitions.GreenBrinstarFirefleas, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarMainShaft4), RoomHeaderDefinitions.EarlySupers, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarMainShaft5), RoomHeaderDefinitions.Dachora, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xBD6C),
        Door(nameof(GreenBrinstarMainShaft6), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x04, new(0x01, 0x76), new(0x01, 0x07), 0x8000, 0xBD25),
        Door(nameof(GreenBrinstarMainShaft7), RoomHeaderDefinitions.EtecoonETank, 0x05, new(0x4E, 0x16), new(0x04, 0x01), 0x8000, 0xBD16),
        Door(nameof(GreenBrinstarMainShaft8), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x05, new(0x0E, 0x76), new(0x00, 0x07), 0x8000, 0xBD07),
        Door(nameof(GreenBrinstarMainShaftA), RoomHeaderDefinitions.GreenBrinstarSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SporeSpawnSuper0), RoomHeaderDefinitions.SporeSpawnFarming, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(SporeSpawnSuper1), RoomHeaderDefinitions.SporeSpawn, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BrinstarPreMap0), RoomHeaderDefinitions.GreenBrinstarMap, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BrinstarPreMap1), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x04, new(0x01, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(EarlySupers0), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x05, new(0x0E, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(EarlySupers1), RoomHeaderDefinitions.BrinstarReserveTank, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BrinstarReserveTank0), RoomHeaderDefinitions.EarlySupers, 0x05, new(0x2E, 0x16), new(0x02, 0x01), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarMap0), RoomHeaderDefinitions.BrinstarPreMap, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarFirefleas0), RoomHeaderDefinitions.GreenBrinstarMissileRefill, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarFirefleas1), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x04, new(0x01, 0x66), new(0x00, 0x06), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarMissileRefill0), RoomHeaderDefinitions.GreenBrinstarFirefleas, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(Dachora0), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x05, new(0x0E, 0x66), new(0x00, 0x06), 0x8000, 0x0000),
        Door(nameof(Dachora1), RoomHeaderDefinitions.BigPink, 0x04, new(0x21, 0x26), new(0x02, 0x02), 0x8000, 0x0000),
        Door(nameof(Dachora2), RoomHeaderDefinitions.DachoraEnergyRefill, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BigPink0), RoomHeaderDefinitions.SporeSpawnKihunters, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BigPink1), RoomHeaderDefinitions.Dachora, 0x05, new(0x6E, 0x06), new(0x06, 0x00), 0x8000, 0x0000),
        Door(nameof(BigPink2), RoomHeaderDefinitions.PinkBrinstarPowerBombs, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0xBD30),
        Door(nameof(BigPink3), RoomHeaderDefinitions.GreenHillZone, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BigPink4), RoomHeaderDefinitions.BigPinkSaveRoom, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BigPink5), RoomHeaderDefinitions.PinkBrinstarPowerBombs, 0x01, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BigPink6), RoomHeaderDefinitions.WaterwayETank, 0x05, new(0x6E, 0x06), new(0x06, 0x00), 0x8000, 0x0000),
        Door(nameof(BigPink7), RoomHeaderDefinitions.PinkBrinstarHoppers, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(BigPink8), RoomHeaderDefinitions.SporeSpawnFarming, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SporeSpawnKihunters0), RoomHeaderDefinitions.BigPink, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(SporeSpawnKihunters1), RoomHeaderDefinitions.SporeSpawn, 0x07, new(0x06, 0x2E), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(SporeSpawn0), RoomHeaderDefinitions.SporeSpawnSuper, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xBDC0),
        Door(nameof(SporeSpawn1), RoomHeaderDefinitions.SporeSpawnKihunters, 0x06, new(0x36, 0x03), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(PinkBrinstarPowerBombs0), RoomHeaderDefinitions.BigPink, 0x00, new(0x00, 0x00), new(0x02, 0x03), 0x8000, 0x0000),
        Door(nameof(PinkBrinstarPowerBombs1), RoomHeaderDefinitions.BigPink, 0x04, new(0x21, 0x46), new(0x02, 0x04), 0x8000, 0x0000),
        Door(nameof(GreenHillZone0), RoomHeaderDefinitions.BigPink, 0x05, new(0x3E, 0x66), new(0x03, 0x06), 0x8000, 0x0000),
        Door(nameof(GreenHillZone1), RoomHeaderDefinitions.MorphBall, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(GreenHillZone2), RoomHeaderDefinitions.NoobBridge, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MorphBall0), RoomHeaderDefinitions.GreenHillZone, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(MorphBall1), RoomHeaderDefinitions.ConstructionZone, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MorphBall2), RoomHeaderDefinitions.ElevToBlueBrinstar, 0x03, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0x0000),
        Door(nameof(ConstructionZone0), RoomHeaderDefinitions.MorphBall, 0x05, new(0x7E, 0x26), new(0x07, 0x02), 0x8000, 0x0000),
        Door(nameof(ConstructionZone1), RoomHeaderDefinitions.BlueBrinstarETank, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(ConstructionZone2), RoomHeaderDefinitions.FirstMissile, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BlueBrinstarETank0), RoomHeaderDefinitions.ConstructionZone, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BlueBrinstarETank1), RoomHeaderDefinitions.BlueBrinstarBoulders, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(NoobBridge0), RoomHeaderDefinitions.GreenHillZone, 0x05, new(0x7E, 0x36), new(0x07, 0x03), 0x8000, 0x0000),
        Door(nameof(NoobBridge1), RoomHeaderDefinitions.RedTower, 0x04, new(0x01, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarBeetoms0), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x04, new(0x01, 0xA6), new(0x00, 0x0A), 0x8000, 0xBD07),
        Door(nameof(GreenBrinstarBeetoms1), RoomHeaderDefinitions.EtecoonETank, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(EtecoonETank0), RoomHeaderDefinitions.GreenBrinstarBeetoms, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(EtecoonETank1), RoomHeaderDefinitions.EtecoonSuper, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(EtecoonETank2), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x04, new(0x21, 0xB6), new(0x02, 0x0B), 0x8000, 0xBD25),
        Door(nameof(EtecoonETank3), RoomHeaderDefinitions.EtecoonSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(EtecoonSuper0), RoomHeaderDefinitions.EtecoonETank, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(DachoraEnergyRefill0), RoomHeaderDefinitions.Dachora, 0x04, new(0x01, 0x66), new(0x00, 0x06), 0x8000, 0xBD50),
        Door(nameof(SporeSpawnFarming0), RoomHeaderDefinitions.SporeSpawnSuper, 0x04, new(0x01, 0x86), new(0x00, 0x08), 0x8000, 0x0000),
        Door(nameof(SporeSpawnFarming1), RoomHeaderDefinitions.BigPink, 0x05, new(0x4E, 0x56), new(0x04, 0x05), 0x8000, 0xBD5B),
        Door(nameof(WaterwayETank0), RoomHeaderDefinitions.BigPink, 0x04, new(0x01, 0x96), new(0x00, 0x09), 0x8000, 0xBD77),
        Door(nameof(WaterwayETank1), RoomHeaderDefinitions.KraidRefill, 0x06, new(0x16, 0x03), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(FirstMissile0), RoomHeaderDefinitions.ConstructionZone, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0xBE25),
        Door(nameof(PinkBrinstarHoppers0), RoomHeaderDefinitions.BigPink, 0x05, new(0x3E, 0x46), new(0x03, 0x04), 0x8000, 0x0000),
        Door(nameof(PinkBrinstarHoppers1), RoomHeaderDefinitions.HopperETank, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(HopperETank0), RoomHeaderDefinitions.PinkBrinstarHoppers, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(BigPinkSaveRoom0), RoomHeaderDefinitions.BigPink, 0x04, new(0x11, 0x06), new(0x01, 0x00), 0x8000, 0xBE00),
        Door(nameof(BlueBrinstarBoulders0), RoomHeaderDefinitions.BlueBrinstarETank, 0x04, new(0x21, 0x06), new(0x02, 0x00), 0x8000, 0xBD8A),
        Door(nameof(BlueBrinstarBoulders1), RoomHeaderDefinitions.BillyMays, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BillyMays0), RoomHeaderDefinitions.BlueBrinstarBoulders, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBrinstarSave0), RoomHeaderDefinitions.GreenBrinstarMainShaft, 0x04, new(0x01, 0x56), new(0x00, 0x05), 0x8000, 0x0000),
        Door(nameof(EtecoonSave0), RoomHeaderDefinitions.EtecoonETank, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0xBD16),
        Door(nameof(RedTower0), RoomHeaderDefinitions.Hellway, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RedTower1), RoomHeaderDefinitions.NoobBridge, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(RedTower2), RoomHeaderDefinitions.RedBrinstarFirefleas, 0x05, new(0x7E, 0x06), new(0x07, 0x00), 0x8000, 0x0000),
        Door(nameof(RedTower3), RoomHeaderDefinitions.SkreeBoost, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RedTower4), RoomHeaderDefinitions.RedTowerRefill, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RedBrinstarFirefleas0), RoomHeaderDefinitions.XrayScope, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(RedBrinstarFirefleas1), RoomHeaderDefinitions.RedTower, 0x04, new(0x01, 0x66), new(0x00, 0x06), 0x8000, 0x0000),
        Door(nameof(XrayScope0), RoomHeaderDefinitions.RedBrinstarFirefleas, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Hellway0), RoomHeaderDefinitions.RedTower, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Hellway1), RoomHeaderDefinitions.Caterpillar, 0x04, new(0x01, 0x56), new(0x00, 0x05), 0x8000, 0xBE1A),
        Door(nameof(Caterpillar0), RoomHeaderDefinitions.AlphaPowerBomb, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Caterpillar1), RoomHeaderDefinitions.BetaPowerBomb, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(Caterpillar2), RoomHeaderDefinitions.Hellway, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Caterpillar3), RoomHeaderDefinitions.RedBinstarElev, 0x03, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0x0000),
        Door(nameof(Caterpillar4), RoomHeaderDefinitions.RedFish, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xBDAF),
        Door(nameof(Caterpillar6), RoomHeaderDefinitions.RedBrinstarSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BetaPowerBomb0), RoomHeaderDefinitions.Caterpillar, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(AlphaPowerBomb0), RoomHeaderDefinitions.Caterpillar, 0x04, new(0x01, 0x76), new(0x00, 0x07), 0x8000, 0xBE0B),
        Door(nameof(SkreeBoost0), RoomHeaderDefinitions.RedTower, 0x05, new(0x0E, 0x96), new(0x00, 0x09), 0x8000, 0xBDA0),
        Door(nameof(SkreeBoost1), RoomHeaderDefinitions.BelowSpazer, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(BelowSpazer0), RoomHeaderDefinitions.SkreeBoost, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BelowSpazer1), RoomHeaderDefinitions.WestTunnel, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BelowSpazer2), RoomHeaderDefinitions.Spazer, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Spazer0), RoomHeaderDefinitions.BelowSpazer, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0xBDF1),
        Door(nameof(WarehouseZeela0), RoomHeaderDefinitions.WarehouseEntrance, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0xBD3F),
        Door(nameof(WarehouseZeela1), RoomHeaderDefinitions.WarehouseETank, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WarehouseZeela2), RoomHeaderDefinitions.WarehouseKihunter, 0x07, new(0x06, 0x0C), new(0x00, 0x00), 0x0240, 0x0000),
        Door(nameof(WarehouseETank0), RoomHeaderDefinitions.WarehouseZeela, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(WarehouseKihunter0), RoomHeaderDefinitions.WarehouseZeela, 0x06, new(0x16, 0x13), new(0x01, 0x01), 0x01C0, 0x0000),
        Door(nameof(WarehouseKihunter1), RoomHeaderDefinitions.MiniKraid, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WarehouseKihunter2), RoomHeaderDefinitions.WarehouseSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MiniKraid0), RoomHeaderDefinitions.WarehouseKihunter, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(MiniKraid1), RoomHeaderDefinitions.KraidEyeDoor, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(KraidEyeDoor0), RoomHeaderDefinitions.MiniKraid, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(KraidEyeDoor1), RoomHeaderDefinitions.Kraid, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(KraidEyeDoor2), RoomHeaderDefinitions.KraidRefill, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Kraid0), RoomHeaderDefinitions.KraidEyeDoor, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(Kraid1), RoomHeaderDefinitions.VariaSuit, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(StatuesHallway0), RoomHeaderDefinitions.GreenPiratesShaft, 0x05, new(0x0E, 0x66), new(0x00, 0x06), 0x8000, 0x0000),
        Door(nameof(StatuesHallway1), RoomHeaderDefinitions.Statues, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RedTowerRefill0), RoomHeaderDefinitions.RedTower, 0x04, new(0x01, 0x96), new(0x00, 0x09), 0x8000, 0xBDA0),
        Door(nameof(KraidRefill0), RoomHeaderDefinitions.KraidEyeDoor, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0xBD95),
        Door(nameof(Statues0), RoomHeaderDefinitions.StatuesHallway, 0x05, new(0x4E, 0x06), new(0x04, 0x00), 0x8000, 0x0000),
        Door(nameof(Statues1), RoomHeaderDefinitions.TourianFirst, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0x0000),
        Door(nameof(WarehouseEntrance0), RoomHeaderDefinitions.EastTunnel, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0xBDD1),
        Door(nameof(WarehouseEntrance1), RoomHeaderDefinitions.WarehouseZeela, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WarehouseEntrance2), RoomHeaderDefinitions.BusinessCenter, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0x0000),
        Door(nameof(VariaSuit0), RoomHeaderDefinitions.Kraid, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(WarehouseSave0), RoomHeaderDefinitions.WarehouseKihunter, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0xBDE2),
        Door(nameof(RedBrinstarSave0), RoomHeaderDefinitions.Caterpillar, 0x05, new(0x0E, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(IceBeamAcid0), RoomHeaderDefinitions.IceBeamGate, 0x04, new(0x31, 0x26), new(0x03, 0x02), 0x8000, 0x0000),
        Door(nameof(IceBeamAcid1), RoomHeaderDefinitions.IceBeamSnake, 0x05, new(0x0E, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(Cathedral0), RoomHeaderDefinitions.CathedralEntrance, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Cathedral1), RoomHeaderDefinitions.RisingTide, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CathedralEntrance0), RoomHeaderDefinitions.BusinessCenter, 0x05, new(0x0E, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(CathedralEntrance1), RoomHeaderDefinitions.Cathedral, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BusinessCenter0), RoomHeaderDefinitions.IceBeamGate, 0x05, new(0x6E, 0x26), new(0x06, 0x02), 0x8000, 0x0000),
        Door(nameof(BusinessCenter1), RoomHeaderDefinitions.CathedralEntrance, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BusinessCenter2), RoomHeaderDefinitions.HiJumpETank, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BusinessCenter3), RoomHeaderDefinitions.CrocomireEscape, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BusinessCenter4), RoomHeaderDefinitions.WarehouseEntrance, 0x03, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0x0000),
        Door(nameof(BusinessCenter6), RoomHeaderDefinitions.FrogSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BusinessCenter7), RoomHeaderDefinitions.NorfairMap, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(IceBeamGate0), RoomHeaderDefinitions.IceBeamTutorial, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(IceBeamGate1), RoomHeaderDefinitions.IceBeamAcid, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(IceBeamGate2), RoomHeaderDefinitions.BusinessCenter, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(IceBeamGate3), RoomHeaderDefinitions.CrumbleShaft, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(IceBeamTutorial0), RoomHeaderDefinitions.IceBeamSnake, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(IceBeamTutorial1), RoomHeaderDefinitions.IceBeamGate, 0x04, new(0x31, 0x06), new(0x03, 0x00), 0x8000, 0xBFFA),
        Door(nameof(IceBeam0), RoomHeaderDefinitions.IceBeamSnake, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0xC03A),
        Door(nameof(IceBeamSnake0), RoomHeaderDefinitions.IceBeamAcid, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(IceBeamSnake1), RoomHeaderDefinitions.IceBeamTutorial, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(IceBeamSnake2), RoomHeaderDefinitions.IceBeam, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrumbleShaft0), RoomHeaderDefinitions.IceBeamGate, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0xC0EF),
        Door(nameof(CrumbleShaft1), RoomHeaderDefinitions.CrocomireSpeedway, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrocomireSpeedway0), RoomHeaderDefinitions.CrumbleShaft, 0x05, new(0x0E, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(CrocomireSpeedway1), RoomHeaderDefinitions.CrocomireEscape, 0x05, new(0x3E, 0x16), new(0x03, 0x01), 0x8000, 0x0000),
        Door(nameof(CrocomireSpeedway2), RoomHeaderDefinitions.CrocomireSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrocomireSpeedway3), RoomHeaderDefinitions.AcidSnakesTunnel, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrocomireSpeedway4), RoomHeaderDefinitions.Crocomire, 0x06, new(0x36, 0x02), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(Crocomire0), RoomHeaderDefinitions.PostCrocFarming, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(Crocomire1), RoomHeaderDefinitions.CrocomireSpeedway, 0x07, new(0xC6, 0x2D), new(0x0C, 0x02), 0x01C0, 0x0000),
        Door(nameof(HiJumpBoots0), RoomHeaderDefinitions.HiJumpETank, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0xC016),
        Door(nameof(CrocomireEscape0), RoomHeaderDefinitions.BusinessCenter, 0x05, new(0x0E, 0x66), new(0x00, 0x06), 0x8000, 0x0000),
        Door(nameof(CrocomireEscape1), RoomHeaderDefinitions.CrocomireSpeedway, 0x04, new(0xC1, 0x06), new(0x0C, 0x00), 0x8000, 0x0000),
        Door(nameof(HiJumpETank0), RoomHeaderDefinitions.BusinessCenter, 0x04, new(0x01, 0x56), new(0x00, 0x05), 0x8000, 0x0000),
        Door(nameof(HiJumpETank1), RoomHeaderDefinitions.HiJumpBoots, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PostCrocFarming0), RoomHeaderDefinitions.Crocomire, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xBFDA),
        Door(nameof(PostCrocFarming1), RoomHeaderDefinitions.PostCrocPowerBombs, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PostCrocFarming2), RoomHeaderDefinitions.PostCrocShaft, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PostCrocFarming3), RoomHeaderDefinitions.PostCrocSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PostCrocSave0), RoomHeaderDefinitions.PostCrocFarming, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(PostCrocPowerBombs0), RoomHeaderDefinitions.PostCrocFarming, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PostCrocShaft0), RoomHeaderDefinitions.PostCrocFarming, 0x07, new(0x06, 0x1D), new(0x00, 0x01), 0x01C0, 0x0000),
        Door(nameof(PostCrocShaft1), RoomHeaderDefinitions.GrappleTutorial3, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(PostCrocShaft2), RoomHeaderDefinitions.PostCrocMissile, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PostCrocShaft3), RoomHeaderDefinitions.PostCrocJump, 0x06, new(0x66, 0x22), new(0x06, 0x02), 0x0140, 0x0000),
        Door(nameof(PostCrocMissile0), RoomHeaderDefinitions.PostCrocShaft, 0x05, new(0x0E, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(GrappleTutorial30), RoomHeaderDefinitions.GrappleTutorial2, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GrappleTutorial31), RoomHeaderDefinitions.PostCrocShaft, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PostCrocJump0), RoomHeaderDefinitions.PostCrocShaft, 0x07, new(0x06, 0x4D), new(0x00, 0x04), 0x01C0, 0x0000),
        Door(nameof(PostCrocJump1), RoomHeaderDefinitions.GrappleBeam, 0x05, new(0x0E, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(GrappleTutorial20), RoomHeaderDefinitions.GrappleTutorial1, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(GrappleTutorial21), RoomHeaderDefinitions.GrappleTutorial3, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GrappleTutorial10), RoomHeaderDefinitions.GrappleBeam, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GrappleTutorial11), RoomHeaderDefinitions.GrappleTutorial2, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(GrappleBeam0), RoomHeaderDefinitions.PostCrocJump, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(GrappleBeam1), RoomHeaderDefinitions.GrappleTutorial1, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(NorfairReserveTank), RoomHeaderDefinitions.GreenBubblesMissiles, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xC0D3),
        Door(nameof(GreenBubblesMissiles0), RoomHeaderDefinitions.BubbleMountain, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GreenBubblesMissiles1), RoomHeaderDefinitions.NorfairReserveTank, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BubbleMountain0), RoomHeaderDefinitions.GreenBubblesMissiles, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BubbleMountain1), RoomHeaderDefinitions.RisingTide, 0x05, new(0x4E, 0x06), new(0x04, 0x00), 0x8000, 0x0000),
        Door(nameof(BubbleMountain2), RoomHeaderDefinitions.UpperNorfairFarming, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BubbleMountain3), RoomHeaderDefinitions.PurpleShaft, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0xC0DE),
        Door(nameof(BubbleMountain4), RoomHeaderDefinitions.SingleChamber, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BubbleMountain5), RoomHeaderDefinitions.BatCave, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(BubbleMountain6), RoomHeaderDefinitions.BubbleMountainSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SpeedBoosterHall0), RoomHeaderDefinitions.BatCave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0xBFE9),
        Door(nameof(SpeedBoosterHall1), RoomHeaderDefinitions.SpeedBooster, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SpeedBooster0), RoomHeaderDefinitions.SpeedBoosterHall, 0x05, new(0xBE, 0x16), new(0x0B, 0x01), 0x8000, 0x0000),
        Door(nameof(SingleChamber0), RoomHeaderDefinitions.BubbleMountain, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(SingleChamber1), RoomHeaderDefinitions.DoubleChamber, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SingleChamber2), RoomHeaderDefinitions.DoubleChamber, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(SingleChamber3), RoomHeaderDefinitions.SpikyPlatformsTunnel, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SingleChamber4), RoomHeaderDefinitions.ThreeMusketeers, 0x04, new(0x11, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(DoubleChamber0), RoomHeaderDefinitions.SingleChamber, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(DoubleChamber1), RoomHeaderDefinitions.SingleChamber, 0x05, new(0x0E, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(DoubleChamber2), RoomHeaderDefinitions.WaveBeam, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WaveBeam0), RoomHeaderDefinitions.DoubleChamber, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(SpikyPlatformsTunnel0), RoomHeaderDefinitions.SingleChamber, 0x05, new(0x0E, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(SpikyPlatformsTunnel1), RoomHeaderDefinitions.Volcano, 0x04, new(0x21, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Volcano0), RoomHeaderDefinitions.SpikyPlatformsTunnel, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(Volcano1), RoomHeaderDefinitions.KronicBoost, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(KronicBoost0), RoomHeaderDefinitions.MagdolliteTunnel, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(KronicBoost1), RoomHeaderDefinitions.Volcano, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0xC025),
        Door(nameof(KronicBoost2), RoomHeaderDefinitions.LavaDive, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(KronicBoost3), RoomHeaderDefinitions.SpikyAcidSnakesTunnel, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(MagdolliteTunnel0), RoomHeaderDefinitions.PurpleShaft, 0x05, new(0x0E, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(MagdolliteTunnel1), RoomHeaderDefinitions.KronicBoost, 0x04, new(0x11, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(PurpleShaft0), RoomHeaderDefinitions.BubbleMountain, 0x07, new(0x06, 0x3D), new(0x00, 0x03), 0x01C0, 0x0000),
        Door(nameof(PurpleShaft1), RoomHeaderDefinitions.MagdolliteTunnel, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PurpleShaft2), RoomHeaderDefinitions.PurpleFarming, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(LavaDive0), RoomHeaderDefinitions.KronicBoost, 0x04, new(0x11, 0x26), new(0x01, 0x02), 0x8000, 0x0000),
        Door(nameof(LavaDive1), RoomHeaderDefinitions.LowerNorfairElev, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(LowerNorfairElev0), RoomHeaderDefinitions.LavaDive, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(LowerNorfairElev1), RoomHeaderDefinitions.MainHall, 0x02, new(0x00, 0x00), new(0x04, 0x00), 0x0000, 0x0000),
        Door(nameof(LowerNorfairElev3), RoomHeaderDefinitions.LNElevSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(UpperNorfairFarming0), RoomHeaderDefinitions.FrogSpeedway, 0x05, new(0x7E, 0x06), new(0x07, 0x00), 0x8000, 0x0000),
        Door(nameof(UpperNorfairFarming1), RoomHeaderDefinitions.BubbleMountain, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(UpperNorfairFarming2), RoomHeaderDefinitions.RedPirateShaft, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RisingTide0), RoomHeaderDefinitions.Cathedral, 0x05, new(0x2E, 0x16), new(0x02, 0x01), 0x8000, 0x0000),
        Door(nameof(RisingTide1), RoomHeaderDefinitions.BubbleMountain, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(AcidSnakesTunnel0), RoomHeaderDefinitions.CrocomireSpeedway, 0x05, new(0xCE, 0x26), new(0x0C, 0x02), 0x8000, 0x0000),
        Door(nameof(AcidSnakesTunnel1), RoomHeaderDefinitions.UpperNorfairRefill, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(AcidSnakesTunnel2), RoomHeaderDefinitions.RedPirateShaft, 0x07, new(0x06, 0x2D), new(0x00, 0x02), 0x0200, 0x0000),
        Door(nameof(SpikyAcidSnakesTunnel0), RoomHeaderDefinitions.UpperNorfairRefill, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SpikyAcidSnakesTunnel1), RoomHeaderDefinitions.KronicBoost, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0xC0FA),
        Door(nameof(UpperNorfairRefill0), RoomHeaderDefinitions.AcidSnakesTunnel, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(UpperNorfairRefill1), RoomHeaderDefinitions.SpikyAcidSnakesTunnel, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PurpleFarming0), RoomHeaderDefinitions.PurpleShaft, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(BatCave0), RoomHeaderDefinitions.BubbleMountain, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BatCave1), RoomHeaderDefinitions.SpeedBoosterHall, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(NorfairMap0), RoomHeaderDefinitions.BusinessCenter, 0x04, new(0x01, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(BubbleMountainSave0), RoomHeaderDefinitions.BubbleMountain, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(FrogSpeedway0), RoomHeaderDefinitions.FrogSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(FrogSpeedway1), RoomHeaderDefinitions.UpperNorfairFarming, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RedPirateShaft0), RoomHeaderDefinitions.UpperNorfairFarming, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(RedPirateShaft1), RoomHeaderDefinitions.AcidSnakesTunnel, 0x06, new(0x36, 0x02), new(0x03, 0x00), 0x0140, 0x0000),
        Door(nameof(FrogSave0), RoomHeaderDefinitions.FrogSpeedway, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(FrogSave1), RoomHeaderDefinitions.BusinessCenter, 0x05, new(0x0E, 0x56), new(0x00, 0x05), 0x8000, 0x0000),
        Door(nameof(CrocomireSave0), RoomHeaderDefinitions.CrocomireSpeedway, 0x05, new(0xAE, 0x16), new(0x0C, 0x01), 0x8000, 0x0000),
        Door(nameof(LNElevSave0), RoomHeaderDefinitions.LowerNorfairElev, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(AcidStatue0), RoomHeaderDefinitions.GoldenTorizo, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xC089),
        Door(nameof(AcidStatue1), RoomHeaderDefinitions.MainHall, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(MainHall0), RoomHeaderDefinitions.AcidStatue, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(MainHall1), RoomHeaderDefinitions.FastPillarsSetup, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(MainHall2), RoomHeaderDefinitions.LowerNorfairElev, 0x03, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0x0000),
        Door(nameof(GoldenTorizo0), RoomHeaderDefinitions.AcidStatue, 0x05, new(0x2E, 0x26), new(0x02, 0x02), 0x8000, 0xBF9E),
        Door(nameof(GoldenTorizo1), RoomHeaderDefinitions.ScrewAttack, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(FastRipper0), RoomHeaderDefinitions.ScrewAttack, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(FastRipper1), RoomHeaderDefinitions.FastPillarsSetup, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0xC056),
        Door(nameof(GTEnergyRefill0), RoomHeaderDefinitions.ScrewAttack, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(Ridley0), RoomHeaderDefinitions.RidleyETank, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Ridley1), RoomHeaderDefinitions.LNFarming, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(LNFarming0), RoomHeaderDefinitions.Ridley, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(LNFarming1), RoomHeaderDefinitions.Plowerhouse, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(FastPillarsSetup0), RoomHeaderDefinitions.MainHall, 0x05, new(0x7E, 0x26), new(0x07, 0x02), 0x8000, 0x0000),
        Door(nameof(FastPillarsSetup1), RoomHeaderDefinitions.MickeyMouse, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(FastPillarsSetup2), RoomHeaderDefinitions.MainHall, 0x05, new(0x7E, 0x26), new(0x07, 0x02), 0x8000, 0x0000),
        Door(nameof(FastPillarsSetup3), RoomHeaderDefinitions.FastRipper, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(FastPillarsSetup4), RoomHeaderDefinitions.Pillar, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(UnusedRoom83991E), RoomHeaderDefinitions.FastPillarsSetup, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MickeyMouse0), RoomHeaderDefinitions.FastPillarsSetup, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MickeyMouse1), RoomHeaderDefinitions.WorstRoomInTheGame, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xC0A2),
        Door(nameof(Pillar0), RoomHeaderDefinitions.FastPillarsSetup, 0x05, new(0x0E, 0x26), new(0x00, 0x02), 0x8000, 0xC056),
        Door(nameof(Pillar1), RoomHeaderDefinitions.WorstRoomInTheGame, 0x04, new(0x01, 0x56), new(0x00, 0x05), 0x8000, 0x0000),
        Door(nameof(Plowerhouse0), RoomHeaderDefinitions.LNFarming, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Plowerhouse1), RoomHeaderDefinitions.MetalPirates, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WorstRoomInTheGame0), RoomHeaderDefinitions.MickeyMouse, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0xC10B),
        Door(nameof(WorstRoomInTheGame1), RoomHeaderDefinitions.Amphitheatre, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(WorstRoomInTheGame2), RoomHeaderDefinitions.Pillar, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(Amphitheatre0), RoomHeaderDefinitions.WorstRoomInTheGame, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(Amphitheatre1), RoomHeaderDefinitions.RedKihunterShaft, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xC067),
        Door(nameof(LNSpringBallMaze0), RoomHeaderDefinitions.ThreeMusketeers, 0x05, new(0x3E, 0x26), new(0x03, 0x02), 0x8000, 0x0000),
        Door(nameof(LNSpringBallMaze1), RoomHeaderDefinitions.LNFireflea, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xC0AD),
        Door(nameof(LNSpringBallMaze2), RoomHeaderDefinitions.LNEscapePowerBombs, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(LNEscapePowerBombs0), RoomHeaderDefinitions.LNFireflea, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0xC078),
        Door(nameof(LNEscapePowerBombs1), RoomHeaderDefinitions.LNSpringBallMaze, 0x07, new(0x46, 0x0D), new(0x04, 0x00), 0x01C0, 0xBFBB),
        Door(nameof(RedKihunterShaft0), RoomHeaderDefinitions.Wasteland, 0x06, new(0x56, 0x02), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(RedKihunterShaft1), RoomHeaderDefinitions.Amphitheatre, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(RedKihunterShaft2), RoomHeaderDefinitions.LNFireflea, 0x04, new(0x11, 0x36), new(0x01, 0x03), 0x8000, 0x0000),
        Door(nameof(RedKihunterShaft3), RoomHeaderDefinitions.LNSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Wasteland0), RoomHeaderDefinitions.MetalPirates, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Wasteland1), RoomHeaderDefinitions.RedKihunterShaft, 0x07, new(0x26, 0x4D), new(0x02, 0x04), 0x0200, 0x0000),
        Door(nameof(MetalPirates0), RoomHeaderDefinitions.Plowerhouse, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(MetalPirates1), RoomHeaderDefinitions.Wasteland, 0x04, new(0x11, 0x26), new(0x01, 0x02), 0x8000, 0xC04B),
        Door(nameof(ThreeMusketeers0), RoomHeaderDefinitions.SingleChamber, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(ThreeMusketeers1), RoomHeaderDefinitions.LNSpringBallMaze, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RidleyETank0), RoomHeaderDefinitions.Ridley, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(ScrewAttack0), RoomHeaderDefinitions.FastRipper, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ScrewAttack1), RoomHeaderDefinitions.GTEnergyRefill, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ScrewAttack2), RoomHeaderDefinitions.GoldenTorizo, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(LNFireflea0), RoomHeaderDefinitions.LNSpringBallMaze, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(LNFireflea1), RoomHeaderDefinitions.LNEscapePowerBombs, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(LNFireflea2), RoomHeaderDefinitions.RedKihunterShaft, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0xC0C2),
        Door(nameof(LNSave0), RoomHeaderDefinitions.RedKihunterShaft, 0x05, new(0x0E, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(BowlingAlley0), RoomHeaderDefinitions.WestOcean, 0x05, new(0x7E, 0x16), new(0x07, 0x01), 0x8000, 0x0000),
        Door(nameof(BowlingAlley1), RoomHeaderDefinitions.PreBowling, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BowlingAlley2), RoomHeaderDefinitions.GravitySuit, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WreckedShipEntrance0), RoomHeaderDefinitions.WestOcean, 0x05, new(0x7E, 0x46), new(0x07, 0x04), 0x8000, 0x0000),
        Door(nameof(WreckedShipEntrance1), RoomHeaderDefinitions.WreckedShipMainShaft, 0x04, new(0x41, 0x36), new(0x04, 0x03), 0x8000, 0x0000),
        Door(nameof(Attic0), RoomHeaderDefinitions.WreckedShipMainShaft, 0x06, new(0x46, 0x02), new(0x04, 0x00), 0x8000, 0x0000),
        Door(nameof(Attic1), RoomHeaderDefinitions.AssemblyLine, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Attic2), RoomHeaderDefinitions.WestOcean, 0x05, new(0x7E, 0x06), new(0x07, 0x00), 0x8000, 0x0000),
        Door(nameof(AssemblyLine0), RoomHeaderDefinitions.Attic, 0x05, new(0x6E, 0x06), new(0x06, 0x00), 0x8000, 0x0000),
        Door(nameof(WreckedShipMainShaft0), RoomHeaderDefinitions.WreckedShipEntrance, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0xE1D8),
        Door(nameof(WreckedShipMainShaft1), RoomHeaderDefinitions.SpongeBath, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WreckedShipMainShaft2), RoomHeaderDefinitions.WSWestSuper, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WreckedShipMainShaft3), RoomHeaderDefinitions.Basement, 0x06, new(0x26, 0x02), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(WreckedShipMainShaft4), RoomHeaderDefinitions.Attic, 0x07, new(0x46, 0x0E), new(0x04, 0x00), 0x01C0, 0x0000),
        Door(nameof(WreckedShipMainShaft5), RoomHeaderDefinitions.WSEastSuper, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WreckedShipMainShaft6), RoomHeaderDefinitions.WreckedShipSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SpikyDeath0), RoomHeaderDefinitions.SpongeBath, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0xE1E8),
        Door(nameof(SpikyDeath1), RoomHeaderDefinitions.ElectricDeath, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(ElectricDeath0), RoomHeaderDefinitions.EastOcean, 0x04, new(0x01, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(ElectricDeath1), RoomHeaderDefinitions.SpikyDeath, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(ElectricDeath2), RoomHeaderDefinitions.WreckedShipETank, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(WreckedShipETank0), RoomHeaderDefinitions.ElectricDeath, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0xE1F3),
        Door(nameof(Basement0), RoomHeaderDefinitions.WreckedShipMainShaft, 0x07, new(0x46, 0x7D), new(0x04, 0x07), 0x01C0, 0xE21A),
        Door(nameof(Basement1), RoomHeaderDefinitions.WreckedShipMap, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Basement2), RoomHeaderDefinitions.Phantoon, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WreckedShipMap0), RoomHeaderDefinitions.Basement, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Phantoon0), RoomHeaderDefinitions.Basement, 0x05, new(0x4E, 0x06), new(0x04, 0x00), 0x8000, 0xE1FE),
        Door(nameof(SpongeBath0), RoomHeaderDefinitions.WreckedShipMainShaft, 0x05, new(0x4E, 0x46), new(0x04, 0x04), 0x8000, 0x0000),
        Door(nameof(SpongeBath1), RoomHeaderDefinitions.SpikyDeath, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WSWestSuper0), RoomHeaderDefinitions.WreckedShipMainShaft, 0x04, new(0x41, 0x66), new(0x04, 0x06), 0x8000, 0x0000),
        Door(nameof(WSEastSuper0), RoomHeaderDefinitions.WreckedShipMainShaft, 0x05, new(0x5E, 0x66), new(0x05, 0x06), 0x8000, 0xE20F),
        Door(nameof(GravitySuit0), RoomHeaderDefinitions.WestOcean, 0x05, new(0x5E, 0x36), new(0x05, 0x03), 0x8000, 0x0000),
        Door(nameof(GravitySuit1), RoomHeaderDefinitions.BowlingAlley, 0x04, new(0x11, 0x26), new(0x01, 0x02), 0x8000, 0xE229),
        Door(nameof(WreckedShipSave0), RoomHeaderDefinitions.WreckedShipMainShaft, 0x05, new(0x4E, 0x36), new(0x04, 0x03), 0x8000, 0x0000),
        Door(nameof(GlassTunnelSave0), RoomHeaderDefinitions.GlassTunnel, 0x05, new(0x0E, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(GlassTunnel0), RoomHeaderDefinitions.MainStreet, 0x07, new(0x16, 0x7D), new(0x01, 0x07), 0x0200, 0x0000),
        Door(nameof(GlassTunnel1), RoomHeaderDefinitions.WestTunnel, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(GlassTunnel2), RoomHeaderDefinitions.EastTunnel, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0xE345),
        Door(nameof(GlassTunnel3), RoomHeaderDefinitions.GlassTunnelSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WestTunnel0), RoomHeaderDefinitions.GlassTunnel, 0x00, new(0x00, 0x00), new(0x00, 0x01), 0x8000, 0xE378),
        Door(nameof(WestTunnel1), RoomHeaderDefinitions.BelowSpazer, 0x05, new(0x1E, 0x16), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(EastTunnel0), RoomHeaderDefinitions.GlassTunnel, 0x01, new(0x00, 0x00), new(0x00, 0x01), 0x8000, 0xE378),
        Door(nameof(EastTunnel1), RoomHeaderDefinitions.WarehouseEntrance, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(EastTunnel2), RoomHeaderDefinitions.CrabHole, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0xE356),
        Door(nameof(MainStreet0), RoomHeaderDefinitions.GlassTunnel, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x0170, 0x0000),
        Door(nameof(MainStreet1), RoomHeaderDefinitions.CrabTunnel, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MainStreet2), RoomHeaderDefinitions.FishTank, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(MainStreet3), RoomHeaderDefinitions.MtEverest, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MainStreet4), RoomHeaderDefinitions.MtEverest, 0x00, new(0x00, 0x00), new(0x01, 0x02), 0x8000, 0x0000),
        Door(nameof(FishTank0), RoomHeaderDefinitions.MainStreet, 0x05, new(0x2E, 0x66), new(0x02, 0x06), 0x8000, 0x0000),
        Door(nameof(FishTank1), RoomHeaderDefinitions.MamaTurtle, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(FishTank2), RoomHeaderDefinitions.MtEverest, 0x07, new(0x16, 0x3D), new(0x01, 0x03), 0x01C0, 0x0000),
        Door(nameof(FishTank3), RoomHeaderDefinitions.MtEverest, 0x07, new(0x46, 0x3D), new(0x04, 0x03), 0x01C0, 0x0000),
        Door(nameof(MamaTurtle0), RoomHeaderDefinitions.FishTank, 0x05, new(0x3E, 0x26), new(0x03, 0x02), 0x8000, 0x0000),
        Door(nameof(CrabTunnel0), RoomHeaderDefinitions.MainStreet, 0x05, new(0x1E, 0x76), new(0x01, 0x07), 0x8000, 0x0000),
        Door(nameof(CrabTunnel1), RoomHeaderDefinitions.CrabHole, 0x00, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MtEverest0), RoomHeaderDefinitions.RedFish, 0x07, new(0x26, 0x1D), new(0x02, 0x01), 0x01C0, 0x0000),
        Door(nameof(MtEverest1), RoomHeaderDefinitions.MainStreet, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(MtEverest2), RoomHeaderDefinitions.FishTank, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MtEverest3), RoomHeaderDefinitions.FishTank, 0x06, new(0x36, 0x02), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(MtEverest4), RoomHeaderDefinitions.MainStreet, 0x01, new(0x00, 0x00), new(0x02, 0x02), 0x8000, 0x0000),
        Door(nameof(MtEverest5), RoomHeaderDefinitions.CrabShaft, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(RedFish0), RoomHeaderDefinitions.MtEverest, 0x06, new(0x26, 0x02), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(RedFish1), RoomHeaderDefinitions.Caterpillar, 0x05, new(0x2E, 0x36), new(0x02, 0x03), 0x8000, 0xE367),
        Door(nameof(WateringHole0), RoomHeaderDefinitions.NWestMaridiaBug, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x0140, 0x0000),
        Door(nameof(NWestMaridiaBug0), RoomHeaderDefinitions.WateringHole, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(NWestMaridiaBug1), RoomHeaderDefinitions.PseudoPlasmaSpark, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(CrabShaft0), RoomHeaderDefinitions.MtEverest, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(CrabShaft1), RoomHeaderDefinitions.PseudoPlasmaSpark, 0x07, new(0x06, 0x2D), new(0x00, 0x02), 0x01C0, 0x0000),
        Door(nameof(CrabShaft2), RoomHeaderDefinitions.Aqueduct, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(PseudoPlasmaSpark0), RoomHeaderDefinitions.NWestMaridiaBug, 0x05, new(0x3E, 0x16), new(0x03, 0x01), 0x8000, 0x0000),
        Door(nameof(PseudoPlasmaSpark1), RoomHeaderDefinitions.CrabShaft, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PseudoPlasmaSpark2), RoomHeaderDefinitions.PlasmaSpark, 0x00, new(0x00, 0x00), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(CrabHole0), RoomHeaderDefinitions.CrabTunnel, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(CrabHole1), RoomHeaderDefinitions.WestSandHallTunnel, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CrabHole2), RoomHeaderDefinitions.EastTunnel, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(CrabHole3), RoomHeaderDefinitions.MaridiaMap, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WestSandHallTunnel0), RoomHeaderDefinitions.CrabHole, 0x01, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WestSandHallTunnel1), RoomHeaderDefinitions.WestSandHall, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PlasmaTutorial0), RoomHeaderDefinitions.Kassiuz, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PlasmaTutorial1), RoomHeaderDefinitions.Plasma, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Plasma0), RoomHeaderDefinitions.PlasmaTutorial, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ThreadTheNeedle0), RoomHeaderDefinitions.BugSandHole, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ThreadTheNeedle1), RoomHeaderDefinitions.MaridiaElev, 0x04, new(0x01, 0x56), new(0x00, 0x05), 0x8000, 0x0000),
        Door(nameof(MaridiaElev0), RoomHeaderDefinitions.ThreadTheNeedle, 0x05, new(0x6E, 0x06), new(0x06, 0x00), 0x8000, 0x0000),
        Door(nameof(MaridiaElev1), RoomHeaderDefinitions.ForgottenHighwaySave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MaridiaElev2), RoomHeaderDefinitions.ForgottenHighwayElev, 0x03, new(0x00, 0x00), new(0x00, 0x00), 0x0000, 0x0000),
        Door(nameof(PlasmaSpark0), RoomHeaderDefinitions.Butterfly, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(PlasmaSpark1), RoomHeaderDefinitions.Toilet, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0xE291),
        Door(nameof(PlasmaSpark2), RoomHeaderDefinitions.Kassiuz, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(PlasmaSpark3), RoomHeaderDefinitions.BugSandHole, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Kassiuz0), RoomHeaderDefinitions.PlasmaSpark, 0x05, new(0x2E, 0x16), new(0x02, 0x01), 0x8000, 0x0000),
        Door(nameof(Kassiuz1), RoomHeaderDefinitions.PlasmaTutorial, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MaridiaMap0), RoomHeaderDefinitions.CrabHole, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0xE356),
        Door(nameof(ForgottenHighwaySave0), RoomHeaderDefinitions.MaridiaElev, 0x05, new(0x0E, 0x46), new(0x00, 0x04), 0x8000, 0x0000),
        Door(nameof(Toilet0), RoomHeaderDefinitions.Oasis, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0xE309),
        Door(nameof(Toilet1), RoomHeaderDefinitions.PlasmaSpark, 0x07, new(0x06, 0x2D), new(0x00, 0x02), 0x0200, 0xE301),
        Door(nameof(BugSandHole0), RoomHeaderDefinitions.ThreadTheNeedle, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BugSandHole1), RoomHeaderDefinitions.PlasmaBeachQuicksand, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BugSandHole2), RoomHeaderDefinitions.PlasmaSpark, 0x05, new(0x3E, 0x36), new(0x03, 0x03), 0x8000, 0x0000),
        Door(nameof(WestSandHall0), RoomHeaderDefinitions.WestSandHallTunnel, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WestSandHall1), RoomHeaderDefinitions.Oasis, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(UnusedWestSandHall283A654), RoomHeaderDefinitions.WestSandHole, 0x03, new(0x00, 0x00), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(Oasis0), RoomHeaderDefinitions.WestSandHall, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(Oasis1), RoomHeaderDefinitions.EastSandHall, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Oasis2), RoomHeaderDefinitions.Toilet, 0x03, new(0x00, 0x00), new(0x00, 0x09), 0x8000, 0xE26C),
        Door(nameof(EastSandHall0), RoomHeaderDefinitions.Oasis, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(EastSandHall1), RoomHeaderDefinitions.Pants, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0xE3A3),
        Door(nameof(UnusedEastSandHall283A69C), RoomHeaderDefinitions.EastSandHole, 0x03, new(0x00, 0x00), new(0x01, 0x01), 0x8000, 0x0000),
        Door(nameof(WestSandHole0), RoomHeaderDefinitions.WestAqueductQuicksand, 0x03, new(0x00, 0x00), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(WestSandHole1), RoomHeaderDefinitions.WestSandHall, 0x02, new(0x00, 0x00), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(EastSandHole0), RoomHeaderDefinitions.EastAqueductQuicksand, 0x03, new(0x00, 0x00), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(EastSandHole1), RoomHeaderDefinitions.EastSandHall, 0x02, new(0x00, 0x00), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(WestAqueductQuicksand0), RoomHeaderDefinitions.Aqueduct, 0x03, new(0x00, 0x00), new(0x01, 0x02), 0x8000, 0x0000),
        Door(nameof(WestAqueductQuicksand1), RoomHeaderDefinitions.WestSandHole, 0x02, new(0x00, 0x00), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(EastAqueductQuicksand0), RoomHeaderDefinitions.Aqueduct, 0x03, new(0x00, 0x00), new(0x03, 0x02), 0x8000, 0x0000),
        Door(nameof(EastAqueductQuicksand1), RoomHeaderDefinitions.EastSandHole, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Aqueduct0), RoomHeaderDefinitions.CrabShaft, 0x05, new(0x1E, 0x36), new(0x01, 0x03), 0x8000, 0xE398),
        Door(nameof(Aqueduct1), RoomHeaderDefinitions.WestAqueductQuicksand, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Aqueduct2), RoomHeaderDefinitions.EastAqueductQuicksand, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Aqueduct3), RoomHeaderDefinitions.BotwoonHallway, 0x07, new(0x06, 0x0D), new(0x00, 0x00), 0x01C0, 0x0000),
        Door(nameof(Aqueduct4), RoomHeaderDefinitions.BelowBotwoonETank, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Aqueduct5), RoomHeaderDefinitions.AqueductSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Butterfly0), RoomHeaderDefinitions.PlasmaSpark, 0x05, new(0x3E, 0x56), new(0x03, 0x05), 0x8000, 0x0000),
        Door(nameof(Butterfly1), RoomHeaderDefinitions.WestCactusAlley, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(BotwoonHallway0), RoomHeaderDefinitions.Aqueduct, 0x06, new(0x06, 0x02), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BotwoonHallway1), RoomHeaderDefinitions.Botwoon, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Pants0), RoomHeaderDefinitions.EastSandHall, 0x05, new(0x2E, 0x06), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(Pants1), RoomHeaderDefinitions.EastPants, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(Pants2), RoomHeaderDefinitions.Shaktool, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Pants3), RoomHeaderDefinitions.Pants, 0x05, new(0x0E, 0x36), new(0x00, 0x03), 0x8000, 0xE3B9),
        Door(nameof(EastPants0), RoomHeaderDefinitions.Pants, 0x05, new(0x0E, 0x36), new(0x00, 0x03), 0x8000, 0xE3A3),
        Door(nameof(EastPants1), RoomHeaderDefinitions.Shaktool, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Springball0), RoomHeaderDefinitions.Shaktool, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(BelowBotwoonETank0), RoomHeaderDefinitions.Aqueduct, 0x05, new(0x5E, 0x16), new(0x05, 0x01), 0x8000, 0x0000),
        Door(nameof(Colosseum0), RoomHeaderDefinitions.HalfieClimb, 0x01, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Colosseum1), RoomHeaderDefinitions.DraygonSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Colosseum2), RoomHeaderDefinitions.ThePrecious, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door("UnusedRoom83A804", RoomHeaderDefinitions.HalfieClimb, 0x01, new(0x00, 0x00), new(0x00, 0x02), 0x8000, 0x0000),
        Door("UnusedRoom83A810", RoomHeaderDefinitions.MaridiaMissileRefill, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(ThePreciousMaridiaLoad11), RoomHeaderDefinitions.ThePrecious, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(AqueductSave0), RoomHeaderDefinitions.Aqueduct, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(ThePrecious0), RoomHeaderDefinitions.Colosseum, 0x05, new(0x6E, 0x16), new(0x06, 0x01), 0x8000, 0x0000),
        Door(nameof(ThePrecious1), RoomHeaderDefinitions.Draygon, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BotwoonETank0), RoomHeaderDefinitions.Botwoon, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0xE38D),
        Door(nameof(BotwoonETank1), RoomHeaderDefinitions.BotwoonQuicksand, 0x02, new(0x00, 0x00), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BotwoonETank2), RoomHeaderDefinitions.BotwoonQuicksand, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BotwoonETank3), RoomHeaderDefinitions.HalfieClimb, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(DraygonSave0), RoomHeaderDefinitions.MaridiaEnergyRefill, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(DraygonSave1), RoomHeaderDefinitions.Colosseum, 0x05, new(0x6E, 0x06), new(0x06, 0x00), 0x8000, 0x0000),
        Door(nameof(MaridiaMissileRefill0), RoomHeaderDefinitions.HalfieClimb, 0x05, new(0x4E, 0x26), new(0x04, 0x02), 0x8000, 0xE318),
        Door(nameof(PlasmaBeachQuicksand0), RoomHeaderDefinitions.Butterfly, 0x02, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BotwoonQuicksand0), RoomHeaderDefinitions.BelowBotwoonETank, 0x02, new(0x00, 0x00), new(0x02, 0x00), 0x8000, 0x0000),
        Door(nameof(BotwoonQuicksand1), RoomHeaderDefinitions.BelowBotwoonETank, 0x02, new(0x00, 0x00), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(Shaktool0), RoomHeaderDefinitions.EastPants, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0xE3C8),
        Door(nameof(Shaktool1), RoomHeaderDefinitions.Springball, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(HalfieClimb0), RoomHeaderDefinitions.BotwoonETank, 0x05, new(0x6E, 0x06), new(0x06, 0x00), 0x8000, 0x0000),
        Door(nameof(HalfieClimb1), RoomHeaderDefinitions.Colosseum, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(HalfieClimb2), RoomHeaderDefinitions.MaridiaMissileRefill, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(HalfieClimb3), RoomHeaderDefinitions.EastCactusAlley, 0x05, new(0x4E, 0x16), new(0x04, 0x01), 0x8000, 0x0000),
        Door(nameof(Botwoon0), RoomHeaderDefinitions.BotwoonHallway, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(Botwoon1), RoomHeaderDefinitions.BotwoonETank, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(SpaceJump0), RoomHeaderDefinitions.Draygon, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(MaridiaEnergyRefill0), RoomHeaderDefinitions.DraygonSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WestCactusAlley0), RoomHeaderDefinitions.Butterfly, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(WestCactusAlley1), RoomHeaderDefinitions.EastCactusAlley, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(EastCactusAlley0), RoomHeaderDefinitions.WestCactusAlley, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(EastCactusAlley1), RoomHeaderDefinitions.HalfieClimb, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(Draygon0), RoomHeaderDefinitions.ThePrecious, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0xE3D9),
        Door(nameof(Draygon1), RoomHeaderDefinitions.SpaceJump, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(TourianFirst0), RoomHeaderDefinitions.Metroids1, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(TourianFirst1), RoomHeaderDefinitions.Statues, 0x03, new(0x00, 0x00), new(0x00, 0x01), 0x0000, 0xE4C0),
        Door(nameof(TourianFirst3), RoomHeaderDefinitions.UpperTourianSave, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Metroids10), RoomHeaderDefinitions.TourianFirst, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(Metroids11), RoomHeaderDefinitions.Metroids2, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Metroids20), RoomHeaderDefinitions.Metroids1, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Metroids21), RoomHeaderDefinitions.Metroids3, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Metroids30), RoomHeaderDefinitions.Metroids2, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(Metroids31), RoomHeaderDefinitions.Metroids4, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Metroids40), RoomHeaderDefinitions.Metroids3, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(Metroids41), RoomHeaderDefinitions.BlueHopper, 0x06, new(0x16, 0x03), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(BlueHopper0), RoomHeaderDefinitions.Metroids4, 0x07, new(0x06, 0x1E), new(0x00, 0x01), 0x01C0, 0x0000),
        Door(nameof(BlueHopper1), RoomHeaderDefinitions.DustTorizo, 0x05, new(0x1E, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(DustTorizo0), RoomHeaderDefinitions.BlueHopper, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(DustTorizo1), RoomHeaderDefinitions.BigBoy, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(BigBoy0), RoomHeaderDefinitions.DustTorizo, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(BigBoy1), RoomHeaderDefinitions.Seaweed, 0x01, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Seaweed0), RoomHeaderDefinitions.BigBoy, 0x00, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Seaweed1), RoomHeaderDefinitions.TourianEyeDoor, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Seaweed2), RoomHeaderDefinitions.TourianRecharge, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(TourianRecharge0), RoomHeaderDefinitions.Seaweed, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(MotherBrain0), RoomHeaderDefinitions.RinkaShaft, 0x04, new(0x01, 0x26), new(0x00, 0x02), 0x8000, 0x0000),
        Door(nameof(MotherBrain1), RoomHeaderDefinitions.TourianEscape1, 0x09, new(0x1F, 0x06), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(TourianEyeDoor0), RoomHeaderDefinitions.Seaweed, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(TourianEyeDoor1), RoomHeaderDefinitions.RinkaShaft, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RinkaShaft0), RoomHeaderDefinitions.TourianEyeDoor, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(RinkaShaft1), RoomHeaderDefinitions.MotherBrainSave, 0x05, new(0x0E, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(RinkaShaft2), RoomHeaderDefinitions.MotherBrain, 0x05, new(0x3E, 0x06), new(0x03, 0x00), 0x8000, 0x0000),
        Door(nameof(MotherBrainSave0), RoomHeaderDefinitions.RinkaShaft, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(TourianEscape10), RoomHeaderDefinitions.MotherBrain, 0x04, new(0x01, 0x06), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(TourianEscape11), RoomHeaderDefinitions.TourianEscape2, 0x06, new(0x06, 0x03), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(TourianEscape20), RoomHeaderDefinitions.TourianEscape1, 0x07, new(0x06, 0x0C), new(0x00, 0x00), 0x0200, 0x0000),
        Door(nameof(TourianEscape21), RoomHeaderDefinitions.TourianEscape3, 0x04, new(0x01, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(TourianEscape30), RoomHeaderDefinitions.TourianEscape2, 0x05, new(0x0E, 0x16), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(TourianEscape31), RoomHeaderDefinitions.TourianEscape4, 0x04, new(0x01, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(TourianEscape40), RoomHeaderDefinitions.TourianEscape3, 0x05, new(0x5E, 0x06), new(0x05, 0x00), 0x8000, 0x0000),
        Door(nameof(TourianEscape41), RoomHeaderDefinitions.Climb, 0x04, new(0x01, 0x86), new(0x00, 0x08), 0x8000, 0xE4CF),
        Door(nameof(UpperTourianSave0), RoomHeaderDefinitions.TourianFirst, 0x05, new(0x0E, 0x36), new(0x00, 0x03), 0x8000, 0x0000),
        Door(nameof(CeresElev0), RoomHeaderDefinitions.FallingTile, 0x00, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0xE513),
        Door(nameof(FallingTile0), RoomHeaderDefinitions.CeresElev, 0x01, new(0x00, 0x00), new(0x00, 0x02), 0x8000, 0xE4E0),
        Door(nameof(FallingTile1), RoomHeaderDefinitions.MagnetStairs, 0x00, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(MagnetStairs0), RoomHeaderDefinitions.FallingTile, 0x01, new(0x00, 0x00), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(MagnetStairs1), RoomHeaderDefinitions.DeadScientist, 0x00, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(DeadScientist0), RoomHeaderDefinitions.MagnetStairs, 0x01, new(0x00, 0x00), new(0x00, 0x01), 0x8000, 0x0000),
        Door(nameof(DeadScientist1), RoomHeaderDefinitions.Room58Escape, 0x00, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(Room58Escape0), RoomHeaderDefinitions.DeadScientist, 0x01, new(0x00, 0x00), new(0x01, 0x00), 0x8000, 0x0000),
        Door(nameof(Room58Escape1), RoomHeaderDefinitions.CeresRidley, 0x00, new(0x00, 0x00), new(0x00, 0x00), 0x8000, 0x0000),
        Door(nameof(CeresRidley), RoomHeaderDefinitions.Room58Escape, 0x01, new(0x00, 0x00), new(0x01, 0x00), 0x8000, 0x0000),
    ];

    /// <summary>The compiled door named <paramref name="name"/>.</summary>
    internal static CartridgeDoorHeader Find(string name) => Definitions.Single(door => door.Name == name);

    /// <summary>Decodes a catalog entry's orientation and setup routine into a physical door header.</summary>
    private static CartridgeDoorHeader Door(string name, RoomHeaderDefinition destination, byte orientation,
        Position16<RoomBlocks> plmBlock, Position16<RoomScreens> destinationScreen, ushort samusDistance, ushort setupCode) =>
        new(name, destination, CartridgeDoorOrientation.Decode(orientation),
            plmBlock, destinationScreen, samusDistance,
            ClosedNativeWords.Decode<DoorSetupCode>(setupCode, "door setup routine"));

    /// <summary>The <c>Door_LandingSite_LandingCutscene</c> door.</summary>
    public static CartridgeDoorHeader LandingSiteLandingCutscene { get; } = Find(nameof(LandingSiteLandingCutscene));
    /// <summary>The <c>Door_DemoSet3_0_2_4</c> door.</summary>
    public static CartridgeDoorHeader DemoSet3024 { get; } = Find(nameof(DemoSet3024));
    /// <summary>The <c>Door_LandingSite_0</c> door.</summary>
    public static CartridgeDoorHeader LandingSite0 { get; } = Find(nameof(LandingSite0));
    /// <summary>The <c>Door_LandingSite_1</c> door.</summary>
    public static CartridgeDoorHeader LandingSite1 { get; } = Find(nameof(LandingSite1));
    /// <summary>The <c>Door_LandingSite_2</c> door.</summary>
    public static CartridgeDoorHeader LandingSite2 { get; } = Find(nameof(LandingSite2));
    /// <summary>The <c>Door_LandingSite_3</c> door.</summary>
    public static CartridgeDoorHeader LandingSite3 { get; } = Find(nameof(LandingSite3));
    /// <summary>The <c>Door_GauntletEntrance_0</c> door.</summary>
    public static CartridgeDoorHeader GauntletEntrance0 { get; } = Find(nameof(GauntletEntrance0));
    /// <summary>The <c>Door_GauntletEntrance_1</c> door.</summary>
    public static CartridgeDoorHeader GauntletEntrance1 { get; } = Find(nameof(GauntletEntrance1));
    /// <summary>The <c>Door_Parlor_0</c> door.</summary>
    public static CartridgeDoorHeader Parlor0 { get; } = Find(nameof(Parlor0));
    /// <summary>The <c>Door_Parlor_1</c> door.</summary>
    public static CartridgeDoorHeader Parlor1 { get; } = Find(nameof(Parlor1));
    /// <summary>The <c>Door_Parlor_2</c> door.</summary>
    public static CartridgeDoorHeader Parlor2 { get; } = Find(nameof(Parlor2));
    /// <summary>The <c>Door_Parlor_3</c> door.</summary>
    public static CartridgeDoorHeader Parlor3 { get; } = Find(nameof(Parlor3));
    /// <summary>The <c>Door_Parlor_4</c> door.</summary>
    public static CartridgeDoorHeader Parlor4 { get; } = Find(nameof(Parlor4));
    /// <summary>The <c>Door_Parlor_5</c> door.</summary>
    public static CartridgeDoorHeader Parlor5 { get; } = Find(nameof(Parlor5));
    /// <summary>The <c>Door_Parlor_6</c> door.</summary>
    public static CartridgeDoorHeader Parlor6 { get; } = Find(nameof(Parlor6));
    /// <summary>The <c>Door_CrateriaPowerBombs_0</c> door.</summary>
    public static CartridgeDoorHeader CrateriaPowerBombs0 { get; } = Find(nameof(CrateriaPowerBombs0));
    /// <summary>The <c>Door_CrateriaSave_0</c> door.</summary>
    public static CartridgeDoorHeader CrateriaSave0 { get; } = Find(nameof(CrateriaSave0));
    /// <summary>The <c>Door_WestOcean_0</c> door.</summary>
    public static CartridgeDoorHeader WestOcean0 { get; } = Find(nameof(WestOcean0));
    /// <summary>The <c>Door_WestOcean_1</c> door.</summary>
    public static CartridgeDoorHeader WestOcean1 { get; } = Find(nameof(WestOcean1));
    /// <summary>The <c>Door_WestOcean_2</c> door.</summary>
    public static CartridgeDoorHeader WestOcean2 { get; } = Find(nameof(WestOcean2));
    /// <summary>The <c>Door_WestOcean_3</c> door.</summary>
    public static CartridgeDoorHeader WestOcean3 { get; } = Find(nameof(WestOcean3));
    /// <summary>The <c>Door_WestOcean_4</c> door.</summary>
    public static CartridgeDoorHeader WestOcean4 { get; } = Find(nameof(WestOcean4));
    /// <summary>The <c>Door_WestOcean_5</c> door.</summary>
    public static CartridgeDoorHeader WestOcean5 { get; } = Find(nameof(WestOcean5));
    /// <summary>The <c>Door_BowlingAlleyPath_0</c> door.</summary>
    public static CartridgeDoorHeader BowlingAlleyPath0 { get; } = Find(nameof(BowlingAlleyPath0));
    /// <summary>The <c>Door_BowlingAlleyPath_1</c> door.</summary>
    public static CartridgeDoorHeader BowlingAlleyPath1 { get; } = Find(nameof(BowlingAlleyPath1));
    /// <summary>The <c>Door_CrateriaKihunter_0</c> door.</summary>
    public static CartridgeDoorHeader CrateriaKihunter0 { get; } = Find(nameof(CrateriaKihunter0));
    /// <summary>The <c>Door_CrateriaKihunter_1</c> door.</summary>
    public static CartridgeDoorHeader CrateriaKihunter1 { get; } = Find(nameof(CrateriaKihunter1));
    /// <summary>The <c>Door_CrateriaKihunter_2</c> door.</summary>
    public static CartridgeDoorHeader CrateriaKihunter2 { get; } = Find(nameof(CrateriaKihunter2));
    /// <summary>The <c>Door_ForgottenHighwayElev_0</c> door.</summary>
    public static CartridgeDoorHeader ForgottenHighwayElev0 { get; } = Find(nameof(ForgottenHighwayElev0));
    /// <summary>The <c>Door_ForgottenHighwayElev_1</c> door.</summary>
    public static CartridgeDoorHeader ForgottenHighwayElev1 { get; } = Find(nameof(ForgottenHighwayElev1));
    /// <summary>The <c>Door_EastOcean_0</c> door.</summary>
    public static CartridgeDoorHeader EastOcean0 { get; } = Find(nameof(EastOcean0));
    /// <summary>The <c>Door_EastOcean_1</c> door.</summary>
    public static CartridgeDoorHeader EastOcean1 { get; } = Find(nameof(EastOcean1));
    /// <summary>The <c>Door_ForgottenHighwayKagos_0</c> door.</summary>
    public static CartridgeDoorHeader ForgottenHighwayKagos0 { get; } = Find(nameof(ForgottenHighwayKagos0));
    /// <summary>The <c>Door_ForgottenHighwayKagos_1</c> door.</summary>
    public static CartridgeDoorHeader ForgottenHighwayKagos1 { get; } = Find(nameof(ForgottenHighwayKagos1));
    /// <summary>The <c>Door_CrabMaze_0</c> door.</summary>
    public static CartridgeDoorHeader CrabMaze0 { get; } = Find(nameof(CrabMaze0));
    /// <summary>The <c>Door_ForgottenHighwayElbow_0</c> door.</summary>
    public static CartridgeDoorHeader ForgottenHighwayElbow0 { get; } = Find(nameof(ForgottenHighwayElbow0));
    /// <summary>The <c>Door_CrabMaze_1</c> door.</summary>
    public static CartridgeDoorHeader CrabMaze1 { get; } = Find(nameof(CrabMaze1));
    /// <summary>The <c>Door_ForgottenHighwayElbow_1</c> door.</summary>
    public static CartridgeDoorHeader ForgottenHighwayElbow1 { get; } = Find(nameof(ForgottenHighwayElbow1));
    /// <summary>The <c>Door_CrateriaTube_0</c> door.</summary>
    public static CartridgeDoorHeader CrateriaTube0 { get; } = Find(nameof(CrateriaTube0));
    /// <summary>The <c>Door_CrateriaTube_1</c> door.</summary>
    public static CartridgeDoorHeader CrateriaTube1 { get; } = Find(nameof(CrateriaTube1));
    /// <summary>The <c>Door_Moat_0</c> door.</summary>
    public static CartridgeDoorHeader Moat0 { get; } = Find(nameof(Moat0));
    /// <summary>The <c>Door_Moat_1</c> door.</summary>
    public static CartridgeDoorHeader Moat1 { get; } = Find(nameof(Moat1));
    /// <summary>The <c>Door_RedBinstarElev_0</c> door.</summary>
    public static CartridgeDoorHeader RedBinstarElev0 { get; } = Find(nameof(RedBinstarElev0));
    /// <summary>The <c>Door_RedBinstarElev_1</c> door.</summary>
    public static CartridgeDoorHeader RedBinstarElev1 { get; } = Find(nameof(RedBinstarElev1));
    /// <summary>The <c>Door_GauntletETank_0</c> door.</summary>
    public static CartridgeDoorHeader GauntletETank0 { get; } = Find(nameof(GauntletETank0));
    /// <summary>The <c>Door_GauntletETank_1</c> door.</summary>
    public static CartridgeDoorHeader GauntletETank1 { get; } = Find(nameof(GauntletETank1));
    /// <summary>The <c>Door_PreBowling_0</c> door.</summary>
    public static CartridgeDoorHeader PreBowling0 { get; } = Find(nameof(PreBowling0));
    /// <summary>The <c>Door_PreBowling_1</c> door.</summary>
    public static CartridgeDoorHeader PreBowling1 { get; } = Find(nameof(PreBowling1));
    /// <summary>The <c>Door_Climb_0</c> door.</summary>
    public static CartridgeDoorHeader Climb0 { get; } = Find(nameof(Climb0));
    /// <summary>The <c>Door_Climb_1</c> door.</summary>
    public static CartridgeDoorHeader Climb1 { get; } = Find(nameof(Climb1));
    /// <summary>The <c>Door_Climb_2</c> door.</summary>
    public static CartridgeDoorHeader Climb2 { get; } = Find(nameof(Climb2));
    /// <summary>The <c>Door_Climb_3</c> door.</summary>
    public static CartridgeDoorHeader Climb3 { get; } = Find(nameof(Climb3));
    /// <summary>The <c>Door_Climb_4</c> door.</summary>
    public static CartridgeDoorHeader Climb4 { get; } = Find(nameof(Climb4));
    /// <summary>The <c>Door_Pit_0</c> door.</summary>
    public static CartridgeDoorHeader Pit0 { get; } = Find(nameof(Pit0));
    /// <summary>The <c>Door_Pit_1</c> door.</summary>
    public static CartridgeDoorHeader Pit1 { get; } = Find(nameof(Pit1));
    /// <summary>The <c>Door_ElevToBlueBrinstar_0</c> door.</summary>
    public static CartridgeDoorHeader ElevToBlueBrinstar0 { get; } = Find(nameof(ElevToBlueBrinstar0));
    /// <summary>The <c>Door_ElevToBlueBrinstar_1</c> door.</summary>
    public static CartridgeDoorHeader ElevToBlueBrinstar1 { get; } = Find(nameof(ElevToBlueBrinstar1));
    /// <summary>The <c>Door_BombTorizo_0</c> door.</summary>
    public static CartridgeDoorHeader BombTorizo0 { get; } = Find(nameof(BombTorizo0));
    /// <summary>The <c>Door_Flyway_0</c> door.</summary>
    public static CartridgeDoorHeader Flyway0 { get; } = Find(nameof(Flyway0));
    /// <summary>The <c>Door_Flyway_1</c> door.</summary>
    public static CartridgeDoorHeader Flyway1 { get; } = Find(nameof(Flyway1));
    /// <summary>The <c>Door_PreMapFlyway_0</c> door.</summary>
    public static CartridgeDoorHeader PreMapFlyway0 { get; } = Find(nameof(PreMapFlyway0));
    /// <summary>The <c>Door_PreMapFlyway_1</c> door.</summary>
    public static CartridgeDoorHeader PreMapFlyway1 { get; } = Find(nameof(PreMapFlyway1));
    /// <summary>The <c>Door_Terminator_0</c> door.</summary>
    public static CartridgeDoorHeader Terminator0 { get; } = Find(nameof(Terminator0));
    /// <summary>The <c>Door_Terminator_1</c> door.</summary>
    public static CartridgeDoorHeader Terminator1 { get; } = Find(nameof(Terminator1));
    /// <summary>The <c>Door_GreenBrinstarElev_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarElev0 { get; } = Find(nameof(GreenBrinstarElev0));
    /// <summary>The <c>Door_GreenBrinstarElev_1</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarElev1 { get; } = Find(nameof(GreenBrinstarElev1));
    /// <summary>The <c>Door_LowerMushrooms_0</c> door.</summary>
    public static CartridgeDoorHeader LowerMushrooms0 { get; } = Find(nameof(LowerMushrooms0));
    /// <summary>The <c>Door_LowerMushrooms_1</c> door.</summary>
    public static CartridgeDoorHeader LowerMushrooms1 { get; } = Find(nameof(LowerMushrooms1));
    /// <summary>The <c>Door_CrateriaMap_0</c> door.</summary>
    public static CartridgeDoorHeader CrateriaMap0 { get; } = Find(nameof(CrateriaMap0));
    /// <summary>The <c>Door_GreenPiratesShaft_0</c> door.</summary>
    public static CartridgeDoorHeader GreenPiratesShaft0 { get; } = Find(nameof(GreenPiratesShaft0));
    /// <summary>The <c>Door_GreenPiratesShaft_1</c> door.</summary>
    public static CartridgeDoorHeader GreenPiratesShaft1 { get; } = Find(nameof(GreenPiratesShaft1));
    /// <summary>The <c>Door_GreenPiratesShaft_2</c> door.</summary>
    public static CartridgeDoorHeader GreenPiratesShaft2 { get; } = Find(nameof(GreenPiratesShaft2));
    /// <summary>The <c>Door_GreenPiratesShaft_3</c> door.</summary>
    public static CartridgeDoorHeader GreenPiratesShaft3 { get; } = Find(nameof(GreenPiratesShaft3));
    /// <summary>The <c>Door_CrateriaSuper_0</c> door.</summary>
    public static CartridgeDoorHeader CrateriaSuper0 { get; } = Find(nameof(CrateriaSuper0));
    /// <summary>The <c>Door_CrateriaSuper_1</c> door.</summary>
    public static CartridgeDoorHeader CrateriaSuper1 { get; } = Find(nameof(CrateriaSuper1));
    /// <summary>The <c>Door_FinalMissileBombway_0</c> door.</summary>
    public static CartridgeDoorHeader FinalMissileBombway0 { get; } = Find(nameof(FinalMissileBombway0));
    /// <summary>The <c>Door_FinalMissileBombway_1</c> door.</summary>
    public static CartridgeDoorHeader FinalMissileBombway1 { get; } = Find(nameof(FinalMissileBombway1));
    /// <summary>The <c>Door_FinalMissile_0</c> door.</summary>
    public static CartridgeDoorHeader FinalMissile0 { get; } = Find(nameof(FinalMissile0));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft0 { get; } = Find(nameof(GreenBrinstarMainShaft0));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_1</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft1 { get; } = Find(nameof(GreenBrinstarMainShaft1));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_2</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft2 { get; } = Find(nameof(GreenBrinstarMainShaft2));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_3</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft3 { get; } = Find(nameof(GreenBrinstarMainShaft3));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_4</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft4 { get; } = Find(nameof(GreenBrinstarMainShaft4));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_5</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft5 { get; } = Find(nameof(GreenBrinstarMainShaft5));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_6</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft6 { get; } = Find(nameof(GreenBrinstarMainShaft6));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_7</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft7 { get; } = Find(nameof(GreenBrinstarMainShaft7));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_8</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaft8 { get; } = Find(nameof(GreenBrinstarMainShaft8));
    /// <summary>The <c>Door_GreenBrinstarMainShaft_A</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMainShaftA { get; } = Find(nameof(GreenBrinstarMainShaftA));
    /// <summary>The <c>Door_SporeSpawnSuper_0</c> door.</summary>
    public static CartridgeDoorHeader SporeSpawnSuper0 { get; } = Find(nameof(SporeSpawnSuper0));
    /// <summary>The <c>Door_SporeSpawnSuper_1</c> door.</summary>
    public static CartridgeDoorHeader SporeSpawnSuper1 { get; } = Find(nameof(SporeSpawnSuper1));
    /// <summary>The <c>Door_BrinstarPreMap_0</c> door.</summary>
    public static CartridgeDoorHeader BrinstarPreMap0 { get; } = Find(nameof(BrinstarPreMap0));
    /// <summary>The <c>Door_BrinstarPreMap_1</c> door.</summary>
    public static CartridgeDoorHeader BrinstarPreMap1 { get; } = Find(nameof(BrinstarPreMap1));
    /// <summary>The <c>Door_EarlySupers_0</c> door.</summary>
    public static CartridgeDoorHeader EarlySupers0 { get; } = Find(nameof(EarlySupers0));
    /// <summary>The <c>Door_EarlySupers_1</c> door.</summary>
    public static CartridgeDoorHeader EarlySupers1 { get; } = Find(nameof(EarlySupers1));
    /// <summary>The <c>Door_BrinstarReserveTank_0</c> door.</summary>
    public static CartridgeDoorHeader BrinstarReserveTank0 { get; } = Find(nameof(BrinstarReserveTank0));
    /// <summary>The <c>Door_GreenBrinstarMap_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMap0 { get; } = Find(nameof(GreenBrinstarMap0));
    /// <summary>The <c>Door_GreenBrinstarFirefleas_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarFirefleas0 { get; } = Find(nameof(GreenBrinstarFirefleas0));
    /// <summary>The <c>Door_GreenBrinstarFirefleas_1</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarFirefleas1 { get; } = Find(nameof(GreenBrinstarFirefleas1));
    /// <summary>The <c>Door_GreenBrinstarMissileRefill_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarMissileRefill0 { get; } = Find(nameof(GreenBrinstarMissileRefill0));
    /// <summary>The <c>Door_Dachora_0</c> door.</summary>
    public static CartridgeDoorHeader Dachora0 { get; } = Find(nameof(Dachora0));
    /// <summary>The <c>Door_Dachora_1</c> door.</summary>
    public static CartridgeDoorHeader Dachora1 { get; } = Find(nameof(Dachora1));
    /// <summary>The <c>Door_Dachora_2</c> door.</summary>
    public static CartridgeDoorHeader Dachora2 { get; } = Find(nameof(Dachora2));
    /// <summary>The <c>Door_BigPink_0</c> door.</summary>
    public static CartridgeDoorHeader BigPink0 { get; } = Find(nameof(BigPink0));
    /// <summary>The <c>Door_BigPink_1</c> door.</summary>
    public static CartridgeDoorHeader BigPink1 { get; } = Find(nameof(BigPink1));
    /// <summary>The <c>Door_BigPink_2</c> door.</summary>
    public static CartridgeDoorHeader BigPink2 { get; } = Find(nameof(BigPink2));
    /// <summary>The <c>Door_BigPink_3</c> door.</summary>
    public static CartridgeDoorHeader BigPink3 { get; } = Find(nameof(BigPink3));
    /// <summary>The <c>Door_BigPink_4</c> door.</summary>
    public static CartridgeDoorHeader BigPink4 { get; } = Find(nameof(BigPink4));
    /// <summary>The <c>Door_BigPink_5</c> door.</summary>
    public static CartridgeDoorHeader BigPink5 { get; } = Find(nameof(BigPink5));
    /// <summary>The <c>Door_BigPink_6</c> door.</summary>
    public static CartridgeDoorHeader BigPink6 { get; } = Find(nameof(BigPink6));
    /// <summary>The <c>Door_BigPink_7</c> door.</summary>
    public static CartridgeDoorHeader BigPink7 { get; } = Find(nameof(BigPink7));
    /// <summary>The <c>Door_BigPink_8</c> door.</summary>
    public static CartridgeDoorHeader BigPink8 { get; } = Find(nameof(BigPink8));
    /// <summary>The <c>Door_SporeSpawnKihunters_0</c> door.</summary>
    public static CartridgeDoorHeader SporeSpawnKihunters0 { get; } = Find(nameof(SporeSpawnKihunters0));
    /// <summary>The <c>Door_SporeSpawnKihunters_1</c> door.</summary>
    public static CartridgeDoorHeader SporeSpawnKihunters1 { get; } = Find(nameof(SporeSpawnKihunters1));
    /// <summary>The <c>Door_SporeSpawn_0</c> door.</summary>
    public static CartridgeDoorHeader SporeSpawn0 { get; } = Find(nameof(SporeSpawn0));
    /// <summary>The <c>Door_SporeSpawn_1</c> door.</summary>
    public static CartridgeDoorHeader SporeSpawn1 { get; } = Find(nameof(SporeSpawn1));
    /// <summary>The <c>Door_PinkBrinstarPowerBombs_0</c> door.</summary>
    public static CartridgeDoorHeader PinkBrinstarPowerBombs0 { get; } = Find(nameof(PinkBrinstarPowerBombs0));
    /// <summary>The <c>Door_PinkBrinstarPowerBombs_1</c> door.</summary>
    public static CartridgeDoorHeader PinkBrinstarPowerBombs1 { get; } = Find(nameof(PinkBrinstarPowerBombs1));
    /// <summary>The <c>Door_GreenHillZone_0</c> door.</summary>
    public static CartridgeDoorHeader GreenHillZone0 { get; } = Find(nameof(GreenHillZone0));
    /// <summary>The <c>Door_GreenHillZone_1</c> door.</summary>
    public static CartridgeDoorHeader GreenHillZone1 { get; } = Find(nameof(GreenHillZone1));
    /// <summary>The <c>Door_GreenHillZone_2</c> door.</summary>
    public static CartridgeDoorHeader GreenHillZone2 { get; } = Find(nameof(GreenHillZone2));
    /// <summary>The <c>Door_MorphBall_0</c> door.</summary>
    public static CartridgeDoorHeader MorphBall0 { get; } = Find(nameof(MorphBall0));
    /// <summary>The <c>Door_MorphBall_1</c> door.</summary>
    public static CartridgeDoorHeader MorphBall1 { get; } = Find(nameof(MorphBall1));
    /// <summary>The <c>Door_MorphBall_2</c> door.</summary>
    public static CartridgeDoorHeader MorphBall2 { get; } = Find(nameof(MorphBall2));
    /// <summary>The <c>Door_ConstructionZone_0</c> door.</summary>
    public static CartridgeDoorHeader ConstructionZone0 { get; } = Find(nameof(ConstructionZone0));
    /// <summary>The <c>Door_ConstructionZone_1</c> door.</summary>
    public static CartridgeDoorHeader ConstructionZone1 { get; } = Find(nameof(ConstructionZone1));
    /// <summary>The <c>Door_ConstructionZone_2</c> door.</summary>
    public static CartridgeDoorHeader ConstructionZone2 { get; } = Find(nameof(ConstructionZone2));
    /// <summary>The <c>Door_BlueBrinstarETank_0</c> door.</summary>
    public static CartridgeDoorHeader BlueBrinstarETank0 { get; } = Find(nameof(BlueBrinstarETank0));
    /// <summary>The <c>Door_BlueBrinstarETank_1</c> door.</summary>
    public static CartridgeDoorHeader BlueBrinstarETank1 { get; } = Find(nameof(BlueBrinstarETank1));
    /// <summary>The <c>Door_NoobBridge_0</c> door.</summary>
    public static CartridgeDoorHeader NoobBridge0 { get; } = Find(nameof(NoobBridge0));
    /// <summary>The <c>Door_NoobBridge_1</c> door.</summary>
    public static CartridgeDoorHeader NoobBridge1 { get; } = Find(nameof(NoobBridge1));
    /// <summary>The <c>Door_GreenBrinstarBeetoms_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarBeetoms0 { get; } = Find(nameof(GreenBrinstarBeetoms0));
    /// <summary>The <c>Door_GreenBrinstarBeetoms_1</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarBeetoms1 { get; } = Find(nameof(GreenBrinstarBeetoms1));
    /// <summary>The <c>Door_EtecoonETank_0</c> door.</summary>
    public static CartridgeDoorHeader EtecoonETank0 { get; } = Find(nameof(EtecoonETank0));
    /// <summary>The <c>Door_EtecoonETank_1</c> door.</summary>
    public static CartridgeDoorHeader EtecoonETank1 { get; } = Find(nameof(EtecoonETank1));
    /// <summary>The <c>Door_EtecoonETank_2</c> door.</summary>
    public static CartridgeDoorHeader EtecoonETank2 { get; } = Find(nameof(EtecoonETank2));
    /// <summary>The <c>Door_EtecoonETank_3</c> door.</summary>
    public static CartridgeDoorHeader EtecoonETank3 { get; } = Find(nameof(EtecoonETank3));
    /// <summary>The <c>Door_EtecoonSuper_0</c> door.</summary>
    public static CartridgeDoorHeader EtecoonSuper0 { get; } = Find(nameof(EtecoonSuper0));
    /// <summary>The <c>Door_DachoraEnergyRefill_0</c> door.</summary>
    public static CartridgeDoorHeader DachoraEnergyRefill0 { get; } = Find(nameof(DachoraEnergyRefill0));
    /// <summary>The <c>Door_SporeSpawnFarming_0</c> door.</summary>
    public static CartridgeDoorHeader SporeSpawnFarming0 { get; } = Find(nameof(SporeSpawnFarming0));
    /// <summary>The <c>Door_SporeSpawnFarming_1</c> door.</summary>
    public static CartridgeDoorHeader SporeSpawnFarming1 { get; } = Find(nameof(SporeSpawnFarming1));
    /// <summary>The <c>Door_WaterwayETank_0</c> door.</summary>
    public static CartridgeDoorHeader WaterwayETank0 { get; } = Find(nameof(WaterwayETank0));
    /// <summary>The <c>Door_WaterwayETank_1</c> door.</summary>
    public static CartridgeDoorHeader WaterwayETank1 { get; } = Find(nameof(WaterwayETank1));
    /// <summary>The <c>Door_FirstMissile_0</c> door.</summary>
    public static CartridgeDoorHeader FirstMissile0 { get; } = Find(nameof(FirstMissile0));
    /// <summary>The <c>Door_PinkBrinstarHoppers_0</c> door.</summary>
    public static CartridgeDoorHeader PinkBrinstarHoppers0 { get; } = Find(nameof(PinkBrinstarHoppers0));
    /// <summary>The <c>Door_PinkBrinstarHoppers_1</c> door.</summary>
    public static CartridgeDoorHeader PinkBrinstarHoppers1 { get; } = Find(nameof(PinkBrinstarHoppers1));
    /// <summary>The <c>Door_HopperETank_0</c> door.</summary>
    public static CartridgeDoorHeader HopperETank0 { get; } = Find(nameof(HopperETank0));
    /// <summary>The <c>Door_BigPinkSaveRoom_0</c> door.</summary>
    public static CartridgeDoorHeader BigPinkSaveRoom0 { get; } = Find(nameof(BigPinkSaveRoom0));
    /// <summary>The <c>Door_BlueBrinstarBoulders_0</c> door.</summary>
    public static CartridgeDoorHeader BlueBrinstarBoulders0 { get; } = Find(nameof(BlueBrinstarBoulders0));
    /// <summary>The <c>Door_BlueBrinstarBoulders_1</c> door.</summary>
    public static CartridgeDoorHeader BlueBrinstarBoulders1 { get; } = Find(nameof(BlueBrinstarBoulders1));
    /// <summary>The <c>Door_BillyMays_0</c> door.</summary>
    public static CartridgeDoorHeader BillyMays0 { get; } = Find(nameof(BillyMays0));
    /// <summary>The <c>Door_GreenBrinstarSave_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBrinstarSave0 { get; } = Find(nameof(GreenBrinstarSave0));
    /// <summary>The <c>Door_EtecoonSave_0</c> door.</summary>
    public static CartridgeDoorHeader EtecoonSave0 { get; } = Find(nameof(EtecoonSave0));
    /// <summary>The <c>Door_RedTower_0</c> door.</summary>
    public static CartridgeDoorHeader RedTower0 { get; } = Find(nameof(RedTower0));
    /// <summary>The <c>Door_RedTower_1</c> door.</summary>
    public static CartridgeDoorHeader RedTower1 { get; } = Find(nameof(RedTower1));
    /// <summary>The <c>Door_RedTower_2</c> door.</summary>
    public static CartridgeDoorHeader RedTower2 { get; } = Find(nameof(RedTower2));
    /// <summary>The <c>Door_RedTower_3</c> door.</summary>
    public static CartridgeDoorHeader RedTower3 { get; } = Find(nameof(RedTower3));
    /// <summary>The <c>Door_RedTower_4</c> door.</summary>
    public static CartridgeDoorHeader RedTower4 { get; } = Find(nameof(RedTower4));
    /// <summary>The <c>Door_RedBrinstarFirefleas_0</c> door.</summary>
    public static CartridgeDoorHeader RedBrinstarFirefleas0 { get; } = Find(nameof(RedBrinstarFirefleas0));
    /// <summary>The <c>Door_RedBrinstarFirefleas_1</c> door.</summary>
    public static CartridgeDoorHeader RedBrinstarFirefleas1 { get; } = Find(nameof(RedBrinstarFirefleas1));
    /// <summary>The <c>Door_XrayScope_0</c> door.</summary>
    public static CartridgeDoorHeader XrayScope0 { get; } = Find(nameof(XrayScope0));
    /// <summary>The <c>Door_Hellway_0</c> door.</summary>
    public static CartridgeDoorHeader Hellway0 { get; } = Find(nameof(Hellway0));
    /// <summary>The <c>Door_Hellway_1</c> door.</summary>
    public static CartridgeDoorHeader Hellway1 { get; } = Find(nameof(Hellway1));
    /// <summary>The <c>Door_Caterpillar_0</c> door.</summary>
    public static CartridgeDoorHeader Caterpillar0 { get; } = Find(nameof(Caterpillar0));
    /// <summary>The <c>Door_Caterpillar_1</c> door.</summary>
    public static CartridgeDoorHeader Caterpillar1 { get; } = Find(nameof(Caterpillar1));
    /// <summary>The <c>Door_Caterpillar_2</c> door.</summary>
    public static CartridgeDoorHeader Caterpillar2 { get; } = Find(nameof(Caterpillar2));
    /// <summary>The <c>Door_Caterpillar_3</c> door.</summary>
    public static CartridgeDoorHeader Caterpillar3 { get; } = Find(nameof(Caterpillar3));
    /// <summary>The <c>Door_Caterpillar_4</c> door.</summary>
    public static CartridgeDoorHeader Caterpillar4 { get; } = Find(nameof(Caterpillar4));
    /// <summary>The <c>Door_Caterpillar_6</c> door.</summary>
    public static CartridgeDoorHeader Caterpillar6 { get; } = Find(nameof(Caterpillar6));
    /// <summary>The <c>Door_BetaPowerBomb_0</c> door.</summary>
    public static CartridgeDoorHeader BetaPowerBomb0 { get; } = Find(nameof(BetaPowerBomb0));
    /// <summary>The <c>Door_AlphaPowerBomb_0</c> door.</summary>
    public static CartridgeDoorHeader AlphaPowerBomb0 { get; } = Find(nameof(AlphaPowerBomb0));
    /// <summary>The <c>Door_SkreeBoost_0</c> door.</summary>
    public static CartridgeDoorHeader SkreeBoost0 { get; } = Find(nameof(SkreeBoost0));
    /// <summary>The <c>Door_SkreeBoost_1</c> door.</summary>
    public static CartridgeDoorHeader SkreeBoost1 { get; } = Find(nameof(SkreeBoost1));
    /// <summary>The <c>Door_BelowSpazer_0</c> door.</summary>
    public static CartridgeDoorHeader BelowSpazer0 { get; } = Find(nameof(BelowSpazer0));
    /// <summary>The <c>Door_BelowSpazer_1</c> door.</summary>
    public static CartridgeDoorHeader BelowSpazer1 { get; } = Find(nameof(BelowSpazer1));
    /// <summary>The <c>Door_BelowSpazer_2</c> door.</summary>
    public static CartridgeDoorHeader BelowSpazer2 { get; } = Find(nameof(BelowSpazer2));
    /// <summary>The <c>Door_Spazer_0</c> door.</summary>
    public static CartridgeDoorHeader Spazer0 { get; } = Find(nameof(Spazer0));
    /// <summary>The <c>Door_WarehouseZeela_0</c> door.</summary>
    public static CartridgeDoorHeader WarehouseZeela0 { get; } = Find(nameof(WarehouseZeela0));
    /// <summary>The <c>Door_WarehouseZeela_1</c> door.</summary>
    public static CartridgeDoorHeader WarehouseZeela1 { get; } = Find(nameof(WarehouseZeela1));
    /// <summary>The <c>Door_WarehouseZeela_2</c> door.</summary>
    public static CartridgeDoorHeader WarehouseZeela2 { get; } = Find(nameof(WarehouseZeela2));
    /// <summary>The <c>Door_WarehouseETank_0</c> door.</summary>
    public static CartridgeDoorHeader WarehouseETank0 { get; } = Find(nameof(WarehouseETank0));
    /// <summary>The <c>Door_WarehouseKihunter_0</c> door.</summary>
    public static CartridgeDoorHeader WarehouseKihunter0 { get; } = Find(nameof(WarehouseKihunter0));
    /// <summary>The <c>Door_WarehouseKihunter_1</c> door.</summary>
    public static CartridgeDoorHeader WarehouseKihunter1 { get; } = Find(nameof(WarehouseKihunter1));
    /// <summary>The <c>Door_WarehouseKihunter_2</c> door.</summary>
    public static CartridgeDoorHeader WarehouseKihunter2 { get; } = Find(nameof(WarehouseKihunter2));
    /// <summary>The <c>Door_MiniKraid_0</c> door.</summary>
    public static CartridgeDoorHeader MiniKraid0 { get; } = Find(nameof(MiniKraid0));
    /// <summary>The <c>Door_MiniKraid_1</c> door.</summary>
    public static CartridgeDoorHeader MiniKraid1 { get; } = Find(nameof(MiniKraid1));
    /// <summary>The <c>Door_KraidEyeDoor_0</c> door.</summary>
    public static CartridgeDoorHeader KraidEyeDoor0 { get; } = Find(nameof(KraidEyeDoor0));
    /// <summary>The <c>Door_KraidEyeDoor_1</c> door.</summary>
    public static CartridgeDoorHeader KraidEyeDoor1 { get; } = Find(nameof(KraidEyeDoor1));
    /// <summary>The <c>Door_KraidEyeDoor_2</c> door.</summary>
    public static CartridgeDoorHeader KraidEyeDoor2 { get; } = Find(nameof(KraidEyeDoor2));
    /// <summary>The <c>Door_Kraid_0</c> door.</summary>
    public static CartridgeDoorHeader Kraid0 { get; } = Find(nameof(Kraid0));
    /// <summary>The <c>Door_Kraid_1</c> door.</summary>
    public static CartridgeDoorHeader Kraid1 { get; } = Find(nameof(Kraid1));
    /// <summary>The <c>Door_StatuesHallway_0</c> door.</summary>
    public static CartridgeDoorHeader StatuesHallway0 { get; } = Find(nameof(StatuesHallway0));
    /// <summary>The <c>Door_StatuesHallway_1</c> door.</summary>
    public static CartridgeDoorHeader StatuesHallway1 { get; } = Find(nameof(StatuesHallway1));
    /// <summary>The <c>Door_RedTowerRefill_0</c> door.</summary>
    public static CartridgeDoorHeader RedTowerRefill0 { get; } = Find(nameof(RedTowerRefill0));
    /// <summary>The <c>Door_KraidRefill_0</c> door.</summary>
    public static CartridgeDoorHeader KraidRefill0 { get; } = Find(nameof(KraidRefill0));
    /// <summary>The <c>Door_Statues_0</c> door.</summary>
    public static CartridgeDoorHeader Statues0 { get; } = Find(nameof(Statues0));
    /// <summary>The <c>Door_Statues_1</c> door.</summary>
    public static CartridgeDoorHeader Statues1 { get; } = Find(nameof(Statues1));
    /// <summary>The <c>Door_WarehouseEntrance_0</c> door.</summary>
    public static CartridgeDoorHeader WarehouseEntrance0 { get; } = Find(nameof(WarehouseEntrance0));
    /// <summary>The <c>Door_WarehouseEntrance_1</c> door.</summary>
    public static CartridgeDoorHeader WarehouseEntrance1 { get; } = Find(nameof(WarehouseEntrance1));
    /// <summary>The <c>Door_WarehouseEntrance_2</c> door.</summary>
    public static CartridgeDoorHeader WarehouseEntrance2 { get; } = Find(nameof(WarehouseEntrance2));
    /// <summary>The <c>Door_VariaSuit_0</c> door.</summary>
    public static CartridgeDoorHeader VariaSuit0 { get; } = Find(nameof(VariaSuit0));
    /// <summary>The <c>Door_WarehouseSave_0</c> door.</summary>
    public static CartridgeDoorHeader WarehouseSave0 { get; } = Find(nameof(WarehouseSave0));
    /// <summary>The <c>Door_RedBrinstarSave_0</c> door.</summary>
    public static CartridgeDoorHeader RedBrinstarSave0 { get; } = Find(nameof(RedBrinstarSave0));
    /// <summary>The <c>Door_IceBeamAcid_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeamAcid0 { get; } = Find(nameof(IceBeamAcid0));
    /// <summary>The <c>Door_IceBeamAcid_1</c> door.</summary>
    public static CartridgeDoorHeader IceBeamAcid1 { get; } = Find(nameof(IceBeamAcid1));
    /// <summary>The <c>Door_Cathedral_0</c> door.</summary>
    public static CartridgeDoorHeader Cathedral0 { get; } = Find(nameof(Cathedral0));
    /// <summary>The <c>Door_Cathedral_1</c> door.</summary>
    public static CartridgeDoorHeader Cathedral1 { get; } = Find(nameof(Cathedral1));
    /// <summary>The <c>Door_CathedralEntrance_0</c> door.</summary>
    public static CartridgeDoorHeader CathedralEntrance0 { get; } = Find(nameof(CathedralEntrance0));
    /// <summary>The <c>Door_CathedralEntrance_1</c> door.</summary>
    public static CartridgeDoorHeader CathedralEntrance1 { get; } = Find(nameof(CathedralEntrance1));
    /// <summary>The <c>Door_BusinessCenter_0</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter0 { get; } = Find(nameof(BusinessCenter0));
    /// <summary>The <c>Door_BusinessCenter_1</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter1 { get; } = Find(nameof(BusinessCenter1));
    /// <summary>The <c>Door_BusinessCenter_2</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter2 { get; } = Find(nameof(BusinessCenter2));
    /// <summary>The <c>Door_BusinessCenter_3</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter3 { get; } = Find(nameof(BusinessCenter3));
    /// <summary>The <c>Door_BusinessCenter_4</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter4 { get; } = Find(nameof(BusinessCenter4));
    /// <summary>The <c>Door_BusinessCenter_6</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter6 { get; } = Find(nameof(BusinessCenter6));
    /// <summary>The <c>Door_BusinessCenter_7</c> door.</summary>
    public static CartridgeDoorHeader BusinessCenter7 { get; } = Find(nameof(BusinessCenter7));
    /// <summary>The <c>Door_IceBeamGate_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeamGate0 { get; } = Find(nameof(IceBeamGate0));
    /// <summary>The <c>Door_IceBeamGate_1</c> door.</summary>
    public static CartridgeDoorHeader IceBeamGate1 { get; } = Find(nameof(IceBeamGate1));
    /// <summary>The <c>Door_IceBeamGate_2</c> door.</summary>
    public static CartridgeDoorHeader IceBeamGate2 { get; } = Find(nameof(IceBeamGate2));
    /// <summary>The <c>Door_IceBeamGate_3</c> door.</summary>
    public static CartridgeDoorHeader IceBeamGate3 { get; } = Find(nameof(IceBeamGate3));
    /// <summary>The <c>Door_IceBeamTutorial_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeamTutorial0 { get; } = Find(nameof(IceBeamTutorial0));
    /// <summary>The <c>Door_IceBeamTutorial_1</c> door.</summary>
    public static CartridgeDoorHeader IceBeamTutorial1 { get; } = Find(nameof(IceBeamTutorial1));
    /// <summary>The <c>Door_IceBeam_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeam0 { get; } = Find(nameof(IceBeam0));
    /// <summary>The <c>Door_IceBeamSnake_0</c> door.</summary>
    public static CartridgeDoorHeader IceBeamSnake0 { get; } = Find(nameof(IceBeamSnake0));
    /// <summary>The <c>Door_IceBeamSnake_1</c> door.</summary>
    public static CartridgeDoorHeader IceBeamSnake1 { get; } = Find(nameof(IceBeamSnake1));
    /// <summary>The <c>Door_IceBeamSnake_2</c> door.</summary>
    public static CartridgeDoorHeader IceBeamSnake2 { get; } = Find(nameof(IceBeamSnake2));
    /// <summary>The <c>Door_CrumbleShaft_0</c> door.</summary>
    public static CartridgeDoorHeader CrumbleShaft0 { get; } = Find(nameof(CrumbleShaft0));
    /// <summary>The <c>Door_CrumbleShaft_1</c> door.</summary>
    public static CartridgeDoorHeader CrumbleShaft1 { get; } = Find(nameof(CrumbleShaft1));
    /// <summary>The <c>Door_CrocomireSpeedway_0</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway0 { get; } = Find(nameof(CrocomireSpeedway0));
    /// <summary>The <c>Door_CrocomireSpeedway_1</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway1 { get; } = Find(nameof(CrocomireSpeedway1));
    /// <summary>The <c>Door_CrocomireSpeedway_2</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway2 { get; } = Find(nameof(CrocomireSpeedway2));
    /// <summary>The <c>Door_CrocomireSpeedway_3</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway3 { get; } = Find(nameof(CrocomireSpeedway3));
    /// <summary>The <c>Door_CrocomireSpeedway_4</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSpeedway4 { get; } = Find(nameof(CrocomireSpeedway4));
    /// <summary>The <c>Door_Crocomire_0</c> door.</summary>
    public static CartridgeDoorHeader Crocomire0 { get; } = Find(nameof(Crocomire0));
    /// <summary>The <c>Door_Crocomire_1</c> door.</summary>
    public static CartridgeDoorHeader Crocomire1 { get; } = Find(nameof(Crocomire1));
    /// <summary>The <c>Door_HiJumpBoots_0</c> door.</summary>
    public static CartridgeDoorHeader HiJumpBoots0 { get; } = Find(nameof(HiJumpBoots0));
    /// <summary>The <c>Door_CrocomireEscape_0</c> door.</summary>
    public static CartridgeDoorHeader CrocomireEscape0 { get; } = Find(nameof(CrocomireEscape0));
    /// <summary>The <c>Door_CrocomireEscape_1</c> door.</summary>
    public static CartridgeDoorHeader CrocomireEscape1 { get; } = Find(nameof(CrocomireEscape1));
    /// <summary>The <c>Door_HiJumpETank_0</c> door.</summary>
    public static CartridgeDoorHeader HiJumpETank0 { get; } = Find(nameof(HiJumpETank0));
    /// <summary>The <c>Door_HiJumpETank_1</c> door.</summary>
    public static CartridgeDoorHeader HiJumpETank1 { get; } = Find(nameof(HiJumpETank1));
    /// <summary>The <c>Door_PostCrocFarming_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocFarming0 { get; } = Find(nameof(PostCrocFarming0));
    /// <summary>The <c>Door_PostCrocFarming_1</c> door.</summary>
    public static CartridgeDoorHeader PostCrocFarming1 { get; } = Find(nameof(PostCrocFarming1));
    /// <summary>The <c>Door_PostCrocFarming_2</c> door.</summary>
    public static CartridgeDoorHeader PostCrocFarming2 { get; } = Find(nameof(PostCrocFarming2));
    /// <summary>The <c>Door_PostCrocFarming_3</c> door.</summary>
    public static CartridgeDoorHeader PostCrocFarming3 { get; } = Find(nameof(PostCrocFarming3));
    /// <summary>The <c>Door_PostCrocSave_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocSave0 { get; } = Find(nameof(PostCrocSave0));
    /// <summary>The <c>Door_PostCrocPowerBombs_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocPowerBombs0 { get; } = Find(nameof(PostCrocPowerBombs0));
    /// <summary>The <c>Door_PostCrocShaft_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocShaft0 { get; } = Find(nameof(PostCrocShaft0));
    /// <summary>The <c>Door_PostCrocShaft_1</c> door.</summary>
    public static CartridgeDoorHeader PostCrocShaft1 { get; } = Find(nameof(PostCrocShaft1));
    /// <summary>The <c>Door_PostCrocShaft_2</c> door.</summary>
    public static CartridgeDoorHeader PostCrocShaft2 { get; } = Find(nameof(PostCrocShaft2));
    /// <summary>The <c>Door_PostCrocShaft_3</c> door.</summary>
    public static CartridgeDoorHeader PostCrocShaft3 { get; } = Find(nameof(PostCrocShaft3));
    /// <summary>The <c>Door_PostCrocMissile_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocMissile0 { get; } = Find(nameof(PostCrocMissile0));
    /// <summary>The <c>Door_GrappleTutorial3_0</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial30 { get; } = Find(nameof(GrappleTutorial30));
    /// <summary>The <c>Door_GrappleTutorial3_1</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial31 { get; } = Find(nameof(GrappleTutorial31));
    /// <summary>The <c>Door_PostCrocJump_0</c> door.</summary>
    public static CartridgeDoorHeader PostCrocJump0 { get; } = Find(nameof(PostCrocJump0));
    /// <summary>The <c>Door_PostCrocJump_1</c> door.</summary>
    public static CartridgeDoorHeader PostCrocJump1 { get; } = Find(nameof(PostCrocJump1));
    /// <summary>The <c>Door_GrappleTutorial2_0</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial20 { get; } = Find(nameof(GrappleTutorial20));
    /// <summary>The <c>Door_GrappleTutorial2_1</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial21 { get; } = Find(nameof(GrappleTutorial21));
    /// <summary>The <c>Door_GrappleTutorial1_0</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial10 { get; } = Find(nameof(GrappleTutorial10));
    /// <summary>The <c>Door_GrappleTutorial1_1</c> door.</summary>
    public static CartridgeDoorHeader GrappleTutorial11 { get; } = Find(nameof(GrappleTutorial11));
    /// <summary>The <c>Door_GrappleBeam_0</c> door.</summary>
    public static CartridgeDoorHeader GrappleBeam0 { get; } = Find(nameof(GrappleBeam0));
    /// <summary>The <c>Door_GrappleBeam_1</c> door.</summary>
    public static CartridgeDoorHeader GrappleBeam1 { get; } = Find(nameof(GrappleBeam1));
    /// <summary>The <c>Door_NorfairReserveTank</c> door.</summary>
    public static CartridgeDoorHeader NorfairReserveTank { get; } = Find(nameof(NorfairReserveTank));
    /// <summary>The <c>Door_GreenBubblesMissiles_0</c> door.</summary>
    public static CartridgeDoorHeader GreenBubblesMissiles0 { get; } = Find(nameof(GreenBubblesMissiles0));
    /// <summary>The <c>Door_GreenBubblesMissiles_1</c> door.</summary>
    public static CartridgeDoorHeader GreenBubblesMissiles1 { get; } = Find(nameof(GreenBubblesMissiles1));
    /// <summary>The <c>Door_BubbleMountain_0</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain0 { get; } = Find(nameof(BubbleMountain0));
    /// <summary>The <c>Door_BubbleMountain_1</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain1 { get; } = Find(nameof(BubbleMountain1));
    /// <summary>The <c>Door_BubbleMountain_2</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain2 { get; } = Find(nameof(BubbleMountain2));
    /// <summary>The <c>Door_BubbleMountain_3</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain3 { get; } = Find(nameof(BubbleMountain3));
    /// <summary>The <c>Door_BubbleMountain_4</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain4 { get; } = Find(nameof(BubbleMountain4));
    /// <summary>The <c>Door_BubbleMountain_5</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain5 { get; } = Find(nameof(BubbleMountain5));
    /// <summary>The <c>Door_BubbleMountain_6</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountain6 { get; } = Find(nameof(BubbleMountain6));
    /// <summary>The <c>Door_SpeedBoosterHall_0</c> door.</summary>
    public static CartridgeDoorHeader SpeedBoosterHall0 { get; } = Find(nameof(SpeedBoosterHall0));
    /// <summary>The <c>Door_SpeedBoosterHall_1</c> door.</summary>
    public static CartridgeDoorHeader SpeedBoosterHall1 { get; } = Find(nameof(SpeedBoosterHall1));
    /// <summary>The <c>Door_SpeedBooster_0</c> door.</summary>
    public static CartridgeDoorHeader SpeedBooster0 { get; } = Find(nameof(SpeedBooster0));
    /// <summary>The <c>Door_SingleChamber_0</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber0 { get; } = Find(nameof(SingleChamber0));
    /// <summary>The <c>Door_SingleChamber_1</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber1 { get; } = Find(nameof(SingleChamber1));
    /// <summary>The <c>Door_SingleChamber_2</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber2 { get; } = Find(nameof(SingleChamber2));
    /// <summary>The <c>Door_SingleChamber_3</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber3 { get; } = Find(nameof(SingleChamber3));
    /// <summary>The <c>Door_SingleChamber_4</c> door.</summary>
    public static CartridgeDoorHeader SingleChamber4 { get; } = Find(nameof(SingleChamber4));
    /// <summary>The <c>Door_DoubleChamber_0</c> door.</summary>
    public static CartridgeDoorHeader DoubleChamber0 { get; } = Find(nameof(DoubleChamber0));
    /// <summary>The <c>Door_DoubleChamber_1</c> door.</summary>
    public static CartridgeDoorHeader DoubleChamber1 { get; } = Find(nameof(DoubleChamber1));
    /// <summary>The <c>Door_DoubleChamber_2</c> door.</summary>
    public static CartridgeDoorHeader DoubleChamber2 { get; } = Find(nameof(DoubleChamber2));
    /// <summary>The <c>Door_WaveBeam_0</c> door.</summary>
    public static CartridgeDoorHeader WaveBeam0 { get; } = Find(nameof(WaveBeam0));
    /// <summary>The <c>Door_SpikyPlatformsTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader SpikyPlatformsTunnel0 { get; } = Find(nameof(SpikyPlatformsTunnel0));
    /// <summary>The <c>Door_SpikyPlatformsTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader SpikyPlatformsTunnel1 { get; } = Find(nameof(SpikyPlatformsTunnel1));
    /// <summary>The <c>Door_Volcano_0</c> door.</summary>
    public static CartridgeDoorHeader Volcano0 { get; } = Find(nameof(Volcano0));
    /// <summary>The <c>Door_Volcano_1</c> door.</summary>
    public static CartridgeDoorHeader Volcano1 { get; } = Find(nameof(Volcano1));
    /// <summary>The <c>Door_KronicBoost_0</c> door.</summary>
    public static CartridgeDoorHeader KronicBoost0 { get; } = Find(nameof(KronicBoost0));
    /// <summary>The <c>Door_KronicBoost_1</c> door.</summary>
    public static CartridgeDoorHeader KronicBoost1 { get; } = Find(nameof(KronicBoost1));
    /// <summary>The <c>Door_KronicBoost_2</c> door.</summary>
    public static CartridgeDoorHeader KronicBoost2 { get; } = Find(nameof(KronicBoost2));
    /// <summary>The <c>Door_KronicBoost_3</c> door.</summary>
    public static CartridgeDoorHeader KronicBoost3 { get; } = Find(nameof(KronicBoost3));
    /// <summary>The <c>Door_MagdolliteTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader MagdolliteTunnel0 { get; } = Find(nameof(MagdolliteTunnel0));
    /// <summary>The <c>Door_MagdolliteTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader MagdolliteTunnel1 { get; } = Find(nameof(MagdolliteTunnel1));
    /// <summary>The <c>Door_PurpleShaft_0</c> door.</summary>
    public static CartridgeDoorHeader PurpleShaft0 { get; } = Find(nameof(PurpleShaft0));
    /// <summary>The <c>Door_PurpleShaft_1</c> door.</summary>
    public static CartridgeDoorHeader PurpleShaft1 { get; } = Find(nameof(PurpleShaft1));
    /// <summary>The <c>Door_PurpleShaft_2</c> door.</summary>
    public static CartridgeDoorHeader PurpleShaft2 { get; } = Find(nameof(PurpleShaft2));
    /// <summary>The <c>Door_LavaDive_0</c> door.</summary>
    public static CartridgeDoorHeader LavaDive0 { get; } = Find(nameof(LavaDive0));
    /// <summary>The <c>Door_LavaDive_1</c> door.</summary>
    public static CartridgeDoorHeader LavaDive1 { get; } = Find(nameof(LavaDive1));
    /// <summary>The <c>Door_LowerNorfairElev_0</c> door.</summary>
    public static CartridgeDoorHeader LowerNorfairElev0 { get; } = Find(nameof(LowerNorfairElev0));
    /// <summary>The <c>Door_LowerNorfairElev_1</c> door.</summary>
    public static CartridgeDoorHeader LowerNorfairElev1 { get; } = Find(nameof(LowerNorfairElev1));
    /// <summary>The <c>Door_LowerNorfairElev_3</c> door.</summary>
    public static CartridgeDoorHeader LowerNorfairElev3 { get; } = Find(nameof(LowerNorfairElev3));
    /// <summary>The <c>Door_UpperNorfairFarming_0</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairFarming0 { get; } = Find(nameof(UpperNorfairFarming0));
    /// <summary>The <c>Door_UpperNorfairFarming_1</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairFarming1 { get; } = Find(nameof(UpperNorfairFarming1));
    /// <summary>The <c>Door_UpperNorfairFarming_2</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairFarming2 { get; } = Find(nameof(UpperNorfairFarming2));
    /// <summary>The <c>Door_RisingTide_0</c> door.</summary>
    public static CartridgeDoorHeader RisingTide0 { get; } = Find(nameof(RisingTide0));
    /// <summary>The <c>Door_RisingTide_1</c> door.</summary>
    public static CartridgeDoorHeader RisingTide1 { get; } = Find(nameof(RisingTide1));
    /// <summary>The <c>Door_AcidSnakesTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader AcidSnakesTunnel0 { get; } = Find(nameof(AcidSnakesTunnel0));
    /// <summary>The <c>Door_AcidSnakesTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader AcidSnakesTunnel1 { get; } = Find(nameof(AcidSnakesTunnel1));
    /// <summary>The <c>Door_AcidSnakesTunnel_2</c> door.</summary>
    public static CartridgeDoorHeader AcidSnakesTunnel2 { get; } = Find(nameof(AcidSnakesTunnel2));
    /// <summary>The <c>Door_SpikyAcidSnakesTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader SpikyAcidSnakesTunnel0 { get; } = Find(nameof(SpikyAcidSnakesTunnel0));
    /// <summary>The <c>Door_SpikyAcidSnakesTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader SpikyAcidSnakesTunnel1 { get; } = Find(nameof(SpikyAcidSnakesTunnel1));
    /// <summary>The <c>Door_UpperNorfairRefill_0</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairRefill0 { get; } = Find(nameof(UpperNorfairRefill0));
    /// <summary>The <c>Door_UpperNorfairRefill_1</c> door.</summary>
    public static CartridgeDoorHeader UpperNorfairRefill1 { get; } = Find(nameof(UpperNorfairRefill1));
    /// <summary>The <c>Door_PurpleFarming_0</c> door.</summary>
    public static CartridgeDoorHeader PurpleFarming0 { get; } = Find(nameof(PurpleFarming0));
    /// <summary>The <c>Door_BatCave_0</c> door.</summary>
    public static CartridgeDoorHeader BatCave0 { get; } = Find(nameof(BatCave0));
    /// <summary>The <c>Door_BatCave_1</c> door.</summary>
    public static CartridgeDoorHeader BatCave1 { get; } = Find(nameof(BatCave1));
    /// <summary>The <c>Door_NorfairMap_0</c> door.</summary>
    public static CartridgeDoorHeader NorfairMap0 { get; } = Find(nameof(NorfairMap0));
    /// <summary>The <c>Door_BubbleMountainSave_0</c> door.</summary>
    public static CartridgeDoorHeader BubbleMountainSave0 { get; } = Find(nameof(BubbleMountainSave0));
    /// <summary>The <c>Door_FrogSpeedway_0</c> door.</summary>
    public static CartridgeDoorHeader FrogSpeedway0 { get; } = Find(nameof(FrogSpeedway0));
    /// <summary>The <c>Door_FrogSpeedway_1</c> door.</summary>
    public static CartridgeDoorHeader FrogSpeedway1 { get; } = Find(nameof(FrogSpeedway1));
    /// <summary>The <c>Door_RedPirateShaft_0</c> door.</summary>
    public static CartridgeDoorHeader RedPirateShaft0 { get; } = Find(nameof(RedPirateShaft0));
    /// <summary>The <c>Door_RedPirateShaft_1</c> door.</summary>
    public static CartridgeDoorHeader RedPirateShaft1 { get; } = Find(nameof(RedPirateShaft1));
    /// <summary>The <c>Door_FrogSave_0</c> door.</summary>
    public static CartridgeDoorHeader FrogSave0 { get; } = Find(nameof(FrogSave0));
    /// <summary>The <c>Door_FrogSave_1</c> door.</summary>
    public static CartridgeDoorHeader FrogSave1 { get; } = Find(nameof(FrogSave1));
    /// <summary>The <c>Door_CrocomireSave_0</c> door.</summary>
    public static CartridgeDoorHeader CrocomireSave0 { get; } = Find(nameof(CrocomireSave0));
    /// <summary>The <c>Door_LNElevSave_0</c> door.</summary>
    public static CartridgeDoorHeader LNElevSave0 { get; } = Find(nameof(LNElevSave0));
    /// <summary>The <c>Door_AcidStatue_0</c> door.</summary>
    public static CartridgeDoorHeader AcidStatue0 { get; } = Find(nameof(AcidStatue0));
    /// <summary>The <c>Door_AcidStatue_1</c> door.</summary>
    public static CartridgeDoorHeader AcidStatue1 { get; } = Find(nameof(AcidStatue1));
    /// <summary>The <c>Door_MainHall_0</c> door.</summary>
    public static CartridgeDoorHeader MainHall0 { get; } = Find(nameof(MainHall0));
    /// <summary>The <c>Door_MainHall_1</c> door.</summary>
    public static CartridgeDoorHeader MainHall1 { get; } = Find(nameof(MainHall1));
    /// <summary>The <c>Door_MainHall_2</c> door.</summary>
    public static CartridgeDoorHeader MainHall2 { get; } = Find(nameof(MainHall2));
    /// <summary>The <c>Door_GoldenTorizo_0</c> door.</summary>
    public static CartridgeDoorHeader GoldenTorizo0 { get; } = Find(nameof(GoldenTorizo0));
    /// <summary>The <c>Door_GoldenTorizo_1</c> door.</summary>
    public static CartridgeDoorHeader GoldenTorizo1 { get; } = Find(nameof(GoldenTorizo1));
    /// <summary>The <c>Door_FastRipper_0</c> door.</summary>
    public static CartridgeDoorHeader FastRipper0 { get; } = Find(nameof(FastRipper0));
    /// <summary>The <c>Door_FastRipper_1</c> door.</summary>
    public static CartridgeDoorHeader FastRipper1 { get; } = Find(nameof(FastRipper1));
    /// <summary>The <c>Door_GTEnergyRefill_0</c> door.</summary>
    public static CartridgeDoorHeader GTEnergyRefill0 { get; } = Find(nameof(GTEnergyRefill0));
    /// <summary>The <c>Door_Ridley_0</c> door.</summary>
    public static CartridgeDoorHeader Ridley0 { get; } = Find(nameof(Ridley0));
    /// <summary>The <c>Door_Ridley_1</c> door.</summary>
    public static CartridgeDoorHeader Ridley1 { get; } = Find(nameof(Ridley1));
    /// <summary>The <c>Door_LNFarming_0</c> door.</summary>
    public static CartridgeDoorHeader LNFarming0 { get; } = Find(nameof(LNFarming0));
    /// <summary>The <c>Door_LNFarming_1</c> door.</summary>
    public static CartridgeDoorHeader LNFarming1 { get; } = Find(nameof(LNFarming1));
    /// <summary>The <c>Door_FastPillarsSetup_0</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup0 { get; } = Find(nameof(FastPillarsSetup0));
    /// <summary>The <c>Door_FastPillarsSetup_1</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup1 { get; } = Find(nameof(FastPillarsSetup1));
    /// <summary>The <c>Door_FastPillarsSetup_2</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup2 { get; } = Find(nameof(FastPillarsSetup2));
    /// <summary>The <c>Door_FastPillarsSetup_3</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup3 { get; } = Find(nameof(FastPillarsSetup3));
    /// <summary>The <c>Door_FastPillarsSetup_4</c> door.</summary>
    public static CartridgeDoorHeader FastPillarsSetup4 { get; } = Find(nameof(FastPillarsSetup4));
    /// <summary>The <c>UNUSED_Door_83991E</c> door.</summary>
    public static CartridgeDoorHeader UnusedRoom83991E { get; } = Find(nameof(UnusedRoom83991E));
    /// <summary>The <c>Door_MickeyMouse_0</c> door.</summary>
    public static CartridgeDoorHeader MickeyMouse0 { get; } = Find(nameof(MickeyMouse0));
    /// <summary>The <c>Door_MickeyMouse_1</c> door.</summary>
    public static CartridgeDoorHeader MickeyMouse1 { get; } = Find(nameof(MickeyMouse1));
    /// <summary>The <c>Door_Pillar_0</c> door.</summary>
    public static CartridgeDoorHeader Pillar0 { get; } = Find(nameof(Pillar0));
    /// <summary>The <c>Door_Pillar_1</c> door.</summary>
    public static CartridgeDoorHeader Pillar1 { get; } = Find(nameof(Pillar1));
    /// <summary>The <c>Door_Plowerhouse_0</c> door.</summary>
    public static CartridgeDoorHeader Plowerhouse0 { get; } = Find(nameof(Plowerhouse0));
    /// <summary>The <c>Door_Plowerhouse_1</c> door.</summary>
    public static CartridgeDoorHeader Plowerhouse1 { get; } = Find(nameof(Plowerhouse1));
    /// <summary>The <c>Door_WorstRoomInTheGame_0</c> door.</summary>
    public static CartridgeDoorHeader WorstRoomInTheGame0 { get; } = Find(nameof(WorstRoomInTheGame0));
    /// <summary>The <c>Door_WorstRoomInTheGame_1</c> door.</summary>
    public static CartridgeDoorHeader WorstRoomInTheGame1 { get; } = Find(nameof(WorstRoomInTheGame1));
    /// <summary>The <c>Door_WorstRoomInTheGame_2</c> door.</summary>
    public static CartridgeDoorHeader WorstRoomInTheGame2 { get; } = Find(nameof(WorstRoomInTheGame2));
    /// <summary>The <c>Door_Amphitheatre_0</c> door.</summary>
    public static CartridgeDoorHeader Amphitheatre0 { get; } = Find(nameof(Amphitheatre0));
    /// <summary>The <c>Door_Amphitheatre_1</c> door.</summary>
    public static CartridgeDoorHeader Amphitheatre1 { get; } = Find(nameof(Amphitheatre1));
    /// <summary>The <c>Door_LNSpringBallMaze_0</c> door.</summary>
    public static CartridgeDoorHeader LNSpringBallMaze0 { get; } = Find(nameof(LNSpringBallMaze0));
    /// <summary>The <c>Door_LNSpringBallMaze_1</c> door.</summary>
    public static CartridgeDoorHeader LNSpringBallMaze1 { get; } = Find(nameof(LNSpringBallMaze1));
    /// <summary>The <c>Door_LNSpringBallMaze_2</c> door.</summary>
    public static CartridgeDoorHeader LNSpringBallMaze2 { get; } = Find(nameof(LNSpringBallMaze2));
    /// <summary>The <c>Door_LNEscapePowerBombs_0</c> door.</summary>
    public static CartridgeDoorHeader LNEscapePowerBombs0 { get; } = Find(nameof(LNEscapePowerBombs0));
    /// <summary>The <c>Door_LNEscapePowerBombs_1</c> door.</summary>
    public static CartridgeDoorHeader LNEscapePowerBombs1 { get; } = Find(nameof(LNEscapePowerBombs1));
    /// <summary>The <c>Door_RedKihunterShaft_0</c> door.</summary>
    public static CartridgeDoorHeader RedKihunterShaft0 { get; } = Find(nameof(RedKihunterShaft0));
    /// <summary>The <c>Door_RedKihunterShaft_1</c> door.</summary>
    public static CartridgeDoorHeader RedKihunterShaft1 { get; } = Find(nameof(RedKihunterShaft1));
    /// <summary>The <c>Door_RedKihunterShaft_2</c> door.</summary>
    public static CartridgeDoorHeader RedKihunterShaft2 { get; } = Find(nameof(RedKihunterShaft2));
    /// <summary>The <c>Door_RedKihunterShaft_3</c> door.</summary>
    public static CartridgeDoorHeader RedKihunterShaft3 { get; } = Find(nameof(RedKihunterShaft3));
    /// <summary>The <c>Door_Wasteland_0</c> door.</summary>
    public static CartridgeDoorHeader Wasteland0 { get; } = Find(nameof(Wasteland0));
    /// <summary>The <c>Door_Wasteland_1</c> door.</summary>
    public static CartridgeDoorHeader Wasteland1 { get; } = Find(nameof(Wasteland1));
    /// <summary>The <c>Door_MetalPirates_0</c> door.</summary>
    public static CartridgeDoorHeader MetalPirates0 { get; } = Find(nameof(MetalPirates0));
    /// <summary>The <c>Door_MetalPirates_1</c> door.</summary>
    public static CartridgeDoorHeader MetalPirates1 { get; } = Find(nameof(MetalPirates1));
    /// <summary>The <c>Door_ThreeMusketeers_0</c> door.</summary>
    public static CartridgeDoorHeader ThreeMusketeers0 { get; } = Find(nameof(ThreeMusketeers0));
    /// <summary>The <c>Door_ThreeMusketeers_1</c> door.</summary>
    public static CartridgeDoorHeader ThreeMusketeers1 { get; } = Find(nameof(ThreeMusketeers1));
    /// <summary>The <c>Door_RidleyETank_0</c> door.</summary>
    public static CartridgeDoorHeader RidleyETank0 { get; } = Find(nameof(RidleyETank0));
    /// <summary>The <c>Door_ScrewAttack_0</c> door.</summary>
    public static CartridgeDoorHeader ScrewAttack0 { get; } = Find(nameof(ScrewAttack0));
    /// <summary>The <c>Door_ScrewAttack_1</c> door.</summary>
    public static CartridgeDoorHeader ScrewAttack1 { get; } = Find(nameof(ScrewAttack1));
    /// <summary>The <c>Door_ScrewAttack_2</c> door.</summary>
    public static CartridgeDoorHeader ScrewAttack2 { get; } = Find(nameof(ScrewAttack2));
    /// <summary>The <c>Door_LNFireflea_0</c> door.</summary>
    public static CartridgeDoorHeader LNFireflea0 { get; } = Find(nameof(LNFireflea0));
    /// <summary>The <c>Door_LNFireflea_1</c> door.</summary>
    public static CartridgeDoorHeader LNFireflea1 { get; } = Find(nameof(LNFireflea1));
    /// <summary>The <c>Door_LNFireflea_2</c> door.</summary>
    public static CartridgeDoorHeader LNFireflea2 { get; } = Find(nameof(LNFireflea2));
    /// <summary>The <c>Door_LNSave_0</c> door.</summary>
    public static CartridgeDoorHeader LNSave0 { get; } = Find(nameof(LNSave0));
    /// <summary>The <c>Door_BowlingAlley_0</c> door.</summary>
    public static CartridgeDoorHeader BowlingAlley0 { get; } = Find(nameof(BowlingAlley0));
    /// <summary>The <c>Door_BowlingAlley_1</c> door.</summary>
    public static CartridgeDoorHeader BowlingAlley1 { get; } = Find(nameof(BowlingAlley1));
    /// <summary>The <c>Door_BowlingAlley_2</c> door.</summary>
    public static CartridgeDoorHeader BowlingAlley2 { get; } = Find(nameof(BowlingAlley2));
    /// <summary>The <c>Door_WreckedShipEntrance_0</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipEntrance0 { get; } = Find(nameof(WreckedShipEntrance0));
    /// <summary>The <c>Door_WreckedShipEntrance_1</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipEntrance1 { get; } = Find(nameof(WreckedShipEntrance1));
    /// <summary>The <c>Door_Attic_0</c> door.</summary>
    public static CartridgeDoorHeader Attic0 { get; } = Find(nameof(Attic0));
    /// <summary>The <c>Door_Attic_1</c> door.</summary>
    public static CartridgeDoorHeader Attic1 { get; } = Find(nameof(Attic1));
    /// <summary>The <c>Door_Attic_2</c> door.</summary>
    public static CartridgeDoorHeader Attic2 { get; } = Find(nameof(Attic2));
    /// <summary>The <c>Door_AssemblyLine_0</c> door.</summary>
    public static CartridgeDoorHeader AssemblyLine0 { get; } = Find(nameof(AssemblyLine0));
    /// <summary>The <c>Door_WreckedShipMainShaft_0</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipMainShaft0 { get; } = Find(nameof(WreckedShipMainShaft0));
    /// <summary>The <c>Door_WreckedShipMainShaft_1</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipMainShaft1 { get; } = Find(nameof(WreckedShipMainShaft1));
    /// <summary>The <c>Door_WreckedShipMainShaft_2</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipMainShaft2 { get; } = Find(nameof(WreckedShipMainShaft2));
    /// <summary>The <c>Door_WreckedShipMainShaft_3</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipMainShaft3 { get; } = Find(nameof(WreckedShipMainShaft3));
    /// <summary>The <c>Door_WreckedShipMainShaft_4</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipMainShaft4 { get; } = Find(nameof(WreckedShipMainShaft4));
    /// <summary>The <c>Door_WreckedShipMainShaft_5</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipMainShaft5 { get; } = Find(nameof(WreckedShipMainShaft5));
    /// <summary>The <c>Door_WreckedShipMainShaft_6</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipMainShaft6 { get; } = Find(nameof(WreckedShipMainShaft6));
    /// <summary>The <c>Door_SpikyDeath_0</c> door.</summary>
    public static CartridgeDoorHeader SpikyDeath0 { get; } = Find(nameof(SpikyDeath0));
    /// <summary>The <c>Door_SpikyDeath_1</c> door.</summary>
    public static CartridgeDoorHeader SpikyDeath1 { get; } = Find(nameof(SpikyDeath1));
    /// <summary>The <c>Door_ElectricDeath_0</c> door.</summary>
    public static CartridgeDoorHeader ElectricDeath0 { get; } = Find(nameof(ElectricDeath0));
    /// <summary>The <c>Door_ElectricDeath_1</c> door.</summary>
    public static CartridgeDoorHeader ElectricDeath1 { get; } = Find(nameof(ElectricDeath1));
    /// <summary>The <c>Door_ElectricDeath_2</c> door.</summary>
    public static CartridgeDoorHeader ElectricDeath2 { get; } = Find(nameof(ElectricDeath2));
    /// <summary>The <c>Door_WreckedShipETank_0</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipETank0 { get; } = Find(nameof(WreckedShipETank0));
    /// <summary>The <c>Door_Basement_0</c> door.</summary>
    public static CartridgeDoorHeader Basement0 { get; } = Find(nameof(Basement0));
    /// <summary>The <c>Door_Basement_1</c> door.</summary>
    public static CartridgeDoorHeader Basement1 { get; } = Find(nameof(Basement1));
    /// <summary>The <c>Door_Basement_2</c> door.</summary>
    public static CartridgeDoorHeader Basement2 { get; } = Find(nameof(Basement2));
    /// <summary>The <c>Door_WreckedShipMap_0</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipMap0 { get; } = Find(nameof(WreckedShipMap0));
    /// <summary>The <c>Door_Phantoon_0</c> door.</summary>
    public static CartridgeDoorHeader Phantoon0 { get; } = Find(nameof(Phantoon0));
    /// <summary>The <c>Door_SpongeBath_0</c> door.</summary>
    public static CartridgeDoorHeader SpongeBath0 { get; } = Find(nameof(SpongeBath0));
    /// <summary>The <c>Door_SpongeBath_1</c> door.</summary>
    public static CartridgeDoorHeader SpongeBath1 { get; } = Find(nameof(SpongeBath1));
    /// <summary>The <c>Door_WSWestSuper_0</c> door.</summary>
    public static CartridgeDoorHeader WSWestSuper0 { get; } = Find(nameof(WSWestSuper0));
    /// <summary>The <c>Door_WSEastSuper_0</c> door.</summary>
    public static CartridgeDoorHeader WSEastSuper0 { get; } = Find(nameof(WSEastSuper0));
    /// <summary>The <c>Door_GravitySuit_0</c> door.</summary>
    public static CartridgeDoorHeader GravitySuit0 { get; } = Find(nameof(GravitySuit0));
    /// <summary>The <c>Door_GravitySuit_1</c> door.</summary>
    public static CartridgeDoorHeader GravitySuit1 { get; } = Find(nameof(GravitySuit1));
    /// <summary>The <c>Door_WreckedShipSave_0</c> door.</summary>
    public static CartridgeDoorHeader WreckedShipSave0 { get; } = Find(nameof(WreckedShipSave0));
    /// <summary>The <c>Door_GlassTunnelSave_0</c> door.</summary>
    public static CartridgeDoorHeader GlassTunnelSave0 { get; } = Find(nameof(GlassTunnelSave0));
    /// <summary>The <c>Door_GlassTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader GlassTunnel0 { get; } = Find(nameof(GlassTunnel0));
    /// <summary>The <c>Door_GlassTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader GlassTunnel1 { get; } = Find(nameof(GlassTunnel1));
    /// <summary>The <c>Door_GlassTunnel_2</c> door.</summary>
    public static CartridgeDoorHeader GlassTunnel2 { get; } = Find(nameof(GlassTunnel2));
    /// <summary>The <c>Door_GlassTunnel_3</c> door.</summary>
    public static CartridgeDoorHeader GlassTunnel3 { get; } = Find(nameof(GlassTunnel3));
    /// <summary>The <c>Door_WestTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader WestTunnel0 { get; } = Find(nameof(WestTunnel0));
    /// <summary>The <c>Door_WestTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader WestTunnel1 { get; } = Find(nameof(WestTunnel1));
    /// <summary>The <c>Door_EastTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader EastTunnel0 { get; } = Find(nameof(EastTunnel0));
    /// <summary>The <c>Door_EastTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader EastTunnel1 { get; } = Find(nameof(EastTunnel1));
    /// <summary>The <c>Door_EastTunnel_2</c> door.</summary>
    public static CartridgeDoorHeader EastTunnel2 { get; } = Find(nameof(EastTunnel2));
    /// <summary>The <c>Door_MainStreet_0</c> door.</summary>
    public static CartridgeDoorHeader MainStreet0 { get; } = Find(nameof(MainStreet0));
    /// <summary>The <c>Door_MainStreet_1</c> door.</summary>
    public static CartridgeDoorHeader MainStreet1 { get; } = Find(nameof(MainStreet1));
    /// <summary>The <c>Door_MainStreet_2</c> door.</summary>
    public static CartridgeDoorHeader MainStreet2 { get; } = Find(nameof(MainStreet2));
    /// <summary>The <c>Door_MainStreet_3</c> door.</summary>
    public static CartridgeDoorHeader MainStreet3 { get; } = Find(nameof(MainStreet3));
    /// <summary>The <c>Door_MainStreet_4</c> door.</summary>
    public static CartridgeDoorHeader MainStreet4 { get; } = Find(nameof(MainStreet4));
    /// <summary>The <c>Door_FishTank_0</c> door.</summary>
    public static CartridgeDoorHeader FishTank0 { get; } = Find(nameof(FishTank0));
    /// <summary>The <c>Door_FishTank_1</c> door.</summary>
    public static CartridgeDoorHeader FishTank1 { get; } = Find(nameof(FishTank1));
    /// <summary>The <c>Door_FishTank_2</c> door.</summary>
    public static CartridgeDoorHeader FishTank2 { get; } = Find(nameof(FishTank2));
    /// <summary>The <c>Door_FishTank_3</c> door.</summary>
    public static CartridgeDoorHeader FishTank3 { get; } = Find(nameof(FishTank3));
    /// <summary>The <c>Door_MamaTurtle_0</c> door.</summary>
    public static CartridgeDoorHeader MamaTurtle0 { get; } = Find(nameof(MamaTurtle0));
    /// <summary>The <c>Door_CrabTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader CrabTunnel0 { get; } = Find(nameof(CrabTunnel0));
    /// <summary>The <c>Door_CrabTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader CrabTunnel1 { get; } = Find(nameof(CrabTunnel1));
    /// <summary>The <c>Door_MtEverest_0</c> door.</summary>
    public static CartridgeDoorHeader MtEverest0 { get; } = Find(nameof(MtEverest0));
    /// <summary>The <c>Door_MtEverest_1</c> door.</summary>
    public static CartridgeDoorHeader MtEverest1 { get; } = Find(nameof(MtEverest1));
    /// <summary>The <c>Door_MtEverest_2</c> door.</summary>
    public static CartridgeDoorHeader MtEverest2 { get; } = Find(nameof(MtEverest2));
    /// <summary>The <c>Door_MtEverest_3</c> door.</summary>
    public static CartridgeDoorHeader MtEverest3 { get; } = Find(nameof(MtEverest3));
    /// <summary>The <c>Door_MtEverest_4</c> door.</summary>
    public static CartridgeDoorHeader MtEverest4 { get; } = Find(nameof(MtEverest4));
    /// <summary>The <c>Door_MtEverest_5</c> door.</summary>
    public static CartridgeDoorHeader MtEverest5 { get; } = Find(nameof(MtEverest5));
    /// <summary>The <c>Door_RedFish_0</c> door.</summary>
    public static CartridgeDoorHeader RedFish0 { get; } = Find(nameof(RedFish0));
    /// <summary>The <c>Door_RedFish_1</c> door.</summary>
    public static CartridgeDoorHeader RedFish1 { get; } = Find(nameof(RedFish1));
    /// <summary>The <c>Door_WateringHole_0</c> door.</summary>
    public static CartridgeDoorHeader WateringHole0 { get; } = Find(nameof(WateringHole0));
    /// <summary>The <c>Door_NWestMaridiaBug_0</c> door.</summary>
    public static CartridgeDoorHeader NWestMaridiaBug0 { get; } = Find(nameof(NWestMaridiaBug0));
    /// <summary>The <c>Door_NWestMaridiaBug_1</c> door.</summary>
    public static CartridgeDoorHeader NWestMaridiaBug1 { get; } = Find(nameof(NWestMaridiaBug1));
    /// <summary>The <c>Door_CrabShaft_0</c> door.</summary>
    public static CartridgeDoorHeader CrabShaft0 { get; } = Find(nameof(CrabShaft0));
    /// <summary>The <c>Door_CrabShaft_1</c> door.</summary>
    public static CartridgeDoorHeader CrabShaft1 { get; } = Find(nameof(CrabShaft1));
    /// <summary>The <c>Door_CrabShaft_2</c> door.</summary>
    public static CartridgeDoorHeader CrabShaft2 { get; } = Find(nameof(CrabShaft2));
    /// <summary>The <c>Door_PseudoPlasmaSpark_0</c> door.</summary>
    public static CartridgeDoorHeader PseudoPlasmaSpark0 { get; } = Find(nameof(PseudoPlasmaSpark0));
    /// <summary>The <c>Door_PseudoPlasmaSpark_1</c> door.</summary>
    public static CartridgeDoorHeader PseudoPlasmaSpark1 { get; } = Find(nameof(PseudoPlasmaSpark1));
    /// <summary>The <c>Door_PseudoPlasmaSpark_2</c> door.</summary>
    public static CartridgeDoorHeader PseudoPlasmaSpark2 { get; } = Find(nameof(PseudoPlasmaSpark2));
    /// <summary>The <c>Door_CrabHole_0</c> door.</summary>
    public static CartridgeDoorHeader CrabHole0 { get; } = Find(nameof(CrabHole0));
    /// <summary>The <c>Door_CrabHole_1</c> door.</summary>
    public static CartridgeDoorHeader CrabHole1 { get; } = Find(nameof(CrabHole1));
    /// <summary>The <c>Door_CrabHole_2</c> door.</summary>
    public static CartridgeDoorHeader CrabHole2 { get; } = Find(nameof(CrabHole2));
    /// <summary>The <c>Door_CrabHole_3</c> door.</summary>
    public static CartridgeDoorHeader CrabHole3 { get; } = Find(nameof(CrabHole3));
    /// <summary>The <c>Door_WestSandHallTunnel_0</c> door.</summary>
    public static CartridgeDoorHeader WestSandHallTunnel0 { get; } = Find(nameof(WestSandHallTunnel0));
    /// <summary>The <c>Door_WestSandHallTunnel_1</c> door.</summary>
    public static CartridgeDoorHeader WestSandHallTunnel1 { get; } = Find(nameof(WestSandHallTunnel1));
    /// <summary>The <c>Door_PlasmaTutorial_0</c> door.</summary>
    public static CartridgeDoorHeader PlasmaTutorial0 { get; } = Find(nameof(PlasmaTutorial0));
    /// <summary>The <c>Door_PlasmaTutorial_1</c> door.</summary>
    public static CartridgeDoorHeader PlasmaTutorial1 { get; } = Find(nameof(PlasmaTutorial1));
    /// <summary>The <c>Door_Plasma_0</c> door.</summary>
    public static CartridgeDoorHeader Plasma0 { get; } = Find(nameof(Plasma0));
    /// <summary>The <c>Door_ThreadTheNeedle_0</c> door.</summary>
    public static CartridgeDoorHeader ThreadTheNeedle0 { get; } = Find(nameof(ThreadTheNeedle0));
    /// <summary>The <c>Door_ThreadTheNeedle_1</c> door.</summary>
    public static CartridgeDoorHeader ThreadTheNeedle1 { get; } = Find(nameof(ThreadTheNeedle1));
    /// <summary>The <c>Door_MaridiaElev_0</c> door.</summary>
    public static CartridgeDoorHeader MaridiaElev0 { get; } = Find(nameof(MaridiaElev0));
    /// <summary>The <c>Door_MaridiaElev_1</c> door.</summary>
    public static CartridgeDoorHeader MaridiaElev1 { get; } = Find(nameof(MaridiaElev1));
    /// <summary>The <c>Door_MaridiaElev_2</c> door.</summary>
    public static CartridgeDoorHeader MaridiaElev2 { get; } = Find(nameof(MaridiaElev2));
    /// <summary>The <c>Door_PlasmaSpark_0</c> door.</summary>
    public static CartridgeDoorHeader PlasmaSpark0 { get; } = Find(nameof(PlasmaSpark0));
    /// <summary>The <c>Door_PlasmaSpark_1</c> door.</summary>
    public static CartridgeDoorHeader PlasmaSpark1 { get; } = Find(nameof(PlasmaSpark1));
    /// <summary>The <c>Door_PlasmaSpark_2</c> door.</summary>
    public static CartridgeDoorHeader PlasmaSpark2 { get; } = Find(nameof(PlasmaSpark2));
    /// <summary>The <c>Door_PlasmaSpark_3</c> door.</summary>
    public static CartridgeDoorHeader PlasmaSpark3 { get; } = Find(nameof(PlasmaSpark3));
    /// <summary>The <c>Door_Kassiuz_0</c> door.</summary>
    public static CartridgeDoorHeader Kassiuz0 { get; } = Find(nameof(Kassiuz0));
    /// <summary>The <c>Door_Kassiuz_1</c> door.</summary>
    public static CartridgeDoorHeader Kassiuz1 { get; } = Find(nameof(Kassiuz1));
    /// <summary>The <c>Door_MaridiaMap_0</c> door.</summary>
    public static CartridgeDoorHeader MaridiaMap0 { get; } = Find(nameof(MaridiaMap0));
    /// <summary>The <c>Door_ForgottenHighwaySave_0</c> door.</summary>
    public static CartridgeDoorHeader ForgottenHighwaySave0 { get; } = Find(nameof(ForgottenHighwaySave0));
    /// <summary>The <c>Door_Toilet_0</c> door.</summary>
    public static CartridgeDoorHeader Toilet0 { get; } = Find(nameof(Toilet0));
    /// <summary>The <c>Door_Toilet_1</c> door.</summary>
    public static CartridgeDoorHeader Toilet1 { get; } = Find(nameof(Toilet1));
    /// <summary>The <c>Door_BugSandHole_0</c> door.</summary>
    public static CartridgeDoorHeader BugSandHole0 { get; } = Find(nameof(BugSandHole0));
    /// <summary>The <c>Door_BugSandHole_1</c> door.</summary>
    public static CartridgeDoorHeader BugSandHole1 { get; } = Find(nameof(BugSandHole1));
    /// <summary>The <c>Door_BugSandHole_2</c> door.</summary>
    public static CartridgeDoorHeader BugSandHole2 { get; } = Find(nameof(BugSandHole2));
    /// <summary>The <c>Door_WestSandHall_0</c> door.</summary>
    public static CartridgeDoorHeader WestSandHall0 { get; } = Find(nameof(WestSandHall0));
    /// <summary>The <c>Door_WestSandHall_1</c> door.</summary>
    public static CartridgeDoorHeader WestSandHall1 { get; } = Find(nameof(WestSandHall1));
    /// <summary>The <c>UNUSED_Door_WestSandHall_2_83A654</c> door.</summary>
    public static CartridgeDoorHeader UnusedWestSandHall283A654 { get; } = Find(nameof(UnusedWestSandHall283A654));
    /// <summary>The <c>Door_Oasis_0</c> door.</summary>
    public static CartridgeDoorHeader Oasis0 { get; } = Find(nameof(Oasis0));
    /// <summary>The <c>Door_Oasis_1</c> door.</summary>
    public static CartridgeDoorHeader Oasis1 { get; } = Find(nameof(Oasis1));
    /// <summary>The <c>Door_Oasis_2</c> door.</summary>
    public static CartridgeDoorHeader Oasis2 { get; } = Find(nameof(Oasis2));
    /// <summary>The <c>Door_EastSandHall_0</c> door.</summary>
    public static CartridgeDoorHeader EastSandHall0 { get; } = Find(nameof(EastSandHall0));
    /// <summary>The <c>Door_EastSandHall_1</c> door.</summary>
    public static CartridgeDoorHeader EastSandHall1 { get; } = Find(nameof(EastSandHall1));
    /// <summary>The <c>UNUSED_Door_EastSandHall_2_83A69C</c> door.</summary>
    public static CartridgeDoorHeader UnusedEastSandHall283A69C { get; } = Find(nameof(UnusedEastSandHall283A69C));
    /// <summary>The <c>Door_WestSandHole_0</c> door.</summary>
    public static CartridgeDoorHeader WestSandHole0 { get; } = Find(nameof(WestSandHole0));
    /// <summary>The <c>Door_WestSandHole_1</c> door.</summary>
    public static CartridgeDoorHeader WestSandHole1 { get; } = Find(nameof(WestSandHole1));
    /// <summary>The <c>Door_EastSandHole_0</c> door.</summary>
    public static CartridgeDoorHeader EastSandHole0 { get; } = Find(nameof(EastSandHole0));
    /// <summary>The <c>Door_EastSandHole_1</c> door.</summary>
    public static CartridgeDoorHeader EastSandHole1 { get; } = Find(nameof(EastSandHole1));
    /// <summary>The <c>Door_WestAqueductQuicksand_0</c> door.</summary>
    public static CartridgeDoorHeader WestAqueductQuicksand0 { get; } = Find(nameof(WestAqueductQuicksand0));
    /// <summary>The <c>Door_WestAqueductQuicksand_1</c> door.</summary>
    public static CartridgeDoorHeader WestAqueductQuicksand1 { get; } = Find(nameof(WestAqueductQuicksand1));
    /// <summary>The <c>Door_EastAqueductQuicksand_0</c> door.</summary>
    public static CartridgeDoorHeader EastAqueductQuicksand0 { get; } = Find(nameof(EastAqueductQuicksand0));
    /// <summary>The <c>Door_EastAqueductQuicksand_1</c> door.</summary>
    public static CartridgeDoorHeader EastAqueductQuicksand1 { get; } = Find(nameof(EastAqueductQuicksand1));
    /// <summary>The <c>Door_Aqueduct_0</c> door.</summary>
    public static CartridgeDoorHeader Aqueduct0 { get; } = Find(nameof(Aqueduct0));
    /// <summary>The <c>Door_Aqueduct_1</c> door.</summary>
    public static CartridgeDoorHeader Aqueduct1 { get; } = Find(nameof(Aqueduct1));
    /// <summary>The <c>Door_Aqueduct_2</c> door.</summary>
    public static CartridgeDoorHeader Aqueduct2 { get; } = Find(nameof(Aqueduct2));
    /// <summary>The <c>Door_Aqueduct_3</c> door.</summary>
    public static CartridgeDoorHeader Aqueduct3 { get; } = Find(nameof(Aqueduct3));
    /// <summary>The <c>Door_Aqueduct_4</c> door.</summary>
    public static CartridgeDoorHeader Aqueduct4 { get; } = Find(nameof(Aqueduct4));
    /// <summary>The <c>Door_Aqueduct_5</c> door.</summary>
    public static CartridgeDoorHeader Aqueduct5 { get; } = Find(nameof(Aqueduct5));
    /// <summary>The <c>Door_Butterfly_0</c> door.</summary>
    public static CartridgeDoorHeader Butterfly0 { get; } = Find(nameof(Butterfly0));
    /// <summary>The <c>Door_Butterfly_1</c> door.</summary>
    public static CartridgeDoorHeader Butterfly1 { get; } = Find(nameof(Butterfly1));
    /// <summary>The <c>Door_BotwoonHallway_0</c> door.</summary>
    public static CartridgeDoorHeader BotwoonHallway0 { get; } = Find(nameof(BotwoonHallway0));
    /// <summary>The <c>Door_BotwoonHallway_1</c> door.</summary>
    public static CartridgeDoorHeader BotwoonHallway1 { get; } = Find(nameof(BotwoonHallway1));
    /// <summary>The <c>Door_Pants_0</c> door.</summary>
    public static CartridgeDoorHeader Pants0 { get; } = Find(nameof(Pants0));
    /// <summary>The <c>Door_Pants_1</c> door.</summary>
    public static CartridgeDoorHeader Pants1 { get; } = Find(nameof(Pants1));
    /// <summary>The <c>Door_Pants_2</c> door.</summary>
    public static CartridgeDoorHeader Pants2 { get; } = Find(nameof(Pants2));
    /// <summary>The <c>Door_Pants_3</c> door.</summary>
    public static CartridgeDoorHeader Pants3 { get; } = Find(nameof(Pants3));
    /// <summary>The <c>Door_EastPants_0</c> door.</summary>
    public static CartridgeDoorHeader EastPants0 { get; } = Find(nameof(EastPants0));
    /// <summary>The <c>Door_EastPants_1</c> door.</summary>
    public static CartridgeDoorHeader EastPants1 { get; } = Find(nameof(EastPants1));
    /// <summary>The <c>Door_Springball_0</c> door.</summary>
    public static CartridgeDoorHeader Springball0 { get; } = Find(nameof(Springball0));
    /// <summary>The <c>Door_BelowBotwoonETank_0</c> door.</summary>
    public static CartridgeDoorHeader BelowBotwoonETank0 { get; } = Find(nameof(BelowBotwoonETank0));
    /// <summary>The <c>Door_Colosseum_0</c> door.</summary>
    public static CartridgeDoorHeader Colosseum0 { get; } = Find(nameof(Colosseum0));
    /// <summary>The <c>Door_Colosseum_1</c> door.</summary>
    public static CartridgeDoorHeader Colosseum1 { get; } = Find(nameof(Colosseum1));
    /// <summary>The <c>Door_Colosseum_2</c> door.</summary>
    public static CartridgeDoorHeader Colosseum2 { get; } = Find(nameof(Colosseum2));
    /// <summary>The <c>Door_ThePrecious_MaridiaLoad11</c> door.</summary>
    public static CartridgeDoorHeader ThePreciousMaridiaLoad11 { get; } = Find(nameof(ThePreciousMaridiaLoad11));
    /// <summary>The <c>Door_AqueductSave_0</c> door.</summary>
    public static CartridgeDoorHeader AqueductSave0 { get; } = Find(nameof(AqueductSave0));
    /// <summary>The <c>Door_ThePrecious_0</c> door.</summary>
    public static CartridgeDoorHeader ThePrecious0 { get; } = Find(nameof(ThePrecious0));
    /// <summary>The <c>Door_ThePrecious_1</c> door.</summary>
    public static CartridgeDoorHeader ThePrecious1 { get; } = Find(nameof(ThePrecious1));
    /// <summary>The <c>Door_BotwoonETank_0</c> door.</summary>
    public static CartridgeDoorHeader BotwoonETank0 { get; } = Find(nameof(BotwoonETank0));
    /// <summary>The <c>Door_BotwoonETank_1</c> door.</summary>
    public static CartridgeDoorHeader BotwoonETank1 { get; } = Find(nameof(BotwoonETank1));
    /// <summary>The <c>Door_BotwoonETank_2</c> door.</summary>
    public static CartridgeDoorHeader BotwoonETank2 { get; } = Find(nameof(BotwoonETank2));
    /// <summary>The <c>Door_BotwoonETank_3</c> door.</summary>
    public static CartridgeDoorHeader BotwoonETank3 { get; } = Find(nameof(BotwoonETank3));
    /// <summary>The <c>Door_DraygonSave_0</c> door.</summary>
    public static CartridgeDoorHeader DraygonSave0 { get; } = Find(nameof(DraygonSave0));
    /// <summary>The <c>Door_DraygonSave_1</c> door.</summary>
    public static CartridgeDoorHeader DraygonSave1 { get; } = Find(nameof(DraygonSave1));
    /// <summary>The <c>Door_MaridiaMissileRefill_0</c> door.</summary>
    public static CartridgeDoorHeader MaridiaMissileRefill0 { get; } = Find(nameof(MaridiaMissileRefill0));
    /// <summary>The <c>Door_PlasmaBeachQuicksand_0</c> door.</summary>
    public static CartridgeDoorHeader PlasmaBeachQuicksand0 { get; } = Find(nameof(PlasmaBeachQuicksand0));
    /// <summary>The <c>Door_BotwoonQuicksand_0</c> door.</summary>
    public static CartridgeDoorHeader BotwoonQuicksand0 { get; } = Find(nameof(BotwoonQuicksand0));
    /// <summary>The <c>Door_BotwoonQuicksand_1</c> door.</summary>
    public static CartridgeDoorHeader BotwoonQuicksand1 { get; } = Find(nameof(BotwoonQuicksand1));
    /// <summary>The <c>Door_Shaktool_0</c> door.</summary>
    public static CartridgeDoorHeader Shaktool0 { get; } = Find(nameof(Shaktool0));
    /// <summary>The <c>Door_Shaktool_1</c> door.</summary>
    public static CartridgeDoorHeader Shaktool1 { get; } = Find(nameof(Shaktool1));
    /// <summary>The <c>Door_HalfieClimb_0</c> door.</summary>
    public static CartridgeDoorHeader HalfieClimb0 { get; } = Find(nameof(HalfieClimb0));
    /// <summary>The <c>Door_HalfieClimb_1</c> door.</summary>
    public static CartridgeDoorHeader HalfieClimb1 { get; } = Find(nameof(HalfieClimb1));
    /// <summary>The <c>Door_HalfieClimb_2</c> door.</summary>
    public static CartridgeDoorHeader HalfieClimb2 { get; } = Find(nameof(HalfieClimb2));
    /// <summary>The <c>Door_HalfieClimb_3</c> door.</summary>
    public static CartridgeDoorHeader HalfieClimb3 { get; } = Find(nameof(HalfieClimb3));
    /// <summary>The <c>Door_Botwoon_0</c> door.</summary>
    public static CartridgeDoorHeader Botwoon0 { get; } = Find(nameof(Botwoon0));
    /// <summary>The <c>Door_Botwoon_1</c> door.</summary>
    public static CartridgeDoorHeader Botwoon1 { get; } = Find(nameof(Botwoon1));
    /// <summary>The <c>Door_SpaceJump_0</c> door.</summary>
    public static CartridgeDoorHeader SpaceJump0 { get; } = Find(nameof(SpaceJump0));
    /// <summary>The <c>Door_MaridiaEnergyRefill_0</c> door.</summary>
    public static CartridgeDoorHeader MaridiaEnergyRefill0 { get; } = Find(nameof(MaridiaEnergyRefill0));
    /// <summary>The <c>Door_WestCactusAlley_0</c> door.</summary>
    public static CartridgeDoorHeader WestCactusAlley0 { get; } = Find(nameof(WestCactusAlley0));
    /// <summary>The <c>Door_WestCactusAlley_1</c> door.</summary>
    public static CartridgeDoorHeader WestCactusAlley1 { get; } = Find(nameof(WestCactusAlley1));
    /// <summary>The <c>Door_EastCactusAlley_0</c> door.</summary>
    public static CartridgeDoorHeader EastCactusAlley0 { get; } = Find(nameof(EastCactusAlley0));
    /// <summary>The <c>Door_EastCactusAlley_1</c> door.</summary>
    public static CartridgeDoorHeader EastCactusAlley1 { get; } = Find(nameof(EastCactusAlley1));
    /// <summary>The <c>Door_Draygon_0</c> door.</summary>
    public static CartridgeDoorHeader Draygon0 { get; } = Find(nameof(Draygon0));
    /// <summary>The <c>Door_Draygon_1</c> door.</summary>
    public static CartridgeDoorHeader Draygon1 { get; } = Find(nameof(Draygon1));
    /// <summary>The <c>Door_TourianFirst_0</c> door.</summary>
    public static CartridgeDoorHeader TourianFirst0 { get; } = Find(nameof(TourianFirst0));
    /// <summary>The <c>Door_TourianFirst_1</c> door.</summary>
    public static CartridgeDoorHeader TourianFirst1 { get; } = Find(nameof(TourianFirst1));
    /// <summary>The <c>Door_TourianFirst_3</c> door.</summary>
    public static CartridgeDoorHeader TourianFirst3 { get; } = Find(nameof(TourianFirst3));
    /// <summary>The <c>Door_Metroids1_0</c> door.</summary>
    public static CartridgeDoorHeader Metroids10 { get; } = Find(nameof(Metroids10));
    /// <summary>The <c>Door_Metroids1_1</c> door.</summary>
    public static CartridgeDoorHeader Metroids11 { get; } = Find(nameof(Metroids11));
    /// <summary>The <c>Door_Metroids2_0</c> door.</summary>
    public static CartridgeDoorHeader Metroids20 { get; } = Find(nameof(Metroids20));
    /// <summary>The <c>Door_Metroids2_1</c> door.</summary>
    public static CartridgeDoorHeader Metroids21 { get; } = Find(nameof(Metroids21));
    /// <summary>The <c>Door_Metroids3_0</c> door.</summary>
    public static CartridgeDoorHeader Metroids30 { get; } = Find(nameof(Metroids30));
    /// <summary>The <c>Door_Metroids3_1</c> door.</summary>
    public static CartridgeDoorHeader Metroids31 { get; } = Find(nameof(Metroids31));
    /// <summary>The <c>Door_Metroids4_0</c> door.</summary>
    public static CartridgeDoorHeader Metroids40 { get; } = Find(nameof(Metroids40));
    /// <summary>The <c>Door_Metroids4_1</c> door.</summary>
    public static CartridgeDoorHeader Metroids41 { get; } = Find(nameof(Metroids41));
    /// <summary>The <c>Door_BlueHopper_0</c> door.</summary>
    public static CartridgeDoorHeader BlueHopper0 { get; } = Find(nameof(BlueHopper0));
    /// <summary>The <c>Door_BlueHopper_1</c> door.</summary>
    public static CartridgeDoorHeader BlueHopper1 { get; } = Find(nameof(BlueHopper1));
    /// <summary>The <c>Door_DustTorizo_0</c> door.</summary>
    public static CartridgeDoorHeader DustTorizo0 { get; } = Find(nameof(DustTorizo0));
    /// <summary>The <c>Door_DustTorizo_1</c> door.</summary>
    public static CartridgeDoorHeader DustTorizo1 { get; } = Find(nameof(DustTorizo1));
    /// <summary>The <c>Door_BigBoy_0</c> door.</summary>
    public static CartridgeDoorHeader BigBoy0 { get; } = Find(nameof(BigBoy0));
    /// <summary>The <c>Door_BigBoy_1</c> door.</summary>
    public static CartridgeDoorHeader BigBoy1 { get; } = Find(nameof(BigBoy1));
    /// <summary>The <c>Door_Seaweed_0</c> door.</summary>
    public static CartridgeDoorHeader Seaweed0 { get; } = Find(nameof(Seaweed0));
    /// <summary>The <c>Door_Seaweed_1</c> door.</summary>
    public static CartridgeDoorHeader Seaweed1 { get; } = Find(nameof(Seaweed1));
    /// <summary>The <c>Door_Seaweed_2</c> door.</summary>
    public static CartridgeDoorHeader Seaweed2 { get; } = Find(nameof(Seaweed2));
    /// <summary>The <c>Door_TourianRecharge_0</c> door.</summary>
    public static CartridgeDoorHeader TourianRecharge0 { get; } = Find(nameof(TourianRecharge0));
    /// <summary>The <c>Door_MotherBrain_0</c> door.</summary>
    public static CartridgeDoorHeader MotherBrain0 { get; } = Find(nameof(MotherBrain0));
    /// <summary>The <c>Door_MotherBrain_1</c> door.</summary>
    public static CartridgeDoorHeader MotherBrain1 { get; } = Find(nameof(MotherBrain1));
    /// <summary>The <c>Door_TourianEyeDoor_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEyeDoor0 { get; } = Find(nameof(TourianEyeDoor0));
    /// <summary>The <c>Door_TourianEyeDoor_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEyeDoor1 { get; } = Find(nameof(TourianEyeDoor1));
    /// <summary>The <c>Door_RinkaShaft_0</c> door.</summary>
    public static CartridgeDoorHeader RinkaShaft0 { get; } = Find(nameof(RinkaShaft0));
    /// <summary>The <c>Door_RinkaShaft_1</c> door.</summary>
    public static CartridgeDoorHeader RinkaShaft1 { get; } = Find(nameof(RinkaShaft1));
    /// <summary>The <c>Door_RinkaShaft_2</c> door.</summary>
    public static CartridgeDoorHeader RinkaShaft2 { get; } = Find(nameof(RinkaShaft2));
    /// <summary>The <c>Door_MotherBrainSave_0</c> door.</summary>
    public static CartridgeDoorHeader MotherBrainSave0 { get; } = Find(nameof(MotherBrainSave0));
    /// <summary>The <c>Door_TourianEscape1_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape10 { get; } = Find(nameof(TourianEscape10));
    /// <summary>The <c>Door_TourianEscape1_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape11 { get; } = Find(nameof(TourianEscape11));
    /// <summary>The <c>Door_TourianEscape2_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape20 { get; } = Find(nameof(TourianEscape20));
    /// <summary>The <c>Door_TourianEscape2_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape21 { get; } = Find(nameof(TourianEscape21));
    /// <summary>The <c>Door_TourianEscape3_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape30 { get; } = Find(nameof(TourianEscape30));
    /// <summary>The <c>Door_TourianEscape3_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape31 { get; } = Find(nameof(TourianEscape31));
    /// <summary>The <c>Door_TourianEscape4_0</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape40 { get; } = Find(nameof(TourianEscape40));
    /// <summary>The <c>Door_TourianEscape4_1</c> door.</summary>
    public static CartridgeDoorHeader TourianEscape41 { get; } = Find(nameof(TourianEscape41));
    /// <summary>The <c>Door_UpperTourianSave_0</c> door.</summary>
    public static CartridgeDoorHeader UpperTourianSave0 { get; } = Find(nameof(UpperTourianSave0));
    /// <summary>The <c>Door_CeresElev_0</c> door.</summary>
    public static CartridgeDoorHeader CeresElev0 { get; } = Find(nameof(CeresElev0));
    /// <summary>The <c>Door_FallingTile_0</c> door.</summary>
    public static CartridgeDoorHeader FallingTile0 { get; } = Find(nameof(FallingTile0));
    /// <summary>The <c>Door_FallingTile_1</c> door.</summary>
    public static CartridgeDoorHeader FallingTile1 { get; } = Find(nameof(FallingTile1));
    /// <summary>The <c>Door_MagnetStairs_0</c> door.</summary>
    public static CartridgeDoorHeader MagnetStairs0 { get; } = Find(nameof(MagnetStairs0));
    /// <summary>The <c>Door_MagnetStairs_1</c> door.</summary>
    public static CartridgeDoorHeader MagnetStairs1 { get; } = Find(nameof(MagnetStairs1));
    /// <summary>The <c>Door_DeadScientist_0</c> door.</summary>
    public static CartridgeDoorHeader DeadScientist0 { get; } = Find(nameof(DeadScientist0));
    /// <summary>The <c>Door_DeadScientist_1</c> door.</summary>
    public static CartridgeDoorHeader DeadScientist1 { get; } = Find(nameof(DeadScientist1));
    /// <summary>The <c>Door_58Escape_0</c> door.</summary>
    public static CartridgeDoorHeader Room58Escape0 { get; } = Find(nameof(Room58Escape0));
    /// <summary>The <c>Door_58Escape_1</c> door.</summary>
    public static CartridgeDoorHeader Room58Escape1 { get; } = Find(nameof(Room58Escape1));
    /// <summary>The <c>Door_CeresRidley</c> door.</summary>
    public static CartridgeDoorHeader CeresRidley { get; } = Find(nameof(CeresRidley));
}
