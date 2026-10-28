namespace SuperMetroid.Core.Enemies;

/// <summary>Application-owned ordered enemy placements selected by all retail room states.</summary>
/// <remarks>
/// The 302 pointers and 1,658 records come from the pinned NTSC J/U v1.0 bank-$A1
/// populations selected by 323 compiled room states. Record order and the byte after
/// each terminator govern linked slot allocation and the room enemy-death quota.
/// This is gameplay data, not an editable visual asset.
/// </remarks>
[NumberedIdentities("Each numbered member is the enemy population of one numbered room or room state, named for it; no caller indexes the populations by number.")]
public static class RoomEnemyPopulationDefinitions
{
    /// <summary>Retail populations in the cartridge's list order, checked complete before gameplay can use them.</summary>
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

    /// <summary>Number of distinct bank-$A1 population pointers selected by the compiled retail room states, including empty lists.</summary>
    public const int ListCount = 302;
    /// <summary>Total placement records across the distinct compiled lists, excluding terminators and without counting repeated references from room states.</summary>
    public const int RecordCount = 1658;

    /// <summary>Each retail population by name.</summary>
    private static readonly Dictionary<string, RoomEnemyPopulationDefinition> ByName = Definitions.ToDictionary(list => list.Name);

    /// <summary>The compiled population named <paramref name="name"/>.</summary>
    private static RoomEnemyPopulationDefinition Find(string name) => ByName[name];

