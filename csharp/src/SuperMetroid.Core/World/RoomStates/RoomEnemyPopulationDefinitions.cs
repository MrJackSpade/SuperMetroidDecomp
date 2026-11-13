using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>Application-owned ordered enemy placements selected by all retail room states, grouped by the area of the rooms that load them.</summary>
/// <remarks>
/// The 302 lists and 1,658 records are the populations the 323 compiled room states select. Record order governs
/// linked slot allocation; each list states the kills that clear its room as its death quota. This is gameplay data,
/// not an editable visual asset.
/// </remarks>
[NumberedIdentities("Each numbered member is the enemy population of one numbered room or room state, named for it; no caller indexes the populations by number.")]
public static class RoomEnemyPopulationDefinitions
{
    /// <summary>Retail populations in their authored list order, checked complete before gameplay can use them.</summary>
    internal static readonly RoomEnemyPopulationDefinition[] Definitions = RoomEnemyPopulations.Complete(
    [
        ..CrateriaRoomEnemyPopulationDefinitions.Build(),
        ..BrinstarRoomEnemyPopulationDefinitions.Build(),
        ..NorfairRoomEnemyPopulationDefinitions.Build(),
        ..WreckedShipRoomEnemyPopulationDefinitions.Build(),
        ..MaridiaRoomEnemyPopulationDefinitions.Build(),
        ..TourianRoomEnemyPopulationDefinitions.Build(),
        ..CeresRoomEnemyPopulationDefinitions.Build(),
    ], ListCount, RecordCount);

    /// <summary>Number of distinct populations selected by the compiled retail room states, including empty lists.</summary>
    public const int ListCount = 302;
    /// <summary>Total placement records across the distinct compiled lists, excluding terminators and without counting repeated references from room states.</summary>
    public const int RecordCount = 1658;

    /// <summary>Each retail population by name.</summary>
    private static readonly Dictionary<string, RoomEnemyPopulationDefinition> ByName = Definitions.ToDictionary(list => list.Name);

    /// <summary>The compiled population named <paramref name="name"/>.</summary>
    private static RoomEnemyPopulationDefinition Find(string name) => ByName[name];

