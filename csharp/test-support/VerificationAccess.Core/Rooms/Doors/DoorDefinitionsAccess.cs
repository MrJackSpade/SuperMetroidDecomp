using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Rooms;

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
    private static readonly Dictionary<ushort, RoomDefinition> RoomByListPointer = new()
    {
        [0x927b] = RoomDefinitions.LandingSite,
        [0x92f9] = RoomDefinitions.GauntletEntrance,
        [0x9362] = RoomDefinitions.Parlor,
        [0x93d1] = RoomDefinitions.CrateriaPowerBombs,
        [0x93fc] = RoomDefinitions.CrateriaSave,
        [0x9425] = RoomDefinitions.WestOcean,
        [0x9488] = RoomDefinitions.BowlingAlleyPath,
        [0x94b3] = RoomDefinitions.CrateriaKihunter,
        [0x94f3] = RoomDefinitions.ForgottenHighwayElev,
        [0x9524] = RoomDefinitions.EastOcean,
        [0x9579] = RoomDefinitions.ForgottenHighwayKagos,
        [0x95a4] = RoomDefinitions.CrabMaze,
        [0x95cf] = RoomDefinitions.ForgottenHighwayElbow,
        [0x95fb] = RoomDefinitions.CrateriaTube,
        [0x9626] = RoomDefinitions.Moat,
        [0x9651] = RoomDefinitions.RedBinstarElev,
        [0x9682] = RoomDefinitions.GauntletETank,
        [0x96b6] = RoomDefinitions.PreBowling,
        [0x971f] = RoomDefinitions.Climb,
        [0x97a1] = RoomDefinitions.Pit,
        [0x97fa] = RoomDefinitions.ElevToBlueBrinstar,
        [0x9869] = RoomDefinitions.BombTorizo,
        [0x98de] = RoomDefinitions.Flyway,
        [0x9909] = RoomDefinitions.PreMapFlyway,
        [0x9934] = RoomDefinitions.Terminator,
        [0x995f] = RoomDefinitions.GreenBrinstarElev,
        [0x9990] = RoomDefinitions.LowerMushrooms,
        [0x99bb] = RoomDefinitions.CrateriaMap,
        [0x99e4] = RoomDefinitions.GreenPiratesShaft,
        [0x9a20] = RoomDefinitions.CrateriaSuper,
        [0x9a8a] = RoomDefinitions.FinalMissileBombway,
        [0x9ad6] = RoomDefinitions.FinalMissile,
        [0x9b00] = RoomDefinitions.GreenBrinstarMainShaft,
        [0x9b82] = RoomDefinitions.SporeSpawnSuper,
        [0x9bc4] = RoomDefinitions.BrinstarPreMap,
        [0x9bef] = RoomDefinitions.EarlySupers,
        [0x9c2e] = RoomDefinitions.BrinstarReserveTank,
        [0x9c5c] = RoomDefinitions.GreenBrinstarMap,
        [0x9c85] = RoomDefinitions.GreenBrinstarFirefleas,
        [0x9cb0] = RoomDefinitions.GreenBrinstarMissileRefill,
        [0x9cda] = RoomDefinitions.Dachora,
        [0x9d40] = RoomDefinitions.BigPink,
        [0x9dc3] = RoomDefinitions.SporeSpawnKihunters,
        [0x9e0d] = RoomDefinitions.SporeSpawn,
        [0x9e38] = RoomDefinitions.PinkBrinstarPowerBombs,
        [0x9e79] = RoomDefinitions.GreenHillZone,
        [0x9ee5] = RoomDefinitions.MorphBall,
        [0x9f57] = RoomDefinitions.ConstructionZone,
        [0x9faa] = RoomDefinitions.BlueBrinstarETank,
        [0x9fe1] = RoomDefinitions.NoobBridge,
        [0xa00c] = RoomDefinitions.GreenBrinstarBeetoms,
        [0xa038] = RoomDefinitions.EtecoonETank,
        [0xa078] = RoomDefinitions.EtecoonSuper,
        [0xa0a2] = RoomDefinitions.DachoraEnergyRefill,
        [0xa0cb] = RoomDefinitions.SporeSpawnFarming,
        [0xa0f9] = RoomDefinitions.WaterwayETank,
        [0xa12e] = RoomDefinitions.FirstMissile,
        [0xa157] = RoomDefinitions.PinkBrinstarHoppers,
        [0xa182] = RoomDefinitions.HopperETank,
        [0xa1ab] = RoomDefinitions.BigPinkSaveRoom,
        [0xa1d4] = RoomDefinitions.BlueBrinstarBoulders,
        [0xa1ff] = RoomDefinitions.BillyMays,
        [0xa228] = RoomDefinitions.GreenBrinstarSave,
        [0xa251] = RoomDefinitions.EtecoonSave,
        [0xa27a] = RoomDefinitions.RedTower,
        [0xa2ba] = RoomDefinitions.RedBrinstarFirefleas,
        [0xa2f5] = RoomDefinitions.XrayScope,
        [0xa31e] = RoomDefinitions.Hellway,
        [0xa349] = RoomDefinitions.Caterpillar,
        [0xa3a3] = RoomDefinitions.BetaPowerBomb,
        [0xa3d5] = RoomDefinitions.AlphaPowerBomb,
        [0xa404] = RoomDefinitions.SkreeBoost,
        [0xa42f] = RoomDefinitions.BelowSpazer,
        [0xa46e] = RoomDefinitions.Spazer,
        [0xa498] = RoomDefinitions.WarehouseZeela,
        [0xa4d8] = RoomDefinitions.WarehouseETank,
        [0xa501] = RoomDefinitions.WarehouseKihunter,
        [0xa567] = RoomDefinitions.MiniKraid,
        [0xa592] = RoomDefinitions.KraidEyeDoor,
        [0xa5e5] = RoomDefinitions.Kraid,
        [0xa614] = RoomDefinitions.StatuesHallway,
        [0xa63f] = RoomDefinitions.RedTowerRefill,
        [0xa668] = RoomDefinitions.KraidRefill,
        [0xa691] = RoomDefinitions.Statues,
        [0xa6c8] = RoomDefinitions.WarehouseEntrance,
        [0xa709] = RoomDefinitions.VariaSuit,
        [0xa732] = RoomDefinitions.WarehouseSave,
        [0xa75b] = RoomDefinitions.RedBrinstarSave,
        [0xa784] = RoomDefinitions.IceBeamAcid,
        [0xa7af] = RoomDefinitions.Cathedral,
        [0xa7da] = RoomDefinitions.CathedralEntrance,
        [0xa805] = RoomDefinitions.BusinessCenter,
        [0xa83c] = RoomDefinitions.IceBeamGate,
        [0xa88c] = RoomDefinitions.IceBeamTutorial,
        [0xa8b7] = RoomDefinitions.IceBeam,
        [0xa8e0] = RoomDefinitions.IceBeamSnake,
        [0xa91f] = RoomDefinitions.CrumbleShaft,
        [0xa94a] = RoomDefinitions.CrocomireSpeedway,
        [0xa9d3] = RoomDefinitions.Crocomire,
        [0xaa0c] = RoomDefinitions.HiJumpBoots,
        [0xaa35] = RoomDefinitions.CrocomireEscape,
        [0xaa68] = RoomDefinitions.HiJumpETank,
        [0xaaa9] = RoomDefinitions.PostCrocFarming,
        [0xaadc] = RoomDefinitions.PostCrocSave,
        [0xab05] = RoomDefinitions.PostCrocPowerBombs,
        [0xab2e] = RoomDefinitions.PostCrocShaft,
        [0xab62] = RoomDefinitions.PostCrocMissile,
        [0xab8b] = RoomDefinitions.GrappleTutorial3,
        [0xabb6] = RoomDefinitions.PostCrocJump,
        [0xabf9] = RoomDefinitions.GrappleTutorial2,
        [0xac27] = RoomDefinitions.GrappleTutorial1,
        [0xac52] = RoomDefinitions.GrappleBeam,
        [0xac81] = RoomDefinitions.NorfairReserveTank,
        [0xacaa] = RoomDefinitions.GreenBubblesMissiles,
        [0xacda] = RoomDefinitions.BubbleMountain,
        [0xad17] = RoomDefinitions.SpeedBoosterHall,
        [0xad5c] = RoomDefinitions.SpeedBooster,
        [0xad85] = RoomDefinitions.SingleChamber,
        [0xadd4] = RoomDefinitions.DoubleChamber,
        [0xae05] = RoomDefinitions.WaveBeam,
        [0xae2e] = RoomDefinitions.SpikyPlatformsTunnel,
        [0xae59] = RoomDefinitions.Volcano,
        [0xae9b] = RoomDefinitions.KronicBoost,
        [0xaedb] = RoomDefinitions.MagdolliteTunnel,
        [0xaf06] = RoomDefinitions.PurpleShaft,
        [0xaf3b] = RoomDefinitions.LavaDive,
        [0xaf66] = RoomDefinitions.LowerNorfairElev,
        [0xaf99] = RoomDefinitions.UpperNorfairFarming,
        [0xafca] = RoomDefinitions.RisingTide,
        [0xaff5] = RoomDefinitions.AcidSnakesTunnel,
        [0xb022] = RoomDefinitions.SpikyAcidSnakesTunnel,
        [0xb04d] = RoomDefinitions.UpperNorfairRefill,
        [0xb078] = RoomDefinitions.PurpleFarming,
        [0xb0a1] = RoomDefinitions.BatCave,
        [0xb0db] = RoomDefinitions.NorfairMap,
        [0xb104] = RoomDefinitions.BubbleMountainSave,
        [0xb12d] = RoomDefinitions.FrogSpeedway,
        [0xb160] = RoomDefinitions.RedPirateShaft,
        [0xb18e] = RoomDefinitions.FrogSave,
        [0xb1b9] = RoomDefinitions.CrocomireSave,
        [0xb1e2] = RoomDefinitions.LNElevSave,
        [0xb20c] = RoomDefinitions.AcidStatue,
        [0xb25d] = RoomDefinitions.MainHall,
        [0xb2c9] = RoomDefinitions.GoldenTorizo,
        [0xb301] = RoomDefinitions.FastRipper,
        [0xb32c] = RoomDefinitions.GTEnergyRefill,
        [0xb374] = RoomDefinitions.Ridley,
        [0xb3a1] = RoomDefinitions.LNFarming,
        [0xb3cc] = RoomDefinitions.FastPillarsSetup,
        [0xb408] = RoomDefinitions.UnusedNorfairRoom,
        [0xb431] = RoomDefinitions.MickeyMouse,
        [0xb47e] = RoomDefinitions.Pillar,
        [0xb4a9] = RoomDefinitions.Plowerhouse,
        [0xb4d4] = RoomDefinitions.WorstRoomInTheGame,
        [0xb50c] = RoomDefinitions.Amphitheatre,
        [0xb537] = RoomDefinitions.LNSpringBallMaze,
        [0xb581] = RoomDefinitions.LNEscapePowerBombs,
        [0xb5ac] = RoomDefinitions.RedKihunterShaft,
        [0xb5fc] = RoomDefinitions.Wasteland,
        [0xb652] = RoomDefinitions.MetalPirates,
        [0xb67d] = RoomDefinitions.ThreeMusketeers,
        [0xb6bf] = RoomDefinitions.RidleyETank,
        [0xb6e8] = RoomDefinitions.ScrewAttack,
        [0xb715] = RoomDefinitions.LNFireflea,
        [0xb768] = RoomDefinitions.LNSave,
        [0xc9d4] = RoomDefinitions.BowlingAlley,
        [0xca4e] = RoomDefinitions.WreckedShipEntrance,
        [0xca98] = RoomDefinitions.Attic,
        [0xcaf4] = RoomDefinitions.AssemblyLine,
        [0xcb3c] = RoomDefinitions.WreckedShipMainShaft,
        [0xcbd1] = RoomDefinitions.SpikyDeath,
        [0xcc1b] = RoomDefinitions.ElectricDeath,
        [0xcc6d] = RoomDefinitions.WreckedShipETank,
        [0xccb5] = RoomDefinitions.Basement,
        [0xcd11] = RoomDefinitions.WreckedShipMap,
        [0xcd59] = RoomDefinitions.Phantoon,
        [0xcda2] = RoomDefinitions.SpongeBath,
        [0xcdee] = RoomDefinitions.WSWestSuper,
        [0xce37] = RoomDefinitions.WSEastSuper,
        [0xce86] = RoomDefinitions.GravitySuit,
        [0xced0] = RoomDefinitions.WreckedShipSave,
        [0xcef9] = RoomDefinitions.GlassTunnelSave,
        [0xcf41] = RoomDefinitions.GlassTunnel,
        [0xcf7b] = RoomDefinitions.WestTunnel,
        [0xcfa7] = RoomDefinitions.EastTunnel,
        [0xcff0] = RoomDefinitions.MainStreet,
        [0xd03e] = RoomDefinitions.FishTank,
        [0xd07c] = RoomDefinitions.MamaTurtle,
        [0xd0b1] = RoomDefinitions.CrabTunnel,
        [0xd0e0] = RoomDefinitions.MtEverest,
        [0xd12b] = RoomDefinitions.RedFish,
        [0xd162] = RoomDefinitions.WateringHole,
        [0xd194] = RoomDefinitions.NWestMaridiaBug,
        [0xd1ca] = RoomDefinitions.CrabShaft,
        [0xd204] = RoomDefinitions.PseudoPlasmaSpark,
        [0xd243] = RoomDefinitions.CrabHole,
        [0xd279] = RoomDefinitions.WestSandHallTunnel,
        [0xd2a5] = RoomDefinitions.PlasmaTutorial,
        [0xd2d1] = RoomDefinitions.Plasma,
        [0xd300] = RoomDefinitions.ThreadTheNeedle,
        [0xd332] = RoomDefinitions.MaridiaElev,
        [0xd367] = RoomDefinitions.PlasmaSpark,
        [0xd3ae] = RoomDefinitions.Kassiuz,
        [0xd3dd] = RoomDefinitions.MaridiaMap,
        [0xd406] = RoomDefinitions.ForgottenHighwaySave,
        [0xd42f] = RoomDefinitions.Toilet,
        [0xd45a] = RoomDefinitions.BugSandHole,
        [0xd488] = RoomDefinitions.WestSandHall,
        [0xd4b5] = RoomDefinitions.Oasis,
        [0xd4e9] = RoomDefinitions.EastSandHall,
        [0xd516] = RoomDefinitions.WestSandHole,
        [0xd545] = RoomDefinitions.EastSandHole,
        [0xd574] = RoomDefinitions.WestAqueductQuicksand,
        [0xd5a1] = RoomDefinitions.EastAqueductQuicksand,
        [0xd5ce] = RoomDefinitions.Aqueduct,
        [0xd613] = RoomDefinitions.Butterfly,
        [0xd63e] = RoomDefinitions.BotwoonHallway,
        [0xd66d] = RoomDefinitions.Pants,
        [0xd6c1] = RoomDefinitions.EastPants,
        [0xd6f7] = RoomDefinitions.Springball,
        [0xd724] = RoomDefinitions.BelowBotwoonETank,
        [0xd751] = RoomDefinitions.Colosseum,
        [0xd78c] = RoomDefinitions.AqueductSave,
        [0xd7d5] = RoomDefinitions.ThePrecious,
        [0xd80b] = RoomDefinitions.BotwoonETank,
        [0xd841] = RoomDefinitions.DraygonSave,
        [0xd86c] = RoomDefinitions.MaridiaMissileRefill,
        [0xd895] = RoomDefinitions.PlasmaBeachQuicksand,
        [0xd8bf] = RoomDefinitions.BotwoonQuicksand,
        [0xd90b] = RoomDefinitions.Shaktool,
        [0xd93a] = RoomDefinitions.HalfieClimb,
        [0xd9a4] = RoomDefinitions.Botwoon,
        [0xd9d1] = RoomDefinitions.SpaceJump,
        [0xd9fb] = RoomDefinitions.MaridiaEnergyRefill,
        [0xda25] = RoomDefinitions.WestCactusAlley,
        [0xda52] = RoomDefinitions.EastCactusAlley,
        [0xdaa6] = RoomDefinitions.Draygon,
        [0xdad5] = RoomDefinitions.TourianFirst,
        [0xdb27] = RoomDefinitions.Metroids1,
        [0xdb77] = RoomDefinitions.Metroids2,
        [0xdbc3] = RoomDefinitions.Metroids3,
        [0xdc13] = RoomDefinitions.Metroids4,
        [0xdc5f] = RoomDefinitions.BlueHopper,
        [0xdcab] = RoomDefinitions.DustTorizo,
        [0xdcf7] = RoomDefinitions.BigBoy,
        [0xdd26] = RoomDefinitions.Seaweed,
        [0xdd55] = RoomDefinitions.TourianRecharge,
        [0xddbc] = RoomDefinitions.MotherBrain,
        [0xddeb] = RoomDefinitions.TourianEyeDoor,
        [0xde1a] = RoomDefinitions.RinkaShaft,
        [0xde4a] = RoomDefinitions.MotherBrainSave,
        [0xde74] = RoomDefinitions.TourianEscape1,
        [0xdea1] = RoomDefinitions.TourianEscape2,
        [0xdece] = RoomDefinitions.TourianEscape3,
        [0xdf05] = RoomDefinitions.TourianEscape4,
        [0xdf42] = RoomDefinitions.UpperTourianSave,
        [0xdf8b] = RoomDefinitions.CeresElev,
        [0xdfd3] = RoomDefinitions.FallingTile,
        [0xe01d] = RoomDefinitions.MagnetStairs,
        [0xe067] = RoomDefinitions.DeadScientist,
        [0xe0b1] = RoomDefinitions.Room58Escape,
        [0xe0fb] = RoomDefinitions.CeresRidley,
    };

    /// <summary>Resolves compiled doors by record pointer.</summary>
    extension(DoorDefinitions)
    {
        /// <inheritdoc cref="DoorHeaderSources.Door"/>
        internal static DoorDefinition Get(ushort pointer) => DoorHeaderSources.Door(pointer);

        /// <inheritdoc cref="DoorHeaderSourcesConstants.PointerOf"/>
        internal static ushort PointerOf(DoorDefinition door) => DoorHeaderSourcesConstants.PointerOf(door);

        /// <summary>Resolves a type-$9 BTS byte through the door list at <paramref name="listPointer"/>, as bank $94 does.</summary>
        internal static DoorListEntry Resolve(ushort listPointer, int behavior)
        {
            IReadOnlyList<DoorListEntry> doors = RoomDoorListDefinitions.ForListPointer(listPointer);
            int index = RoomBlockBehaviorCodec.Decode(behavior).DoorLink;
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
            RoomByListPointer.TryGetValue(listPointer, out RoomDefinition? room)
                ? RoomDoorListDefinitions.ByRoom[room]
                : throw new ArgumentOutOfRangeException(nameof(listPointer), listPointer, "Pointer is not one of the 262 retail room door lists.");

        /// <summary>The bank-$8F pointer of <paramref name="room"/>'s door list.</summary>
        internal static ushort ListPointerOf(RoomDefinition room) =>
            RoomByListPointer.Single(pair => pair.Value.Name == room.Name).Key;
    }

    /// <summary>The record pointer of a compiled door.</summary>
    extension(DoorDefinition self)
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
