using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Enemies.Elevator;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Verification's view of the cartridge pointers behind compiled doors and door lists: the import pipeline's door
/// records (<see cref="DoorHeaderSources"/>), the elevator pseudo-door words, and each room's door-list pointer.
/// </summary>
internal static class DoorDefinitionsAccess
{
    /// <summary>The shared elevator pseudo-door's door-list word.</summary>
    internal const ushort SharedElevatorPseudoDoorPointer = 0x88fc;

    /// <summary>The Maridia/Tourian elevator pseudo-door's door-list word.</summary>
    internal const ushort MaridiaTourianElevatorPseudoDoorPointer = 0xa18a;

    /// <summary>Each room, by the bank-$8F pointer of its door list.</summary>
    private static readonly Dictionary<ushort, RoomHeaderDefinition> RoomByListPointer = new()
    {
        [0x927b] = RoomHeaderDefinitions.LandingSite,
        [0x92f9] = RoomHeaderDefinitions.GauntletEntrance,
        [0x9362] = RoomHeaderDefinitions.Parlor,
        [0x93d1] = RoomHeaderDefinitions.CrateriaPowerBombs,
        [0x93fc] = RoomHeaderDefinitions.CrateriaSave,
        [0x9425] = RoomHeaderDefinitions.WestOcean,
        [0x9488] = RoomHeaderDefinitions.BowlingAlleyPath,
        [0x94b3] = RoomHeaderDefinitions.CrateriaKihunter,
        [0x94f3] = RoomHeaderDefinitions.ForgottenHighwayElev,
        [0x9524] = RoomHeaderDefinitions.EastOcean,
        [0x9579] = RoomHeaderDefinitions.ForgottenHighwayKagos,
        [0x95a4] = RoomHeaderDefinitions.CrabMaze,
        [0x95cf] = RoomHeaderDefinitions.ForgottenHighwayElbow,
        [0x95fb] = RoomHeaderDefinitions.CrateriaTube,
        [0x9626] = RoomHeaderDefinitions.Moat,
        [0x9651] = RoomHeaderDefinitions.RedBinstarElev,
        [0x9682] = RoomHeaderDefinitions.GauntletETank,
        [0x96b6] = RoomHeaderDefinitions.PreBowling,
        [0x971f] = RoomHeaderDefinitions.Climb,
        [0x97a1] = RoomHeaderDefinitions.Pit,
        [0x97fa] = RoomHeaderDefinitions.ElevToBlueBrinstar,
        [0x9869] = RoomHeaderDefinitions.BombTorizo,
        [0x98de] = RoomHeaderDefinitions.Flyway,
        [0x9909] = RoomHeaderDefinitions.PreMapFlyway,
        [0x9934] = RoomHeaderDefinitions.Terminator,
        [0x995f] = RoomHeaderDefinitions.GreenBrinstarElev,
        [0x9990] = RoomHeaderDefinitions.LowerMushrooms,
        [0x99bb] = RoomHeaderDefinitions.CrateriaMap,
        [0x99e4] = RoomHeaderDefinitions.GreenPiratesShaft,
        [0x9a20] = RoomHeaderDefinitions.CrateriaSuper,
        [0x9a8a] = RoomHeaderDefinitions.FinalMissileBombway,
        [0x9ad6] = RoomHeaderDefinitions.FinalMissile,
        [0x9b00] = RoomHeaderDefinitions.GreenBrinstarMainShaft,
        [0x9b82] = RoomHeaderDefinitions.SporeSpawnSuper,
        [0x9bc4] = RoomHeaderDefinitions.BrinstarPreMap,
        [0x9bef] = RoomHeaderDefinitions.EarlySupers,
        [0x9c2e] = RoomHeaderDefinitions.BrinstarReserveTank,
        [0x9c5c] = RoomHeaderDefinitions.GreenBrinstarMap,
        [0x9c85] = RoomHeaderDefinitions.GreenBrinstarFirefleas,
        [0x9cb0] = RoomHeaderDefinitions.GreenBrinstarMissileRefill,
        [0x9cda] = RoomHeaderDefinitions.Dachora,
        [0x9d40] = RoomHeaderDefinitions.BigPink,
        [0x9dc3] = RoomHeaderDefinitions.SporeSpawnKihunters,
        [0x9e0d] = RoomHeaderDefinitions.SporeSpawn,
        [0x9e38] = RoomHeaderDefinitions.PinkBrinstarPowerBombs,
        [0x9e79] = RoomHeaderDefinitions.GreenHillZone,
        [0x9ee5] = RoomHeaderDefinitions.MorphBall,
        [0x9f57] = RoomHeaderDefinitions.ConstructionZone,
        [0x9faa] = RoomHeaderDefinitions.BlueBrinstarETank,
        [0x9fe1] = RoomHeaderDefinitions.NoobBridge,
        [0xa00c] = RoomHeaderDefinitions.GreenBrinstarBeetoms,
        [0xa038] = RoomHeaderDefinitions.EtecoonETank,
        [0xa078] = RoomHeaderDefinitions.EtecoonSuper,
        [0xa0a2] = RoomHeaderDefinitions.DachoraEnergyRefill,
        [0xa0cb] = RoomHeaderDefinitions.SporeSpawnFarming,
        [0xa0f9] = RoomHeaderDefinitions.WaterwayETank,
        [0xa12e] = RoomHeaderDefinitions.FirstMissile,
        [0xa157] = RoomHeaderDefinitions.PinkBrinstarHoppers,
        [0xa182] = RoomHeaderDefinitions.HopperETank,
        [0xa1ab] = RoomHeaderDefinitions.BigPinkSaveRoom,
        [0xa1d4] = RoomHeaderDefinitions.BlueBrinstarBoulders,
        [0xa1ff] = RoomHeaderDefinitions.BillyMays,
        [0xa228] = RoomHeaderDefinitions.GreenBrinstarSave,
        [0xa251] = RoomHeaderDefinitions.EtecoonSave,
        [0xa27a] = RoomHeaderDefinitions.RedTower,
        [0xa2ba] = RoomHeaderDefinitions.RedBrinstarFirefleas,
        [0xa2f5] = RoomHeaderDefinitions.XrayScope,
        [0xa31e] = RoomHeaderDefinitions.Hellway,
        [0xa349] = RoomHeaderDefinitions.Caterpillar,
        [0xa3a3] = RoomHeaderDefinitions.BetaPowerBomb,
        [0xa3d5] = RoomHeaderDefinitions.AlphaPowerBomb,
        [0xa404] = RoomHeaderDefinitions.SkreeBoost,
        [0xa42f] = RoomHeaderDefinitions.BelowSpazer,
        [0xa46e] = RoomHeaderDefinitions.Spazer,
        [0xa498] = RoomHeaderDefinitions.WarehouseZeela,
        [0xa4d8] = RoomHeaderDefinitions.WarehouseETank,
        [0xa501] = RoomHeaderDefinitions.WarehouseKihunter,
        [0xa567] = RoomHeaderDefinitions.MiniKraid,
        [0xa592] = RoomHeaderDefinitions.KraidEyeDoor,
        [0xa5e5] = RoomHeaderDefinitions.Kraid,
        [0xa614] = RoomHeaderDefinitions.StatuesHallway,
        [0xa63f] = RoomHeaderDefinitions.RedTowerRefill,
        [0xa668] = RoomHeaderDefinitions.KraidRefill,
        [0xa691] = RoomHeaderDefinitions.Statues,
        [0xa6c8] = RoomHeaderDefinitions.WarehouseEntrance,
        [0xa709] = RoomHeaderDefinitions.VariaSuit,
        [0xa732] = RoomHeaderDefinitions.WarehouseSave,
        [0xa75b] = RoomHeaderDefinitions.RedBrinstarSave,
        [0xa784] = RoomHeaderDefinitions.IceBeamAcid,
        [0xa7af] = RoomHeaderDefinitions.Cathedral,
        [0xa7da] = RoomHeaderDefinitions.CathedralEntrance,
        [0xa805] = RoomHeaderDefinitions.BusinessCenter,
        [0xa83c] = RoomHeaderDefinitions.IceBeamGate,
        [0xa88c] = RoomHeaderDefinitions.IceBeamTutorial,
        [0xa8b7] = RoomHeaderDefinitions.IceBeam,
        [0xa8e0] = RoomHeaderDefinitions.IceBeamSnake,
        [0xa91f] = RoomHeaderDefinitions.CrumbleShaft,
        [0xa94a] = RoomHeaderDefinitions.CrocomireSpeedway,
        [0xa9d3] = RoomHeaderDefinitions.Crocomire,
        [0xaa0c] = RoomHeaderDefinitions.HiJumpBoots,
        [0xaa35] = RoomHeaderDefinitions.CrocomireEscape,
        [0xaa68] = RoomHeaderDefinitions.HiJumpETank,
        [0xaaa9] = RoomHeaderDefinitions.PostCrocFarming,
        [0xaadc] = RoomHeaderDefinitions.PostCrocSave,
        [0xab05] = RoomHeaderDefinitions.PostCrocPowerBombs,
        [0xab2e] = RoomHeaderDefinitions.PostCrocShaft,
        [0xab62] = RoomHeaderDefinitions.PostCrocMissile,
        [0xab8b] = RoomHeaderDefinitions.GrappleTutorial3,
        [0xabb6] = RoomHeaderDefinitions.PostCrocJump,
        [0xabf9] = RoomHeaderDefinitions.GrappleTutorial2,
        [0xac27] = RoomHeaderDefinitions.GrappleTutorial1,
        [0xac52] = RoomHeaderDefinitions.GrappleBeam,
        [0xac81] = RoomHeaderDefinitions.NorfairReserveTank,
        [0xacaa] = RoomHeaderDefinitions.GreenBubblesMissiles,
        [0xacda] = RoomHeaderDefinitions.BubbleMountain,
        [0xad17] = RoomHeaderDefinitions.SpeedBoosterHall,
        [0xad5c] = RoomHeaderDefinitions.SpeedBooster,
        [0xad85] = RoomHeaderDefinitions.SingleChamber,
        [0xadd4] = RoomHeaderDefinitions.DoubleChamber,
        [0xae05] = RoomHeaderDefinitions.WaveBeam,
        [0xae2e] = RoomHeaderDefinitions.SpikyPlatformsTunnel,
        [0xae59] = RoomHeaderDefinitions.Volcano,
        [0xae9b] = RoomHeaderDefinitions.KronicBoost,
        [0xaedb] = RoomHeaderDefinitions.MagdolliteTunnel,
        [0xaf06] = RoomHeaderDefinitions.PurpleShaft,
        [0xaf3b] = RoomHeaderDefinitions.LavaDive,
        [0xaf66] = RoomHeaderDefinitions.LowerNorfairElev,
        [0xaf99] = RoomHeaderDefinitions.UpperNorfairFarming,
        [0xafca] = RoomHeaderDefinitions.RisingTide,
        [0xaff5] = RoomHeaderDefinitions.AcidSnakesTunnel,
        [0xb022] = RoomHeaderDefinitions.SpikyAcidSnakesTunnel,
        [0xb04d] = RoomHeaderDefinitions.UpperNorfairRefill,
        [0xb078] = RoomHeaderDefinitions.PurpleFarming,
        [0xb0a1] = RoomHeaderDefinitions.BatCave,
        [0xb0db] = RoomHeaderDefinitions.NorfairMap,
        [0xb104] = RoomHeaderDefinitions.BubbleMountainSave,
        [0xb12d] = RoomHeaderDefinitions.FrogSpeedway,
        [0xb160] = RoomHeaderDefinitions.RedPirateShaft,
        [0xb18e] = RoomHeaderDefinitions.FrogSave,
        [0xb1b9] = RoomHeaderDefinitions.CrocomireSave,
        [0xb1e2] = RoomHeaderDefinitions.LNElevSave,
        [0xb20c] = RoomHeaderDefinitions.AcidStatue,
        [0xb25d] = RoomHeaderDefinitions.MainHall,
        [0xb2c9] = RoomHeaderDefinitions.GoldenTorizo,
        [0xb301] = RoomHeaderDefinitions.FastRipper,
        [0xb32c] = RoomHeaderDefinitions.GTEnergyRefill,
        [0xb374] = RoomHeaderDefinitions.Ridley,
        [0xb3a1] = RoomHeaderDefinitions.LNFarming,
        [0xb3cc] = RoomHeaderDefinitions.FastPillarsSetup,
        [0xb408] = RoomHeaderDefinitions.UnusedRoom8FB3E1,
        [0xb431] = RoomHeaderDefinitions.MickeyMouse,
        [0xb47e] = RoomHeaderDefinitions.Pillar,
        [0xb4a9] = RoomHeaderDefinitions.Plowerhouse,
        [0xb4d4] = RoomHeaderDefinitions.WorstRoomInTheGame,
        [0xb50c] = RoomHeaderDefinitions.Amphitheatre,
        [0xb537] = RoomHeaderDefinitions.LNSpringBallMaze,
        [0xb581] = RoomHeaderDefinitions.LNEscapePowerBombs,
        [0xb5ac] = RoomHeaderDefinitions.RedKihunterShaft,
        [0xb5fc] = RoomHeaderDefinitions.Wasteland,
        [0xb652] = RoomHeaderDefinitions.MetalPirates,
        [0xb67d] = RoomHeaderDefinitions.ThreeMusketeers,
        [0xb6bf] = RoomHeaderDefinitions.RidleyETank,
        [0xb6e8] = RoomHeaderDefinitions.ScrewAttack,
        [0xb715] = RoomHeaderDefinitions.LNFireflea,
        [0xb768] = RoomHeaderDefinitions.LNSave,
        [0xc9d4] = RoomHeaderDefinitions.BowlingAlley,
        [0xca4e] = RoomHeaderDefinitions.WreckedShipEntrance,
        [0xca98] = RoomHeaderDefinitions.Attic,
        [0xcaf4] = RoomHeaderDefinitions.AssemblyLine,
        [0xcb3c] = RoomHeaderDefinitions.WreckedShipMainShaft,
        [0xcbd1] = RoomHeaderDefinitions.SpikyDeath,
        [0xcc1b] = RoomHeaderDefinitions.ElectricDeath,
        [0xcc6d] = RoomHeaderDefinitions.WreckedShipETank,
        [0xccb5] = RoomHeaderDefinitions.Basement,
        [0xcd11] = RoomHeaderDefinitions.WreckedShipMap,
        [0xcd59] = RoomHeaderDefinitions.Phantoon,
        [0xcda2] = RoomHeaderDefinitions.SpongeBath,
        [0xcdee] = RoomHeaderDefinitions.WSWestSuper,
        [0xce37] = RoomHeaderDefinitions.WSEastSuper,
        [0xce86] = RoomHeaderDefinitions.GravitySuit,
        [0xced0] = RoomHeaderDefinitions.WreckedShipSave,
        [0xcef9] = RoomHeaderDefinitions.GlassTunnelSave,
        [0xcf41] = RoomHeaderDefinitions.GlassTunnel,
        [0xcf7b] = RoomHeaderDefinitions.WestTunnel,
        [0xcfa7] = RoomHeaderDefinitions.EastTunnel,
        [0xcff0] = RoomHeaderDefinitions.MainStreet,
        [0xd03e] = RoomHeaderDefinitions.FishTank,
        [0xd07c] = RoomHeaderDefinitions.MamaTurtle,
        [0xd0b1] = RoomHeaderDefinitions.CrabTunnel,
        [0xd0e0] = RoomHeaderDefinitions.MtEverest,
        [0xd12b] = RoomHeaderDefinitions.RedFish,
        [0xd162] = RoomHeaderDefinitions.WateringHole,
        [0xd194] = RoomHeaderDefinitions.NWestMaridiaBug,
        [0xd1ca] = RoomHeaderDefinitions.CrabShaft,
        [0xd204] = RoomHeaderDefinitions.PseudoPlasmaSpark,
        [0xd243] = RoomHeaderDefinitions.CrabHole,
        [0xd279] = RoomHeaderDefinitions.WestSandHallTunnel,
        [0xd2a5] = RoomHeaderDefinitions.PlasmaTutorial,
        [0xd2d1] = RoomHeaderDefinitions.Plasma,
        [0xd300] = RoomHeaderDefinitions.ThreadTheNeedle,
        [0xd332] = RoomHeaderDefinitions.MaridiaElev,
        [0xd367] = RoomHeaderDefinitions.PlasmaSpark,
        [0xd3ae] = RoomHeaderDefinitions.Kassiuz,
        [0xd3dd] = RoomHeaderDefinitions.MaridiaMap,
        [0xd406] = RoomHeaderDefinitions.ForgottenHighwaySave,
        [0xd42f] = RoomHeaderDefinitions.Toilet,
        [0xd45a] = RoomHeaderDefinitions.BugSandHole,
        [0xd488] = RoomHeaderDefinitions.WestSandHall,
        [0xd4b5] = RoomHeaderDefinitions.Oasis,
        [0xd4e9] = RoomHeaderDefinitions.EastSandHall,
        [0xd516] = RoomHeaderDefinitions.WestSandHole,
        [0xd545] = RoomHeaderDefinitions.EastSandHole,
        [0xd574] = RoomHeaderDefinitions.WestAqueductQuicksand,
        [0xd5a1] = RoomHeaderDefinitions.EastAqueductQuicksand,
        [0xd5ce] = RoomHeaderDefinitions.Aqueduct,
        [0xd613] = RoomHeaderDefinitions.Butterfly,
        [0xd63e] = RoomHeaderDefinitions.BotwoonHallway,
        [0xd66d] = RoomHeaderDefinitions.Pants,
        [0xd6c1] = RoomHeaderDefinitions.EastPants,
        [0xd6f7] = RoomHeaderDefinitions.Springball,
        [0xd724] = RoomHeaderDefinitions.BelowBotwoonETank,
        [0xd751] = RoomHeaderDefinitions.Colosseum,
        [0xd78c] = RoomHeaderDefinitions.AqueductSave,
        [0xd7d5] = RoomHeaderDefinitions.ThePrecious,
        [0xd80b] = RoomHeaderDefinitions.BotwoonETank,
        [0xd841] = RoomHeaderDefinitions.DraygonSave,
        [0xd86c] = RoomHeaderDefinitions.MaridiaMissileRefill,
        [0xd895] = RoomHeaderDefinitions.PlasmaBeachQuicksand,
        [0xd8bf] = RoomHeaderDefinitions.BotwoonQuicksand,
        [0xd90b] = RoomHeaderDefinitions.Shaktool,
        [0xd93a] = RoomHeaderDefinitions.HalfieClimb,
        [0xd9a4] = RoomHeaderDefinitions.Botwoon,
        [0xd9d1] = RoomHeaderDefinitions.SpaceJump,
        [0xd9fb] = RoomHeaderDefinitions.MaridiaEnergyRefill,
        [0xda25] = RoomHeaderDefinitions.WestCactusAlley,
        [0xda52] = RoomHeaderDefinitions.EastCactusAlley,
        [0xdaa6] = RoomHeaderDefinitions.Draygon,
        [0xdad5] = RoomHeaderDefinitions.TourianFirst,
        [0xdb27] = RoomHeaderDefinitions.Metroids1,
        [0xdb77] = RoomHeaderDefinitions.Metroids2,
        [0xdbc3] = RoomHeaderDefinitions.Metroids3,
        [0xdc13] = RoomHeaderDefinitions.Metroids4,
        [0xdc5f] = RoomHeaderDefinitions.BlueHopper,
        [0xdcab] = RoomHeaderDefinitions.DustTorizo,
        [0xdcf7] = RoomHeaderDefinitions.BigBoy,
        [0xdd26] = RoomHeaderDefinitions.Seaweed,
        [0xdd55] = RoomHeaderDefinitions.TourianRecharge,
        [0xddbc] = RoomHeaderDefinitions.MotherBrain,
        [0xddeb] = RoomHeaderDefinitions.TourianEyeDoor,
        [0xde1a] = RoomHeaderDefinitions.RinkaShaft,
        [0xde4a] = RoomHeaderDefinitions.MotherBrainSave,
        [0xde74] = RoomHeaderDefinitions.TourianEscape1,
        [0xdea1] = RoomHeaderDefinitions.TourianEscape2,
        [0xdece] = RoomHeaderDefinitions.TourianEscape3,
        [0xdf05] = RoomHeaderDefinitions.TourianEscape4,
        [0xdf42] = RoomHeaderDefinitions.UpperTourianSave,
        [0xdf8b] = RoomHeaderDefinitions.CeresElev,
        [0xdfd3] = RoomHeaderDefinitions.FallingTile,
        [0xe01d] = RoomHeaderDefinitions.MagnetStairs,
        [0xe067] = RoomHeaderDefinitions.DeadScientist,
        [0xe0b1] = RoomHeaderDefinitions.Room58Escape,
        [0xe0fb] = RoomHeaderDefinitions.CeresRidley,
    };