    /// <summary>The enemy population shared by the Draygon room once the area's boss is dead and the Mother Brain room once the main area boss is dead.</summary>
    public static RoomEnemyPopulationDefinition DraygonWhenAreaBossDeadOrMotherBrainWhenMainAreaBossDead { get; } = Find(nameof(DraygonWhenAreaBossDeadOrMotherBrainWhenMainAreaBossDead));
    /// <summary>The enemy population of the East Ocean room.</summary>
    public static RoomEnemyPopulationDefinition EastOcean { get; } = Find(nameof(EastOcean));
    /// <summary>The enemy population of the Crateria Power Bombs room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaPowerBombs { get; } = Find(nameof(CrateriaPowerBombs));
    /// <summary>The enemy population of the Terminator room.</summary>
    public static RoomEnemyPopulationDefinition Terminator { get; } = Find(nameof(Terminator));
    /// <summary>The enemy population of the Final Missile Bombway room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileBombwayWhenZebesAwake { get; } = Find(nameof(FinalMissileBombwayWhenZebesAwake));
    /// <summary>The enemy population of the Final Missile Bombway room in its default state.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileBombwayDefault { get; } = Find(nameof(FinalMissileBombwayDefault));
    /// <summary>The enemy population of the Parlor room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition ParlorWhenZebesAwake { get; } = Find(nameof(ParlorWhenZebesAwake));
    /// <summary>The enemy population of the Flyway room while the Zebes timebomb is not set.</summary>
    public static RoomEnemyPopulationDefinition Flyway01 { get; } = Find(nameof(Flyway01));
    /// <summary>The enemy population of the Pit room once Samus has the Morph Ball and Missiles.</summary>
    public static RoomEnemyPopulationDefinition PitWithMorphBallAndMissiles { get; } = Find(nameof(PitWithMorphBallAndMissiles));
    /// <summary>The enemy population of the Gauntlet Entrance room.</summary>
    public static RoomEnemyPopulationDefinition GauntletEntrance { get; } = Find(nameof(GauntletEntrance));
    /// <summary>The enemy population of the Bomb Torizo room while the Zebes timebomb is not set.</summary>
    public static RoomEnemyPopulationDefinition BombTorizo01 { get; } = Find(nameof(BombTorizo01));
    /// <summary>The enemy population of the Green Pirates Shaft room.</summary>
    public static RoomEnemyPopulationDefinition GreenPiratesShaft { get; } = Find(nameof(GreenPiratesShaft));
    /// <summary>The enemy population of the Green Brinstar Elevator room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarElev { get; } = Find(nameof(GreenBrinstarElev));
    /// <summary>The enemy population of the Final Missile room in its default state.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileDefault { get; } = Find(nameof(FinalMissileDefault));
    /// <summary>The enemy population of the Crateria Map room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaMap { get; } = Find(nameof(CrateriaMap));
    /// <summary>The enemy population of the Final Missile room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileWhenZebesAwake { get; } = Find(nameof(FinalMissileWhenZebesAwake));
    /// <summary>The enemy population of the Climb room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition ClimbWhenZebesTimebombSet { get; } = Find(nameof(ClimbWhenZebesTimebombSet));
    /// <summary>The enemy population of the Crateria Save room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaSave { get; } = Find(nameof(CrateriaSave));
    /// <summary>The enemy population of the Forgotten Highway Elbow room.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayElbow { get; } = Find(nameof(ForgottenHighwayElbow));
    /// <summary>The enemy population of the Moat room.</summary>
    public static RoomEnemyPopulationDefinition Moat { get; } = Find(nameof(Moat));
    /// <summary>The enemy population of the Climb room in its default state.</summary>
    public static RoomEnemyPopulationDefinition ClimbDefault { get; } = Find(nameof(ClimbDefault));
    /// <summary>The enemy population of the West Ocean room.</summary>
    public static RoomEnemyPopulationDefinition WestOcean { get; } = Find(nameof(WestOcean));
    /// <summary>The enemy population of the Parlor room in its default state.</summary>
    public static RoomEnemyPopulationDefinition ParlorDefault { get; } = Find(nameof(ParlorDefault));
    /// <summary>The enemy population of the Landing Site room while the Zebes timebomb is not set.</summary>
    public static RoomEnemyPopulationDefinition LandingSite012 { get; } = Find(nameof(LandingSite012));
    /// <summary>The enemy population of the Crateria Super room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaSuper { get; } = Find(nameof(CrateriaSuper));
    /// <summary>The enemy population of the Pre Bowling room.</summary>
    public static RoomEnemyPopulationDefinition PreBowling { get; } = Find(nameof(PreBowling));
    /// <summary>The enemy population of the Climb room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition ClimbWhenZebesAwake { get; } = Find(nameof(ClimbWhenZebesAwake));
    /// <summary>The enemy population of the Bowling Alley Path room.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlleyPath { get; } = Find(nameof(BowlingAlleyPath));
    /// <summary>The enemy population of the Red Brinstar Elevator room.</summary>
    public static RoomEnemyPopulationDefinition RedBinstarElev { get; } = Find(nameof(RedBinstarElev));
    /// <summary>The enemy population of the Gauntlet Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition GauntletETank { get; } = Find(nameof(GauntletETank));
    /// <summary>The enemy population of the Pit room in its default state.</summary>
    public static RoomEnemyPopulationDefinition PitDefault { get; } = Find(nameof(PitDefault));
    /// <summary>The enemy population of the Crab Maze room.</summary>
    public static RoomEnemyPopulationDefinition CrabMaze { get; } = Find(nameof(CrabMaze));
    /// <summary>The enemy population of the Crateria Tube room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaTube { get; } = Find(nameof(CrateriaTube));
    /// <summary>The enemy population of the Forgotten Highway Kagos room.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayKagos { get; } = Find(nameof(ForgottenHighwayKagos));
    /// <summary>The enemy population of the Elevator to Blue Brinstar room.</summary>
    public static RoomEnemyPopulationDefinition ElevToBlueBrinstar { get; } = Find(nameof(ElevToBlueBrinstar));
    /// <summary>The enemy population of the Forgotten Highway Elevator room.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayElev { get; } = Find(nameof(ForgottenHighwayElev));
    /// <summary>The enemy population of the Pre Map Flyway room.</summary>
    public static RoomEnemyPopulationDefinition PreMapFlyway { get; } = Find(nameof(PreMapFlyway));
    /// <summary>The enemy population of the Lower Mushrooms room.</summary>
    public static RoomEnemyPopulationDefinition LowerMushrooms { get; } = Find(nameof(LowerMushrooms));
    /// <summary>The enemy population of the Landing Site room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition LandingSiteWhenZebesTimebombSet { get; } = Find(nameof(LandingSiteWhenZebesTimebombSet));
    /// <summary>The enemy population of the Parlor room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition ParlorWhenZebesTimebombSet { get; } = Find(nameof(ParlorWhenZebesTimebombSet));
    /// <summary>The enemy population of the Bomb Torizo room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition BombTorizoWhenZebesTimebombSet { get; } = Find(nameof(BombTorizoWhenZebesTimebombSet));
    /// <summary>The enemy population of the Flyway room once the Zebes timebomb is set.</summary>
    public static RoomEnemyPopulationDefinition FlywayWhenZebesTimebombSet { get; } = Find(nameof(FlywayWhenZebesTimebombSet));
    /// <summary>The enemy population of the Crateria Kihunter room.</summary>
    public static RoomEnemyPopulationDefinition CrateriaKihunter { get; } = Find(nameof(CrateriaKihunter));
    /// <summary>The enemy population of the Brinstar Pre Map room.</summary>
    public static RoomEnemyPopulationDefinition BrinstarPreMap { get; } = Find(nameof(BrinstarPreMap));
    /// <summary>The enemy population of the Green Brinstar Map room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMap { get; } = Find(nameof(GreenBrinstarMap));
    /// <summary>The enemy population of the Green Brinstar Missile Refill room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMissileRefill { get; } = Find(nameof(GreenBrinstarMissileRefill));
    /// <summary>The enemy population of the Spore Spawn Kihunters room.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnKihunters { get; } = Find(nameof(SporeSpawnKihunters));
    /// <summary>The enemy population of the Etecoon Super room.</summary>
    public static RoomEnemyPopulationDefinition EtecoonSuper { get; } = Find(nameof(EtecoonSuper));
    /// <summary>The enemy population of the Dachora Energy Refill room.</summary>
    public static RoomEnemyPopulationDefinition DachoraEnergyRefill { get; } = Find(nameof(DachoraEnergyRefill));
    /// <summary>The enemy population of the Spore Spawn Farming room.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnFarming { get; } = Find(nameof(SporeSpawnFarming));
    /// <summary>The enemy population of the Statues room.</summary>
    public static RoomEnemyPopulationDefinition Statues { get; } = Find(nameof(Statues));
    /// <summary>The enemy population of the Hopper Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition HopperETank { get; } = Find(nameof(HopperETank));
    /// <summary>The enemy population of the Beta Power Bomb room.</summary>
    public static RoomEnemyPopulationDefinition BetaPowerBomb { get; } = Find(nameof(BetaPowerBomb));
    /// <summary>The enemy population of the Waterway Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition WaterwayETank { get; } = Find(nameof(WaterwayETank));
    /// <summary>The enemy population of the Construction Zone room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition ConstructionZoneWhenZebesAwake { get; } = Find(nameof(ConstructionZoneWhenZebesAwake));
    /// <summary>The enemy population of the Blue Brinstar Energy Tank room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarETankWhenZebesAwake { get; } = Find(nameof(BlueBrinstarETankWhenZebesAwake));
    /// <summary>The enemy population of the Noob Bridge room.</summary>
    public static RoomEnemyPopulationDefinition NoobBridge { get; } = Find(nameof(NoobBridge));
    /// <summary>The enemy population of the Morph Ball room once Zebes is awake.</summary>
    public static RoomEnemyPopulationDefinition MorphBallWhenZebesAwake { get; } = Find(nameof(MorphBallWhenZebesAwake));
    /// <summary>The enemy population of the Warehouse Save room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseSave { get; } = Find(nameof(WarehouseSave));
    /// <summary>The enemy population of the Morph Ball room in its default state.</summary>
    public static RoomEnemyPopulationDefinition MorphBallDefault { get; } = Find(nameof(MorphBallDefault));
    /// <summary>The enemy population of the Warehouse Zeela room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseZeela { get; } = Find(nameof(WarehouseZeela));
    /// <summary>The enemy population of the Red Tower room.</summary>
    public static RoomEnemyPopulationDefinition RedTower { get; } = Find(nameof(RedTower));
    /// <summary>The enemy population of the Blue Brinstar Boulders room.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarBoulders { get; } = Find(nameof(BlueBrinstarBoulders));
    /// <summary>The enemy population of the Billy Mays room.</summary>
    public static RoomEnemyPopulationDefinition BillyMays { get; } = Find(nameof(BillyMays));
    /// <summary>The enemy population of the Big Pink Save Room.</summary>
    public static RoomEnemyPopulationDefinition BigPinkSaveRoom { get; } = Find(nameof(BigPinkSaveRoom));
    /// <summary>The enemy population of the Big Pink room.</summary>
    public static RoomEnemyPopulationDefinition BigPink { get; } = Find(nameof(BigPink));
    /// <summary>The enemy population of the Etecoon Save room.</summary>
    public static RoomEnemyPopulationDefinition EtecoonSave { get; } = Find(nameof(EtecoonSave));
    /// <summary>The enemy population of the Below Spazer room.</summary>
    public static RoomEnemyPopulationDefinition BelowSpazer { get; } = Find(nameof(BelowSpazer));
    /// <summary>The enemy population of the Green Brinstar Save room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarSave { get; } = Find(nameof(GreenBrinstarSave));
    /// <summary>The enemy population of the Spazer room.</summary>
    public static RoomEnemyPopulationDefinition Spazer { get; } = Find(nameof(Spazer));
    /// <summary>The enemy population of the Pink Brinstar Power Bombs room.</summary>
    public static RoomEnemyPopulationDefinition PinkBrinstarPowerBombs { get; } = Find(nameof(PinkBrinstarPowerBombs));
    /// <summary>The enemy population of the Red Tower Refill room.</summary>
    public static RoomEnemyPopulationDefinition RedTowerRefill { get; } = Find(nameof(RedTowerRefill));
    /// <summary>The enemy population of the Kraid Refill room.</summary>
    public static RoomEnemyPopulationDefinition KraidRefill { get; } = Find(nameof(KraidRefill));
    /// <summary>The enemy population of the Varia Suit room.</summary>
    public static RoomEnemyPopulationDefinition VariaSuit { get; } = Find(nameof(VariaSuit));
    /// <summary>The enemy population of the Red Brinstar Save room.</summary>
    public static RoomEnemyPopulationDefinition RedBrinstarSave { get; } = Find(nameof(RedBrinstarSave));
    /// <summary>The enemy population of the Blue Brinstar Energy Tank room in its default state.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarETankDefault { get; } = Find(nameof(BlueBrinstarETankDefault));
    /// <summary>The enemy population of the Green Brinstar Firefleas room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarFirefleas { get; } = Find(nameof(GreenBrinstarFirefleas));
    /// <summary>The enemy population of the Green Brinstar Beetoms room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarBeetoms { get; } = Find(nameof(GreenBrinstarBeetoms));
    /// <summary>The enemy population of the Etecoon Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition EtecoonETank { get; } = Find(nameof(EtecoonETank));
    /// <summary>The enemy population of the Pink Brinstar Hoppers room.</summary>
    public static RoomEnemyPopulationDefinition PinkBrinstarHoppers { get; } = Find(nameof(PinkBrinstarHoppers));
    /// <summary>The enemy population of the Warehouse Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseETank { get; } = Find(nameof(WarehouseETank));
    /// <summary>The enemy population of the Warehouse Entrance room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseEntrance { get; } = Find(nameof(WarehouseEntrance));
    /// <summary>The enemy population of the Warehouse Kihunter room.</summary>
    public static RoomEnemyPopulationDefinition WarehouseKihunter { get; } = Find(nameof(WarehouseKihunter));
    /// <summary>The enemy population of the Green Brinstar Main Shaft room.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMainShaft { get; } = Find(nameof(GreenBrinstarMainShaft));
    /// <summary>The enemy population of the Spore Spawn Super room.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnSuper { get; } = Find(nameof(SporeSpawnSuper));
    /// <summary>The enemy population of the Early Supers room.</summary>
    public static RoomEnemyPopulationDefinition EarlySupers { get; } = Find(nameof(EarlySupers));
    /// <summary>The enemy population of the Red Brinstar Firefleas room.</summary>
    public static RoomEnemyPopulationDefinition RedBrinstarFirefleas { get; } = Find(nameof(RedBrinstarFirefleas));
    /// <summary>The enemy population of the Construction Zone room in its default state.</summary>
    public static RoomEnemyPopulationDefinition ConstructionZoneDefault { get; } = Find(nameof(ConstructionZoneDefault));
    /// <summary>The enemy population of the Green Hill Zone room.</summary>
    public static RoomEnemyPopulationDefinition GreenHillZone { get; } = Find(nameof(GreenHillZone));
    /// <summary>The enemy population of the Dachora room.</summary>
    public static RoomEnemyPopulationDefinition Dachora { get; } = Find(nameof(Dachora));
    /// <summary>The enemy population of the Hellway room.</summary>
    public static RoomEnemyPopulationDefinition Hellway { get; } = Find(nameof(Hellway));
    /// <summary>The enemy population of the First Missile room.</summary>
    public static RoomEnemyPopulationDefinition FirstMissile { get; } = Find(nameof(FirstMissile));
    /// <summary>The enemy population of the Kraid room.</summary>
    public static RoomEnemyPopulationDefinition Kraid { get; } = Find(nameof(Kraid));
    /// <summary>The enemy population of the Brinstar Reserve Tank room.</summary>
    public static RoomEnemyPopulationDefinition BrinstarReserveTank { get; } = Find(nameof(BrinstarReserveTank));
    /// <summary>The enemy population of the X-Ray Scope room.</summary>
    public static RoomEnemyPopulationDefinition XrayScope { get; } = Find(nameof(XrayScope));
    /// <summary>The enemy population of the Statues Hallway room.</summary>
    public static RoomEnemyPopulationDefinition StatuesHallway { get; } = Find(nameof(StatuesHallway));
    /// <summary>The enemy population of the Alpha Power Bomb room.</summary>
    public static RoomEnemyPopulationDefinition AlphaPowerBomb { get; } = Find(nameof(AlphaPowerBomb));
    /// <summary>The enemy population of the Kraid Eye Door room.</summary>
    public static RoomEnemyPopulationDefinition KraidEyeDoor { get; } = Find(nameof(KraidEyeDoor));
    /// <summary>The enemy population of the Caterpillar room.</summary>
    public static RoomEnemyPopulationDefinition Caterpillar { get; } = Find(nameof(Caterpillar));
    /// <summary>The enemy population of the Mini Kraid room.</summary>
    public static RoomEnemyPopulationDefinition MiniKraid { get; } = Find(nameof(MiniKraid));
    /// <summary>The enemy population of the Spore Spawn room.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawn { get; } = Find(nameof(SporeSpawn));
    /// <summary>The enemy population of the Skree Boost room.</summary>
    public static RoomEnemyPopulationDefinition SkreeBoost { get; } = Find(nameof(SkreeBoost));
    /// <summary>The enemy population of the Grapple Tutorial 3 room.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial3 { get; } = Find(nameof(GrappleTutorial3));
    /// <summary>The enemy population of the Post-Crocomire Shaft room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocShaft { get; } = Find(nameof(PostCrocShaft));
    /// <summary>The enemy population of the Metal Pirates room.</summary>
    public static RoomEnemyPopulationDefinition MetalPirates { get; } = Find(nameof(MetalPirates));
    /// <summary>The enemy population of the Acid Statue room.</summary>
    public static RoomEnemyPopulationDefinition AcidStatue { get; } = Find(nameof(AcidStatue));
    /// <summary>The enemy population of the Purple Farming room.</summary>
    public static RoomEnemyPopulationDefinition PurpleFarming { get; } = Find(nameof(PurpleFarming));
    /// <summary>The enemy population of the Ice Beam Gate room.</summary>
    public static RoomEnemyPopulationDefinition IceBeamGate { get; } = Find(nameof(IceBeamGate));
    /// <summary>The enemy population of the Spiky Acid Snakes Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition SpikyAcidSnakesTunnel { get; } = Find(nameof(SpikyAcidSnakesTunnel));
    /// <summary>The enemy population of the Red Kihunter Shaft room.</summary>
    public static RoomEnemyPopulationDefinition RedKihunterShaft { get; } = Find(nameof(RedKihunterShaft));
    /// <summary>The enemy population of the Fast Ripper room.</summary>
    public static RoomEnemyPopulationDefinition FastRipper { get; } = Find(nameof(FastRipper));
    /// <summary>The enemy population of the Wave Beam room.</summary>
    public static RoomEnemyPopulationDefinition WaveBeam { get; } = Find(nameof(WaveBeam));
    /// <summary>The enemy population of the Fast Pillars Setup room.</summary>
    public static RoomEnemyPopulationDefinition FastPillarsSetup { get; } = Find(nameof(FastPillarsSetup));
    /// <summary>The enemy population of the Lower Norfair Elevator room.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairElev { get; } = Find(nameof(LowerNorfairElev));
    /// <summary>The enemy population of the Lower Norfair Save room.</summary>
    public static RoomEnemyPopulationDefinition LNSave { get; } = Find(nameof(LNSave));
    /// <summary>The enemy population of the Upper Norfair Refill room.</summary>
    public static RoomEnemyPopulationDefinition UpperNorfairRefill { get; } = Find(nameof(UpperNorfairRefill));
    /// <summary>The enemy population of the Norfair Map room.</summary>
    public static RoomEnemyPopulationDefinition NorfairMap { get; } = Find(nameof(NorfairMap));
    /// <summary>The enemy population of the Mickey Mouse room.</summary>
    public static RoomEnemyPopulationDefinition MickeyMouse { get; } = Find(nameof(MickeyMouse));
    /// <summary>The enemy population of the Golden Torizo Energy Refill room.</summary>
    public static RoomEnemyPopulationDefinition GTEnergyRefill { get; } = Find(nameof(GTEnergyRefill));
    /// <summary>The enemy population of the Ridley room.</summary>
    public static RoomEnemyPopulationDefinition Ridley { get; } = Find(nameof(Ridley));
    /// <summary>The enemy population of the Post-Crocomire Power Bombs room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocPowerBombs { get; } = Find(nameof(PostCrocPowerBombs));
    /// <summary>The enemy population of the Ice Beam room.</summary>
    public static RoomEnemyPopulationDefinition IceBeam { get; } = Find(nameof(IceBeam));
    /// <summary>The enemy population of the Hi-Jump Boots room.</summary>
    public static RoomEnemyPopulationDefinition HiJumpBoots { get; } = Find(nameof(HiJumpBoots));
    /// <summary>The enemy population of the Post-Crocomire Save room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocSave { get; } = Find(nameof(PostCrocSave));
    /// <summary>The enemy population of the Frog Speedway room.</summary>
    public static RoomEnemyPopulationDefinition FrogSpeedway { get; } = Find(nameof(FrogSpeedway));
    /// <summary>The enemy population of the Rising Tide room.</summary>
    public static RoomEnemyPopulationDefinition RisingTide { get; } = Find(nameof(RisingTide));
    /// <summary>The enemy population of the Post-Crocomire Farming room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocFarming { get; } = Find(nameof(PostCrocFarming));
    /// <summary>The enemy population of the Worst Room in the Game.</summary>
    public static RoomEnemyPopulationDefinition WorstRoomInTheGame { get; } = Find(nameof(WorstRoomInTheGame));
    /// <summary>The enemy population of the Double Chamber room.</summary>
    public static RoomEnemyPopulationDefinition DoubleChamber { get; } = Find(nameof(DoubleChamber));
    /// <summary>The enemy population of the Grapple Beam room.</summary>
    public static RoomEnemyPopulationDefinition GrappleBeam { get; } = Find(nameof(GrappleBeam));
    /// <summary>The enemy population of the Red Pirate Shaft room.</summary>
    public static RoomEnemyPopulationDefinition RedPirateShaft { get; } = Find(nameof(RedPirateShaft));
    /// <summary>The enemy population of the Crocomire Speedway room.</summary>
    public static RoomEnemyPopulationDefinition CrocomireSpeedway { get; } = Find(nameof(CrocomireSpeedway));
    /// <summary>The enemy population of the Plowerhouse room.</summary>
    public static RoomEnemyPopulationDefinition Plowerhouse { get; } = Find(nameof(Plowerhouse));
    /// <summary>The enemy population of the Lower Norfair Fireflea room.</summary>
    public static RoomEnemyPopulationDefinition LNFireflea { get; } = Find(nameof(LNFireflea));
    /// <summary>The enemy population of the Magdollite Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition MagdolliteTunnel { get; } = Find(nameof(MagdolliteTunnel));
    /// <summary>The enemy population of an unused room state.</summary>
    public static RoomEnemyPopulationDefinition UnusedNorfairRoom { get; } = Find(nameof(UnusedNorfairRoom));
    /// <summary>The enemy population of the Lava Dive room.</summary>
    public static RoomEnemyPopulationDefinition LavaDive { get; } = Find(nameof(LavaDive));
    /// <summary>The enemy population of the Pillar room.</summary>
    public static RoomEnemyPopulationDefinition Pillar { get; } = Find(nameof(Pillar));
    /// <summary>The enemy population of the Crocomire Escape room.</summary>
    public static RoomEnemyPopulationDefinition CrocomireEscape { get; } = Find(nameof(CrocomireEscape));
    /// <summary>The enemy population of the Lower Norfair Spring Ball Maze room.</summary>
    public static RoomEnemyPopulationDefinition LNSpringBallMaze { get; } = Find(nameof(LNSpringBallMaze));
    /// <summary>The enemy population of the Lower Norfair Escape Power Bombs room.</summary>
    public static RoomEnemyPopulationDefinition LNEscapePowerBombs { get; } = Find(nameof(LNEscapePowerBombs));
    /// <summary>The enemy population of the Purple Shaft room.</summary>
    public static RoomEnemyPopulationDefinition PurpleShaft { get; } = Find(nameof(PurpleShaft));
    /// <summary>The enemy population of the Ridley Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition RidleyETank { get; } = Find(nameof(RidleyETank));
    /// <summary>The enemy population of the Screw Attack room.</summary>
    public static RoomEnemyPopulationDefinition ScrewAttack { get; } = Find(nameof(ScrewAttack));
    /// <summary>The enemy population of the Green Bubbles Missiles room.</summary>
    public static RoomEnemyPopulationDefinition GreenBubblesMissiles { get; } = Find(nameof(GreenBubblesMissiles));
    /// <summary>The enemy population of the Amphitheatre room.</summary>
    public static RoomEnemyPopulationDefinition Amphitheatre { get; } = Find(nameof(Amphitheatre));
    /// <summary>The enemy population of the Cathedral Entrance room.</summary>
    public static RoomEnemyPopulationDefinition CathedralEntrance { get; } = Find(nameof(CathedralEntrance));
    /// <summary>The enemy population of the Main Hall room.</summary>
    public static RoomEnemyPopulationDefinition MainHall { get; } = Find(nameof(MainHall));
    /// <summary>The enemy population of the Post-Crocomire Jump room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocJump { get; } = Find(nameof(PostCrocJump));
    /// <summary>The enemy population of the Frog Save room.</summary>
    public static RoomEnemyPopulationDefinition FrogSave { get; } = Find(nameof(FrogSave));
    /// <summary>The enemy population of the Crocomire Save room.</summary>
    public static RoomEnemyPopulationDefinition CrocomireSave { get; } = Find(nameof(CrocomireSave));
    /// <summary>The enemy population of the Crumble Shaft room.</summary>
    public static RoomEnemyPopulationDefinition CrumbleShaft { get; } = Find(nameof(CrumbleShaft));
    /// <summary>The enemy population of the Wasteland room.</summary>
    public static RoomEnemyPopulationDefinition Wasteland { get; } = Find(nameof(Wasteland));
    /// <summary>The enemy population of the Acid Snakes Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition AcidSnakesTunnel { get; } = Find(nameof(AcidSnakesTunnel));
    /// <summary>The enemy population of the Grapple Tutorial 1 room.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial1 { get; } = Find(nameof(GrappleTutorial1));
    /// <summary>The enemy population of the Bubble Mountain Save room.</summary>
    public static RoomEnemyPopulationDefinition BubbleMountainSave { get; } = Find(nameof(BubbleMountainSave));
    /// <summary>The enemy population of the Hi-Jump Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition HiJumpETank { get; } = Find(nameof(HiJumpETank));
    /// <summary>The enemy population of the Bubble Mountain room.</summary>
    public static RoomEnemyPopulationDefinition BubbleMountain { get; } = Find(nameof(BubbleMountain));
    /// <summary>The enemy population of the Ice Beam Acid room.</summary>
    public static RoomEnemyPopulationDefinition IceBeamAcid { get; } = Find(nameof(IceBeamAcid));
    /// <summary>The enemy population of the Ice Beam Tutorial room.</summary>
    public static RoomEnemyPopulationDefinition IceBeamTutorial { get; } = Find(nameof(IceBeamTutorial));
    /// <summary>The enemy population of the Post-Crocomire Missile room.</summary>
    public static RoomEnemyPopulationDefinition PostCrocMissile { get; } = Find(nameof(PostCrocMissile));
    /// <summary>The enemy population of the Cathedral room.</summary>
    public static RoomEnemyPopulationDefinition Cathedral { get; } = Find(nameof(Cathedral));
    /// <summary>The enemy population of the Norfair Reserve Tank room.</summary>
    public static RoomEnemyPopulationDefinition NorfairReserveTank { get; } = Find(nameof(NorfairReserveTank));
    /// <summary>The enemy population of the Grapple Tutorial 2 room.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial2 { get; } = Find(nameof(GrappleTutorial2));
    /// <summary>The enemy population of the Ice Beam Snake room.</summary>
    public static RoomEnemyPopulationDefinition IceBeamSnake { get; } = Find(nameof(IceBeamSnake));
    /// <summary>The enemy population of the Golden Torizo room.</summary>
    public static RoomEnemyPopulationDefinition GoldenTorizo { get; } = Find(nameof(GoldenTorizo));
    /// <summary>The enemy population of the Spiky Platforms Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition SpikyPlatformsTunnel { get; } = Find(nameof(SpikyPlatformsTunnel));
    /// <summary>The enemy population of the Speed Booster room.</summary>
    public static RoomEnemyPopulationDefinition SpeedBooster { get; } = Find(nameof(SpeedBooster));
    /// <summary>The enemy population of the Lower Norfair Farming room.</summary>
    public static RoomEnemyPopulationDefinition LNFarming { get; } = Find(nameof(LNFarming));
    /// <summary>The enemy population of the Single Chamber room.</summary>
    public static RoomEnemyPopulationDefinition SingleChamber { get; } = Find(nameof(SingleChamber));
    /// <summary>The enemy population of the Speed Booster Hall room.</summary>
    public static RoomEnemyPopulationDefinition SpeedBoosterHall { get; } = Find(nameof(SpeedBoosterHall));
    /// <summary>The enemy population of the Bat Cave room.</summary>
    public static RoomEnemyPopulationDefinition BatCave { get; } = Find(nameof(BatCave));
    /// <summary>The enemy population of the Kronic Boost room.</summary>
    public static RoomEnemyPopulationDefinition KronicBoost { get; } = Find(nameof(KronicBoost));
    /// <summary>The enemy population of the Business Center room.</summary>
    public static RoomEnemyPopulationDefinition BusinessCenter { get; } = Find(nameof(BusinessCenter));
    /// <summary>The enemy population of the Three Musketeers room.</summary>
    public static RoomEnemyPopulationDefinition ThreeMusketeers { get; } = Find(nameof(ThreeMusketeers));
    /// <summary>The enemy population of the Crocomire room.</summary>
    public static RoomEnemyPopulationDefinition Crocomire { get; } = Find(nameof(Crocomire));
    /// <summary>The enemy population of the Lower Norfair Elevator Save room.</summary>
    public static RoomEnemyPopulationDefinition LNElevSave { get; } = Find(nameof(LNElevSave));
    /// <summary>The enemy population of the Volcano room.</summary>
    public static RoomEnemyPopulationDefinition Volcano { get; } = Find(nameof(Volcano));
    /// <summary>The enemy population of the Upper Norfair Farming room.</summary>
    public static RoomEnemyPopulationDefinition UpperNorfairFarming { get; } = Find(nameof(UpperNorfairFarming));
    /// <summary>The enemy population of the Sponge Bath room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition SpongeBathWhenAreaBossDead { get; } = Find(nameof(SpongeBathWhenAreaBossDead));
    /// <summary>The enemy population of the Wrecked Ship Energy Tank room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipETankWhenAreaBossDead { get; } = Find(nameof(WreckedShipETankWhenAreaBossDead));
    /// <summary>The enemy population of the Wrecked Ship Main Shaft room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMainShaftDefault { get; } = Find(nameof(WreckedShipMainShaftDefault));
    /// <summary>The enemy population of the Bowling Alley room in its default state.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlleyDefault { get; } = Find(nameof(BowlingAlleyDefault));
    /// <summary>The enemy population of the Attic room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition AtticWhenAreaBossDead { get; } = Find(nameof(AtticWhenAreaBossDead));
    /// <summary>The enemy population of the Basement room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition BasementWhenAreaBossDead { get; } = Find(nameof(BasementWhenAreaBossDead));
    /// <summary>The enemy population of the Wrecked Ship Map room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMapWhenAreaBossDead { get; } = Find(nameof(WreckedShipMapWhenAreaBossDead));
    /// <summary>The enemy population of the Wrecked Ship West Super room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WSWestSuperDefault { get; } = Find(nameof(WSWestSuperDefault));
    /// <summary>The enemy population of the Spiky Death room in its default state.</summary>
    public static RoomEnemyPopulationDefinition SpikyDeathDefault { get; } = Find(nameof(SpikyDeathDefault));
    /// <summary>The enemy population of the Sponge Bath room in its default state.</summary>
    public static RoomEnemyPopulationDefinition SpongeBathDefault { get; } = Find(nameof(SpongeBathDefault));
    /// <summary>The enemy population of the Wrecked Ship Entrance room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEntranceWhenAreaBossDead { get; } = Find(nameof(WreckedShipEntranceWhenAreaBossDead));
    /// <summary>The enemy population of the Electric Death room in its default state.</summary>
    public static RoomEnemyPopulationDefinition ElectricDeathDefault { get; } = Find(nameof(ElectricDeathDefault));
    /// <summary>The enemy population of the Bowling Alley room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlleyWhenAreaBossDead { get; } = Find(nameof(BowlingAlleyWhenAreaBossDead));
    /// <summary>The enemy population of Wrecked Ship's Assembly Line room in its default state.</summary>
    public static RoomEnemyPopulationDefinition AssemblyLineDefault { get; } = Find(nameof(AssemblyLineDefault));
    /// <summary>The enemy population of the Phantoon room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition PhantoonWhenAreaBossDead { get; } = Find(nameof(PhantoonWhenAreaBossDead));
    /// <summary>The enemy population of the Wrecked Ship Energy Tank room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipETankDefault { get; } = Find(nameof(WreckedShipETankDefault));
    /// <summary>The enemy population of the Gravity Suit room in its default state.</summary>
    public static RoomEnemyPopulationDefinition GravitySuitDefault { get; } = Find(nameof(GravitySuitDefault));
    /// <summary>The enemy population of the Wrecked Ship Map room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMapDefault { get; } = Find(nameof(WreckedShipMapDefault));
    /// <summary>The enemy population of the Gravity Suit room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition GravitySuitWhenAreaBossDead { get; } = Find(nameof(GravitySuitWhenAreaBossDead));
    /// <summary>The enemy population of the Basement room in its default state.</summary>
    public static RoomEnemyPopulationDefinition BasementDefault { get; } = Find(nameof(BasementDefault));
    /// <summary>The enemy population of the Wrecked Ship Entrance room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEntranceDefault { get; } = Find(nameof(WreckedShipEntranceDefault));
    /// <summary>The enemy population of Wrecked Ship's Assembly Line room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition AssemblyLineWhenAreaBossDead { get; } = Find(nameof(AssemblyLineWhenAreaBossDead));
    /// <summary>The enemy population of the Wrecked Ship Save room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipSaveWhenAreaBossDead { get; } = Find(nameof(WreckedShipSaveWhenAreaBossDead));
    /// <summary>The enemy population of the Wrecked Ship West Super room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WSWestSuperWhenAreaBossDead { get; } = Find(nameof(WSWestSuperWhenAreaBossDead));
    /// <summary>The enemy population of the Attic room in its default state.</summary>
    public static RoomEnemyPopulationDefinition AtticDefault { get; } = Find(nameof(AtticDefault));
    /// <summary>The enemy population of the Wrecked Ship East Super room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WSEastSuperDefault { get; } = Find(nameof(WSEastSuperDefault));
    /// <summary>The enemy population of the Wrecked Ship Save room in its default state.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipSaveDefault { get; } = Find(nameof(WreckedShipSaveDefault));
    /// <summary>The enemy population of the Electric Death room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition ElectricDeathWhenAreaBossDead { get; } = Find(nameof(ElectricDeathWhenAreaBossDead));
    /// <summary>The enemy population of the Spiky Death room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition SpikyDeathWhenAreaBossDead { get; } = Find(nameof(SpikyDeathWhenAreaBossDead));
    /// <summary>The enemy population of the Wrecked Ship East Super room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WSEastSuperWhenAreaBossDead { get; } = Find(nameof(WSEastSuperWhenAreaBossDead));
    /// <summary>The enemy population of the Phantoon room in its default state.</summary>
    public static RoomEnemyPopulationDefinition PhantoonDefault { get; } = Find(nameof(PhantoonDefault));
    /// <summary>The enemy population of the Wrecked Ship Main Shaft room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMainShaftWhenAreaBossDead { get; } = Find(nameof(WreckedShipMainShaftWhenAreaBossDead));
    /// <summary>The enemy population of the Mt. Everest room.</summary>
    public static RoomEnemyPopulationDefinition MtEverest { get; } = Find(nameof(MtEverest));
    /// <summary>The enemy population of the Halfie Climb room.</summary>
    public static RoomEnemyPopulationDefinition HalfieClimb { get; } = Find(nameof(HalfieClimb));
    /// <summary>The enemy population of the Watering Hole room.</summary>
    public static RoomEnemyPopulationDefinition WateringHole { get; } = Find(nameof(WateringHole));
    /// <summary>The enemy population of the Crab Shaft room.</summary>
    public static RoomEnemyPopulationDefinition CrabShaft { get; } = Find(nameof(CrabShaft));
    /// <summary>The enemy population of the Maridia Elevator room.</summary>
    public static RoomEnemyPopulationDefinition MaridiaElev { get; } = Find(nameof(MaridiaElev));
    /// <summary>The enemy population of the Colosseum room.</summary>
    public static RoomEnemyPopulationDefinition Colosseum { get; } = Find(nameof(Colosseum));
    /// <summary>The enemy population of the Aqueduct Save room.</summary>
    public static RoomEnemyPopulationDefinition AqueductSave { get; } = Find(nameof(AqueductSave));
    /// <summary>The enemy population of the Precious room.</summary>
    public static RoomEnemyPopulationDefinition ThePrecious { get; } = Find(nameof(ThePrecious));
    /// <summary>The enemy population of the Botwoon Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition BotwoonETank { get; } = Find(nameof(BotwoonETank));
    /// <summary>The enemy population of the Draygon Save room.</summary>
    public static RoomEnemyPopulationDefinition DraygonSave { get; } = Find(nameof(DraygonSave));
    /// <summary>The enemy population of the Maridia Missile Refill room.</summary>
    public static RoomEnemyPopulationDefinition MaridiaMissileRefill { get; } = Find(nameof(MaridiaMissileRefill));
    /// <summary>The enemy population of the Plasma Beach Quicksand room.</summary>
    public static RoomEnemyPopulationDefinition PlasmaBeachQuicksand { get; } = Find(nameof(PlasmaBeachQuicksand));
    /// <summary>The enemy population of the Botwoon Quicksand room.</summary>
    public static RoomEnemyPopulationDefinition BotwoonQuicksand { get; } = Find(nameof(BotwoonQuicksand));
    /// <summary>The enemy population of the Shaktool room.</summary>
    public static RoomEnemyPopulationDefinition Shaktool { get; } = Find(nameof(Shaktool));
    /// <summary>The enemy population of the Draygon room in its default state.</summary>
    public static RoomEnemyPopulationDefinition DraygonDefault { get; } = Find(nameof(DraygonDefault));
    /// <summary>The enemy population of the Aqueduct room.</summary>
    public static RoomEnemyPopulationDefinition Aqueduct { get; } = Find(nameof(Aqueduct));
    /// <summary>The enemy population of the Red Fish room.</summary>
    public static RoomEnemyPopulationDefinition RedFish { get; } = Find(nameof(RedFish));
    /// <summary>The enemy population of the Plasma room.</summary>
    public static RoomEnemyPopulationDefinition Plasma { get; } = Find(nameof(Plasma));
    /// <summary>The enemy population of the Space Jump room.</summary>
    public static RoomEnemyPopulationDefinition SpaceJump { get; } = Find(nameof(SpaceJump));
    /// <summary>The enemy population of the Pants room.</summary>
    public static RoomEnemyPopulationDefinition Pants { get; } = Find(nameof(Pants));
    /// <summary>The enemy population of the Glass Tunnel Save room.</summary>
    public static RoomEnemyPopulationDefinition GlassTunnelSave { get; } = Find(nameof(GlassTunnelSave));
    /// <summary>The enemy population of the Glass Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition GlassTunnel { get; } = Find(nameof(GlassTunnel));
    /// <summary>The enemy population of the West Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition WestTunnel { get; } = Find(nameof(WestTunnel));
    /// <summary>The enemy population of the East Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition EastTunnel { get; } = Find(nameof(EastTunnel));
    /// <summary>The enemy population of the Mama Turtle room.</summary>
    public static RoomEnemyPopulationDefinition MamaTurtle { get; } = Find(nameof(MamaTurtle));
    /// <summary>The enemy population of the Crab Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition CrabTunnel { get; } = Find(nameof(CrabTunnel));
    /// <summary>The enemy population of the Northwest Maridia Bug room.</summary>
    public static RoomEnemyPopulationDefinition NWestMaridiaBug { get; } = Find(nameof(NWestMaridiaBug));
    /// <summary>The enemy population of the Pseudo Plasma Spark room.</summary>
    public static RoomEnemyPopulationDefinition PseudoPlasmaSpark { get; } = Find(nameof(PseudoPlasmaSpark));
    /// <summary>The enemy population of the West Sand Hall Tunnel room.</summary>
    public static RoomEnemyPopulationDefinition WestSandHallTunnel { get; } = Find(nameof(WestSandHallTunnel));
    /// <summary>The enemy population of the Plasma Tutorial room.</summary>
    public static RoomEnemyPopulationDefinition PlasmaTutorial { get; } = Find(nameof(PlasmaTutorial));
    /// <summary>The enemy population of the Thread the Needle room.</summary>
    public static RoomEnemyPopulationDefinition ThreadTheNeedle { get; } = Find(nameof(ThreadTheNeedle));
    /// <summary>The enemy population of the Plasma Spark room.</summary>
    public static RoomEnemyPopulationDefinition PlasmaSpark { get; } = Find(nameof(PlasmaSpark));
    /// <summary>The enemy population of the Toilet room.</summary>
    public static RoomEnemyPopulationDefinition Toilet { get; } = Find(nameof(Toilet));
    /// <summary>The enemy population of the West Sand Hall room.</summary>
    public static RoomEnemyPopulationDefinition WestSandHall { get; } = Find(nameof(WestSandHall));
    /// <summary>The enemy population of the Oasis room.</summary>
    public static RoomEnemyPopulationDefinition Oasis { get; } = Find(nameof(Oasis));
    /// <summary>The enemy population of the East Sand Hall room.</summary>
    public static RoomEnemyPopulationDefinition EastSandHall { get; } = Find(nameof(EastSandHall));
    /// <summary>The enemy population of the West Aqueduct Quicksand room.</summary>
    public static RoomEnemyPopulationDefinition WestAqueductQuicksand { get; } = Find(nameof(WestAqueductQuicksand));
    /// <summary>The enemy population of the East Aqueduct Quicksand room.</summary>
    public static RoomEnemyPopulationDefinition EastAqueductQuicksand { get; } = Find(nameof(EastAqueductQuicksand));
    /// <summary>The enemy population of the Butterfly room.</summary>
    public static RoomEnemyPopulationDefinition Butterfly { get; } = Find(nameof(Butterfly));
    /// <summary>The enemy population of the Botwoon Hallway room.</summary>
    public static RoomEnemyPopulationDefinition BotwoonHallway { get; } = Find(nameof(BotwoonHallway));
    /// <summary>The enemy population of the East Pants room.</summary>
    public static RoomEnemyPopulationDefinition EastPants { get; } = Find(nameof(EastPants));
    /// <summary>The enemy population of the Springball room.</summary>
    public static RoomEnemyPopulationDefinition Springball { get; } = Find(nameof(Springball));
    /// <summary>The enemy population of the Below Botwoon Energy Tank room.</summary>
    public static RoomEnemyPopulationDefinition BelowBotwoonETank { get; } = Find(nameof(BelowBotwoonETank));
    /// <summary>The enemy population of the Kassiuz room.</summary>
    public static RoomEnemyPopulationDefinition Kassiuz { get; } = Find(nameof(Kassiuz));
    /// <summary>The enemy population of the Forgotten Highway Save room.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwaySave { get; } = Find(nameof(ForgottenHighwaySave));
    /// <summary>The enemy population of the Maridia Energy Refill room.</summary>
    public static RoomEnemyPopulationDefinition MaridiaEnergyRefill { get; } = Find(nameof(MaridiaEnergyRefill));
    /// <summary>The enemy population of the Maridia Map room.</summary>
    public static RoomEnemyPopulationDefinition MaridiaMap { get; } = Find(nameof(MaridiaMap));
    /// <summary>The enemy population of the Crab Hole room.</summary>
    public static RoomEnemyPopulationDefinition CrabHole { get; } = Find(nameof(CrabHole));
    /// <summary>The enemy population of the Botwoon room.</summary>
    public static RoomEnemyPopulationDefinition Botwoon { get; } = Find(nameof(Botwoon));
    /// <summary>The enemy population of the Main Street room.</summary>
    public static RoomEnemyPopulationDefinition MainStreet { get; } = Find(nameof(MainStreet));
    /// <summary>The enemy population of the Bug Sand Hole room.</summary>
    public static RoomEnemyPopulationDefinition BugSandHole { get; } = Find(nameof(BugSandHole));
    /// <summary>The enemy population of the East Sand Hole room.</summary>
    public static RoomEnemyPopulationDefinition EastSandHole { get; } = Find(nameof(EastSandHole));
    /// <summary>The enemy population of the West Sand Hole room.</summary>
    public static RoomEnemyPopulationDefinition WestSandHole { get; } = Find(nameof(WestSandHole));
    /// <summary>The enemy population of the West Cactus Alley room.</summary>
    public static RoomEnemyPopulationDefinition WestCactusAlley { get; } = Find(nameof(WestCactusAlley));
    /// <summary>The enemy population of the East Cactus Alley room.</summary>
    public static RoomEnemyPopulationDefinition EastCactusAlley { get; } = Find(nameof(EastCactusAlley));
    /// <summary>The enemy population of the Fish Tank room.</summary>
    public static RoomEnemyPopulationDefinition FishTank { get; } = Find(nameof(FishTank));
    /// <summary>The enemy population of the Tourian Escape 4 room.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape4 { get; } = Find(nameof(TourianEscape4));
    /// <summary>The enemy population of the Mother Brain Save room.</summary>
    public static RoomEnemyPopulationDefinition MotherBrainSave { get; } = Find(nameof(MotherBrainSave));
    /// <summary>The enemy population of the Metroids 1 room in its default state.</summary>
    public static RoomEnemyPopulationDefinition Metroids1Default { get; } = Find(nameof(Metroids1Default));
    /// <summary>The enemy population of the Dust Torizo room.</summary>
    public static RoomEnemyPopulationDefinition DustTorizo { get; } = Find(nameof(DustTorizo));
    /// <summary>The enemy population of the Big Boy room.</summary>
    public static RoomEnemyPopulationDefinition BigBoy { get; } = Find(nameof(BigBoy));
    /// <summary>The enemy population of the Mother Brain room in its first two states.</summary>
    public static RoomEnemyPopulationDefinition MotherBrain01 { get; } = Find(nameof(MotherBrain01));
    /// <summary>The enemy population of the Tourian Eye Door room.</summary>
    public static RoomEnemyPopulationDefinition TourianEyeDoor { get; } = Find(nameof(TourianEyeDoor));
    /// <summary>The enemy population of the Blue Hopper room.</summary>
    public static RoomEnemyPopulationDefinition BlueHopper { get; } = Find(nameof(BlueHopper));
    /// <summary>The enemy population of the Tourian Escape 1 room.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape1 { get; } = Find(nameof(TourianEscape1));
    /// <summary>The enemy population of the Tourian First room.</summary>
    public static RoomEnemyPopulationDefinition TourianFirst { get; } = Find(nameof(TourianFirst));
    /// <summary>The enemy population of the Metroids 2 room in its default state.</summary>
    public static RoomEnemyPopulationDefinition Metroids2Default { get; } = Find(nameof(Metroids2Default));
    /// <summary>The enemy population of the Metroids 4 room in its default state.</summary>
    public static RoomEnemyPopulationDefinition Metroids4Default { get; } = Find(nameof(Metroids4Default));
    /// <summary>The enemy population of the Metroids 1 room once the first Metroid hall is cleared.</summary>
    public static RoomEnemyPopulationDefinition Metroids1WhenFirstMetroidHallCleared { get; } = Find(nameof(Metroids1WhenFirstMetroidHallCleared));
    /// <summary>The enemy population of the Metroids 2 room once the first Metroid shaft is cleared.</summary>
    public static RoomEnemyPopulationDefinition Metroids2WhenFirstMetroidShaftCleared { get; } = Find(nameof(Metroids2WhenFirstMetroidShaftCleared));
    /// <summary>The enemy population of the Tourian Escape 2 room.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape2 { get; } = Find(nameof(TourianEscape2));
    /// <summary>The enemy population of the Metroids 3 room once the second Metroid hall is cleared.</summary>
    public static RoomEnemyPopulationDefinition Metroids3WhenSecondMetroidHallCleared { get; } = Find(nameof(Metroids3WhenSecondMetroidHallCleared));
    /// <summary>The enemy population of the Metroids 4 room once the second Metroid shaft is cleared.</summary>
    public static RoomEnemyPopulationDefinition Metroids4WhenSecondMetroidShaftCleared { get; } = Find(nameof(Metroids4WhenSecondMetroidShaftCleared));
    /// <summary>The enemy population of the Tourian Escape 3 room.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape3 { get; } = Find(nameof(TourianEscape3));
    /// <summary>The enemy population of the Seaweed room.</summary>
    public static RoomEnemyPopulationDefinition Seaweed { get; } = Find(nameof(Seaweed));
    /// <summary>The enemy population of the Tourian Recharge room.</summary>
    public static RoomEnemyPopulationDefinition TourianRecharge { get; } = Find(nameof(TourianRecharge));
    /// <summary>The enemy population of the Upper Tourian Save room.</summary>
    public static RoomEnemyPopulationDefinition UpperTourianSave { get; } = Find(nameof(UpperTourianSave));
    /// <summary>The enemy population of the Metroids 3 room in its default state.</summary>
    public static RoomEnemyPopulationDefinition Metroids3Default { get; } = Find(nameof(Metroids3Default));
    /// <summary>The enemy population of the Rinka Shaft room.</summary>
    public static RoomEnemyPopulationDefinition RinkaShaft { get; } = Find(nameof(RinkaShaft));
    /// <summary>The enemy population of the Ceres Elevator room in its default state.</summary>
    public static RoomEnemyPopulationDefinition CeresElevDefault { get; } = Find(nameof(CeresElevDefault));
    /// <summary>The enemy population of the Falling Tile room in its default state.</summary>
    public static RoomEnemyPopulationDefinition FallingTileDefault { get; } = Find(nameof(FallingTileDefault));
    /// <summary>The enemy population of the Magnet Stairs room in its default state.</summary>
    public static RoomEnemyPopulationDefinition MagnetStairsDefault { get; } = Find(nameof(MagnetStairsDefault));
    /// <summary>The enemy population of the Dead Scientist room in its default state.</summary>
    public static RoomEnemyPopulationDefinition DeadScientistDefault { get; } = Find(nameof(DeadScientistDefault));
    /// <summary>The enemy population of the Room 58 Escape in its default state.</summary>
    public static RoomEnemyPopulationDefinition Room58EscapeDefault { get; } = Find(nameof(Room58EscapeDefault));
    /// <summary>The enemy population of the Ceres Ridley room in its default state.</summary>
    public static RoomEnemyPopulationDefinition CeresRidleyDefault { get; } = Find(nameof(CeresRidleyDefault));
    /// <summary>The enemy population of the Ceres Elevator room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition CeresElevWhenAreaBossDead { get; } = Find(nameof(CeresElevWhenAreaBossDead));
    /// <summary>The enemy population of the Falling Tile room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition FallingTileWhenAreaBossDead { get; } = Find(nameof(FallingTileWhenAreaBossDead));
    /// <summary>The enemy population of the Magnet Stairs room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition MagnetStairsWhenAreaBossDead { get; } = Find(nameof(MagnetStairsWhenAreaBossDead));
    /// <summary>The enemy population of the Dead Scientist room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition DeadScientistWhenAreaBossDead { get; } = Find(nameof(DeadScientistWhenAreaBossDead));
    /// <summary>The enemy population of the Room 58 Escape once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition Room58EscapeWhenAreaBossDead { get; } = Find(nameof(Room58EscapeWhenAreaBossDead));
    /// <summary>The enemy population of the Ceres Ridley room once the area's boss is dead.</summary>
    public static RoomEnemyPopulationDefinition CeresRidleyWhenAreaBossDead { get; } = Find(nameof(CeresRidleyWhenAreaBossDead));

}
