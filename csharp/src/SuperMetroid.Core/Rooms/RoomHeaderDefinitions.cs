namespace SuperMetroid.Core.Rooms;

/// <summary>The fixed configuration of the 262 non-debug retail rooms.</summary>
/// <remarks>Each entry holds a room's room/area indexes, map position, dimensions,
/// scrolling thresholds and CRE flags, with the unsigned byte/word
/// interpretation of the native eleven-byte header. Each room is named by the pinned
/// disassembly's label for its header; the developer area-seven room is not compiled.</remarks>
[NumberedIdentities("Rooms such as Metroids1-4 and TourianEscape1-4 are distinct rooms with their own headers; the number is part of each room's name, not a position callers index.")]
public static class RoomHeaderDefinitions
{
    /// <summary>Retail rooms in the cartridge's header order.</summary>
    private static readonly RoomHeaderDefinition[] Definitions =
    [
        new(nameof(LandingSite), 0x00, AreaId.Crateria, new(0x17, 0x00), new(0x09, 0x05), 0x70, 0xA0, 0x00),
        new(nameof(GauntletEntrance), 0x01, AreaId.Crateria, new(0x12, 0x02), new(0x05, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Parlor), 0x02, AreaId.Crateria, new(0x12, 0x04), new(0x05, 0x05), 0x70, 0xA0, 0x00),
        new(nameof(CrateriaPowerBombs), 0x03, AreaId.Crateria, new(0x20, 0x01), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(CrateriaSave), 0x04, AreaId.Crateria, new(0x12, 0x06), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WestOcean), 0x05, AreaId.Crateria, new(0x26, 0x00), new(0x08, 0x06), 0x90, 0xA0, 0x00),
        new(nameof(BowlingAlleyPath), 0x06, AreaId.Crateria, new(0x29, 0x02), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(CrateriaKihunter), 0x07, AreaId.Crateria, new(0x21, 0x04), new(0x03, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(ForgottenHighwayElev), 0x08, AreaId.Crateria, new(0x34, 0x0A), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(EastOcean), 0x09, AreaId.Crateria, new(0x31, 0x00), new(0x07, 0x06), 0x70, 0xA0, 0x00),
        new(nameof(ForgottenHighwayKagos), 0x0A, AreaId.Crateria, new(0x38, 0x04), new(0x01, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(CrabMaze), 0x0B, AreaId.Crateria, new(0x35, 0x08), new(0x04, 0x02), 0x90, 0xA0, 0x00),
        new(nameof(ForgottenHighwayElbow), 0x0C, AreaId.Crateria, new(0x34, 0x09), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(CrateriaTube), 0x0D, AreaId.Crateria, new(0x20, 0x04), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Moat), 0x0E, AreaId.Crateria, new(0x24, 0x04), new(0x02, 0x02), 0x90, 0xA0, 0x00),
        new(nameof(RedBinstarElev), 0x0F, AreaId.Crateria, new(0x22, 0x07), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GauntletETank), 0x10, AreaId.Crateria, new(0x0C, 0x02), new(0x06, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PreBowling), 0x11, AreaId.Crateria, new(0x2B, 0x02), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Climb), 0x12, AreaId.Crateria, new(0x12, 0x09), new(0x03, 0x09), 0x70, 0xA0, 0x00),
        new(nameof(Pit), 0x13, AreaId.Crateria, new(0x14, 0x11), new(0x03, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(ElevToBlueBrinstar), 0x14, AreaId.Crateria, new(0x17, 0x11), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BombTorizo), 0x15, AreaId.Crateria, new(0x19, 0x06), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Flyway), 0x16, AreaId.Crateria, new(0x16, 0x06), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PreMapFlyway), 0x17, AreaId.Crateria, new(0x14, 0x07), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Terminator), 0x18, AreaId.Crateria, new(0x0C, 0x04), new(0x06, 0x03), 0xA0, 0xA0, 0x00),
        new(nameof(GreenBrinstarElev), 0x19, AreaId.Crateria, new(0x06, 0x08), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(LowerMushrooms), 0x1A, AreaId.Crateria, new(0x07, 0x08), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(CrateriaMap), 0x1B, AreaId.Crateria, new(0x17, 0x07), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GreenPiratesShaft), 0x1C, AreaId.Crateria, new(0x0B, 0x02), new(0x01, 0x07), 0x70, 0xA0, 0x00),
        new(nameof(CrateriaSuper), 0x1D, AreaId.Crateria, new(0x15, 0x09), new(0x04, 0x08), 0x70, 0xA0, 0x00),
        new(nameof(FinalMissileBombway), 0x1E, AreaId.Crateria, new(0x11, 0x07), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(FinalMissile), 0x1F, AreaId.Crateria, new(0x10, 0x07), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GreenBrinstarMainShaft), 0x00, AreaId.Brinstar, new(0x09, 0x00), new(0x04, 0x0C), 0x70, 0xA0, 0x00),
        new(nameof(SporeSpawnSuper), 0x01, AreaId.Brinstar, new(0x17, 0x01), new(0x02, 0x09), 0x70, 0xA0, 0x00),
        new(nameof(BrinstarPreMap), 0x02, AreaId.Brinstar, new(0x06, 0x04), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(EarlySupers), 0x03, AreaId.Brinstar, new(0x0A, 0x03), new(0x03, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(BrinstarReserveTank), 0x04, AreaId.Brinstar, new(0x0D, 0x04), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GreenBrinstarMap), 0x05, AreaId.Brinstar, new(0x05, 0x04), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GreenBrinstarFirefleas), 0x06, AreaId.Brinstar, new(0x06, 0x06), new(0x03, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(GreenBrinstarMissileRefill), 0x07, AreaId.Brinstar, new(0x05, 0x07), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Dachora), 0x08, AreaId.Brinstar, new(0x0A, 0x06), new(0x07, 0x07), 0x70, 0xA0, 0x00),
        new(nameof(BigPink), 0x09, AreaId.Brinstar, new(0x0F, 0x04), new(0x05, 0x0A), 0x70, 0xA0, 0x00),
        new(nameof(SporeSpawnKihunters), 0x0A, AreaId.Brinstar, new(0x13, 0x04), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(SporeSpawn), 0x0B, AreaId.Brinstar, new(0x16, 0x01), new(0x01, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(PinkBrinstarPowerBombs), 0x0C, AreaId.Brinstar, new(0x0F, 0x07), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(GreenHillZone), 0x0D, AreaId.Brinstar, new(0x13, 0x0A), new(0x08, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(MorphBall), 0x0E, AreaId.Brinstar, new(0x15, 0x08), new(0x08, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(ConstructionZone), 0x0F, AreaId.Brinstar, new(0x1D, 0x0A), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(BlueBrinstarETank), 0x10, AreaId.Brinstar, new(0x1E, 0x08), new(0x03, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(NoobBridge), 0x11, AreaId.Brinstar, new(0x1B, 0x0D), new(0x06, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GreenBrinstarBeetoms), 0x12, AreaId.Brinstar, new(0x08, 0x0A), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(EtecoonETank), 0x13, AreaId.Brinstar, new(0x06, 0x0A), new(0x05, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(EtecoonSuper), 0x14, AreaId.Brinstar, new(0x05, 0x0A), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(DachoraEnergyRefill), 0x15, AreaId.Brinstar, new(0x09, 0x0C), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(SporeSpawnFarming), 0x16, AreaId.Brinstar, new(0x14, 0x09), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WaterwayETank), 0x17, AreaId.Brinstar, new(0x08, 0x0D), new(0x07, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(FirstMissile), 0x18, AreaId.Brinstar, new(0x1C, 0x0B), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PinkBrinstarHoppers), 0x19, AreaId.Brinstar, new(0x13, 0x07), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(HopperETank), 0x1A, AreaId.Brinstar, new(0x15, 0x08), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BigPinkSaveRoom), 0x1B, AreaId.Brinstar, new(0x0F, 0x04), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BlueBrinstarBoulders), 0x1C, AreaId.Brinstar, new(0x1E, 0x08), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BillyMays), 0x1D, AreaId.Brinstar, new(0x1D, 0x08), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GreenBrinstarSave), 0x1E, AreaId.Brinstar, new(0x08, 0x05), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(EtecoonSave), 0x1F, AreaId.Brinstar, new(0x05, 0x0B), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(RedTower), 0x20, AreaId.Brinstar, new(0x21, 0x09), new(0x01, 0x0A), 0x70, 0xA0, 0x00),
        new(nameof(RedBrinstarFirefleas), 0x21, AreaId.Brinstar, new(0x19, 0x0F), new(0x08, 0x02), 0x90, 0xA0, 0x00),
        new(nameof(XrayScope), 0x22, AreaId.Brinstar, new(0x17, 0x0F), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Hellway), 0x23, AreaId.Brinstar, new(0x22, 0x09), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Caterpillar), 0x24, AreaId.Brinstar, new(0x25, 0x04), new(0x03, 0x08), 0x70, 0xA0, 0x00),
        new(nameof(BetaPowerBomb), 0x25, AreaId.Brinstar, new(0x23, 0x07), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(AlphaPowerBomb), 0x26, AreaId.Brinstar, new(0x22, 0x0B), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(SkreeBoost), 0x27, AreaId.Brinstar, new(0x22, 0x12), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BelowSpazer), 0x28, AreaId.Brinstar, new(0x24, 0x11), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(Spazer), 0x29, AreaId.Brinstar, new(0x26, 0x11), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WarehouseZeela), 0x2A, AreaId.Brinstar, new(0x2C, 0x12), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(WarehouseETank), 0x2B, AreaId.Brinstar, new(0x2B, 0x13), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WarehouseKihunter), 0x2C, AreaId.Brinstar, new(0x2D, 0x12), new(0x04, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(MiniKraid), 0x2D, AreaId.Brinstar, new(0x2F, 0x13), new(0x06, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(KraidEyeDoor), 0x2E, AreaId.Brinstar, new(0x35, 0x12), new(0x02, 0x02), 0x70, 0xA0, 0x02),
        new(nameof(Kraid), 0x2F, AreaId.Brinstar, new(0x37, 0x12), new(0x02, 0x02), 0x70, 0xA0, 0x05),
        new(nameof(StatuesHallway), 0x30, AreaId.Crateria, new(0x0C, 0x08), new(0x05, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(RedTowerRefill), 0x31, AreaId.Brinstar, new(0x20, 0x12), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(KraidRefill), 0x32, AreaId.Brinstar, new(0x36, 0x12), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Statues), 0x33, AreaId.Crateria, new(0x11, 0x08), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(WarehouseEntrance), 0x34, AreaId.Brinstar, new(0x29, 0x12), new(0x03, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(VariaSuit), 0x35, AreaId.Brinstar, new(0x39, 0x13), new(0x01, 0x01), 0x70, 0xA0, 0x02),
        new(nameof(WarehouseSave), 0x36, AreaId.Brinstar, new(0x31, 0x12), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(RedBrinstarSave), 0x37, AreaId.Brinstar, new(0x26, 0x08), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(IceBeamAcid), 0x00, AreaId.Norfair, new(0x04, 0x03), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Cathedral), 0x01, AreaId.Norfair, new(0x0E, 0x03), new(0x03, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(CathedralEntrance), 0x02, AreaId.Norfair, new(0x0B, 0x03), new(0x03, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(BusinessCenter), 0x03, AreaId.Norfair, new(0x0A, 0x00), new(0x01, 0x07), 0x70, 0xA0, 0x00),
        new(nameof(IceBeamGate), 0x04, AreaId.Norfair, new(0x03, 0x01), new(0x07, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(IceBeamTutorial), 0x05, AreaId.Norfair, new(0x04, 0x01), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(IceBeam), 0x06, AreaId.Norfair, new(0x05, 0x02), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(IceBeamSnake), 0x07, AreaId.Norfair, new(0x03, 0x01), new(0x02, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(CrumbleShaft), 0x08, AreaId.Norfair, new(0x02, 0x04), new(0x01, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(CrocomireSpeedway), 0x09, AreaId.Norfair, new(0x03, 0x07), new(0x0D, 0x03), 0x70, 0xA0, 0x02),
        new(nameof(Crocomire), 0x0A, AreaId.Norfair, new(0x0C, 0x0A), new(0x08, 0x01), 0x70, 0xA0, 0x01),
        new(nameof(HiJumpBoots), 0x0B, AreaId.Norfair, new(0x07, 0x06), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(CrocomireEscape), 0x0C, AreaId.Norfair, new(0x0B, 0x06), new(0x04, 0x02), 0x90, 0xA0, 0x00),
        new(nameof(HiJumpETank), 0x0D, AreaId.Norfair, new(0x08, 0x05), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(PostCrocFarming), 0x0E, AreaId.Norfair, new(0x0A, 0x0A), new(0x02, 0x02), 0x90, 0xA0, 0x02),
        new(nameof(PostCrocSave), 0x0F, AreaId.Norfair, new(0x0C, 0x0B), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PostCrocPowerBombs), 0x10, AreaId.Norfair, new(0x09, 0x0A), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PostCrocShaft), 0x11, AreaId.Norfair, new(0x0A, 0x0C), new(0x01, 0x05), 0x90, 0xA0, 0x00),
        new(nameof(PostCrocMissile), 0x12, AreaId.Norfair, new(0x0B, 0x0F), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GrappleTutorial3), 0x13, AreaId.Norfair, new(0x07, 0x0C), new(0x03, 0x02), 0x90, 0xA0, 0x00),
        new(nameof(PostCrocJump), 0x14, AreaId.Norfair, new(0x04, 0x0F), new(0x08, 0x03), 0x90, 0xA0, 0x00),
        new(nameof(GrappleTutorial2), 0x15, AreaId.Norfair, new(0x06, 0x0C), new(0x01, 0x03), 0x90, 0xA0, 0x00),
        new(nameof(GrappleTutorial1), 0x16, AreaId.Norfair, new(0x04, 0x0E), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GrappleBeam), 0x17, AreaId.Norfair, new(0x03, 0x0E), new(0x01, 0x04), 0x90, 0xA0, 0x00),
        new(nameof(NorfairReserveTank), 0x18, AreaId.Norfair, new(0x12, 0x02), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GreenBubblesMissiles), 0x19, AreaId.Norfair, new(0x14, 0x02), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BubbleMountain), 0x1A, AreaId.Norfair, new(0x16, 0x02), new(0x02, 0x04), 0x90, 0xA0, 0x00),
        new(nameof(SpeedBoosterHall), 0x1B, AreaId.Norfair, new(0x19, 0x01), new(0x0C, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(SpeedBooster), 0x1C, AreaId.Norfair, new(0x25, 0x02), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(SingleChamber), 0x1D, AreaId.Norfair, new(0x18, 0x03), new(0x06, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(DoubleChamber), 0x1E, AreaId.Norfair, new(0x19, 0x04), new(0x04, 0x02), 0x90, 0xA0, 0x00),
        new(nameof(WaveBeam), 0x1F, AreaId.Norfair, new(0x1D, 0x04), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(SpikyPlatformsTunnel), 0x20, AreaId.Norfair, new(0x19, 0x06), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Volcano), 0x21, AreaId.Norfair, new(0x1B, 0x06), new(0x03, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(KronicBoost), 0x22, AreaId.Norfair, new(0x19, 0x08), new(0x02, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(MagdolliteTunnel), 0x23, AreaId.Norfair, new(0x17, 0x08), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PurpleShaft), 0x24, AreaId.Norfair, new(0x16, 0x06), new(0x01, 0x03), 0xA0, 0xA0, 0x00),
        new(nameof(LavaDive), 0x25, AreaId.Norfair, new(0x16, 0x0A), new(0x04, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(LowerNorfairElev), 0x26, AreaId.Norfair, new(0x15, 0x0A), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(UpperNorfairFarming), 0x27, AreaId.Norfair, new(0x14, 0x05), new(0x02, 0x02), 0xA0, 0xA0, 0x00),
        new(nameof(RisingTide), 0x28, AreaId.Norfair, new(0x11, 0x04), new(0x05, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(AcidSnakesTunnel), 0x29, AreaId.Norfair, new(0x10, 0x09), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(SpikyAcidSnakesTunnel), 0x2A, AreaId.Norfair, new(0x15, 0x09), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(UpperNorfairRefill), 0x2B, AreaId.Norfair, new(0x14, 0x09), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PurpleFarming), 0x2C, AreaId.Norfair, new(0x17, 0x07), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BatCave), 0x2D, AreaId.Norfair, new(0x18, 0x01), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(NorfairMap), 0x2E, AreaId.Norfair, new(0x09, 0x04), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BubbleMountainSave), 0x2F, AreaId.Norfair, new(0x15, 0x03), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(FrogSpeedway), 0x30, AreaId.Norfair, new(0x0C, 0x05), new(0x08, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(RedPirateShaft), 0x31, AreaId.Norfair, new(0x13, 0x06), new(0x01, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(FrogSave), 0x32, AreaId.Norfair, new(0x0B, 0x05), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(CrocomireSave), 0x33, AreaId.Norfair, new(0x10, 0x08), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(LNElevSave), 0x34, AreaId.Norfair, new(0x14, 0x0A), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(AcidStatue), 0x35, AreaId.Norfair, new(0x0F, 0x0D), new(0x03, 0x03), 0x90, 0xA0, 0x00),
        new(nameof(MainHall), 0x36, AreaId.Norfair, new(0x11, 0x0B), new(0x08, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(GoldenTorizo), 0x37, AreaId.Norfair, new(0x12, 0x0F), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(FastRipper), 0x38, AreaId.Norfair, new(0x15, 0x0E), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GTEnergyRefill), 0x39, AreaId.Norfair, new(0x15, 0x0F), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Ridley), 0x3A, AreaId.Norfair, new(0x17, 0x10), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(LNFarming), 0x3B, AreaId.Norfair, new(0x18, 0x10), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(FastPillarsSetup), 0x3C, AreaId.Norfair, new(0x19, 0x0C), new(0x01, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(UnusedRoom8FB3E1), 0x3D, AreaId.Norfair, new(0x1A, 0x0B), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(MickeyMouse), 0x3E, AreaId.Norfair, new(0x1A, 0x09), new(0x04, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(Pillar), 0x3F, AreaId.Norfair, new(0x1A, 0x0E), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Plowerhouse), 0x40, AreaId.Norfair, new(0x1B, 0x10), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WorstRoomInTheGame), 0x41, AreaId.Norfair, new(0x1E, 0x09), new(0x01, 0x06), 0x70, 0xA0, 0x00),
        new(nameof(Amphitheatre), 0x42, AreaId.Norfair, new(0x1F, 0x09), new(0x04, 0x05), 0x70, 0xA0, 0x00),
        new(nameof(LNSpringBallMaze), 0x43, AreaId.Norfair, new(0x21, 0x05), new(0x05, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(LNEscapePowerBombs), 0x44, AreaId.Norfair, new(0x25, 0x06), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(RedKihunterShaft), 0x45, AreaId.Norfair, new(0x23, 0x09), new(0x03, 0x05), 0x70, 0xA0, 0x00),
        new(nameof(Wasteland), 0x46, AreaId.Norfair, new(0x20, 0x0E), new(0x06, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(MetalPirates), 0x47, AreaId.Norfair, new(0x1E, 0x10), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(ThreeMusketeers), 0x48, AreaId.Norfair, new(0x1D, 0x03), new(0x04, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(RidleyETank), 0x49, AreaId.Norfair, new(0x16, 0x11), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(ScrewAttack), 0x4A, AreaId.Norfair, new(0x14, 0x0E), new(0x01, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(LNFireflea), 0x4B, AreaId.Norfair, new(0x23, 0x06), new(0x03, 0x06), 0x70, 0xA0, 0x00),
        new(nameof(LNSave), 0x4C, AreaId.Norfair, new(0x24, 0x0C), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BowlingAlley), 0x00, AreaId.WreckedShip, new(0x0A, 0x0B), new(0x06, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(WreckedShipEntrance), 0x01, AreaId.WreckedShip, new(0x0C, 0x0E), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Attic), 0x02, AreaId.WreckedShip, new(0x0C, 0x0A), new(0x07, 0x01), 0x00, 0x00, 0x00),
        new(nameof(AssemblyLine), 0x03, AreaId.WreckedShip, new(0x13, 0x0A), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WreckedShipMainShaft), 0x04, AreaId.WreckedShip, new(0x0C, 0x0B), new(0x06, 0x08), 0x70, 0xA0, 0x00),
        new(nameof(SpikyDeath), 0x05, AreaId.WreckedShip, new(0x13, 0x0F), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(ElectricDeath), 0x06, AreaId.WreckedShip, new(0x15, 0x0D), new(0x01, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(WreckedShipETank), 0x07, AreaId.WreckedShip, new(0x12, 0x0D), new(0x03, 0x02), 0x90, 0xA0, 0x00),
        new(nameof(Basement), 0x08, AreaId.WreckedShip, new(0x0E, 0x13), new(0x05, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WreckedShipMap), 0x09, AreaId.WreckedShip, new(0x0D, 0x13), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Phantoon), 0x0A, AreaId.WreckedShip, new(0x13, 0x13), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(SpongeBath), 0x0B, AreaId.WreckedShip, new(0x11, 0x0F), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WSWestSuper), 0x0C, AreaId.WreckedShip, new(0x0F, 0x11), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WSEastSuper), 0x0D, AreaId.WreckedShip, new(0x12, 0x11), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GravitySuit), 0x0E, AreaId.WreckedShip, new(0x0A, 0x0D), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WreckedShipSave), 0x0F, AreaId.WreckedShip, new(0x11, 0x0E), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GlassTunnelSave), 0x00, AreaId.Maridia, new(0x0C, 0x13), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(GlassTunnel), 0x01, AreaId.Maridia, new(0x0B, 0x11), new(0x01, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(WestTunnel), 0x02, AreaId.Maridia, new(0x0A, 0x12), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(EastTunnel), 0x03, AreaId.Maridia, new(0x0C, 0x11), new(0x04, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(MainStreet), 0x04, AreaId.Maridia, new(0x0A, 0x09), new(0x03, 0x08), 0x70, 0xA0, 0x00),
        new(nameof(FishTank), 0x05, AreaId.Maridia, new(0x0D, 0x0D), new(0x04, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(MamaTurtle), 0x06, AreaId.Maridia, new(0x11, 0x0C), new(0x03, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(CrabTunnel), 0x07, AreaId.Maridia, new(0x0C, 0x10), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(MtEverest), 0x08, AreaId.Maridia, new(0x0C, 0x09), new(0x06, 0x04), 0x90, 0xA0, 0x00),
        new(nameof(RedFish), 0x09, AreaId.Maridia, new(0x0C, 0x07), new(0x03, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(WateringHole), 0x0A, AreaId.Maridia, new(0x0C, 0x04), new(0x02, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(NWestMaridiaBug), 0x0B, AreaId.Maridia, new(0x0E, 0x04), new(0x04, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(CrabShaft), 0x0C, AreaId.Maridia, new(0x12, 0x07), new(0x02, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(PseudoPlasmaSpark), 0x0D, AreaId.Maridia, new(0x12, 0x04), new(0x04, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(CrabHole), 0x0E, AreaId.Maridia, new(0x10, 0x10), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(WestSandHallTunnel), 0x0F, AreaId.Maridia, new(0x11, 0x10), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PlasmaTutorial), 0x10, AreaId.Maridia, new(0x1A, 0x00), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Plasma), 0x11, AreaId.Maridia, new(0x1B, 0x00), new(0x02, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(ThreadTheNeedle), 0x12, AreaId.Maridia, new(0x1B, 0x05), new(0x07, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(MaridiaElev), 0x13, AreaId.Maridia, new(0x22, 0x00), new(0x01, 0x06), 0x70, 0xA0, 0x00),
        new(nameof(PlasmaSpark), 0x14, AreaId.Maridia, new(0x16, 0x02), new(0x04, 0x06), 0x70, 0xA0, 0x00),
        new(nameof(Kassiuz), 0x15, AreaId.Maridia, new(0x19, 0x00), new(0x01, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(MaridiaMap), 0x16, AreaId.Maridia, new(0x11, 0x11), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(ForgottenHighwaySave), 0x17, AreaId.Maridia, new(0x23, 0x04), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Toilet), 0x18, AreaId.Maridia, new(0x16, 0x05), new(0x01, 0x0A), 0x70, 0xA0, 0x00),
        new(nameof(BugSandHole), 0x19, AreaId.Maridia, new(0x1A, 0x05), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WestSandHall), 0x1A, AreaId.Maridia, new(0x12, 0x10), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Oasis), 0x1B, AreaId.Maridia, new(0x16, 0x0F), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(EastSandHall), 0x1C, AreaId.Maridia, new(0x17, 0x10), new(0x03, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WestSandHole), 0x1D, AreaId.Maridia, new(0x14, 0x0E), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(EastSandHole), 0x1E, AreaId.Maridia, new(0x17, 0x0E), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(WestAqueductQuicksand), 0x1F, AreaId.Maridia, new(0x15, 0x0C), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(EastAqueductQuicksand), 0x20, AreaId.Maridia, new(0x17, 0x0C), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(Aqueduct), 0x21, AreaId.Maridia, new(0x14, 0x09), new(0x06, 0x03), 0x90, 0xA0, 0x00),
        new(nameof(Butterfly), 0x22, AreaId.Maridia, new(0x1A, 0x07), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BotwoonHallway), 0x23, AreaId.Maridia, new(0x14, 0x08), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Pants), 0x24, AreaId.Maridia, new(0x1A, 0x0D), new(0x02, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(EastPants), 0x25, AreaId.Maridia, new(0x1B, 0x0E), new(0x01, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(Springball), 0x26, AreaId.Maridia, new(0x20, 0x0F), new(0x02, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(BelowBotwoonETank), 0x27, AreaId.Maridia, new(0x1A, 0x0A), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Colosseum), 0x28, AreaId.Maridia, new(0x22, 0x06), new(0x07, 0x02), 0x90, 0xA0, 0x00),
        new(nameof(AqueductSave), 0x29, AreaId.Maridia, new(0x13, 0x0B), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(ThePrecious), 0x2A, AreaId.Maridia, new(0x29, 0x07), new(0x02, 0x03), 0x90, 0xA0, 0x00),
        new(nameof(BotwoonETank), 0x2B, AreaId.Maridia, new(0x1A, 0x08), new(0x07, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(DraygonSave), 0x2C, AreaId.Maridia, new(0x29, 0x06), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(MaridiaMissileRefill), 0x2D, AreaId.Maridia, new(0x26, 0x08), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(PlasmaBeachQuicksand), 0x2E, AreaId.Maridia, new(0x1A, 0x06), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BotwoonQuicksand), 0x2F, AreaId.Maridia, new(0x1C, 0x09), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Shaktool), 0x30, AreaId.Maridia, new(0x1C, 0x0F), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(HalfieClimb), 0x31, AreaId.Maridia, new(0x21, 0x06), new(0x05, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(Botwoon), 0x32, AreaId.Maridia, new(0x18, 0x08), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(SpaceJump), 0x33, AreaId.Maridia, new(0x26, 0x0A), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(MaridiaEnergyRefill), 0x34, AreaId.Maridia, new(0x2A, 0x06), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(WestCactusAlley), 0x35, AreaId.Maridia, new(0x1B, 0x06), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(EastCactusAlley), 0x36, AreaId.Maridia, new(0x1C, 0x06), new(0x05, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(Draygon), 0x37, AreaId.Maridia, new(0x27, 0x09), new(0x02, 0x02), 0x70, 0xA0, 0x01),
        new(nameof(TourianFirst), 0x00, AreaId.Tourian, new(0x14, 0x09), new(0x01, 0x04), 0x70, 0xA0, 0x00),
        new(nameof(Metroids1), 0x01, AreaId.Tourian, new(0x0E, 0x0C), new(0x06, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Metroids2), 0x02, AreaId.Tourian, new(0x0D, 0x0C), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(Metroids3), 0x03, AreaId.Tourian, new(0x0E, 0x0D), new(0x06, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Metroids4), 0x04, AreaId.Tourian, new(0x14, 0x0D), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(BlueHopper), 0x05, AreaId.Tourian, new(0x13, 0x0F), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(DustTorizo), 0x06, AreaId.Tourian, new(0x11, 0x0F), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(BigBoy), 0x07, AreaId.Tourian, new(0x0D, 0x0F), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Seaweed), 0x08, AreaId.Tourian, new(0x0C, 0x0F), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(TourianRecharge), 0x09, AreaId.Tourian, new(0x0B, 0x10), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(MotherBrain), 0x0A, AreaId.Tourian, new(0x0D, 0x12), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(TourianEyeDoor), 0x0B, AreaId.Tourian, new(0x0D, 0x10), new(0x04, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(RinkaShaft), 0x0C, AreaId.Tourian, new(0x11, 0x10), new(0x01, 0x03), 0x70, 0xA0, 0x00),
        new(nameof(MotherBrainSave), 0x0D, AreaId.Tourian, new(0x10, 0x11), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(TourianEscape1), 0x0E, AreaId.Tourian, new(0x0B, 0x12), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(TourianEscape2), 0x0F, AreaId.Tourian, new(0x0B, 0x13), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(TourianEscape3), 0x10, AreaId.Tourian, new(0x0C, 0x13), new(0x06, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(TourianEscape4), 0x11, AreaId.Tourian, new(0x12, 0x10), new(0x03, 0x06), 0x70, 0xA0, 0x00),
        new(nameof(UpperTourianSave), 0x12, AreaId.Tourian, new(0x15, 0x0C), new(0x01, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(CeresElev), 0x00, AreaId.Ceres, new(0x0C, 0x0D), new(0x01, 0x03), 0x70, 0xA0, 0x05),
        new(nameof(FallingTile), 0x01, AreaId.Ceres, new(0x0D, 0x0F), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(MagnetStairs), 0x02, AreaId.Ceres, new(0x0F, 0x0F), new(0x01, 0x02), 0x70, 0xA0, 0x00),
        new(nameof(DeadScientist), 0x03, AreaId.Ceres, new(0x10, 0x10), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(Room58Escape), 0x04, AreaId.Ceres, new(0x12, 0x10), new(0x02, 0x01), 0x70, 0xA0, 0x00),
        new(nameof(CeresRidley), 0x05, AreaId.Ceres, new(0x14, 0x10), new(0x01, 0x01), 0x70, 0xA0, 0x05),
    ];

    /// <summary>Every retail room, in the cartridge's header order.</summary>
    public static IReadOnlyList<RoomHeaderDefinition> All => Definitions;

    /// <summary>The compiled room named <paramref name="name"/>.</summary>
    private static RoomHeaderDefinition Find(string name) => Definitions.Single(room => room.Name == name);

    /// <summary>The <c>RoomHeader_LandingSite</c> room.</summary>
    public static RoomHeaderDefinition LandingSite { get; } = Find(nameof(LandingSite));
    /// <summary>The <c>RoomHeader_GauntletEntrance</c> room.</summary>
    public static RoomHeaderDefinition GauntletEntrance { get; } = Find(nameof(GauntletEntrance));
    /// <summary>The <c>RoomHeader_Parlor</c> room.</summary>
    public static RoomHeaderDefinition Parlor { get; } = Find(nameof(Parlor));
    /// <summary>The <c>RoomHeader_CrateriaPowerBombs</c> room.</summary>
    public static RoomHeaderDefinition CrateriaPowerBombs { get; } = Find(nameof(CrateriaPowerBombs));
    /// <summary>The <c>RoomHeader_CrateriaSave</c> room.</summary>
    public static RoomHeaderDefinition CrateriaSave { get; } = Find(nameof(CrateriaSave));
    /// <summary>The <c>RoomHeader_WestOcean</c> room.</summary>
    public static RoomHeaderDefinition WestOcean { get; } = Find(nameof(WestOcean));
    /// <summary>The <c>RoomHeader_BowlingAlleyPath</c> room.</summary>
    public static RoomHeaderDefinition BowlingAlleyPath { get; } = Find(nameof(BowlingAlleyPath));
    /// <summary>The <c>RoomHeader_CrateriaKihunter</c> room.</summary>
    public static RoomHeaderDefinition CrateriaKihunter { get; } = Find(nameof(CrateriaKihunter));
    /// <summary>The <c>RoomHeader_ForgottenHighwayElev</c> room.</summary>
    public static RoomHeaderDefinition ForgottenHighwayElev { get; } = Find(nameof(ForgottenHighwayElev));
    /// <summary>The <c>RoomHeader_EastOcean</c> room.</summary>
    public static RoomHeaderDefinition EastOcean { get; } = Find(nameof(EastOcean));
    /// <summary>The <c>RoomHeader_ForgottenHighwayKagos</c> room.</summary>
    public static RoomHeaderDefinition ForgottenHighwayKagos { get; } = Find(nameof(ForgottenHighwayKagos));
    /// <summary>The <c>RoomHeader_CrabMaze</c> room.</summary>
    public static RoomHeaderDefinition CrabMaze { get; } = Find(nameof(CrabMaze));
    /// <summary>The <c>RoomHeader_ForgottenHighwayElbow</c> room.</summary>
    public static RoomHeaderDefinition ForgottenHighwayElbow { get; } = Find(nameof(ForgottenHighwayElbow));
    /// <summary>The <c>RoomHeader_CrateriaTube</c> room.</summary>
    public static RoomHeaderDefinition CrateriaTube { get; } = Find(nameof(CrateriaTube));
    /// <summary>The <c>RoomHeader_Moat</c> room.</summary>
    public static RoomHeaderDefinition Moat { get; } = Find(nameof(Moat));
    /// <summary>The <c>RoomHeader_RedBinstarElev</c> room.</summary>
    public static RoomHeaderDefinition RedBinstarElev { get; } = Find(nameof(RedBinstarElev));
    /// <summary>The <c>RoomHeader_GauntletETank</c> room.</summary>
    public static RoomHeaderDefinition GauntletETank { get; } = Find(nameof(GauntletETank));
    /// <summary>The <c>RoomHeader_PreBowling</c> room.</summary>
    public static RoomHeaderDefinition PreBowling { get; } = Find(nameof(PreBowling));
    /// <summary>The <c>RoomHeader_Climb</c> room.</summary>
    public static RoomHeaderDefinition Climb { get; } = Find(nameof(Climb));
    /// <summary>The <c>RoomHeader_Pit</c> room.</summary>
    public static RoomHeaderDefinition Pit { get; } = Find(nameof(Pit));
    /// <summary>The <c>RoomHeader_ElevToBlueBrinstar</c> room.</summary>
    public static RoomHeaderDefinition ElevToBlueBrinstar { get; } = Find(nameof(ElevToBlueBrinstar));
    /// <summary>The <c>RoomHeader_BombTorizo</c> room.</summary>
    public static RoomHeaderDefinition BombTorizo { get; } = Find(nameof(BombTorizo));
    /// <summary>The <c>RoomHeader_Flyway</c> room.</summary>
    public static RoomHeaderDefinition Flyway { get; } = Find(nameof(Flyway));
    /// <summary>The <c>RoomHeader_PreMapFlyway</c> room.</summary>
    public static RoomHeaderDefinition PreMapFlyway { get; } = Find(nameof(PreMapFlyway));
    /// <summary>The <c>RoomHeader_Terminator</c> room.</summary>
    public static RoomHeaderDefinition Terminator { get; } = Find(nameof(Terminator));
    /// <summary>The <c>RoomHeader_GreenBrinstarElev</c> room.</summary>
    public static RoomHeaderDefinition GreenBrinstarElev { get; } = Find(nameof(GreenBrinstarElev));
    /// <summary>The <c>RoomHeader_LowerMushrooms</c> room.</summary>
    public static RoomHeaderDefinition LowerMushrooms { get; } = Find(nameof(LowerMushrooms));
    /// <summary>The <c>RoomHeader_CrateriaMap</c> room.</summary>
    public static RoomHeaderDefinition CrateriaMap { get; } = Find(nameof(CrateriaMap));
    /// <summary>The <c>RoomHeader_GreenPiratesShaft</c> room.</summary>
    public static RoomHeaderDefinition GreenPiratesShaft { get; } = Find(nameof(GreenPiratesShaft));
    /// <summary>The <c>RoomHeader_CrateriaSuper</c> room.</summary>
    public static RoomHeaderDefinition CrateriaSuper { get; } = Find(nameof(CrateriaSuper));
    /// <summary>The <c>RoomHeader_FinalMissileBombway</c> room.</summary>
    public static RoomHeaderDefinition FinalMissileBombway { get; } = Find(nameof(FinalMissileBombway));
    /// <summary>The <c>RoomHeader_FinalMissile</c> room.</summary>
    public static RoomHeaderDefinition FinalMissile { get; } = Find(nameof(FinalMissile));
    /// <summary>The <c>RoomHeader_GreenBrinstarMainShaft</c> room.</summary>
    public static RoomHeaderDefinition GreenBrinstarMainShaft { get; } = Find(nameof(GreenBrinstarMainShaft));
    /// <summary>The <c>RoomHeader_SporeSpawnSuper</c> room.</summary>
    public static RoomHeaderDefinition SporeSpawnSuper { get; } = Find(nameof(SporeSpawnSuper));
    /// <summary>The <c>RoomHeader_BrinstarPreMap</c> room.</summary>
    public static RoomHeaderDefinition BrinstarPreMap { get; } = Find(nameof(BrinstarPreMap));
    /// <summary>The <c>RoomHeader_EarlySupers</c> room.</summary>
    public static RoomHeaderDefinition EarlySupers { get; } = Find(nameof(EarlySupers));
    /// <summary>The <c>RoomHeader_BrinstarReserveTank</c> room.</summary>
    public static RoomHeaderDefinition BrinstarReserveTank { get; } = Find(nameof(BrinstarReserveTank));
    /// <summary>The <c>RoomHeader_GreenBrinstarMap</c> room.</summary>
    public static RoomHeaderDefinition GreenBrinstarMap { get; } = Find(nameof(GreenBrinstarMap));
    /// <summary>The <c>RoomHeader_GreenBrinstarFirefleas</c> room.</summary>
    public static RoomHeaderDefinition GreenBrinstarFirefleas { get; } = Find(nameof(GreenBrinstarFirefleas));
    /// <summary>The <c>RoomHeader_GreenBrinstarMissileRefill</c> room.</summary>
    public static RoomHeaderDefinition GreenBrinstarMissileRefill { get; } = Find(nameof(GreenBrinstarMissileRefill));
    /// <summary>The <c>RoomHeader_Dachora</c> room.</summary>
    public static RoomHeaderDefinition Dachora { get; } = Find(nameof(Dachora));
    /// <summary>The <c>RoomHeader_BigPink</c> room.</summary>
    public static RoomHeaderDefinition BigPink { get; } = Find(nameof(BigPink));
    /// <summary>The <c>RoomHeader_SporeSpawnKihunters</c> room.</summary>
    public static RoomHeaderDefinition SporeSpawnKihunters { get; } = Find(nameof(SporeSpawnKihunters));
    /// <summary>The <c>RoomHeader_SporeSpawn</c> room.</summary>
    public static RoomHeaderDefinition SporeSpawn { get; } = Find(nameof(SporeSpawn));
    /// <summary>The <c>RoomHeader_PinkBrinstarPowerBombs</c> room.</summary>
    public static RoomHeaderDefinition PinkBrinstarPowerBombs { get; } = Find(nameof(PinkBrinstarPowerBombs));
    /// <summary>The <c>RoomHeader_GreenHillZone</c> room.</summary>
    public static RoomHeaderDefinition GreenHillZone { get; } = Find(nameof(GreenHillZone));
    /// <summary>The <c>RoomHeader_MorphBall</c> room.</summary>
    public static RoomHeaderDefinition MorphBall { get; } = Find(nameof(MorphBall));
    /// <summary>The <c>RoomHeader_ConstructionZone</c> room.</summary>
    public static RoomHeaderDefinition ConstructionZone { get; } = Find(nameof(ConstructionZone));
    /// <summary>The <c>RoomHeader_BlueBrinstarETank</c> room.</summary>
    public static RoomHeaderDefinition BlueBrinstarETank { get; } = Find(nameof(BlueBrinstarETank));
    /// <summary>The <c>RoomHeader_NoobBridge</c> room.</summary>
    public static RoomHeaderDefinition NoobBridge { get; } = Find(nameof(NoobBridge));
    /// <summary>The <c>RoomHeader_GreenBrinstarBeetoms</c> room.</summary>
    public static RoomHeaderDefinition GreenBrinstarBeetoms { get; } = Find(nameof(GreenBrinstarBeetoms));
    /// <summary>The <c>RoomHeader_EtecoonETank</c> room.</summary>
    public static RoomHeaderDefinition EtecoonETank { get; } = Find(nameof(EtecoonETank));
    /// <summary>The <c>RoomHeader_EtecoonSuper</c> room.</summary>
    public static RoomHeaderDefinition EtecoonSuper { get; } = Find(nameof(EtecoonSuper));
    /// <summary>The <c>RoomHeader_DachoraEnergyRefill</c> room.</summary>
    public static RoomHeaderDefinition DachoraEnergyRefill { get; } = Find(nameof(DachoraEnergyRefill));
    /// <summary>The <c>RoomHeader_SporeSpawnFarming</c> room.</summary>
    public static RoomHeaderDefinition SporeSpawnFarming { get; } = Find(nameof(SporeSpawnFarming));
    /// <summary>The <c>RoomHeader_WaterwayETank</c> room.</summary>
    public static RoomHeaderDefinition WaterwayETank { get; } = Find(nameof(WaterwayETank));
    /// <summary>The <c>RoomHeader_FirstMissile</c> room.</summary>
    public static RoomHeaderDefinition FirstMissile { get; } = Find(nameof(FirstMissile));
    /// <summary>The <c>RoomHeader_PinkBrinstarHoppers</c> room.</summary>
    public static RoomHeaderDefinition PinkBrinstarHoppers { get; } = Find(nameof(PinkBrinstarHoppers));
    /// <summary>The <c>RoomHeader_HopperETank</c> room.</summary>
    public static RoomHeaderDefinition HopperETank { get; } = Find(nameof(HopperETank));
    /// <summary>The <c>RoomHeader_BigPinkSaveRoom</c> room.</summary>
    public static RoomHeaderDefinition BigPinkSaveRoom { get; } = Find(nameof(BigPinkSaveRoom));
    /// <summary>The <c>RoomHeader_BlueBrinstarBoulders</c> room.</summary>
    public static RoomHeaderDefinition BlueBrinstarBoulders { get; } = Find(nameof(BlueBrinstarBoulders));
    /// <summary>The <c>RoomHeader_BillyMays</c> room.</summary>
    public static RoomHeaderDefinition BillyMays { get; } = Find(nameof(BillyMays));
    /// <summary>The <c>RoomHeader_GreenBrinstarSave</c> room.</summary>
    public static RoomHeaderDefinition GreenBrinstarSave { get; } = Find(nameof(GreenBrinstarSave));
    /// <summary>The <c>RoomHeader_EtecoonSave</c> room.</summary>
    public static RoomHeaderDefinition EtecoonSave { get; } = Find(nameof(EtecoonSave));
    /// <summary>The <c>RoomHeader_RedTower</c> room.</summary>
    public static RoomHeaderDefinition RedTower { get; } = Find(nameof(RedTower));
    /// <summary>The <c>RoomHeader_RedBrinstarFirefleas</c> room.</summary>
    public static RoomHeaderDefinition RedBrinstarFirefleas { get; } = Find(nameof(RedBrinstarFirefleas));
    /// <summary>The <c>RoomHeader_XrayScope</c> room.</summary>
    public static RoomHeaderDefinition XrayScope { get; } = Find(nameof(XrayScope));
    /// <summary>The <c>RoomHeader_Hellway</c> room.</summary>
    public static RoomHeaderDefinition Hellway { get; } = Find(nameof(Hellway));
    /// <summary>The <c>RoomHeader_Caterpillar</c> room.</summary>
    public static RoomHeaderDefinition Caterpillar { get; } = Find(nameof(Caterpillar));
    /// <summary>The <c>RoomHeader_BetaPowerBomb</c> room.</summary>
    public static RoomHeaderDefinition BetaPowerBomb { get; } = Find(nameof(BetaPowerBomb));
    /// <summary>The <c>RoomHeader_AlphaPowerBomb</c> room.</summary>
    public static RoomHeaderDefinition AlphaPowerBomb { get; } = Find(nameof(AlphaPowerBomb));
    /// <summary>The <c>RoomHeader_SkreeBoost</c> room.</summary>
    public static RoomHeaderDefinition SkreeBoost { get; } = Find(nameof(SkreeBoost));
    /// <summary>The <c>RoomHeader_BelowSpazer</c> room.</summary>
    public static RoomHeaderDefinition BelowSpazer { get; } = Find(nameof(BelowSpazer));
    /// <summary>The <c>RoomHeader_Spazer</c> room.</summary>
    public static RoomHeaderDefinition Spazer { get; } = Find(nameof(Spazer));
    /// <summary>The <c>RoomHeader_WarehouseZeela</c> room.</summary>
    public static RoomHeaderDefinition WarehouseZeela { get; } = Find(nameof(WarehouseZeela));
    /// <summary>The <c>RoomHeader_WarehouseETank</c> room.</summary>
    public static RoomHeaderDefinition WarehouseETank { get; } = Find(nameof(WarehouseETank));
    /// <summary>The <c>RoomHeader_WarehouseKihunter</c> room.</summary>
    public static RoomHeaderDefinition WarehouseKihunter { get; } = Find(nameof(WarehouseKihunter));
    /// <summary>The <c>RoomHeader_MiniKraid</c> room.</summary>
    public static RoomHeaderDefinition MiniKraid { get; } = Find(nameof(MiniKraid));
    /// <summary>The <c>RoomHeader_KraidEyeDoor</c> room.</summary>
    public static RoomHeaderDefinition KraidEyeDoor { get; } = Find(nameof(KraidEyeDoor));
    /// <summary>The <c>RoomHeader_Kraid</c> room.</summary>
    public static RoomHeaderDefinition Kraid { get; } = Find(nameof(Kraid));
    /// <summary>The <c>RoomHeader_StatuesHallway</c> room.</summary>
    public static RoomHeaderDefinition StatuesHallway { get; } = Find(nameof(StatuesHallway));
    /// <summary>The <c>RoomHeader_RedTowerRefill</c> room.</summary>
    public static RoomHeaderDefinition RedTowerRefill { get; } = Find(nameof(RedTowerRefill));
    /// <summary>The <c>RoomHeader_KraidRefill</c> room.</summary>
    public static RoomHeaderDefinition KraidRefill { get; } = Find(nameof(KraidRefill));
    /// <summary>The <c>RoomHeader_Statues</c> room.</summary>
    public static RoomHeaderDefinition Statues { get; } = Find(nameof(Statues));
    /// <summary>The <c>RoomHeader_WarehouseEntrance</c> room.</summary>
    public static RoomHeaderDefinition WarehouseEntrance { get; } = Find(nameof(WarehouseEntrance));
    /// <summary>The <c>RoomHeader_VariaSuit</c> room.</summary>
    public static RoomHeaderDefinition VariaSuit { get; } = Find(nameof(VariaSuit));
    /// <summary>The <c>RoomHeader_WarehouseSave</c> room.</summary>
    public static RoomHeaderDefinition WarehouseSave { get; } = Find(nameof(WarehouseSave));
    /// <summary>The <c>RoomHeader_RedBrinstarSave</c> room.</summary>
    public static RoomHeaderDefinition RedBrinstarSave { get; } = Find(nameof(RedBrinstarSave));
    /// <summary>The <c>RoomHeader_IceBeamAcid</c> room.</summary>
    public static RoomHeaderDefinition IceBeamAcid { get; } = Find(nameof(IceBeamAcid));
    /// <summary>The <c>RoomHeader_Cathedral</c> room.</summary>
    public static RoomHeaderDefinition Cathedral { get; } = Find(nameof(Cathedral));
    /// <summary>The <c>RoomHeader_CathedralEntrance</c> room.</summary>
    public static RoomHeaderDefinition CathedralEntrance { get; } = Find(nameof(CathedralEntrance));
    /// <summary>The <c>RoomHeader_BusinessCenter</c> room.</summary>
    public static RoomHeaderDefinition BusinessCenter { get; } = Find(nameof(BusinessCenter));
    /// <summary>The <c>RoomHeader_IceBeamGate</c> room.</summary>
    public static RoomHeaderDefinition IceBeamGate { get; } = Find(nameof(IceBeamGate));
    /// <summary>The <c>RoomHeader_IceBeamTutorial</c> room.</summary>
    public static RoomHeaderDefinition IceBeamTutorial { get; } = Find(nameof(IceBeamTutorial));
    /// <summary>The <c>RoomHeader_IceBeam</c> room.</summary>
    public static RoomHeaderDefinition IceBeam { get; } = Find(nameof(IceBeam));
    /// <summary>The <c>RoomHeader_IceBeamSnake</c> room.</summary>
    public static RoomHeaderDefinition IceBeamSnake { get; } = Find(nameof(IceBeamSnake));
    /// <summary>The <c>RoomHeader_CrumbleShaft</c> room.</summary>
    public static RoomHeaderDefinition CrumbleShaft { get; } = Find(nameof(CrumbleShaft));
    /// <summary>The <c>RoomHeader_CrocomireSpeedway</c> room.</summary>
    public static RoomHeaderDefinition CrocomireSpeedway { get; } = Find(nameof(CrocomireSpeedway));
    /// <summary>The <c>RoomHeader_Crocomire</c> room.</summary>
    public static RoomHeaderDefinition Crocomire { get; } = Find(nameof(Crocomire));
    /// <summary>The <c>RoomHeader_HiJumpBoots</c> room.</summary>
    public static RoomHeaderDefinition HiJumpBoots { get; } = Find(nameof(HiJumpBoots));
    /// <summary>The <c>RoomHeader_CrocomireEscape</c> room.</summary>
    public static RoomHeaderDefinition CrocomireEscape { get; } = Find(nameof(CrocomireEscape));
    /// <summary>The <c>RoomHeader_HiJumpETank</c> room.</summary>
    public static RoomHeaderDefinition HiJumpETank { get; } = Find(nameof(HiJumpETank));
    /// <summary>The <c>RoomHeader_PostCrocFarming</c> room.</summary>
    public static RoomHeaderDefinition PostCrocFarming { get; } = Find(nameof(PostCrocFarming));
    /// <summary>The <c>RoomHeader_PostCrocSave</c> room.</summary>
    public static RoomHeaderDefinition PostCrocSave { get; } = Find(nameof(PostCrocSave));
    /// <summary>The <c>RoomHeader_PostCrocPowerBombs</c> room.</summary>
    public static RoomHeaderDefinition PostCrocPowerBombs { get; } = Find(nameof(PostCrocPowerBombs));
    /// <summary>The <c>RoomHeader_PostCrocShaft</c> room.</summary>
    public static RoomHeaderDefinition PostCrocShaft { get; } = Find(nameof(PostCrocShaft));
    /// <summary>The <c>RoomHeader_PostCrocMissile</c> room.</summary>
    public static RoomHeaderDefinition PostCrocMissile { get; } = Find(nameof(PostCrocMissile));
    /// <summary>The <c>RoomHeader_GrappleTutorial3</c> room.</summary>
    public static RoomHeaderDefinition GrappleTutorial3 { get; } = Find(nameof(GrappleTutorial3));
    /// <summary>The <c>RoomHeader_PostCrocJump</c> room.</summary>
    public static RoomHeaderDefinition PostCrocJump { get; } = Find(nameof(PostCrocJump));
    /// <summary>The <c>RoomHeader_GrappleTutorial2</c> room.</summary>
    public static RoomHeaderDefinition GrappleTutorial2 { get; } = Find(nameof(GrappleTutorial2));
    /// <summary>The <c>RoomHeader_GrappleTutorial1</c> room.</summary>
    public static RoomHeaderDefinition GrappleTutorial1 { get; } = Find(nameof(GrappleTutorial1));
    /// <summary>The <c>RoomHeader_GrappleBeam</c> room.</summary>
    public static RoomHeaderDefinition GrappleBeam { get; } = Find(nameof(GrappleBeam));
    /// <summary>The <c>RoomHeader_NorfairReserveTank</c> room.</summary>
    public static RoomHeaderDefinition NorfairReserveTank { get; } = Find(nameof(NorfairReserveTank));
    /// <summary>The <c>RoomHeader_GreenBubblesMissiles</c> room.</summary>
    public static RoomHeaderDefinition GreenBubblesMissiles { get; } = Find(nameof(GreenBubblesMissiles));
    /// <summary>The <c>RoomHeader_BubbleMountain</c> room.</summary>
    public static RoomHeaderDefinition BubbleMountain { get; } = Find(nameof(BubbleMountain));
    /// <summary>The <c>RoomHeader_SpeedBoosterHall</c> room.</summary>
    public static RoomHeaderDefinition SpeedBoosterHall { get; } = Find(nameof(SpeedBoosterHall));
    /// <summary>The <c>RoomHeader_SpeedBooster</c> room.</summary>
    public static RoomHeaderDefinition SpeedBooster { get; } = Find(nameof(SpeedBooster));
    /// <summary>The <c>RoomHeader_SingleChamber</c> room.</summary>
    public static RoomHeaderDefinition SingleChamber { get; } = Find(nameof(SingleChamber));
    /// <summary>The <c>RoomHeader_DoubleChamber</c> room.</summary>
    public static RoomHeaderDefinition DoubleChamber { get; } = Find(nameof(DoubleChamber));
    /// <summary>The <c>RoomHeader_WaveBeam</c> room.</summary>
    public static RoomHeaderDefinition WaveBeam { get; } = Find(nameof(WaveBeam));
    /// <summary>The <c>RoomHeader_SpikyPlatformsTunnel</c> room.</summary>
    public static RoomHeaderDefinition SpikyPlatformsTunnel { get; } = Find(nameof(SpikyPlatformsTunnel));
    /// <summary>The <c>RoomHeader_Volcano</c> room.</summary>
    public static RoomHeaderDefinition Volcano { get; } = Find(nameof(Volcano));
    /// <summary>The <c>RoomHeader_KronicBoost</c> room.</summary>
    public static RoomHeaderDefinition KronicBoost { get; } = Find(nameof(KronicBoost));
    /// <summary>The <c>RoomHeader_MagdolliteTunnel</c> room.</summary>
    public static RoomHeaderDefinition MagdolliteTunnel { get; } = Find(nameof(MagdolliteTunnel));
    /// <summary>The <c>RoomHeader_PurpleShaft</c> room.</summary>
    public static RoomHeaderDefinition PurpleShaft { get; } = Find(nameof(PurpleShaft));
    /// <summary>The <c>RoomHeader_LavaDive</c> room.</summary>
    public static RoomHeaderDefinition LavaDive { get; } = Find(nameof(LavaDive));
    /// <summary>The <c>RoomHeader_LowerNorfairElev</c> room.</summary>
    public static RoomHeaderDefinition LowerNorfairElev { get; } = Find(nameof(LowerNorfairElev));
    /// <summary>The <c>RoomHeader_UpperNorfairFarming</c> room.</summary>
    public static RoomHeaderDefinition UpperNorfairFarming { get; } = Find(nameof(UpperNorfairFarming));
    /// <summary>The <c>RoomHeader_RisingTide</c> room.</summary>
    public static RoomHeaderDefinition RisingTide { get; } = Find(nameof(RisingTide));
    /// <summary>The <c>RoomHeader_AcidSnakesTunnel</c> room.</summary>
    public static RoomHeaderDefinition AcidSnakesTunnel { get; } = Find(nameof(AcidSnakesTunnel));
    /// <summary>The <c>RoomHeader_SpikyAcidSnakesTunnel</c> room.</summary>
    public static RoomHeaderDefinition SpikyAcidSnakesTunnel { get; } = Find(nameof(SpikyAcidSnakesTunnel));
    /// <summary>The <c>RoomHeader_UpperNorfairRefill</c> room.</summary>
    public static RoomHeaderDefinition UpperNorfairRefill { get; } = Find(nameof(UpperNorfairRefill));
    /// <summary>The <c>RoomHeader_PurpleFarming</c> room.</summary>
    public static RoomHeaderDefinition PurpleFarming { get; } = Find(nameof(PurpleFarming));
    /// <summary>The <c>RoomHeader_BatCave</c> room.</summary>
    public static RoomHeaderDefinition BatCave { get; } = Find(nameof(BatCave));
    /// <summary>The <c>RoomHeader_NorfairMap</c> room.</summary>
    public static RoomHeaderDefinition NorfairMap { get; } = Find(nameof(NorfairMap));
    /// <summary>The <c>RoomHeader_BubbleMountainSave</c> room.</summary>
    public static RoomHeaderDefinition BubbleMountainSave { get; } = Find(nameof(BubbleMountainSave));
    /// <summary>The <c>RoomHeader_FrogSpeedway</c> room.</summary>
    public static RoomHeaderDefinition FrogSpeedway { get; } = Find(nameof(FrogSpeedway));
    /// <summary>The <c>RoomHeader_RedPirateShaft</c> room.</summary>
    public static RoomHeaderDefinition RedPirateShaft { get; } = Find(nameof(RedPirateShaft));
    /// <summary>The <c>RoomHeader_FrogSave</c> room.</summary>
    public static RoomHeaderDefinition FrogSave { get; } = Find(nameof(FrogSave));
    /// <summary>The <c>RoomHeader_CrocomireSave</c> room.</summary>
    public static RoomHeaderDefinition CrocomireSave { get; } = Find(nameof(CrocomireSave));
    /// <summary>The <c>RoomHeader_LNElevSave</c> room.</summary>
    public static RoomHeaderDefinition LNElevSave { get; } = Find(nameof(LNElevSave));
    /// <summary>The <c>RoomHeader_AcidStatue</c> room.</summary>
    public static RoomHeaderDefinition AcidStatue { get; } = Find(nameof(AcidStatue));
    /// <summary>The <c>RoomHeader_MainHall</c> room.</summary>
    public static RoomHeaderDefinition MainHall { get; } = Find(nameof(MainHall));
    /// <summary>The <c>RoomHeader_GoldenTorizo</c> room.</summary>
    public static RoomHeaderDefinition GoldenTorizo { get; } = Find(nameof(GoldenTorizo));
    /// <summary>The <c>RoomHeader_FastRipper</c> room.</summary>
    public static RoomHeaderDefinition FastRipper { get; } = Find(nameof(FastRipper));
    /// <summary>The <c>RoomHeader_GTEnergyRefill</c> room.</summary>
    public static RoomHeaderDefinition GTEnergyRefill { get; } = Find(nameof(GTEnergyRefill));
    /// <summary>The <c>RoomHeader_Ridley</c> room.</summary>
    public static RoomHeaderDefinition Ridley { get; } = Find(nameof(Ridley));
    /// <summary>The <c>RoomHeader_LNFarming</c> room.</summary>
    public static RoomHeaderDefinition LNFarming { get; } = Find(nameof(LNFarming));
    /// <summary>The <c>RoomHeader_FastPillarsSetup</c> room.</summary>
    public static RoomHeaderDefinition FastPillarsSetup { get; } = Find(nameof(FastPillarsSetup));
    /// <summary>The <c>UNUSED_RoomHeader_8FB3E1</c> room.</summary>
    public static RoomHeaderDefinition UnusedRoom8FB3E1 { get; } = Find(nameof(UnusedRoom8FB3E1));
    /// <summary>The <c>RoomHeader_MickeyMouse</c> room.</summary>
    public static RoomHeaderDefinition MickeyMouse { get; } = Find(nameof(MickeyMouse));
    /// <summary>The <c>RoomHeader_Pillar</c> room.</summary>
    public static RoomHeaderDefinition Pillar { get; } = Find(nameof(Pillar));
    /// <summary>The <c>RoomHeader_Plowerhouse</c> room.</summary>
    public static RoomHeaderDefinition Plowerhouse { get; } = Find(nameof(Plowerhouse));
    /// <summary>The <c>RoomHeader_WorstRoomInTheGame</c> room.</summary>
    public static RoomHeaderDefinition WorstRoomInTheGame { get; } = Find(nameof(WorstRoomInTheGame));
    /// <summary>The <c>RoomHeader_Amphitheatre</c> room.</summary>
    public static RoomHeaderDefinition Amphitheatre { get; } = Find(nameof(Amphitheatre));
    /// <summary>The <c>RoomHeader_LNSpringBallMaze</c> room.</summary>
    public static RoomHeaderDefinition LNSpringBallMaze { get; } = Find(nameof(LNSpringBallMaze));
    /// <summary>The <c>RoomHeader_LNEscapePowerBombs</c> room.</summary>
    public static RoomHeaderDefinition LNEscapePowerBombs { get; } = Find(nameof(LNEscapePowerBombs));
    /// <summary>The <c>RoomHeader_RedKihunterShaft</c> room.</summary>
    public static RoomHeaderDefinition RedKihunterShaft { get; } = Find(nameof(RedKihunterShaft));
    /// <summary>The <c>RoomHeader_Wasteland</c> room.</summary>
    public static RoomHeaderDefinition Wasteland { get; } = Find(nameof(Wasteland));
    /// <summary>The <c>RoomHeader_MetalPirates</c> room.</summary>
    public static RoomHeaderDefinition MetalPirates { get; } = Find(nameof(MetalPirates));
    /// <summary>The <c>RoomHeader_ThreeMusketeers</c> room.</summary>
    public static RoomHeaderDefinition ThreeMusketeers { get; } = Find(nameof(ThreeMusketeers));
    /// <summary>The <c>RoomHeader_RidleyETank</c> room.</summary>
    public static RoomHeaderDefinition RidleyETank { get; } = Find(nameof(RidleyETank));
    /// <summary>The <c>RoomHeader_ScrewAttack</c> room.</summary>
    public static RoomHeaderDefinition ScrewAttack { get; } = Find(nameof(ScrewAttack));
    /// <summary>The <c>RoomHeader_LNFireflea</c> room.</summary>
    public static RoomHeaderDefinition LNFireflea { get; } = Find(nameof(LNFireflea));
    /// <summary>The <c>RoomHeader_LNSave</c> room.</summary>
    public static RoomHeaderDefinition LNSave { get; } = Find(nameof(LNSave));
    /// <summary>The <c>RoomHeader_BowlingAlley</c> room.</summary>
    public static RoomHeaderDefinition BowlingAlley { get; } = Find(nameof(BowlingAlley));
    /// <summary>The <c>RoomHeader_WreckedShipEntrance</c> room.</summary>
    public static RoomHeaderDefinition WreckedShipEntrance { get; } = Find(nameof(WreckedShipEntrance));
    /// <summary>The <c>RoomHeader_Attic</c> room.</summary>
    public static RoomHeaderDefinition Attic { get; } = Find(nameof(Attic));
    /// <summary>The <c>RoomHeader_AssemblyLine</c> room.</summary>
    public static RoomHeaderDefinition AssemblyLine { get; } = Find(nameof(AssemblyLine));
    /// <summary>The <c>RoomHeader_WreckedShipMainShaft</c> room.</summary>
    public static RoomHeaderDefinition WreckedShipMainShaft { get; } = Find(nameof(WreckedShipMainShaft));
    /// <summary>The <c>RoomHeader_SpikyDeath</c> room.</summary>
    public static RoomHeaderDefinition SpikyDeath { get; } = Find(nameof(SpikyDeath));
    /// <summary>The <c>RoomHeader_ElectricDeath</c> room.</summary>
    public static RoomHeaderDefinition ElectricDeath { get; } = Find(nameof(ElectricDeath));
    /// <summary>The <c>RoomHeader_WreckedShipETank</c> room.</summary>
    public static RoomHeaderDefinition WreckedShipETank { get; } = Find(nameof(WreckedShipETank));
    /// <summary>The <c>RoomHeader_Basement</c> room.</summary>
    public static RoomHeaderDefinition Basement { get; } = Find(nameof(Basement));
    /// <summary>The <c>RoomHeader_WreckedShipMap</c> room.</summary>
    public static RoomHeaderDefinition WreckedShipMap { get; } = Find(nameof(WreckedShipMap));
    /// <summary>The <c>RoomHeader_Phantoon</c> room.</summary>
    public static RoomHeaderDefinition Phantoon { get; } = Find(nameof(Phantoon));
    /// <summary>The <c>RoomHeader_SpongeBath</c> room.</summary>
    public static RoomHeaderDefinition SpongeBath { get; } = Find(nameof(SpongeBath));
    /// <summary>The <c>RoomHeader_WSWestSuper</c> room.</summary>
    public static RoomHeaderDefinition WSWestSuper { get; } = Find(nameof(WSWestSuper));
    /// <summary>The <c>RoomHeader_WSEastSuper</c> room.</summary>
    public static RoomHeaderDefinition WSEastSuper { get; } = Find(nameof(WSEastSuper));
    /// <summary>The <c>RoomHeader_GravitySuit</c> room.</summary>
    public static RoomHeaderDefinition GravitySuit { get; } = Find(nameof(GravitySuit));
    /// <summary>The <c>RoomHeader_WreckedShipSave</c> room.</summary>
    public static RoomHeaderDefinition WreckedShipSave { get; } = Find(nameof(WreckedShipSave));
    /// <summary>The <c>RoomHeader_GlassTunnelSave</c> room.</summary>
    public static RoomHeaderDefinition GlassTunnelSave { get; } = Find(nameof(GlassTunnelSave));
    /// <summary>The <c>RoomHeader_GlassTunnel</c> room.</summary>
    public static RoomHeaderDefinition GlassTunnel { get; } = Find(nameof(GlassTunnel));
    /// <summary>The <c>RoomHeader_WestTunnel</c> room.</summary>
    public static RoomHeaderDefinition WestTunnel { get; } = Find(nameof(WestTunnel));
    /// <summary>The <c>RoomHeader_EastTunnel</c> room.</summary>
    public static RoomHeaderDefinition EastTunnel { get; } = Find(nameof(EastTunnel));
    /// <summary>The <c>RoomHeader_MainStreet</c> room.</summary>
    public static RoomHeaderDefinition MainStreet { get; } = Find(nameof(MainStreet));
    /// <summary>The <c>RoomHeader_FishTank</c> room.</summary>
    public static RoomHeaderDefinition FishTank { get; } = Find(nameof(FishTank));
    /// <summary>The <c>RoomHeader_MamaTurtle</c> room.</summary>
    public static RoomHeaderDefinition MamaTurtle { get; } = Find(nameof(MamaTurtle));
    /// <summary>The <c>RoomHeader_CrabTunnel</c> room.</summary>
    public static RoomHeaderDefinition CrabTunnel { get; } = Find(nameof(CrabTunnel));
    /// <summary>The <c>RoomHeader_MtEverest</c> room.</summary>
    public static RoomHeaderDefinition MtEverest { get; } = Find(nameof(MtEverest));
    /// <summary>The <c>RoomHeader_RedFish</c> room.</summary>
    public static RoomHeaderDefinition RedFish { get; } = Find(nameof(RedFish));
    /// <summary>The <c>RoomHeader_WateringHole</c> room.</summary>
    public static RoomHeaderDefinition WateringHole { get; } = Find(nameof(WateringHole));
    /// <summary>The <c>RoomHeader_NWestMaridiaBug</c> room.</summary>
    public static RoomHeaderDefinition NWestMaridiaBug { get; } = Find(nameof(NWestMaridiaBug));
    /// <summary>The <c>RoomHeader_CrabShaft</c> room.</summary>
    public static RoomHeaderDefinition CrabShaft { get; } = Find(nameof(CrabShaft));
    /// <summary>The <c>RoomHeader_PseudoPlasmaSpark</c> room.</summary>
    public static RoomHeaderDefinition PseudoPlasmaSpark { get; } = Find(nameof(PseudoPlasmaSpark));
    /// <summary>The <c>RoomHeader_CrabHole</c> room.</summary>
    public static RoomHeaderDefinition CrabHole { get; } = Find(nameof(CrabHole));
    /// <summary>The <c>RoomHeader_WestSandHallTunnel</c> room.</summary>
    public static RoomHeaderDefinition WestSandHallTunnel { get; } = Find(nameof(WestSandHallTunnel));
    /// <summary>The <c>RoomHeader_PlasmaTutorial</c> room.</summary>
    public static RoomHeaderDefinition PlasmaTutorial { get; } = Find(nameof(PlasmaTutorial));
    /// <summary>The <c>RoomHeader_Plasma</c> room.</summary>
    public static RoomHeaderDefinition Plasma { get; } = Find(nameof(Plasma));
    /// <summary>The <c>RoomHeader_ThreadTheNeedle</c> room.</summary>
    public static RoomHeaderDefinition ThreadTheNeedle { get; } = Find(nameof(ThreadTheNeedle));
    /// <summary>The <c>RoomHeader_MaridiaElev</c> room.</summary>
    public static RoomHeaderDefinition MaridiaElev { get; } = Find(nameof(MaridiaElev));
    /// <summary>The <c>RoomHeader_PlasmaSpark</c> room.</summary>
    public static RoomHeaderDefinition PlasmaSpark { get; } = Find(nameof(PlasmaSpark));
    /// <summary>The <c>RoomHeader_Kassiuz</c> room.</summary>
    public static RoomHeaderDefinition Kassiuz { get; } = Find(nameof(Kassiuz));
    /// <summary>The <c>RoomHeader_MaridiaMap</c> room.</summary>
    public static RoomHeaderDefinition MaridiaMap { get; } = Find(nameof(MaridiaMap));
    /// <summary>The <c>RoomHeader_ForgottenHighwaySave</c> room.</summary>
    public static RoomHeaderDefinition ForgottenHighwaySave { get; } = Find(nameof(ForgottenHighwaySave));
    /// <summary>The <c>RoomHeader_Toilet</c> room.</summary>
    public static RoomHeaderDefinition Toilet { get; } = Find(nameof(Toilet));
    /// <summary>The <c>RoomHeader_BugSandHole</c> room.</summary>
    public static RoomHeaderDefinition BugSandHole { get; } = Find(nameof(BugSandHole));
    /// <summary>The <c>RoomHeader_WestSandHall</c> room.</summary>
    public static RoomHeaderDefinition WestSandHall { get; } = Find(nameof(WestSandHall));
    /// <summary>The <c>RoomHeader_Oasis</c> room.</summary>
    public static RoomHeaderDefinition Oasis { get; } = Find(nameof(Oasis));
    /// <summary>The <c>RoomHeader_EastSandHall</c> room.</summary>
    public static RoomHeaderDefinition EastSandHall { get; } = Find(nameof(EastSandHall));
    /// <summary>The <c>RoomHeader_WestSandHole</c> room.</summary>
    public static RoomHeaderDefinition WestSandHole { get; } = Find(nameof(WestSandHole));
    /// <summary>The <c>RoomHeader_EastSandHole</c> room.</summary>
    public static RoomHeaderDefinition EastSandHole { get; } = Find(nameof(EastSandHole));
    /// <summary>The <c>RoomHeader_WestAqueductQuicksand</c> room.</summary>
    public static RoomHeaderDefinition WestAqueductQuicksand { get; } = Find(nameof(WestAqueductQuicksand));
    /// <summary>The <c>RoomHeader_EastAqueductQuicksand</c> room.</summary>
    public static RoomHeaderDefinition EastAqueductQuicksand { get; } = Find(nameof(EastAqueductQuicksand));
    /// <summary>The <c>RoomHeader_Aqueduct</c> room.</summary>
    public static RoomHeaderDefinition Aqueduct { get; } = Find(nameof(Aqueduct));
    /// <summary>The <c>RoomHeader_Butterfly</c> room.</summary>
    public static RoomHeaderDefinition Butterfly { get; } = Find(nameof(Butterfly));
    /// <summary>The <c>RoomHeader_BotwoonHallway</c> room.</summary>
    public static RoomHeaderDefinition BotwoonHallway { get; } = Find(nameof(BotwoonHallway));
    /// <summary>The <c>RoomHeader_Pants</c> room.</summary>
    public static RoomHeaderDefinition Pants { get; } = Find(nameof(Pants));
    /// <summary>The <c>RoomHeader_EastPants</c> room.</summary>
    public static RoomHeaderDefinition EastPants { get; } = Find(nameof(EastPants));
    /// <summary>The <c>RoomHeader_Springball</c> room.</summary>
    public static RoomHeaderDefinition Springball { get; } = Find(nameof(Springball));
    /// <summary>The <c>RoomHeader_BelowBotwoonETank</c> room.</summary>
    public static RoomHeaderDefinition BelowBotwoonETank { get; } = Find(nameof(BelowBotwoonETank));
    /// <summary>The <c>RoomHeader_Colosseum</c> room.</summary>
    public static RoomHeaderDefinition Colosseum { get; } = Find(nameof(Colosseum));
    /// <summary>The <c>RoomHeader_AqueductSave</c> room.</summary>
    public static RoomHeaderDefinition AqueductSave { get; } = Find(nameof(AqueductSave));
    /// <summary>The <c>RoomHeader_ThePrecious</c> room.</summary>
    public static RoomHeaderDefinition ThePrecious { get; } = Find(nameof(ThePrecious));
    /// <summary>The <c>RoomHeader_BotwoonETank</c> room.</summary>
    public static RoomHeaderDefinition BotwoonETank { get; } = Find(nameof(BotwoonETank));
    /// <summary>The <c>RoomHeader_DraygonSave</c> room.</summary>
    public static RoomHeaderDefinition DraygonSave { get; } = Find(nameof(DraygonSave));
    /// <summary>The <c>RoomHeader_MaridiaMissileRefill</c> room.</summary>
    public static RoomHeaderDefinition MaridiaMissileRefill { get; } = Find(nameof(MaridiaMissileRefill));
    /// <summary>The <c>RoomHeader_PlasmaBeachQuicksand</c> room.</summary>
    public static RoomHeaderDefinition PlasmaBeachQuicksand { get; } = Find(nameof(PlasmaBeachQuicksand));
    /// <summary>The <c>RoomHeader_BotwoonQuicksand</c> room.</summary>
    public static RoomHeaderDefinition BotwoonQuicksand { get; } = Find(nameof(BotwoonQuicksand));
    /// <summary>The <c>RoomHeader_Shaktool</c> room.</summary>
    public static RoomHeaderDefinition Shaktool { get; } = Find(nameof(Shaktool));
    /// <summary>The <c>RoomHeader_HalfieClimb</c> room.</summary>
    public static RoomHeaderDefinition HalfieClimb { get; } = Find(nameof(HalfieClimb));
    /// <summary>The <c>RoomHeader_Botwoon</c> room.</summary>
    public static RoomHeaderDefinition Botwoon { get; } = Find(nameof(Botwoon));
    /// <summary>The <c>RoomHeader_SpaceJump</c> room.</summary>
    public static RoomHeaderDefinition SpaceJump { get; } = Find(nameof(SpaceJump));
    /// <summary>The <c>RoomHeader_MaridiaEnergyRefill</c> room.</summary>
    public static RoomHeaderDefinition MaridiaEnergyRefill { get; } = Find(nameof(MaridiaEnergyRefill));
    /// <summary>The <c>RoomHeader_WestCactusAlley</c> room.</summary>
    public static RoomHeaderDefinition WestCactusAlley { get; } = Find(nameof(WestCactusAlley));
    /// <summary>The <c>RoomHeader_EastCactusAlley</c> room.</summary>
    public static RoomHeaderDefinition EastCactusAlley { get; } = Find(nameof(EastCactusAlley));
    /// <summary>The <c>RoomHeader_Draygon</c> room.</summary>
    public static RoomHeaderDefinition Draygon { get; } = Find(nameof(Draygon));
    /// <summary>The <c>RoomHeader_TourianFirst</c> room.</summary>
    public static RoomHeaderDefinition TourianFirst { get; } = Find(nameof(TourianFirst));
    /// <summary>The <c>RoomHeader_Metroids1</c> room.</summary>
    public static RoomHeaderDefinition Metroids1 { get; } = Find(nameof(Metroids1));
    /// <summary>The <c>RoomHeader_Metroids2</c> room.</summary>
    public static RoomHeaderDefinition Metroids2 { get; } = Find(nameof(Metroids2));
    /// <summary>The <c>RoomHeader_Metroids3</c> room.</summary>
    public static RoomHeaderDefinition Metroids3 { get; } = Find(nameof(Metroids3));
    /// <summary>The <c>RoomHeader_Metroids4</c> room.</summary>
    public static RoomHeaderDefinition Metroids4 { get; } = Find(nameof(Metroids4));
    /// <summary>The <c>RoomHeader_BlueHopper</c> room.</summary>
    public static RoomHeaderDefinition BlueHopper { get; } = Find(nameof(BlueHopper));
    /// <summary>The <c>RoomHeader_DustTorizo</c> room.</summary>
    public static RoomHeaderDefinition DustTorizo { get; } = Find(nameof(DustTorizo));
    /// <summary>The <c>RoomHeader_BigBoy</c> room.</summary>
    public static RoomHeaderDefinition BigBoy { get; } = Find(nameof(BigBoy));
    /// <summary>The <c>RoomHeader_Seaweed</c> room.</summary>
    public static RoomHeaderDefinition Seaweed { get; } = Find(nameof(Seaweed));
    /// <summary>The <c>RoomHeader_TourianRecharge</c> room.</summary>
    public static RoomHeaderDefinition TourianRecharge { get; } = Find(nameof(TourianRecharge));
    /// <summary>The <c>RoomHeader_MotherBrain</c> room.</summary>
    public static RoomHeaderDefinition MotherBrain { get; } = Find(nameof(MotherBrain));
    /// <summary>The <c>RoomHeader_TourianEyeDoor</c> room.</summary>
    public static RoomHeaderDefinition TourianEyeDoor { get; } = Find(nameof(TourianEyeDoor));
    /// <summary>The <c>RoomHeader_RinkaShaft</c> room.</summary>
    public static RoomHeaderDefinition RinkaShaft { get; } = Find(nameof(RinkaShaft));
    /// <summary>The <c>RoomHeader_MotherBrainSave</c> room.</summary>
    public static RoomHeaderDefinition MotherBrainSave { get; } = Find(nameof(MotherBrainSave));
    /// <summary>The <c>RoomHeader_TourianEscape1</c> room.</summary>
    public static RoomHeaderDefinition TourianEscape1 { get; } = Find(nameof(TourianEscape1));
    /// <summary>The <c>RoomHeader_TourianEscape2</c> room.</summary>
    public static RoomHeaderDefinition TourianEscape2 { get; } = Find(nameof(TourianEscape2));
    /// <summary>The <c>RoomHeader_TourianEscape3</c> room.</summary>
    public static RoomHeaderDefinition TourianEscape3 { get; } = Find(nameof(TourianEscape3));
    /// <summary>The <c>RoomHeader_TourianEscape4</c> room.</summary>
    public static RoomHeaderDefinition TourianEscape4 { get; } = Find(nameof(TourianEscape4));
    /// <summary>The <c>RoomHeader_UpperTourianSave</c> room.</summary>
    public static RoomHeaderDefinition UpperTourianSave { get; } = Find(nameof(UpperTourianSave));
    /// <summary>The <c>RoomHeader_CeresElev</c> room.</summary>
    public static RoomHeaderDefinition CeresElev { get; } = Find(nameof(CeresElev));
    /// <summary>The <c>RoomHeader_FallingTile</c> room.</summary>
    public static RoomHeaderDefinition FallingTile { get; } = Find(nameof(FallingTile));
    /// <summary>The <c>RoomHeader_MagnetStairs</c> room.</summary>
    public static RoomHeaderDefinition MagnetStairs { get; } = Find(nameof(MagnetStairs));
    /// <summary>The <c>RoomHeader_DeadScientist</c> room.</summary>
    public static RoomHeaderDefinition DeadScientist { get; } = Find(nameof(DeadScientist));
    /// <summary>The <c>RoomHeader_58Escape</c> room.</summary>
    public static RoomHeaderDefinition Room58Escape { get; } = Find(nameof(Room58Escape));
    /// <summary>The <c>RoomHeader_CeresRidley</c> room.</summary>
    public static RoomHeaderDefinition CeresRidley { get; } = Find(nameof(CeresRidley));
}
