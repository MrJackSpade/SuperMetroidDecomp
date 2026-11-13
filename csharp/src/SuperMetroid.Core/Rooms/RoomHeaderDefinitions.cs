using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>The fixed configuration of the 262 non-debug retail rooms.</summary>
/// <remarks>Each entry holds a room's room/area indexes, map position, dimensions,
/// scrolling thresholds and whether its door transitions are isolated, as the room's fixed metadata defines
/// them. The developer area-seven room is not compiled.</remarks>
[NumberedIdentities("Rooms such as Metroids1-4 and TourianEscape1-4 are distinct rooms with their own headers; the number is part of each room's name, not a position callers index.")]
public static class RoomHeaderDefinitions
{
    /// <summary>Retail rooms in their authored order.</summary>
    internal static readonly RoomHeaderDefinition[] Retail =
    [
        new(nameof(LandingSite), 0, AreaId.Crateria, new(23, 0), new(9, 5), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GauntletEntrance), 1, AreaId.Crateria, new(18, 2), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Parlor), 2, AreaId.Crateria, new(18, 4), new(5, 5), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaPowerBombs), 3, AreaId.Crateria, new(32, 1), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaSave), 4, AreaId.Crateria, new(18, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestOcean), 5, AreaId.Crateria, new(38, 0), new(8, 6), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BowlingAlleyPath), 6, AreaId.Crateria, new(41, 2), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaKihunter), 7, AreaId.Crateria, new(33, 4), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ForgottenHighwayElev), 8, AreaId.Crateria, new(52, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastOcean), 9, AreaId.Crateria, new(49, 0), new(7, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ForgottenHighwayKagos), 10, AreaId.Crateria, new(56, 4), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrabMaze), 11, AreaId.Crateria, new(53, 8), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ForgottenHighwayElbow), 12, AreaId.Crateria, new(52, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaTube), 13, AreaId.Crateria, new(32, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Moat), 14, AreaId.Crateria, new(36, 4), new(2, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedBinstarElev), 15, AreaId.Crateria, new(34, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GauntletETank), 16, AreaId.Crateria, new(12, 2), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PreBowling), 17, AreaId.Crateria, new(43, 2), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Climb), 18, AreaId.Crateria, new(18, 9), new(3, 9), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Pit), 19, AreaId.Crateria, new(20, 17), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ElevToBlueBrinstar), 20, AreaId.Crateria, new(23, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BombTorizo), 21, AreaId.Crateria, new(25, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Flyway), 22, AreaId.Crateria, new(22, 6), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PreMapFlyway), 23, AreaId.Crateria, new(20, 7), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Terminator), 24, AreaId.Crateria, new(12, 4), new(6, 3), 160, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarElev), 25, AreaId.Crateria, new(6, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LowerMushrooms), 26, AreaId.Crateria, new(7, 8), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaMap), 27, AreaId.Crateria, new(23, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenPiratesShaft), 28, AreaId.Crateria, new(11, 2), new(1, 7), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaSuper), 29, AreaId.Crateria, new(21, 9), new(4, 8), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FinalMissileBombway), 30, AreaId.Crateria, new(17, 7), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FinalMissile), 31, AreaId.Crateria, new(16, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarMainShaft), 0, AreaId.Brinstar, new(9, 0), new(4, 12), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SporeSpawnSuper), 1, AreaId.Brinstar, new(23, 1), new(2, 9), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BrinstarPreMap), 2, AreaId.Brinstar, new(6, 4), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EarlySupers), 3, AreaId.Brinstar, new(10, 3), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BrinstarReserveTank), 4, AreaId.Brinstar, new(13, 4), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarMap), 5, AreaId.Brinstar, new(5, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarFirefleas), 6, AreaId.Brinstar, new(6, 6), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarMissileRefill), 7, AreaId.Brinstar, new(5, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Dachora), 8, AreaId.Brinstar, new(10, 6), new(7, 7), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BigPink), 9, AreaId.Brinstar, new(15, 4), new(5, 10), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SporeSpawnKihunters), 10, AreaId.Brinstar, new(19, 4), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SporeSpawn), 11, AreaId.Brinstar, new(22, 1), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PinkBrinstarPowerBombs), 12, AreaId.Brinstar, new(15, 7), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenHillZone), 13, AreaId.Brinstar, new(19, 10), new(8, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MorphBall), 14, AreaId.Brinstar, new(21, 8), new(8, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ConstructionZone), 15, AreaId.Brinstar, new(29, 10), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BlueBrinstarETank), 16, AreaId.Brinstar, new(30, 8), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(NoobBridge), 17, AreaId.Brinstar, new(27, 13), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarBeetoms), 18, AreaId.Brinstar, new(8, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EtecoonETank), 19, AreaId.Brinstar, new(6, 10), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EtecoonSuper), 20, AreaId.Brinstar, new(5, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DachoraEnergyRefill), 21, AreaId.Brinstar, new(9, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SporeSpawnFarming), 22, AreaId.Brinstar, new(20, 9), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WaterwayETank), 23, AreaId.Brinstar, new(8, 13), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FirstMissile), 24, AreaId.Brinstar, new(28, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PinkBrinstarHoppers), 25, AreaId.Brinstar, new(19, 7), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(HopperETank), 26, AreaId.Brinstar, new(21, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BigPinkSaveRoom), 27, AreaId.Brinstar, new(15, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BlueBrinstarBoulders), 28, AreaId.Brinstar, new(30, 8), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BillyMays), 29, AreaId.Brinstar, new(29, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarSave), 30, AreaId.Brinstar, new(8, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EtecoonSave), 31, AreaId.Brinstar, new(5, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedTower), 32, AreaId.Brinstar, new(33, 9), new(1, 10), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedBrinstarFirefleas), 33, AreaId.Brinstar, new(25, 15), new(8, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(XrayScope), 34, AreaId.Brinstar, new(23, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Hellway), 35, AreaId.Brinstar, new(34, 9), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Caterpillar), 36, AreaId.Brinstar, new(37, 4), new(3, 8), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BetaPowerBomb), 37, AreaId.Brinstar, new(35, 7), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(AlphaPowerBomb), 38, AreaId.Brinstar, new(34, 11), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SkreeBoost), 39, AreaId.Brinstar, new(34, 18), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BelowSpazer), 40, AreaId.Brinstar, new(36, 17), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Spazer), 41, AreaId.Brinstar, new(38, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseZeela), 42, AreaId.Brinstar, new(44, 18), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseETank), 43, AreaId.Brinstar, new(43, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseKihunter), 44, AreaId.Brinstar, new(45, 18), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MiniKraid), 45, AreaId.Brinstar, new(47, 19), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(KraidEyeDoor), 46, AreaId.Brinstar, new(53, 18), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Kraid), 47, AreaId.Brinstar, new(55, 18), new(2, 2), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
        new(nameof(StatuesHallway), 48, AreaId.Crateria, new(12, 8), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedTowerRefill), 49, AreaId.Brinstar, new(32, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(KraidRefill), 50, AreaId.Brinstar, new(54, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Statues), 51, AreaId.Crateria, new(17, 8), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseEntrance), 52, AreaId.Brinstar, new(41, 18), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(VariaSuit), 53, AreaId.Brinstar, new(57, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseSave), 54, AreaId.Brinstar, new(49, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedBrinstarSave), 55, AreaId.Brinstar, new(38, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeamAcid), 0, AreaId.Norfair, new(4, 3), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Cathedral), 1, AreaId.Norfair, new(14, 3), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CathedralEntrance), 2, AreaId.Norfair, new(11, 3), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BusinessCenter), 3, AreaId.Norfair, new(10, 0), new(1, 7), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeamGate), 4, AreaId.Norfair, new(3, 1), new(7, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeamTutorial), 5, AreaId.Norfair, new(4, 1), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeam), 6, AreaId.Norfair, new(5, 2), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeamSnake), 7, AreaId.Norfair, new(3, 1), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrumbleShaft), 8, AreaId.Norfair, new(2, 4), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrocomireSpeedway), 9, AreaId.Norfair, new(3, 7), new(13, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Crocomire), 10, AreaId.Norfair, new(12, 10), new(8, 1), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
        new(nameof(HiJumpBoots), 11, AreaId.Norfair, new(7, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrocomireEscape), 12, AreaId.Norfair, new(11, 6), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(HiJumpETank), 13, AreaId.Norfair, new(8, 5), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocFarming), 14, AreaId.Norfair, new(10, 10), new(2, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocSave), 15, AreaId.Norfair, new(12, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocPowerBombs), 16, AreaId.Norfair, new(9, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocShaft), 17, AreaId.Norfair, new(10, 12), new(1, 5), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocMissile), 18, AreaId.Norfair, new(11, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GrappleTutorial3), 19, AreaId.Norfair, new(7, 12), new(3, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocJump), 20, AreaId.Norfair, new(4, 15), new(8, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GrappleTutorial2), 21, AreaId.Norfair, new(6, 12), new(1, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GrappleTutorial1), 22, AreaId.Norfair, new(4, 14), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GrappleBeam), 23, AreaId.Norfair, new(3, 14), new(1, 4), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(NorfairReserveTank), 24, AreaId.Norfair, new(18, 2), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBubblesMissiles), 25, AreaId.Norfair, new(20, 2), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BubbleMountain), 26, AreaId.Norfair, new(22, 2), new(2, 4), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpeedBoosterHall), 27, AreaId.Norfair, new(25, 1), new(12, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpeedBooster), 28, AreaId.Norfair, new(37, 2), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SingleChamber), 29, AreaId.Norfair, new(24, 3), new(6, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DoubleChamber), 30, AreaId.Norfair, new(25, 4), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WaveBeam), 31, AreaId.Norfair, new(29, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpikyPlatformsTunnel), 32, AreaId.Norfair, new(25, 6), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Volcano), 33, AreaId.Norfair, new(27, 6), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(KronicBoost), 34, AreaId.Norfair, new(25, 8), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MagdolliteTunnel), 35, AreaId.Norfair, new(23, 8), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PurpleShaft), 36, AreaId.Norfair, new(22, 6), new(1, 3), 160, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LavaDive), 37, AreaId.Norfair, new(22, 10), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LowerNorfairElev), 38, AreaId.Norfair, new(21, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(UpperNorfairFarming), 39, AreaId.Norfair, new(20, 5), new(2, 2), 160, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RisingTide), 40, AreaId.Norfair, new(17, 4), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(AcidSnakesTunnel), 41, AreaId.Norfair, new(16, 9), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpikyAcidSnakesTunnel), 42, AreaId.Norfair, new(21, 9), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(UpperNorfairRefill), 43, AreaId.Norfair, new(20, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PurpleFarming), 44, AreaId.Norfair, new(23, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BatCave), 45, AreaId.Norfair, new(24, 1), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(NorfairMap), 46, AreaId.Norfair, new(9, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BubbleMountainSave), 47, AreaId.Norfair, new(21, 3), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FrogSpeedway), 48, AreaId.Norfair, new(12, 5), new(8, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedPirateShaft), 49, AreaId.Norfair, new(19, 6), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FrogSave), 50, AreaId.Norfair, new(11, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrocomireSave), 51, AreaId.Norfair, new(16, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNElevSave), 52, AreaId.Norfair, new(20, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(AcidStatue), 53, AreaId.Norfair, new(15, 13), new(3, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MainHall), 54, AreaId.Norfair, new(17, 11), new(8, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GoldenTorizo), 55, AreaId.Norfair, new(18, 15), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FastRipper), 56, AreaId.Norfair, new(21, 14), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GTEnergyRefill), 57, AreaId.Norfair, new(21, 15), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Ridley), 58, AreaId.Norfair, new(23, 16), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNFarming), 59, AreaId.Norfair, new(24, 16), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FastPillarsSetup), 60, AreaId.Norfair, new(25, 12), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(UnusedNorfairRoom), 61, AreaId.Norfair, new(26, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MickeyMouse), 62, AreaId.Norfair, new(26, 9), new(4, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Pillar), 63, AreaId.Norfair, new(26, 14), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Plowerhouse), 64, AreaId.Norfair, new(27, 16), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WorstRoomInTheGame), 65, AreaId.Norfair, new(30, 9), new(1, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Amphitheatre), 66, AreaId.Norfair, new(31, 9), new(4, 5), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNSpringBallMaze), 67, AreaId.Norfair, new(33, 5), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNEscapePowerBombs), 68, AreaId.Norfair, new(37, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedKihunterShaft), 69, AreaId.Norfair, new(35, 9), new(3, 5), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Wasteland), 70, AreaId.Norfair, new(32, 14), new(6, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MetalPirates), 71, AreaId.Norfair, new(30, 16), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ThreeMusketeers), 72, AreaId.Norfair, new(29, 3), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RidleyETank), 73, AreaId.Norfair, new(22, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ScrewAttack), 74, AreaId.Norfair, new(20, 14), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNFireflea), 75, AreaId.Norfair, new(35, 6), new(3, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNSave), 76, AreaId.Norfair, new(36, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BowlingAlley), 0, AreaId.WreckedShip, new(10, 11), new(6, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipEntrance), 1, AreaId.WreckedShip, new(12, 14), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Attic), 2, AreaId.WreckedShip, new(12, 10), new(7, 1), 0, 0, RoomDoorTransitionStyle.Standard),
        new(nameof(AssemblyLine), 3, AreaId.WreckedShip, new(19, 10), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipMainShaft), 4, AreaId.WreckedShip, new(12, 11), new(6, 8), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpikyDeath), 5, AreaId.WreckedShip, new(19, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ElectricDeath), 6, AreaId.WreckedShip, new(21, 13), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipETank), 7, AreaId.WreckedShip, new(18, 13), new(3, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Basement), 8, AreaId.WreckedShip, new(14, 19), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipMap), 9, AreaId.WreckedShip, new(13, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Phantoon), 10, AreaId.WreckedShip, new(19, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpongeBath), 11, AreaId.WreckedShip, new(17, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WSWestSuper), 12, AreaId.WreckedShip, new(15, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WSEastSuper), 13, AreaId.WreckedShip, new(18, 17), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GravitySuit), 14, AreaId.WreckedShip, new(10, 13), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipSave), 15, AreaId.WreckedShip, new(17, 14), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GlassTunnelSave), 0, AreaId.Maridia, new(12, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GlassTunnel), 1, AreaId.Maridia, new(11, 17), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestTunnel), 2, AreaId.Maridia, new(10, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastTunnel), 3, AreaId.Maridia, new(12, 17), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MainStreet), 4, AreaId.Maridia, new(10, 9), new(3, 8), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FishTank), 5, AreaId.Maridia, new(13, 13), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MamaTurtle), 6, AreaId.Maridia, new(17, 12), new(3, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrabTunnel), 7, AreaId.Maridia, new(12, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MtEverest), 8, AreaId.Maridia, new(12, 9), new(6, 4), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedFish), 9, AreaId.Maridia, new(12, 7), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WateringHole), 10, AreaId.Maridia, new(12, 4), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(NWestMaridiaBug), 11, AreaId.Maridia, new(14, 4), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrabShaft), 12, AreaId.Maridia, new(18, 7), new(2, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PseudoPlasmaSpark), 13, AreaId.Maridia, new(18, 4), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrabHole), 14, AreaId.Maridia, new(16, 16), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestSandHallTunnel), 15, AreaId.Maridia, new(17, 16), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PlasmaTutorial), 16, AreaId.Maridia, new(26, 0), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Plasma), 17, AreaId.Maridia, new(27, 0), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ThreadTheNeedle), 18, AreaId.Maridia, new(27, 5), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MaridiaElev), 19, AreaId.Maridia, new(34, 0), new(1, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PlasmaSpark), 20, AreaId.Maridia, new(22, 2), new(4, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Kassiuz), 21, AreaId.Maridia, new(25, 0), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MaridiaMap), 22, AreaId.Maridia, new(17, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ForgottenHighwaySave), 23, AreaId.Maridia, new(35, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Toilet), 24, AreaId.Maridia, new(22, 5), new(1, 10), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BugSandHole), 25, AreaId.Maridia, new(26, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestSandHall), 26, AreaId.Maridia, new(18, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Oasis), 27, AreaId.Maridia, new(22, 15), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastSandHall), 28, AreaId.Maridia, new(23, 16), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestSandHole), 29, AreaId.Maridia, new(20, 14), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastSandHole), 30, AreaId.Maridia, new(23, 14), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestAqueductQuicksand), 31, AreaId.Maridia, new(21, 12), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastAqueductQuicksand), 32, AreaId.Maridia, new(23, 12), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Aqueduct), 33, AreaId.Maridia, new(20, 9), new(6, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Butterfly), 34, AreaId.Maridia, new(26, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BotwoonHallway), 35, AreaId.Maridia, new(20, 8), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Pants), 36, AreaId.Maridia, new(26, 13), new(2, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastPants), 37, AreaId.Maridia, new(27, 14), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Springball), 38, AreaId.Maridia, new(32, 15), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BelowBotwoonETank), 39, AreaId.Maridia, new(26, 10), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Colosseum), 40, AreaId.Maridia, new(34, 6), new(7, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(AqueductSave), 41, AreaId.Maridia, new(19, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ThePrecious), 42, AreaId.Maridia, new(41, 7), new(2, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BotwoonETank), 43, AreaId.Maridia, new(26, 8), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DraygonSave), 44, AreaId.Maridia, new(41, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MaridiaMissileRefill), 45, AreaId.Maridia, new(38, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PlasmaBeachQuicksand), 46, AreaId.Maridia, new(26, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BotwoonQuicksand), 47, AreaId.Maridia, new(28, 9), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Shaktool), 48, AreaId.Maridia, new(28, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(HalfieClimb), 49, AreaId.Maridia, new(33, 6), new(5, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Botwoon), 50, AreaId.Maridia, new(24, 8), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpaceJump), 51, AreaId.Maridia, new(38, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MaridiaEnergyRefill), 52, AreaId.Maridia, new(42, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestCactusAlley), 53, AreaId.Maridia, new(27, 6), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastCactusAlley), 54, AreaId.Maridia, new(28, 6), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Draygon), 55, AreaId.Maridia, new(39, 9), new(2, 2), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
        new(nameof(TourianFirst), 0, AreaId.Tourian, new(20, 9), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Metroids1), 1, AreaId.Tourian, new(14, 12), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Metroids2), 2, AreaId.Tourian, new(13, 12), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Metroids3), 3, AreaId.Tourian, new(14, 13), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Metroids4), 4, AreaId.Tourian, new(20, 13), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BlueHopper), 5, AreaId.Tourian, new(19, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DustTorizo), 6, AreaId.Tourian, new(17, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BigBoy), 7, AreaId.Tourian, new(13, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Seaweed), 8, AreaId.Tourian, new(12, 15), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianRecharge), 9, AreaId.Tourian, new(11, 16), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MotherBrain), 10, AreaId.Tourian, new(13, 18), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEyeDoor), 11, AreaId.Tourian, new(13, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RinkaShaft), 12, AreaId.Tourian, new(17, 16), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MotherBrainSave), 13, AreaId.Tourian, new(16, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEscape1), 14, AreaId.Tourian, new(11, 18), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEscape2), 15, AreaId.Tourian, new(11, 19), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEscape3), 16, AreaId.Tourian, new(12, 19), new(6, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEscape4), 17, AreaId.Tourian, new(18, 16), new(3, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(UpperTourianSave), 18, AreaId.Tourian, new(21, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CeresElev), 0, AreaId.Ceres, new(12, 13), new(1, 3), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
        new(nameof(FallingTile), 1, AreaId.Ceres, new(13, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MagnetStairs), 2, AreaId.Ceres, new(15, 15), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DeadScientist), 3, AreaId.Ceres, new(16, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Room58Escape), 4, AreaId.Ceres, new(18, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CeresRidley), 5, AreaId.Ceres, new(20, 16), new(1, 1), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
    ];

    /// <summary>Every compiled room by its name.</summary>
    private static readonly Dictionary<string, RoomHeaderDefinition> ByName = Retail.ToDictionary(room => room.Name);

    /// <summary>The compiled room named <paramref name="name"/>.</summary>
    private static RoomHeaderDefinition Find(string name) => ByName[name];

    /// <summary>The Landing Site room.</summary>
    public static RoomHeaderDefinition LandingSite { get; } = Find(nameof(LandingSite));
    /// <summary>The Gauntlet Entrance room.</summary>
    public static RoomHeaderDefinition GauntletEntrance { get; } = Find(nameof(GauntletEntrance));
    /// <summary>The Parlor room.</summary>
    public static RoomHeaderDefinition Parlor { get; } = Find(nameof(Parlor));
    /// <summary>The Crateria Power Bombs room.</summary>
    public static RoomHeaderDefinition CrateriaPowerBombs { get; } = Find(nameof(CrateriaPowerBombs));
    /// <summary>The Crateria Save room.</summary>
    public static RoomHeaderDefinition CrateriaSave { get; } = Find(nameof(CrateriaSave));
    /// <summary>The West Ocean room.</summary>
    public static RoomHeaderDefinition WestOcean { get; } = Find(nameof(WestOcean));
    /// <summary>The Bowling Alley Path room.</summary>
    public static RoomHeaderDefinition BowlingAlleyPath { get; } = Find(nameof(BowlingAlleyPath));
    /// <summary>The Crateria Kihunter room.</summary>
    public static RoomHeaderDefinition CrateriaKihunter { get; } = Find(nameof(CrateriaKihunter));
    /// <summary>The Forgotten Highway Elevator room.</summary>
    public static RoomHeaderDefinition ForgottenHighwayElev { get; } = Find(nameof(ForgottenHighwayElev));
    /// <summary>The East Ocean room.</summary>
    public static RoomHeaderDefinition EastOcean { get; } = Find(nameof(EastOcean));
    /// <summary>The Forgotten Highway Kagos room.</summary>
    public static RoomHeaderDefinition ForgottenHighwayKagos { get; } = Find(nameof(ForgottenHighwayKagos));
    /// <summary>The Crab Maze room.</summary>
    public static RoomHeaderDefinition CrabMaze { get; } = Find(nameof(CrabMaze));
    /// <summary>The Forgotten Highway Elbow room.</summary>
    public static RoomHeaderDefinition ForgottenHighwayElbow { get; } = Find(nameof(ForgottenHighwayElbow));
    /// <summary>The Crateria Tube room.</summary>
    public static RoomHeaderDefinition CrateriaTube { get; } = Find(nameof(CrateriaTube));
    /// <summary>The Moat room.</summary>
    public static RoomHeaderDefinition Moat { get; } = Find(nameof(Moat));
    /// <summary>The Red Brinstar Elevator room.</summary>
    public static RoomHeaderDefinition RedBinstarElev { get; } = Find(nameof(RedBinstarElev));
    /// <summary>The Gauntlet Energy Tank room.</summary>
    public static RoomHeaderDefinition GauntletETank { get; } = Find(nameof(GauntletETank));
    /// <summary>The Pre Bowling room.</summary>
    public static RoomHeaderDefinition PreBowling { get; } = Find(nameof(PreBowling));
    /// <summary>The Climb room.</summary>
    public static RoomHeaderDefinition Climb { get; } = Find(nameof(Climb));
    /// <summary>The Pit room.</summary>
    public static RoomHeaderDefinition Pit { get; } = Find(nameof(Pit));
    /// <summary>The Elevator to Blue Brinstar room.</summary>
    public static RoomHeaderDefinition ElevToBlueBrinstar { get; } = Find(nameof(ElevToBlueBrinstar));
    /// <summary>The Bomb Torizo room.</summary>
    public static RoomHeaderDefinition BombTorizo { get; } = Find(nameof(BombTorizo));
    /// <summary>The Flyway room.</summary>
    public static RoomHeaderDefinition Flyway { get; } = Find(nameof(Flyway));
    /// <summary>The Pre Map Flyway room.</summary>
    public static RoomHeaderDefinition PreMapFlyway { get; } = Find(nameof(PreMapFlyway));
    /// <summary>The Terminator room.</summary>
    public static RoomHeaderDefinition Terminator { get; } = Find(nameof(Terminator));
    /// <summary>The Green Brinstar Elevator room.</summary>
    public static RoomHeaderDefinition GreenBrinstarElev { get; } = Find(nameof(GreenBrinstarElev));
    /// <summary>The Lower Mushrooms room.</summary>
    public static RoomHeaderDefinition LowerMushrooms { get; } = Find(nameof(LowerMushrooms));
    /// <summary>The Crateria Map room.</summary>
    public static RoomHeaderDefinition CrateriaMap { get; } = Find(nameof(CrateriaMap));
    /// <summary>The Green Pirates Shaft room.</summary>
    public static RoomHeaderDefinition GreenPiratesShaft { get; } = Find(nameof(GreenPiratesShaft));
    /// <summary>The Crateria Super room.</summary>
    public static RoomHeaderDefinition CrateriaSuper { get; } = Find(nameof(CrateriaSuper));
    /// <summary>The Final Missile Bombway room.</summary>
    public static RoomHeaderDefinition FinalMissileBombway { get; } = Find(nameof(FinalMissileBombway));
    /// <summary>The Final Missile room.</summary>
    public static RoomHeaderDefinition FinalMissile { get; } = Find(nameof(FinalMissile));
    /// <summary>The Green Brinstar Main Shaft room.</summary>
    public static RoomHeaderDefinition GreenBrinstarMainShaft { get; } = Find(nameof(GreenBrinstarMainShaft));
    /// <summary>The Spore Spawn Super room.</summary>
    public static RoomHeaderDefinition SporeSpawnSuper { get; } = Find(nameof(SporeSpawnSuper));
    /// <summary>The Brinstar Pre Map room.</summary>
    public static RoomHeaderDefinition BrinstarPreMap { get; } = Find(nameof(BrinstarPreMap));
    /// <summary>The Early Supers room.</summary>
    public static RoomHeaderDefinition EarlySupers { get; } = Find(nameof(EarlySupers));
    /// <summary>The Brinstar Reserve Tank room.</summary>
    public static RoomHeaderDefinition BrinstarReserveTank { get; } = Find(nameof(BrinstarReserveTank));
    /// <summary>The Green Brinstar Map room.</summary>
    public static RoomHeaderDefinition GreenBrinstarMap { get; } = Find(nameof(GreenBrinstarMap));
    /// <summary>The Green Brinstar Firefleas room.</summary>
    public static RoomHeaderDefinition GreenBrinstarFirefleas { get; } = Find(nameof(GreenBrinstarFirefleas));
    /// <summary>The Green Brinstar Missile Refill room.</summary>
    public static RoomHeaderDefinition GreenBrinstarMissileRefill { get; } = Find(nameof(GreenBrinstarMissileRefill));
    /// <summary>The Dachora room.</summary>
    public static RoomHeaderDefinition Dachora { get; } = Find(nameof(Dachora));
    /// <summary>The Big Pink room.</summary>
    public static RoomHeaderDefinition BigPink { get; } = Find(nameof(BigPink));
    /// <summary>The Spore Spawn Kihunters room.</summary>
    public static RoomHeaderDefinition SporeSpawnKihunters { get; } = Find(nameof(SporeSpawnKihunters));
    /// <summary>The Spore Spawn room.</summary>
    public static RoomHeaderDefinition SporeSpawn { get; } = Find(nameof(SporeSpawn));
    /// <summary>The Pink Brinstar Power Bombs room.</summary>
    public static RoomHeaderDefinition PinkBrinstarPowerBombs { get; } = Find(nameof(PinkBrinstarPowerBombs));
    /// <summary>The Green Hill Zone room.</summary>
    public static RoomHeaderDefinition GreenHillZone { get; } = Find(nameof(GreenHillZone));
    /// <summary>The Morph Ball room.</summary>
    public static RoomHeaderDefinition MorphBall { get; } = Find(nameof(MorphBall));
    /// <summary>The Construction Zone room.</summary>
    public static RoomHeaderDefinition ConstructionZone { get; } = Find(nameof(ConstructionZone));
    /// <summary>The Blue Brinstar Energy Tank room.</summary>
    public static RoomHeaderDefinition BlueBrinstarETank { get; } = Find(nameof(BlueBrinstarETank));
    /// <summary>The Noob Bridge room.</summary>
    public static RoomHeaderDefinition NoobBridge { get; } = Find(nameof(NoobBridge));
    /// <summary>The Green Brinstar Beetoms room.</summary>
    public static RoomHeaderDefinition GreenBrinstarBeetoms { get; } = Find(nameof(GreenBrinstarBeetoms));
    /// <summary>The Etecoon Energy Tank room.</summary>
    public static RoomHeaderDefinition EtecoonETank { get; } = Find(nameof(EtecoonETank));
    /// <summary>The Etecoon Super room.</summary>
    public static RoomHeaderDefinition EtecoonSuper { get; } = Find(nameof(EtecoonSuper));
    /// <summary>The Dachora Energy Refill room.</summary>
    public static RoomHeaderDefinition DachoraEnergyRefill { get; } = Find(nameof(DachoraEnergyRefill));
    /// <summary>The Spore Spawn Farming room.</summary>
    public static RoomHeaderDefinition SporeSpawnFarming { get; } = Find(nameof(SporeSpawnFarming));
    /// <summary>The Waterway Energy Tank room.</summary>
    public static RoomHeaderDefinition WaterwayETank { get; } = Find(nameof(WaterwayETank));
    /// <summary>The First Missile room.</summary>
    public static RoomHeaderDefinition FirstMissile { get; } = Find(nameof(FirstMissile));
    /// <summary>The Pink Brinstar Hoppers room.</summary>
    public static RoomHeaderDefinition PinkBrinstarHoppers { get; } = Find(nameof(PinkBrinstarHoppers));
    /// <summary>The Hopper Energy Tank room.</summary>
    public static RoomHeaderDefinition HopperETank { get; } = Find(nameof(HopperETank));
    /// <summary>The Big Pink Save Room.</summary>
    public static RoomHeaderDefinition BigPinkSaveRoom { get; } = Find(nameof(BigPinkSaveRoom));
    /// <summary>The Blue Brinstar Boulders room.</summary>
    public static RoomHeaderDefinition BlueBrinstarBoulders { get; } = Find(nameof(BlueBrinstarBoulders));
    /// <summary>The Billy Mays room.</summary>
    public static RoomHeaderDefinition BillyMays { get; } = Find(nameof(BillyMays));
    /// <summary>The Green Brinstar Save room.</summary>
    public static RoomHeaderDefinition GreenBrinstarSave { get; } = Find(nameof(GreenBrinstarSave));
    /// <summary>The Etecoon Save room.</summary>
    public static RoomHeaderDefinition EtecoonSave { get; } = Find(nameof(EtecoonSave));
    /// <summary>The Red Tower room.</summary>
    public static RoomHeaderDefinition RedTower { get; } = Find(nameof(RedTower));
    /// <summary>The Red Brinstar Firefleas room.</summary>
    public static RoomHeaderDefinition RedBrinstarFirefleas { get; } = Find(nameof(RedBrinstarFirefleas));
    /// <summary>The X-Ray Scope room.</summary>
    public static RoomHeaderDefinition XrayScope { get; } = Find(nameof(XrayScope));
    /// <summary>The Hellway room.</summary>
    public static RoomHeaderDefinition Hellway { get; } = Find(nameof(Hellway));
    /// <summary>The Caterpillar room.</summary>
    public static RoomHeaderDefinition Caterpillar { get; } = Find(nameof(Caterpillar));
    /// <summary>The Beta Power Bomb room.</summary>
    public static RoomHeaderDefinition BetaPowerBomb { get; } = Find(nameof(BetaPowerBomb));
    /// <summary>The Alpha Power Bomb room.</summary>
    public static RoomHeaderDefinition AlphaPowerBomb { get; } = Find(nameof(AlphaPowerBomb));
    /// <summary>The Skree Boost room.</summary>
    public static RoomHeaderDefinition SkreeBoost { get; } = Find(nameof(SkreeBoost));
    /// <summary>The Below Spazer room.</summary>
    public static RoomHeaderDefinition BelowSpazer { get; } = Find(nameof(BelowSpazer));
    /// <summary>The Spazer room.</summary>
    public static RoomHeaderDefinition Spazer { get; } = Find(nameof(Spazer));
    /// <summary>The Warehouse Zeela room.</summary>
    public static RoomHeaderDefinition WarehouseZeela { get; } = Find(nameof(WarehouseZeela));
    /// <summary>The Warehouse Energy Tank room.</summary>
    public static RoomHeaderDefinition WarehouseETank { get; } = Find(nameof(WarehouseETank));
    /// <summary>The Warehouse Kihunter room.</summary>
    public static RoomHeaderDefinition WarehouseKihunter { get; } = Find(nameof(WarehouseKihunter));
    /// <summary>The Mini Kraid room.</summary>
    public static RoomHeaderDefinition MiniKraid { get; } = Find(nameof(MiniKraid));
    /// <summary>The Kraid Eye Door room.</summary>
    public static RoomHeaderDefinition KraidEyeDoor { get; } = Find(nameof(KraidEyeDoor));
    /// <summary>The Kraid room.</summary>
    public static RoomHeaderDefinition Kraid { get; } = Find(nameof(Kraid));
    /// <summary>The Statues Hallway room.</summary>
    public static RoomHeaderDefinition StatuesHallway { get; } = Find(nameof(StatuesHallway));
    /// <summary>The Red Tower Refill room.</summary>
    public static RoomHeaderDefinition RedTowerRefill { get; } = Find(nameof(RedTowerRefill));
    /// <summary>The Kraid Refill room.</summary>
    public static RoomHeaderDefinition KraidRefill { get; } = Find(nameof(KraidRefill));
    /// <summary>The Statues room.</summary>
    public static RoomHeaderDefinition Statues { get; } = Find(nameof(Statues));
    /// <summary>The Warehouse Entrance room.</summary>
    public static RoomHeaderDefinition WarehouseEntrance { get; } = Find(nameof(WarehouseEntrance));
    /// <summary>The Varia Suit room.</summary>
    public static RoomHeaderDefinition VariaSuit { get; } = Find(nameof(VariaSuit));
    /// <summary>The Warehouse Save room.</summary>
    public static RoomHeaderDefinition WarehouseSave { get; } = Find(nameof(WarehouseSave));
    /// <summary>The Red Brinstar Save room.</summary>
    public static RoomHeaderDefinition RedBrinstarSave { get; } = Find(nameof(RedBrinstarSave));
    /// <summary>The Ice Beam Acid room.</summary>
    public static RoomHeaderDefinition IceBeamAcid { get; } = Find(nameof(IceBeamAcid));
    /// <summary>The Cathedral room.</summary>
    public static RoomHeaderDefinition Cathedral { get; } = Find(nameof(Cathedral));
    /// <summary>The Cathedral Entrance room.</summary>
    public static RoomHeaderDefinition CathedralEntrance { get; } = Find(nameof(CathedralEntrance));
    /// <summary>The Business Center room.</summary>
    public static RoomHeaderDefinition BusinessCenter { get; } = Find(nameof(BusinessCenter));
    /// <summary>The Ice Beam Gate room.</summary>
    public static RoomHeaderDefinition IceBeamGate { get; } = Find(nameof(IceBeamGate));
    /// <summary>The Ice Beam Tutorial room.</summary>
    public static RoomHeaderDefinition IceBeamTutorial { get; } = Find(nameof(IceBeamTutorial));
    /// <summary>The Ice Beam room.</summary>
    public static RoomHeaderDefinition IceBeam { get; } = Find(nameof(IceBeam));
    /// <summary>The Ice Beam Snake room.</summary>
    public static RoomHeaderDefinition IceBeamSnake { get; } = Find(nameof(IceBeamSnake));
    /// <summary>The Crumble Shaft room.</summary>
    public static RoomHeaderDefinition CrumbleShaft { get; } = Find(nameof(CrumbleShaft));
    /// <summary>The Crocomire Speedway room.</summary>
    public static RoomHeaderDefinition CrocomireSpeedway { get; } = Find(nameof(CrocomireSpeedway));
    /// <summary>The Crocomire room.</summary>
    public static RoomHeaderDefinition Crocomire { get; } = Find(nameof(Crocomire));
    /// <summary>The Hi-Jump Boots room.</summary>
    public static RoomHeaderDefinition HiJumpBoots { get; } = Find(nameof(HiJumpBoots));
    /// <summary>The Crocomire Escape room.</summary>
    public static RoomHeaderDefinition CrocomireEscape { get; } = Find(nameof(CrocomireEscape));
    /// <summary>The Hi-Jump Energy Tank room.</summary>
    public static RoomHeaderDefinition HiJumpETank { get; } = Find(nameof(HiJumpETank));
    /// <summary>The Post-Crocomire Farming room.</summary>
    public static RoomHeaderDefinition PostCrocFarming { get; } = Find(nameof(PostCrocFarming));
    /// <summary>The Post-Crocomire Save room.</summary>
    public static RoomHeaderDefinition PostCrocSave { get; } = Find(nameof(PostCrocSave));
    /// <summary>The Post-Crocomire Power Bombs room.</summary>
    public static RoomHeaderDefinition PostCrocPowerBombs { get; } = Find(nameof(PostCrocPowerBombs));
    /// <summary>The Post-Crocomire Shaft room.</summary>
    public static RoomHeaderDefinition PostCrocShaft { get; } = Find(nameof(PostCrocShaft));
    /// <summary>The Post-Crocomire Missile room.</summary>
    public static RoomHeaderDefinition PostCrocMissile { get; } = Find(nameof(PostCrocMissile));
    /// <summary>The Grapple Tutorial 3 room.</summary>
    public static RoomHeaderDefinition GrappleTutorial3 { get; } = Find(nameof(GrappleTutorial3));
    /// <summary>The Post-Crocomire Jump room.</summary>
    public static RoomHeaderDefinition PostCrocJump { get; } = Find(nameof(PostCrocJump));
    /// <summary>The Grapple Tutorial 2 room.</summary>
    public static RoomHeaderDefinition GrappleTutorial2 { get; } = Find(nameof(GrappleTutorial2));
    /// <summary>The Grapple Tutorial 1 room.</summary>
    public static RoomHeaderDefinition GrappleTutorial1 { get; } = Find(nameof(GrappleTutorial1));
    /// <summary>The Grapple Beam room.</summary>
    public static RoomHeaderDefinition GrappleBeam { get; } = Find(nameof(GrappleBeam));
    /// <summary>The Norfair Reserve Tank room.</summary>
    public static RoomHeaderDefinition NorfairReserveTank { get; } = Find(nameof(NorfairReserveTank));
    /// <summary>The Green Bubbles Missiles room.</summary>
    public static RoomHeaderDefinition GreenBubblesMissiles { get; } = Find(nameof(GreenBubblesMissiles));
    /// <summary>The Bubble Mountain room.</summary>
    public static RoomHeaderDefinition BubbleMountain { get; } = Find(nameof(BubbleMountain));
    /// <summary>The Speed Booster Hall room.</summary>
    public static RoomHeaderDefinition SpeedBoosterHall { get; } = Find(nameof(SpeedBoosterHall));
    /// <summary>The Speed Booster room.</summary>
    public static RoomHeaderDefinition SpeedBooster { get; } = Find(nameof(SpeedBooster));
    /// <summary>The Single Chamber room.</summary>
    public static RoomHeaderDefinition SingleChamber { get; } = Find(nameof(SingleChamber));
    /// <summary>The Double Chamber room.</summary>
    public static RoomHeaderDefinition DoubleChamber { get; } = Find(nameof(DoubleChamber));
    /// <summary>The Wave Beam room.</summary>
    public static RoomHeaderDefinition WaveBeam { get; } = Find(nameof(WaveBeam));
    /// <summary>The Spiky Platforms Tunnel room.</summary>
    public static RoomHeaderDefinition SpikyPlatformsTunnel { get; } = Find(nameof(SpikyPlatformsTunnel));
    /// <summary>The Volcano room.</summary>
    public static RoomHeaderDefinition Volcano { get; } = Find(nameof(Volcano));
    /// <summary>The Kronic Boost room.</summary>
    public static RoomHeaderDefinition KronicBoost { get; } = Find(nameof(KronicBoost));
    /// <summary>The Magdollite Tunnel room.</summary>
    public static RoomHeaderDefinition MagdolliteTunnel { get; } = Find(nameof(MagdolliteTunnel));
    /// <summary>The Purple Shaft room.</summary>
    public static RoomHeaderDefinition PurpleShaft { get; } = Find(nameof(PurpleShaft));
    /// <summary>The Lava Dive room.</summary>
    public static RoomHeaderDefinition LavaDive { get; } = Find(nameof(LavaDive));
    /// <summary>The Lower Norfair Elevator room.</summary>
    public static RoomHeaderDefinition LowerNorfairElev { get; } = Find(nameof(LowerNorfairElev));
    /// <summary>The Upper Norfair Farming room.</summary>
    public static RoomHeaderDefinition UpperNorfairFarming { get; } = Find(nameof(UpperNorfairFarming));
    /// <summary>The Rising Tide room.</summary>
    public static RoomHeaderDefinition RisingTide { get; } = Find(nameof(RisingTide));
    /// <summary>The Acid Snakes Tunnel room.</summary>
    public static RoomHeaderDefinition AcidSnakesTunnel { get; } = Find(nameof(AcidSnakesTunnel));
    /// <summary>The Spiky Acid Snakes Tunnel room.</summary>
    public static RoomHeaderDefinition SpikyAcidSnakesTunnel { get; } = Find(nameof(SpikyAcidSnakesTunnel));
    /// <summary>The Upper Norfair Refill room.</summary>
    public static RoomHeaderDefinition UpperNorfairRefill { get; } = Find(nameof(UpperNorfairRefill));
    /// <summary>The Purple Farming room.</summary>
    public static RoomHeaderDefinition PurpleFarming { get; } = Find(nameof(PurpleFarming));
    /// <summary>The Bat Cave room.</summary>
    public static RoomHeaderDefinition BatCave { get; } = Find(nameof(BatCave));
    /// <summary>The Norfair Map room.</summary>
    public static RoomHeaderDefinition NorfairMap { get; } = Find(nameof(NorfairMap));
    /// <summary>The Bubble Mountain Save room.</summary>
    public static RoomHeaderDefinition BubbleMountainSave { get; } = Find(nameof(BubbleMountainSave));
    /// <summary>The Frog Speedway room.</summary>
    public static RoomHeaderDefinition FrogSpeedway { get; } = Find(nameof(FrogSpeedway));
    /// <summary>The Red Pirate Shaft room.</summary>
    public static RoomHeaderDefinition RedPirateShaft { get; } = Find(nameof(RedPirateShaft));
    /// <summary>The Frog Save room.</summary>
    public static RoomHeaderDefinition FrogSave { get; } = Find(nameof(FrogSave));
    /// <summary>The Crocomire Save room.</summary>
    public static RoomHeaderDefinition CrocomireSave { get; } = Find(nameof(CrocomireSave));
    /// <summary>The Lower Norfair Elevator Save room.</summary>
    public static RoomHeaderDefinition LNElevSave { get; } = Find(nameof(LNElevSave));
    /// <summary>The Acid Statue room.</summary>
    public static RoomHeaderDefinition AcidStatue { get; } = Find(nameof(AcidStatue));
    /// <summary>The Main Hall room.</summary>
    public static RoomHeaderDefinition MainHall { get; } = Find(nameof(MainHall));
    /// <summary>The Golden Torizo room.</summary>
    public static RoomHeaderDefinition GoldenTorizo { get; } = Find(nameof(GoldenTorizo));
    /// <summary>The Fast Ripper room.</summary>
    public static RoomHeaderDefinition FastRipper { get; } = Find(nameof(FastRipper));
    /// <summary>The Golden Torizo Energy Refill room.</summary>
    public static RoomHeaderDefinition GTEnergyRefill { get; } = Find(nameof(GTEnergyRefill));
    /// <summary>The Ridley room.</summary>
    public static RoomHeaderDefinition Ridley { get; } = Find(nameof(Ridley));
    /// <summary>The Lower Norfair Farming room.</summary>
    public static RoomHeaderDefinition LNFarming { get; } = Find(nameof(LNFarming));
    /// <summary>The Fast Pillars Setup room.</summary>
    public static RoomHeaderDefinition FastPillarsSetup { get; } = Find(nameof(FastPillarsSetup));
    /// <summary>An unused room that no door leads to.</summary>
    public static RoomHeaderDefinition UnusedNorfairRoom { get; } = Find(nameof(UnusedNorfairRoom));
    /// <summary>The Mickey Mouse room.</summary>
    public static RoomHeaderDefinition MickeyMouse { get; } = Find(nameof(MickeyMouse));
    /// <summary>The Pillar room.</summary>
    public static RoomHeaderDefinition Pillar { get; } = Find(nameof(Pillar));
    /// <summary>The Plowerhouse room.</summary>
    public static RoomHeaderDefinition Plowerhouse { get; } = Find(nameof(Plowerhouse));
    /// <summary>The Worst Room in the Game.</summary>
    public static RoomHeaderDefinition WorstRoomInTheGame { get; } = Find(nameof(WorstRoomInTheGame));
    /// <summary>The Amphitheatre room.</summary>
    public static RoomHeaderDefinition Amphitheatre { get; } = Find(nameof(Amphitheatre));
    /// <summary>The Lower Norfair Spring Ball Maze room.</summary>
    public static RoomHeaderDefinition LNSpringBallMaze { get; } = Find(nameof(LNSpringBallMaze));
    /// <summary>The Lower Norfair Escape Power Bombs room.</summary>
    public static RoomHeaderDefinition LNEscapePowerBombs { get; } = Find(nameof(LNEscapePowerBombs));
    /// <summary>The Red Kihunter Shaft room.</summary>
    public static RoomHeaderDefinition RedKihunterShaft { get; } = Find(nameof(RedKihunterShaft));
    /// <summary>The Wasteland room.</summary>
    public static RoomHeaderDefinition Wasteland { get; } = Find(nameof(Wasteland));
    /// <summary>The Metal Pirates room.</summary>
    public static RoomHeaderDefinition MetalPirates { get; } = Find(nameof(MetalPirates));
    /// <summary>The Three Musketeers room.</summary>
    public static RoomHeaderDefinition ThreeMusketeers { get; } = Find(nameof(ThreeMusketeers));
    /// <summary>The Ridley Energy Tank room.</summary>
    public static RoomHeaderDefinition RidleyETank { get; } = Find(nameof(RidleyETank));
    /// <summary>The Screw Attack room.</summary>
    public static RoomHeaderDefinition ScrewAttack { get; } = Find(nameof(ScrewAttack));
    /// <summary>The Lower Norfair Fireflea room.</summary>
    public static RoomHeaderDefinition LNFireflea { get; } = Find(nameof(LNFireflea));
    /// <summary>The Lower Norfair Save room.</summary>
    public static RoomHeaderDefinition LNSave { get; } = Find(nameof(LNSave));
    /// <summary>The Bowling Alley room.</summary>
    public static RoomHeaderDefinition BowlingAlley { get; } = Find(nameof(BowlingAlley));
    /// <summary>The Wrecked Ship Entrance room.</summary>
    public static RoomHeaderDefinition WreckedShipEntrance { get; } = Find(nameof(WreckedShipEntrance));
    /// <summary>The Attic room.</summary>
    public static RoomHeaderDefinition Attic { get; } = Find(nameof(Attic));
    /// <summary>Wrecked Ship's Assembly Line room.</summary>
    public static RoomHeaderDefinition AssemblyLine { get; } = Find(nameof(AssemblyLine));
    /// <summary>The Wrecked Ship Main Shaft room.</summary>
    public static RoomHeaderDefinition WreckedShipMainShaft { get; } = Find(nameof(WreckedShipMainShaft));
    /// <summary>The Spiky Death room.</summary>
    public static RoomHeaderDefinition SpikyDeath { get; } = Find(nameof(SpikyDeath));
    /// <summary>The Electric Death room.</summary>
    public static RoomHeaderDefinition ElectricDeath { get; } = Find(nameof(ElectricDeath));
    /// <summary>The Wrecked Ship Energy Tank room.</summary>
    public static RoomHeaderDefinition WreckedShipETank { get; } = Find(nameof(WreckedShipETank));
    /// <summary>The Basement room.</summary>
    public static RoomHeaderDefinition Basement { get; } = Find(nameof(Basement));
    /// <summary>The Wrecked Ship Map room.</summary>
    public static RoomHeaderDefinition WreckedShipMap { get; } = Find(nameof(WreckedShipMap));
    /// <summary>The Phantoon room.</summary>
    public static RoomHeaderDefinition Phantoon { get; } = Find(nameof(Phantoon));
    /// <summary>The Sponge Bath room.</summary>
    public static RoomHeaderDefinition SpongeBath { get; } = Find(nameof(SpongeBath));
    /// <summary>The Wrecked Ship West Super room.</summary>
    public static RoomHeaderDefinition WSWestSuper { get; } = Find(nameof(WSWestSuper));
    /// <summary>The Wrecked Ship East Super room.</summary>
    public static RoomHeaderDefinition WSEastSuper { get; } = Find(nameof(WSEastSuper));
    /// <summary>The Gravity Suit room.</summary>
    public static RoomHeaderDefinition GravitySuit { get; } = Find(nameof(GravitySuit));
    /// <summary>The Wrecked Ship Save room.</summary>
    public static RoomHeaderDefinition WreckedShipSave { get; } = Find(nameof(WreckedShipSave));
    /// <summary>The Glass Tunnel Save room.</summary>
    public static RoomHeaderDefinition GlassTunnelSave { get; } = Find(nameof(GlassTunnelSave));
    /// <summary>The Glass Tunnel room.</summary>
    public static RoomHeaderDefinition GlassTunnel { get; } = Find(nameof(GlassTunnel));
    /// <summary>The West Tunnel room.</summary>
    public static RoomHeaderDefinition WestTunnel { get; } = Find(nameof(WestTunnel));
    /// <summary>The East Tunnel room.</summary>
    public static RoomHeaderDefinition EastTunnel { get; } = Find(nameof(EastTunnel));
    /// <summary>The Main Street room.</summary>
    public static RoomHeaderDefinition MainStreet { get; } = Find(nameof(MainStreet));
    /// <summary>The Fish Tank room.</summary>
    public static RoomHeaderDefinition FishTank { get; } = Find(nameof(FishTank));
    /// <summary>The Mama Turtle room.</summary>
    public static RoomHeaderDefinition MamaTurtle { get; } = Find(nameof(MamaTurtle));
    /// <summary>The Crab Tunnel room.</summary>
    public static RoomHeaderDefinition CrabTunnel { get; } = Find(nameof(CrabTunnel));
    /// <summary>The Mt. Everest room.</summary>
    public static RoomHeaderDefinition MtEverest { get; } = Find(nameof(MtEverest));
    /// <summary>The Red Fish room.</summary>
    public static RoomHeaderDefinition RedFish { get; } = Find(nameof(RedFish));
    /// <summary>The Watering Hole room.</summary>
    public static RoomHeaderDefinition WateringHole { get; } = Find(nameof(WateringHole));
    /// <summary>The Northwest Maridia Bug room.</summary>
    public static RoomHeaderDefinition NWestMaridiaBug { get; } = Find(nameof(NWestMaridiaBug));
    /// <summary>The Crab Shaft room.</summary>
    public static RoomHeaderDefinition CrabShaft { get; } = Find(nameof(CrabShaft));
    /// <summary>The Pseudo Plasma Spark room.</summary>
    public static RoomHeaderDefinition PseudoPlasmaSpark { get; } = Find(nameof(PseudoPlasmaSpark));
    /// <summary>The Crab Hole room.</summary>
    public static RoomHeaderDefinition CrabHole { get; } = Find(nameof(CrabHole));
    /// <summary>The West Sand Hall Tunnel room.</summary>
    public static RoomHeaderDefinition WestSandHallTunnel { get; } = Find(nameof(WestSandHallTunnel));
    /// <summary>The Plasma Tutorial room.</summary>
    public static RoomHeaderDefinition PlasmaTutorial { get; } = Find(nameof(PlasmaTutorial));
    /// <summary>The Plasma room.</summary>
    public static RoomHeaderDefinition Plasma { get; } = Find(nameof(Plasma));
    /// <summary>The Thread the Needle room.</summary>
    public static RoomHeaderDefinition ThreadTheNeedle { get; } = Find(nameof(ThreadTheNeedle));
    /// <summary>The Maridia Elevator room.</summary>
    public static RoomHeaderDefinition MaridiaElev { get; } = Find(nameof(MaridiaElev));
    /// <summary>The Plasma Spark room.</summary>
    public static RoomHeaderDefinition PlasmaSpark { get; } = Find(nameof(PlasmaSpark));
    /// <summary>The Kassiuz room.</summary>
    public static RoomHeaderDefinition Kassiuz { get; } = Find(nameof(Kassiuz));
    /// <summary>The Maridia Map room.</summary>
    public static RoomHeaderDefinition MaridiaMap { get; } = Find(nameof(MaridiaMap));
    /// <summary>The Forgotten Highway Save room.</summary>
    public static RoomHeaderDefinition ForgottenHighwaySave { get; } = Find(nameof(ForgottenHighwaySave));
    /// <summary>The Toilet room.</summary>
    public static RoomHeaderDefinition Toilet { get; } = Find(nameof(Toilet));
    /// <summary>The Bug Sand Hole room.</summary>
    public static RoomHeaderDefinition BugSandHole { get; } = Find(nameof(BugSandHole));
    /// <summary>The West Sand Hall room.</summary>
    public static RoomHeaderDefinition WestSandHall { get; } = Find(nameof(WestSandHall));
    /// <summary>The Oasis room.</summary>
    public static RoomHeaderDefinition Oasis { get; } = Find(nameof(Oasis));
    /// <summary>The East Sand Hall room.</summary>
    public static RoomHeaderDefinition EastSandHall { get; } = Find(nameof(EastSandHall));
    /// <summary>The West Sand Hole room.</summary>
    public static RoomHeaderDefinition WestSandHole { get; } = Find(nameof(WestSandHole));
    /// <summary>The East Sand Hole room.</summary>
    public static RoomHeaderDefinition EastSandHole { get; } = Find(nameof(EastSandHole));
    /// <summary>The West Aqueduct Quicksand room.</summary>
    public static RoomHeaderDefinition WestAqueductQuicksand { get; } = Find(nameof(WestAqueductQuicksand));
    /// <summary>The East Aqueduct Quicksand room.</summary>
    public static RoomHeaderDefinition EastAqueductQuicksand { get; } = Find(nameof(EastAqueductQuicksand));
    /// <summary>The Aqueduct room.</summary>
    public static RoomHeaderDefinition Aqueduct { get; } = Find(nameof(Aqueduct));
    /// <summary>The Butterfly room.</summary>
    public static RoomHeaderDefinition Butterfly { get; } = Find(nameof(Butterfly));
    /// <summary>The Botwoon Hallway room.</summary>
    public static RoomHeaderDefinition BotwoonHallway { get; } = Find(nameof(BotwoonHallway));
    /// <summary>The Pants room.</summary>
    public static RoomHeaderDefinition Pants { get; } = Find(nameof(Pants));
    /// <summary>The East Pants room.</summary>
    public static RoomHeaderDefinition EastPants { get; } = Find(nameof(EastPants));
    /// <summary>The Springball room.</summary>
    public static RoomHeaderDefinition Springball { get; } = Find(nameof(Springball));
    /// <summary>The Below Botwoon Energy Tank room.</summary>
    public static RoomHeaderDefinition BelowBotwoonETank { get; } = Find(nameof(BelowBotwoonETank));
    /// <summary>The Colosseum room.</summary>
    public static RoomHeaderDefinition Colosseum { get; } = Find(nameof(Colosseum));
    /// <summary>The Aqueduct Save room.</summary>
    public static RoomHeaderDefinition AqueductSave { get; } = Find(nameof(AqueductSave));
    /// <summary>The Precious room.</summary>
    public static RoomHeaderDefinition ThePrecious { get; } = Find(nameof(ThePrecious));
    /// <summary>The Botwoon Energy Tank room.</summary>
    public static RoomHeaderDefinition BotwoonETank { get; } = Find(nameof(BotwoonETank));
    /// <summary>The Draygon Save room.</summary>
    public static RoomHeaderDefinition DraygonSave { get; } = Find(nameof(DraygonSave));
    /// <summary>The Maridia Missile Refill room.</summary>
    public static RoomHeaderDefinition MaridiaMissileRefill { get; } = Find(nameof(MaridiaMissileRefill));
    /// <summary>The Plasma Beach Quicksand room.</summary>
    public static RoomHeaderDefinition PlasmaBeachQuicksand { get; } = Find(nameof(PlasmaBeachQuicksand));
    /// <summary>The Botwoon Quicksand room.</summary>
    public static RoomHeaderDefinition BotwoonQuicksand { get; } = Find(nameof(BotwoonQuicksand));
    /// <summary>The Shaktool room.</summary>
    public static RoomHeaderDefinition Shaktool { get; } = Find(nameof(Shaktool));
    /// <summary>The Halfie Climb room.</summary>
    public static RoomHeaderDefinition HalfieClimb { get; } = Find(nameof(HalfieClimb));
    /// <summary>The Botwoon room.</summary>
    public static RoomHeaderDefinition Botwoon { get; } = Find(nameof(Botwoon));
    /// <summary>The Space Jump room.</summary>
    public static RoomHeaderDefinition SpaceJump { get; } = Find(nameof(SpaceJump));
    /// <summary>The Maridia Energy Refill room.</summary>
    public static RoomHeaderDefinition MaridiaEnergyRefill { get; } = Find(nameof(MaridiaEnergyRefill));
    /// <summary>The West Cactus Alley room.</summary>
    public static RoomHeaderDefinition WestCactusAlley { get; } = Find(nameof(WestCactusAlley));
    /// <summary>The East Cactus Alley room.</summary>
    public static RoomHeaderDefinition EastCactusAlley { get; } = Find(nameof(EastCactusAlley));
    /// <summary>The Draygon room.</summary>
    public static RoomHeaderDefinition Draygon { get; } = Find(nameof(Draygon));
    /// <summary>The Tourian First room.</summary>
    public static RoomHeaderDefinition TourianFirst { get; } = Find(nameof(TourianFirst));
    /// <summary>The Metroids 1 room.</summary>
    public static RoomHeaderDefinition Metroids1 { get; } = Find(nameof(Metroids1));
    /// <summary>The Metroids 2 room.</summary>
    public static RoomHeaderDefinition Metroids2 { get; } = Find(nameof(Metroids2));
    /// <summary>The Metroids 3 room.</summary>
    public static RoomHeaderDefinition Metroids3 { get; } = Find(nameof(Metroids3));
    /// <summary>The Metroids 4 room.</summary>
    public static RoomHeaderDefinition Metroids4 { get; } = Find(nameof(Metroids4));
    /// <summary>The Blue Hopper room.</summary>
    public static RoomHeaderDefinition BlueHopper { get; } = Find(nameof(BlueHopper));
    /// <summary>The Dust Torizo room.</summary>
    public static RoomHeaderDefinition DustTorizo { get; } = Find(nameof(DustTorizo));
    /// <summary>The Big Boy room.</summary>
    public static RoomHeaderDefinition BigBoy { get; } = Find(nameof(BigBoy));
    /// <summary>The Seaweed room.</summary>
    public static RoomHeaderDefinition Seaweed { get; } = Find(nameof(Seaweed));
    /// <summary>The Tourian Recharge room.</summary>
    public static RoomHeaderDefinition TourianRecharge { get; } = Find(nameof(TourianRecharge));
    /// <summary>The Mother Brain room.</summary>
    public static RoomHeaderDefinition MotherBrain { get; } = Find(nameof(MotherBrain));
    /// <summary>The Tourian Eye Door room.</summary>
    public static RoomHeaderDefinition TourianEyeDoor { get; } = Find(nameof(TourianEyeDoor));
    /// <summary>The Rinka Shaft room.</summary>
    public static RoomHeaderDefinition RinkaShaft { get; } = Find(nameof(RinkaShaft));
    /// <summary>The Mother Brain Save room.</summary>
    public static RoomHeaderDefinition MotherBrainSave { get; } = Find(nameof(MotherBrainSave));
    /// <summary>The Tourian Escape 1 room.</summary>
    public static RoomHeaderDefinition TourianEscape1 { get; } = Find(nameof(TourianEscape1));
    /// <summary>The Tourian Escape 2 room.</summary>
    public static RoomHeaderDefinition TourianEscape2 { get; } = Find(nameof(TourianEscape2));
    /// <summary>The Tourian Escape 3 room.</summary>
    public static RoomHeaderDefinition TourianEscape3 { get; } = Find(nameof(TourianEscape3));
    /// <summary>The Tourian Escape 4 room.</summary>
    public static RoomHeaderDefinition TourianEscape4 { get; } = Find(nameof(TourianEscape4));
    /// <summary>The Upper Tourian Save room.</summary>
    public static RoomHeaderDefinition UpperTourianSave { get; } = Find(nameof(UpperTourianSave));
    /// <summary>The Ceres Elevator room.</summary>
    public static RoomHeaderDefinition CeresElev { get; } = Find(nameof(CeresElev));
    /// <summary>The Falling Tile room.</summary>
    public static RoomHeaderDefinition FallingTile { get; } = Find(nameof(FallingTile));
    /// <summary>The Magnet Stairs room.</summary>
    public static RoomHeaderDefinition MagnetStairs { get; } = Find(nameof(MagnetStairs));
    /// <summary>The Dead Scientist room.</summary>
    public static RoomHeaderDefinition DeadScientist { get; } = Find(nameof(DeadScientist));
    /// <summary>The Room 58 Escape.</summary>
    public static RoomHeaderDefinition Room58Escape { get; } = Find(nameof(Room58Escape));
    /// <summary>The Ceres Ridley room.</summary>
    public static RoomHeaderDefinition CeresRidley { get; } = Find(nameof(CeresRidley));
}
