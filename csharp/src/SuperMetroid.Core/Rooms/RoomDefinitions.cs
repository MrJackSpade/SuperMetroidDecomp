using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>The game's 262 rooms.</summary>
/// <remarks>Each entry holds a room's area, map position, size in screens, camera
/// scroll offsets and whether its door transitions are isolated. The developer area-seven room is not compiled.</remarks>
[NumberedIdentities("Rooms such as Metroids1-4 and TourianEscape1-4 are distinct rooms with their own definitions; the number is part of each room's name, not a position callers index.")]
public static class RoomDefinitions
{
    /// <summary>Retail rooms in their authored order.</summary>
    internal static readonly RoomDefinition[] Retail =
    [
        new(nameof(LandingSite), AreaId.Crateria, new(23, 0), new(9, 5), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GauntletEntrance), AreaId.Crateria, new(18, 2), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Parlor), AreaId.Crateria, new(18, 4), new(5, 5), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaPowerBombs), AreaId.Crateria, new(32, 1), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaSave), AreaId.Crateria, new(18, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestOcean), AreaId.Crateria, new(38, 0), new(8, 6), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BowlingAlleyPath), AreaId.Crateria, new(41, 2), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaKihunter), AreaId.Crateria, new(33, 4), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ForgottenHighwayElev), AreaId.Crateria, new(52, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastOcean), AreaId.Crateria, new(49, 0), new(7, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ForgottenHighwayKagos), AreaId.Crateria, new(56, 4), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrabMaze), AreaId.Crateria, new(53, 8), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ForgottenHighwayElbow), AreaId.Crateria, new(52, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaTube), AreaId.Crateria, new(32, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Moat), AreaId.Crateria, new(36, 4), new(2, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedBinstarElev), AreaId.Crateria, new(34, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GauntletETank), AreaId.Crateria, new(12, 2), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PreBowling), AreaId.Crateria, new(43, 2), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Climb), AreaId.Crateria, new(18, 9), new(3, 9), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Pit), AreaId.Crateria, new(20, 17), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ElevToBlueBrinstar), AreaId.Crateria, new(23, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BombTorizo), AreaId.Crateria, new(25, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Flyway), AreaId.Crateria, new(22, 6), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PreMapFlyway), AreaId.Crateria, new(20, 7), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Terminator), AreaId.Crateria, new(12, 4), new(6, 3), 160, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarElev), AreaId.Crateria, new(6, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LowerMushrooms), AreaId.Crateria, new(7, 8), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaMap), AreaId.Crateria, new(23, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenPiratesShaft), AreaId.Crateria, new(11, 2), new(1, 7), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrateriaSuper), AreaId.Crateria, new(21, 9), new(4, 8), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FinalMissileBombway), AreaId.Crateria, new(17, 7), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FinalMissile), AreaId.Crateria, new(16, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarMainShaft), AreaId.Brinstar, new(9, 0), new(4, 12), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SporeSpawnSuper), AreaId.Brinstar, new(23, 1), new(2, 9), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BrinstarPreMap), AreaId.Brinstar, new(6, 4), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EarlySupers), AreaId.Brinstar, new(10, 3), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BrinstarReserveTank), AreaId.Brinstar, new(13, 4), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarMap), AreaId.Brinstar, new(5, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarFirefleas), AreaId.Brinstar, new(6, 6), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarMissileRefill), AreaId.Brinstar, new(5, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Dachora), AreaId.Brinstar, new(10, 6), new(7, 7), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BigPink), AreaId.Brinstar, new(15, 4), new(5, 10), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SporeSpawnKihunters), AreaId.Brinstar, new(19, 4), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SporeSpawn), AreaId.Brinstar, new(22, 1), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PinkBrinstarPowerBombs), AreaId.Brinstar, new(15, 7), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenHillZone), AreaId.Brinstar, new(19, 10), new(8, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MorphBall), AreaId.Brinstar, new(21, 8), new(8, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ConstructionZone), AreaId.Brinstar, new(29, 10), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BlueBrinstarETank), AreaId.Brinstar, new(30, 8), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(NoobBridge), AreaId.Brinstar, new(27, 13), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarBeetoms), AreaId.Brinstar, new(8, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EtecoonETank), AreaId.Brinstar, new(6, 10), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EtecoonSuper), AreaId.Brinstar, new(5, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DachoraEnergyRefill), AreaId.Brinstar, new(9, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SporeSpawnFarming), AreaId.Brinstar, new(20, 9), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WaterwayETank), AreaId.Brinstar, new(8, 13), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FirstMissile), AreaId.Brinstar, new(28, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PinkBrinstarHoppers), AreaId.Brinstar, new(19, 7), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(HopperETank), AreaId.Brinstar, new(21, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BigPinkSaveRoom), AreaId.Brinstar, new(15, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BlueBrinstarBoulders), AreaId.Brinstar, new(30, 8), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BillyMays), AreaId.Brinstar, new(29, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBrinstarSave), AreaId.Brinstar, new(8, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EtecoonSave), AreaId.Brinstar, new(5, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedTower), AreaId.Brinstar, new(33, 9), new(1, 10), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedBrinstarFirefleas), AreaId.Brinstar, new(25, 15), new(8, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(XrayScope), AreaId.Brinstar, new(23, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Hellway), AreaId.Brinstar, new(34, 9), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Caterpillar), AreaId.Brinstar, new(37, 4), new(3, 8), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BetaPowerBomb), AreaId.Brinstar, new(35, 7), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(AlphaPowerBomb), AreaId.Brinstar, new(34, 11), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SkreeBoost), AreaId.Brinstar, new(34, 18), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BelowSpazer), AreaId.Brinstar, new(36, 17), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Spazer), AreaId.Brinstar, new(38, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseZeela), AreaId.Brinstar, new(44, 18), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseETank), AreaId.Brinstar, new(43, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseKihunter), AreaId.Brinstar, new(45, 18), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MiniKraid), AreaId.Brinstar, new(47, 19), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(KraidEyeDoor), AreaId.Brinstar, new(53, 18), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Kraid), AreaId.Brinstar, new(55, 18), new(2, 2), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
        new(nameof(StatuesHallway), AreaId.Crateria, new(12, 8), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedTowerRefill), AreaId.Brinstar, new(32, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(KraidRefill), AreaId.Brinstar, new(54, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Statues), AreaId.Crateria, new(17, 8), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseEntrance), AreaId.Brinstar, new(41, 18), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(VariaSuit), AreaId.Brinstar, new(57, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WarehouseSave), AreaId.Brinstar, new(49, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedBrinstarSave), AreaId.Brinstar, new(38, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeamAcid), AreaId.Norfair, new(4, 3), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Cathedral), AreaId.Norfair, new(14, 3), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CathedralEntrance), AreaId.Norfair, new(11, 3), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BusinessCenter), AreaId.Norfair, new(10, 0), new(1, 7), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeamGate), AreaId.Norfair, new(3, 1), new(7, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeamTutorial), AreaId.Norfair, new(4, 1), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeam), AreaId.Norfair, new(5, 2), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(IceBeamSnake), AreaId.Norfair, new(3, 1), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrumbleShaft), AreaId.Norfair, new(2, 4), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrocomireSpeedway), AreaId.Norfair, new(3, 7), new(13, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Crocomire), AreaId.Norfair, new(12, 10), new(8, 1), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
        new(nameof(HiJumpBoots), AreaId.Norfair, new(7, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrocomireEscape), AreaId.Norfair, new(11, 6), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(HiJumpETank), AreaId.Norfair, new(8, 5), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocFarming), AreaId.Norfair, new(10, 10), new(2, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocSave), AreaId.Norfair, new(12, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocPowerBombs), AreaId.Norfair, new(9, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocShaft), AreaId.Norfair, new(10, 12), new(1, 5), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocMissile), AreaId.Norfair, new(11, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GrappleTutorial3), AreaId.Norfair, new(7, 12), new(3, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PostCrocJump), AreaId.Norfair, new(4, 15), new(8, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GrappleTutorial2), AreaId.Norfair, new(6, 12), new(1, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GrappleTutorial1), AreaId.Norfair, new(4, 14), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GrappleBeam), AreaId.Norfair, new(3, 14), new(1, 4), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(NorfairReserveTank), AreaId.Norfair, new(18, 2), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GreenBubblesMissiles), AreaId.Norfair, new(20, 2), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BubbleMountain), AreaId.Norfair, new(22, 2), new(2, 4), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpeedBoosterHall), AreaId.Norfair, new(25, 1), new(12, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpeedBooster), AreaId.Norfair, new(37, 2), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SingleChamber), AreaId.Norfair, new(24, 3), new(6, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DoubleChamber), AreaId.Norfair, new(25, 4), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WaveBeam), AreaId.Norfair, new(29, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpikyPlatformsTunnel), AreaId.Norfair, new(25, 6), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Volcano), AreaId.Norfair, new(27, 6), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(KronicBoost), AreaId.Norfair, new(25, 8), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MagdolliteTunnel), AreaId.Norfair, new(23, 8), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PurpleShaft), AreaId.Norfair, new(22, 6), new(1, 3), 160, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LavaDive), AreaId.Norfair, new(22, 10), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LowerNorfairElev), AreaId.Norfair, new(21, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(UpperNorfairFarming), AreaId.Norfair, new(20, 5), new(2, 2), 160, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RisingTide), AreaId.Norfair, new(17, 4), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(AcidSnakesTunnel), AreaId.Norfair, new(16, 9), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpikyAcidSnakesTunnel), AreaId.Norfair, new(21, 9), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(UpperNorfairRefill), AreaId.Norfair, new(20, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PurpleFarming), AreaId.Norfair, new(23, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BatCave), AreaId.Norfair, new(24, 1), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(NorfairMap), AreaId.Norfair, new(9, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BubbleMountainSave), AreaId.Norfair, new(21, 3), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FrogSpeedway), AreaId.Norfair, new(12, 5), new(8, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedPirateShaft), AreaId.Norfair, new(19, 6), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FrogSave), AreaId.Norfair, new(11, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrocomireSave), AreaId.Norfair, new(16, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNElevSave), AreaId.Norfair, new(20, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(AcidStatue), AreaId.Norfair, new(15, 13), new(3, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MainHall), AreaId.Norfair, new(17, 11), new(8, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GoldenTorizo), AreaId.Norfair, new(18, 15), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FastRipper), AreaId.Norfair, new(21, 14), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GTEnergyRefill), AreaId.Norfair, new(21, 15), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Ridley), AreaId.Norfair, new(23, 16), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNFarming), AreaId.Norfair, new(24, 16), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FastPillarsSetup), AreaId.Norfair, new(25, 12), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MickeyMouse), AreaId.Norfair, new(26, 9), new(4, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Pillar), AreaId.Norfair, new(26, 14), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Plowerhouse), AreaId.Norfair, new(27, 16), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WorstRoomInTheGame), AreaId.Norfair, new(30, 9), new(1, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Amphitheatre), AreaId.Norfair, new(31, 9), new(4, 5), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNSpringBallMaze), AreaId.Norfair, new(33, 5), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNEscapePowerBombs), AreaId.Norfair, new(37, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedKihunterShaft), AreaId.Norfair, new(35, 9), new(3, 5), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Wasteland), AreaId.Norfair, new(32, 14), new(6, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MetalPirates), AreaId.Norfair, new(30, 16), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ThreeMusketeers), AreaId.Norfair, new(29, 3), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RidleyETank), AreaId.Norfair, new(22, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ScrewAttack), AreaId.Norfair, new(20, 14), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNFireflea), AreaId.Norfair, new(35, 6), new(3, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(LNSave), AreaId.Norfair, new(36, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BowlingAlley), AreaId.WreckedShip, new(10, 11), new(6, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipEntrance), AreaId.WreckedShip, new(12, 14), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Attic), AreaId.WreckedShip, new(12, 10), new(7, 1), 0, 0, RoomDoorTransitionStyle.Standard),
        new(nameof(AssemblyLine), AreaId.WreckedShip, new(19, 10), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipMainShaft), AreaId.WreckedShip, new(12, 11), new(6, 8), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpikyDeath), AreaId.WreckedShip, new(19, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ElectricDeath), AreaId.WreckedShip, new(21, 13), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipETank), AreaId.WreckedShip, new(18, 13), new(3, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Basement), AreaId.WreckedShip, new(14, 19), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipMap), AreaId.WreckedShip, new(13, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Phantoon), AreaId.WreckedShip, new(19, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpongeBath), AreaId.WreckedShip, new(17, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WSWestSuper), AreaId.WreckedShip, new(15, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WSEastSuper), AreaId.WreckedShip, new(18, 17), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GravitySuit), AreaId.WreckedShip, new(10, 13), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WreckedShipSave), AreaId.WreckedShip, new(17, 14), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GlassTunnelSave), AreaId.Maridia, new(12, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(GlassTunnel), AreaId.Maridia, new(11, 17), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestTunnel), AreaId.Maridia, new(10, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastTunnel), AreaId.Maridia, new(12, 17), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MainStreet), AreaId.Maridia, new(10, 9), new(3, 8), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(FishTank), AreaId.Maridia, new(13, 13), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MamaTurtle), AreaId.Maridia, new(17, 12), new(3, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrabTunnel), AreaId.Maridia, new(12, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MtEverest), AreaId.Maridia, new(12, 9), new(6, 4), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RedFish), AreaId.Maridia, new(12, 7), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WateringHole), AreaId.Maridia, new(12, 4), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(NWestMaridiaBug), AreaId.Maridia, new(14, 4), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrabShaft), AreaId.Maridia, new(18, 7), new(2, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PseudoPlasmaSpark), AreaId.Maridia, new(18, 4), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CrabHole), AreaId.Maridia, new(16, 16), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestSandHallTunnel), AreaId.Maridia, new(17, 16), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PlasmaTutorial), AreaId.Maridia, new(26, 0), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Plasma), AreaId.Maridia, new(27, 0), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ThreadTheNeedle), AreaId.Maridia, new(27, 5), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MaridiaElev), AreaId.Maridia, new(34, 0), new(1, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PlasmaSpark), AreaId.Maridia, new(22, 2), new(4, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Kassiuz), AreaId.Maridia, new(25, 0), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MaridiaMap), AreaId.Maridia, new(17, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ForgottenHighwaySave), AreaId.Maridia, new(35, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Toilet), AreaId.Maridia, new(22, 5), new(1, 10), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BugSandHole), AreaId.Maridia, new(26, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestSandHall), AreaId.Maridia, new(18, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Oasis), AreaId.Maridia, new(22, 15), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastSandHall), AreaId.Maridia, new(23, 16), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestSandHole), AreaId.Maridia, new(20, 14), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastSandHole), AreaId.Maridia, new(23, 14), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestAqueductQuicksand), AreaId.Maridia, new(21, 12), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastAqueductQuicksand), AreaId.Maridia, new(23, 12), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Aqueduct), AreaId.Maridia, new(20, 9), new(6, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Butterfly), AreaId.Maridia, new(26, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BotwoonHallway), AreaId.Maridia, new(20, 8), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Pants), AreaId.Maridia, new(26, 13), new(2, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastPants), AreaId.Maridia, new(27, 14), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Springball), AreaId.Maridia, new(32, 15), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BelowBotwoonETank), AreaId.Maridia, new(26, 10), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Colosseum), AreaId.Maridia, new(34, 6), new(7, 2), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(AqueductSave), AreaId.Maridia, new(19, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(ThePrecious), AreaId.Maridia, new(41, 7), new(2, 3), 144, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BotwoonETank), AreaId.Maridia, new(26, 8), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DraygonSave), AreaId.Maridia, new(41, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MaridiaMissileRefill), AreaId.Maridia, new(38, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(PlasmaBeachQuicksand), AreaId.Maridia, new(26, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BotwoonQuicksand), AreaId.Maridia, new(28, 9), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Shaktool), AreaId.Maridia, new(28, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(HalfieClimb), AreaId.Maridia, new(33, 6), new(5, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Botwoon), AreaId.Maridia, new(24, 8), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(SpaceJump), AreaId.Maridia, new(38, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MaridiaEnergyRefill), AreaId.Maridia, new(42, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(WestCactusAlley), AreaId.Maridia, new(27, 6), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(EastCactusAlley), AreaId.Maridia, new(28, 6), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Draygon), AreaId.Maridia, new(39, 9), new(2, 2), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
        new(nameof(TourianFirst), AreaId.Tourian, new(20, 9), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Metroids1), AreaId.Tourian, new(14, 12), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Metroids2), AreaId.Tourian, new(13, 12), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Metroids3), AreaId.Tourian, new(14, 13), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Metroids4), AreaId.Tourian, new(20, 13), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BlueHopper), AreaId.Tourian, new(19, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DustTorizo), AreaId.Tourian, new(17, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(BigBoy), AreaId.Tourian, new(13, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Seaweed), AreaId.Tourian, new(12, 15), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianRecharge), AreaId.Tourian, new(11, 16), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MotherBrain), AreaId.Tourian, new(13, 18), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEyeDoor), AreaId.Tourian, new(13, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(RinkaShaft), AreaId.Tourian, new(17, 16), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MotherBrainSave), AreaId.Tourian, new(16, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEscape1), AreaId.Tourian, new(11, 18), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEscape2), AreaId.Tourian, new(11, 19), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEscape3), AreaId.Tourian, new(12, 19), new(6, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(TourianEscape4), AreaId.Tourian, new(18, 16), new(3, 6), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(UpperTourianSave), AreaId.Tourian, new(21, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CeresElev), AreaId.Ceres, new(12, 13), new(1, 3), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
        new(nameof(FallingTile), AreaId.Ceres, new(13, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(MagnetStairs), AreaId.Ceres, new(15, 15), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(DeadScientist), AreaId.Ceres, new(16, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(Room58Escape), AreaId.Ceres, new(18, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard),
        new(nameof(CeresRidley), AreaId.Ceres, new(20, 16), new(1, 1), 112, 160, RoomDoorTransitionStyle.ObjectsOnly),
    ];

    /// <summary>Every compiled room by its name.</summary>
    private static readonly Dictionary<string, RoomDefinition> ByName = Retail.ToDictionary(room => room.Name);

    /// <summary>The compiled room named <paramref name="name"/>.</summary>
    private static RoomDefinition Find(string name) => ByName[name];

    /// <summary>The Landing Site room.</summary>
    public static RoomDefinition LandingSite { get; } = Find(nameof(LandingSite));
    /// <summary>The Gauntlet Entrance room.</summary>
    public static RoomDefinition GauntletEntrance { get; } = Find(nameof(GauntletEntrance));
    /// <summary>The Parlor room.</summary>
    public static RoomDefinition Parlor { get; } = Find(nameof(Parlor));
    /// <summary>The Crateria Power Bombs room.</summary>
    public static RoomDefinition CrateriaPowerBombs { get; } = Find(nameof(CrateriaPowerBombs));
    /// <summary>The Crateria Save room.</summary>
    public static RoomDefinition CrateriaSave { get; } = Find(nameof(CrateriaSave));
    /// <summary>The West Ocean room.</summary>
    public static RoomDefinition WestOcean { get; } = Find(nameof(WestOcean));
    /// <summary>The Bowling Alley Path room.</summary>
    public static RoomDefinition BowlingAlleyPath { get; } = Find(nameof(BowlingAlleyPath));
    /// <summary>The Crateria Kihunter room.</summary>
    public static RoomDefinition CrateriaKihunter { get; } = Find(nameof(CrateriaKihunter));
    /// <summary>The Forgotten Highway Elevator room.</summary>
    public static RoomDefinition ForgottenHighwayElev { get; } = Find(nameof(ForgottenHighwayElev));
    /// <summary>The East Ocean room.</summary>
    public static RoomDefinition EastOcean { get; } = Find(nameof(EastOcean));
    /// <summary>The Forgotten Highway Kagos room.</summary>
    public static RoomDefinition ForgottenHighwayKagos { get; } = Find(nameof(ForgottenHighwayKagos));
    /// <summary>The Crab Maze room.</summary>
    public static RoomDefinition CrabMaze { get; } = Find(nameof(CrabMaze));
    /// <summary>The Forgotten Highway Elbow room.</summary>
    public static RoomDefinition ForgottenHighwayElbow { get; } = Find(nameof(ForgottenHighwayElbow));
    /// <summary>The Crateria Tube room.</summary>
    public static RoomDefinition CrateriaTube { get; } = Find(nameof(CrateriaTube));
    /// <summary>The Moat room.</summary>
    public static RoomDefinition Moat { get; } = Find(nameof(Moat));
    /// <summary>The Red Brinstar Elevator room.</summary>
    public static RoomDefinition RedBinstarElev { get; } = Find(nameof(RedBinstarElev));
    /// <summary>The Gauntlet Energy Tank room.</summary>
    public static RoomDefinition GauntletETank { get; } = Find(nameof(GauntletETank));
    /// <summary>The Pre Bowling room.</summary>
    public static RoomDefinition PreBowling { get; } = Find(nameof(PreBowling));
    /// <summary>The Climb room.</summary>
    public static RoomDefinition Climb { get; } = Find(nameof(Climb));
    /// <summary>The Pit room.</summary>
    public static RoomDefinition Pit { get; } = Find(nameof(Pit));
    /// <summary>The Elevator to Blue Brinstar room.</summary>
    public static RoomDefinition ElevToBlueBrinstar { get; } = Find(nameof(ElevToBlueBrinstar));
    /// <summary>The Bomb Torizo room.</summary>
    public static RoomDefinition BombTorizo { get; } = Find(nameof(BombTorizo));
    /// <summary>The Flyway room.</summary>
    public static RoomDefinition Flyway { get; } = Find(nameof(Flyway));
    /// <summary>The Pre Map Flyway room.</summary>
    public static RoomDefinition PreMapFlyway { get; } = Find(nameof(PreMapFlyway));
    /// <summary>The Terminator room.</summary>
    public static RoomDefinition Terminator { get; } = Find(nameof(Terminator));
    /// <summary>The Green Brinstar Elevator room.</summary>
    public static RoomDefinition GreenBrinstarElev { get; } = Find(nameof(GreenBrinstarElev));
    /// <summary>The Lower Mushrooms room.</summary>
    public static RoomDefinition LowerMushrooms { get; } = Find(nameof(LowerMushrooms));
    /// <summary>The Crateria Map room.</summary>
    public static RoomDefinition CrateriaMap { get; } = Find(nameof(CrateriaMap));
    /// <summary>The Green Pirates Shaft room.</summary>
    public static RoomDefinition GreenPiratesShaft { get; } = Find(nameof(GreenPiratesShaft));
    /// <summary>The Crateria Super room.</summary>
    public static RoomDefinition CrateriaSuper { get; } = Find(nameof(CrateriaSuper));
    /// <summary>The Final Missile Bombway room.</summary>
    public static RoomDefinition FinalMissileBombway { get; } = Find(nameof(FinalMissileBombway));
    /// <summary>The Final Missile room.</summary>
    public static RoomDefinition FinalMissile { get; } = Find(nameof(FinalMissile));
    /// <summary>The Green Brinstar Main Shaft room.</summary>
    public static RoomDefinition GreenBrinstarMainShaft { get; } = Find(nameof(GreenBrinstarMainShaft));
    /// <summary>The Spore Spawn Super room.</summary>
    public static RoomDefinition SporeSpawnSuper { get; } = Find(nameof(SporeSpawnSuper));
    /// <summary>The Brinstar Pre Map room.</summary>
    public static RoomDefinition BrinstarPreMap { get; } = Find(nameof(BrinstarPreMap));
    /// <summary>The Early Supers room.</summary>
    public static RoomDefinition EarlySupers { get; } = Find(nameof(EarlySupers));
    /// <summary>The Brinstar Reserve Tank room.</summary>
    public static RoomDefinition BrinstarReserveTank { get; } = Find(nameof(BrinstarReserveTank));
    /// <summary>The Green Brinstar Map room.</summary>
    public static RoomDefinition GreenBrinstarMap { get; } = Find(nameof(GreenBrinstarMap));
    /// <summary>The Green Brinstar Firefleas room.</summary>
    public static RoomDefinition GreenBrinstarFirefleas { get; } = Find(nameof(GreenBrinstarFirefleas));
    /// <summary>The Green Brinstar Missile Refill room.</summary>
    public static RoomDefinition GreenBrinstarMissileRefill { get; } = Find(nameof(GreenBrinstarMissileRefill));
    /// <summary>The Dachora room.</summary>
    public static RoomDefinition Dachora { get; } = Find(nameof(Dachora));
    /// <summary>The Big Pink room.</summary>
    public static RoomDefinition BigPink { get; } = Find(nameof(BigPink));
    /// <summary>The Spore Spawn Kihunters room.</summary>
    public static RoomDefinition SporeSpawnKihunters { get; } = Find(nameof(SporeSpawnKihunters));
    /// <summary>The Spore Spawn room.</summary>
    public static RoomDefinition SporeSpawn { get; } = Find(nameof(SporeSpawn));
    /// <summary>The Pink Brinstar Power Bombs room.</summary>
    public static RoomDefinition PinkBrinstarPowerBombs { get; } = Find(nameof(PinkBrinstarPowerBombs));
    /// <summary>The Green Hill Zone room.</summary>
    public static RoomDefinition GreenHillZone { get; } = Find(nameof(GreenHillZone));
    /// <summary>The Morph Ball room.</summary>
    public static RoomDefinition MorphBall { get; } = Find(nameof(MorphBall));
    /// <summary>The Construction Zone room.</summary>
    public static RoomDefinition ConstructionZone { get; } = Find(nameof(ConstructionZone));
    /// <summary>The Blue Brinstar Energy Tank room.</summary>
    public static RoomDefinition BlueBrinstarETank { get; } = Find(nameof(BlueBrinstarETank));
    /// <summary>The Noob Bridge room.</summary>
    public static RoomDefinition NoobBridge { get; } = Find(nameof(NoobBridge));
    /// <summary>The Green Brinstar Beetoms room.</summary>
    public static RoomDefinition GreenBrinstarBeetoms { get; } = Find(nameof(GreenBrinstarBeetoms));
    /// <summary>The Etecoon Energy Tank room.</summary>
    public static RoomDefinition EtecoonETank { get; } = Find(nameof(EtecoonETank));
    /// <summary>The Etecoon Super room.</summary>
    public static RoomDefinition EtecoonSuper { get; } = Find(nameof(EtecoonSuper));
    /// <summary>The Dachora Energy Refill room.</summary>
    public static RoomDefinition DachoraEnergyRefill { get; } = Find(nameof(DachoraEnergyRefill));
    /// <summary>The Spore Spawn Farming room.</summary>
    public static RoomDefinition SporeSpawnFarming { get; } = Find(nameof(SporeSpawnFarming));
    /// <summary>The Waterway Energy Tank room.</summary>
    public static RoomDefinition WaterwayETank { get; } = Find(nameof(WaterwayETank));
    /// <summary>The First Missile room.</summary>
    public static RoomDefinition FirstMissile { get; } = Find(nameof(FirstMissile));
    /// <summary>The Pink Brinstar Hoppers room.</summary>
    public static RoomDefinition PinkBrinstarHoppers { get; } = Find(nameof(PinkBrinstarHoppers));
    /// <summary>The Hopper Energy Tank room.</summary>
    public static RoomDefinition HopperETank { get; } = Find(nameof(HopperETank));
    /// <summary>The Big Pink Save Room.</summary>
    public static RoomDefinition BigPinkSaveRoom { get; } = Find(nameof(BigPinkSaveRoom));
    /// <summary>The Blue Brinstar Boulders room.</summary>
    public static RoomDefinition BlueBrinstarBoulders { get; } = Find(nameof(BlueBrinstarBoulders));
    /// <summary>The Billy Mays room.</summary>
    public static RoomDefinition BillyMays { get; } = Find(nameof(BillyMays));
    /// <summary>The Green Brinstar Save room.</summary>
    public static RoomDefinition GreenBrinstarSave { get; } = Find(nameof(GreenBrinstarSave));
    /// <summary>The Etecoon Save room.</summary>
    public static RoomDefinition EtecoonSave { get; } = Find(nameof(EtecoonSave));
    /// <summary>The Red Tower room.</summary>
    public static RoomDefinition RedTower { get; } = Find(nameof(RedTower));
    /// <summary>The Red Brinstar Firefleas room.</summary>
    public static RoomDefinition RedBrinstarFirefleas { get; } = Find(nameof(RedBrinstarFirefleas));
    /// <summary>The X-Ray Scope room.</summary>
    public static RoomDefinition XrayScope { get; } = Find(nameof(XrayScope));
    /// <summary>The Hellway room.</summary>
    public static RoomDefinition Hellway { get; } = Find(nameof(Hellway));
    /// <summary>The Caterpillar room.</summary>
    public static RoomDefinition Caterpillar { get; } = Find(nameof(Caterpillar));
    /// <summary>The Beta Power Bomb room.</summary>
    public static RoomDefinition BetaPowerBomb { get; } = Find(nameof(BetaPowerBomb));
    /// <summary>The Alpha Power Bomb room.</summary>
    public static RoomDefinition AlphaPowerBomb { get; } = Find(nameof(AlphaPowerBomb));
    /// <summary>The Skree Boost room.</summary>
    public static RoomDefinition SkreeBoost { get; } = Find(nameof(SkreeBoost));
    /// <summary>The Below Spazer room.</summary>
    public static RoomDefinition BelowSpazer { get; } = Find(nameof(BelowSpazer));
    /// <summary>The Spazer room.</summary>
    public static RoomDefinition Spazer { get; } = Find(nameof(Spazer));
    /// <summary>The Warehouse Zeela room.</summary>
    public static RoomDefinition WarehouseZeela { get; } = Find(nameof(WarehouseZeela));
    /// <summary>The Warehouse Energy Tank room.</summary>
    public static RoomDefinition WarehouseETank { get; } = Find(nameof(WarehouseETank));
    /// <summary>The Warehouse Kihunter room.</summary>
    public static RoomDefinition WarehouseKihunter { get; } = Find(nameof(WarehouseKihunter));
    /// <summary>The Mini Kraid room.</summary>
    public static RoomDefinition MiniKraid { get; } = Find(nameof(MiniKraid));
    /// <summary>The Kraid Eye Door room.</summary>
    public static RoomDefinition KraidEyeDoor { get; } = Find(nameof(KraidEyeDoor));
    /// <summary>The Kraid room.</summary>
    public static RoomDefinition Kraid { get; } = Find(nameof(Kraid));
    /// <summary>The Statues Hallway room.</summary>
    public static RoomDefinition StatuesHallway { get; } = Find(nameof(StatuesHallway));
    /// <summary>The Red Tower Refill room.</summary>
    public static RoomDefinition RedTowerRefill { get; } = Find(nameof(RedTowerRefill));
    /// <summary>The Kraid Refill room.</summary>
    public static RoomDefinition KraidRefill { get; } = Find(nameof(KraidRefill));
    /// <summary>The Statues room.</summary>
    public static RoomDefinition Statues { get; } = Find(nameof(Statues));
    /// <summary>The Warehouse Entrance room.</summary>
    public static RoomDefinition WarehouseEntrance { get; } = Find(nameof(WarehouseEntrance));
    /// <summary>The Varia Suit room.</summary>
    public static RoomDefinition VariaSuit { get; } = Find(nameof(VariaSuit));
    /// <summary>The Warehouse Save room.</summary>
    public static RoomDefinition WarehouseSave { get; } = Find(nameof(WarehouseSave));
    /// <summary>The Red Brinstar Save room.</summary>
    public static RoomDefinition RedBrinstarSave { get; } = Find(nameof(RedBrinstarSave));
    /// <summary>The Ice Beam Acid room.</summary>
    public static RoomDefinition IceBeamAcid { get; } = Find(nameof(IceBeamAcid));
    /// <summary>The Cathedral room.</summary>
    public static RoomDefinition Cathedral { get; } = Find(nameof(Cathedral));
    /// <summary>The Cathedral Entrance room.</summary>
    public static RoomDefinition CathedralEntrance { get; } = Find(nameof(CathedralEntrance));
    /// <summary>The Business Center room.</summary>
    public static RoomDefinition BusinessCenter { get; } = Find(nameof(BusinessCenter));
    /// <summary>The Ice Beam Gate room.</summary>
    public static RoomDefinition IceBeamGate { get; } = Find(nameof(IceBeamGate));
    /// <summary>The Ice Beam Tutorial room.</summary>
    public static RoomDefinition IceBeamTutorial { get; } = Find(nameof(IceBeamTutorial));
    /// <summary>The Ice Beam room.</summary>
    public static RoomDefinition IceBeam { get; } = Find(nameof(IceBeam));
    /// <summary>The Ice Beam Snake room.</summary>
    public static RoomDefinition IceBeamSnake { get; } = Find(nameof(IceBeamSnake));
    /// <summary>The Crumble Shaft room.</summary>
    public static RoomDefinition CrumbleShaft { get; } = Find(nameof(CrumbleShaft));
    /// <summary>The Crocomire Speedway room.</summary>
    public static RoomDefinition CrocomireSpeedway { get; } = Find(nameof(CrocomireSpeedway));
    /// <summary>The Crocomire room.</summary>
    public static RoomDefinition Crocomire { get; } = Find(nameof(Crocomire));
    /// <summary>The Hi-Jump Boots room.</summary>
    public static RoomDefinition HiJumpBoots { get; } = Find(nameof(HiJumpBoots));
    /// <summary>The Crocomire Escape room.</summary>
    public static RoomDefinition CrocomireEscape { get; } = Find(nameof(CrocomireEscape));
    /// <summary>The Hi-Jump Energy Tank room.</summary>
    public static RoomDefinition HiJumpETank { get; } = Find(nameof(HiJumpETank));
    /// <summary>The Post-Crocomire Farming room.</summary>
    public static RoomDefinition PostCrocFarming { get; } = Find(nameof(PostCrocFarming));
    /// <summary>The Post-Crocomire Save room.</summary>
    public static RoomDefinition PostCrocSave { get; } = Find(nameof(PostCrocSave));
    /// <summary>The Post-Crocomire Power Bombs room.</summary>
    public static RoomDefinition PostCrocPowerBombs { get; } = Find(nameof(PostCrocPowerBombs));
    /// <summary>The Post-Crocomire Shaft room.</summary>
    public static RoomDefinition PostCrocShaft { get; } = Find(nameof(PostCrocShaft));
    /// <summary>The Post-Crocomire Missile room.</summary>
    public static RoomDefinition PostCrocMissile { get; } = Find(nameof(PostCrocMissile));
    /// <summary>The Grapple Tutorial 3 room.</summary>
    public static RoomDefinition GrappleTutorial3 { get; } = Find(nameof(GrappleTutorial3));
    /// <summary>The Post-Crocomire Jump room.</summary>
    public static RoomDefinition PostCrocJump { get; } = Find(nameof(PostCrocJump));
    /// <summary>The Grapple Tutorial 2 room.</summary>
    public static RoomDefinition GrappleTutorial2 { get; } = Find(nameof(GrappleTutorial2));
    /// <summary>The Grapple Tutorial 1 room.</summary>
    public static RoomDefinition GrappleTutorial1 { get; } = Find(nameof(GrappleTutorial1));
    /// <summary>The Grapple Beam room.</summary>
    public static RoomDefinition GrappleBeam { get; } = Find(nameof(GrappleBeam));
    /// <summary>The Norfair Reserve Tank room.</summary>
    public static RoomDefinition NorfairReserveTank { get; } = Find(nameof(NorfairReserveTank));
    /// <summary>The Green Bubbles Missiles room.</summary>
    public static RoomDefinition GreenBubblesMissiles { get; } = Find(nameof(GreenBubblesMissiles));
    /// <summary>The Bubble Mountain room.</summary>
    public static RoomDefinition BubbleMountain { get; } = Find(nameof(BubbleMountain));
    /// <summary>The Speed Booster Hall room.</summary>
    public static RoomDefinition SpeedBoosterHall { get; } = Find(nameof(SpeedBoosterHall));
    /// <summary>The Speed Booster room.</summary>
    public static RoomDefinition SpeedBooster { get; } = Find(nameof(SpeedBooster));
    /// <summary>The Single Chamber room.</summary>
    public static RoomDefinition SingleChamber { get; } = Find(nameof(SingleChamber));
    /// <summary>The Double Chamber room.</summary>
    public static RoomDefinition DoubleChamber { get; } = Find(nameof(DoubleChamber));
    /// <summary>The Wave Beam room.</summary>
    public static RoomDefinition WaveBeam { get; } = Find(nameof(WaveBeam));
    /// <summary>The Spiky Platforms Tunnel room.</summary>
    public static RoomDefinition SpikyPlatformsTunnel { get; } = Find(nameof(SpikyPlatformsTunnel));
    /// <summary>The Volcano room.</summary>
    public static RoomDefinition Volcano { get; } = Find(nameof(Volcano));
    /// <summary>The Kronic Boost room.</summary>
    public static RoomDefinition KronicBoost { get; } = Find(nameof(KronicBoost));
    /// <summary>The Magdollite Tunnel room.</summary>
    public static RoomDefinition MagdolliteTunnel { get; } = Find(nameof(MagdolliteTunnel));
    /// <summary>The Purple Shaft room.</summary>
    public static RoomDefinition PurpleShaft { get; } = Find(nameof(PurpleShaft));
    /// <summary>The Lava Dive room.</summary>
    public static RoomDefinition LavaDive { get; } = Find(nameof(LavaDive));
    /// <summary>The Lower Norfair Elevator room.</summary>
    public static RoomDefinition LowerNorfairElev { get; } = Find(nameof(LowerNorfairElev));
    /// <summary>The Upper Norfair Farming room.</summary>
    public static RoomDefinition UpperNorfairFarming { get; } = Find(nameof(UpperNorfairFarming));
    /// <summary>The Rising Tide room.</summary>
    public static RoomDefinition RisingTide { get; } = Find(nameof(RisingTide));
    /// <summary>The Acid Snakes Tunnel room.</summary>
    public static RoomDefinition AcidSnakesTunnel { get; } = Find(nameof(AcidSnakesTunnel));
    /// <summary>The Spiky Acid Snakes Tunnel room.</summary>
    public static RoomDefinition SpikyAcidSnakesTunnel { get; } = Find(nameof(SpikyAcidSnakesTunnel));
    /// <summary>The Upper Norfair Refill room.</summary>
    public static RoomDefinition UpperNorfairRefill { get; } = Find(nameof(UpperNorfairRefill));
    /// <summary>The Purple Farming room.</summary>
    public static RoomDefinition PurpleFarming { get; } = Find(nameof(PurpleFarming));
    /// <summary>The Bat Cave room.</summary>
    public static RoomDefinition BatCave { get; } = Find(nameof(BatCave));
    /// <summary>The Norfair Map room.</summary>
    public static RoomDefinition NorfairMap { get; } = Find(nameof(NorfairMap));
    /// <summary>The Bubble Mountain Save room.</summary>
    public static RoomDefinition BubbleMountainSave { get; } = Find(nameof(BubbleMountainSave));
    /// <summary>The Frog Speedway room.</summary>
    public static RoomDefinition FrogSpeedway { get; } = Find(nameof(FrogSpeedway));
    /// <summary>The Red Pirate Shaft room.</summary>
    public static RoomDefinition RedPirateShaft { get; } = Find(nameof(RedPirateShaft));
    /// <summary>The Frog Save room.</summary>
    public static RoomDefinition FrogSave { get; } = Find(nameof(FrogSave));
    /// <summary>The Crocomire Save room.</summary>
    public static RoomDefinition CrocomireSave { get; } = Find(nameof(CrocomireSave));
    /// <summary>The Lower Norfair Elevator Save room.</summary>
    public static RoomDefinition LNElevSave { get; } = Find(nameof(LNElevSave));
    /// <summary>The Acid Statue room.</summary>
    public static RoomDefinition AcidStatue { get; } = Find(nameof(AcidStatue));
    /// <summary>The Main Hall room.</summary>
    public static RoomDefinition MainHall { get; } = Find(nameof(MainHall));
    /// <summary>The Golden Torizo room.</summary>
    public static RoomDefinition GoldenTorizo { get; } = Find(nameof(GoldenTorizo));
    /// <summary>The Fast Ripper room.</summary>
    public static RoomDefinition FastRipper { get; } = Find(nameof(FastRipper));
    /// <summary>The Golden Torizo Energy Refill room.</summary>
    public static RoomDefinition GTEnergyRefill { get; } = Find(nameof(GTEnergyRefill));
    /// <summary>The Ridley room.</summary>
    public static RoomDefinition Ridley { get; } = Find(nameof(Ridley));
    /// <summary>The Lower Norfair Farming room.</summary>
    public static RoomDefinition LNFarming { get; } = Find(nameof(LNFarming));
    /// <summary>The Fast Pillars Setup room.</summary>
    public static RoomDefinition FastPillarsSetup { get; } = Find(nameof(FastPillarsSetup));
    /// <summary>The Mickey Mouse room.</summary>
    public static RoomDefinition MickeyMouse { get; } = Find(nameof(MickeyMouse));
    /// <summary>The Pillar room.</summary>
    public static RoomDefinition Pillar { get; } = Find(nameof(Pillar));
    /// <summary>The Plowerhouse room.</summary>
    public static RoomDefinition Plowerhouse { get; } = Find(nameof(Plowerhouse));
    /// <summary>The Worst Room in the Game.</summary>
    public static RoomDefinition WorstRoomInTheGame { get; } = Find(nameof(WorstRoomInTheGame));
    /// <summary>The Amphitheatre room.</summary>
    public static RoomDefinition Amphitheatre { get; } = Find(nameof(Amphitheatre));
    /// <summary>The Lower Norfair Spring Ball Maze room.</summary>
    public static RoomDefinition LNSpringBallMaze { get; } = Find(nameof(LNSpringBallMaze));
    /// <summary>The Lower Norfair Escape Power Bombs room.</summary>
    public static RoomDefinition LNEscapePowerBombs { get; } = Find(nameof(LNEscapePowerBombs));
    /// <summary>The Red Kihunter Shaft room.</summary>
    public static RoomDefinition RedKihunterShaft { get; } = Find(nameof(RedKihunterShaft));
    /// <summary>The Wasteland room.</summary>
    public static RoomDefinition Wasteland { get; } = Find(nameof(Wasteland));
    /// <summary>The Metal Pirates room.</summary>
    public static RoomDefinition MetalPirates { get; } = Find(nameof(MetalPirates));
    /// <summary>The Three Musketeers room.</summary>
    public static RoomDefinition ThreeMusketeers { get; } = Find(nameof(ThreeMusketeers));
    /// <summary>The Ridley Energy Tank room.</summary>
    public static RoomDefinition RidleyETank { get; } = Find(nameof(RidleyETank));
    /// <summary>The Screw Attack room.</summary>
    public static RoomDefinition ScrewAttack { get; } = Find(nameof(ScrewAttack));
    /// <summary>The Lower Norfair Fireflea room.</summary>
    public static RoomDefinition LNFireflea { get; } = Find(nameof(LNFireflea));
    /// <summary>The Lower Norfair Save room.</summary>
    public static RoomDefinition LNSave { get; } = Find(nameof(LNSave));
    /// <summary>The Bowling Alley room.</summary>
    public static RoomDefinition BowlingAlley { get; } = Find(nameof(BowlingAlley));
    /// <summary>The Wrecked Ship Entrance room.</summary>
    public static RoomDefinition WreckedShipEntrance { get; } = Find(nameof(WreckedShipEntrance));
    /// <summary>The Attic room.</summary>
    public static RoomDefinition Attic { get; } = Find(nameof(Attic));
    /// <summary>Wrecked Ship's Assembly Line room.</summary>
    public static RoomDefinition AssemblyLine { get; } = Find(nameof(AssemblyLine));
    /// <summary>The Wrecked Ship Main Shaft room.</summary>
    public static RoomDefinition WreckedShipMainShaft { get; } = Find(nameof(WreckedShipMainShaft));
    /// <summary>The Spiky Death room.</summary>
    public static RoomDefinition SpikyDeath { get; } = Find(nameof(SpikyDeath));
    /// <summary>The Electric Death room.</summary>
    public static RoomDefinition ElectricDeath { get; } = Find(nameof(ElectricDeath));
    /// <summary>The Wrecked Ship Energy Tank room.</summary>
    public static RoomDefinition WreckedShipETank { get; } = Find(nameof(WreckedShipETank));
    /// <summary>The Basement room.</summary>
    public static RoomDefinition Basement { get; } = Find(nameof(Basement));
    /// <summary>The Wrecked Ship Map room.</summary>
    public static RoomDefinition WreckedShipMap { get; } = Find(nameof(WreckedShipMap));
    /// <summary>The Phantoon room.</summary>
    public static RoomDefinition Phantoon { get; } = Find(nameof(Phantoon));
    /// <summary>The Sponge Bath room.</summary>
    public static RoomDefinition SpongeBath { get; } = Find(nameof(SpongeBath));
    /// <summary>The Wrecked Ship West Super room.</summary>
    public static RoomDefinition WSWestSuper { get; } = Find(nameof(WSWestSuper));
    /// <summary>The Wrecked Ship East Super room.</summary>
    public static RoomDefinition WSEastSuper { get; } = Find(nameof(WSEastSuper));
    /// <summary>The Gravity Suit room.</summary>
    public static RoomDefinition GravitySuit { get; } = Find(nameof(GravitySuit));
    /// <summary>The Wrecked Ship Save room.</summary>
    public static RoomDefinition WreckedShipSave { get; } = Find(nameof(WreckedShipSave));
    /// <summary>The Glass Tunnel Save room.</summary>
    public static RoomDefinition GlassTunnelSave { get; } = Find(nameof(GlassTunnelSave));
    /// <summary>The Glass Tunnel room.</summary>
    public static RoomDefinition GlassTunnel { get; } = Find(nameof(GlassTunnel));
    /// <summary>The West Tunnel room.</summary>
    public static RoomDefinition WestTunnel { get; } = Find(nameof(WestTunnel));
    /// <summary>The East Tunnel room.</summary>
    public static RoomDefinition EastTunnel { get; } = Find(nameof(EastTunnel));
    /// <summary>The Main Street room.</summary>
    public static RoomDefinition MainStreet { get; } = Find(nameof(MainStreet));
    /// <summary>The Fish Tank room.</summary>
    public static RoomDefinition FishTank { get; } = Find(nameof(FishTank));
    /// <summary>The Mama Turtle room.</summary>
    public static RoomDefinition MamaTurtle { get; } = Find(nameof(MamaTurtle));
    /// <summary>The Crab Tunnel room.</summary>
    public static RoomDefinition CrabTunnel { get; } = Find(nameof(CrabTunnel));
    /// <summary>The Mt. Everest room.</summary>
    public static RoomDefinition MtEverest { get; } = Find(nameof(MtEverest));
    /// <summary>The Red Fish room.</summary>
    public static RoomDefinition RedFish { get; } = Find(nameof(RedFish));
    /// <summary>The Watering Hole room.</summary>
    public static RoomDefinition WateringHole { get; } = Find(nameof(WateringHole));
    /// <summary>The Northwest Maridia Bug room.</summary>
    public static RoomDefinition NWestMaridiaBug { get; } = Find(nameof(NWestMaridiaBug));
    /// <summary>The Crab Shaft room.</summary>
    public static RoomDefinition CrabShaft { get; } = Find(nameof(CrabShaft));
    /// <summary>The Pseudo Plasma Spark room.</summary>
    public static RoomDefinition PseudoPlasmaSpark { get; } = Find(nameof(PseudoPlasmaSpark));
    /// <summary>The Crab Hole room.</summary>
    public static RoomDefinition CrabHole { get; } = Find(nameof(CrabHole));
    /// <summary>The West Sand Hall Tunnel room.</summary>
    public static RoomDefinition WestSandHallTunnel { get; } = Find(nameof(WestSandHallTunnel));
    /// <summary>The Plasma Tutorial room.</summary>
    public static RoomDefinition PlasmaTutorial { get; } = Find(nameof(PlasmaTutorial));
    /// <summary>The Plasma room.</summary>
    public static RoomDefinition Plasma { get; } = Find(nameof(Plasma));
    /// <summary>The Thread the Needle room.</summary>
    public static RoomDefinition ThreadTheNeedle { get; } = Find(nameof(ThreadTheNeedle));
    /// <summary>The Maridia Elevator room.</summary>
    public static RoomDefinition MaridiaElev { get; } = Find(nameof(MaridiaElev));
    /// <summary>The Plasma Spark room.</summary>
    public static RoomDefinition PlasmaSpark { get; } = Find(nameof(PlasmaSpark));
    /// <summary>The Kassiuz room.</summary>
    public static RoomDefinition Kassiuz { get; } = Find(nameof(Kassiuz));
    /// <summary>The Maridia Map room.</summary>
    public static RoomDefinition MaridiaMap { get; } = Find(nameof(MaridiaMap));
    /// <summary>The Forgotten Highway Save room.</summary>
    public static RoomDefinition ForgottenHighwaySave { get; } = Find(nameof(ForgottenHighwaySave));
    /// <summary>The Toilet room.</summary>
    public static RoomDefinition Toilet { get; } = Find(nameof(Toilet));
    /// <summary>The Bug Sand Hole room.</summary>
    public static RoomDefinition BugSandHole { get; } = Find(nameof(BugSandHole));
    /// <summary>The West Sand Hall room.</summary>
    public static RoomDefinition WestSandHall { get; } = Find(nameof(WestSandHall));
    /// <summary>The Oasis room.</summary>
    public static RoomDefinition Oasis { get; } = Find(nameof(Oasis));
    /// <summary>The East Sand Hall room.</summary>
    public static RoomDefinition EastSandHall { get; } = Find(nameof(EastSandHall));
    /// <summary>The West Sand Hole room.</summary>
    public static RoomDefinition WestSandHole { get; } = Find(nameof(WestSandHole));
    /// <summary>The East Sand Hole room.</summary>
    public static RoomDefinition EastSandHole { get; } = Find(nameof(EastSandHole));
    /// <summary>The West Aqueduct Quicksand room.</summary>
    public static RoomDefinition WestAqueductQuicksand { get; } = Find(nameof(WestAqueductQuicksand));
    /// <summary>The East Aqueduct Quicksand room.</summary>
    public static RoomDefinition EastAqueductQuicksand { get; } = Find(nameof(EastAqueductQuicksand));
    /// <summary>The Aqueduct room.</summary>
    public static RoomDefinition Aqueduct { get; } = Find(nameof(Aqueduct));
    /// <summary>The Butterfly room.</summary>
    public static RoomDefinition Butterfly { get; } = Find(nameof(Butterfly));
    /// <summary>The Botwoon Hallway room.</summary>
    public static RoomDefinition BotwoonHallway { get; } = Find(nameof(BotwoonHallway));
    /// <summary>The Pants room.</summary>
    public static RoomDefinition Pants { get; } = Find(nameof(Pants));
    /// <summary>The East Pants room.</summary>
    public static RoomDefinition EastPants { get; } = Find(nameof(EastPants));
    /// <summary>The Springball room.</summary>
    public static RoomDefinition Springball { get; } = Find(nameof(Springball));
    /// <summary>The Below Botwoon Energy Tank room.</summary>
    public static RoomDefinition BelowBotwoonETank { get; } = Find(nameof(BelowBotwoonETank));
    /// <summary>The Colosseum room.</summary>
    public static RoomDefinition Colosseum { get; } = Find(nameof(Colosseum));
    /// <summary>The Aqueduct Save room.</summary>
    public static RoomDefinition AqueductSave { get; } = Find(nameof(AqueductSave));
    /// <summary>The Precious room.</summary>
    public static RoomDefinition ThePrecious { get; } = Find(nameof(ThePrecious));
    /// <summary>The Botwoon Energy Tank room.</summary>
    public static RoomDefinition BotwoonETank { get; } = Find(nameof(BotwoonETank));
    /// <summary>The Draygon Save room.</summary>
    public static RoomDefinition DraygonSave { get; } = Find(nameof(DraygonSave));
    /// <summary>The Maridia Missile Refill room.</summary>
    public static RoomDefinition MaridiaMissileRefill { get; } = Find(nameof(MaridiaMissileRefill));
    /// <summary>The Plasma Beach Quicksand room.</summary>
    public static RoomDefinition PlasmaBeachQuicksand { get; } = Find(nameof(PlasmaBeachQuicksand));
    /// <summary>The Botwoon Quicksand room.</summary>
    public static RoomDefinition BotwoonQuicksand { get; } = Find(nameof(BotwoonQuicksand));
    /// <summary>The Shaktool room.</summary>
    public static RoomDefinition Shaktool { get; } = Find(nameof(Shaktool));
    /// <summary>The Halfie Climb room.</summary>
    public static RoomDefinition HalfieClimb { get; } = Find(nameof(HalfieClimb));
    /// <summary>The Botwoon room.</summary>
    public static RoomDefinition Botwoon { get; } = Find(nameof(Botwoon));
    /// <summary>The Space Jump room.</summary>
    public static RoomDefinition SpaceJump { get; } = Find(nameof(SpaceJump));
    /// <summary>The Maridia Energy Refill room.</summary>
    public static RoomDefinition MaridiaEnergyRefill { get; } = Find(nameof(MaridiaEnergyRefill));
    /// <summary>The West Cactus Alley room.</summary>
    public static RoomDefinition WestCactusAlley { get; } = Find(nameof(WestCactusAlley));
    /// <summary>The East Cactus Alley room.</summary>
    public static RoomDefinition EastCactusAlley { get; } = Find(nameof(EastCactusAlley));
    /// <summary>The Draygon room.</summary>
    public static RoomDefinition Draygon { get; } = Find(nameof(Draygon));
    /// <summary>The Tourian First room.</summary>
    public static RoomDefinition TourianFirst { get; } = Find(nameof(TourianFirst));
    /// <summary>The Metroids 1 room.</summary>
    public static RoomDefinition Metroids1 { get; } = Find(nameof(Metroids1));
    /// <summary>The Metroids 2 room.</summary>
    public static RoomDefinition Metroids2 { get; } = Find(nameof(Metroids2));
    /// <summary>The Metroids 3 room.</summary>
    public static RoomDefinition Metroids3 { get; } = Find(nameof(Metroids3));
    /// <summary>The Metroids 4 room.</summary>
    public static RoomDefinition Metroids4 { get; } = Find(nameof(Metroids4));
    /// <summary>The Blue Hopper room.</summary>
    public static RoomDefinition BlueHopper { get; } = Find(nameof(BlueHopper));
    /// <summary>The Dust Torizo room.</summary>
    public static RoomDefinition DustTorizo { get; } = Find(nameof(DustTorizo));
    /// <summary>The Big Boy room.</summary>
    public static RoomDefinition BigBoy { get; } = Find(nameof(BigBoy));
    /// <summary>The Seaweed room.</summary>
    public static RoomDefinition Seaweed { get; } = Find(nameof(Seaweed));
    /// <summary>The Tourian Recharge room.</summary>
    public static RoomDefinition TourianRecharge { get; } = Find(nameof(TourianRecharge));
    /// <summary>The Mother Brain room.</summary>
    public static RoomDefinition MotherBrain { get; } = Find(nameof(MotherBrain));
    /// <summary>The Tourian Eye Door room.</summary>
    public static RoomDefinition TourianEyeDoor { get; } = Find(nameof(TourianEyeDoor));
    /// <summary>The Rinka Shaft room.</summary>
    public static RoomDefinition RinkaShaft { get; } = Find(nameof(RinkaShaft));
    /// <summary>The Mother Brain Save room.</summary>
    public static RoomDefinition MotherBrainSave { get; } = Find(nameof(MotherBrainSave));
    /// <summary>The Tourian Escape 1 room.</summary>
    public static RoomDefinition TourianEscape1 { get; } = Find(nameof(TourianEscape1));
    /// <summary>The Tourian Escape 2 room.</summary>
    public static RoomDefinition TourianEscape2 { get; } = Find(nameof(TourianEscape2));
    /// <summary>The Tourian Escape 3 room.</summary>
    public static RoomDefinition TourianEscape3 { get; } = Find(nameof(TourianEscape3));
    /// <summary>The Tourian Escape 4 room.</summary>
    public static RoomDefinition TourianEscape4 { get; } = Find(nameof(TourianEscape4));
    /// <summary>The Upper Tourian Save room.</summary>
    public static RoomDefinition UpperTourianSave { get; } = Find(nameof(UpperTourianSave));
    /// <summary>The Ceres Elevator room.</summary>
    public static RoomDefinition CeresElev { get; } = Find(nameof(CeresElev));
    /// <summary>The Falling Tile room.</summary>
    public static RoomDefinition FallingTile { get; } = Find(nameof(FallingTile));
    /// <summary>The Magnet Stairs room.</summary>
    public static RoomDefinition MagnetStairs { get; } = Find(nameof(MagnetStairs));
    /// <summary>The Dead Scientist room.</summary>
    public static RoomDefinition DeadScientist { get; } = Find(nameof(DeadScientist));
    /// <summary>The Room 58 Escape.</summary>
    public static RoomDefinition Room58Escape { get; } = Find(nameof(Room58Escape));
    /// <summary>The Ceres Ridley room.</summary>
    public static RoomDefinition CeresRidley { get; } = Find(nameof(CeresRidley));
}
