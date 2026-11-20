using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>The game's rooms, every one of which <see cref="All"/> lists.</summary>
/// <remarks>Each entry holds a room's area, map position, size in screens, camera
/// scroll offsets and whether its door transitions are isolated. The developers' debug room is not among them.</remarks>
[NumberedIdentities("Rooms such as Metroids1-4 and TourianEscape1-4 are distinct rooms with their own definitions; the number is part of each room's name, not a position anything indexes.")]
public static class RoomDefinitions
{
    /// <summary>The Landing Site room.</summary>
    public static RoomDefinition LandingSite { get; } = new("LandingSite", AreaId.Crateria, new(23, 1), new(9, 5), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.WetWhileRaining };
    /// <summary>The Gauntlet Entrance room.</summary>
    public static RoomDefinition GauntletEntrance { get; } = new("GauntletEntrance", AreaId.Crateria, new(18, 3), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Parlor room.</summary>
    public static RoomDefinition Parlor { get; } = new("Parlor", AreaId.Crateria, new(18, 5), new(5, 5), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crateria Power Bombs room.</summary>
    public static RoomDefinition CrateriaPowerBombs { get; } = new("CrateriaPowerBombs", AreaId.Crateria, new(32, 2), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crateria Save room.</summary>
    public static RoomDefinition CrateriaSave { get; } = new("CrateriaSave", AreaId.Crateria, new(18, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The West Ocean room.</summary>
    public static RoomDefinition WestOcean { get; } = new("WestOcean", AreaId.Crateria, new(38, 1), new(8, 6), 144, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.WetBelowWaterline };
    /// <summary>The Bowling Alley Path room.</summary>
    public static RoomDefinition BowlingAlleyPath { get; } = new("BowlingAlleyPath", AreaId.Crateria, new(41, 3), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crateria Kihunter room.</summary>
    public static RoomDefinition CrateriaKihunter { get; } = new("CrateriaKihunter", AreaId.Crateria, new(33, 5), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Wet };
    /// <summary>The Forgotten Highway Elevator room.</summary>
    public static RoomDefinition ForgottenHighwayElevator { get; } = new("ForgottenHighwayElevator", AreaId.Crateria, new(52, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The East Ocean room.</summary>
    public static RoomDefinition EastOcean { get; } = new("EastOcean", AreaId.Crateria, new(49, 1), new(7, 6), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Wet };
    /// <summary>The Forgotten Highway Kagos room.</summary>
    public static RoomDefinition ForgottenHighwayKagos { get; } = new("ForgottenHighwayKagos", AreaId.Crateria, new(56, 5), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Wet };
    /// <summary>The Crab Maze room.</summary>
    public static RoomDefinition CrabMaze { get; } = new("CrabMaze", AreaId.Crateria, new(53, 9), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Wet };
    /// <summary>The Forgotten Highway Elbow room.</summary>
    public static RoomDefinition ForgottenHighwayElbow { get; } = new("ForgottenHighwayElbow", AreaId.Crateria, new(52, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Wet };
    /// <summary>The Crateria Tube room.</summary>
    public static RoomDefinition CrateriaTube { get; } = new("CrateriaTube", AreaId.Crateria, new(32, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Moat room.</summary>
    public static RoomDefinition Moat { get; } = new("Moat", AreaId.Crateria, new(36, 5), new(2, 2), 144, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Wet };
    /// <summary>The Red Brinstar Elevator room.</summary>
    public static RoomDefinition RedBrinstarElevator { get; } = new("RedBrinstarElevator", AreaId.Crateria, new(34, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Gauntlet Energy Tank room.</summary>
    public static RoomDefinition GauntletEnergyTank { get; } = new("GauntletEnergyTank", AreaId.Crateria, new(12, 3), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Pre Bowling room.</summary>
    public static RoomDefinition PreBowling { get; } = new("PreBowling", AreaId.Crateria, new(43, 3), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Climb room.</summary>
    public static RoomDefinition Climb { get; } = new("Climb", AreaId.Crateria, new(18, 10), new(3, 9), 112, 160, RoomDoorTransitionStyle.Standard) { QuakesAreSilent = true };
    /// <summary>The Pit room.</summary>
    public static RoomDefinition Pit { get; } = new("Pit", AreaId.Crateria, new(20, 18), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Elevator to Blue Brinstar room.</summary>
    public static RoomDefinition ElevatorToBlueBrinstar { get; } = new("ElevatorToBlueBrinstar", AreaId.Crateria, new(23, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Bomb Torizo room.</summary>
    public static RoomDefinition BombTorizo { get; } = new("BombTorizo", AreaId.Crateria, new(25, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard) { QuakesAreSilent = true };
    /// <summary>The Flyway room.</summary>
    public static RoomDefinition Flyway { get; } = new("Flyway", AreaId.Crateria, new(22, 7), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Pre Map Flyway room.</summary>
    public static RoomDefinition PreMapFlyway { get; } = new("PreMapFlyway", AreaId.Crateria, new(20, 8), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Terminator room.</summary>
    public static RoomDefinition Terminator { get; } = new("Terminator", AreaId.Crateria, new(12, 5), new(6, 3), 160, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Green Brinstar Elevator room.</summary>
    public static RoomDefinition GreenBrinstarElevator { get; } = new("GreenBrinstarElevator", AreaId.Crateria, new(6, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Lower Mushrooms room.</summary>
    public static RoomDefinition LowerMushrooms { get; } = new("LowerMushrooms", AreaId.Crateria, new(7, 9), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crateria Map room.</summary>
    public static RoomDefinition CrateriaMap { get; } = new("CrateriaMap", AreaId.Crateria, new(23, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Green Pirates Shaft room.</summary>
    public static RoomDefinition GreenPiratesShaft { get; } = new("GreenPiratesShaft", AreaId.Crateria, new(11, 3), new(1, 7), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Crateria Super room.</summary>
    public static RoomDefinition CrateriaSuper { get; } = new("CrateriaSuper", AreaId.Crateria, new(21, 10), new(4, 8), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Final Missile Bombway room.</summary>
    public static RoomDefinition FinalMissileBombway { get; } = new("FinalMissileBombway", AreaId.Crateria, new(17, 8), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Final Missile room.</summary>
    public static RoomDefinition FinalMissile { get; } = new("FinalMissile", AreaId.Crateria, new(16, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Green Brinstar Main Shaft room.</summary>
    public static RoomDefinition GreenBrinstarMainShaft { get; } = new("GreenBrinstarMainShaft", AreaId.Brinstar, new(9, 1), new(4, 12), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Spore Spawn Super room.</summary>
    public static RoomDefinition SporeSpawnSuper { get; } = new("SporeSpawnSuper", AreaId.Brinstar, new(23, 2), new(2, 9), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Brinstar Pre Map room.</summary>
    public static RoomDefinition BrinstarPreMap { get; } = new("BrinstarPreMap", AreaId.Brinstar, new(6, 5), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Early Supers room.</summary>
    public static RoomDefinition EarlySupers { get; } = new("EarlySupers", AreaId.Brinstar, new(10, 4), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Brinstar Reserve Tank room.</summary>
    public static RoomDefinition BrinstarReserveTank { get; } = new("BrinstarReserveTank", AreaId.Brinstar, new(13, 5), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Green Brinstar Map room.</summary>
    public static RoomDefinition GreenBrinstarMap { get; } = new("GreenBrinstarMap", AreaId.Brinstar, new(5, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Green Brinstar Firefleas room.</summary>
    public static RoomDefinition GreenBrinstarFirefleas { get; } = new("GreenBrinstarFirefleas", AreaId.Brinstar, new(6, 7), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Green Brinstar Missile Refill room.</summary>
    public static RoomDefinition GreenBrinstarMissileRefill { get; } = new("GreenBrinstarMissileRefill", AreaId.Brinstar, new(5, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Dachora room.</summary>
    public static RoomDefinition Dachora { get; } = new("Dachora", AreaId.Brinstar, new(10, 7), new(7, 7), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Big Pink room.</summary>
    public static RoomDefinition BigPink { get; } = new("BigPink", AreaId.Brinstar, new(15, 5), new(5, 10), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Spore Spawn Kihunters room.</summary>
    public static RoomDefinition SporeSpawnKihunters { get; } = new("SporeSpawnKihunters", AreaId.Brinstar, new(19, 5), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Spore Spawn room.</summary>
    public static RoomDefinition SporeSpawn { get; } = new("SporeSpawn", AreaId.Brinstar, new(22, 2), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Pink Brinstar Power Bombs room.</summary>
    public static RoomDefinition PinkBrinstarPowerBombs { get; } = new("PinkBrinstarPowerBombs", AreaId.Brinstar, new(15, 8), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Green Hill Zone room.</summary>
    public static RoomDefinition GreenHillZone { get; } = new("GreenHillZone", AreaId.Brinstar, new(19, 11), new(8, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Morph Ball room.</summary>
    public static RoomDefinition MorphBall { get; } = new("MorphBall", AreaId.Brinstar, new(21, 9), new(8, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Construction Zone room.</summary>
    public static RoomDefinition ConstructionZone { get; } = new("ConstructionZone", AreaId.Brinstar, new(29, 11), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Blue Brinstar Energy Tank room.</summary>
    public static RoomDefinition BlueBrinstarEnergyTank { get; } = new("BlueBrinstarEnergyTank", AreaId.Brinstar, new(30, 9), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Noob Bridge room.</summary>
    public static RoomDefinition NoobBridge { get; } = new("NoobBridge", AreaId.Brinstar, new(27, 14), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Green Brinstar Beetoms room.</summary>
    public static RoomDefinition GreenBrinstarBeetoms { get; } = new("GreenBrinstarBeetoms", AreaId.Brinstar, new(8, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Etecoon Energy Tank room.</summary>
    public static RoomDefinition EtecoonEnergyTank { get; } = new("EtecoonEnergyTank", AreaId.Brinstar, new(6, 11), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Etecoon Super room.</summary>
    public static RoomDefinition EtecoonSuper { get; } = new("EtecoonSuper", AreaId.Brinstar, new(5, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Dachora Energy Refill room.</summary>
    public static RoomDefinition DachoraEnergyRefill { get; } = new("DachoraEnergyRefill", AreaId.Brinstar, new(9, 13), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Spore Spawn Farming room.</summary>
    public static RoomDefinition SporeSpawnFarming { get; } = new("SporeSpawnFarming", AreaId.Brinstar, new(20, 10), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Waterway Energy Tank room.</summary>
    public static RoomDefinition WaterwayEnergyTank { get; } = new("WaterwayEnergyTank", AreaId.Brinstar, new(8, 14), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The First Missile room.</summary>
    public static RoomDefinition FirstMissile { get; } = new("FirstMissile", AreaId.Brinstar, new(28, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Pink Brinstar Hoppers room.</summary>
    public static RoomDefinition PinkBrinstarHoppers { get; } = new("PinkBrinstarHoppers", AreaId.Brinstar, new(19, 8), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Hopper Energy Tank room.</summary>
    public static RoomDefinition HopperEnergyTank { get; } = new("HopperEnergyTank", AreaId.Brinstar, new(21, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Big Pink Save Room.</summary>
    public static RoomDefinition BigPinkSaveRoom { get; } = new("BigPinkSaveRoom", AreaId.Brinstar, new(15, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Blue Brinstar Boulders room.</summary>
    public static RoomDefinition BlueBrinstarBoulders { get; } = new("BlueBrinstarBoulders", AreaId.Brinstar, new(30, 9), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Billy Mays room.</summary>
    public static RoomDefinition BillyMays { get; } = new("BillyMays", AreaId.Brinstar, new(29, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Green Brinstar Save room.</summary>
    public static RoomDefinition GreenBrinstarSave { get; } = new("GreenBrinstarSave", AreaId.Brinstar, new(8, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Etecoon Save room.</summary>
    public static RoomDefinition EtecoonSave { get; } = new("EtecoonSave", AreaId.Brinstar, new(5, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Red Tower room.</summary>
    public static RoomDefinition RedTower { get; } = new("RedTower", AreaId.Brinstar, new(33, 10), new(1, 10), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Red Brinstar Firefleas room.</summary>
    public static RoomDefinition RedBrinstarFirefleas { get; } = new("RedBrinstarFirefleas", AreaId.Brinstar, new(25, 16), new(8, 2), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The X-Ray Scope room.</summary>
    public static RoomDefinition XrayScope { get; } = new("XrayScope", AreaId.Brinstar, new(23, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Hellway room.</summary>
    public static RoomDefinition Hellway { get; } = new("Hellway", AreaId.Brinstar, new(34, 10), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Caterpillar room.</summary>
    public static RoomDefinition Caterpillar { get; } = new("Caterpillar", AreaId.Brinstar, new(37, 5), new(3, 8), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Beta Power Bomb room.</summary>
    public static RoomDefinition BetaPowerBomb { get; } = new("BetaPowerBomb", AreaId.Brinstar, new(35, 8), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Alpha Power Bomb room.</summary>
    public static RoomDefinition AlphaPowerBomb { get; } = new("AlphaPowerBomb", AreaId.Brinstar, new(34, 12), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Skree Boost room.</summary>
    public static RoomDefinition SkreeBoost { get; } = new("SkreeBoost", AreaId.Brinstar, new(34, 19), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Below Spazer room.</summary>
    public static RoomDefinition BelowSpazer { get; } = new("BelowSpazer", AreaId.Brinstar, new(36, 18), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Spazer room.</summary>
    public static RoomDefinition Spazer { get; } = new("Spazer", AreaId.Brinstar, new(38, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Warehouse Zeela room.</summary>
    public static RoomDefinition WarehouseZeela { get; } = new("WarehouseZeela", AreaId.Brinstar, new(44, 19), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Warehouse Energy Tank room.</summary>
    public static RoomDefinition WarehouseEnergyTank { get; } = new("WarehouseEnergyTank", AreaId.Brinstar, new(43, 20), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Warehouse Kihunter room.</summary>
    public static RoomDefinition WarehouseKihunter { get; } = new("WarehouseKihunter", AreaId.Brinstar, new(45, 19), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Mini Kraid room.</summary>
    public static RoomDefinition MiniKraid { get; } = new("MiniKraid", AreaId.Brinstar, new(47, 20), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Kraid Eye Door room.</summary>
    public static RoomDefinition KraidEyeDoor { get; } = new("KraidEyeDoor", AreaId.Brinstar, new(53, 19), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Kraid room.</summary>
    public static RoomDefinition Kraid { get; } = new("Kraid", AreaId.Brinstar, new(55, 19), new(2, 2), 112, 160, RoomDoorTransitionStyle.ObjectsOnly);
    /// <summary>The Statues Hallway room.</summary>
    public static RoomDefinition StatuesHallway { get; } = new("StatuesHallway", AreaId.Crateria, new(12, 9), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Red Tower Refill room.</summary>
    public static RoomDefinition RedTowerRefill { get; } = new("RedTowerRefill", AreaId.Brinstar, new(32, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Kraid Refill room.</summary>
    public static RoomDefinition KraidRefill { get; } = new("KraidRefill", AreaId.Brinstar, new(54, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Statues room.</summary>
    public static RoomDefinition Statues { get; } = new("Statues", AreaId.Crateria, new(17, 9), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard) { XrayRevealsNoBlocks = true };
    /// <summary>The Warehouse Entrance room.</summary>
    public static RoomDefinition WarehouseEntrance { get; } = new("WarehouseEntrance", AreaId.Brinstar, new(41, 19), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Varia Suit room.</summary>
    public static RoomDefinition VariaSuit { get; } = new("VariaSuit", AreaId.Brinstar, new(57, 20), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Warehouse Save room.</summary>
    public static RoomDefinition WarehouseSave { get; } = new("WarehouseSave", AreaId.Brinstar, new(49, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Red Brinstar Save room.</summary>
    public static RoomDefinition RedBrinstarSave { get; } = new("RedBrinstarSave", AreaId.Brinstar, new(38, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ice Beam Acid room.</summary>
    public static RoomDefinition IceBeamAcid { get; } = new("IceBeamAcid", AreaId.Norfair, new(4, 4), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Cathedral room.</summary>
    public static RoomDefinition Cathedral { get; } = new("Cathedral", AreaId.Norfair, new(14, 4), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Cathedral Entrance room.</summary>
    public static RoomDefinition CathedralEntrance { get; } = new("CathedralEntrance", AreaId.Norfair, new(11, 4), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Business Center room.</summary>
    public static RoomDefinition BusinessCenter { get; } = new("BusinessCenter", AreaId.Norfair, new(10, 1), new(1, 7), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ice Beam Gate room.</summary>
    public static RoomDefinition IceBeamGate { get; } = new("IceBeamGate", AreaId.Norfair, new(3, 2), new(7, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ice Beam Tutorial room.</summary>
    public static RoomDefinition IceBeamTutorial { get; } = new("IceBeamTutorial", AreaId.Norfair, new(4, 2), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ice Beam room.</summary>
    public static RoomDefinition IceBeam { get; } = new("IceBeam", AreaId.Norfair, new(5, 3), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ice Beam Snake room.</summary>
    public static RoomDefinition IceBeamSnake { get; } = new("IceBeamSnake", AreaId.Norfair, new(3, 2), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crumble Shaft room.</summary>
    public static RoomDefinition CrumbleShaft { get; } = new("CrumbleShaft", AreaId.Norfair, new(2, 5), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crocomire Speedway room.</summary>
    public static RoomDefinition CrocomireSpeedway { get; } = new("CrocomireSpeedway", AreaId.Norfair, new(3, 8), new(13, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crocomire room.</summary>
    public static RoomDefinition Crocomire { get; } = new("Crocomire", AreaId.Norfair, new(12, 11), new(8, 1), 112, 160, RoomDoorTransitionStyle.ObjectsOnly);
    /// <summary>The Hi-Jump Boots room.</summary>
    public static RoomDefinition HiJumpBoots { get; } = new("HiJumpBoots", AreaId.Norfair, new(7, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crocomire Escape room.</summary>
    public static RoomDefinition CrocomireEscape { get; } = new("CrocomireEscape", AreaId.Norfair, new(11, 7), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Hi-Jump Energy Tank room.</summary>
    public static RoomDefinition HiJumpEnergyTank { get; } = new("HiJumpEnergyTank", AreaId.Norfair, new(8, 6), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Post-Crocomire Farming room.</summary>
    public static RoomDefinition PostCrocomireFarming { get; } = new("PostCrocomireFarming", AreaId.Norfair, new(10, 11), new(2, 2), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Post-Crocomire Save room.</summary>
    public static RoomDefinition PostCrocomireSave { get; } = new("PostCrocomireSave", AreaId.Norfair, new(12, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Post-Crocomire Power Bombs room.</summary>
    public static RoomDefinition PostCrocomirePowerBombs { get; } = new("PostCrocomirePowerBombs", AreaId.Norfair, new(9, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Post-Crocomire Shaft room.</summary>
    public static RoomDefinition PostCrocomireShaft { get; } = new("PostCrocomireShaft", AreaId.Norfair, new(10, 13), new(1, 5), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Post-Crocomire Missile room.</summary>
    public static RoomDefinition PostCrocomireMissile { get; } = new("PostCrocomireMissile", AreaId.Norfair, new(11, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Grapple Tutorial 3 room.</summary>
    public static RoomDefinition GrappleTutorial3 { get; } = new("GrappleTutorial3", AreaId.Norfair, new(7, 13), new(3, 2), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Post-Crocomire Jump room.</summary>
    public static RoomDefinition PostCrocomireJump { get; } = new("PostCrocomireJump", AreaId.Norfair, new(4, 16), new(8, 3), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Grapple Tutorial 2 room.</summary>
    public static RoomDefinition GrappleTutorial2 { get; } = new("GrappleTutorial2", AreaId.Norfair, new(6, 13), new(1, 3), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Grapple Tutorial 1 room.</summary>
    public static RoomDefinition GrappleTutorial1 { get; } = new("GrappleTutorial1", AreaId.Norfair, new(4, 15), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Grapple Beam room.</summary>
    public static RoomDefinition GrappleBeam { get; } = new("GrappleBeam", AreaId.Norfair, new(3, 15), new(1, 4), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Norfair Reserve Tank room.</summary>
    public static RoomDefinition NorfairReserveTank { get; } = new("NorfairReserveTank", AreaId.Norfair, new(18, 3), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Green Bubbles Missiles room.</summary>
    public static RoomDefinition GreenBubblesMissiles { get; } = new("GreenBubblesMissiles", AreaId.Norfair, new(20, 3), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Bubble Mountain room.</summary>
    public static RoomDefinition BubbleMountain { get; } = new("BubbleMountain", AreaId.Norfair, new(22, 3), new(2, 4), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Speed Booster Hall room.</summary>
    public static RoomDefinition SpeedBoosterHall { get; } = new("SpeedBoosterHall", AreaId.Norfair, new(25, 2), new(12, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Speed Booster room.</summary>
    public static RoomDefinition SpeedBooster { get; } = new("SpeedBooster", AreaId.Norfair, new(37, 3), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Single Chamber room.</summary>
    public static RoomDefinition SingleChamber { get; } = new("SingleChamber", AreaId.Norfair, new(24, 4), new(6, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Double Chamber room.</summary>
    public static RoomDefinition DoubleChamber { get; } = new("DoubleChamber", AreaId.Norfair, new(25, 5), new(4, 2), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wave Beam room.</summary>
    public static RoomDefinition WaveBeam { get; } = new("WaveBeam", AreaId.Norfair, new(29, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Spiky Platforms Tunnel room.</summary>
    public static RoomDefinition SpikyPlatformsTunnel { get; } = new("SpikyPlatformsTunnel", AreaId.Norfair, new(25, 7), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Volcano room.</summary>
    public static RoomDefinition Volcano { get; } = new("Volcano", AreaId.Norfair, new(27, 7), new(3, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Kronic Boost room.</summary>
    public static RoomDefinition KronicBoost { get; } = new("KronicBoost", AreaId.Norfair, new(25, 9), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Magdollite Tunnel room.</summary>
    public static RoomDefinition MagdolliteTunnel { get; } = new("MagdolliteTunnel", AreaId.Norfair, new(23, 9), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Purple Shaft room.</summary>
    public static RoomDefinition PurpleShaft { get; } = new("PurpleShaft", AreaId.Norfair, new(22, 7), new(1, 3), 160, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Lava Dive room.</summary>
    public static RoomDefinition LavaDive { get; } = new("LavaDive", AreaId.Norfair, new(22, 11), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Lower Norfair Elevator room.</summary>
    public static RoomDefinition LowerNorfairElevator { get; } = new("LowerNorfairElevator", AreaId.Norfair, new(21, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Upper Norfair Farming room.</summary>
    public static RoomDefinition UpperNorfairFarming { get; } = new("UpperNorfairFarming", AreaId.Norfair, new(20, 6), new(2, 2), 160, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Rising Tide room.</summary>
    public static RoomDefinition RisingTide { get; } = new("RisingTide", AreaId.Norfair, new(17, 5), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Acid Snakes Tunnel room.</summary>
    public static RoomDefinition AcidSnakesTunnel { get; } = new("AcidSnakesTunnel", AreaId.Norfair, new(16, 10), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Spiky Acid Snakes Tunnel room.</summary>
    public static RoomDefinition SpikyAcidSnakesTunnel { get; } = new("SpikyAcidSnakesTunnel", AreaId.Norfair, new(21, 10), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Upper Norfair Refill room.</summary>
    public static RoomDefinition UpperNorfairRefill { get; } = new("UpperNorfairRefill", AreaId.Norfair, new(20, 10), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Purple Farming room.</summary>
    public static RoomDefinition PurpleFarming { get; } = new("PurpleFarming", AreaId.Norfair, new(23, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Bat Cave room.</summary>
    public static RoomDefinition BatCave { get; } = new("BatCave", AreaId.Norfair, new(24, 2), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Norfair Map room.</summary>
    public static RoomDefinition NorfairMap { get; } = new("NorfairMap", AreaId.Norfair, new(9, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Bubble Mountain Save room.</summary>
    public static RoomDefinition BubbleMountainSave { get; } = new("BubbleMountainSave", AreaId.Norfair, new(21, 4), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Frog Speedway room.</summary>
    public static RoomDefinition FrogSpeedway { get; } = new("FrogSpeedway", AreaId.Norfair, new(12, 6), new(8, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Red Pirate Shaft room.</summary>
    public static RoomDefinition RedPirateShaft { get; } = new("RedPirateShaft", AreaId.Norfair, new(19, 7), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Frog Save room.</summary>
    public static RoomDefinition FrogSave { get; } = new("FrogSave", AreaId.Norfair, new(11, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crocomire Save room.</summary>
    public static RoomDefinition CrocomireSave { get; } = new("CrocomireSave", AreaId.Norfair, new(16, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Lower Norfair Elevator Save room.</summary>
    public static RoomDefinition LowerNorfairElevatorSave { get; } = new("LowerNorfairElevatorSave", AreaId.Norfair, new(20, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Acid Statue room.</summary>
    public static RoomDefinition AcidStatue { get; } = new("AcidStatue", AreaId.Norfair, new(15, 14), new(3, 3), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Main Hall room.</summary>
    public static RoomDefinition MainHall { get; } = new("MainHall", AreaId.Norfair, new(17, 12), new(8, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Golden Torizo room.</summary>
    public static RoomDefinition GoldenTorizo { get; } = new("GoldenTorizo", AreaId.Norfair, new(18, 16), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Fast Ripper room.</summary>
    public static RoomDefinition FastRipper { get; } = new("FastRipper", AreaId.Norfair, new(21, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Golden Torizo Energy Refill room.</summary>
    public static RoomDefinition GoldenTorizoEnergyRefill { get; } = new("GoldenTorizoEnergyRefill", AreaId.Norfair, new(21, 16), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ridley room.</summary>
    public static RoomDefinition Ridley { get; } = new("Ridley", AreaId.Norfair, new(23, 17), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard) { QuakesAreSilent = true };
    /// <summary>The Lower Norfair Farming room.</summary>
    public static RoomDefinition LowerNorfairFarming { get; } = new("LowerNorfairFarming", AreaId.Norfair, new(24, 17), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Fast Pillars Setup room.</summary>
    public static RoomDefinition FastPillarsSetup { get; } = new("FastPillarsSetup", AreaId.Norfair, new(25, 13), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Mickey Mouse room.</summary>
    public static RoomDefinition MickeyMouse { get; } = new("MickeyMouse", AreaId.Norfair, new(26, 10), new(4, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Pillar room.</summary>
    public static RoomDefinition Pillar { get; } = new("Pillar", AreaId.Norfair, new(26, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard) { QuakesAreSilent = true };
    /// <summary>The Plowerhouse room.</summary>
    public static RoomDefinition Plowerhouse { get; } = new("Plowerhouse", AreaId.Norfair, new(27, 17), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Worst Room in the Game.</summary>
    public static RoomDefinition WorstRoomInTheGame { get; } = new("WorstRoomInTheGame", AreaId.Norfair, new(30, 10), new(1, 6), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Amphitheatre room.</summary>
    public static RoomDefinition Amphitheatre { get; } = new("Amphitheatre", AreaId.Norfair, new(31, 10), new(4, 5), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Lower Norfair Spring Ball Maze room.</summary>
    public static RoomDefinition LowerNorfairSpringBallMaze { get; } = new("LowerNorfairSpringBallMaze", AreaId.Norfair, new(33, 6), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Lower Norfair Escape Power Bombs room.</summary>
    public static RoomDefinition LowerNorfairEscapePowerBombs { get; } = new("LowerNorfairEscapePowerBombs", AreaId.Norfair, new(37, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Red Kihunter Shaft room.</summary>
    public static RoomDefinition RedKihunterShaft { get; } = new("RedKihunterShaft", AreaId.Norfair, new(35, 10), new(3, 5), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wasteland room.</summary>
    public static RoomDefinition Wasteland { get; } = new("Wasteland", AreaId.Norfair, new(32, 15), new(6, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Metal Pirates room.</summary>
    public static RoomDefinition MetalPirates { get; } = new("MetalPirates", AreaId.Norfair, new(30, 17), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Three Musketeers room.</summary>
    public static RoomDefinition ThreeMusketeers { get; } = new("ThreeMusketeers", AreaId.Norfair, new(29, 4), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ridley Energy Tank room.</summary>
    public static RoomDefinition RidleyEnergyTank { get; } = new("RidleyEnergyTank", AreaId.Norfair, new(22, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Screw Attack room.</summary>
    public static RoomDefinition ScrewAttack { get; } = new("ScrewAttack", AreaId.Norfair, new(20, 15), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Lower Norfair Fireflea room.</summary>
    public static RoomDefinition LowerNorfairFireflea { get; } = new("LowerNorfairFireflea", AreaId.Norfair, new(35, 7), new(3, 6), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Lower Norfair Save room.</summary>
    public static RoomDefinition LowerNorfairSave { get; } = new("LowerNorfairSave", AreaId.Norfair, new(36, 13), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Bowling Alley room.</summary>
    public static RoomDefinition BowlingAlley { get; } = new("BowlingAlley", AreaId.WreckedShip, new(10, 12), new(6, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wrecked Ship Entrance room.</summary>
    public static RoomDefinition WreckedShipEntrance { get; } = new("WreckedShipEntrance", AreaId.WreckedShip, new(12, 15), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Attic room.</summary>
    public static RoomDefinition Attic { get; } = new("Attic", AreaId.WreckedShip, new(12, 11), new(7, 1), 0, 0, RoomDoorTransitionStyle.Standard);
    /// <summary>Wrecked Ship's Assembly Line room.</summary>
    public static RoomDefinition AssemblyLine { get; } = new("AssemblyLine", AreaId.WreckedShip, new(19, 11), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wrecked Ship Main Shaft room.</summary>
    public static RoomDefinition WreckedShipMainShaft { get; } = new("WreckedShipMainShaft", AreaId.WreckedShip, new(12, 12), new(6, 8), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Spiky Death room.</summary>
    public static RoomDefinition SpikyDeath { get; } = new("SpikyDeath", AreaId.WreckedShip, new(19, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Electric Death room.</summary>
    public static RoomDefinition ElectricDeath { get; } = new("ElectricDeath", AreaId.WreckedShip, new(21, 14), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wrecked Ship Energy Tank room.</summary>
    public static RoomDefinition WreckedShipEnergyTank { get; } = new("WreckedShipEnergyTank", AreaId.WreckedShip, new(18, 14), new(3, 2), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Basement room.</summary>
    public static RoomDefinition Basement { get; } = new("Basement", AreaId.WreckedShip, new(14, 20), new(5, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wrecked Ship Map room.</summary>
    public static RoomDefinition WreckedShipMap { get; } = new("WreckedShipMap", AreaId.WreckedShip, new(13, 20), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Phantoon room.</summary>
    public static RoomDefinition Phantoon { get; } = new("Phantoon", AreaId.WreckedShip, new(19, 20), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Sponge Bath room.</summary>
    public static RoomDefinition SpongeBath { get; } = new("SpongeBath", AreaId.WreckedShip, new(17, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wrecked Ship West Super room.</summary>
    public static RoomDefinition WreckedShipWestSuper { get; } = new("WreckedShipWestSuper", AreaId.WreckedShip, new(15, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wrecked Ship East Super room.</summary>
    public static RoomDefinition WreckedShipEastSuper { get; } = new("WreckedShipEastSuper", AreaId.WreckedShip, new(18, 18), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Gravity Suit room.</summary>
    public static RoomDefinition GravitySuit { get; } = new("GravitySuit", AreaId.WreckedShip, new(10, 14), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Wrecked Ship Save room.</summary>
    public static RoomDefinition WreckedShipSave { get; } = new("WreckedShipSave", AreaId.WreckedShip, new(17, 15), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Glass Tunnel Save room.</summary>
    public static RoomDefinition GlassTunnelSave { get; } = new("GlassTunnelSave", AreaId.Maridia, new(12, 20), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Glass Tunnel room.</summary>
    public static RoomDefinition GlassTunnel { get; } = new("GlassTunnel", AreaId.Maridia, new(11, 18), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard) { XrayHidesBackground = true, XrayRevealsNoBlocks = true };
    /// <summary>The West Tunnel room.</summary>
    public static RoomDefinition WestTunnel { get; } = new("WestTunnel", AreaId.Maridia, new(10, 19), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The East Tunnel room.</summary>
    public static RoomDefinition EastTunnel { get; } = new("EastTunnel", AreaId.Maridia, new(12, 18), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Main Street room.</summary>
    public static RoomDefinition MainStreet { get; } = new("MainStreet", AreaId.Maridia, new(10, 10), new(3, 8), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Fish Tank room.</summary>
    public static RoomDefinition FishTank { get; } = new("FishTank", AreaId.Maridia, new(13, 14), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Mama Turtle room.</summary>
    public static RoomDefinition MamaTurtle { get; } = new("MamaTurtle", AreaId.Maridia, new(17, 13), new(3, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crab Tunnel room.</summary>
    public static RoomDefinition CrabTunnel { get; } = new("CrabTunnel", AreaId.Maridia, new(12, 17), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Mt. Everest room.</summary>
    public static RoomDefinition MtEverest { get; } = new("MtEverest", AreaId.Maridia, new(12, 10), new(6, 4), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Red Fish room.</summary>
    public static RoomDefinition RedFish { get; } = new("RedFish", AreaId.Maridia, new(12, 8), new(3, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Watering Hole room.</summary>
    public static RoomDefinition WateringHole { get; } = new("WateringHole", AreaId.Maridia, new(12, 5), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Northwest Maridia Bug room.</summary>
    public static RoomDefinition NorthwestMaridiaBug { get; } = new("NorthwestMaridiaBug", AreaId.Maridia, new(14, 5), new(4, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crab Shaft room.</summary>
    public static RoomDefinition CrabShaft { get; } = new("CrabShaft", AreaId.Maridia, new(18, 8), new(2, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Pseudo Plasma Spark room.</summary>
    public static RoomDefinition PseudoPlasmaSpark { get; } = new("PseudoPlasmaSpark", AreaId.Maridia, new(18, 5), new(4, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Crab Hole room.</summary>
    public static RoomDefinition CrabHole { get; } = new("CrabHole", AreaId.Maridia, new(16, 17), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The West Sand Hall Tunnel room.</summary>
    public static RoomDefinition WestSandHallTunnel { get; } = new("WestSandHallTunnel", AreaId.Maridia, new(17, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Plasma Tutorial room.</summary>
    public static RoomDefinition PlasmaTutorial { get; } = new("PlasmaTutorial", AreaId.Maridia, new(26, 1), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Plasma room.</summary>
    public static RoomDefinition Plasma { get; } = new("Plasma", AreaId.Maridia, new(27, 1), new(2, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Thread the Needle room.</summary>
    public static RoomDefinition ThreadTheNeedle { get; } = new("ThreadTheNeedle", AreaId.Maridia, new(27, 6), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Maridia Elevator room.</summary>
    public static RoomDefinition MaridiaElevator { get; } = new("MaridiaElevator", AreaId.Maridia, new(34, 1), new(1, 6), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Plasma Spark room.</summary>
    public static RoomDefinition PlasmaSpark { get; } = new("PlasmaSpark", AreaId.Maridia, new(22, 3), new(4, 6), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Kassiuz room.</summary>
    public static RoomDefinition Kassiuz { get; } = new("Kassiuz", AreaId.Maridia, new(25, 1), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Maridia Map room.</summary>
    public static RoomDefinition MaridiaMap { get; } = new("MaridiaMap", AreaId.Maridia, new(17, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Forgotten Highway Save room.</summary>
    public static RoomDefinition ForgottenHighwaySave { get; } = new("ForgottenHighwaySave", AreaId.Maridia, new(35, 5), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Toilet room.</summary>
    public static RoomDefinition Toilet { get; } = new("Toilet", AreaId.Maridia, new(22, 6), new(1, 10), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Bug Sand Hole room.</summary>
    public static RoomDefinition BugSandHole { get; } = new("BugSandHole", AreaId.Maridia, new(26, 6), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The West Sand Hall room.</summary>
    public static RoomDefinition WestSandHall { get; } = new("WestSandHall", AreaId.Maridia, new(18, 17), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Oasis room.</summary>
    public static RoomDefinition Oasis { get; } = new("Oasis", AreaId.Maridia, new(22, 16), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The East Sand Hall room.</summary>
    public static RoomDefinition EastSandHall { get; } = new("EastSandHall", AreaId.Maridia, new(23, 17), new(3, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The West Sand Hole room.</summary>
    public static RoomDefinition WestSandHole { get; } = new("WestSandHole", AreaId.Maridia, new(20, 15), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The East Sand Hole room.</summary>
    public static RoomDefinition EastSandHole { get; } = new("EastSandHole", AreaId.Maridia, new(23, 15), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The West Aqueduct Quicksand room.</summary>
    public static RoomDefinition WestAqueductQuicksand { get; } = new("WestAqueductQuicksand", AreaId.Maridia, new(21, 13), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The East Aqueduct Quicksand room.</summary>
    public static RoomDefinition EastAqueductQuicksand { get; } = new("EastAqueductQuicksand", AreaId.Maridia, new(23, 13), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Aqueduct room.</summary>
    public static RoomDefinition Aqueduct { get; } = new("Aqueduct", AreaId.Maridia, new(20, 10), new(6, 3), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Butterfly room.</summary>
    public static RoomDefinition Butterfly { get; } = new("Butterfly", AreaId.Maridia, new(26, 8), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Botwoon Hallway room.</summary>
    public static RoomDefinition BotwoonHallway { get; } = new("BotwoonHallway", AreaId.Maridia, new(20, 9), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Pants room.</summary>
    public static RoomDefinition Pants { get; } = new("Pants", AreaId.Maridia, new(26, 14), new(2, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The East Pants room.</summary>
    public static RoomDefinition EastPants { get; } = new("EastPants", AreaId.Maridia, new(27, 15), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Springball room.</summary>
    public static RoomDefinition Springball { get; } = new("Springball", AreaId.Maridia, new(32, 16), new(2, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Below Botwoon Energy Tank room.</summary>
    public static RoomDefinition BelowBotwoonEnergyTank { get; } = new("BelowBotwoonEnergyTank", AreaId.Maridia, new(26, 11), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Colosseum room.</summary>
    public static RoomDefinition Colosseum { get; } = new("Colosseum", AreaId.Maridia, new(34, 7), new(7, 2), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Aqueduct Save room.</summary>
    public static RoomDefinition AqueductSave { get; } = new("AqueductSave", AreaId.Maridia, new(19, 12), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Precious room.</summary>
    public static RoomDefinition ThePrecious { get; } = new("ThePrecious", AreaId.Maridia, new(41, 8), new(2, 3), 144, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Botwoon Energy Tank room.</summary>
    public static RoomDefinition BotwoonEnergyTank { get; } = new("BotwoonEnergyTank", AreaId.Maridia, new(26, 9), new(7, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Draygon Save room.</summary>
    public static RoomDefinition DraygonSave { get; } = new("DraygonSave", AreaId.Maridia, new(41, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Maridia Missile Refill room.</summary>
    public static RoomDefinition MaridiaMissileRefill { get; } = new("MaridiaMissileRefill", AreaId.Maridia, new(38, 9), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Plasma Beach Quicksand room.</summary>
    public static RoomDefinition PlasmaBeachQuicksand { get; } = new("PlasmaBeachQuicksand", AreaId.Maridia, new(26, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Botwoon Quicksand room.</summary>
    public static RoomDefinition BotwoonQuicksand { get; } = new("BotwoonQuicksand", AreaId.Maridia, new(28, 10), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Shaktool room.</summary>
    public static RoomDefinition Shaktool { get; } = new("Shaktool", AreaId.Maridia, new(28, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Halfie Climb room.</summary>
    public static RoomDefinition HalfieClimb { get; } = new("HalfieClimb", AreaId.Maridia, new(33, 7), new(5, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Botwoon room.</summary>
    public static RoomDefinition Botwoon { get; } = new("Botwoon", AreaId.Maridia, new(24, 9), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Space Jump room.</summary>
    public static RoomDefinition SpaceJump { get; } = new("SpaceJump", AreaId.Maridia, new(38, 11), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Maridia Energy Refill room.</summary>
    public static RoomDefinition MaridiaEnergyRefill { get; } = new("MaridiaEnergyRefill", AreaId.Maridia, new(42, 7), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The West Cactus Alley room.</summary>
    public static RoomDefinition WestCactusAlley { get; } = new("WestCactusAlley", AreaId.Maridia, new(27, 7), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The East Cactus Alley room.</summary>
    public static RoomDefinition EastCactusAlley { get; } = new("EastCactusAlley", AreaId.Maridia, new(28, 7), new(5, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Draygon room.</summary>
    public static RoomDefinition Draygon { get; } = new("Draygon", AreaId.Maridia, new(39, 10), new(2, 2), 112, 160, RoomDoorTransitionStyle.ObjectsOnly);
    /// <summary>The Tourian First room.</summary>
    public static RoomDefinition TourianFirst { get; } = new("TourianFirst", AreaId.Tourian, new(20, 10), new(1, 4), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Metroids 1 room.</summary>
    public static RoomDefinition Metroids1 { get; } = new("Metroids1", AreaId.Tourian, new(14, 13), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Metroids 2 room.</summary>
    public static RoomDefinition Metroids2 { get; } = new("Metroids2", AreaId.Tourian, new(13, 13), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Metroids 3 room.</summary>
    public static RoomDefinition Metroids3 { get; } = new("Metroids3", AreaId.Tourian, new(14, 14), new(6, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Metroids 4 room.</summary>
    public static RoomDefinition Metroids4 { get; } = new("Metroids4", AreaId.Tourian, new(20, 14), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Blue Hopper room.</summary>
    public static RoomDefinition BlueHopper { get; } = new("BlueHopper", AreaId.Tourian, new(19, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Dust Torizo room.</summary>
    public static RoomDefinition DustTorizo { get; } = new("DustTorizo", AreaId.Tourian, new(17, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Big Boy room.</summary>
    public static RoomDefinition BigBoy { get; } = new("BigBoy", AreaId.Tourian, new(13, 16), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Seaweed room.</summary>
    public static RoomDefinition Seaweed { get; } = new("Seaweed", AreaId.Tourian, new(12, 16), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Tourian Recharge room.</summary>
    public static RoomDefinition TourianRecharge { get; } = new("TourianRecharge", AreaId.Tourian, new(11, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Mother Brain room.</summary>
    public static RoomDefinition MotherBrain { get; } = new("MotherBrain", AreaId.Tourian, new(13, 19), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard) { QuakesAreSilent = true };
    /// <summary>The Tourian Eye Door room.</summary>
    public static RoomDefinition TourianEyeDoor { get; } = new("TourianEyeDoor", AreaId.Tourian, new(13, 17), new(4, 1), 112, 160, RoomDoorTransitionStyle.Standard) { Ground = RoomGroundDefinitions.Dusty };
    /// <summary>The Rinka Shaft room.</summary>
    public static RoomDefinition RinkaShaft { get; } = new("RinkaShaft", AreaId.Tourian, new(17, 17), new(1, 3), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Mother Brain Save room.</summary>
    public static RoomDefinition MotherBrainSave { get; } = new("MotherBrainSave", AreaId.Tourian, new(16, 18), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Tourian Escape 1 room.</summary>
    public static RoomDefinition TourianEscape1 { get; } = new("TourianEscape1", AreaId.Tourian, new(11, 19), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Tourian Escape 2 room.</summary>
    public static RoomDefinition TourianEscape2 { get; } = new("TourianEscape2", AreaId.Tourian, new(11, 20), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Tourian Escape 3 room.</summary>
    public static RoomDefinition TourianEscape3 { get; } = new("TourianEscape3", AreaId.Tourian, new(12, 20), new(6, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Tourian Escape 4 room.</summary>
    public static RoomDefinition TourianEscape4 { get; } = new("TourianEscape4", AreaId.Tourian, new(18, 17), new(3, 6), 112, 160, RoomDoorTransitionStyle.Standard) { QuakesAreSilent = true };
    /// <summary>The Upper Tourian Save room.</summary>
    public static RoomDefinition UpperTourianSave { get; } = new("UpperTourianSave", AreaId.Tourian, new(21, 13), new(1, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ceres Elevator room.</summary>
    public static RoomDefinition CeresElevator { get; } = new("CeresElevator", AreaId.Ceres, new(12, 14), new(1, 3), 112, 160, RoomDoorTransitionStyle.ObjectsOnly);
    /// <summary>The Falling Tile room.</summary>
    public static RoomDefinition FallingTile { get; } = new("FallingTile", AreaId.Ceres, new(13, 16), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Magnet Stairs room.</summary>
    public static RoomDefinition MagnetStairs { get; } = new("MagnetStairs", AreaId.Ceres, new(15, 16), new(1, 2), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Dead Scientist room.</summary>
    public static RoomDefinition DeadScientist { get; } = new("DeadScientist", AreaId.Ceres, new(16, 17), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The hallway between the Dead Scientist room and Ceres Ridley's room.</summary>
    public static RoomDefinition CeresRidleyHallway { get; } = new("CeresRidleyHallway", AreaId.Ceres, new(18, 17), new(2, 1), 112, 160, RoomDoorTransitionStyle.Standard);
    /// <summary>The Ceres Ridley room.</summary>
    public static RoomDefinition CeresRidley { get; } = new("CeresRidley", AreaId.Ceres, new(20, 17), new(1, 1), 112, 160, RoomDoorTransitionStyle.ObjectsOnly);

    /// <summary>Rooms in their authored order.</summary>
    internal static readonly RoomDefinition[] All =
    [
        LandingSite,
        GauntletEntrance,
        Parlor,
        CrateriaPowerBombs,
        CrateriaSave,
        WestOcean,
        BowlingAlleyPath,
        CrateriaKihunter,
        ForgottenHighwayElevator,
        EastOcean,
        ForgottenHighwayKagos,
        CrabMaze,
        ForgottenHighwayElbow,
        CrateriaTube,
        Moat,
        RedBrinstarElevator,
        GauntletEnergyTank,
        PreBowling,
        Climb,
        Pit,
        ElevatorToBlueBrinstar,
        BombTorizo,
        Flyway,
        PreMapFlyway,
        Terminator,
        GreenBrinstarElevator,
        LowerMushrooms,
        CrateriaMap,
        GreenPiratesShaft,
        CrateriaSuper,
        FinalMissileBombway,
        FinalMissile,
        GreenBrinstarMainShaft,
        SporeSpawnSuper,
        BrinstarPreMap,
        EarlySupers,
        BrinstarReserveTank,
        GreenBrinstarMap,
        GreenBrinstarFirefleas,
        GreenBrinstarMissileRefill,
        Dachora,
        BigPink,
        SporeSpawnKihunters,
        SporeSpawn,
        PinkBrinstarPowerBombs,
        GreenHillZone,
        MorphBall,
        ConstructionZone,
        BlueBrinstarEnergyTank,
        NoobBridge,
        GreenBrinstarBeetoms,
        EtecoonEnergyTank,
        EtecoonSuper,
        DachoraEnergyRefill,
        SporeSpawnFarming,
        WaterwayEnergyTank,
        FirstMissile,
        PinkBrinstarHoppers,
        HopperEnergyTank,
        BigPinkSaveRoom,
        BlueBrinstarBoulders,
        BillyMays,
        GreenBrinstarSave,
        EtecoonSave,
        RedTower,
        RedBrinstarFirefleas,
        XrayScope,
        Hellway,
        Caterpillar,
        BetaPowerBomb,
        AlphaPowerBomb,
        SkreeBoost,
        BelowSpazer,
        Spazer,
        WarehouseZeela,
        WarehouseEnergyTank,
        WarehouseKihunter,
        MiniKraid,
        KraidEyeDoor,
        Kraid,
        StatuesHallway,
        RedTowerRefill,
        KraidRefill,
        Statues,
        WarehouseEntrance,
        VariaSuit,
        WarehouseSave,
        RedBrinstarSave,
        IceBeamAcid,
        Cathedral,
        CathedralEntrance,
        BusinessCenter,
        IceBeamGate,
        IceBeamTutorial,
        IceBeam,
        IceBeamSnake,
        CrumbleShaft,
        CrocomireSpeedway,
        Crocomire,
        HiJumpBoots,
        CrocomireEscape,
        HiJumpEnergyTank,
        PostCrocomireFarming,
        PostCrocomireSave,
        PostCrocomirePowerBombs,
        PostCrocomireShaft,
        PostCrocomireMissile,
        GrappleTutorial3,
        PostCrocomireJump,
        GrappleTutorial2,
        GrappleTutorial1,
        GrappleBeam,
        NorfairReserveTank,
        GreenBubblesMissiles,
        BubbleMountain,
        SpeedBoosterHall,
        SpeedBooster,
        SingleChamber,
        DoubleChamber,
        WaveBeam,
        SpikyPlatformsTunnel,
        Volcano,
        KronicBoost,
        MagdolliteTunnel,
        PurpleShaft,
        LavaDive,
        LowerNorfairElevator,
        UpperNorfairFarming,
        RisingTide,
        AcidSnakesTunnel,
        SpikyAcidSnakesTunnel,
        UpperNorfairRefill,
        PurpleFarming,
        BatCave,
        NorfairMap,
        BubbleMountainSave,
        FrogSpeedway,
        RedPirateShaft,
        FrogSave,
        CrocomireSave,
        LowerNorfairElevatorSave,
        AcidStatue,
        MainHall,
        GoldenTorizo,
        FastRipper,
        GoldenTorizoEnergyRefill,
        Ridley,
        LowerNorfairFarming,
        FastPillarsSetup,
        MickeyMouse,
        Pillar,
        Plowerhouse,
        WorstRoomInTheGame,
        Amphitheatre,
        LowerNorfairSpringBallMaze,
        LowerNorfairEscapePowerBombs,
        RedKihunterShaft,
        Wasteland,
        MetalPirates,
        ThreeMusketeers,
        RidleyEnergyTank,
        ScrewAttack,
        LowerNorfairFireflea,
        LowerNorfairSave,
        BowlingAlley,
        WreckedShipEntrance,
        Attic,
        AssemblyLine,
        WreckedShipMainShaft,
        SpikyDeath,
        ElectricDeath,
        WreckedShipEnergyTank,
        Basement,
        WreckedShipMap,
        Phantoon,
        SpongeBath,
        WreckedShipWestSuper,
        WreckedShipEastSuper,
        GravitySuit,
        WreckedShipSave,
        GlassTunnelSave,
        GlassTunnel,
        WestTunnel,
        EastTunnel,
        MainStreet,
        FishTank,
        MamaTurtle,
        CrabTunnel,
        MtEverest,
        RedFish,
        WateringHole,
        NorthwestMaridiaBug,
        CrabShaft,
        PseudoPlasmaSpark,
        CrabHole,
        WestSandHallTunnel,
        PlasmaTutorial,
        Plasma,
        ThreadTheNeedle,
        MaridiaElevator,
        PlasmaSpark,
        Kassiuz,
        MaridiaMap,
        ForgottenHighwaySave,
        Toilet,
        BugSandHole,
        WestSandHall,
        Oasis,
        EastSandHall,
        WestSandHole,
        EastSandHole,
        WestAqueductQuicksand,
        EastAqueductQuicksand,
        Aqueduct,
        Butterfly,
        BotwoonHallway,
        Pants,
        EastPants,
        Springball,
        BelowBotwoonEnergyTank,
        Colosseum,
        AqueductSave,
        ThePrecious,
        BotwoonEnergyTank,
        DraygonSave,
        MaridiaMissileRefill,
        PlasmaBeachQuicksand,
        BotwoonQuicksand,
        Shaktool,
        HalfieClimb,
        Botwoon,
        SpaceJump,
        MaridiaEnergyRefill,
        WestCactusAlley,
        EastCactusAlley,
        Draygon,
        TourianFirst,
        Metroids1,
        Metroids2,
        Metroids3,
        Metroids4,
        BlueHopper,
        DustTorizo,
        BigBoy,
        Seaweed,
        TourianRecharge,
        MotherBrain,
        TourianEyeDoor,
        RinkaShaft,
        MotherBrainSave,
        TourianEscape1,
        TourianEscape2,
        TourianEscape3,
        TourianEscape4,
        UpperTourianSave,
        CeresElevator,
        FallingTile,
        MagnetStairs,
        DeadScientist,
        CeresRidleyHallway,
        CeresRidley,
    ];
}
