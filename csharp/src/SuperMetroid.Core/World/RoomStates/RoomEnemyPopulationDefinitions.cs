using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>Each room state's ordered enemy placements, grouped by the area of the rooms that load them.</summary>
/// <remarks>
/// The populations and their placements are the ones the room states select, which the import's check of the game's data
/// confirms. Placement order decides which place each linked enemy takes; each list states the kills that clear its room
/// as its death quota.
/// </remarks>
[NumberedIdentities("Each numbered member is the enemy population of a room whose own name carries the number (Grapple Tutorial Room 1-3, Metroid Room 1-4, Tourian Escape Room 1-4); nothing indexes the populations by number.")]
public static class RoomEnemyPopulationDefinitions
{
    /// <summary>The rooms' enemy populations in their authored list order, each named once, checked before gameplay can use them.</summary>
    internal static readonly RoomEnemyPopulationDefinition[] Definitions = RoomEnemyPopulations.Complete(
    [
        ..CrateriaRoomEnemyPopulationDefinitions.Lists,
        ..BrinstarRoomEnemyPopulationDefinitions.Lists,
        ..NorfairRoomEnemyPopulationDefinitions.Lists,
        ..WreckedShipRoomEnemyPopulationDefinitions.Lists,
        ..MaridiaRoomEnemyPopulationDefinitions.Lists,
        ..TourianRoomEnemyPopulationDefinitions.Lists,
        ..CeresRoomEnemyPopulationDefinitions.Lists,
    ]);

