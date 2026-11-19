using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The enemy graphics set of every retail room state, by name (#1527, #1599).</summary>
/// <remarks>
/// The sets are the import's knowledge alone: it decodes which set each room state names once, composes each set's
/// pictures as the cartridge loads them, and names the variant each population's enemies draw in. The game knows no set.
/// </remarks>
[NumberedIdentities("Each numbered member is the enemy graphics set of a room whose own name carries the number (Grapple Tutorial Room 1-3, Metroid Room 1-4, Tourian Escape Room 1-4); no caller indexes the sets by number.")]
internal static class RoomEnemyGraphicsSetDefinitions
{
    /// <summary>
    /// The enemy graphics a room draws under when it loads no set (#1392, #1476): no set's pictures or colours loaded over
    /// the standard ones. A room whose population is empty loads no set and draws its spawned objects under this; its
    /// pictures are installed as the standard variants (#1599).
    /// </summary>
    internal static RoomEnemyGraphicsSetDefinition SessionStart { get; } = new("SessionStart");

    /// <summary>The enemy graphics set shared by the Draygon room once the area's boss is dead and the Mother Brain room once the main area boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition Draygon1MotherBrain2 { get; } = new("Draygon1MotherBrain2");
    /// <summary>The enemy graphics set of the East Ocean room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastOcean { get; } = new("EastOcean");
    /// <summary>The enemy graphics set of the Crateria Power Bombs room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaPowerBombs { get; } = new("CrateriaPowerBombs");
    /// <summary>The enemy graphics set of the Terminator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Terminator { get; } = new("Terminator");
    /// <summary>The enemy graphics set of the Final Missile Bombway room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition FinalMissileBombwayWhenZebesAwake { get; } = new("FinalMissileBombwayWhenZebesAwake");
    /// <summary>The enemy graphics set of the Final Missile Bombway room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition FinalMissileBombwayDefault { get; } = new("FinalMissileBombwayDefault");
    /// <summary>The enemy graphics set of the Parlor room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition ParlorWhenZebesAwake { get; } = new("ParlorWhenZebesAwake");
    /// <summary>The enemy graphics set of the Flyway room while the Zebes timebomb is not set.</summary>
    internal static RoomEnemyGraphicsSetDefinition FlywayBeforeTimebomb { get; } = new("FlywayBeforeTimebomb");
    /// <summary>The enemy graphics set of the Pit room once Samus has the Morph Ball and Missiles.</summary>
    internal static RoomEnemyGraphicsSetDefinition PitWithMorphBallAndMissiles { get; } = new("PitWithMorphBallAndMissiles");
    /// <summary>The enemy graphics set of the Gauntlet Entrance room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GauntletEntrance { get; } = new("GauntletEntrance");
    /// <summary>The enemy graphics set of the Bomb Torizo room while the Zebes timebomb is not set.</summary>
    internal static RoomEnemyGraphicsSetDefinition BombTorizoBeforeTimebomb { get; } = new("BombTorizoBeforeTimebomb");
    /// <summary>The enemy graphics set of the Green Pirates Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenPiratesShaft { get; } = new("GreenPiratesShaft");
    /// <summary>The enemy graphics set of the Green Brinstar Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarElevator { get; } = new("GreenBrinstarElevator");
    /// <summary>The enemy graphics set of the Final Missile room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition FinalMissileDefault { get; } = new("FinalMissileDefault");
    /// <summary>The enemy graphics set of the Crateria Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaMap { get; } = new("CrateriaMap");
    /// <summary>The enemy graphics set of the Final Missile room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition FinalMissileWhenZebesAwake { get; } = new("FinalMissileWhenZebesAwake");
    /// <summary>The enemy graphics set of the Climb room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition ClimbWhenZebesTimebombSet { get; } = new("ClimbWhenZebesTimebombSet");
    /// <summary>The enemy graphics set of the Crateria Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaSave { get; } = new("CrateriaSave");
    /// <summary>The enemy graphics set of the Forgotten Highway Elbow room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ForgottenHighwayElbow { get; } = new("ForgottenHighwayElbow");
    /// <summary>The enemy graphics set of the Moat room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Moat { get; } = new("Moat");
    /// <summary>The enemy graphics set of the Climb room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition ClimbDefault { get; } = new("ClimbDefault");
    /// <summary>The enemy graphics set of the West Ocean room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestOcean { get; } = new("WestOcean");
    /// <summary>The enemy graphics set of the Parlor room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition ParlorDefault { get; } = new("ParlorDefault");
    /// <summary>The enemy graphics set of the Landing Site room while the Zebes timebomb is not set.</summary>
    internal static RoomEnemyGraphicsSetDefinition LandingSiteBeforeTimebomb { get; } = new("LandingSiteBeforeTimebomb");
    /// <summary>The enemy graphics set of the Crateria Super room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaSuper { get; } = new("CrateriaSuper");
    /// <summary>The enemy graphics set of the Pre Bowling room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PreBowling { get; } = new("PreBowling");
    /// <summary>The enemy graphics set of the Climb room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition ClimbWhenZebesAwake { get; } = new("ClimbWhenZebesAwake");
    /// <summary>The enemy graphics set of the Bowling Alley Path room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BowlingAlleyPath { get; } = new("BowlingAlleyPath");
    /// <summary>The enemy graphics set of the Red Brinstar Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedBrinstarElevator { get; } = new("RedBrinstarElevator");
    /// <summary>The enemy graphics set of the Gauntlet Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GauntletEnergyTank { get; } = new("GauntletEnergyTank");
    /// <summary>The enemy graphics set of the Pit room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition PitDefault { get; } = new("PitDefault");
    /// <summary>The enemy graphics set of the Crab Maze room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrabMaze { get; } = new("CrabMaze");
    /// <summary>The enemy graphics set of the Crateria Tube room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaTube { get; } = new("CrateriaTube");
    /// <summary>The enemy graphics set of the Forgotten Highway Kagos room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ForgottenHighwayKagos { get; } = new("ForgottenHighwayKagos");
    /// <summary>The enemy graphics set of the Elevator to Blue Brinstar room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ElevatorToBlueBrinstar { get; } = new("ElevatorToBlueBrinstar");
    /// <summary>The enemy graphics set of the Forgotten Highway Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ForgottenHighwayElevator { get; } = new("ForgottenHighwayElevator");
    /// <summary>The enemy graphics set of the Pre Map Flyway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PreMapFlyway { get; } = new("PreMapFlyway");
    /// <summary>The enemy graphics set of the Lower Mushrooms room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerMushrooms { get; } = new("LowerMushrooms");
    /// <summary>The enemy graphics set of the Landing Site room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition LandingSiteWhenZebesTimebombSet { get; } = new("LandingSiteWhenZebesTimebombSet");
    /// <summary>The enemy graphics set of the Parlor room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition ParlorWhenZebesTimebombSet { get; } = new("ParlorWhenZebesTimebombSet");
    /// <summary>The enemy graphics set of the Bomb Torizo room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition BombTorizoWhenZebesTimebombSet { get; } = new("BombTorizoWhenZebesTimebombSet");
    /// <summary>The enemy graphics set of the Flyway room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition FlywayWhenZebesTimebombSet { get; } = new("FlywayWhenZebesTimebombSet");
    /// <summary>The enemy graphics set of the Crateria Kihunter room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaKihunter { get; } = new("CrateriaKihunter");
    /// <summary>The enemy graphics set of the Brinstar Pre Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BrinstarPreMap { get; } = new("BrinstarPreMap");
    /// <summary>The enemy graphics set of the Green Brinstar Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarMap { get; } = new("GreenBrinstarMap");
    /// <summary>The enemy graphics set of the Green Brinstar Missile Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarMissileRefill { get; } = new("GreenBrinstarMissileRefill");
    /// <summary>The enemy graphics set of the Spore Spawn Kihunters room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SporeSpawnKihunters { get; } = new("SporeSpawnKihunters");
    /// <summary>The enemy graphics set of the Etecoon Super room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EtecoonSuper { get; } = new("EtecoonSuper");
    /// <summary>The enemy graphics set of the Dachora Energy Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition DachoraEnergyRefill { get; } = new("DachoraEnergyRefill");
    /// <summary>The enemy graphics set of the Spore Spawn Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SporeSpawnFarming { get; } = new("SporeSpawnFarming");
    /// <summary>The enemy graphics set of the Statues room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Statues { get; } = new("Statues");
    /// <summary>The enemy graphics set of the Hopper Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition HopperEnergyTank { get; } = new("HopperEnergyTank");
    /// <summary>The enemy graphics set of the Beta Power Bomb room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BetaPowerBomb { get; } = new("BetaPowerBomb");
    /// <summary>The enemy graphics set of the Waterway Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WaterwayEnergyTank { get; } = new("WaterwayEnergyTank");
    /// <summary>The enemy graphics set of the Construction Zone room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition ConstructionZoneWhenZebesAwake { get; } = new("ConstructionZoneWhenZebesAwake");
    /// <summary>The enemy graphics set of the Blue Brinstar Energy Tank room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition BlueBrinstarEnergyTankWhenZebesAwake { get; } = new("BlueBrinstarEnergyTankWhenZebesAwake");
    /// <summary>The enemy graphics set of the Noob Bridge room.</summary>
    internal static RoomEnemyGraphicsSetDefinition NoobBridge { get; } = new("NoobBridge");
    /// <summary>The enemy graphics set of the Morph Ball room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition MorphBallWhenZebesAwake { get; } = new("MorphBallWhenZebesAwake");
    /// <summary>The enemy graphics set of the Warehouse Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseSave { get; } = new("WarehouseSave");
    /// <summary>The enemy graphics set of the Morph Ball room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition MorphBallDefault { get; } = new("MorphBallDefault");
    /// <summary>The enemy graphics set of the Warehouse Zeela room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseZeela { get; } = new("WarehouseZeela");
    /// <summary>The enemy graphics set of the Red Tower room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedTower { get; } = new("RedTower");
    /// <summary>The enemy graphics set of the Blue Brinstar Boulders room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BlueBrinstarBoulders { get; } = new("BlueBrinstarBoulders");
    /// <summary>The enemy graphics set of the Billy Mays room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BillyMays { get; } = new("BillyMays");
    /// <summary>The enemy graphics set of the Big Pink Save Room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BigPinkSaveRoom { get; } = new("BigPinkSaveRoom");
    /// <summary>The enemy graphics set of the Big Pink room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BigPink { get; } = new("BigPink");
    /// <summary>The enemy graphics set of the Etecoon Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EtecoonSave { get; } = new("EtecoonSave");
    /// <summary>The enemy graphics set of the Below Spazer room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BelowSpazer { get; } = new("BelowSpazer");
    /// <summary>The enemy graphics set of the Green Brinstar Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarSave { get; } = new("GreenBrinstarSave");
    /// <summary>The enemy graphics set of the Spazer room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Spazer { get; } = new("Spazer");
    /// <summary>The enemy graphics set of the Pink Brinstar Power Bombs room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PinkBrinstarPowerBombs { get; } = new("PinkBrinstarPowerBombs");
    /// <summary>The enemy graphics set of the Red Tower Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedTowerRefill { get; } = new("RedTowerRefill");
    /// <summary>The enemy graphics set of the Kraid Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition KraidRefill { get; } = new("KraidRefill");
    /// <summary>The enemy graphics set of the Varia Suit room.</summary>
    internal static RoomEnemyGraphicsSetDefinition VariaSuit { get; } = new("VariaSuit");
    /// <summary>The enemy graphics set of the Red Brinstar Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedBrinstarSave { get; } = new("RedBrinstarSave");
    /// <summary>The enemy graphics set of the Blue Brinstar Energy Tank room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition BlueBrinstarEnergyTankDefault { get; } = new("BlueBrinstarEnergyTankDefault");
    /// <summary>The enemy graphics set of the Green Brinstar Firefleas room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarFirefleas { get; } = new("GreenBrinstarFirefleas");
    /// <summary>The enemy graphics set of the Green Brinstar Beetoms room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarBeetoms { get; } = new("GreenBrinstarBeetoms");
    /// <summary>The enemy graphics set of the Etecoon Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EtecoonEnergyTank { get; } = new("EtecoonEnergyTank");
    /// <summary>The enemy graphics set of the Pink Brinstar Hoppers room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PinkBrinstarHoppers { get; } = new("PinkBrinstarHoppers");
    /// <summary>The enemy graphics set of the Warehouse Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseEnergyTank { get; } = new("WarehouseEnergyTank");
    /// <summary>The enemy graphics set of the Warehouse Entrance room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseEntrance { get; } = new("WarehouseEntrance");
    /// <summary>The enemy graphics set of the Warehouse Kihunter room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseKihunter { get; } = new("WarehouseKihunter");
    /// <summary>The enemy graphics set of the Green Brinstar Main Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarMainShaft { get; } = new("GreenBrinstarMainShaft");
    /// <summary>The enemy graphics set of the Spore Spawn Super room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SporeSpawnSuper { get; } = new("SporeSpawnSuper");
    /// <summary>The enemy graphics set of the Early Supers room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EarlySupers { get; } = new("EarlySupers");
    /// <summary>The enemy graphics set of the Red Brinstar Firefleas room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedBrinstarFirefleas { get; } = new("RedBrinstarFirefleas");
    /// <summary>The enemy graphics set of the Construction Zone room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition ConstructionZoneDefault { get; } = new("ConstructionZoneDefault");
    /// <summary>The enemy graphics set of the Green Hill Zone room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenHillZone { get; } = new("GreenHillZone");
    /// <summary>The enemy graphics set of the Dachora room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Dachora { get; } = new("Dachora");
    /// <summary>The enemy graphics set of the Hellway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Hellway { get; } = new("Hellway");
    /// <summary>The enemy graphics set of the First Missile room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FirstMissile { get; } = new("FirstMissile");
    /// <summary>The enemy graphics set of the Kraid room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Kraid { get; } = new("Kraid");
    /// <summary>The enemy graphics set of the Brinstar Reserve Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BrinstarReserveTank { get; } = new("BrinstarReserveTank");
    /// <summary>The enemy graphics set of the X-Ray Scope room.</summary>
    internal static RoomEnemyGraphicsSetDefinition XrayScope { get; } = new("XrayScope");
    /// <summary>The enemy graphics set of the Statues Hallway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition StatuesHallway { get; } = new("StatuesHallway");
    /// <summary>The enemy graphics set of the Alpha Power Bomb room.</summary>
    internal static RoomEnemyGraphicsSetDefinition AlphaPowerBomb { get; } = new("AlphaPowerBomb");
    /// <summary>The enemy graphics set of the Kraid Eye Door room.</summary>
    internal static RoomEnemyGraphicsSetDefinition KraidEyeDoor { get; } = new("KraidEyeDoor");
    /// <summary>The enemy graphics set of the Caterpillar room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Caterpillar { get; } = new("Caterpillar");
    /// <summary>The enemy graphics set of the Mini Kraid room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MiniKraid { get; } = new("MiniKraid");
    /// <summary>The enemy graphics set of the Spore Spawn room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SporeSpawn { get; } = new("SporeSpawn");
    /// <summary>The enemy graphics set of the Skree Boost room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SkreeBoost { get; } = new("SkreeBoost");
    /// <summary>The enemy graphics set of the Grapple Tutorial 3 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GrappleTutorial3 { get; } = new("GrappleTutorial3");
    /// <summary>The enemy graphics set of the Post-Crocomire Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocomireShaft { get; } = new("PostCrocomireShaft");
    /// <summary>The enemy graphics set of the Metal Pirates room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MetalPirates { get; } = new("MetalPirates");
    /// <summary>The enemy graphics set of the Acid Statue room.</summary>
    internal static RoomEnemyGraphicsSetDefinition AcidStatue { get; } = new("AcidStatue");
    /// <summary>The enemy graphics set of the Purple Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PurpleFarming { get; } = new("PurpleFarming");
    /// <summary>The enemy graphics set of the Ice Beam Gate room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeamGate { get; } = new("IceBeamGate");
    /// <summary>The enemy graphics set of the Spiky Acid Snakes Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpikyAcidSnakesTunnel { get; } = new("SpikyAcidSnakesTunnel");
    /// <summary>The enemy graphics set of the Red Kihunter Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedKihunterShaft { get; } = new("RedKihunterShaft");
    /// <summary>The enemy graphics set of the Fast Ripper room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FastRipper { get; } = new("FastRipper");
    /// <summary>The enemy graphics set of the Wave Beam room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WaveBeam { get; } = new("WaveBeam");
    /// <summary>The enemy graphics set of the Fast Pillars Setup room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FastPillarsSetup { get; } = new("FastPillarsSetup");
    /// <summary>The enemy graphics set of the Lower Norfair Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerNorfairElevator { get; } = new("LowerNorfairElevator");
    /// <summary>The enemy graphics set of the Lower Norfair Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerNorfairSave { get; } = new("LowerNorfairSave");
    /// <summary>The enemy graphics set of the Upper Norfair Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition UpperNorfairRefill { get; } = new("UpperNorfairRefill");
    /// <summary>The enemy graphics set of the Norfair Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition NorfairMap { get; } = new("NorfairMap");
    /// <summary>The enemy graphics set of the Mickey Mouse room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MickeyMouse { get; } = new("MickeyMouse");
    /// <summary>The enemy graphics set of the Golden Torizo Energy Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GoldenTorizoEnergyRefill { get; } = new("GoldenTorizoEnergyRefill");
    /// <summary>The enemy graphics set of the Ridley room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Ridley { get; } = new("Ridley");
    /// <summary>The enemy graphics set of the Post-Crocomire Power Bombs room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocomirePowerBombs { get; } = new("PostCrocomirePowerBombs");
    /// <summary>The enemy graphics set of the Ice Beam room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeam { get; } = new("IceBeam");
    /// <summary>The enemy graphics set of the Hi-Jump Boots room.</summary>
    internal static RoomEnemyGraphicsSetDefinition HiJumpBoots { get; } = new("HiJumpBoots");
    /// <summary>The enemy graphics set of the Post-Crocomire Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocomireSave { get; } = new("PostCrocomireSave");
    /// <summary>The enemy graphics set of the Frog Speedway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FrogSpeedway { get; } = new("FrogSpeedway");
    /// <summary>The enemy graphics set of the Rising Tide room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RisingTide { get; } = new("RisingTide");
    /// <summary>The enemy graphics set of the Post-Crocomire Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocomireFarming { get; } = new("PostCrocomireFarming");
    /// <summary>The enemy graphics set of the Worst Room in the Game.</summary>
    internal static RoomEnemyGraphicsSetDefinition WorstRoomInTheGame { get; } = new("WorstRoomInTheGame");
    /// <summary>The enemy graphics set of the Double Chamber room.</summary>
    internal static RoomEnemyGraphicsSetDefinition DoubleChamber { get; } = new("DoubleChamber");
    /// <summary>The enemy graphics set of the Grapple Beam room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GrappleBeam { get; } = new("GrappleBeam");
    /// <summary>The enemy graphics set of the Red Pirate Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedPirateShaft { get; } = new("RedPirateShaft");
    /// <summary>The enemy graphics set of the Crocomire Speedway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrocomireSpeedway { get; } = new("CrocomireSpeedway");
    /// <summary>The enemy graphics set of the Plowerhouse room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Plowerhouse { get; } = new("Plowerhouse");
    /// <summary>The enemy graphics set of the Lower Norfair Fireflea room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerNorfairFireflea { get; } = new("LowerNorfairFireflea");
    /// <summary>The enemy graphics set of the Magdollite Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MagdolliteTunnel { get; } = new("MagdolliteTunnel");
    /// <summary>The enemy graphics set of the Lava Dive room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LavaDive { get; } = new("LavaDive");
    /// <summary>The enemy graphics set of the Pillar room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Pillar { get; } = new("Pillar");
    /// <summary>The enemy graphics set of the Crocomire Escape room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrocomireEscape { get; } = new("CrocomireEscape");
    /// <summary>The enemy graphics set of the Lower Norfair Spring Ball Maze room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerNorfairSpringBallMaze { get; } = new("LowerNorfairSpringBallMaze");
    /// <summary>The enemy graphics set of the Lower Norfair Escape Power Bombs room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerNorfairEscapePowerBombs { get; } = new("LowerNorfairEscapePowerBombs");
    /// <summary>The enemy graphics set of the Purple Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PurpleShaft { get; } = new("PurpleShaft");
    /// <summary>The enemy graphics set of the Ridley Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RidleyEnergyTank { get; } = new("RidleyEnergyTank");
    /// <summary>The enemy graphics set of the Screw Attack room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ScrewAttack { get; } = new("ScrewAttack");
    /// <summary>The enemy graphics set of the Green Bubbles Missiles room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBubblesMissiles { get; } = new("GreenBubblesMissiles");
    /// <summary>The enemy graphics set of the Amphitheatre room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Amphitheatre { get; } = new("Amphitheatre");
    /// <summary>The enemy graphics set of the Cathedral Entrance room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CathedralEntrance { get; } = new("CathedralEntrance");
    /// <summary>The enemy graphics set of the Main Hall room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MainHall { get; } = new("MainHall");
    /// <summary>The enemy graphics set of the Post-Crocomire Jump room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocomireJump { get; } = new("PostCrocomireJump");
    /// <summary>The enemy graphics set of the Frog Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FrogSave { get; } = new("FrogSave");
    /// <summary>The enemy graphics set of the Crocomire Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrocomireSave { get; } = new("CrocomireSave");
    /// <summary>The enemy graphics set of the Crumble Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrumbleShaft { get; } = new("CrumbleShaft");
    /// <summary>The enemy graphics set of the Wasteland room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Wasteland { get; } = new("Wasteland");
    /// <summary>The enemy graphics set of the Acid Snakes Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition AcidSnakesTunnel { get; } = new("AcidSnakesTunnel");
    /// <summary>The enemy graphics set of the Grapple Tutorial 1 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GrappleTutorial1 { get; } = new("GrappleTutorial1");
    /// <summary>The enemy graphics set of the Bubble Mountain Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BubbleMountainSave { get; } = new("BubbleMountainSave");
    /// <summary>The enemy graphics set of the Hi-Jump Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition HiJumpEnergyTank { get; } = new("HiJumpEnergyTank");
    /// <summary>The enemy graphics set of the Bubble Mountain room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BubbleMountain { get; } = new("BubbleMountain");
    /// <summary>The enemy graphics set of the Ice Beam Acid room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeamAcid { get; } = new("IceBeamAcid");
    /// <summary>The enemy graphics set of the Ice Beam Tutorial room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeamTutorial { get; } = new("IceBeamTutorial");
    /// <summary>The enemy graphics set of the Post-Crocomire Missile room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocomireMissile { get; } = new("PostCrocomireMissile");
    /// <summary>The enemy graphics set of the Cathedral room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Cathedral { get; } = new("Cathedral");
    /// <summary>The enemy graphics set of the Norfair Reserve Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition NorfairReserveTank { get; } = new("NorfairReserveTank");
    /// <summary>The enemy graphics set of the Grapple Tutorial 2 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GrappleTutorial2 { get; } = new("GrappleTutorial2");
    /// <summary>The enemy graphics set of the Ice Beam Snake room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeamSnake { get; } = new("IceBeamSnake");
    /// <summary>The enemy graphics set of the Golden Torizo room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GoldenTorizo { get; } = new("GoldenTorizo");
    /// <summary>The enemy graphics set of the Spiky Platforms Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpikyPlatformsTunnel { get; } = new("SpikyPlatformsTunnel");
    /// <summary>The enemy graphics set of the Speed Booster room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpeedBooster { get; } = new("SpeedBooster");
    /// <summary>The enemy graphics set of the Lower Norfair Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerNorfairFarming { get; } = new("LowerNorfairFarming");
    /// <summary>The enemy graphics set of the Single Chamber room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SingleChamber { get; } = new("SingleChamber");
    /// <summary>The enemy graphics set of the Speed Booster Hall room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpeedBoosterHall { get; } = new("SpeedBoosterHall");
    /// <summary>The enemy graphics set of the Bat Cave room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BatCave { get; } = new("BatCave");
    /// <summary>The enemy graphics set of the Kronic Boost room.</summary>
    internal static RoomEnemyGraphicsSetDefinition KronicBoost { get; } = new("KronicBoost");
    /// <summary>The enemy graphics set of the Business Center room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BusinessCenter { get; } = new("BusinessCenter");
    /// <summary>The enemy graphics set of the Three Musketeers room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ThreeMusketeers { get; } = new("ThreeMusketeers");
    /// <summary>The enemy graphics set of the Crocomire room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Crocomire { get; } = new("Crocomire");
    /// <summary>The enemy graphics set of the Lower Norfair Elevator Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerNorfairElevatorSave { get; } = new("LowerNorfairElevatorSave");
    /// <summary>The enemy graphics set of the Volcano room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Volcano { get; } = new("Volcano");
    /// <summary>The enemy graphics set of the Upper Norfair Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition UpperNorfairFarming { get; } = new("UpperNorfairFarming");
    /// <summary>The enemy graphics set of the Sponge Bath room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpongeBathWhenAreaBossDead { get; } = new("SpongeBathWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Wrecked Ship Energy Tank room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipEnergyTankWhenAreaBossDead { get; } = new("WreckedShipEnergyTankWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Wrecked Ship Main Shaft room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipMainShaftDefault { get; } = new("WreckedShipMainShaftDefault");
    /// <summary>The enemy graphics set of the Bowling Alley room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition BowlingAlleyDefault { get; } = new("BowlingAlleyDefault");
    /// <summary>The enemy graphics set of the Attic room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition AtticWhenAreaBossDead { get; } = new("AtticWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Basement room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition BasementWhenAreaBossDead { get; } = new("BasementWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Wrecked Ship Map room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipMapWhenAreaBossDead { get; } = new("WreckedShipMapWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Wrecked Ship West Super room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipWestSuperDefault { get; } = new("WreckedShipWestSuperDefault");
    /// <summary>The enemy graphics set of the Spiky Death room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpikyDeathDefault { get; } = new("SpikyDeathDefault");
    /// <summary>The enemy graphics set of the Sponge Bath room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpongeBathDefault { get; } = new("SpongeBathDefault");
    /// <summary>The enemy graphics set of the Wrecked Ship Entrance room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipEntranceWhenAreaBossDead { get; } = new("WreckedShipEntranceWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Electric Death room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition ElectricDeathDefault { get; } = new("ElectricDeathDefault");
    /// <summary>The enemy graphics set of the Bowling Alley room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition BowlingAlleyWhenAreaBossDead { get; } = new("BowlingAlleyWhenAreaBossDead");
    /// <summary>The enemy graphics set of Wrecked Ship's Assembly Line room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition AssemblyLineDefault { get; } = new("AssemblyLineDefault");
    /// <summary>The enemy graphics set of the Phantoon room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition PhantoonWhenAreaBossDead { get; } = new("PhantoonWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Wrecked Ship Energy Tank room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipEnergyTankDefault { get; } = new("WreckedShipEnergyTankDefault");
    /// <summary>The enemy graphics set of the Gravity Suit room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition GravitySuitDefault { get; } = new("GravitySuitDefault");
    /// <summary>The enemy graphics set of the Wrecked Ship Map room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipMapDefault { get; } = new("WreckedShipMapDefault");
    /// <summary>The enemy graphics set of the Gravity Suit room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition GravitySuitWhenAreaBossDead { get; } = new("GravitySuitWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Basement room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition BasementDefault { get; } = new("BasementDefault");
    /// <summary>The enemy graphics set of the Wrecked Ship Entrance room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipEntranceDefault { get; } = new("WreckedShipEntranceDefault");
    /// <summary>The enemy graphics set of Wrecked Ship's Assembly Line room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition AssemblyLineWhenAreaBossDead { get; } = new("AssemblyLineWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Wrecked Ship Save room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipSaveWhenAreaBossDead { get; } = new("WreckedShipSaveWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Wrecked Ship West Super room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipWestSuperWhenAreaBossDead { get; } = new("WreckedShipWestSuperWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Attic room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition AtticDefault { get; } = new("AtticDefault");
    /// <summary>The enemy graphics set of the Wrecked Ship East Super room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipEastSuperDefault { get; } = new("WreckedShipEastSuperDefault");
    /// <summary>The enemy graphics set of the Wrecked Ship Save room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipSaveDefault { get; } = new("WreckedShipSaveDefault");
    /// <summary>The enemy graphics set of the Electric Death room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition ElectricDeathWhenAreaBossDead { get; } = new("ElectricDeathWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Spiky Death room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpikyDeathWhenAreaBossDead { get; } = new("SpikyDeathWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Wrecked Ship East Super room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipEastSuperWhenAreaBossDead { get; } = new("WreckedShipEastSuperWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Phantoon room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition PhantoonDefault { get; } = new("PhantoonDefault");
    /// <summary>The enemy graphics set of the Wrecked Ship Main Shaft room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipMainShaftWhenAreaBossDead { get; } = new("WreckedShipMainShaftWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Mt. Everest room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MtEverest { get; } = new("MtEverest");
    /// <summary>The enemy graphics set of the Halfie Climb room.</summary>
    internal static RoomEnemyGraphicsSetDefinition HalfieClimb { get; } = new("HalfieClimb");
    /// <summary>The enemy graphics set of the Watering Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WateringHole { get; } = new("WateringHole");
    /// <summary>The enemy graphics set of the Crab Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrabShaft { get; } = new("CrabShaft");
    /// <summary>The enemy graphics set of the Maridia Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MaridiaElevator { get; } = new("MaridiaElevator");
    /// <summary>The enemy graphics set of the Colosseum room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Colosseum { get; } = new("Colosseum");
    /// <summary>The enemy graphics set of the Aqueduct Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition AqueductSave { get; } = new("AqueductSave");
    /// <summary>The enemy graphics set of the Precious room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ThePrecious { get; } = new("ThePrecious");
    /// <summary>The enemy graphics set of the Botwoon Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BotwoonEnergyTank { get; } = new("BotwoonEnergyTank");
    /// <summary>The enemy graphics set of the Draygon Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition DraygonSave { get; } = new("DraygonSave");
    /// <summary>The enemy graphics set of the Maridia Missile Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MaridiaMissileRefill { get; } = new("MaridiaMissileRefill");
    /// <summary>The enemy graphics set of the Plasma Beach Quicksand room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PlasmaBeachQuicksand { get; } = new("PlasmaBeachQuicksand");
    /// <summary>The enemy graphics set of the Botwoon Quicksand room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BotwoonQuicksand { get; } = new("BotwoonQuicksand");
    /// <summary>The enemy graphics set of the Shaktool room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Shaktool { get; } = new("Shaktool");
    /// <summary>The enemy graphics set of the Draygon room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition DraygonDefault { get; } = new("DraygonDefault");
    /// <summary>The enemy graphics set of the Aqueduct room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Aqueduct { get; } = new("Aqueduct");
    /// <summary>The enemy graphics set of the Red Fish room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedFish { get; } = new("RedFish");
    /// <summary>The enemy graphics set of the Plasma room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Plasma { get; } = new("Plasma");
    /// <summary>The enemy graphics set of the Space Jump room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpaceJump { get; } = new("SpaceJump");
    /// <summary>The enemy graphics set of the Pants room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Pants { get; } = new("Pants");
    /// <summary>The enemy graphics set of the Glass Tunnel Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GlassTunnelSave { get; } = new("GlassTunnelSave");
    /// <summary>The enemy graphics set of the Glass Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GlassTunnel { get; } = new("GlassTunnel");
    /// <summary>The enemy graphics set of the West Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestTunnel { get; } = new("WestTunnel");
    /// <summary>The enemy graphics set of the East Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastTunnel { get; } = new("EastTunnel");
    /// <summary>The enemy graphics set of the Mama Turtle room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MamaTurtle { get; } = new("MamaTurtle");
    /// <summary>The enemy graphics set of the Crab Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrabTunnel { get; } = new("CrabTunnel");
    /// <summary>The enemy graphics set of the Northwest Maridia Bug room.</summary>
    internal static RoomEnemyGraphicsSetDefinition NorthwestMaridiaBug { get; } = new("NorthwestMaridiaBug");
    /// <summary>The enemy graphics set of the Pseudo Plasma Spark room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PseudoPlasmaSpark { get; } = new("PseudoPlasmaSpark");
    /// <summary>The enemy graphics set of the West Sand Hall Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestSandHallTunnel { get; } = new("WestSandHallTunnel");
    /// <summary>The enemy graphics set of the Plasma Tutorial room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PlasmaTutorial { get; } = new("PlasmaTutorial");
    /// <summary>The enemy graphics set of the Thread the Needle room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ThreadTheNeedle { get; } = new("ThreadTheNeedle");
    /// <summary>The enemy graphics set of the Plasma Spark room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PlasmaSpark { get; } = new("PlasmaSpark");
    /// <summary>The enemy graphics set of the Toilet room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Toilet { get; } = new("Toilet");
    /// <summary>The enemy graphics set of the West Sand Hall room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestSandHall { get; } = new("WestSandHall");
    /// <summary>The enemy graphics set of the Oasis room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Oasis { get; } = new("Oasis");
    /// <summary>The enemy graphics set of the East Sand Hall room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastSandHall { get; } = new("EastSandHall");
    /// <summary>The enemy graphics set of the West Aqueduct Quicksand room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestAqueductQuicksand { get; } = new("WestAqueductQuicksand");
    /// <summary>The enemy graphics set of the East Aqueduct Quicksand room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastAqueductQuicksand { get; } = new("EastAqueductQuicksand");
    /// <summary>The enemy graphics set of the Butterfly room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Butterfly { get; } = new("Butterfly");
    /// <summary>The enemy graphics set of the Botwoon Hallway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BotwoonHallway { get; } = new("BotwoonHallway");
    /// <summary>The enemy graphics set of the East Pants room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastPants { get; } = new("EastPants");
    /// <summary>The enemy graphics set of the Springball room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Springball { get; } = new("Springball");
    /// <summary>The enemy graphics set of the Below Botwoon Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BelowBotwoonEnergyTank { get; } = new("BelowBotwoonEnergyTank");
    /// <summary>The enemy graphics set of the Kassiuz room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Kassiuz { get; } = new("Kassiuz");
    /// <summary>The enemy graphics set of the Forgotten Highway Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ForgottenHighwaySave { get; } = new("ForgottenHighwaySave");
    /// <summary>The enemy graphics set of the Maridia Energy Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MaridiaEnergyRefill { get; } = new("MaridiaEnergyRefill");
    /// <summary>The enemy graphics set of the Maridia Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MaridiaMap { get; } = new("MaridiaMap");
    /// <summary>The enemy graphics set of the Crab Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrabHole { get; } = new("CrabHole");
    /// <summary>The enemy graphics set of the Botwoon room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Botwoon { get; } = new("Botwoon");
    /// <summary>The enemy graphics set of the Main Street room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MainStreet { get; } = new("MainStreet");
    /// <summary>The enemy graphics set of the Bug Sand Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BugSandHole { get; } = new("BugSandHole");
    /// <summary>The enemy graphics set of the East Sand Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastSandHole { get; } = new("EastSandHole");
    /// <summary>The enemy graphics set of the West Sand Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestSandHole { get; } = new("WestSandHole");
    /// <summary>The enemy graphics set of the West Cactus Alley room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestCactusAlley { get; } = new("WestCactusAlley");
    /// <summary>The enemy graphics set of the East Cactus Alley room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastCactusAlley { get; } = new("EastCactusAlley");
    /// <summary>The enemy graphics set of the Fish Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FishTank { get; } = new("FishTank");
    /// <summary>The enemy graphics set of the Tourian Escape 4 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEscape4 { get; } = new("TourianEscape4");
    /// <summary>The enemy graphics set of the Mother Brain Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MotherBrainSave { get; } = new("MotherBrainSave");
    /// <summary>The enemy graphics set of the Metroids 1 room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids1Default { get; } = new("Metroids1Default");
    /// <summary>The enemy graphics set of the Dust Torizo room.</summary>
    internal static RoomEnemyGraphicsSetDefinition DustTorizo { get; } = new("DustTorizo");
    /// <summary>The enemy graphics set of the Big Boy room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BigBoy { get; } = new("BigBoy");
    /// <summary>The enemy graphics set of the Mother Brain room until she is defeated.</summary>
    internal static RoomEnemyGraphicsSetDefinition MotherBrainBeforeDefeat { get; } = new("MotherBrainBeforeDefeat");
    /// <summary>The enemy graphics set of the Tourian Eye Door room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEyeDoor { get; } = new("TourianEyeDoor");
    /// <summary>The enemy graphics set of the Blue Hopper room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BlueHopper { get; } = new("BlueHopper");
    /// <summary>The enemy graphics set of the Tourian Escape 1 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEscape1 { get; } = new("TourianEscape1");
    /// <summary>The enemy graphics set of the Tourian First room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianFirst { get; } = new("TourianFirst");
    /// <summary>The enemy graphics set of the Metroids 2 room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids2Default { get; } = new("Metroids2Default");
    /// <summary>The enemy graphics set of the Metroids 4 room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids4Default { get; } = new("Metroids4Default");
    /// <summary>The enemy graphics set of the Metroids 1 room once the first Metroid hall is cleared.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids1WhenFirstMetroidHallCleared { get; } = new("Metroids1WhenFirstMetroidHallCleared");
    /// <summary>The enemy graphics set of the Metroids 2 room once the first Metroid shaft is cleared.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids2WhenFirstMetroidShaftCleared { get; } = new("Metroids2WhenFirstMetroidShaftCleared");
    /// <summary>The enemy graphics set of the Tourian Escape 2 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEscape2 { get; } = new("TourianEscape2");
    /// <summary>The enemy graphics set of the Metroids 3 room once the second Metroid hall is cleared.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids3WhenSecondMetroidHallCleared { get; } = new("Metroids3WhenSecondMetroidHallCleared");
    /// <summary>The enemy graphics set of the Metroids 4 room once the second Metroid shaft is cleared.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids4WhenSecondMetroidShaftCleared { get; } = new("Metroids4WhenSecondMetroidShaftCleared");
    /// <summary>The enemy graphics set of the Tourian Escape 3 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEscape3 { get; } = new("TourianEscape3");
    /// <summary>The enemy graphics set of the Seaweed room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Seaweed { get; } = new("Seaweed");
    /// <summary>The enemy graphics set of the Tourian Recharge room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianRecharge { get; } = new("TourianRecharge");
    /// <summary>The enemy graphics set of the Upper Tourian Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition UpperTourianSave { get; } = new("UpperTourianSave");
    /// <summary>The enemy graphics set of the Metroids 3 room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids3Default { get; } = new("Metroids3Default");
    /// <summary>The enemy graphics set of the Rinka Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RinkaShaft { get; } = new("RinkaShaft");
    /// <summary>The enemy graphics set of the Ceres Elevator room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresElevatorDefault { get; } = new("CeresElevatorDefault");
    /// <summary>The enemy graphics set of the Falling Tile room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition FallingTileDefault { get; } = new("FallingTileDefault");
    /// <summary>The enemy graphics set of the Magnet Stairs room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition MagnetStairsDefault { get; } = new("MagnetStairsDefault");
    /// <summary>The enemy graphics set of the Dead Scientist room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition DeadScientistDefault { get; } = new("DeadScientistDefault");
    /// <summary>The enemy graphics set of the Ceres Ridley hallway in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresRidleyHallwayDefault { get; } = new("CeresRidleyHallwayDefault");
    /// <summary>The enemy graphics set of the Ceres Ridley room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresRidleyDefault { get; } = new("CeresRidleyDefault");
    /// <summary>The enemy graphics set of the Ceres Elevator room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresElevatorWhenAreaBossDead { get; } = new("CeresElevatorWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Falling Tile room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition FallingTileWhenAreaBossDead { get; } = new("FallingTileWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Magnet Stairs room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition MagnetStairsWhenAreaBossDead { get; } = new("MagnetStairsWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Dead Scientist room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition DeadScientistWhenAreaBossDead { get; } = new("DeadScientistWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Ceres Ridley hallway once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresRidleyHallwayWhenAreaBossDead { get; } = new("CeresRidleyHallwayWhenAreaBossDead");
    /// <summary>The enemy graphics set of the Ceres Ridley room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresRidleyWhenAreaBossDead { get; } = new("CeresRidleyWhenAreaBossDead");

}
