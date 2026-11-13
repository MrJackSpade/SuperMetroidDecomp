using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The enemy graphics set of every retail room state, by name (#1527, #1599).</summary>
/// <remarks>
/// The sets are the import's knowledge alone: it decodes which set each room state names once, composes each set's
/// pictures as the cartridge loads them, and names the variant each population's enemies draw in. The game knows no set.
/// </remarks>
[NumberedIdentities("Each numbered member is the enemy graphics set of one numbered room or room state, named for it; no caller indexes the sets by number.")]
internal static class RoomEnemyGraphicsSetDefinitions
{
    /// <summary>
    /// The enemy graphics a room draws under when it loads no set (#1392, #1476): no set's pictures or colours loaded over
    /// the standard ones. A room whose population is empty loads no set and draws its spawned objects under this; its
    /// pictures are installed as the standard variants (#1599).
    /// </summary>
    internal static RoomEnemyGraphicsSetDefinition SessionStart { get; } = new(nameof(SessionStart));

    /// <summary>The enemy graphics set shared by the Draygon room once the area's boss is dead and the Mother Brain room once the main area boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition Draygon1MotherBrain2 { get; } = new(nameof(Draygon1MotherBrain2));
    /// <summary>The enemy graphics set of the East Ocean room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastOcean { get; } = new(nameof(EastOcean));
    /// <summary>The enemy graphics set of the Crateria Power Bombs room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaPowerBombs { get; } = new(nameof(CrateriaPowerBombs));
    /// <summary>The enemy graphics set of the Terminator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Terminator { get; } = new(nameof(Terminator));
    /// <summary>The enemy graphics set of the Final Missile Bombway room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition FinalMissileBombwayWhenZebesAwake { get; } = new(nameof(FinalMissileBombwayWhenZebesAwake));
    /// <summary>The enemy graphics set of the Final Missile Bombway room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition FinalMissileBombwayDefault { get; } = new(nameof(FinalMissileBombwayDefault));
    /// <summary>The enemy graphics set of the Parlor room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition ParlorWhenZebesAwake { get; } = new(nameof(ParlorWhenZebesAwake));
    /// <summary>The enemy graphics set of the Flyway room while the Zebes timebomb is not set.</summary>
    internal static RoomEnemyGraphicsSetDefinition Flyway01 { get; } = new(nameof(Flyway01));
    /// <summary>The enemy graphics set of the Pit room once Samus has the Morph Ball and Missiles.</summary>
    internal static RoomEnemyGraphicsSetDefinition PitWithMorphBallAndMissiles { get; } = new(nameof(PitWithMorphBallAndMissiles));
    /// <summary>The enemy graphics set of the Gauntlet Entrance room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GauntletEntrance { get; } = new(nameof(GauntletEntrance));
    /// <summary>The enemy graphics set of the Bomb Torizo room while the Zebes timebomb is not set.</summary>
    internal static RoomEnemyGraphicsSetDefinition BombTorizo01 { get; } = new(nameof(BombTorizo01));
    /// <summary>The enemy graphics set of the Green Pirates Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenPiratesShaft { get; } = new(nameof(GreenPiratesShaft));
    /// <summary>The enemy graphics set of the Green Brinstar Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarElev { get; } = new(nameof(GreenBrinstarElev));
    /// <summary>The enemy graphics set of the Final Missile room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition FinalMissileDefault { get; } = new(nameof(FinalMissileDefault));
    /// <summary>The enemy graphics set of the Crateria Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaMap { get; } = new(nameof(CrateriaMap));
    /// <summary>The enemy graphics set of the Final Missile room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition FinalMissileWhenZebesAwake { get; } = new(nameof(FinalMissileWhenZebesAwake));
    /// <summary>The enemy graphics set of the Climb room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition ClimbWhenZebesTimebombSet { get; } = new(nameof(ClimbWhenZebesTimebombSet));
    /// <summary>The enemy graphics set of the Crateria Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaSave { get; } = new(nameof(CrateriaSave));
    /// <summary>The enemy graphics set of the Forgotten Highway Elbow room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ForgottenHighwayElbow { get; } = new(nameof(ForgottenHighwayElbow));
    /// <summary>The enemy graphics set of the Moat room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Moat { get; } = new(nameof(Moat));
    /// <summary>The enemy graphics set of the Climb room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition ClimbDefault { get; } = new(nameof(ClimbDefault));
    /// <summary>The enemy graphics set of the West Ocean room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestOcean { get; } = new(nameof(WestOcean));
    /// <summary>The enemy graphics set of the Parlor room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition ParlorDefault { get; } = new(nameof(ParlorDefault));
    /// <summary>The enemy graphics set of the Landing Site room while the Zebes timebomb is not set.</summary>
    internal static RoomEnemyGraphicsSetDefinition LandingSite012 { get; } = new(nameof(LandingSite012));
    /// <summary>The enemy graphics set of the Crateria Super room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaSuper { get; } = new(nameof(CrateriaSuper));
    /// <summary>The enemy graphics set of the Pre Bowling room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PreBowling { get; } = new(nameof(PreBowling));
    /// <summary>The enemy graphics set of the Climb room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition ClimbWhenZebesAwake { get; } = new(nameof(ClimbWhenZebesAwake));
    /// <summary>The enemy graphics set of the Bowling Alley Path room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BowlingAlleyPath { get; } = new(nameof(BowlingAlleyPath));
    /// <summary>The enemy graphics set of the Red Brinstar Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedBinstarElev { get; } = new(nameof(RedBinstarElev));
    /// <summary>The enemy graphics set of the Gauntlet Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GauntletETank { get; } = new(nameof(GauntletETank));
    /// <summary>The enemy graphics set of the Pit room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition PitDefault { get; } = new(nameof(PitDefault));
    /// <summary>The enemy graphics set of the Crab Maze room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrabMaze { get; } = new(nameof(CrabMaze));
    /// <summary>The enemy graphics set of the Crateria Tube room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaTube { get; } = new(nameof(CrateriaTube));
    /// <summary>The enemy graphics set of the Forgotten Highway Kagos room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ForgottenHighwayKagos { get; } = new(nameof(ForgottenHighwayKagos));
    /// <summary>The enemy graphics set of the Elevator to Blue Brinstar room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ElevToBlueBrinstar { get; } = new(nameof(ElevToBlueBrinstar));
    /// <summary>The enemy graphics set of the Forgotten Highway Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ForgottenHighwayElev { get; } = new(nameof(ForgottenHighwayElev));
    /// <summary>The enemy graphics set of the Pre Map Flyway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PreMapFlyway { get; } = new(nameof(PreMapFlyway));
    /// <summary>The enemy graphics set of the Lower Mushrooms room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerMushrooms { get; } = new(nameof(LowerMushrooms));
    /// <summary>The enemy graphics set of the Landing Site room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition LandingSiteWhenZebesTimebombSet { get; } = new(nameof(LandingSiteWhenZebesTimebombSet));
    /// <summary>The enemy graphics set of the Parlor room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition ParlorWhenZebesTimebombSet { get; } = new(nameof(ParlorWhenZebesTimebombSet));
    /// <summary>The enemy graphics set of the Bomb Torizo room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition BombTorizoWhenZebesTimebombSet { get; } = new(nameof(BombTorizoWhenZebesTimebombSet));
    /// <summary>The enemy graphics set of the Flyway room once the Zebes timebomb is set.</summary>
    internal static RoomEnemyGraphicsSetDefinition FlywayWhenZebesTimebombSet { get; } = new(nameof(FlywayWhenZebesTimebombSet));
    /// <summary>The enemy graphics set of the Crateria Kihunter room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrateriaKihunter { get; } = new(nameof(CrateriaKihunter));
    /// <summary>The enemy graphics set of the Brinstar Pre Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BrinstarPreMap { get; } = new(nameof(BrinstarPreMap));
    /// <summary>The enemy graphics set of the Green Brinstar Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarMap { get; } = new(nameof(GreenBrinstarMap));
    /// <summary>The enemy graphics set of the Green Brinstar Missile Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarMissileRefill { get; } = new(nameof(GreenBrinstarMissileRefill));
    /// <summary>The enemy graphics set of the Spore Spawn Kihunters room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SporeSpawnKihunters { get; } = new(nameof(SporeSpawnKihunters));
    /// <summary>The enemy graphics set of the Etecoon Super room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EtecoonSuper { get; } = new(nameof(EtecoonSuper));
    /// <summary>The enemy graphics set of the Dachora Energy Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition DachoraEnergyRefill { get; } = new(nameof(DachoraEnergyRefill));
    /// <summary>The enemy graphics set of the Spore Spawn Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SporeSpawnFarming { get; } = new(nameof(SporeSpawnFarming));
    /// <summary>The enemy graphics set of the Statues room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Statues { get; } = new(nameof(Statues));
    /// <summary>The enemy graphics set of the Hopper Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition HopperETank { get; } = new(nameof(HopperETank));
    /// <summary>The enemy graphics set of the Beta Power Bomb room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BetaPowerBomb { get; } = new(nameof(BetaPowerBomb));
    /// <summary>The enemy graphics set of the Waterway Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WaterwayETank { get; } = new(nameof(WaterwayETank));
    /// <summary>The enemy graphics set of the Construction Zone room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition ConstructionZoneWhenZebesAwake { get; } = new(nameof(ConstructionZoneWhenZebesAwake));
    /// <summary>The enemy graphics set of the Blue Brinstar Energy Tank room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition BlueBrinstarETankWhenZebesAwake { get; } = new(nameof(BlueBrinstarETankWhenZebesAwake));
    /// <summary>The enemy graphics set of the Noob Bridge room.</summary>
    internal static RoomEnemyGraphicsSetDefinition NoobBridge { get; } = new(nameof(NoobBridge));
    /// <summary>The enemy graphics set of the Morph Ball room once Zebes is awake.</summary>
    internal static RoomEnemyGraphicsSetDefinition MorphBallWhenZebesAwake { get; } = new(nameof(MorphBallWhenZebesAwake));
    /// <summary>The enemy graphics set of the Warehouse Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseSave { get; } = new(nameof(WarehouseSave));
    /// <summary>The enemy graphics set of the Morph Ball room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition MorphBallDefault { get; } = new(nameof(MorphBallDefault));
    /// <summary>The enemy graphics set of the Warehouse Zeela room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseZeela { get; } = new(nameof(WarehouseZeela));
    /// <summary>The enemy graphics set of the Red Tower room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedTower { get; } = new(nameof(RedTower));
    /// <summary>The enemy graphics set of the Blue Brinstar Boulders room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BlueBrinstarBoulders { get; } = new(nameof(BlueBrinstarBoulders));
    /// <summary>The enemy graphics set of the Billy Mays room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BillyMays { get; } = new(nameof(BillyMays));
    /// <summary>The enemy graphics set of the Big Pink Save Room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BigPinkSaveRoom { get; } = new(nameof(BigPinkSaveRoom));
    /// <summary>The enemy graphics set of the Big Pink room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BigPink { get; } = new(nameof(BigPink));
    /// <summary>The enemy graphics set of the Etecoon Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EtecoonSave { get; } = new(nameof(EtecoonSave));
    /// <summary>The enemy graphics set of the Below Spazer room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BelowSpazer { get; } = new(nameof(BelowSpazer));
    /// <summary>The enemy graphics set of the Green Brinstar Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarSave { get; } = new(nameof(GreenBrinstarSave));
    /// <summary>The enemy graphics set of the Spazer room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Spazer { get; } = new(nameof(Spazer));
    /// <summary>The enemy graphics set of the Pink Brinstar Power Bombs room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PinkBrinstarPowerBombs { get; } = new(nameof(PinkBrinstarPowerBombs));
    /// <summary>The enemy graphics set of the Red Tower Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedTowerRefill { get; } = new(nameof(RedTowerRefill));
    /// <summary>The enemy graphics set of the Kraid Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition KraidRefill { get; } = new(nameof(KraidRefill));
    /// <summary>The enemy graphics set of the Varia Suit room.</summary>
    internal static RoomEnemyGraphicsSetDefinition VariaSuit { get; } = new(nameof(VariaSuit));
    /// <summary>The enemy graphics set of the Red Brinstar Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedBrinstarSave { get; } = new(nameof(RedBrinstarSave));
    /// <summary>The enemy graphics set of the Blue Brinstar Energy Tank room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition BlueBrinstarETankDefault { get; } = new(nameof(BlueBrinstarETankDefault));
    /// <summary>The enemy graphics set of the Green Brinstar Firefleas room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarFirefleas { get; } = new(nameof(GreenBrinstarFirefleas));
    /// <summary>The enemy graphics set of the Green Brinstar Beetoms room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarBeetoms { get; } = new(nameof(GreenBrinstarBeetoms));
    /// <summary>The enemy graphics set of the Etecoon Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EtecoonETank { get; } = new(nameof(EtecoonETank));
    /// <summary>The enemy graphics set of the Pink Brinstar Hoppers room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PinkBrinstarHoppers { get; } = new(nameof(PinkBrinstarHoppers));
    /// <summary>The enemy graphics set of the Warehouse Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseETank { get; } = new(nameof(WarehouseETank));
    /// <summary>The enemy graphics set of the Warehouse Entrance room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseEntrance { get; } = new(nameof(WarehouseEntrance));
    /// <summary>The enemy graphics set of the Warehouse Kihunter room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WarehouseKihunter { get; } = new(nameof(WarehouseKihunter));
    /// <summary>The enemy graphics set of the Green Brinstar Main Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBrinstarMainShaft { get; } = new(nameof(GreenBrinstarMainShaft));
    /// <summary>The enemy graphics set of the Spore Spawn Super room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SporeSpawnSuper { get; } = new(nameof(SporeSpawnSuper));
    /// <summary>The enemy graphics set of the Early Supers room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EarlySupers { get; } = new(nameof(EarlySupers));
    /// <summary>The enemy graphics set of the Red Brinstar Firefleas room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedBrinstarFirefleas { get; } = new(nameof(RedBrinstarFirefleas));
    /// <summary>The enemy graphics set of the Construction Zone room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition ConstructionZoneDefault { get; } = new(nameof(ConstructionZoneDefault));
    /// <summary>The enemy graphics set of the Green Hill Zone room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenHillZone { get; } = new(nameof(GreenHillZone));
    /// <summary>The enemy graphics set of the Dachora room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Dachora { get; } = new(nameof(Dachora));
    /// <summary>The enemy graphics set of the Hellway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Hellway { get; } = new(nameof(Hellway));
    /// <summary>The enemy graphics set of the First Missile room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FirstMissile { get; } = new(nameof(FirstMissile));
    /// <summary>The enemy graphics set of the Kraid room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Kraid { get; } = new(nameof(Kraid));
    /// <summary>The enemy graphics set of the Brinstar Reserve Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BrinstarReserveTank { get; } = new(nameof(BrinstarReserveTank));
    /// <summary>The enemy graphics set of the X-Ray Scope room.</summary>
    internal static RoomEnemyGraphicsSetDefinition XrayScope { get; } = new(nameof(XrayScope));
    /// <summary>The enemy graphics set of the Statues Hallway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition StatuesHallway { get; } = new(nameof(StatuesHallway));
    /// <summary>The enemy graphics set of the Alpha Power Bomb room.</summary>
    internal static RoomEnemyGraphicsSetDefinition AlphaPowerBomb { get; } = new(nameof(AlphaPowerBomb));
    /// <summary>The enemy graphics set of the Kraid Eye Door room.</summary>
    internal static RoomEnemyGraphicsSetDefinition KraidEyeDoor { get; } = new(nameof(KraidEyeDoor));
    /// <summary>The enemy graphics set of the Caterpillar room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Caterpillar { get; } = new(nameof(Caterpillar));
    /// <summary>The enemy graphics set of the Mini Kraid room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MiniKraid { get; } = new(nameof(MiniKraid));
    /// <summary>The enemy graphics set of the Spore Spawn room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SporeSpawn { get; } = new(nameof(SporeSpawn));
    /// <summary>The enemy graphics set of the Skree Boost room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SkreeBoost { get; } = new(nameof(SkreeBoost));
    /// <summary>The enemy graphics set of the Grapple Tutorial 3 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GrappleTutorial3 { get; } = new(nameof(GrappleTutorial3));
    /// <summary>The enemy graphics set of the Post-Crocomire Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocShaft { get; } = new(nameof(PostCrocShaft));
    /// <summary>The enemy graphics set of the Metal Pirates room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MetalPirates { get; } = new(nameof(MetalPirates));
    /// <summary>The enemy graphics set of the Acid Statue room.</summary>
    internal static RoomEnemyGraphicsSetDefinition AcidStatue { get; } = new(nameof(AcidStatue));
    /// <summary>The enemy graphics set of the Purple Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PurpleFarming { get; } = new(nameof(PurpleFarming));
    /// <summary>The enemy graphics set of the Ice Beam Gate room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeamGate { get; } = new(nameof(IceBeamGate));
    /// <summary>The enemy graphics set of the Spiky Acid Snakes Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpikyAcidSnakesTunnel { get; } = new(nameof(SpikyAcidSnakesTunnel));
    /// <summary>The enemy graphics set of the Red Kihunter Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedKihunterShaft { get; } = new(nameof(RedKihunterShaft));
    /// <summary>The enemy graphics set of the Fast Ripper room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FastRipper { get; } = new(nameof(FastRipper));
    /// <summary>The enemy graphics set of the Wave Beam room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WaveBeam { get; } = new(nameof(WaveBeam));
    /// <summary>The enemy graphics set of the Fast Pillars Setup room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FastPillarsSetup { get; } = new(nameof(FastPillarsSetup));
    /// <summary>The enemy graphics set of the Lower Norfair Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LowerNorfairElev { get; } = new(nameof(LowerNorfairElev));
    /// <summary>The enemy graphics set of the Lower Norfair Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LNSave { get; } = new(nameof(LNSave));
    /// <summary>The enemy graphics set of the Upper Norfair Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition UpperNorfairRefill { get; } = new(nameof(UpperNorfairRefill));
    /// <summary>The enemy graphics set of the Norfair Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition NorfairMap { get; } = new(nameof(NorfairMap));
    /// <summary>The enemy graphics set of the Mickey Mouse room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MickeyMouse { get; } = new(nameof(MickeyMouse));
    /// <summary>The enemy graphics set of the Golden Torizo Energy Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GTEnergyRefill { get; } = new(nameof(GTEnergyRefill));
    /// <summary>The enemy graphics set of the Ridley room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Ridley { get; } = new(nameof(Ridley));
    /// <summary>The enemy graphics set of the Post-Crocomire Power Bombs room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocPowerBombs { get; } = new(nameof(PostCrocPowerBombs));
    /// <summary>The enemy graphics set of the Ice Beam room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeam { get; } = new(nameof(IceBeam));
    /// <summary>The enemy graphics set of the Hi-Jump Boots room.</summary>
    internal static RoomEnemyGraphicsSetDefinition HiJumpBoots { get; } = new(nameof(HiJumpBoots));
    /// <summary>The enemy graphics set of the Post-Crocomire Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocSave { get; } = new(nameof(PostCrocSave));
    /// <summary>The enemy graphics set of the Frog Speedway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FrogSpeedway { get; } = new(nameof(FrogSpeedway));
    /// <summary>The enemy graphics set of the Rising Tide room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RisingTide { get; } = new(nameof(RisingTide));
    /// <summary>The enemy graphics set of the Post-Crocomire Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocFarming { get; } = new(nameof(PostCrocFarming));
    /// <summary>The enemy graphics set of the Worst Room in the Game.</summary>
    internal static RoomEnemyGraphicsSetDefinition WorstRoomInTheGame { get; } = new(nameof(WorstRoomInTheGame));
    /// <summary>The enemy graphics set of the Double Chamber room.</summary>
    internal static RoomEnemyGraphicsSetDefinition DoubleChamber { get; } = new(nameof(DoubleChamber));
    /// <summary>The enemy graphics set of the Grapple Beam room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GrappleBeam { get; } = new(nameof(GrappleBeam));
    /// <summary>The enemy graphics set of the Red Pirate Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedPirateShaft { get; } = new(nameof(RedPirateShaft));
    /// <summary>The enemy graphics set of the Crocomire Speedway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrocomireSpeedway { get; } = new(nameof(CrocomireSpeedway));
    /// <summary>The enemy graphics set of the Plowerhouse room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Plowerhouse { get; } = new(nameof(Plowerhouse));
    /// <summary>The enemy graphics set of the Lower Norfair Fireflea room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LNFireflea { get; } = new(nameof(LNFireflea));
    /// <summary>The enemy graphics set of the Magdollite Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MagdolliteTunnel { get; } = new(nameof(MagdolliteTunnel));
    /// <summary>The enemy graphics set of the Lava Dive room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LavaDive { get; } = new(nameof(LavaDive));
    /// <summary>The enemy graphics set of the Pillar room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Pillar { get; } = new(nameof(Pillar));
    /// <summary>The enemy graphics set of the Crocomire Escape room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrocomireEscape { get; } = new(nameof(CrocomireEscape));
    /// <summary>The enemy graphics set of the Lower Norfair Spring Ball Maze room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LNSpringBallMaze { get; } = new(nameof(LNSpringBallMaze));
    /// <summary>The enemy graphics set of the Lower Norfair Escape Power Bombs room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LNEscapePowerBombs { get; } = new(nameof(LNEscapePowerBombs));
    /// <summary>The enemy graphics set of the Purple Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PurpleShaft { get; } = new(nameof(PurpleShaft));
    /// <summary>The enemy graphics set of the Ridley Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RidleyETank { get; } = new(nameof(RidleyETank));
    /// <summary>The enemy graphics set of the Screw Attack room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ScrewAttack { get; } = new(nameof(ScrewAttack));
    /// <summary>The enemy graphics set of the Green Bubbles Missiles room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GreenBubblesMissiles { get; } = new(nameof(GreenBubblesMissiles));
    /// <summary>The enemy graphics set of the Amphitheatre room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Amphitheatre { get; } = new(nameof(Amphitheatre));
    /// <summary>The enemy graphics set of the Cathedral Entrance room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CathedralEntrance { get; } = new(nameof(CathedralEntrance));
    /// <summary>The enemy graphics set of the Main Hall room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MainHall { get; } = new(nameof(MainHall));
    /// <summary>The enemy graphics set of the Post-Crocomire Jump room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocJump { get; } = new(nameof(PostCrocJump));
    /// <summary>The enemy graphics set of the Frog Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FrogSave { get; } = new(nameof(FrogSave));
    /// <summary>The enemy graphics set of the Crocomire Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrocomireSave { get; } = new(nameof(CrocomireSave));
    /// <summary>The enemy graphics set of the Crumble Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrumbleShaft { get; } = new(nameof(CrumbleShaft));
    /// <summary>The enemy graphics set of the Wasteland room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Wasteland { get; } = new(nameof(Wasteland));
    /// <summary>The enemy graphics set of the Acid Snakes Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition AcidSnakesTunnel { get; } = new(nameof(AcidSnakesTunnel));
    /// <summary>The enemy graphics set of the Grapple Tutorial 1 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GrappleTutorial1 { get; } = new(nameof(GrappleTutorial1));
    /// <summary>The enemy graphics set of the Bubble Mountain Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BubbleMountainSave { get; } = new(nameof(BubbleMountainSave));
    /// <summary>The enemy graphics set of the Hi-Jump Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition HiJumpETank { get; } = new(nameof(HiJumpETank));
    /// <summary>The enemy graphics set of the Bubble Mountain room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BubbleMountain { get; } = new(nameof(BubbleMountain));
    /// <summary>The enemy graphics set of the Ice Beam Acid room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeamAcid { get; } = new(nameof(IceBeamAcid));
    /// <summary>The enemy graphics set of the Ice Beam Tutorial room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeamTutorial { get; } = new(nameof(IceBeamTutorial));
    /// <summary>The enemy graphics set of the Post-Crocomire Missile room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PostCrocMissile { get; } = new(nameof(PostCrocMissile));
    /// <summary>The enemy graphics set of the Cathedral room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Cathedral { get; } = new(nameof(Cathedral));
    /// <summary>The enemy graphics set of the Norfair Reserve Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition NorfairReserveTank { get; } = new(nameof(NorfairReserveTank));
    /// <summary>The enemy graphics set of the Grapple Tutorial 2 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GrappleTutorial2 { get; } = new(nameof(GrappleTutorial2));
    /// <summary>The enemy graphics set of the Ice Beam Snake room.</summary>
    internal static RoomEnemyGraphicsSetDefinition IceBeamSnake { get; } = new(nameof(IceBeamSnake));
    /// <summary>The enemy graphics set of the Golden Torizo room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GoldenTorizo { get; } = new(nameof(GoldenTorizo));
    /// <summary>The enemy graphics set of the Spiky Platforms Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpikyPlatformsTunnel { get; } = new(nameof(SpikyPlatformsTunnel));
    /// <summary>The enemy graphics set of the Speed Booster room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpeedBooster { get; } = new(nameof(SpeedBooster));
    /// <summary>The enemy graphics set of the Lower Norfair Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LNFarming { get; } = new(nameof(LNFarming));
    /// <summary>The enemy graphics set of the Single Chamber room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SingleChamber { get; } = new(nameof(SingleChamber));
    /// <summary>The enemy graphics set of the Speed Booster Hall room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpeedBoosterHall { get; } = new(nameof(SpeedBoosterHall));
    /// <summary>The enemy graphics set of the Bat Cave room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BatCave { get; } = new(nameof(BatCave));
    /// <summary>The enemy graphics set of the Kronic Boost room.</summary>
    internal static RoomEnemyGraphicsSetDefinition KronicBoost { get; } = new(nameof(KronicBoost));
    /// <summary>The enemy graphics set of the Business Center room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BusinessCenter { get; } = new(nameof(BusinessCenter));
    /// <summary>The enemy graphics set of the Three Musketeers room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ThreeMusketeers { get; } = new(nameof(ThreeMusketeers));
    /// <summary>The enemy graphics set of the Crocomire room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Crocomire { get; } = new(nameof(Crocomire));
    /// <summary>The enemy graphics set of the Lower Norfair Elevator Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition LNElevSave { get; } = new(nameof(LNElevSave));
    /// <summary>The enemy graphics set of the Volcano room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Volcano { get; } = new(nameof(Volcano));
    /// <summary>The enemy graphics set of the Upper Norfair Farming room.</summary>
    internal static RoomEnemyGraphicsSetDefinition UpperNorfairFarming { get; } = new(nameof(UpperNorfairFarming));
    /// <summary>The enemy graphics set of the Sponge Bath room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpongeBathWhenAreaBossDead { get; } = new(nameof(SpongeBathWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Wrecked Ship Energy Tank room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipETankWhenAreaBossDead { get; } = new(nameof(WreckedShipETankWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Wrecked Ship Main Shaft room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipMainShaftDefault { get; } = new(nameof(WreckedShipMainShaftDefault));
    /// <summary>The enemy graphics set of the Bowling Alley room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition BowlingAlleyDefault { get; } = new(nameof(BowlingAlleyDefault));
    /// <summary>The enemy graphics set of the Attic room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition AtticWhenAreaBossDead { get; } = new(nameof(AtticWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Basement room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition BasementWhenAreaBossDead { get; } = new(nameof(BasementWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Wrecked Ship Map room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipMapWhenAreaBossDead { get; } = new(nameof(WreckedShipMapWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Wrecked Ship West Super room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WSWestSuperDefault { get; } = new(nameof(WSWestSuperDefault));
    /// <summary>The enemy graphics set of the Spiky Death room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpikyDeathDefault { get; } = new(nameof(SpikyDeathDefault));
    /// <summary>The enemy graphics set of the Sponge Bath room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpongeBathDefault { get; } = new(nameof(SpongeBathDefault));
    /// <summary>The enemy graphics set of the Wrecked Ship Entrance room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipEntranceWhenAreaBossDead { get; } = new(nameof(WreckedShipEntranceWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Electric Death room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition ElectricDeathDefault { get; } = new(nameof(ElectricDeathDefault));
    /// <summary>The enemy graphics set of the Bowling Alley room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition BowlingAlleyWhenAreaBossDead { get; } = new(nameof(BowlingAlleyWhenAreaBossDead));
    /// <summary>The enemy graphics set of Wrecked Ship's Assembly Line room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition AssemblyLineDefault { get; } = new(nameof(AssemblyLineDefault));
    /// <summary>The enemy graphics set of the Phantoon room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition PhantoonWhenAreaBossDead { get; } = new(nameof(PhantoonWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Wrecked Ship Energy Tank room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipETankDefault { get; } = new(nameof(WreckedShipETankDefault));
    /// <summary>The enemy graphics set of the Gravity Suit room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition GravitySuitDefault { get; } = new(nameof(GravitySuitDefault));
    /// <summary>The enemy graphics set of the Wrecked Ship Map room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipMapDefault { get; } = new(nameof(WreckedShipMapDefault));
    /// <summary>The enemy graphics set of the Gravity Suit room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition GravitySuitWhenAreaBossDead { get; } = new(nameof(GravitySuitWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Basement room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition BasementDefault { get; } = new(nameof(BasementDefault));
    /// <summary>The enemy graphics set of the Wrecked Ship Entrance room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipEntranceDefault { get; } = new(nameof(WreckedShipEntranceDefault));
    /// <summary>The enemy graphics set of Wrecked Ship's Assembly Line room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition AssemblyLineWhenAreaBossDead { get; } = new(nameof(AssemblyLineWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Wrecked Ship Save room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipSaveWhenAreaBossDead { get; } = new(nameof(WreckedShipSaveWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Wrecked Ship West Super room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WSWestSuperWhenAreaBossDead { get; } = new(nameof(WSWestSuperWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Attic room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition AtticDefault { get; } = new(nameof(AtticDefault));
    /// <summary>The enemy graphics set of the Wrecked Ship East Super room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WSEastSuperDefault { get; } = new(nameof(WSEastSuperDefault));
    /// <summary>The enemy graphics set of the Wrecked Ship Save room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipSaveDefault { get; } = new(nameof(WreckedShipSaveDefault));
    /// <summary>The enemy graphics set of the Electric Death room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition ElectricDeathWhenAreaBossDead { get; } = new(nameof(ElectricDeathWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Spiky Death room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpikyDeathWhenAreaBossDead { get; } = new(nameof(SpikyDeathWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Wrecked Ship East Super room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WSEastSuperWhenAreaBossDead { get; } = new(nameof(WSEastSuperWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Phantoon room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition PhantoonDefault { get; } = new(nameof(PhantoonDefault));
    /// <summary>The enemy graphics set of the Wrecked Ship Main Shaft room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition WreckedShipMainShaftWhenAreaBossDead { get; } = new(nameof(WreckedShipMainShaftWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Mt. Everest room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MtEverest { get; } = new(nameof(MtEverest));
    /// <summary>The enemy graphics set of the Halfie Climb room.</summary>
    internal static RoomEnemyGraphicsSetDefinition HalfieClimb { get; } = new(nameof(HalfieClimb));
    /// <summary>The enemy graphics set of the Watering Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WateringHole { get; } = new(nameof(WateringHole));
    /// <summary>The enemy graphics set of the Crab Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrabShaft { get; } = new(nameof(CrabShaft));
    /// <summary>The enemy graphics set of the Maridia Elevator room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MaridiaElev { get; } = new(nameof(MaridiaElev));
    /// <summary>The enemy graphics set of the Colosseum room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Colosseum { get; } = new(nameof(Colosseum));
    /// <summary>The enemy graphics set of the Aqueduct Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition AqueductSave { get; } = new(nameof(AqueductSave));
    /// <summary>The enemy graphics set of the Precious room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ThePrecious { get; } = new(nameof(ThePrecious));
    /// <summary>The enemy graphics set of the Botwoon Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BotwoonETank { get; } = new(nameof(BotwoonETank));
    /// <summary>The enemy graphics set of the Draygon Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition DraygonSave { get; } = new(nameof(DraygonSave));
    /// <summary>The enemy graphics set of the Maridia Missile Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MaridiaMissileRefill { get; } = new(nameof(MaridiaMissileRefill));
    /// <summary>The enemy graphics set of the Plasma Beach Quicksand room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PlasmaBeachQuicksand { get; } = new(nameof(PlasmaBeachQuicksand));
    /// <summary>The enemy graphics set of the Botwoon Quicksand room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BotwoonQuicksand { get; } = new(nameof(BotwoonQuicksand));
    /// <summary>The enemy graphics set of the Shaktool room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Shaktool { get; } = new(nameof(Shaktool));
    /// <summary>The enemy graphics set of the Draygon room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition DraygonDefault { get; } = new(nameof(DraygonDefault));
    /// <summary>The enemy graphics set of the Aqueduct room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Aqueduct { get; } = new(nameof(Aqueduct));
    /// <summary>The enemy graphics set of the Red Fish room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RedFish { get; } = new(nameof(RedFish));
    /// <summary>The enemy graphics set of the Plasma room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Plasma { get; } = new(nameof(Plasma));
    /// <summary>The enemy graphics set of the Space Jump room.</summary>
    internal static RoomEnemyGraphicsSetDefinition SpaceJump { get; } = new(nameof(SpaceJump));
    /// <summary>The enemy graphics set of the Pants room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Pants { get; } = new(nameof(Pants));
    /// <summary>The enemy graphics set of the Glass Tunnel Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GlassTunnelSave { get; } = new(nameof(GlassTunnelSave));
    /// <summary>The enemy graphics set of the Glass Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition GlassTunnel { get; } = new(nameof(GlassTunnel));
    /// <summary>The enemy graphics set of the West Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestTunnel { get; } = new(nameof(WestTunnel));
    /// <summary>The enemy graphics set of the East Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastTunnel { get; } = new(nameof(EastTunnel));
    /// <summary>The enemy graphics set of the Mama Turtle room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MamaTurtle { get; } = new(nameof(MamaTurtle));
    /// <summary>The enemy graphics set of the Crab Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrabTunnel { get; } = new(nameof(CrabTunnel));
    /// <summary>The enemy graphics set of the Northwest Maridia Bug room.</summary>
    internal static RoomEnemyGraphicsSetDefinition NWestMaridiaBug { get; } = new(nameof(NWestMaridiaBug));
    /// <summary>The enemy graphics set of the Pseudo Plasma Spark room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PseudoPlasmaSpark { get; } = new(nameof(PseudoPlasmaSpark));
    /// <summary>The enemy graphics set of the West Sand Hall Tunnel room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestSandHallTunnel { get; } = new(nameof(WestSandHallTunnel));
    /// <summary>The enemy graphics set of the Plasma Tutorial room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PlasmaTutorial { get; } = new(nameof(PlasmaTutorial));
    /// <summary>The enemy graphics set of the Thread the Needle room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ThreadTheNeedle { get; } = new(nameof(ThreadTheNeedle));
    /// <summary>The enemy graphics set of the Plasma Spark room.</summary>
    internal static RoomEnemyGraphicsSetDefinition PlasmaSpark { get; } = new(nameof(PlasmaSpark));
    /// <summary>The enemy graphics set of the Toilet room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Toilet { get; } = new(nameof(Toilet));
    /// <summary>The enemy graphics set of the West Sand Hall room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestSandHall { get; } = new(nameof(WestSandHall));
    /// <summary>The enemy graphics set of the Oasis room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Oasis { get; } = new(nameof(Oasis));
    /// <summary>The enemy graphics set of the East Sand Hall room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastSandHall { get; } = new(nameof(EastSandHall));
    /// <summary>The enemy graphics set of the West Aqueduct Quicksand room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestAqueductQuicksand { get; } = new(nameof(WestAqueductQuicksand));
    /// <summary>The enemy graphics set of the East Aqueduct Quicksand room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastAqueductQuicksand { get; } = new(nameof(EastAqueductQuicksand));
    /// <summary>The enemy graphics set of the Butterfly room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Butterfly { get; } = new(nameof(Butterfly));
    /// <summary>The enemy graphics set of the Botwoon Hallway room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BotwoonHallway { get; } = new(nameof(BotwoonHallway));
    /// <summary>The enemy graphics set of the East Pants room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastPants { get; } = new(nameof(EastPants));
    /// <summary>The enemy graphics set of the Springball room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Springball { get; } = new(nameof(Springball));
    /// <summary>The enemy graphics set of the Below Botwoon Energy Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BelowBotwoonETank { get; } = new(nameof(BelowBotwoonETank));
    /// <summary>The enemy graphics set of the Kassiuz room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Kassiuz { get; } = new(nameof(Kassiuz));
    /// <summary>The enemy graphics set of the Forgotten Highway Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition ForgottenHighwaySave { get; } = new(nameof(ForgottenHighwaySave));
    /// <summary>The enemy graphics set of the Maridia Energy Refill room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MaridiaEnergyRefill { get; } = new(nameof(MaridiaEnergyRefill));
    /// <summary>The enemy graphics set of the Maridia Map room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MaridiaMap { get; } = new(nameof(MaridiaMap));
    /// <summary>The enemy graphics set of the Crab Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition CrabHole { get; } = new(nameof(CrabHole));
    /// <summary>The enemy graphics set of the Botwoon room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Botwoon { get; } = new(nameof(Botwoon));
    /// <summary>The enemy graphics set of the Main Street room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MainStreet { get; } = new(nameof(MainStreet));
    /// <summary>The enemy graphics set of the Bug Sand Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BugSandHole { get; } = new(nameof(BugSandHole));
    /// <summary>The enemy graphics set of the East Sand Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastSandHole { get; } = new(nameof(EastSandHole));
    /// <summary>The enemy graphics set of the West Sand Hole room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestSandHole { get; } = new(nameof(WestSandHole));
    /// <summary>The enemy graphics set of the West Cactus Alley room.</summary>
    internal static RoomEnemyGraphicsSetDefinition WestCactusAlley { get; } = new(nameof(WestCactusAlley));
    /// <summary>The enemy graphics set of the East Cactus Alley room.</summary>
    internal static RoomEnemyGraphicsSetDefinition EastCactusAlley { get; } = new(nameof(EastCactusAlley));
    /// <summary>The enemy graphics set of the Fish Tank room.</summary>
    internal static RoomEnemyGraphicsSetDefinition FishTank { get; } = new(nameof(FishTank));
    /// <summary>The enemy graphics set of the Tourian Escape 4 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEscape4 { get; } = new(nameof(TourianEscape4));
    /// <summary>The enemy graphics set of the Mother Brain Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition MotherBrainSave { get; } = new(nameof(MotherBrainSave));
    /// <summary>The enemy graphics set of the Metroids 1 room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids1Default { get; } = new(nameof(Metroids1Default));
    /// <summary>The enemy graphics set of the Dust Torizo room.</summary>
    internal static RoomEnemyGraphicsSetDefinition DustTorizo { get; } = new(nameof(DustTorizo));
    /// <summary>The enemy graphics set of the Big Boy room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BigBoy { get; } = new(nameof(BigBoy));
    /// <summary>The enemy graphics set of the Mother Brain room in its first two states.</summary>
    internal static RoomEnemyGraphicsSetDefinition MotherBrain01 { get; } = new(nameof(MotherBrain01));
    /// <summary>The enemy graphics set of the Tourian Eye Door room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEyeDoor { get; } = new(nameof(TourianEyeDoor));
    /// <summary>The enemy graphics set of the Blue Hopper room.</summary>
    internal static RoomEnemyGraphicsSetDefinition BlueHopper { get; } = new(nameof(BlueHopper));
    /// <summary>The enemy graphics set of the Tourian Escape 1 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEscape1 { get; } = new(nameof(TourianEscape1));
    /// <summary>The enemy graphics set of the Tourian First room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianFirst { get; } = new(nameof(TourianFirst));
    /// <summary>The enemy graphics set of the Metroids 2 room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids2Default { get; } = new(nameof(Metroids2Default));
    /// <summary>The enemy graphics set of the Metroids 4 room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids4Default { get; } = new(nameof(Metroids4Default));
    /// <summary>The enemy graphics set of the Metroids 1 room once the first Metroid hall is cleared.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids1WhenFirstMetroidHallCleared { get; } = new(nameof(Metroids1WhenFirstMetroidHallCleared));
    /// <summary>The enemy graphics set of the Metroids 2 room once the first Metroid shaft is cleared.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids2WhenFirstMetroidShaftCleared { get; } = new(nameof(Metroids2WhenFirstMetroidShaftCleared));
    /// <summary>The enemy graphics set of the Tourian Escape 2 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEscape2 { get; } = new(nameof(TourianEscape2));
    /// <summary>The enemy graphics set of the Metroids 3 room once the second Metroid hall is cleared.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids3WhenSecondMetroidHallCleared { get; } = new(nameof(Metroids3WhenSecondMetroidHallCleared));
    /// <summary>The enemy graphics set of the Metroids 4 room once the second Metroid shaft is cleared.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids4WhenSecondMetroidShaftCleared { get; } = new(nameof(Metroids4WhenSecondMetroidShaftCleared));
    /// <summary>The enemy graphics set of the Tourian Escape 3 room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianEscape3 { get; } = new(nameof(TourianEscape3));
    /// <summary>The enemy graphics set of the Seaweed room.</summary>
    internal static RoomEnemyGraphicsSetDefinition Seaweed { get; } = new(nameof(Seaweed));
    /// <summary>The enemy graphics set of the Tourian Recharge room.</summary>
    internal static RoomEnemyGraphicsSetDefinition TourianRecharge { get; } = new(nameof(TourianRecharge));
    /// <summary>The enemy graphics set of the Upper Tourian Save room.</summary>
    internal static RoomEnemyGraphicsSetDefinition UpperTourianSave { get; } = new(nameof(UpperTourianSave));
    /// <summary>The enemy graphics set of the Metroids 3 room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Metroids3Default { get; } = new(nameof(Metroids3Default));
    /// <summary>The enemy graphics set of the Rinka Shaft room.</summary>
    internal static RoomEnemyGraphicsSetDefinition RinkaShaft { get; } = new(nameof(RinkaShaft));
    /// <summary>The enemy graphics set of the Ceres Elevator room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresElevDefault { get; } = new(nameof(CeresElevDefault));
    /// <summary>The enemy graphics set of the Falling Tile room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition FallingTileDefault { get; } = new(nameof(FallingTileDefault));
    /// <summary>The enemy graphics set of the Magnet Stairs room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition MagnetStairsDefault { get; } = new(nameof(MagnetStairsDefault));
    /// <summary>The enemy graphics set of the Dead Scientist room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition DeadScientistDefault { get; } = new(nameof(DeadScientistDefault));
    /// <summary>The enemy graphics set of the Room 58 Escape in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition Room58EscapeDefault { get; } = new(nameof(Room58EscapeDefault));
    /// <summary>The enemy graphics set of the Ceres Ridley room in its default state.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresRidleyDefault { get; } = new(nameof(CeresRidleyDefault));
    /// <summary>The enemy graphics set of the Ceres Elevator room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresElevWhenAreaBossDead { get; } = new(nameof(CeresElevWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Falling Tile room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition FallingTileWhenAreaBossDead { get; } = new(nameof(FallingTileWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Magnet Stairs room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition MagnetStairsWhenAreaBossDead { get; } = new(nameof(MagnetStairsWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Dead Scientist room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition DeadScientistWhenAreaBossDead { get; } = new(nameof(DeadScientistWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Room 58 Escape once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition Room58EscapeWhenAreaBossDead { get; } = new(nameof(Room58EscapeWhenAreaBossDead));
    /// <summary>The enemy graphics set of the Ceres Ridley room once the area's boss is dead.</summary>
    internal static RoomEnemyGraphicsSetDefinition CeresRidleyWhenAreaBossDead { get; } = new(nameof(CeresRidleyWhenAreaBossDead));

}