    /// <summary>Resolves compiled doors by record pointer.</summary>
    extension(DoorDefinitions)
    {
        /// <inheritdoc cref="DoorHeaderSources.Door"/>
        internal static CartridgeDoorHeader Get(ushort pointer) => DoorHeaderSources.Door(pointer);

        /// <inheritdoc cref="DoorHeaderSourcesConstants.PointerOf"/>
        internal static ushort PointerOf(CartridgeDoorHeader door) => DoorHeaderSourcesConstants.PointerOf(door);

        /// <summary>Resolves a type-$9 BTS byte through the door list at <paramref name="listPointer"/>, as bank $94 does.</summary>
        internal static DoorListEntry Resolve(ushort listPointer, int behavior)
        {
            IReadOnlyList<DoorListEntry> doors = RoomDoorListDefinitions.ForListPointer(listPointer);
            int index = behavior & RoomDoorListDefinitions.BtsIndexMask;
            return index < doors.Count
                ? doors[index]
                : throw new InvalidDataException($"Door index {index} is outside the {doors.Count}-entry list $8F:{listPointer:X4}.");
        }
    }

    /// <summary>Resolves room door lists by their cartridge pointer.</summary>
    extension(RoomDoorListDefinitions)
    {
        /// <summary>The door list of the room whose list is at <paramref name="listPointer"/>.</summary>
        internal static IReadOnlyList<DoorListEntry> ForListPointer(ushort listPointer) =>
            RoomByListPointer.TryGetValue(listPointer, out RoomHeaderDefinition? room)
                ? RoomDoorListDefinitions.ByRoom[room]
                : throw new ArgumentOutOfRangeException(nameof(listPointer), listPointer, "Pointer is not one of the 262 retail room door lists.");

        /// <summary>The bank-$8F pointer of <paramref name="room"/>'s door list.</summary>
        internal static ushort ListPointerOf(RoomHeaderDefinition room) =>
            RoomByListPointer.Single(pair => pair.Value.Name == room.Name).Key;
    }

    /// <summary>The record pointer of a compiled door.</summary>
    extension(CartridgeDoorHeader self)
    {
        /// <summary>The bank-$83 record pointer of this door.</summary>
        internal ushort Pointer => DoorHeaderSourcesConstants.PointerOf(self);
    }

    /// <summary>The door-list word of an entry.</summary>
    extension(DoorListEntry self)
    {
        /// <summary>The word the cartridge's door list stores for this entry.</summary>
        internal ushort Pointer => self.Door is { } door
            ? DoorHeaderSourcesConstants.PointerOf(door)
            : self.PseudoDoor == ElevatorPseudoDoor.Shared ? SharedElevatorPseudoDoorPointer : MaridiaTourianElevatorPseudoDoorPointer;
    }
}