    /// <summary>The <c>EnemyPopulations_DraygonState1MotherBrainState2</c> list.</summary>
    public static RoomEnemyPopulationDefinition DraygonState1MotherBrainState2 { get; } = Find(nameof(DraygonState1MotherBrainState2));
    /// <summary>The <c>EnemyPopulations_EastOcean</c> list.</summary>
    public static RoomEnemyPopulationDefinition EastOcean { get; } = Find(nameof(EastOcean));
    /// <summary>The <c>EnemyPopulations_CrateriaPowerBombs</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrateriaPowerBombs { get; } = Find(nameof(CrateriaPowerBombs));
    /// <summary>The <c>EnemyPopulations_Terminator</c> list.</summary>
    public static RoomEnemyPopulationDefinition Terminator { get; } = Find(nameof(Terminator));
    /// <summary>The <c>EnemyPopulations_FinalMissileBombway1</c> list.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileBombway1 { get; } = Find(nameof(FinalMissileBombway1));
    /// <summary>The <c>EnemyPopulations_FinalMissileBombway0</c> list.</summary>
    public static RoomEnemyPopulationDefinition FinalMissileBombway0 { get; } = Find(nameof(FinalMissileBombway0));
    /// <summary>The <c>EnemyPopulations_Parlor1</c> list.</summary>
    public static RoomEnemyPopulationDefinition Parlor1 { get; } = Find(nameof(Parlor1));
    /// <summary>The <c>EnemyPopulations_Flyway01</c> list.</summary>
    public static RoomEnemyPopulationDefinition Flyway01 { get; } = Find(nameof(Flyway01));
    /// <summary>The <c>EnemyPopulations_Pit1</c> list.</summary>
    public static RoomEnemyPopulationDefinition Pit1 { get; } = Find(nameof(Pit1));
    /// <summary>The <c>EnemyPopulations_GauntletEntrance</c> list.</summary>
    public static RoomEnemyPopulationDefinition GauntletEntrance { get; } = Find(nameof(GauntletEntrance));
    /// <summary>The <c>EnemyPopulations_BombTorizo01</c> list.</summary>
    public static RoomEnemyPopulationDefinition BombTorizo01 { get; } = Find(nameof(BombTorizo01));
    /// <summary>The <c>EnemyPopulations_GreenPiratesShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenPiratesShaft { get; } = Find(nameof(GreenPiratesShaft));
    /// <summary>The <c>EnemyPopulations_GreenBrinstarElev</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarElev { get; } = Find(nameof(GreenBrinstarElev));
    /// <summary>The <c>EnemyPopulations_FinalMissile0</c> list.</summary>
    public static RoomEnemyPopulationDefinition FinalMissile0 { get; } = Find(nameof(FinalMissile0));
    /// <summary>The <c>EnemyPopulations_CrateriaMap</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrateriaMap { get; } = Find(nameof(CrateriaMap));
    /// <summary>The <c>EnemyPopulations_FinalMissile1</c> list.</summary>
    public static RoomEnemyPopulationDefinition FinalMissile1 { get; } = Find(nameof(FinalMissile1));
    /// <summary>The <c>EnemyPopulations_Climb2</c> list.</summary>
    public static RoomEnemyPopulationDefinition Climb2 { get; } = Find(nameof(Climb2));
    /// <summary>The <c>EnemyPopulations_CrateriaSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrateriaSave { get; } = Find(nameof(CrateriaSave));
    /// <summary>The <c>EnemyPopulations_ForgottenHighwayElbow</c> list.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayElbow { get; } = Find(nameof(ForgottenHighwayElbow));
    /// <summary>The <c>EnemyPopulations_Moat</c> list.</summary>
    public static RoomEnemyPopulationDefinition Moat { get; } = Find(nameof(Moat));
    /// <summary>The <c>EnemyPopulations_Climb0</c> list.</summary>
    public static RoomEnemyPopulationDefinition Climb0 { get; } = Find(nameof(Climb0));
    /// <summary>The <c>EnemyPopulations_WestOcean</c> list.</summary>
    public static RoomEnemyPopulationDefinition WestOcean { get; } = Find(nameof(WestOcean));
    /// <summary>The <c>EnemyPopulations_Parlor0</c> list.</summary>
    public static RoomEnemyPopulationDefinition Parlor0 { get; } = Find(nameof(Parlor0));
    /// <summary>The <c>EnemyPopulations_LandingSite012</c> list.</summary>
    public static RoomEnemyPopulationDefinition LandingSite012 { get; } = Find(nameof(LandingSite012));
    /// <summary>The <c>EnemyPopulations_CrateriaSuper</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrateriaSuper { get; } = Find(nameof(CrateriaSuper));
    /// <summary>The <c>EnemyPopulations_PreBowling</c> list.</summary>
    public static RoomEnemyPopulationDefinition PreBowling { get; } = Find(nameof(PreBowling));
    /// <summary>The <c>EnemyPopulations_Climb1</c> list.</summary>
    public static RoomEnemyPopulationDefinition Climb1 { get; } = Find(nameof(Climb1));
    /// <summary>The <c>EnemyPopulations_BowlingAlleyPath</c> list.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlleyPath { get; } = Find(nameof(BowlingAlleyPath));
    /// <summary>The <c>EnemyPopulations_RedBinstarElev</c> list.</summary>
    public static RoomEnemyPopulationDefinition RedBinstarElev { get; } = Find(nameof(RedBinstarElev));
    /// <summary>The <c>EnemyPopulations_GauntletETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition GauntletETank { get; } = Find(nameof(GauntletETank));
    /// <summary>The <c>EnemyPopulations_Pit0</c> list.</summary>
    public static RoomEnemyPopulationDefinition Pit0 { get; } = Find(nameof(Pit0));
    /// <summary>The <c>EnemyPopulations_CrabMaze</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrabMaze { get; } = Find(nameof(CrabMaze));
    /// <summary>The <c>EnemyPopulations_CrateriaTube</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrateriaTube { get; } = Find(nameof(CrateriaTube));
    /// <summary>The <c>EnemyPopulations_ForgottenHighwayKagos</c> list.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayKagos { get; } = Find(nameof(ForgottenHighwayKagos));
    /// <summary>The <c>EnemyPopulations_ElevToBlueBrinstar</c> list.</summary>
    public static RoomEnemyPopulationDefinition ElevToBlueBrinstar { get; } = Find(nameof(ElevToBlueBrinstar));
    /// <summary>The <c>EnemyPopulations_ForgottenHighwayElev</c> list.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwayElev { get; } = Find(nameof(ForgottenHighwayElev));
    /// <summary>The <c>EnemyPopulations_PreMapFlyway</c> list.</summary>
    public static RoomEnemyPopulationDefinition PreMapFlyway { get; } = Find(nameof(PreMapFlyway));
    /// <summary>The <c>EnemyPopulations_LowerMushrooms</c> list.</summary>
    public static RoomEnemyPopulationDefinition LowerMushrooms { get; } = Find(nameof(LowerMushrooms));
    /// <summary>The <c>EnemyPopulations_LandingSite3</c> list.</summary>
    public static RoomEnemyPopulationDefinition LandingSite3 { get; } = Find(nameof(LandingSite3));
    /// <summary>The <c>EnemyPopulations_Parlor2</c> list.</summary>
    public static RoomEnemyPopulationDefinition Parlor2 { get; } = Find(nameof(Parlor2));
    /// <summary>The <c>EnemyPopulations_BombTorizo2</c> list.</summary>
    public static RoomEnemyPopulationDefinition BombTorizo2 { get; } = Find(nameof(BombTorizo2));
    /// <summary>The <c>EnemyPopulations_Flyway2</c> list.</summary>
    public static RoomEnemyPopulationDefinition Flyway2 { get; } = Find(nameof(Flyway2));
    /// <summary>The <c>EnemyPopulations_CrateriaKihunter</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrateriaKihunter { get; } = Find(nameof(CrateriaKihunter));
    /// <summary>The <c>EnemyPopulations_BrinstarPreMap</c> list.</summary>
    public static RoomEnemyPopulationDefinition BrinstarPreMap { get; } = Find(nameof(BrinstarPreMap));
    /// <summary>The <c>EnemyPopulations_GreenBrinstarMap</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMap { get; } = Find(nameof(GreenBrinstarMap));
    /// <summary>The <c>EnemyPopulations_GreenBrinstarMissileRefill</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMissileRefill { get; } = Find(nameof(GreenBrinstarMissileRefill));
    /// <summary>The <c>EnemyPopulations_SporeSpawnKihunters</c> list.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnKihunters { get; } = Find(nameof(SporeSpawnKihunters));
    /// <summary>The <c>EnemyPopulations_EtecoonSuper</c> list.</summary>
    public static RoomEnemyPopulationDefinition EtecoonSuper { get; } = Find(nameof(EtecoonSuper));
    /// <summary>The <c>EnemyPopulations_DachoraEnergyRefill</c> list.</summary>
    public static RoomEnemyPopulationDefinition DachoraEnergyRefill { get; } = Find(nameof(DachoraEnergyRefill));
    /// <summary>The <c>EnemyPopulations_SporeSpawnFarming</c> list.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnFarming { get; } = Find(nameof(SporeSpawnFarming));
    /// <summary>The <c>EnemyPopulations_Statues</c> list.</summary>
    public static RoomEnemyPopulationDefinition Statues { get; } = Find(nameof(Statues));
    /// <summary>The <c>EnemyPopulations_HopperETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition HopperETank { get; } = Find(nameof(HopperETank));
    /// <summary>The <c>EnemyPopulations_BetaPowerBomb</c> list.</summary>
    public static RoomEnemyPopulationDefinition BetaPowerBomb { get; } = Find(nameof(BetaPowerBomb));
    /// <summary>The <c>EnemyPopulations_WaterwayETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition WaterwayETank { get; } = Find(nameof(WaterwayETank));
    /// <summary>The <c>EnemyPopulations_ConstructionZone1</c> list.</summary>
    public static RoomEnemyPopulationDefinition ConstructionZone1 { get; } = Find(nameof(ConstructionZone1));
    /// <summary>The <c>EnemyPopulations_BlueBrinstarETank1</c> list.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarETank1 { get; } = Find(nameof(BlueBrinstarETank1));
    /// <summary>The <c>EnemyPopulations_NoobBridge</c> list.</summary>
    public static RoomEnemyPopulationDefinition NoobBridge { get; } = Find(nameof(NoobBridge));
    /// <summary>The <c>EnemyPopulations_MorphBall1</c> list.</summary>
    public static RoomEnemyPopulationDefinition MorphBall1 { get; } = Find(nameof(MorphBall1));
    /// <summary>The <c>EnemyPopulations_WarehouseSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition WarehouseSave { get; } = Find(nameof(WarehouseSave));
    /// <summary>The <c>EnemyPopulations_MorphBall0</c> list.</summary>
    public static RoomEnemyPopulationDefinition MorphBall0 { get; } = Find(nameof(MorphBall0));
    /// <summary>The <c>EnemyPopulations_WarehouseZeela</c> list.</summary>
    public static RoomEnemyPopulationDefinition WarehouseZeela { get; } = Find(nameof(WarehouseZeela));
    /// <summary>The <c>EnemyPopulations_RedTower</c> list.</summary>
    public static RoomEnemyPopulationDefinition RedTower { get; } = Find(nameof(RedTower));
    /// <summary>The <c>EnemyPopulations_BlueBrinstarBoulders</c> list.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarBoulders { get; } = Find(nameof(BlueBrinstarBoulders));
    /// <summary>The <c>EnemyPopulations_BillyMays</c> list.</summary>
    public static RoomEnemyPopulationDefinition BillyMays { get; } = Find(nameof(BillyMays));
    /// <summary>The <c>EnemyPopulations_BigPinkSaveRoom</c> list.</summary>
    public static RoomEnemyPopulationDefinition BigPinkSaveRoom { get; } = Find(nameof(BigPinkSaveRoom));
    /// <summary>The <c>EnemyPopulations_BigPink</c> list.</summary>
    public static RoomEnemyPopulationDefinition BigPink { get; } = Find(nameof(BigPink));
    /// <summary>The <c>EnemyPopulations_EtecoonSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition EtecoonSave { get; } = Find(nameof(EtecoonSave));
    /// <summary>The <c>EnemyPopulations_BelowSpazer</c> list.</summary>
    public static RoomEnemyPopulationDefinition BelowSpazer { get; } = Find(nameof(BelowSpazer));
    /// <summary>The <c>EnemyPopulations_GreenBrinstarSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarSave { get; } = Find(nameof(GreenBrinstarSave));
    /// <summary>The <c>EnemyPopulations_Spazer</c> list.</summary>
    public static RoomEnemyPopulationDefinition Spazer { get; } = Find(nameof(Spazer));
    /// <summary>The <c>EnemyPopulations_PinkBrinstarPowerBombs</c> list.</summary>
    public static RoomEnemyPopulationDefinition PinkBrinstarPowerBombs { get; } = Find(nameof(PinkBrinstarPowerBombs));
    /// <summary>The <c>EnemyPopulations_RedTowerRefill</c> list.</summary>
    public static RoomEnemyPopulationDefinition RedTowerRefill { get; } = Find(nameof(RedTowerRefill));
    /// <summary>The <c>EnemyPopulations_KraidRefill</c> list.</summary>
    public static RoomEnemyPopulationDefinition KraidRefill { get; } = Find(nameof(KraidRefill));
    /// <summary>The <c>EnemyPopulations_VariaSuit</c> list.</summary>
    public static RoomEnemyPopulationDefinition VariaSuit { get; } = Find(nameof(VariaSuit));
    /// <summary>The <c>EnemyPopulations_RedBrinstarSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition RedBrinstarSave { get; } = Find(nameof(RedBrinstarSave));
    /// <summary>The <c>EnemyPopulations_BlueBrinstarETank0</c> list.</summary>
    public static RoomEnemyPopulationDefinition BlueBrinstarETank0 { get; } = Find(nameof(BlueBrinstarETank0));
    /// <summary>The <c>EnemyPopulations_GreenBrinstarFirefleas</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarFirefleas { get; } = Find(nameof(GreenBrinstarFirefleas));
    /// <summary>The <c>EnemyPopulations_GreenBrinstarBeetoms</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarBeetoms { get; } = Find(nameof(GreenBrinstarBeetoms));
    /// <summary>The <c>EnemyPopulations_EtecoonETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition EtecoonETank { get; } = Find(nameof(EtecoonETank));
    /// <summary>The <c>EnemyPopulations_PinkBrinstarHoppers</c> list.</summary>
    public static RoomEnemyPopulationDefinition PinkBrinstarHoppers { get; } = Find(nameof(PinkBrinstarHoppers));
    /// <summary>The <c>EnemyPopulations_WarehouseETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition WarehouseETank { get; } = Find(nameof(WarehouseETank));
    /// <summary>The <c>EnemyPopulations_WarehouseEntrance</c> list.</summary>
    public static RoomEnemyPopulationDefinition WarehouseEntrance { get; } = Find(nameof(WarehouseEntrance));
    /// <summary>The <c>EnemyPopulations_WarehouseKihunter</c> list.</summary>
    public static RoomEnemyPopulationDefinition WarehouseKihunter { get; } = Find(nameof(WarehouseKihunter));
    /// <summary>The <c>EnemyPopulations_GreenBrinstarMainShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenBrinstarMainShaft { get; } = Find(nameof(GreenBrinstarMainShaft));
    /// <summary>The <c>EnemyPopulations_SporeSpawnSuper</c> list.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawnSuper { get; } = Find(nameof(SporeSpawnSuper));
    /// <summary>The <c>EnemyPopulations_EarlySupers</c> list.</summary>
    public static RoomEnemyPopulationDefinition EarlySupers { get; } = Find(nameof(EarlySupers));
    /// <summary>The <c>EnemyPopulations_RedBrinstarFirefleas</c> list.</summary>
    public static RoomEnemyPopulationDefinition RedBrinstarFirefleas { get; } = Find(nameof(RedBrinstarFirefleas));
    /// <summary>The <c>EnemyPopulations_ConstructionZone0</c> list.</summary>
    public static RoomEnemyPopulationDefinition ConstructionZone0 { get; } = Find(nameof(ConstructionZone0));
    /// <summary>The <c>EnemyPopulations_GreenHillZone</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenHillZone { get; } = Find(nameof(GreenHillZone));
    /// <summary>The <c>EnemyPopulations_Dachora</c> list.</summary>
    public static RoomEnemyPopulationDefinition Dachora { get; } = Find(nameof(Dachora));
    /// <summary>The <c>EnemyPopulations_Hellway</c> list.</summary>
    public static RoomEnemyPopulationDefinition Hellway { get; } = Find(nameof(Hellway));
    /// <summary>The <c>EnemyPopulations_FirstMissile</c> list.</summary>
    public static RoomEnemyPopulationDefinition FirstMissile { get; } = Find(nameof(FirstMissile));
    /// <summary>The <c>EnemyPopulations_Kraid</c> list.</summary>
    public static RoomEnemyPopulationDefinition Kraid { get; } = Find(nameof(Kraid));
    /// <summary>The <c>EnemyPopulations_BrinstarReserveTank</c> list.</summary>
    public static RoomEnemyPopulationDefinition BrinstarReserveTank { get; } = Find(nameof(BrinstarReserveTank));
    /// <summary>The <c>EnemyPopulations_XrayScope</c> list.</summary>
    public static RoomEnemyPopulationDefinition XrayScope { get; } = Find(nameof(XrayScope));
    /// <summary>The <c>EnemyPopulations_StatuesHallway</c> list.</summary>
    public static RoomEnemyPopulationDefinition StatuesHallway { get; } = Find(nameof(StatuesHallway));
    /// <summary>The <c>EnemyPopulations_AlphaPowerBomb</c> list.</summary>
    public static RoomEnemyPopulationDefinition AlphaPowerBomb { get; } = Find(nameof(AlphaPowerBomb));
    /// <summary>The <c>EnemyPopulations_KraidEyeDoor</c> list.</summary>
    public static RoomEnemyPopulationDefinition KraidEyeDoor { get; } = Find(nameof(KraidEyeDoor));
    /// <summary>The <c>EnemyPopulations_Caterpillar</c> list.</summary>
    public static RoomEnemyPopulationDefinition Caterpillar { get; } = Find(nameof(Caterpillar));
    /// <summary>The <c>EnemyPopulations_MiniKraid</c> list.</summary>
    public static RoomEnemyPopulationDefinition MiniKraid { get; } = Find(nameof(MiniKraid));
    /// <summary>The <c>EnemyPopulations_SporeSpawn</c> list.</summary>
    public static RoomEnemyPopulationDefinition SporeSpawn { get; } = Find(nameof(SporeSpawn));
    /// <summary>The <c>EnemyPopulations_SkreeBoost</c> list.</summary>
    public static RoomEnemyPopulationDefinition SkreeBoost { get; } = Find(nameof(SkreeBoost));
    /// <summary>The <c>EnemyPopulations_GrappleTutorial3</c> list.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial3 { get; } = Find(nameof(GrappleTutorial3));
    /// <summary>The <c>EnemyPopulations_PostCrocShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition PostCrocShaft { get; } = Find(nameof(PostCrocShaft));
    /// <summary>The <c>EnemyPopulations_MetalPirates</c> list.</summary>
    public static RoomEnemyPopulationDefinition MetalPirates { get; } = Find(nameof(MetalPirates));
    /// <summary>The <c>EnemyPopulations_AcidStatue</c> list.</summary>
    public static RoomEnemyPopulationDefinition AcidStatue { get; } = Find(nameof(AcidStatue));
    /// <summary>The <c>EnemyPopulations_PurpleFarming</c> list.</summary>
    public static RoomEnemyPopulationDefinition PurpleFarming { get; } = Find(nameof(PurpleFarming));
    /// <summary>The <c>EnemyPopulations_IceBeamGate</c> list.</summary>
    public static RoomEnemyPopulationDefinition IceBeamGate { get; } = Find(nameof(IceBeamGate));
    /// <summary>The <c>EnemyPopulations_SpikyAcidSnakesTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpikyAcidSnakesTunnel { get; } = Find(nameof(SpikyAcidSnakesTunnel));
    /// <summary>The <c>EnemyPopulations_RedKihunterShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition RedKihunterShaft { get; } = Find(nameof(RedKihunterShaft));
    /// <summary>The <c>EnemyPopulations_FastRipper</c> list.</summary>
    public static RoomEnemyPopulationDefinition FastRipper { get; } = Find(nameof(FastRipper));
    /// <summary>The <c>EnemyPopulations_WaveBeam</c> list.</summary>
    public static RoomEnemyPopulationDefinition WaveBeam { get; } = Find(nameof(WaveBeam));
    /// <summary>The <c>EnemyPopulations_FastPillarsSetup</c> list.</summary>
    public static RoomEnemyPopulationDefinition FastPillarsSetup { get; } = Find(nameof(FastPillarsSetup));
    /// <summary>The <c>EnemyPopulations_LowerNorfairElev</c> list.</summary>
    public static RoomEnemyPopulationDefinition LowerNorfairElev { get; } = Find(nameof(LowerNorfairElev));
    /// <summary>The <c>EnemyPopulations_LNSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition LNSave { get; } = Find(nameof(LNSave));
    /// <summary>The <c>EnemyPopulations_UpperNorfairRefill</c> list.</summary>
    public static RoomEnemyPopulationDefinition UpperNorfairRefill { get; } = Find(nameof(UpperNorfairRefill));
    /// <summary>The <c>EnemyPopulations_NorfairMap</c> list.</summary>
    public static RoomEnemyPopulationDefinition NorfairMap { get; } = Find(nameof(NorfairMap));
    /// <summary>The <c>EnemyPopulations_MickeyMouse</c> list.</summary>
    public static RoomEnemyPopulationDefinition MickeyMouse { get; } = Find(nameof(MickeyMouse));
    /// <summary>The <c>EnemyPopulations_GTEnergyRefill</c> list.</summary>
    public static RoomEnemyPopulationDefinition GTEnergyRefill { get; } = Find(nameof(GTEnergyRefill));
    /// <summary>The <c>EnemyPopulations_Ridley</c> list.</summary>
    public static RoomEnemyPopulationDefinition Ridley { get; } = Find(nameof(Ridley));
    /// <summary>The <c>EnemyPopulations_PostCrocPowerBombs</c> list.</summary>
    public static RoomEnemyPopulationDefinition PostCrocPowerBombs { get; } = Find(nameof(PostCrocPowerBombs));
    /// <summary>The <c>EnemyPopulations_IceBeam</c> list.</summary>
    public static RoomEnemyPopulationDefinition IceBeam { get; } = Find(nameof(IceBeam));
    /// <summary>The <c>EnemyPopulations_HiJumpBoots</c> list.</summary>
    public static RoomEnemyPopulationDefinition HiJumpBoots { get; } = Find(nameof(HiJumpBoots));
    /// <summary>The <c>EnemyPopulations_PostCrocSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition PostCrocSave { get; } = Find(nameof(PostCrocSave));
    /// <summary>The <c>EnemyPopulations_FrogSpeedway</c> list.</summary>
    public static RoomEnemyPopulationDefinition FrogSpeedway { get; } = Find(nameof(FrogSpeedway));
    /// <summary>The <c>EnemyPopulations_RisingTide</c> list.</summary>
    public static RoomEnemyPopulationDefinition RisingTide { get; } = Find(nameof(RisingTide));
    /// <summary>The <c>EnemyPopulations_PostCrocFarming</c> list.</summary>
    public static RoomEnemyPopulationDefinition PostCrocFarming { get; } = Find(nameof(PostCrocFarming));
    /// <summary>The <c>EnemyPopulations_WorstRoomInTheGame</c> list.</summary>
    public static RoomEnemyPopulationDefinition WorstRoomInTheGame { get; } = Find(nameof(WorstRoomInTheGame));
    /// <summary>The <c>EnemyPopulations_DoubleChamber</c> list.</summary>
    public static RoomEnemyPopulationDefinition DoubleChamber { get; } = Find(nameof(DoubleChamber));
    /// <summary>The <c>EnemyPopulations_GrappleBeam</c> list.</summary>
    public static RoomEnemyPopulationDefinition GrappleBeam { get; } = Find(nameof(GrappleBeam));
    /// <summary>The <c>EnemyPopulations_RedPirateShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition RedPirateShaft { get; } = Find(nameof(RedPirateShaft));
    /// <summary>The <c>EnemyPopulations_CrocomireSpeedway</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrocomireSpeedway { get; } = Find(nameof(CrocomireSpeedway));
    /// <summary>The <c>EnemyPopulations_Plowerhouse</c> list.</summary>
    public static RoomEnemyPopulationDefinition Plowerhouse { get; } = Find(nameof(Plowerhouse));
    /// <summary>The <c>EnemyPopulations_LNFireflea</c> list.</summary>
    public static RoomEnemyPopulationDefinition LNFireflea { get; } = Find(nameof(LNFireflea));
    /// <summary>The <c>EnemyPopulations_MagdolliteTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition MagdolliteTunnel { get; } = Find(nameof(MagdolliteTunnel));
    /// <summary>The <c>EnemyPopulations_UnusedRoom8FB3EEA1AD06</c> list.</summary>
    public static RoomEnemyPopulationDefinition UnusedRoom8FB3EEA1AD06 { get; } = Find(nameof(UnusedRoom8FB3EEA1AD06));
    /// <summary>The <c>EnemyPopulations_LavaDive</c> list.</summary>
    public static RoomEnemyPopulationDefinition LavaDive { get; } = Find(nameof(LavaDive));
    /// <summary>The <c>EnemyPopulations_Pillar</c> list.</summary>
    public static RoomEnemyPopulationDefinition Pillar { get; } = Find(nameof(Pillar));
    /// <summary>The <c>EnemyPopulations_CrocomireEscape</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrocomireEscape { get; } = Find(nameof(CrocomireEscape));
    /// <summary>The <c>EnemyPopulations_LNSpringBallMaze</c> list.</summary>
    public static RoomEnemyPopulationDefinition LNSpringBallMaze { get; } = Find(nameof(LNSpringBallMaze));
    /// <summary>The <c>EnemyPopulations_LNEscapePowerBombs</c> list.</summary>
    public static RoomEnemyPopulationDefinition LNEscapePowerBombs { get; } = Find(nameof(LNEscapePowerBombs));
    /// <summary>The <c>EnemyPopulations_PurpleShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition PurpleShaft { get; } = Find(nameof(PurpleShaft));
    /// <summary>The <c>EnemyPopulations_RidleyETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition RidleyETank { get; } = Find(nameof(RidleyETank));
    /// <summary>The <c>EnemyPopulations_ScrewAttack</c> list.</summary>
    public static RoomEnemyPopulationDefinition ScrewAttack { get; } = Find(nameof(ScrewAttack));
    /// <summary>The <c>EnemyPopulations_GreenBubblesMissiles</c> list.</summary>
    public static RoomEnemyPopulationDefinition GreenBubblesMissiles { get; } = Find(nameof(GreenBubblesMissiles));
    /// <summary>The <c>EnemyPopulations_Amphitheatre</c> list.</summary>
    public static RoomEnemyPopulationDefinition Amphitheatre { get; } = Find(nameof(Amphitheatre));
    /// <summary>The <c>EnemyPopulations_CathedralEntrance</c> list.</summary>
    public static RoomEnemyPopulationDefinition CathedralEntrance { get; } = Find(nameof(CathedralEntrance));
    /// <summary>The <c>EnemyPopulations_MainHall</c> list.</summary>
    public static RoomEnemyPopulationDefinition MainHall { get; } = Find(nameof(MainHall));
    /// <summary>The <c>EnemyPopulations_PostCrocJump</c> list.</summary>
    public static RoomEnemyPopulationDefinition PostCrocJump { get; } = Find(nameof(PostCrocJump));
    /// <summary>The <c>EnemyPopulations_FrogSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition FrogSave { get; } = Find(nameof(FrogSave));
    /// <summary>The <c>EnemyPopulations_CrocomireSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrocomireSave { get; } = Find(nameof(CrocomireSave));
    /// <summary>The <c>EnemyPopulations_CrumbleShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrumbleShaft { get; } = Find(nameof(CrumbleShaft));
    /// <summary>The <c>EnemyPopulations_Wasteland</c> list.</summary>
    public static RoomEnemyPopulationDefinition Wasteland { get; } = Find(nameof(Wasteland));
    /// <summary>The <c>EnemyPopulations_AcidSnakesTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition AcidSnakesTunnel { get; } = Find(nameof(AcidSnakesTunnel));
    /// <summary>The <c>EnemyPopulations_GrappleTutorial1</c> list.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial1 { get; } = Find(nameof(GrappleTutorial1));
    /// <summary>The <c>EnemyPopulations_BubbleMountainSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition BubbleMountainSave { get; } = Find(nameof(BubbleMountainSave));
    /// <summary>The <c>EnemyPopulations_HiJumpETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition HiJumpETank { get; } = Find(nameof(HiJumpETank));
    /// <summary>The <c>EnemyPopulations_BubbleMountain</c> list.</summary>
    public static RoomEnemyPopulationDefinition BubbleMountain { get; } = Find(nameof(BubbleMountain));
    /// <summary>The <c>EnemyPopulations_IceBeamAcid</c> list.</summary>
    public static RoomEnemyPopulationDefinition IceBeamAcid { get; } = Find(nameof(IceBeamAcid));
    /// <summary>The <c>EnemyPopulations_IceBeamTutorial</c> list.</summary>
    public static RoomEnemyPopulationDefinition IceBeamTutorial { get; } = Find(nameof(IceBeamTutorial));
    /// <summary>The <c>EnemyPopulations_PostCrocMissile</c> list.</summary>
    public static RoomEnemyPopulationDefinition PostCrocMissile { get; } = Find(nameof(PostCrocMissile));
    /// <summary>The <c>EnemyPopulations_Cathedral</c> list.</summary>
    public static RoomEnemyPopulationDefinition Cathedral { get; } = Find(nameof(Cathedral));
    /// <summary>The <c>EnemyPopulations_NorfairReserveTank</c> list.</summary>
    public static RoomEnemyPopulationDefinition NorfairReserveTank { get; } = Find(nameof(NorfairReserveTank));
    /// <summary>The <c>EnemyPopulations_GrappleTutorial2</c> list.</summary>
    public static RoomEnemyPopulationDefinition GrappleTutorial2 { get; } = Find(nameof(GrappleTutorial2));
    /// <summary>The <c>EnemyPopulations_IceBeamSnake</c> list.</summary>
    public static RoomEnemyPopulationDefinition IceBeamSnake { get; } = Find(nameof(IceBeamSnake));
    /// <summary>The <c>EnemyPopulations_GoldenTorizo</c> list.</summary>
    public static RoomEnemyPopulationDefinition GoldenTorizo { get; } = Find(nameof(GoldenTorizo));
    /// <summary>The <c>EnemyPopulations_SpikyPlatformsTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpikyPlatformsTunnel { get; } = Find(nameof(SpikyPlatformsTunnel));
    /// <summary>The <c>EnemyPopulations_SpeedBooster</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpeedBooster { get; } = Find(nameof(SpeedBooster));
    /// <summary>The <c>EnemyPopulations_LNFarming</c> list.</summary>
    public static RoomEnemyPopulationDefinition LNFarming { get; } = Find(nameof(LNFarming));
    /// <summary>The <c>EnemyPopulations_SingleChamber</c> list.</summary>
    public static RoomEnemyPopulationDefinition SingleChamber { get; } = Find(nameof(SingleChamber));
    /// <summary>The <c>EnemyPopulations_SpeedBoosterHall</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpeedBoosterHall { get; } = Find(nameof(SpeedBoosterHall));
    /// <summary>The <c>EnemyPopulations_BatCave</c> list.</summary>
    public static RoomEnemyPopulationDefinition BatCave { get; } = Find(nameof(BatCave));
    /// <summary>The <c>EnemyPopulations_KronicBoost</c> list.</summary>
    public static RoomEnemyPopulationDefinition KronicBoost { get; } = Find(nameof(KronicBoost));
    /// <summary>The <c>EnemyPopulations_BusinessCenter</c> list.</summary>
    public static RoomEnemyPopulationDefinition BusinessCenter { get; } = Find(nameof(BusinessCenter));
    /// <summary>The <c>EnemyPopulations_ThreeMusketeers</c> list.</summary>
    public static RoomEnemyPopulationDefinition ThreeMusketeers { get; } = Find(nameof(ThreeMusketeers));
    /// <summary>The <c>EnemyPopulations_Crocomire</c> list.</summary>
    public static RoomEnemyPopulationDefinition Crocomire { get; } = Find(nameof(Crocomire));
    /// <summary>The <c>EnemyPopulations_LNElevSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition LNElevSave { get; } = Find(nameof(LNElevSave));
    /// <summary>The <c>EnemyPopulations_Volcano</c> list.</summary>
    public static RoomEnemyPopulationDefinition Volcano { get; } = Find(nameof(Volcano));
    /// <summary>The <c>EnemyPopulations_UpperNorfairFarming</c> list.</summary>
    public static RoomEnemyPopulationDefinition UpperNorfairFarming { get; } = Find(nameof(UpperNorfairFarming));
    /// <summary>The <c>EnemyPopulations_SpongeBath1</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpongeBath1 { get; } = Find(nameof(SpongeBath1));
    /// <summary>The <c>EnemyPopulations_WreckedShipETank1</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipETank1 { get; } = Find(nameof(WreckedShipETank1));
    /// <summary>The <c>EnemyPopulations_WreckedShipMainShaft0</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMainShaft0 { get; } = Find(nameof(WreckedShipMainShaft0));
    /// <summary>The <c>EnemyPopulations_BowlingAlley0</c> list.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlley0 { get; } = Find(nameof(BowlingAlley0));
    /// <summary>The <c>EnemyPopulations_Attic1</c> list.</summary>
    public static RoomEnemyPopulationDefinition Attic1 { get; } = Find(nameof(Attic1));
    /// <summary>The <c>EnemyPopulations_Basement1</c> list.</summary>
    public static RoomEnemyPopulationDefinition Basement1 { get; } = Find(nameof(Basement1));
    /// <summary>The <c>EnemyPopulations_WreckedShipMap1</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMap1 { get; } = Find(nameof(WreckedShipMap1));
    /// <summary>The <c>EnemyPopulations_WSWestSuper0</c> list.</summary>
    public static RoomEnemyPopulationDefinition WSWestSuper0 { get; } = Find(nameof(WSWestSuper0));
    /// <summary>The <c>EnemyPopulations_SpikyDeath0</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpikyDeath0 { get; } = Find(nameof(SpikyDeath0));
    /// <summary>The <c>EnemyPopulations_SpongeBath0</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpongeBath0 { get; } = Find(nameof(SpongeBath0));
    /// <summary>The <c>EnemyPopulations_WreckedShipEntrance1</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEntrance1 { get; } = Find(nameof(WreckedShipEntrance1));
    /// <summary>The <c>EnemyPopulations_ElectricDeath0</c> list.</summary>
    public static RoomEnemyPopulationDefinition ElectricDeath0 { get; } = Find(nameof(ElectricDeath0));
    /// <summary>The <c>EnemyPopulations_BowlingAlley1</c> list.</summary>
    public static RoomEnemyPopulationDefinition BowlingAlley1 { get; } = Find(nameof(BowlingAlley1));
    /// <summary>The <c>EnemyPopulations_AssemblyLine0</c> list.</summary>
    public static RoomEnemyPopulationDefinition AssemblyLine0 { get; } = Find(nameof(AssemblyLine0));
    /// <summary>The <c>EnemyPopulations_Phantoon1</c> list.</summary>
    public static RoomEnemyPopulationDefinition Phantoon1 { get; } = Find(nameof(Phantoon1));
    /// <summary>The <c>EnemyPopulations_WreckedShipETank0</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipETank0 { get; } = Find(nameof(WreckedShipETank0));
    /// <summary>The <c>EnemyPopulations_GravitySuit0</c> list.</summary>
    public static RoomEnemyPopulationDefinition GravitySuit0 { get; } = Find(nameof(GravitySuit0));
    /// <summary>The <c>EnemyPopulations_WreckedShipMap0</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMap0 { get; } = Find(nameof(WreckedShipMap0));
    /// <summary>The <c>EnemyPopulations_GravitySuit1</c> list.</summary>
    public static RoomEnemyPopulationDefinition GravitySuit1 { get; } = Find(nameof(GravitySuit1));
    /// <summary>The <c>EnemyPopulations_Basement0</c> list.</summary>
    public static RoomEnemyPopulationDefinition Basement0 { get; } = Find(nameof(Basement0));
    /// <summary>The <c>EnemyPopulations_WreckedShipEntrance0</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipEntrance0 { get; } = Find(nameof(WreckedShipEntrance0));
    /// <summary>The <c>EnemyPopulations_AssemblyLine1</c> list.</summary>
    public static RoomEnemyPopulationDefinition AssemblyLine1 { get; } = Find(nameof(AssemblyLine1));
    /// <summary>The <c>EnemyPopulations_WreckedShipSave1</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipSave1 { get; } = Find(nameof(WreckedShipSave1));
    /// <summary>The <c>EnemyPopulations_WSWestSuper1</c> list.</summary>
    public static RoomEnemyPopulationDefinition WSWestSuper1 { get; } = Find(nameof(WSWestSuper1));
    /// <summary>The <c>EnemyPopulations_Attic0</c> list.</summary>
    public static RoomEnemyPopulationDefinition Attic0 { get; } = Find(nameof(Attic0));
    /// <summary>The <c>EnemyPopulations_WSEastSuper0</c> list.</summary>
    public static RoomEnemyPopulationDefinition WSEastSuper0 { get; } = Find(nameof(WSEastSuper0));
    /// <summary>The <c>EnemyPopulations_WreckedShipSave0</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipSave0 { get; } = Find(nameof(WreckedShipSave0));
    /// <summary>The <c>EnemyPopulations_ElectricDeath1</c> list.</summary>
    public static RoomEnemyPopulationDefinition ElectricDeath1 { get; } = Find(nameof(ElectricDeath1));
    /// <summary>The <c>EnemyPopulations_SpikyDeath1</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpikyDeath1 { get; } = Find(nameof(SpikyDeath1));
    /// <summary>The <c>EnemyPopulations_WSEastSuper1</c> list.</summary>
    public static RoomEnemyPopulationDefinition WSEastSuper1 { get; } = Find(nameof(WSEastSuper1));
    /// <summary>The <c>EnemyPopulations_Phantoon0</c> list.</summary>
    public static RoomEnemyPopulationDefinition Phantoon0 { get; } = Find(nameof(Phantoon0));
    /// <summary>The <c>EnemyPopulations_WreckedShipMainShaft1</c> list.</summary>
    public static RoomEnemyPopulationDefinition WreckedShipMainShaft1 { get; } = Find(nameof(WreckedShipMainShaft1));
    /// <summary>The <c>EnemyPopulations_MtEverest</c> list.</summary>
    public static RoomEnemyPopulationDefinition MtEverest { get; } = Find(nameof(MtEverest));
    /// <summary>The <c>EnemyPopulations_HalfieClimb</c> list.</summary>
    public static RoomEnemyPopulationDefinition HalfieClimb { get; } = Find(nameof(HalfieClimb));
    /// <summary>The <c>EnemyPopulations_WateringHole</c> list.</summary>
    public static RoomEnemyPopulationDefinition WateringHole { get; } = Find(nameof(WateringHole));
    /// <summary>The <c>EnemyPopulations_CrabShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrabShaft { get; } = Find(nameof(CrabShaft));
    /// <summary>The <c>EnemyPopulations_MaridiaElev</c> list.</summary>
    public static RoomEnemyPopulationDefinition MaridiaElev { get; } = Find(nameof(MaridiaElev));
    /// <summary>The <c>EnemyPopulations_Colosseum</c> list.</summary>
    public static RoomEnemyPopulationDefinition Colosseum { get; } = Find(nameof(Colosseum));
    /// <summary>The <c>EnemyPopulations_AqueductSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition AqueductSave { get; } = Find(nameof(AqueductSave));
    /// <summary>The <c>EnemyPopulations_ThePrecious</c> list.</summary>
    public static RoomEnemyPopulationDefinition ThePrecious { get; } = Find(nameof(ThePrecious));
    /// <summary>The <c>EnemyPopulations_BotwoonETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition BotwoonETank { get; } = Find(nameof(BotwoonETank));
    /// <summary>The <c>EnemyPopulations_DraygonSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition DraygonSave { get; } = Find(nameof(DraygonSave));
    /// <summary>The <c>EnemyPopulations_MaridiaMissileRefill</c> list.</summary>
    public static RoomEnemyPopulationDefinition MaridiaMissileRefill { get; } = Find(nameof(MaridiaMissileRefill));
    /// <summary>The <c>EnemyPopulations_PlasmaBeachQuicksand</c> list.</summary>
    public static RoomEnemyPopulationDefinition PlasmaBeachQuicksand { get; } = Find(nameof(PlasmaBeachQuicksand));
    /// <summary>The <c>EnemyPopulations_BotwoonQuicksand</c> list.</summary>
    public static RoomEnemyPopulationDefinition BotwoonQuicksand { get; } = Find(nameof(BotwoonQuicksand));
    /// <summary>The <c>EnemyPopulations_Shaktool</c> list.</summary>
    public static RoomEnemyPopulationDefinition Shaktool { get; } = Find(nameof(Shaktool));
    /// <summary>The <c>EnemyPopulations_Draygon0</c> list.</summary>
    public static RoomEnemyPopulationDefinition Draygon0 { get; } = Find(nameof(Draygon0));
    /// <summary>The <c>EnemyPopulations_Aqueduct</c> list.</summary>
    public static RoomEnemyPopulationDefinition Aqueduct { get; } = Find(nameof(Aqueduct));
    /// <summary>The <c>EnemyPopulations_RedFish</c> list.</summary>
    public static RoomEnemyPopulationDefinition RedFish { get; } = Find(nameof(RedFish));
    /// <summary>The <c>EnemyPopulations_Plasma</c> list.</summary>
    public static RoomEnemyPopulationDefinition Plasma { get; } = Find(nameof(Plasma));
    /// <summary>The <c>EnemyPopulations_SpaceJump</c> list.</summary>
    public static RoomEnemyPopulationDefinition SpaceJump { get; } = Find(nameof(SpaceJump));
    /// <summary>The <c>EnemyPopulations_Pants</c> list.</summary>
    public static RoomEnemyPopulationDefinition Pants { get; } = Find(nameof(Pants));
    /// <summary>The <c>EnemyPopulations_GlassTunnelSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition GlassTunnelSave { get; } = Find(nameof(GlassTunnelSave));
    /// <summary>The <c>EnemyPopulations_GlassTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition GlassTunnel { get; } = Find(nameof(GlassTunnel));
    /// <summary>The <c>EnemyPopulations_WestTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition WestTunnel { get; } = Find(nameof(WestTunnel));
    /// <summary>The <c>EnemyPopulations_EastTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition EastTunnel { get; } = Find(nameof(EastTunnel));
    /// <summary>The <c>EnemyPopulations_MamaTurtle</c> list.</summary>
    public static RoomEnemyPopulationDefinition MamaTurtle { get; } = Find(nameof(MamaTurtle));
    /// <summary>The <c>EnemyPopulations_CrabTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrabTunnel { get; } = Find(nameof(CrabTunnel));
    /// <summary>The <c>EnemyPopulations_NWestMaridiaBug</c> list.</summary>
    public static RoomEnemyPopulationDefinition NWestMaridiaBug { get; } = Find(nameof(NWestMaridiaBug));
    /// <summary>The <c>EnemyPopulations_PseudoPlasmaSpark</c> list.</summary>
    public static RoomEnemyPopulationDefinition PseudoPlasmaSpark { get; } = Find(nameof(PseudoPlasmaSpark));
    /// <summary>The <c>EnemyPopulations_WestSandHallTunnel</c> list.</summary>
    public static RoomEnemyPopulationDefinition WestSandHallTunnel { get; } = Find(nameof(WestSandHallTunnel));
    /// <summary>The <c>EnemyPopulations_PlasmaTutorial</c> list.</summary>
    public static RoomEnemyPopulationDefinition PlasmaTutorial { get; } = Find(nameof(PlasmaTutorial));
    /// <summary>The <c>EnemyPopulations_ThreadTheNeedle</c> list.</summary>
    public static RoomEnemyPopulationDefinition ThreadTheNeedle { get; } = Find(nameof(ThreadTheNeedle));
    /// <summary>The <c>EnemyPopulations_PlasmaSpark</c> list.</summary>
    public static RoomEnemyPopulationDefinition PlasmaSpark { get; } = Find(nameof(PlasmaSpark));
    /// <summary>The <c>EnemyPopulations_Toilet</c> list.</summary>
    public static RoomEnemyPopulationDefinition Toilet { get; } = Find(nameof(Toilet));
    /// <summary>The <c>EnemyPopulations_WestSandHall</c> list.</summary>
    public static RoomEnemyPopulationDefinition WestSandHall { get; } = Find(nameof(WestSandHall));
    /// <summary>The <c>EnemyPopulations_Oasis</c> list.</summary>
    public static RoomEnemyPopulationDefinition Oasis { get; } = Find(nameof(Oasis));
    /// <summary>The <c>EnemyPopulations_EastSandHall</c> list.</summary>
    public static RoomEnemyPopulationDefinition EastSandHall { get; } = Find(nameof(EastSandHall));
    /// <summary>The <c>EnemyPopulations_WestAqueductQuicksand</c> list.</summary>
    public static RoomEnemyPopulationDefinition WestAqueductQuicksand { get; } = Find(nameof(WestAqueductQuicksand));
    /// <summary>The <c>EnemyPopulations_EastAqueductQuicksand</c> list.</summary>
    public static RoomEnemyPopulationDefinition EastAqueductQuicksand { get; } = Find(nameof(EastAqueductQuicksand));
    /// <summary>The <c>EnemyPopulations_Butterfly</c> list.</summary>
    public static RoomEnemyPopulationDefinition Butterfly { get; } = Find(nameof(Butterfly));
    /// <summary>The <c>EnemyPopulations_BotwoonHallway</c> list.</summary>
    public static RoomEnemyPopulationDefinition BotwoonHallway { get; } = Find(nameof(BotwoonHallway));
    /// <summary>The <c>EnemyPopulations_EastPants</c> list.</summary>
    public static RoomEnemyPopulationDefinition EastPants { get; } = Find(nameof(EastPants));
    /// <summary>The <c>EnemyPopulations_Springball</c> list.</summary>
    public static RoomEnemyPopulationDefinition Springball { get; } = Find(nameof(Springball));
    /// <summary>The <c>EnemyPopulations_BelowBotwoonETank</c> list.</summary>
    public static RoomEnemyPopulationDefinition BelowBotwoonETank { get; } = Find(nameof(BelowBotwoonETank));
    /// <summary>The <c>EnemyPopulations_Kassiuz</c> list.</summary>
    public static RoomEnemyPopulationDefinition Kassiuz { get; } = Find(nameof(Kassiuz));
    /// <summary>The <c>EnemyPopulations_ForgottenHighwaySave</c> list.</summary>
    public static RoomEnemyPopulationDefinition ForgottenHighwaySave { get; } = Find(nameof(ForgottenHighwaySave));
    /// <summary>The <c>EnemyPopulations_MaridiaEnergyRefill</c> list.</summary>
    public static RoomEnemyPopulationDefinition MaridiaEnergyRefill { get; } = Find(nameof(MaridiaEnergyRefill));
    /// <summary>The <c>EnemyPopulations_MaridiaMap</c> list.</summary>
    public static RoomEnemyPopulationDefinition MaridiaMap { get; } = Find(nameof(MaridiaMap));
    /// <summary>The <c>EnemyPopulations_CrabHole</c> list.</summary>
    public static RoomEnemyPopulationDefinition CrabHole { get; } = Find(nameof(CrabHole));
    /// <summary>The <c>EnemyPopulations_Botwoon</c> list.</summary>
    public static RoomEnemyPopulationDefinition Botwoon { get; } = Find(nameof(Botwoon));
    /// <summary>The <c>EnemyPopulations_MainStreet</c> list.</summary>
    public static RoomEnemyPopulationDefinition MainStreet { get; } = Find(nameof(MainStreet));
    /// <summary>The <c>EnemyPopulations_BugSandHole</c> list.</summary>
    public static RoomEnemyPopulationDefinition BugSandHole { get; } = Find(nameof(BugSandHole));
    /// <summary>The <c>EnemyPopulations_EastSandHole</c> list.</summary>
    public static RoomEnemyPopulationDefinition EastSandHole { get; } = Find(nameof(EastSandHole));
    /// <summary>The <c>EnemyPopulations_WestSandHole</c> list.</summary>
    public static RoomEnemyPopulationDefinition WestSandHole { get; } = Find(nameof(WestSandHole));
    /// <summary>The <c>EnemyPopulations_WestCactusAlley</c> list.</summary>
    public static RoomEnemyPopulationDefinition WestCactusAlley { get; } = Find(nameof(WestCactusAlley));
    /// <summary>The <c>EnemyPopulations_EastCactusAlley</c> list.</summary>
    public static RoomEnemyPopulationDefinition EastCactusAlley { get; } = Find(nameof(EastCactusAlley));
    /// <summary>The <c>EnemyPopulations_FishTank</c> list.</summary>
    public static RoomEnemyPopulationDefinition FishTank { get; } = Find(nameof(FishTank));
    /// <summary>The <c>EnemyPopulations_TourianEscape4</c> list.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape4 { get; } = Find(nameof(TourianEscape4));
    /// <summary>The <c>EnemyPopulations_MotherBrainSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition MotherBrainSave { get; } = Find(nameof(MotherBrainSave));
    /// <summary>The <c>EnemyPopulations_Metroids10</c> list.</summary>
    public static RoomEnemyPopulationDefinition Metroids10 { get; } = Find(nameof(Metroids10));
    /// <summary>The <c>EnemyPopulations_DustTorizo</c> list.</summary>
    public static RoomEnemyPopulationDefinition DustTorizo { get; } = Find(nameof(DustTorizo));
    /// <summary>The <c>EnemyPopulations_BigBoy</c> list.</summary>
    public static RoomEnemyPopulationDefinition BigBoy { get; } = Find(nameof(BigBoy));
    /// <summary>The <c>EnemyPopulations_MotherBrain01</c> list.</summary>
    public static RoomEnemyPopulationDefinition MotherBrain01 { get; } = Find(nameof(MotherBrain01));
    /// <summary>The <c>EnemyPopulations_TourianEyeDoor</c> list.</summary>
    public static RoomEnemyPopulationDefinition TourianEyeDoor { get; } = Find(nameof(TourianEyeDoor));
    /// <summary>The <c>EnemyPopulations_BlueHopper</c> list.</summary>
    public static RoomEnemyPopulationDefinition BlueHopper { get; } = Find(nameof(BlueHopper));
    /// <summary>The <c>EnemyPopulations_TourianEscape1</c> list.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape1 { get; } = Find(nameof(TourianEscape1));
    /// <summary>The <c>EnemyPopulations_TourianFirst</c> list.</summary>
    public static RoomEnemyPopulationDefinition TourianFirst { get; } = Find(nameof(TourianFirst));
    /// <summary>The <c>EnemyPopulations_Metroids20</c> list.</summary>
    public static RoomEnemyPopulationDefinition Metroids20 { get; } = Find(nameof(Metroids20));
    /// <summary>The <c>EnemyPopulations_Metroids40</c> list.</summary>
    public static RoomEnemyPopulationDefinition Metroids40 { get; } = Find(nameof(Metroids40));
    /// <summary>The <c>EnemyPopulations_Metroids11</c> list.</summary>
    public static RoomEnemyPopulationDefinition Metroids11 { get; } = Find(nameof(Metroids11));
    /// <summary>The <c>EnemyPopulations_Metroids21</c> list.</summary>
    public static RoomEnemyPopulationDefinition Metroids21 { get; } = Find(nameof(Metroids21));
    /// <summary>The <c>EnemyPopulations_TourianEscape2</c> list.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape2 { get; } = Find(nameof(TourianEscape2));
    /// <summary>The <c>EnemyPopulations_Metroids31</c> list.</summary>
    public static RoomEnemyPopulationDefinition Metroids31 { get; } = Find(nameof(Metroids31));
    /// <summary>The <c>EnemyPopulations_Metroids41</c> list.</summary>
    public static RoomEnemyPopulationDefinition Metroids41 { get; } = Find(nameof(Metroids41));
    /// <summary>The <c>EnemyPopulations_TourianEscape3</c> list.</summary>
    public static RoomEnemyPopulationDefinition TourianEscape3 { get; } = Find(nameof(TourianEscape3));
    /// <summary>The <c>EnemyPopulations_Seaweed</c> list.</summary>
    public static RoomEnemyPopulationDefinition Seaweed { get; } = Find(nameof(Seaweed));
    /// <summary>The <c>EnemyPopulations_TourianRecharge</c> list.</summary>
    public static RoomEnemyPopulationDefinition TourianRecharge { get; } = Find(nameof(TourianRecharge));
    /// <summary>The <c>EnemyPopulations_UpperTourianSave</c> list.</summary>
    public static RoomEnemyPopulationDefinition UpperTourianSave { get; } = Find(nameof(UpperTourianSave));
    /// <summary>The <c>EnemyPopulations_Metroids30</c> list.</summary>
    public static RoomEnemyPopulationDefinition Metroids30 { get; } = Find(nameof(Metroids30));
    /// <summary>The <c>EnemyPopulations_RinkaShaft</c> list.</summary>
    public static RoomEnemyPopulationDefinition RinkaShaft { get; } = Find(nameof(RinkaShaft));
    /// <summary>The <c>EnemyPopulations_CeresElev0</c> list.</summary>
    public static RoomEnemyPopulationDefinition CeresElev0 { get; } = Find(nameof(CeresElev0));
    /// <summary>The <c>EnemyPopulations_FallingTile0</c> list.</summary>
    public static RoomEnemyPopulationDefinition FallingTile0 { get; } = Find(nameof(FallingTile0));
    /// <summary>The <c>EnemyPopulations_MagnetStairs0</c> list.</summary>
    public static RoomEnemyPopulationDefinition MagnetStairs0 { get; } = Find(nameof(MagnetStairs0));
    /// <summary>The <c>EnemyPopulations_DeadScientist0</c> list.</summary>
    public static RoomEnemyPopulationDefinition DeadScientist0 { get; } = Find(nameof(DeadScientist0));
    /// <summary>The <c>EnemyPopulations_Room58Escape0</c> list.</summary>
    public static RoomEnemyPopulationDefinition Room58Escape0 { get; } = Find(nameof(Room58Escape0));
    /// <summary>The <c>EnemyPopulations_CeresRidley0</c> list.</summary>
    public static RoomEnemyPopulationDefinition CeresRidley0 { get; } = Find(nameof(CeresRidley0));
    /// <summary>The <c>EnemyPopulations_CeresElev1</c> list.</summary>
    public static RoomEnemyPopulationDefinition CeresElev1 { get; } = Find(nameof(CeresElev1));
    /// <summary>The <c>EnemyPopulations_FallingTile1</c> list.</summary>
    public static RoomEnemyPopulationDefinition FallingTile1 { get; } = Find(nameof(FallingTile1));
    /// <summary>The <c>EnemyPopulations_MagnetStairs1</c> list.</summary>
    public static RoomEnemyPopulationDefinition MagnetStairs1 { get; } = Find(nameof(MagnetStairs1));
    /// <summary>The <c>EnemyPopulations_DeadScientist1</c> list.</summary>
    public static RoomEnemyPopulationDefinition DeadScientist1 { get; } = Find(nameof(DeadScientist1));
    /// <summary>The <c>EnemyPopulations_Room58Escape1</c> list.</summary>
    public static RoomEnemyPopulationDefinition Room58Escape1 { get; } = Find(nameof(Room58Escape1));
    /// <summary>The <c>EnemyPopulations_CeresRidley1</c> list.</summary>
    public static RoomEnemyPopulationDefinition CeresRidley1 { get; } = Find(nameof(CeresRidley1));

}