    /// <summary>The enemy population shared by the Draygon room and the Mother Brain room once each area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition DraygonOrMotherBrainWhenAreaBossDead => MaridiaRoomEnemyPopulationDefinitions.DraygonOrMotherBrainWhenAreaBossDead;
    /// <summary>The enemy population of the East Ocean room.</summary>
    public static RoomEnemyPopulationDefinition EastOcean => CrateriaRoomEnemyPopulationDefinitions.EastOcean;
    /// <summary>The enemy population of the Crateria Power Bombs room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaPowerBombs => CrateriaRoomEnemyPopulationDefinitions.CrateriaPowerBombs;
    /// <summary>The enemy population of the Terminator room.</summary>
    public static RoomEnemyPopulationDefinition Terminator => CrateriaRoomEnemyPopulationDefinitions.Terminator;
    /// <summary>The enemy population of the Final Missile Bombway room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileBombwayWhenZebesAwake => CrateriaRoomEnemyPopulationDefinitions.FinalMissileBombwayWhenZebesAwake;
    /// <summary>The enemy population of the Final Missile Bombway room in its default state.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileBombwayDefault => CrateriaRoomEnemyPopulationDefinitions.FinalMissileBombwayDefault;
    /// <summary>The enemy population of the Parlor room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition ParlorWhenZebesAwake => CrateriaRoomEnemyPopulationDefinitions.ParlorWhenZebesAwake;
    /// <summary>The enemy population of the Flyway room while the Zebes timebomb is not set.</summary>
    public static RoomEnemyPopulationDefinition FlywayBeforeTimebomb => CrateriaRoomEnemyPopulationDefinitions.FlywayBeforeTimebomb;
    /// <summary>The enemy population of the Pit room once Samus has the Morph Ball and Missiles.</summary>
    public static RoomEnemyPopulationDefinition PitWithMorphBallAndMissiles => CrateriaRoomEnemyPopulationDefinitions.PitWithMorphBallAndMissiles;
    /// <summary>The enemy population of the Gauntlet Entrance room.</summary>
    public static RoomEnemyPopulationDefinition GauntletEntrance => CrateriaRoomEnemyPopulationDefinitions.GauntletEntrance;
    /// <summary>The enemy population of the Bomb Torizo room while the Zebes timebomb is not set.</summary>
    public static RoomEnemyPopulationDefinition BombTorizoBeforeTimebomb => CrateriaRoomEnemyPopulationDefinitions.BombTorizoBeforeTimebomb;
    /// <summary>The enemy population of the Green Pirates Shaft room.</summary>
    public static RoomEnemyPopulationDefinition GreenPiratesShaft => CrateriaRoomEnemyPopulationDefinitions.GreenPiratesShaft;
    /// <summary>The enemy population of the Green Brinstar Elevator room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarElevator => CrateriaRoomEnemyPopulationDefinitions.GreenBrinstarElevator;
    /// <summary>The enemy population of the Final Missile room in its default state.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileDefault => CrateriaRoomEnemyPopulationDefinitions.FinalMissileDefault;
    /// <summary>The enemy population of the Crateria Map room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaMap => CrateriaRoomEnemyPopulationDefinitions.CrateriaMap;
    /// <summary>The enemy population of the Final Missile room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileWhenZebesAwake => CrateriaRoomEnemyPopulationDefinitions.FinalMissileWhenZebesAwake;
    /// <summary>The enemy population of the Climb room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition ClimbWhenZebesTimebombSet => CrateriaRoomEnemyPopulationDefinitions.ClimbWhenZebesTimebombSet;
    /// <summary>The enemy population of the Crateria Save room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaSave => CrateriaRoomEnemyPopulationDefinitions.CrateriaSave;
    /// <summary>The enemy population of the Forgotten Highway Elbow room.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayElbow => CrateriaRoomEnemyPopulationDefinitions.ForgottenHighwayElbow;
    /// <summary>The enemy population of the Moat room.</summary>
    public static RoomEnemyPopulationDefinition Moat => CrateriaRoomEnemyPopulationDefinitions.Moat;
    /// <summary>The enemy population of the Climb room in its default state.</summary>
    public static RoomEnemyPopulationDefinition ClimbDefault => CrateriaRoomEnemyPopulationDefinitions.ClimbDefault;
    /// <summary>The enemy population of the West Ocean room.</summary>
    public static RoomEnemyPopulationDefinition WestOcean => CrateriaRoomEnemyPopulationDefinitions.WestOcean;
    /// <summary>The enemy population of the Parlor room in its default state.</summary>
    public static RoomEnemyPopulationDefinition ParlorDefault => CrateriaRoomEnemyPopulationDefinitions.ParlorDefault;
    /// <summary>The enemy population of the Landing Site room while the Zebes timebomb is not set.</summary>
    public static RoomEnemyPopulationDefinition LandingSiteBeforeTimebomb => CrateriaRoomEnemyPopulationDefinitions.LandingSiteBeforeTimebomb;
    /// <summary>The enemy population of the Crateria Super room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaSuper => CrateriaRoomEnemyPopulationDefinitions.CrateriaSuper;
    /// <summary>The enemy population of the Pre Bowling room.</summary>
    public static RoomEnemyPopulationDefinition PreBowling => CrateriaRoomEnemyPopulationDefinitions.PreBowling;
    /// <summary>The enemy population of the Climb room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition ClimbWhenZebesAwake => CrateriaRoomEnemyPopulationDefinitions.ClimbWhenZebesAwake;
    /// <summary>The enemy population of the Bowling Alley Path room.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlleyPath => CrateriaRoomEnemyPopulationDefinitions.BowlingAlleyPath;
    /// <summary>The enemy population of the Red Brinstar Elevator room.</summary>
    public static RoomEnemyPopulationDefinition RedBrinstarElevator => CrateriaRoomEnemyPopulationDefinitions.RedBrinstarElevator;
    /// <summary>The enemy population of the Gauntlet Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition GauntletEnergyTank => CrateriaRoomEnemyPopulationDefinitions.GauntletEnergyTank;
    /// <summary>The enemy population of the Pit room in its default state.</summary>
    public static RoomEnemyPopulationDefinition PitDefault => CrateriaRoomEnemyPopulationDefinitions.PitDefault;
    /// <summary>The enemy population of the Crab Maze room.</summary>
    public static RoomEnemyPopulationDefinition CrabMaze => CrateriaRoomEnemyPopulationDefinitions.CrabMaze;
    /// <summary>The enemy population of the Crateria Tube room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaTube => CrateriaRoomEnemyPopulationDefinitions.CrateriaTube;
    /// <summary>The enemy population of the Forgotten Highway Kagos room.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayKagos => CrateriaRoomEnemyPopulationDefinitions.ForgottenHighwayKagos;
    /// <summary>The enemy population of the Elevator to Blue Brinstar room.</summary>
    public static RoomEnemyPopulationDefinition ElevatorToBlueBrinstar => CrateriaRoomEnemyPopulationDefinitions.ElevatorToBlueBrinstar;
    /// <summary>The enemy population of the Forgotten Highway Elevator room.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayElevator => CrateriaRoomEnemyPopulationDefinitions.ForgottenHighwayElevator;
    /// <summary>The enemy population of the Pre Map Flyway room.</summary>
    public static RoomEnemyPopulationDefinition PreMapFlyway => CrateriaRoomEnemyPopulationDefinitions.PreMapFlyway;
    /// <summary>The enemy population of the Lower Mushrooms room.</summary>
    public static RoomEnemyPopulationDefinition LowerMushrooms => CrateriaRoomEnemyPopulationDefinitions.LowerMushrooms;
    /// <summary>The enemy population of the Landing Site room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition LandingSiteWhenZebesTimebombSet => CrateriaRoomEnemyPopulationDefinitions.LandingSiteWhenZebesTimebombSet;
    /// <summary>The enemy population of the Parlor room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition ParlorWhenZebesTimebombSet => CrateriaRoomEnemyPopulationDefinitions.ParlorWhenZebesTimebombSet;
    /// <summary>The enemy population of the Bomb Torizo room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition BombTorizoWhenZebesTimebombSet => CrateriaRoomEnemyPopulationDefinitions.BombTorizoWhenZebesTimebombSet;
    /// <summary>The enemy population of the Flyway room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition FlywayWhenZebesTimebombSet => CrateriaRoomEnemyPopulationDefinitions.FlywayWhenZebesTimebombSet;
    /// <summary>The enemy population of the Crateria Kihunter room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaKihunter => CrateriaRoomEnemyPopulationDefinitions.CrateriaKihunter;
    /// <summary>The enemy population of the Brinstar Pre Map room.</summary>
    public static RoomEnemyPopulationDefinition BrinstarPreMap => BrinstarRoomEnemyPopulationDefinitions.BrinstarPreMap;
    /// <summary>The enemy population of the Green Brinstar Map room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMap => BrinstarRoomEnemyPopulationDefinitions.GreenBrinstarMap;
    /// <summary>The enemy population of the Green Brinstar Missile Refill room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMissileRefill => BrinstarRoomEnemyPopulationDefinitions.GreenBrinstarMissileRefill;
    /// <summary>The enemy population of the Spore Spawn Kihunters room.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnKihunters => BrinstarRoomEnemyPopulationDefinitions.SporeSpawnKihunters;
    /// <summary>The enemy population of the Etecoon Super room.</summary>
    public static RoomEnemyPopulationDefinition EtecoonSuper => BrinstarRoomEnemyPopulationDefinitions.EtecoonSuper;
    /// <summary>The enemy population of the Dachora Energy Refill room.</summary>
    public static RoomEnemyPopulationDefinition DachoraEnergyRefill => BrinstarRoomEnemyPopulationDefinitions.DachoraEnergyRefill;
    /// <summary>The enemy population of the Spore Spawn Farming room.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnFarming => BrinstarRoomEnemyPopulationDefinitions.SporeSpawnFarming;
    /// <summary>The enemy population of the Statues room.</summary>
    public static RoomEnemyPopulationDefinition Statues => CrateriaRoomEnemyPopulationDefinitions.Statues;
    /// <summary>The enemy population of the Hopper Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition HopperEnergyTank => BrinstarRoomEnemyPopulationDefinitions.HopperEnergyTank;
    /// <summary>The enemy population of the Beta Power Bomb room.</summary>
    public static RoomEnemyPopulationDefinition BetaPowerBomb => BrinstarRoomEnemyPopulationDefinitions.BetaPowerBomb;
    /// <summary>The enemy population of the Waterway Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition WaterwayEnergyTank => BrinstarRoomEnemyPopulationDefinitions.WaterwayEnergyTank;
    /// <summary>The enemy population of the Construction Zone room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition ConstructionZoneWhenZebesAwake => BrinstarRoomEnemyPopulationDefinitions.ConstructionZoneWhenZebesAwake;
    /// <summary>The enemy population of the Blue Brinstar Energy Tank room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarEnergyTankWhenZebesAwake => BrinstarRoomEnemyPopulationDefinitions.BlueBrinstarEnergyTankWhenZebesAwake;
    /// <summary>The enemy population of the Noob Bridge room.</summary>
    public static RoomEnemyPopulationDefinition NoobBridge => BrinstarRoomEnemyPopulationDefinitions.NoobBridge;
    /// <summary>The enemy population of the Morph Ball room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition MorphBallWhenZebesAwake => BrinstarRoomEnemyPopulationDefinitions.MorphBallWhenZebesAwake;
    /// <summary>The enemy population of the Warehouse Save room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseSave => BrinstarRoomEnemyPopulationDefinitions.WarehouseSave;
    /// <summary>The enemy population of the Morph Ball room in its default state.</summary>
    public static RoomEnemyPopulationDefinition MorphBallDefault => BrinstarRoomEnemyPopulationDefinitions.MorphBallDefault;
    /// <summary>The enemy population of the Warehouse Zeela room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseZeela => BrinstarRoomEnemyPopulationDefinitions.WarehouseZeela;
    /// <summary>The enemy population of the Red Tower room.</summary>
    public static RoomEnemyPopulationDefinition RedTower => BrinstarRoomEnemyPopulationDefinitions.RedTower;
    /// <summary>The enemy population of the Blue Brinstar Boulders room.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarBoulders => BrinstarRoomEnemyPopulationDefinitions.BlueBrinstarBoulders;
    /// <summary>The enemy population of the Billy Mays room.</summary>
    public static RoomEnemyPopulationDefinition BillyMays => BrinstarRoomEnemyPopulationDefinitions.BillyMays;
    /// <summary>The enemy population of the Big Pink Save Room.</summary>
    public static RoomEnemyPopulationDefinition BigPinkSaveRoom => BrinstarRoomEnemyPopulationDefinitions.BigPinkSaveRoom;
    /// <summary>The enemy population of the Big Pink room.</summary>
    public static RoomEnemyPopulationDefinition BigPink => BrinstarRoomEnemyPopulationDefinitions.BigPink;
    /// <summary>The enemy population of the Etecoon Save room.</summary>
    public static RoomEnemyPopulationDefinition EtecoonSave => BrinstarRoomEnemyPopulationDefinitions.EtecoonSave;
    /// <summary>The enemy population of the Below Spazer room.</summary>
    public static RoomEnemyPopulationDefinition BelowSpazer => BrinstarRoomEnemyPopulationDefinitions.BelowSpazer;
    /// <summary>The enemy population of the Green Brinstar Save room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarSave => BrinstarRoomEnemyPopulationDefinitions.GreenBrinstarSave;
    /// <summary>The enemy population of the Spazer room.</summary>
    public static RoomEnemyPopulationDefinition Spazer => BrinstarRoomEnemyPopulationDefinitions.Spazer;
    /// <summary>The enemy population of the Pink Brinstar Power Bombs room.</summary>
    public static RoomEnemyPopulationDefinition PinkBrinstarPowerBombs => BrinstarRoomEnemyPopulationDefinitions.PinkBrinstarPowerBombs;
    /// <summary>The enemy population of the Red Tower Refill room.</summary>
    public static RoomEnemyPopulationDefinition RedTowerRefill => BrinstarRoomEnemyPopulationDefinitions.RedTowerRefill;
    /// <summary>The enemy population of the Kraid Refill room.</summary>
    public static RoomEnemyPopulationDefinition KraidRefill => BrinstarRoomEnemyPopulationDefinitions.KraidRefill;
    /// <summary>The enemy population of the Varia Suit room.</summary>
    public static RoomEnemyPopulationDefinition VariaSuit => BrinstarRoomEnemyPopulationDefinitions.VariaSuit;
    /// <summary>The enemy population of the Red Brinstar Save room.</summary>
    public static RoomEnemyPopulationDefinition RedBrinstarSave => BrinstarRoomEnemyPopulationDefinitions.RedBrinstarSave;
    /// <summary>The enemy population of the Blue Brinstar Energy Tank room in its default state.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarEnergyTankDefault => BrinstarRoomEnemyPopulationDefinitions.BlueBrinstarEnergyTankDefault;
    /// <summary>The enemy population of the Green Brinstar Firefleas room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarFirefleas => BrinstarRoomEnemyPopulationDefinitions.GreenBrinstarFirefleas;
    /// <summary>The enemy population of the Green Brinstar Beetoms room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarBeetoms => BrinstarRoomEnemyPopulationDefinitions.GreenBrinstarBeetoms;
    /// <summary>The enemy population of the Etecoon Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition EtecoonEnergyTank => BrinstarRoomEnemyPopulationDefinitions.EtecoonEnergyTank;
    /// <summary>The enemy population of the Pink Brinstar Hoppers room.</summary>
    public static RoomEnemyPopulationDefinition PinkBrinstarHoppers => BrinstarRoomEnemyPopulationDefinitions.PinkBrinstarHoppers;
    /// <summary>The enemy population of the Warehouse Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseEnergyTank => BrinstarRoomEnemyPopulationDefinitions.WarehouseEnergyTank;
    /// <summary>The enemy population of the Warehouse Entrance room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseEntrance => BrinstarRoomEnemyPopulationDefinitions.WarehouseEntrance;
    /// <summary>The enemy population of the Warehouse Kihunter room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseKihunter => BrinstarRoomEnemyPopulationDefinitions.WarehouseKihunter;
    /// <summary>The enemy population of the Green Brinstar Main Shaft room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMainShaft => BrinstarRoomEnemyPopulationDefinitions.GreenBrinstarMainShaft;
    /// <summary>The enemy population of the Spore Spawn Super room.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnSuper => BrinstarRoomEnemyPopulationDefinitions.SporeSpawnSuper;
    /// <summary>The enemy population of the Early Supers room.</summary>
    public static RoomEnemyPopulationDefinition EarlySupers => BrinstarRoomEnemyPopulationDefinitions.EarlySupers;
    /// <summary>The enemy population of the Red Brinstar Firefleas room.</summary>
    public static RoomEnemyPopulationDefinition RedBrinstarFirefleas => BrinstarRoomEnemyPopulationDefinitions.RedBrinstarFirefleas;
    /// <summary>The enemy population of the Construction Zone room in its default state.</summary>
    public static RoomEnemyPopulationDefinition ConstructionZoneDefault => BrinstarRoomEnemyPopulationDefinitions.ConstructionZoneDefault;
    /// <summary>The enemy population of the Green Hill Zone room.</summary>
    public static RoomEnemyPopulationDefinition GreenHillZone => BrinstarRoomEnemyPopulationDefinitions.GreenHillZone;
    /// <summary>The enemy population of the Dachora room.</summary>
    public static RoomEnemyPopulationDefinition Dachora => BrinstarRoomEnemyPopulationDefinitions.Dachora;
    /// <summary>The enemy population of the Hellway room.</summary>
    public static RoomEnemyPopulationDefinition Hellway => BrinstarRoomEnemyPopulationDefinitions.Hellway;
    /// <summary>The enemy population of the First Missile room.</summary>
    public static RoomEnemyPopulationDefinition FirstMissile => BrinstarRoomEnemyPopulationDefinitions.FirstMissile;
    /// <summary>The enemy population of the Kraid room.</summary>
    public static RoomEnemyPopulationDefinition Kraid => BrinstarRoomEnemyPopulationDefinitions.Kraid;
    /// <summary>The enemy population of the Brinstar Reserve Tank room.</summary>
    public static RoomEnemyPopulationDefinition BrinstarReserveTank => BrinstarRoomEnemyPopulationDefinitions.BrinstarReserveTank;
    /// <summary>The enemy population of the X-Ray Scope room.</summary>
    public static RoomEnemyPopulationDefinition XrayScope => BrinstarRoomEnemyPopulationDefinitions.XrayScope;
    /// <summary>The enemy population of the Statues Hallway room.</summary>
    public static RoomEnemyPopulationDefinition StatuesHallway => CrateriaRoomEnemyPopulationDefinitions.StatuesHallway;
    /// <summary>The enemy population of the Alpha Power Bomb room.</summary>
    public static RoomEnemyPopulationDefinition AlphaPowerBomb => BrinstarRoomEnemyPopulationDefinitions.AlphaPowerBomb;
    /// <summary>The enemy population of the Kraid Eye Door room.</summary>
    public static RoomEnemyPopulationDefinition KraidEyeDoor => BrinstarRoomEnemyPopulationDefinitions.KraidEyeDoor;
    /// <summary>The enemy population of the Caterpillar room.</summary>
    public static RoomEnemyPopulationDefinition Caterpillar => BrinstarRoomEnemyPopulationDefinitions.Caterpillar;
    /// <summary>The enemy population of the Mini Kraid room.</summary>
    public static RoomEnemyPopulationDefinition MiniKraid => BrinstarRoomEnemyPopulationDefinitions.MiniKraid;
    /// <summary>The enemy population of the Spore Spawn room.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawn => BrinstarRoomEnemyPopulationDefinitions.SporeSpawn;
    /// <summary>The enemy population of the Skree Boost room.</summary>
    public static RoomEnemyPopulationDefinition SkreeBoost => BrinstarRoomEnemyPopulationDefinitions.SkreeBoost;
    /// <summary>The enemy population of the Grapple Tutorial 3 room.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial3 => NorfairRoomEnemyPopulationDefinitions.GrappleTutorial3;
    /// <summary>The enemy population of the Post-Crocomire Shaft room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocomireShaft => NorfairRoomEnemyPopulationDefinitions.PostCrocomireShaft;
    /// <summary>The enemy population of the Metal Pirates room.</summary>
    public static RoomEnemyPopulationDefinition MetalPirates => NorfairRoomEnemyPopulationDefinitions.MetalPirates;
    /// <summary>The enemy population of the Acid Statue room.</summary>
    public static RoomEnemyPopulationDefinition AcidStatue => NorfairRoomEnemyPopulationDefinitions.AcidStatue;
    /// <summary>The enemy population of the Purple Farming room.</summary>
    public static RoomEnemyPopulationDefinition PurpleFarming => NorfairRoomEnemyPopulationDefinitions.PurpleFarming;
    /// <summary>The enemy population of the Ice Beam Gate room.</summary>
    public static RoomEnemyPopulationDefinition IceBeamGate => NorfairRoomEnemyPopulationDefinitions.IceBeamGate;
    /// <summary>The enemy population of the Spiky Acid Snakes Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition SpikyAcidSnakesTunnel => NorfairRoomEnemyPopulationDefinitions.SpikyAcidSnakesTunnel;
    /// <summary>The enemy population of the Red Kihunter Shaft room.</summary>
    public static RoomEnemyPopulationDefinition RedKihunterShaft => NorfairRoomEnemyPopulationDefinitions.RedKihunterShaft;
    /// <summary>The enemy population of the Fast Ripper room.</summary>
    public static RoomEnemyPopulationDefinition FastRipper => NorfairRoomEnemyPopulationDefinitions.FastRipper;
    /// <summary>The enemy population of the Wave Beam room.</summary>
    public static RoomEnemyPopulationDefinition WaveBeam => NorfairRoomEnemyPopulationDefinitions.WaveBeam;
    /// <summary>The enemy population of the Fast Pillars Setup room.</summary>
    public static RoomEnemyPopulationDefinition FastPillarsSetup => NorfairRoomEnemyPopulationDefinitions.FastPillarsSetup;
    /// <summary>The enemy population of the Lower Norfair Elevator room.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairElevator => NorfairRoomEnemyPopulationDefinitions.LowerNorfairElevator;
    /// <summary>The enemy population of the Lower Norfair Save room.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairSave => NorfairRoomEnemyPopulationDefinitions.LowerNorfairSave;
    /// <summary>The enemy population of the Upper Norfair Refill room.</summary>
    public static RoomEnemyPopulationDefinition UpperNorfairRefill => NorfairRoomEnemyPopulationDefinitions.UpperNorfairRefill;
    /// <summary>The enemy population of the Norfair Map room.</summary>
    public static RoomEnemyPopulationDefinition NorfairMap => NorfairRoomEnemyPopulationDefinitions.NorfairMap;
    /// <summary>The enemy population of the Mickey Mouse room.</summary>
    public static RoomEnemyPopulationDefinition MickeyMouse => NorfairRoomEnemyPopulationDefinitions.MickeyMouse;
    /// <summary>The enemy population of the Golden Torizo Energy Refill room.</summary>
    public static RoomEnemyPopulationDefinition GoldenTorizoEnergyRefill => NorfairRoomEnemyPopulationDefinitions.GoldenTorizoEnergyRefill;
    /// <summary>The enemy population of the Ridley room.</summary>
    public static RoomEnemyPopulationDefinition Ridley => NorfairRoomEnemyPopulationDefinitions.Ridley;
    /// <summary>The enemy population of the Post-Crocomire Power Bombs room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocomirePowerBombs => NorfairRoomEnemyPopulationDefinitions.PostCrocomirePowerBombs;
    /// <summary>The enemy population of the Ice Beam room.</summary>
    public static RoomEnemyPopulationDefinition IceBeam => NorfairRoomEnemyPopulationDefinitions.IceBeam;
    /// <summary>The enemy population of the Hi-Jump Boots room.</summary>
    public static RoomEnemyPopulationDefinition HiJumpBoots => NorfairRoomEnemyPopulationDefinitions.HiJumpBoots;
    /// <summary>The enemy population of the Post-Crocomire Save room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocomireSave => NorfairRoomEnemyPopulationDefinitions.PostCrocomireSave;
    /// <summary>The enemy population of the Frog Speedway room.</summary>
    public static RoomEnemyPopulationDefinition FrogSpeedway => NorfairRoomEnemyPopulationDefinitions.FrogSpeedway;
    /// <summary>The enemy population of the Rising Tide room.</summary>
    public static RoomEnemyPopulationDefinition RisingTide => NorfairRoomEnemyPopulationDefinitions.RisingTide;
    /// <summary>The enemy population of the Post-Crocomire Farming room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocomireFarming => NorfairRoomEnemyPopulationDefinitions.PostCrocomireFarming;
    /// <summary>The enemy population of the Worst Room in the Game.</summary>
    public static RoomEnemyPopulationDefinition WorstRoomInTheGame => NorfairRoomEnemyPopulationDefinitions.WorstRoomInTheGame;
    /// <summary>The enemy population of the Double Chamber room.</summary>
    public static RoomEnemyPopulationDefinition DoubleChamber => NorfairRoomEnemyPopulationDefinitions.DoubleChamber;
    /// <summary>The enemy population of the Grapple Beam room.</summary>
    public static RoomEnemyPopulationDefinition GrappleBeam => NorfairRoomEnemyPopulationDefinitions.GrappleBeam;
    /// <summary>The enemy population of the Red Pirate Shaft room.</summary>
    public static RoomEnemyPopulationDefinition RedPirateShaft => NorfairRoomEnemyPopulationDefinitions.RedPirateShaft;
    /// <summary>The enemy population of the Crocomire Speedway room.</summary>
    public static RoomEnemyPopulationDefinition CrocomireSpeedway => NorfairRoomEnemyPopulationDefinitions.CrocomireSpeedway;
    /// <summary>The enemy population of the Plowerhouse room.</summary>
    public static RoomEnemyPopulationDefinition Plowerhouse => NorfairRoomEnemyPopulationDefinitions.Plowerhouse;
    /// <summary>The enemy population of the Lower Norfair Fireflea room.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairFireflea => NorfairRoomEnemyPopulationDefinitions.LowerNorfairFireflea;
    /// <summary>The enemy population of the Magdollite Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition MagdolliteTunnel => NorfairRoomEnemyPopulationDefinitions.MagdolliteTunnel;
    /// <summary>The enemy population of the Lava Dive room.</summary>
    public static RoomEnemyPopulationDefinition LavaDive => NorfairRoomEnemyPopulationDefinitions.LavaDive;
    /// <summary>The enemy population of the Pillar room.</summary>
    public static RoomEnemyPopulationDefinition Pillar => NorfairRoomEnemyPopulationDefinitions.Pillar;
    /// <summary>The enemy population of the Crocomire Escape room.</summary>
    public static RoomEnemyPopulationDefinition CrocomireEscape => NorfairRoomEnemyPopulationDefinitions.CrocomireEscape;
    /// <summary>The enemy population of the Lower Norfair Spring Ball Maze room.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairSpringBallMaze => NorfairRoomEnemyPopulationDefinitions.LowerNorfairSpringBallMaze;
    /// <summary>The enemy population of the Lower Norfair Escape Power Bombs room.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairEscapePowerBombs => NorfairRoomEnemyPopulationDefinitions.LowerNorfairEscapePowerBombs;
    /// <summary>The enemy population of the Purple Shaft room.</summary>
    public static RoomEnemyPopulationDefinition PurpleShaft => NorfairRoomEnemyPopulationDefinitions.PurpleShaft;
    /// <summary>The enemy population of the Ridley Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition RidleyEnergyTank => NorfairRoomEnemyPopulationDefinitions.RidleyEnergyTank;
    /// <summary>The enemy population of the Screw Attack room.</summary>
    public static RoomEnemyPopulationDefinition ScrewAttack => NorfairRoomEnemyPopulationDefinitions.ScrewAttack;
    /// <summary>The enemy population of the Green Bubbles Missiles room.</summary>
    public static RoomEnemyPopulationDefinition GreenBubblesMissiles => NorfairRoomEnemyPopulationDefinitions.GreenBubblesMissiles;
    /// <summary>The enemy population of the Amphitheatre room.</summary>
    public static RoomEnemyPopulationDefinition Amphitheatre => NorfairRoomEnemyPopulationDefinitions.Amphitheatre;
    /// <summary>The enemy population of the Cathedral Entrance room.</summary>
    public static RoomEnemyPopulationDefinition CathedralEntrance => NorfairRoomEnemyPopulationDefinitions.CathedralEntrance;
    /// <summary>The enemy population of the Main Hall room.</summary>
    public static RoomEnemyPopulationDefinition MainHall => NorfairRoomEnemyPopulationDefinitions.MainHall;
    /// <summary>The enemy population of the Post-Crocomire Jump room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocomireJump => NorfairRoomEnemyPopulationDefinitions.PostCrocomireJump;
    /// <summary>The enemy population of the Frog Save room.</summary>
    public static RoomEnemyPopulationDefinition FrogSave => NorfairRoomEnemyPopulationDefinitions.FrogSave;
    /// <summary>The enemy population of the Crocomire Save room.</summary>
    public static RoomEnemyPopulationDefinition CrocomireSave => NorfairRoomEnemyPopulationDefinitions.CrocomireSave;
    /// <summary>The enemy population of the Crumble Shaft room.</summary>
    public static RoomEnemyPopulationDefinition CrumbleShaft => NorfairRoomEnemyPopulationDefinitions.CrumbleShaft;
    /// <summary>The enemy population of the Wasteland room.</summary>
    public static RoomEnemyPopulationDefinition Wasteland => NorfairRoomEnemyPopulationDefinitions.Wasteland;
    /// <summary>The enemy population of the Acid Snakes Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition AcidSnakesTunnel => NorfairRoomEnemyPopulationDefinitions.AcidSnakesTunnel;
    /// <summary>The enemy population of the Grapple Tutorial 1 room.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial1 => NorfairRoomEnemyPopulationDefinitions.GrappleTutorial1;
    /// <summary>The enemy population of the Bubble Mountain Save room.</summary>
    public static RoomEnemyPopulationDefinition BubbleMountainSave => NorfairRoomEnemyPopulationDefinitions.BubbleMountainSave;
    /// <summary>The enemy population of the Hi-Jump Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition HiJumpEnergyTank => NorfairRoomEnemyPopulationDefinitions.HiJumpEnergyTank;
    /// <summary>The enemy population of the Bubble Mountain room.</summary>
    public static RoomEnemyPopulationDefinition BubbleMountain => NorfairRoomEnemyPopulationDefinitions.BubbleMountain;
    /// <summary>The enemy population of the Ice Beam Acid room.</summary>
    public static RoomEnemyPopulationDefinition IceBeamAcid => NorfairRoomEnemyPopulationDefinitions.IceBeamAcid;
    /// <summary>The enemy population of the Ice Beam Tutorial room.</summary>
    public static RoomEnemyPopulationDefinition IceBeamTutorial => NorfairRoomEnemyPopulationDefinitions.IceBeamTutorial;
    /// <summary>The enemy population of the Post-Crocomire Missile room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocomireMissile => NorfairRoomEnemyPopulationDefinitions.PostCrocomireMissile;
    /// <summary>The enemy population of the Cathedral room.</summary>
    public static RoomEnemyPopulationDefinition Cathedral => NorfairRoomEnemyPopulationDefinitions.Cathedral;
    /// <summary>The enemy population of the Norfair Reserve Tank room.</summary>
    public static RoomEnemyPopulationDefinition NorfairReserveTank => NorfairRoomEnemyPopulationDefinitions.NorfairReserveTank;
    /// <summary>The enemy population of the Grapple Tutorial 2 room.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial2 => NorfairRoomEnemyPopulationDefinitions.GrappleTutorial2;
    /// <summary>The enemy population of the Ice Beam Snake room.</summary>
    public static RoomEnemyPopulationDefinition IceBeamSnake => NorfairRoomEnemyPopulationDefinitions.IceBeamSnake;
    /// <summary>The enemy population of the Golden Torizo room.</summary>
    public static RoomEnemyPopulationDefinition GoldenTorizo => NorfairRoomEnemyPopulationDefinitions.GoldenTorizo;
    /// <summary>The enemy population of the Spiky Platforms Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition SpikyPlatformsTunnel => NorfairRoomEnemyPopulationDefinitions.SpikyPlatformsTunnel;
    /// <summary>The enemy population of the Speed Booster room.</summary>
    public static RoomEnemyPopulationDefinition SpeedBooster => NorfairRoomEnemyPopulationDefinitions.SpeedBooster;
    /// <summary>The enemy population of the Lower Norfair Farming room.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairFarming => NorfairRoomEnemyPopulationDefinitions.LowerNorfairFarming;
    /// <summary>The enemy population of the Single Chamber room.</summary>
    public static RoomEnemyPopulationDefinition SingleChamber => NorfairRoomEnemyPopulationDefinitions.SingleChamber;
    /// <summary>The enemy population of the Speed Booster Hall room.</summary>
    public static RoomEnemyPopulationDefinition SpeedBoosterHall => NorfairRoomEnemyPopulationDefinitions.SpeedBoosterHall;
    /// <summary>The enemy population of the Bat Cave room.</summary>
    public static RoomEnemyPopulationDefinition BatCave => NorfairRoomEnemyPopulationDefinitions.BatCave;
    /// <summary>The enemy population of the Kronic Boost room.</summary>
    public static RoomEnemyPopulationDefinition KronicBoost => NorfairRoomEnemyPopulationDefinitions.KronicBoost;
    /// <summary>The enemy population of the Business Center room.</summary>
    public static RoomEnemyPopulationDefinition BusinessCenter => NorfairRoomEnemyPopulationDefinitions.BusinessCenter;
    /// <summary>The enemy population of the Three Musketeers room.</summary>
    public static RoomEnemyPopulationDefinition ThreeMusketeers => NorfairRoomEnemyPopulationDefinitions.ThreeMusketeers;
    /// <summary>The enemy population of the Crocomire room.</summary>
    public static RoomEnemyPopulationDefinition Crocomire => NorfairRoomEnemyPopulationDefinitions.Crocomire;
    /// <summary>The enemy population of the Lower Norfair Elevator Save room.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairElevatorSave => NorfairRoomEnemyPopulationDefinitions.LowerNorfairElevatorSave;
    /// <summary>The enemy population of the Volcano room.</summary>
    public static RoomEnemyPopulationDefinition Volcano => NorfairRoomEnemyPopulationDefinitions.Volcano;
    /// <summary>The enemy population of the Upper Norfair Farming room.</summary>
    public static RoomEnemyPopulationDefinition UpperNorfairFarming => NorfairRoomEnemyPopulationDefinitions.UpperNorfairFarming;
    /// <summary>The enemy population of the Sponge Bath room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition SpongeBathWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.SpongeBathWhenAreaBossDead;
    /// <summary>The enemy population of the Wrecked Ship Energy Tank room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEnergyTankWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipEnergyTankWhenAreaBossDead;
    /// <summary>The enemy population of the Wrecked Ship Main Shaft room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMainShaftDefault => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipMainShaftDefault;
    /// <summary>The enemy population of the Bowling Alley room in its default state.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlleyDefault => WreckedShipRoomEnemyPopulationDefinitions.BowlingAlleyDefault;
    /// <summary>The enemy population of the Attic room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition AtticWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.AtticWhenAreaBossDead;
    /// <summary>The enemy population of the Basement room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition BasementWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.BasementWhenAreaBossDead;
    /// <summary>The enemy population of the Wrecked Ship Map room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMapWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipMapWhenAreaBossDead;
    /// <summary>The enemy population of the Wrecked Ship West Super room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipWestSuperDefault => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipWestSuperDefault;
    /// <summary>The enemy population of the Spiky Death room in its default state.</summary>
    public static RoomEnemyPopulationDefinition SpikyDeathDefault => WreckedShipRoomEnemyPopulationDefinitions.SpikyDeathDefault;
    /// <summary>The enemy population of the Sponge Bath room in its default state.</summary>
    public static RoomEnemyPopulationDefinition SpongeBathDefault => WreckedShipRoomEnemyPopulationDefinitions.SpongeBathDefault;
    /// <summary>The enemy population of the Wrecked Ship Entrance room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEntranceWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipEntranceWhenAreaBossDead;
    /// <summary>The enemy population of the Electric Death room in its default state.</summary>
    public static RoomEnemyPopulationDefinition ElectricDeathDefault => WreckedShipRoomEnemyPopulationDefinitions.ElectricDeathDefault;
    /// <summary>The enemy population of the Bowling Alley room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlleyWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.BowlingAlleyWhenAreaBossDead;
    /// <summary>The enemy population of Wrecked Ship's Assembly Line room in its default state.</summary>
    public static RoomEnemyPopulationDefinition AssemblyLineDefault => WreckedShipRoomEnemyPopulationDefinitions.AssemblyLineDefault;
    /// <summary>The enemy population of the Phantoon room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition PhantoonWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.PhantoonWhenAreaBossDead;
    /// <summary>The enemy population of the Wrecked Ship Energy Tank room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEnergyTankDefault => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipEnergyTankDefault;
    /// <summary>The enemy population of the Gravity Suit room in its default state.</summary>
    public static RoomEnemyPopulationDefinition GravitySuitDefault => WreckedShipRoomEnemyPopulationDefinitions.GravitySuitDefault;
    /// <summary>The enemy population of the Wrecked Ship Map room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMapDefault => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipMapDefault;
    /// <summary>The enemy population of the Gravity Suit room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition GravitySuitWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.GravitySuitWhenAreaBossDead;
    /// <summary>The enemy population of the Basement room in its default state.</summary>
    public static RoomEnemyPopulationDefinition BasementDefault => WreckedShipRoomEnemyPopulationDefinitions.BasementDefault;
    /// <summary>The enemy population of the Wrecked Ship Entrance room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEntranceDefault => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipEntranceDefault;
    /// <summary>The enemy population of Wrecked Ship's Assembly Line room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition AssemblyLineWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.AssemblyLineWhenAreaBossDead;
    /// <summary>The enemy population of the Wrecked Ship Save room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipSaveWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipSaveWhenAreaBossDead;
    /// <summary>The enemy population of the Wrecked Ship West Super room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipWestSuperWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipWestSuperWhenAreaBossDead;
    /// <summary>The enemy population of the Attic room in its default state.</summary>
    public static RoomEnemyPopulationDefinition AtticDefault => WreckedShipRoomEnemyPopulationDefinitions.AtticDefault;
    /// <summary>The enemy population of the Wrecked Ship East Super room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEastSuperDefault => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipEastSuperDefault;
    /// <summary>The enemy population of the Wrecked Ship Save room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipSaveDefault => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipSaveDefault;
    /// <summary>The enemy population of the Electric Death room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition ElectricDeathWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.ElectricDeathWhenAreaBossDead;
    /// <summary>The enemy population of the Spiky Death room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition SpikyDeathWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.SpikyDeathWhenAreaBossDead;
    /// <summary>The enemy population of the Wrecked Ship East Super room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEastSuperWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipEastSuperWhenAreaBossDead;
    /// <summary>The enemy population of the Phantoon room in its default state.</summary>
    public static RoomEnemyPopulationDefinition PhantoonDefault => WreckedShipRoomEnemyPopulationDefinitions.PhantoonDefault;
    /// <summary>The enemy population of the Wrecked Ship Main Shaft room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMainShaftWhenAreaBossDead => WreckedShipRoomEnemyPopulationDefinitions.WreckedShipMainShaftWhenAreaBossDead;
    /// <summary>The enemy population of the Mt. Everest room.</summary>
    public static RoomEnemyPopulationDefinition MtEverest => MaridiaRoomEnemyPopulationDefinitions.MtEverest;
    /// <summary>The enemy population of the Halfie Climb room.</summary>
    public static RoomEnemyPopulationDefinition HalfieClimb => MaridiaRoomEnemyPopulationDefinitions.HalfieClimb;
    /// <summary>The enemy population of the Watering Hole room.</summary>
    public static RoomEnemyPopulationDefinition WateringHole => MaridiaRoomEnemyPopulationDefinitions.WateringHole;
    /// <summary>The enemy population of the Crab Shaft room.</summary>
    public static RoomEnemyPopulationDefinition CrabShaft => MaridiaRoomEnemyPopulationDefinitions.CrabShaft;
    /// <summary>The enemy population of the Maridia Elevator room.</summary>
    public static RoomEnemyPopulationDefinition MaridiaElevator => MaridiaRoomEnemyPopulationDefinitions.MaridiaElevator;
    /// <summary>The enemy population of the Colosseum room.</summary>
    public static RoomEnemyPopulationDefinition Colosseum => MaridiaRoomEnemyPopulationDefinitions.Colosseum;
    /// <summary>The enemy population of the Aqueduct Save room.</summary>
    public static RoomEnemyPopulationDefinition AqueductSave => MaridiaRoomEnemyPopulationDefinitions.AqueductSave;
    /// <summary>The enemy population of the Precious room.</summary>
    public static RoomEnemyPopulationDefinition ThePrecious => MaridiaRoomEnemyPopulationDefinitions.ThePrecious;
    /// <summary>The enemy population of the Botwoon Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition BotwoonEnergyTank => MaridiaRoomEnemyPopulationDefinitions.BotwoonEnergyTank;
    /// <summary>The enemy population of the Draygon Save room.</summary>
    public static RoomEnemyPopulationDefinition DraygonSave => MaridiaRoomEnemyPopulationDefinitions.DraygonSave;
    /// <summary>The enemy population of the Maridia Missile Refill room.</summary>
    public static RoomEnemyPopulationDefinition MaridiaMissileRefill => MaridiaRoomEnemyPopulationDefinitions.MaridiaMissileRefill;
    /// <summary>The enemy population of the Plasma Beach Quicksand room.</summary>
    public static RoomEnemyPopulationDefinition PlasmaBeachQuicksand => MaridiaRoomEnemyPopulationDefinitions.PlasmaBeachQuicksand;
    /// <summary>The enemy population of the Botwoon Quicksand room.</summary>
    public static RoomEnemyPopulationDefinition BotwoonQuicksand => MaridiaRoomEnemyPopulationDefinitions.BotwoonQuicksand;
    /// <summary>The enemy population of the Shaktool room.</summary>
    public static RoomEnemyPopulationDefinition Shaktool => MaridiaRoomEnemyPopulationDefinitions.Shaktool;
    /// <summary>The enemy population of the Draygon room in its default state.</summary>
    public static RoomEnemyPopulationDefinition DraygonDefault => MaridiaRoomEnemyPopulationDefinitions.DraygonDefault;
    /// <summary>The enemy population of the Aqueduct room.</summary>
    public static RoomEnemyPopulationDefinition Aqueduct => MaridiaRoomEnemyPopulationDefinitions.Aqueduct;
    /// <summary>The enemy population of the Red Fish room.</summary>
    public static RoomEnemyPopulationDefinition RedFish => MaridiaRoomEnemyPopulationDefinitions.RedFish;
    /// <summary>The enemy population of the Plasma room.</summary>
    public static RoomEnemyPopulationDefinition Plasma => MaridiaRoomEnemyPopulationDefinitions.Plasma;
    /// <summary>The enemy population of the Space Jump room.</summary>
    public static RoomEnemyPopulationDefinition SpaceJump => MaridiaRoomEnemyPopulationDefinitions.SpaceJump;
    /// <summary>The enemy population of the Pants room.</summary>
    public static RoomEnemyPopulationDefinition Pants => MaridiaRoomEnemyPopulationDefinitions.Pants;
    /// <summary>The enemy population of the Glass Tunnel Save room.</summary>
    public static RoomEnemyPopulationDefinition GlassTunnelSave => MaridiaRoomEnemyPopulationDefinitions.GlassTunnelSave;
    /// <summary>The enemy population of the Glass Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition GlassTunnel => MaridiaRoomEnemyPopulationDefinitions.GlassTunnel;
    /// <summary>The enemy population of the West Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition WestTunnel => MaridiaRoomEnemyPopulationDefinitions.WestTunnel;
    /// <summary>The enemy population of the East Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition EastTunnel => MaridiaRoomEnemyPopulationDefinitions.EastTunnel;
    /// <summary>The enemy population of the Mama Turtle room.</summary>
    public static RoomEnemyPopulationDefinition MamaTurtle => MaridiaRoomEnemyPopulationDefinitions.MamaTurtle;
    /// <summary>The enemy population of the Crab Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition CrabTunnel => MaridiaRoomEnemyPopulationDefinitions.CrabTunnel;
    /// <summary>The enemy population of the Northwest Maridia Bug room.</summary>
    public static RoomEnemyPopulationDefinition NorthwestMaridiaBug => MaridiaRoomEnemyPopulationDefinitions.NorthwestMaridiaBug;
    /// <summary>The enemy population of the Pseudo Plasma Spark room.</summary>
    public static RoomEnemyPopulationDefinition PseudoPlasmaSpark => MaridiaRoomEnemyPopulationDefinitions.PseudoPlasmaSpark;
    /// <summary>The enemy population of the West Sand Hall Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition WestSandHallTunnel => MaridiaRoomEnemyPopulationDefinitions.WestSandHallTunnel;
    /// <summary>The enemy population of the Plasma Tutorial room.</summary>
    public static RoomEnemyPopulationDefinition PlasmaTutorial => MaridiaRoomEnemyPopulationDefinitions.PlasmaTutorial;
    /// <summary>The enemy population of the Thread the Needle room.</summary>
    public static RoomEnemyPopulationDefinition ThreadTheNeedle => MaridiaRoomEnemyPopulationDefinitions.ThreadTheNeedle;
    /// <summary>The enemy population of the Plasma Spark room.</summary>
    public static RoomEnemyPopulationDefinition PlasmaSpark => MaridiaRoomEnemyPopulationDefinitions.PlasmaSpark;
    /// <summary>The enemy population of the Toilet room.</summary>
    public static RoomEnemyPopulationDefinition Toilet => MaridiaRoomEnemyPopulationDefinitions.Toilet;
    /// <summary>The enemy population of the West Sand Hall room.</summary>
    public static RoomEnemyPopulationDefinition WestSandHall => MaridiaRoomEnemyPopulationDefinitions.WestSandHall;
    /// <summary>The enemy population of the Oasis room.</summary>
    public static RoomEnemyPopulationDefinition Oasis => MaridiaRoomEnemyPopulationDefinitions.Oasis;
    /// <summary>The enemy population of the East Sand Hall room.</summary>
    public static RoomEnemyPopulationDefinition EastSandHall => MaridiaRoomEnemyPopulationDefinitions.EastSandHall;
    /// <summary>The enemy population of the West Aqueduct Quicksand room.</summary>
    public static RoomEnemyPopulationDefinition WestAqueductQuicksand => MaridiaRoomEnemyPopulationDefinitions.WestAqueductQuicksand;
    /// <summary>The enemy population of the East Aqueduct Quicksand room.</summary>
    public static RoomEnemyPopulationDefinition EastAqueductQuicksand => MaridiaRoomEnemyPopulationDefinitions.EastAqueductQuicksand;
    /// <summary>The enemy population of the Butterfly room.</summary>
    public static RoomEnemyPopulationDefinition Butterfly => MaridiaRoomEnemyPopulationDefinitions.Butterfly;
    /// <summary>The enemy population of the Botwoon Hallway room.</summary>
    public static RoomEnemyPopulationDefinition BotwoonHallway => MaridiaRoomEnemyPopulationDefinitions.BotwoonHallway;
    /// <summary>The enemy population of the East Pants room.</summary>
    public static RoomEnemyPopulationDefinition EastPants => MaridiaRoomEnemyPopulationDefinitions.EastPants;
    /// <summary>The enemy population of the Springball room.</summary>
    public static RoomEnemyPopulationDefinition Springball => MaridiaRoomEnemyPopulationDefinitions.Springball;
    /// <summary>The enemy population of the Below Botwoon Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition BelowBotwoonEnergyTank => MaridiaRoomEnemyPopulationDefinitions.BelowBotwoonEnergyTank;
    /// <summary>The enemy population of the Kassiuz room.</summary>
    public static RoomEnemyPopulationDefinition Kassiuz => MaridiaRoomEnemyPopulationDefinitions.Kassiuz;
    /// <summary>The enemy population of the Forgotten Highway Save room.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwaySave => MaridiaRoomEnemyPopulationDefinitions.ForgottenHighwaySave;
    /// <summary>The enemy population of the Maridia Energy Refill room.</summary>
    public static RoomEnemyPopulationDefinition MaridiaEnergyRefill => MaridiaRoomEnemyPopulationDefinitions.MaridiaEnergyRefill;
    /// <summary>The enemy population of the Maridia Map room.</summary>
    public static RoomEnemyPopulationDefinition MaridiaMap => MaridiaRoomEnemyPopulationDefinitions.MaridiaMap;
    /// <summary>The enemy population of the Crab Hole room.</summary>
    public static RoomEnemyPopulationDefinition CrabHole => MaridiaRoomEnemyPopulationDefinitions.CrabHole;
    /// <summary>The enemy population of the Botwoon room.</summary>
    public static RoomEnemyPopulationDefinition Botwoon => MaridiaRoomEnemyPopulationDefinitions.Botwoon;
    /// <summary>The enemy population of the Main Street room.</summary>
    public static RoomEnemyPopulationDefinition MainStreet => MaridiaRoomEnemyPopulationDefinitions.MainStreet;
    /// <summary>The enemy population of the Bug Sand Hole room.</summary>
    public static RoomEnemyPopulationDefinition BugSandHole => MaridiaRoomEnemyPopulationDefinitions.BugSandHole;
    /// <summary>The enemy population of the East Sand Hole room.</summary>
    public static RoomEnemyPopulationDefinition EastSandHole => MaridiaRoomEnemyPopulationDefinitions.EastSandHole;
    /// <summary>The enemy population of the West Sand Hole room.</summary>
    public static RoomEnemyPopulationDefinition WestSandHole => MaridiaRoomEnemyPopulationDefinitions.WestSandHole;
    /// <summary>The enemy population of the West Cactus Alley room.</summary>
    public static RoomEnemyPopulationDefinition WestCactusAlley => MaridiaRoomEnemyPopulationDefinitions.WestCactusAlley;
    /// <summary>The enemy population of the East Cactus Alley room.</summary>
    public static RoomEnemyPopulationDefinition EastCactusAlley => MaridiaRoomEnemyPopulationDefinitions.EastCactusAlley;
    /// <summary>The enemy population of the Fish Tank room.</summary>
    public static RoomEnemyPopulationDefinition FishTank => MaridiaRoomEnemyPopulationDefinitions.FishTank;
    /// <summary>The enemy population of the Tourian Escape 4 room.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape4 => TourianRoomEnemyPopulationDefinitions.TourianEscape4;
    /// <summary>The enemy population of the Mother Brain Save room.</summary>
    public static RoomEnemyPopulationDefinition MotherBrainSave => TourianRoomEnemyPopulationDefinitions.MotherBrainSave;
    /// <summary>The enemy population of the Metroids 1 room in its default state.</summary>
    public static RoomEnemyPopulationDefinition Metroids1Default => TourianRoomEnemyPopulationDefinitions.Metroids1Default;
    /// <summary>The enemy population of the Dust Torizo room.</summary>
    public static RoomEnemyPopulationDefinition DustTorizo => TourianRoomEnemyPopulationDefinitions.DustTorizo;
    /// <summary>The enemy population of the Big Boy room.</summary>
    public static RoomEnemyPopulationDefinition BigBoy => TourianRoomEnemyPopulationDefinitions.BigBoy;
    /// <summary>The enemy population of the Mother Brain room until she is defeated.</summary>
    public static RoomEnemyPopulationDefinition MotherBrainBeforeDefeat => TourianRoomEnemyPopulationDefinitions.MotherBrainBeforeDefeat;
    /// <summary>The enemy population of the Tourian Eye Door room.</summary>
    public static RoomEnemyPopulationDefinition TourianEyeDoor => TourianRoomEnemyPopulationDefinitions.TourianEyeDoor;
    /// <summary>The enemy population of the Blue Hopper room.</summary>
    public static RoomEnemyPopulationDefinition BlueHopper => TourianRoomEnemyPopulationDefinitions.BlueHopper;
    /// <summary>The enemy population of the Tourian Escape 1 room.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape1 => TourianRoomEnemyPopulationDefinitions.TourianEscape1;
    /// <summary>The enemy population of the Tourian First room.</summary>
    public static RoomEnemyPopulationDefinition TourianFirst => TourianRoomEnemyPopulationDefinitions.TourianFirst;
    /// <summary>The enemy population of the Metroids 2 room in its default state.</summary>
    public static RoomEnemyPopulationDefinition Metroids2Default => TourianRoomEnemyPopulationDefinitions.Metroids2Default;
    /// <summary>The enemy population of the Metroids 4 room in its default state.</summary>
    public static RoomEnemyPopulationDefinition Metroids4Default => TourianRoomEnemyPopulationDefinitions.Metroids4Default;
    /// <summary>The enemy population of the Metroids 1 room once the first Metroid hall is cleared.</summary>
    public static RoomEnemyPopulationDefinition Metroids1WhenFirstMetroidHallCleared => TourianRoomEnemyPopulationDefinitions.Metroids1WhenFirstMetroidHallCleared;
    /// <summary>The enemy population of the Metroids 2 room once the first Metroid shaft is cleared.</summary>
    public static RoomEnemyPopulationDefinition Metroids2WhenFirstMetroidShaftCleared => TourianRoomEnemyPopulationDefinitions.Metroids2WhenFirstMetroidShaftCleared;
    /// <summary>The enemy population of the Tourian Escape 2 room.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape2 => TourianRoomEnemyPopulationDefinitions.TourianEscape2;
    /// <summary>The enemy population of the Metroids 3 room once the second Metroid hall is cleared.</summary>
    public static RoomEnemyPopulationDefinition Metroids3WhenSecondMetroidHallCleared => TourianRoomEnemyPopulationDefinitions.Metroids3WhenSecondMetroidHallCleared;
    /// <summary>The enemy population of the Metroids 4 room once the second Metroid shaft is cleared.</summary>
    public static RoomEnemyPopulationDefinition Metroids4WhenSecondMetroidShaftCleared => TourianRoomEnemyPopulationDefinitions.Metroids4WhenSecondMetroidShaftCleared;
    /// <summary>The enemy population of the Tourian Escape 3 room.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape3 => TourianRoomEnemyPopulationDefinitions.TourianEscape3;
    /// <summary>The enemy population of the Seaweed room.</summary>
    public static RoomEnemyPopulationDefinition Seaweed => TourianRoomEnemyPopulationDefinitions.Seaweed;
    /// <summary>The enemy population of the Tourian Recharge room.</summary>
    public static RoomEnemyPopulationDefinition TourianRecharge => TourianRoomEnemyPopulationDefinitions.TourianRecharge;
    /// <summary>The enemy population of the Upper Tourian Save room.</summary>
    public static RoomEnemyPopulationDefinition UpperTourianSave => TourianRoomEnemyPopulationDefinitions.UpperTourianSave;
    /// <summary>The enemy population of the Metroids 3 room in its default state.</summary>
    public static RoomEnemyPopulationDefinition Metroids3Default => TourianRoomEnemyPopulationDefinitions.Metroids3Default;
    /// <summary>The enemy population of the Rinka Shaft room.</summary>
    public static RoomEnemyPopulationDefinition RinkaShaft => TourianRoomEnemyPopulationDefinitions.RinkaShaft;
    /// <summary>The enemy population of the Ceres Elevator room in its default state.</summary>
    public static RoomEnemyPopulationDefinition CeresElevatorDefault => CeresRoomEnemyPopulationDefinitions.CeresElevatorDefault;
    /// <summary>The enemy population of the Falling Tile room in its default state.</summary>
    public static RoomEnemyPopulationDefinition FallingTileDefault => CeresRoomEnemyPopulationDefinitions.FallingTileDefault;
    /// <summary>The enemy population of the Magnet Stairs room in its default state.</summary>
    public static RoomEnemyPopulationDefinition MagnetStairsDefault => CeresRoomEnemyPopulationDefinitions.MagnetStairsDefault;
    /// <summary>The enemy population of the Dead Scientist room in its default state.</summary>
    public static RoomEnemyPopulationDefinition DeadScientistDefault => CeresRoomEnemyPopulationDefinitions.DeadScientistDefault;
    /// <summary>The enemy population of the Ceres Ridley hallway in its default state.</summary>
    public static RoomEnemyPopulationDefinition CeresRidleyHallwayDefault => CeresRoomEnemyPopulationDefinitions.CeresRidleyHallwayDefault;
    /// <summary>The enemy population of the Ceres Ridley room in its default state.</summary>
    public static RoomEnemyPopulationDefinition CeresRidleyDefault => CeresRoomEnemyPopulationDefinitions.CeresRidleyDefault;
    /// <summary>The enemy population of the Ceres Elevator room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition CeresElevatorWhenAreaBossDead => CeresRoomEnemyPopulationDefinitions.CeresElevatorWhenAreaBossDead;
    /// <summary>The enemy population of the Falling Tile room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition FallingTileWhenAreaBossDead => CeresRoomEnemyPopulationDefinitions.FallingTileWhenAreaBossDead;
    /// <summary>The enemy population of the Magnet Stairs room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition MagnetStairsWhenAreaBossDead => CeresRoomEnemyPopulationDefinitions.MagnetStairsWhenAreaBossDead;
    /// <summary>The enemy population of the Dead Scientist room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition DeadScientistWhenAreaBossDead => CeresRoomEnemyPopulationDefinitions.DeadScientistWhenAreaBossDead;
    /// <summary>The enemy population of the Ceres Ridley hallway once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition CeresRidleyHallwayWhenAreaBossDead => CeresRoomEnemyPopulationDefinitions.CeresRidleyHallwayWhenAreaBossDead;
    /// <summary>The enemy population of the Ceres Ridley room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition CeresRidleyWhenAreaBossDead => CeresRoomEnemyPopulationDefinitions.CeresRidleyWhenAreaBossDead;

}
