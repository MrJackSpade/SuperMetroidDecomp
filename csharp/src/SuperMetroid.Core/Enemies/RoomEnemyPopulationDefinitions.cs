namespace SuperMetroid.Core.Enemies;

/// <summary>Application-owned ordered enemy placements selected by all retail room states.</summary>
/// <remarks>
/// The 302 pointers and 1,658 records come from the pinned NTSC J/U v1.0 bank-$A1
/// populations selected by 323 compiled room states. Record order and the byte after
/// each terminator govern linked slot allocation and the room enemy-death quota.
/// This is gameplay data, not an editable visual asset.
/// </remarks>
public static class RoomEnemyPopulationDefinitions
{
    /// <summary>Retail populations in the cartridge's list order.</summary>
    internal static readonly RoomEnemyPopulationDefinition[] Definitions =
    [
        ..BuildSegment0(),
        ..BuildSegment1(),
        ..BuildSegment2(),
        ..BuildSegment3(),
    ];

    /// <summary>Number of distinct bank-$A1 population pointers selected by the compiled retail room states, including empty lists.</summary>
    public const int ListCount = 302;
    /// <summary>Total placement records across the distinct compiled lists, excluding terminators and without counting repeated references from room states.</summary>
    public const int RecordCount = 1658;

    /// <summary>Rejects an incomplete or unordered compiled population table before gameplay can use it.</summary>
    static RoomEnemyPopulationDefinitions()
    {
        if (Definitions.Length != ListCount || Definitions.Sum(list => list.Records.Length) != RecordCount)
            throw new InvalidDataException("Compiled enemy-population counts changed.");
        if (Definitions.Select(list => list.Name).Distinct().Count() != ListCount)
            throw new InvalidDataException("Compiled enemy populations repeat a name.");
    }

    /// <summary>The compiled population named <paramref name="name"/>.</summary>
    private static RoomEnemyPopulationDefinition Find(string name) =>
        Definitions.Single(list => list.Name == name);

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

    /// <summary>Provides the first contiguous segment of compiled bank-$A1 placement lists.</summary>
    private static RoomEnemyPopulationDefinition[] BuildSegment0() =>
    [
        new(nameof(DraygonState1MotherBrainState2),
        [
        ], 0xbf),
        new(nameof(EastOcean),
        [
            new(EnemyDefinitionId.Choot, new(0x0150, 0x054c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0203, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x0190, 0x050c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0203, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x0214, 0x053c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0203, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x05c8, 0x05be), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0203, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x00b0, 0x051c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0203, 0x0010),
            new(EnemyDefinitionId.Skultera, new(0x00fc, 0x0563), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0110, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x01d5, 0x0571), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x0314, 0x0587), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0110, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x03c4, 0x056b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x04d5, 0x0577), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Kamer2, new(0x04c0, 0x04f0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2800),
            new(EnemyDefinitionId.Kamer2, new(0x03c0, 0x04f0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2800),
            new(EnemyDefinitionId.Kamer2, new(0x02c8, 0x04f0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2800),
        ], 0x0a),
        new(nameof(CrateriaPowerBombs),
        [
            new(EnemyDefinitionId.Alcoon, new(0x0088, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x0108, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x0178, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x03),
        new(nameof(Terminator),
        [
            new(EnemyDefinitionId.Waver, new(0x0316, 0x015c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0213, 0x01e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0102, 0x0262), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0378, 0x0130), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0498, 0x00a0), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x02e9, 0x01d6), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0207, 0x024a), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0178, 0x0230), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0298, 0x01a0), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x09),
        new(nameof(FinalMissileBombway1),
        [
            new(EnemyDefinitionId.FaceBlock, new(0x0100, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0080, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0180, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0080, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0100, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0180, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(FinalMissileBombway0),
        [
            new(EnemyDefinitionId.FaceBlock, new(0x0100, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0080, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0180, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0080, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0100, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0180, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(Parlor1),
        [
            new(EnemyDefinitionId.Zoomer, new(0x0498, 0x0040), 0x0002, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x041f, 0x0088), 0x0003, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0448, 0x00b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Skree, new(0x02fe, 0x003b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Skree, new(0x02be, 0x005b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Skree, new(0x027e, 0x003b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x01a0, 0x0178), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x01b8, 0x0228), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0128, 0x0438), 0x0000, EnemyPropertyWord.FromNativeWord(0x2003), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x01d4, 0x0438), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0180, 0x0408), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0168, 0x0328), 0x0000, EnemyPropertyWord.FromNativeWord(0x2002), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x01ea, 0x02ea), 0x0001, EnemyPropertyWord.FromNativeWord(0x2003), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x01c8, 0x0278), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x0181, 0x0115), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x03ad, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
        ], 0x10),
        new(nameof(Flyway01),
        [
            new(EnemyDefinitionId.Mellow, new(0x00ed, 0x0061), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x015e, 0x005c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x011b, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x0132, 0x0049), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x013a, 0x0065), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x00ea, 0x0042), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x01ca, 0x006b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x0100, 0x0076), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x019a, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x0190, 0x007e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x017f, 0x003e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x01c2, 0x0041), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(Pit1),
        [
            new(EnemyDefinitionId.PirateGreyWalking, new(0x0268, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0010),
            new(EnemyDefinitionId.PirateGreyWall, new(0x02cd, 0x003f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x0020),
            new(EnemyDefinitionId.PirateGreyWalking, new(0x01f9, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0010),
            new(EnemyDefinitionId.PirateGreyWalking, new(0x0178, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0010),
            new(EnemyDefinitionId.PirateGreyWalking, new(0x0068, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0020),
        ], 0x05),
        new(nameof(GauntletEntrance),
        [
            new(EnemyDefinitionId.YappingMaw, new(0x0408, 0x0038), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0070, 0x0000),
            new(EnemyDefinitionId.YappingMaw, new(0x02f8, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0070, 0x0001),
            new(EnemyDefinitionId.YappingMaw, new(0x01f8, 0x0038), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0070, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0371, 0x0065), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x023a, 0x007d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0166, 0x0077), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.YappingMaw, new(0x0108, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0070, 0x0001),
        ], 0x03),
        new(nameof(BombTorizo01),
        [
            new(EnemyDefinitionId.BombTorizo, new(0x00db, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(GreenPiratesShaft),
        [
            new(EnemyDefinitionId.PirateGreenWalking, new(0x0080, 0x04d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0018),
            new(EnemyDefinitionId.PirateGreenWalking, new(0x008d, 0x0520), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0018),
            new(EnemyDefinitionId.PirateGreenWalking, new(0x008b, 0x0570), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0018),
            new(EnemyDefinitionId.PirateGreenWalking, new(0x0084, 0x05c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0018),
            new(EnemyDefinitionId.PirateGreenWalking, new(0x0096, 0x0610), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0018),
            new(EnemyDefinitionId.Beetom, new(0x00b8, 0x03d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0048, 0x03d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x07),
        new(nameof(GreenBrinstarElev),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x00a2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0140),
        ], 0x00),
        new(nameof(FinalMissile0),
        [
            new(EnemyDefinitionId.Eye, new(0x0032, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x8000),
            new(EnemyDefinitionId.Eye, new(0x0032, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(CrateriaMap),
        [
        ], 0x00),
        new(nameof(FinalMissile1),
        [
        ], 0x00),
        new(nameof(Climb2),
        [
            new(EnemyDefinitionId.PirateSilverNinja, new(0x0080, 0x0880), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.PirateSilverNinja, new(0x00c0, 0x0880), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
        ], 0x02),
        new(nameof(CrateriaSave),
        [
        ], 0x00),
        new(nameof(ForgottenHighwayElbow),
        [
        ], 0x00),
        new(nameof(Moat),
        [
        ], 0x00),
        new(nameof(Climb0),
        [
            new(EnemyDefinitionId.Sbug, new(0x0114, 0x004c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5003, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0110, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x010d, 0x0072), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xac03, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x01eb, 0x0096), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc804, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x01f3, 0x009a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc303, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0115, 0x0126), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9203, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0114, 0x0123), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x6003, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0111, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9c02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x01ee, 0x0217), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xf004, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0116, 0x06b9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xbc02, 0x0050),
        ], 0x00),
        new(nameof(WestOcean),
        [
            new(EnemyDefinitionId.Skultera, new(0x04a4, 0x0570), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x03cb, 0x057a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0110, 0x0210),
            new(EnemyDefinitionId.Zeb, new(0x0278, 0x02c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.GRipper, new(0x0590, 0x0430), 0x0010, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0560, 0x05c0),
            new(EnemyDefinitionId.Tripper, new(0x0150, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0010),
            new(EnemyDefinitionId.Tripper, new(0x0388, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0010),
            new(EnemyDefinitionId.Tripper, new(0x0370, 0x0160), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
        ], 0x07),
        new(nameof(Parlor0),
        [
            new(EnemyDefinitionId.Sbug, new(0x04be, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xaa02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x04a4, 0x00c9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb502, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x04bb, 0x00cf), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd003, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x04e4, 0x0041), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x4804, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0455, 0x002a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x6003, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x043f, 0x002e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x6c03, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x046f, 0x0027), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7802, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x03d8, 0x00c3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xca03, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03d2, 0x00c9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc803, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03db, 0x00ce), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe202, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x011b, 0x00bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xa002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0112, 0x00c4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xab02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x012c, 0x0229), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7c03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0124, 0x022d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9403, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0143, 0x023f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8402, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x013f, 0x02bf), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xa102, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x0145, 0x02ba), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9103, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x013d, 0x02c3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8c02, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x010d, 0x02db), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xac03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01a5, 0x04e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe804, 0x0020),
        ], 0x00),
        new(nameof(LandingSite012),
        [
            new(EnemyDefinitionId.ShipTop, new(0x0480, 0x0478), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.ShipBottomEntrance, new(0x0480, 0x0478), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.ShipBottomEntrance, new(0x0480, 0x0478), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
        ], 0x00),
        new(nameof(CrateriaSuper),
        [
            new(EnemyDefinitionId.Boyon, new(0x0258, 0x07a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0103, 0x0020),
            new(EnemyDefinitionId.Boyon, new(0x0278, 0x07a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0103, 0x0020),
            new(EnemyDefinitionId.Boyon, new(0x0298, 0x07a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0103, 0x0020),
            new(EnemyDefinitionId.Boyon, new(0x02b8, 0x07a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0103, 0x0020),
        ], 0x00),
        new(nameof(PreBowling),
        [
            new(EnemyDefinitionId.HZoomer, new(0x0080, 0x0048), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x01),
        new(nameof(Climb1),
        [
            new(EnemyDefinitionId.PirateGreyWall, new(0x0130, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x01d0, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x0130, 0x01d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x0130, 0x0338), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x01d0, 0x03d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x0130, 0x04b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x01d0, 0x05a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x012d, 0x0698), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x01d0, 0x0278), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x01d0, 0x0708), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x00a0),
            new(EnemyDefinitionId.PirateGreyWall, new(0x0130, 0x07c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x00a0),
        ], 0x0b),
        new(nameof(BowlingAlleyPath),
        [
            new(EnemyDefinitionId.Choot, new(0x0070, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0204, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x00e0, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0204, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x0150, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0204, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x00aa, 0x005d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x00e9, 0x0075), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0136, 0x0063), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x06),
        new(nameof(RedBinstarElev),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x00c2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0140),
        ], 0x00),
        new(nameof(GauntletETank),
        [
            new(EnemyDefinitionId.YappingMaw, new(0x0088, 0x0038), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0070, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0088, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x01),
        new(nameof(Pit0),
        [
            new(EnemyDefinitionId.Sbug, new(0x006b, 0x0038), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x2802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0070, 0x0034), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7803, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x005a, 0x002d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7c02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0073, 0x00ad), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9402, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x007a, 0x00b7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc403, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x006c, 0x00c3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7c03, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x007f, 0x00b2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc203, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00bd, 0x00ca), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc103, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x02c9, 0x00a6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xf002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x02cc, 0x00ab), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xec04, 0x0050),
        ], 0x00),
        new(nameof(CrabMaze),
        [
            new(EnemyDefinitionId.Sciser, new(0x0278, 0x00ac), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0015, 0x0098), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x00d1, 0x00c8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0202, 0x0068), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x011e, 0x01d8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0118, 0x0158), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x00b2, 0x01b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x02a8, 0x01c4), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
        ], 0x08),
        new(nameof(CrateriaTube),
        [
        ], 0x00),
        new(nameof(ForgottenHighwayKagos),
        [
            new(EnemyDefinitionId.Kago, new(0x0050, 0x0340), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0000),
            new(EnemyDefinitionId.Kago, new(0x00a8, 0x0200), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0000),
        ], 0x02),
        new(nameof(ElevToBlueBrinstar),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x00a2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0140),
        ], 0x00),
        new(nameof(ForgottenHighwayElev),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0140),
        ], 0x00),
        new(nameof(PreMapFlyway),
        [
            new(EnemyDefinitionId.Mellow, new(0x014c, 0x006e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x00fb, 0x0059), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mellow, new(0x01a7, 0x005a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rio, new(0x014b, 0x0045), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x01),
        new(nameof(LowerMushrooms),
        [
            new(EnemyDefinitionId.Kago, new(0x0100, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0000),
            new(EnemyDefinitionId.Kago, new(0x0210, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0000),
            new(EnemyDefinitionId.MZoomer, new(0x02f8, 0x0070), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.MZoomer, new(0x01a8, 0x0070), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x04),
        new(nameof(LandingSite3),
        [
            new(EnemyDefinitionId.ShipTop, new(0x0480, 0x0478), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.ShipBottomEntrance, new(0x0480, 0x0478), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.ShipBottomEntrance, new(0x0480, 0x0478), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.Steam, new(0x0051, 0x04aa), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0080, 0x04bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00a1, 0x04d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00e5, 0x04d7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x015b, 0x04d7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x019d, 0x04d5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x01c0, 0x04c9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0222, 0x04c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0243, 0x04c4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x027c, 0x04ce), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0317, 0x04d7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x033f, 0x04c9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x036c, 0x04b6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0390, 0x04a9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x03d9, 0x04b7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x041a, 0x04d9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x049c, 0x04d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x04f9, 0x04ca), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0525, 0x04bb), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0557, 0x04c9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x05c6, 0x04d4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0624, 0x04d4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(Parlor2),
        [
            new(EnemyDefinitionId.Steam, new(0x0145, 0x00b5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x01b9, 0x00ba), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x01de, 0x00a9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0227, 0x00b1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x023d, 0x00b9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x026f, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x02b1, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x02d1, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x030b, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x032f, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x035e, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0384, 0x0077), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x03dc, 0x0079), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x03fe, 0x008e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x041f, 0x00a1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x045c, 0x00c4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x047f, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x04aa, 0x00ae), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x04cc, 0x00a6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(BombTorizo2),
        [
            new(EnemyDefinitionId.DachoraEscape, new(0x00e0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.EtecoonEscape, new(0x00e0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.EtecoonEscape, new(0x00e0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.EtecoonEscape, new(0x00e0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
        ], 0x04),
        new(nameof(Flyway2),
        [
        ], 0x00),
        new(nameof(CrateriaKihunter),
        [
            new(EnemyDefinitionId.KihunterGreen, new(0x0264, 0x0073), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x0264, 0x0073), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterGreen, new(0x00a8, 0x0087), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x00a8, 0x0087), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0198, 0x01d8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0165, 0x021a), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x04),
        new(nameof(BrinstarPreMap),
        [
            new(EnemyDefinitionId.Sidehopper, new(0x0244, 0x003b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zeela, new(0x0178, 0x0058), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x01a8, 0x0058), 0x0003, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x0049, 0x0040), 0x0001, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
        ], 0x04),
        new(nameof(GreenBrinstarMap),
        [
        ], 0x00),
        new(nameof(GreenBrinstarMissileRefill),
        [
        ], 0x00),
        new(nameof(SporeSpawnKihunters),
        [
            new(EnemyDefinitionId.KihunterGreen, new(0x00d9, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x003c, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x00d9, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterGreen, new(0x02d5, 0x0062), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x003c, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x02d5, 0x0062), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterGreen, new(0x03d7, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x003c, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x03d7, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
        ], 0x03),
        new(nameof(EtecoonSuper),
        [
        ], 0x00),
        new(nameof(DachoraEnergyRefill),
        [
        ], 0x00),
        new(nameof(SporeSpawnFarming),
        [
            new(EnemyDefinitionId.Zeb, new(0x0180, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zeb, new(0x0110, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zeb, new(0x00e0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zeela, new(0x0080, 0x0042), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00ff, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x00b0, 0x0042), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00ff, 0x0002),
        ], 0x05),
        new(nameof(Statues),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x01b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0240),
            new(EnemyDefinitionId.TourianStatue, new(0x0078, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.TourianStatue, new(0x008e, 0x0055), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.TourianStatue, new(0x0084, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
        ], 0x00),
        new(nameof(HopperETank),
        [
        ], 0x00),
        new(nameof(BetaPowerBomb),
        [
            new(EnemyDefinitionId.YappingMaw, new(0x0170, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0036, 0x0001),
            new(EnemyDefinitionId.YappingMaw, new(0x00f0, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0036, 0x0001),
            new(EnemyDefinitionId.SidehopperLarge, new(0x00f0, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.SidehopperLarge, new(0x0170, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.SidehopperLarge, new(0x0078, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
        ], 0x03),
        new(nameof(WaterwayETank),
        [
            new(EnemyDefinitionId.Skultera, new(0x0140, 0x00ae), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x1001),
            new(EnemyDefinitionId.Puyo, new(0x0169, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0001),
            new(EnemyDefinitionId.Puyo, new(0x01f7, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0001),
            new(EnemyDefinitionId.Puyo, new(0x0224, 0x00b5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0001),
            new(EnemyDefinitionId.Skultera, new(0x024b, 0x00ae), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x1001),
            new(EnemyDefinitionId.Zero, new(0x0430, 0x00b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zero, new(0x05f0, 0x0058), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x07),
        new(nameof(ConstructionZone1),
        [
            new(EnemyDefinitionId.Zoomer, new(0x0058, 0x0078), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x00a8, 0x0094), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0048, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x00b8, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x00e8, 0x0116), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x00e8, 0x0166), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0018, 0x0116), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x02),
        new(nameof(BlueBrinstarETank1),
        [
            new(EnemyDefinitionId.Zoomer, new(0x00a8, 0x02c8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zoomer, new(0x00d8, 0x02c8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Skree, new(0x00de, 0x024b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Skree, new(0x00fe, 0x023b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rio, new(0x01f0, 0x0240), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0038, 0x0228), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0038, 0x02a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x02e8, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0268, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0288, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x05),
        new(nameof(NoobBridge),
        [
            new(EnemyDefinitionId.Cacatac, new(0x00c0, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0301),
            new(EnemyDefinitionId.Cacatac, new(0x01b0, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0301),
            new(EnemyDefinitionId.Cacatac, new(0x0570, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0301),
            new(EnemyDefinitionId.Cacatac, new(0x03d0, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0301),
            new(EnemyDefinitionId.Zeela, new(0x0248, 0x0076), 0x0002, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x02a8, 0x0076), 0x0002, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x0300, 0x0076), 0x0002, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x0368, 0x0076), 0x0002, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
        ], 0x08),
        new(nameof(MorphBall1),
        [
            new(EnemyDefinitionId.Elevator, new(0x0580, 0x02c2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.SidehopperLarge, new(0x0098, 0x02a6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.SidehopperLarge, new(0x017c, 0x02a6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.SidehopperLarge, new(0x0107, 0x026c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0548, 0x0240), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x05b8, 0x0240), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0488, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0428, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x03),
        new(nameof(WarehouseSave),
        [
        ], 0x00),
        new(nameof(MorphBall0),
        [
            new(EnemyDefinitionId.Eye, new(0x0408, 0x0248), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x8000),
            new(EnemyDefinitionId.Eye, new(0x0408, 0x0248), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Elevator, new(0x0580, 0x02c2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.FaceBlock, new(0x0548, 0x0240), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0001),
            new(EnemyDefinitionId.FaceBlock, new(0x05b8, 0x0240), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0488, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0428, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0001),
        ], 0x00),
        new(nameof(WarehouseZeela),
        [
            new(EnemyDefinitionId.Zeela, new(0x006a, 0x0040), 0x0001, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x006a, 0x0068), 0x0001, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x002b, 0x00e8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
        ], 0x03),
        new(nameof(RedTower),
        [
            new(EnemyDefinitionId.Ripper, new(0x0098, 0x0208), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x0060, 0x00e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0001),
            new(EnemyDefinitionId.Ripper, new(0x0098, 0x0140), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x0068, 0x01a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0001),
            new(EnemyDefinitionId.Beetom, new(0x00cf, 0x0658), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x0098, 0x0948), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x005f, 0x08e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0001),
            new(EnemyDefinitionId.Ripper, new(0x009d, 0x0888), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x0068, 0x0800), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0001),
            new(EnemyDefinitionId.Geega, new(0x0018, 0x0530), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0020),
            new(EnemyDefinitionId.Geega, new(0x00c8, 0x0550), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0020),
        ], 0x0b),
        new(nameof(BlueBrinstarBoulders),
        [
            new(EnemyDefinitionId.Boulder, new(0x0158, 0x00c0), 0x0080, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0xa050),
            new(EnemyDefinitionId.Boulder, new(0x00f0, 0x00c0), 0x0080, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0xa050),
            new(EnemyDefinitionId.Boulder, new(0x0090, 0x00c0), 0x0080, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0xa050),
        ], 0x00),
        new(nameof(BillyMays),
        [
        ], 0x00),
        new(nameof(BigPinkSaveRoom),
        [
        ], 0x00),
        new(nameof(BigPink),
        [
            new(EnemyDefinitionId.Rio, new(0x02e8, 0x0478), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rio, new(0x0328, 0x0478), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rio, new(0x0335, 0x05a9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x0239, 0x00fc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x02f8, 0x00ed), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x0250, 0x0418), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x02b0, 0x0418), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zeb, new(0x02c0, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zeb, new(0x02b0, 0x0228), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zeb, new(0x0320, 0x0549), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x0a),
        new(nameof(EtecoonSave),
        [
        ], 0x00),
        new(nameof(BelowSpazer),
        [
            new(EnemyDefinitionId.YappingMaw, new(0x0190, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0001),
            new(EnemyDefinitionId.YappingMaw, new(0x0080, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0001),
            new(EnemyDefinitionId.Cacatac, new(0x0108, 0x0193), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0200),
        ], 0x01),
        new(nameof(GreenBrinstarSave),
        [
        ], 0x00),
        new(nameof(Spazer),
        [
        ], 0x00),
        new(nameof(PinkBrinstarPowerBombs),
        [
            new(EnemyDefinitionId.ShutterShootable, new(0x0038, 0x0190), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xffff), 0x6004, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0198, 0x01f0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xffff), 0x4004, 0x0000),
            new(EnemyDefinitionId.SidehopperLarge, new(0x00f1, 0x00a6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.SidehopperLarge, new(0x0068, 0x0064), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
        ], 0x02),
        new(nameof(RedTowerRefill),
        [
        ], 0x00),
        new(nameof(KraidRefill),
        [
        ], 0x00),
        new(nameof(VariaSuit),
        [
        ], 0x00),
        new(nameof(RedBrinstarSave),
        [
        ], 0x00),
        new(nameof(BlueBrinstarETank0),
        [
            new(EnemyDefinitionId.Eye, new(0x0228, 0x0268), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x8001),
            new(EnemyDefinitionId.Eye, new(0x0228, 0x0268), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0038, 0x0228), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0038, 0x02a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x02e8, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0268, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0288, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
    ];

    /// <summary>Provides the bank-$A1 placement lists starting at $96E2.</summary>
    private static RoomEnemyPopulationDefinition[] BuildSegment1() =>
    [
        new(nameof(GreenBrinstarFirefleas),
        [
            new(EnemyDefinitionId.Fireflea, new(0x0180, 0x005f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0210),
            new(EnemyDefinitionId.Fireflea, new(0x00e1, 0x006a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0210),
            new(EnemyDefinitionId.Fireflea, new(0x0045, 0x005b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0210),
            new(EnemyDefinitionId.Fireflea, new(0x0070, 0x0118), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0308),
            new(EnemyDefinitionId.Fireflea, new(0x00a0, 0x0138), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0408),
        ], 0x05),
        new(nameof(GreenBrinstarBeetoms),
        [
            new(EnemyDefinitionId.Beetom, new(0x0050, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0070, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0090, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x00b0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x04),
        new(nameof(EtecoonETank),
        [
            new(EnemyDefinitionId.Zebbo, new(0x0250, 0x01d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x02f0, 0x01d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0430, 0x01d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x01d8, 0x0198), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0278, 0x0198), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0360, 0x01a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x03d8, 0x01a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0458, 0x01a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x08),
        new(nameof(PinkBrinstarHoppers),
        [
            new(EnemyDefinitionId.SidehopperLarge, new(0x00b5, 0x01b5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x00f8, 0x01a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x006a, 0x01c1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0158, 0x01e0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0x4003, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0158, 0x01e0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0x8003, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0158, 0x01e0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0xc003, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0158, 0x01e0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0xf003, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0148, 0x01e0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0168, 0x01e0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x03),
        new(nameof(WarehouseETank),
        [
            new(EnemyDefinitionId.Beetom, new(0x002d, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0047, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x008b, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0060, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x04),
        new(nameof(WarehouseEntrance),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0140),
        ], 0x00),
        new(nameof(WarehouseKihunter),
        [
            new(EnemyDefinitionId.KihunterGreen, new(0x0169, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x0169, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterGreen, new(0x0289, 0x0059), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x0289, 0x0059), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterGreen, new(0x01fe, 0x0063), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x01fe, 0x0063), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterGreen, new(0x0242, 0x007a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KihunterGreenWings, new(0x0242, 0x007a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
        ], 0x04),
        new(nameof(GreenBrinstarMainShaft),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x02c2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.Zeela, new(0x00da, 0x02cb), 0x0001, EnemyPropertyWord.FromNativeWord(0x2803), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x0070, 0x0398), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0002),
            new(EnemyDefinitionId.Ripper2, new(0x006c, 0x03f6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0018, 0x0001),
            new(EnemyDefinitionId.Zeela, new(0x0079, 0x06e8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0002),
            new(EnemyDefinitionId.Ripper2, new(0x0043, 0x0548), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0018, 0x0001),
            new(EnemyDefinitionId.Zeela, new(0x007b, 0x05f8), 0x0001, EnemyPropertyWord.FromNativeWord(0x2002), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x0027, 0x03cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2003), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0002),
            new(EnemyDefinitionId.Etecoon, new(0x025f, 0x0b98), 0x0000, EnemyPropertyWord.FromNativeWord(0x0c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Etecoon, new(0x026f, 0x0b98), 0x0000, EnemyPropertyWord.FromNativeWord(0x0c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.Etecoon, new(0x027f, 0x0b98), 0x0000, EnemyPropertyWord.FromNativeWord(0x0c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
        ], 0x07),
        new(nameof(SporeSpawnSuper),
        [
            new(EnemyDefinitionId.Zeb, new(0x00c0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x01),
        new(nameof(EarlySupers),
        [
            new(EnemyDefinitionId.Sidehopper, new(0x01ba, 0x01a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x0200, 0x01a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.ShutterGrowing, new(0x0198, 0x0128), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0090, 0x0010),
            new(EnemyDefinitionId.ShutterGrowing, new(0x0228, 0x0128), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0090, 0x0010),
            new(EnemyDefinitionId.ShutterGrowing, new(0x0288, 0x0128), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0090, 0x0010),
            new(EnemyDefinitionId.Waver, new(0x00ab, 0x004b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0108, 0x006b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0211, 0x0065), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0141, 0x0056), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Zeb, new(0x0090, 0x01b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x0142, 0x0085), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x01c3, 0x0084), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x0183, 0x003d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x0a),
        new(nameof(RedBrinstarFirefleas),
        [
            new(EnemyDefinitionId.Fireflea, new(0x06d0, 0x0090), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0306),
            new(EnemyDefinitionId.Fireflea, new(0x0710, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0518),
            new(EnemyDefinitionId.Fireflea, new(0x0688, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0418),
            new(EnemyDefinitionId.Fireflea, new(0x04e0, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0210),
            new(EnemyDefinitionId.Fireflea, new(0x0630, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0220),
            new(EnemyDefinitionId.Waver, new(0x0745, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0690, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0600, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.YappingMaw, new(0x0198, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0001),
            new(EnemyDefinitionId.YappingMaw, new(0x0258, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0001),
            new(EnemyDefinitionId.YappingMaw, new(0x03a8, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0001),
        ], 0x08),
        new(nameof(ConstructionZone0),
        [
            new(EnemyDefinitionId.Sbug, new(0x002e, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8a03, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00de, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc803, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00db, 0x00b7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xf802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00d9, 0x00b1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xec02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00d7, 0x00c1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xf403, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00b3, 0x01d3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc402, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00b9, 0x01db), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd003, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00bb, 0x01ea), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xdc03, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00ae, 0x01dc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xbb02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00d5, 0x01f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0303, 0x0080),
            new(EnemyDefinitionId.FaceBlock, new(0x0048, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x00b8, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.FaceBlock, new(0x00e8, 0x0116), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x00e8, 0x0166), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.FaceBlock, new(0x0018, 0x0116), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(GreenHillZone),
        [
            new(EnemyDefinitionId.Sidehopper, new(0x0400, 0x02a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x0500, 0x0300), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x0687, 0x03b6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geega, new(0x0170, 0x0120), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0024),
            new(EnemyDefinitionId.Geega, new(0x0230, 0x01e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0024),
            new(EnemyDefinitionId.Geega, new(0x0380, 0x0220), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0024),
            new(EnemyDefinitionId.Geega, new(0x0560, 0x02e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0024),
            new(EnemyDefinitionId.Geega, new(0x0500, 0x0340), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0024),
            new(EnemyDefinitionId.Sidehopper, new(0x06d0, 0x0350), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sidehopper, new(0x0718, 0x0350), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x0a),
        new(nameof(Dachora),
        [
            new(EnemyDefinitionId.Zeela, new(0x00e0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2803), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x0108, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2003), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Metaree, new(0x062d, 0x006c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Metaree, new(0x0658, 0x004c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Metaree, new(0x0673, 0x004c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rio, new(0x0130, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rio, new(0x01b0, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rio, new(0x02e8, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dachora, new(0x0060, 0x06a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x0c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dachora, new(0x0060, 0x06a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x0d00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8001, 0x0000),
            new(EnemyDefinitionId.Dachora, new(0x0060, 0x06a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x0d00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8001, 0x0000),
            new(EnemyDefinitionId.Dachora, new(0x0060, 0x06a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x0d00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8001, 0x0000),
            new(EnemyDefinitionId.Dachora, new(0x0060, 0x06a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x0d00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8001, 0x0000),
        ], 0x08),
        new(nameof(Hellway),
        [
            new(EnemyDefinitionId.Zeela, new(0x00a0, 0x0098), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x0127, 0x0088), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x0170, 0x0098), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Zeela, new(0x01bf, 0x0098), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Zebbo, new(0x01a0, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0100, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x01e0, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0080, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x08),
        new(nameof(FirstMissile),
        [
        ], 0x00),
        new(nameof(Kraid),
        [
            new(EnemyDefinitionId.Kraid, new(0x0100, 0x0218), 0x0000, EnemyPropertyWord.FromNativeWord(0x0d00), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.KraidArm, new(0x00e8, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0001),
            new(EnemyDefinitionId.KraidLintTop, new(0x00c8, 0x0210), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KraidLintMiddle, new(0x00b0, 0x0250), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.KraidLintBottom, new(0x00b2, 0x0288), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
            new(EnemyDefinitionId.KraidFoot, new(0x0100, 0x0278), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0003),
            new(EnemyDefinitionId.KraidNail, new(0x00e8, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KraidNailBad, new(0x00e8, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(BrinstarReserveTank),
        [
        ], 0x00),
        new(nameof(XrayScope),
        [
            new(EnemyDefinitionId.ShutterShootable, new(0x0168, 0x00a0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xffff), 0x4004, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x00b8, 0x00e0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xffff), 0x4004, 0x0000),
        ], 0x00),
        new(nameof(StatuesHallway),
        [
        ], 0x00),
        new(nameof(AlphaPowerBomb),
        [
            new(EnemyDefinitionId.Boyon, new(0x0258, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0020),
            new(EnemyDefinitionId.Boyon, new(0x0268, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0020),
            new(EnemyDefinitionId.Boyon, new(0x01f8, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0020),
            new(EnemyDefinitionId.Boyon, new(0x0208, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0020),
        ], 0x00),
        new(nameof(KraidEyeDoor),
        [
            new(EnemyDefinitionId.Zeb, new(0x0090, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sbug, new(0x0136, 0x019d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc803, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x0137, 0x01a4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xba03, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x013d, 0x01ab), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xa802, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x0133, 0x01ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb403, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x013b, 0x019e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xca02, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x0142, 0x01a4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc903, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x0145, 0x01a7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xbe02, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x0143, 0x01af), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc203, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x0137, 0x01b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9003, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x014e, 0x01ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc802, 0x0020),
        ], 0x01),
        new(nameof(Caterpillar),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x02c2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.Zero, new(0x005f, 0x0788), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zero, new(0x0017, 0x06de), 0x0000, EnemyPropertyWord.FromNativeWord(0x2002), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zero, new(0x0078, 0x068f), 0x0001, EnemyPropertyWord.FromNativeWord(0x2002), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x0071, 0x0473), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0300),
            new(EnemyDefinitionId.Cacatac, new(0x00c0, 0x0503), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0300),
        ], 0x05),
        new(nameof(MiniKraid),
        [
            new(EnemyDefinitionId.PirateGreenWalking, new(0x00d9, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0050),
            new(EnemyDefinitionId.PirateGreenWalking, new(0x0120, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0050),
            new(EnemyDefinitionId.PirateGreenWalking, new(0x01f4, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0050),
            new(EnemyDefinitionId.MiniKraid, new(0x0530, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x04),
        new(nameof(SporeSpawn),
        [
            new(EnemyDefinitionId.SporeSpawn, new(0x0080, 0x0270), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(SkreeBoost),
        [
            new(EnemyDefinitionId.Skree, new(0x0148, 0x003c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Skree, new(0x0170, 0x003c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x02),
        new(nameof(GrappleTutorial3),
        [
            new(EnemyDefinitionId.Gamet, new(0x00d0, 0x0168), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3030),
            new(EnemyDefinitionId.Gamet, new(0x00d0, 0x0168), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x00d0, 0x0168), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x00d0, 0x0168), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x00d0, 0x0168), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Puyo, new(0x01cd, 0x014c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x01ba, 0x0114), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0208, 0x01a2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x023c, 0x01a7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0270, 0x01a1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x06),
        new(nameof(PostCrocShaft),
        [
            new(EnemyDefinitionId.Viola, new(0x0077, 0x0348), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x0060, 0x03a8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x0093, 0x03f8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x007d, 0x0478), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
        ], 0x04),
        new(nameof(MetalPirates),
        [
            new(EnemyDefinitionId.PirateGoldNinja, new(0x00e8, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x00c0),
            new(EnemyDefinitionId.PirateGoldNinja, new(0x0218, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x00c0),
        ], 0x02),
        new(nameof(AcidStatue),
        [
            new(EnemyDefinitionId.Chozo, new(0x002c, 0x009a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
            new(EnemyDefinitionId.Magdollite, new(0x01d8, 0x02d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x20f0),
            new(EnemyDefinitionId.Magdollite, new(0x01d8, 0x02d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Magdollite, new(0x01d8, 0x02d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0200, 0x0268), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0200, 0x0268), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x01b0, 0x0250), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x01b0, 0x0250), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0254, 0x024d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0254, 0x024d), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
        ], 0x04),
        new(nameof(PurpleFarming),
        [
            new(EnemyDefinitionId.Gamet, new(0x0080, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3030),
            new(EnemyDefinitionId.Gamet, new(0x0080, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0080, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0080, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0080, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
        ], 0x01),
        new(nameof(IceBeamGate),
        [
            new(EnemyDefinitionId.ShutterGrowing, new(0x0408, 0x0258), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00f0, 0x000d),
            new(EnemyDefinitionId.ShutterGrowing, new(0x0608, 0x0258), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0068, 0x0010),
            new(EnemyDefinitionId.Dessgeega, new(0x0104, 0x03af), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dessgeega, new(0x0195, 0x035f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dessgeega, new(0x0222, 0x03b2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x0139, 0x0374), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x0196, 0x03a7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x01fc, 0x0371), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x0254, 0x0379), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x0281, 0x039e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x00b0, 0x0362), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sova, new(0x05c8, 0x0238), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0004),
        ], 0x04),
        new(nameof(SpikyAcidSnakesTunnel),
        [
            new(EnemyDefinitionId.YappingMaw, new(0x0188, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0080, 0x0001),
            new(EnemyDefinitionId.YappingMaw, new(0x0219, 0x00d7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0080, 0x0001),
            new(EnemyDefinitionId.YappingMaw, new(0x02f8, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0080, 0x0001),
        ], 0x00),
        new(nameof(RedKihunterShaft),
        [
            new(EnemyDefinitionId.KihunterRed, new(0x00b9, 0x01d6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0000),
            new(EnemyDefinitionId.KihunterRedWings, new(0x00b9, 0x01d6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterRed, new(0x004d, 0x023a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0000),
            new(EnemyDefinitionId.KihunterRedWings, new(0x004d, 0x023a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterRed, new(0x0087, 0x02f2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0000),
            new(EnemyDefinitionId.KihunterRedWings, new(0x0087, 0x02f2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
        ], 0x03),
        new(nameof(FastRipper),
        [
            new(EnemyDefinitionId.Ripper2, new(0x01d8, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0000),
            new(EnemyDefinitionId.Ripper2, new(0x01d8, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0000),
            new(EnemyDefinitionId.Ripper2, new(0x0268, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0000),
            new(EnemyDefinitionId.Ripper2, new(0x0268, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0000),
            new(EnemyDefinitionId.Ripper2, new(0x0268, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0000),
            new(EnemyDefinitionId.Ripper2, new(0x0158, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0000),
        ], 0x06),
        new(nameof(WaveBeam),
        [
        ], 0x00),
        new(nameof(FastPillarsSetup),
        [
            new(EnemyDefinitionId.PirateGoldWall, new(0x00d1, 0x0138), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x00a0),
            new(EnemyDefinitionId.PirateGoldWall, new(0x002d, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x00a0),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x00b0, 0x0180), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x00a0),
            new(EnemyDefinitionId.Viola, new(0x0078, 0x0237), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x0088, 0x0237), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0006),
        ], 0x05),
        new(nameof(LowerNorfairElev),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0140),
        ], 0x00),
        new(nameof(LNSave),
        [
        ], 0x00),
        new(nameof(UpperNorfairRefill),
        [
        ], 0x00),
        new(nameof(NorfairMap),
        [
        ], 0x00),
        new(nameof(MickeyMouse),
        [
            new(EnemyDefinitionId.DessgeegaLarge, new(0x03c0, 0x038b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.DessgeegaLarge, new(0x0357, 0x038a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.DessgeegaLarge, new(0x01b1, 0x038b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.DessgeegaLarge, new(0x0105, 0x0380), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.DessgeegaLarge, new(0x026a, 0x0381), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Multiviola, new(0x0095, 0x0377), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000e, 0x0006),
            new(EnemyDefinitionId.Multiviola, new(0x0161, 0x0390), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00a8, 0x0006),
            new(EnemyDefinitionId.Multiviola, new(0x0396, 0x0316), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0007),
            new(EnemyDefinitionId.Multiviola, new(0x0349, 0x02fb), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0007),
            new(EnemyDefinitionId.Multiviola, new(0x0373, 0x02a9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00c8, 0x0006),
            new(EnemyDefinitionId.Multiviola, new(0x0358, 0x0238), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00f0, 0x0006),
            new(EnemyDefinitionId.Multiviola, new(0x03cb, 0x0214), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0048, 0x0005),
        ], 0x05),
        new(nameof(GTEnergyRefill),
        [
        ], 0x00),
        new(nameof(Ridley),
        [
            new(EnemyDefinitionId.Ridley, new(0x0030, 0xfff0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(PostCrocPowerBombs),
        [
        ], 0x00),
        new(nameof(IceBeam),
        [
        ], 0x00),
        new(nameof(HiJumpBoots),
        [
        ], 0x00),
        new(nameof(PostCrocSave),
        [
        ], 0x00),
        new(nameof(FrogSpeedway),
        [
            new(EnemyDefinitionId.Beetom, new(0x02b8, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x02e8, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0310, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0390, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0400, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Beetom, new(0x0460, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x06),
        new(nameof(RisingTide),
        [
            new(EnemyDefinitionId.Squeept, new(0x00e8, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Squeept, new(0x00e8, 0x00f0), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Sova, new(0x01e8, 0x0098), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x03e8, 0x00a8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0178, 0x00a0), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x02c8, 0x00ab), 0x0001, EnemyPropertyWord.FromNativeWord(0x2002), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0288, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2003), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Dragon, new(0x0238, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0238, 0x00f0), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0338, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0338, 0x00f0), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0468, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0468, 0x00f0), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x03b8, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x03b8, 0x00f0), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Squeept, new(0x0168, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Squeept, new(0x0168, 0x00f0), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
        ], 0x0b),
        new(nameof(PostCrocFarming),
        [
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3030),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.GRipper, new(0x0100, 0x0050), 0x0010, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00e8, 0x0130),
            new(EnemyDefinitionId.Kamer2, new(0x0158, 0x0148), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2800),
        ], 0x02),
        new(nameof(WorstRoomInTheGame),
        [
            new(EnemyDefinitionId.Namihe, new(0x00da, 0x0577), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1001, 0x4007),
            new(EnemyDefinitionId.PirateGoldWall, new(0x00d0, 0x04f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x00a0),
            new(EnemyDefinitionId.PirateGoldWall, new(0x0030, 0x0490), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x00a0),
            new(EnemyDefinitionId.PirateGoldWall, new(0x00d0, 0x0440), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x00a0),
            new(EnemyDefinitionId.Namihe, new(0x0027, 0x0320), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1011, 0x4007),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x0030, 0x02b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0001),
            new(EnemyDefinitionId.Namihe, new(0x00da, 0x0388), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1001, 0x4007),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x00d0, 0x02e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.Namihe, new(0x00da, 0x0240), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1001, 0x4007),
            new(EnemyDefinitionId.Namihe, new(0x0027, 0x01e0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1011, 0x4007),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x0080, 0x0170), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
        ], 0x0b),
        new(nameof(DoubleChamber),
        [
            new(EnemyDefinitionId.Fune, new(0x0030, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5010, 0xf007),
            new(EnemyDefinitionId.Fune, new(0x01c0, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5000, 0xf007),
            new(EnemyDefinitionId.Kamer, new(0x0088, 0x00a8), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0404), 0x4001, 0x0020),
            new(EnemyDefinitionId.Kamer, new(0x00d0, 0x00e8), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0808), 0x3001, 0x0030),
            new(EnemyDefinitionId.Kamer, new(0x0110, 0x00a8), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0202), 0x6001, 0x0030),
            new(EnemyDefinitionId.Kamer, new(0x0178, 0x00a8), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0303), 0x5001, 0x0030),
            new(EnemyDefinitionId.Kago, new(0x02f0, 0x01d0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0000),
            new(EnemyDefinitionId.GRipper, new(0x02e8, 0x00f0), 0x0010, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0298, 0x0338),
        ], 0x04),
        new(nameof(GrappleBeam),
        [
        ], 0x00),
        new(nameof(RedPirateShaft),
        [
            new(EnemyDefinitionId.PirateRedWalking, new(0x0050, 0x0200), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0008),
            new(EnemyDefinitionId.PirateRedWalking, new(0x00b0, 0x0180), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0008),
            new(EnemyDefinitionId.PirateRedWalking, new(0x0050, 0x0100), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0008),
            new(EnemyDefinitionId.MZoomer, new(0x0080, 0x0268), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.MZoomer, new(0x0080, 0x01d8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.MZoomer, new(0x0080, 0x0158), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.MZoomer, new(0x0080, 0x00d8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x07),
        new(nameof(CrocomireSpeedway),
        [
            new(EnemyDefinitionId.PirateRedWalking, new(0x08b8, 0x0270), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0000),
            new(EnemyDefinitionId.Multiviola, new(0x0413, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00f0, 0x0003),
            new(EnemyDefinitionId.PirateRedWalking, new(0x02f4, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0030),
            new(EnemyDefinitionId.Multiviola, new(0x048d, 0x0116), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0003),
            new(EnemyDefinitionId.PirateRedWalking, new(0x039a, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0020),
            new(EnemyDefinitionId.Multiviola, new(0x050c, 0x0158), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0068, 0x0003),
            new(EnemyDefinitionId.Multiviola, new(0x05af, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00d0, 0x0003),
            new(EnemyDefinitionId.PirateRedWalking, new(0x09fb, 0x0270), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0000),
            new(EnemyDefinitionId.PirateRedWalking, new(0x06e7, 0x0290), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0010),
            new(EnemyDefinitionId.Cacatac, new(0x0c58, 0x01e3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x0ca8, 0x01e3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0000),
        ], 0x07),
        new(nameof(Plowerhouse),
        [
            new(EnemyDefinitionId.Holtz, new(0x0258, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0258, 0x0058), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0288, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0288, 0x0058), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0118, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0118, 0x0058), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x01c8, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x01c8, 0x0058), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0068, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0068, 0x0058), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0098, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Holtz, new(0x0098, 0x0058), 0x0006, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x00c0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0180, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0240, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x09),
        new(nameof(LNFireflea),
        [
            new(EnemyDefinitionId.Fune, new(0x01f0, 0x02d0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0f07),
            new(EnemyDefinitionId.Fune, new(0x0110, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0110, 0x0f07),
            new(EnemyDefinitionId.Fune, new(0x01f0, 0x01b0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0f07),
            new(EnemyDefinitionId.Fune, new(0x01d0, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0f07),
            new(EnemyDefinitionId.Fune, new(0x01f0, 0x0380), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0f07),
            new(EnemyDefinitionId.Boulder, new(0x0150, 0x0130), 0x0050, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0080),
            new(EnemyDefinitionId.Boulder, new(0x01b8, 0x01d0), 0x0050, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0080),
            new(EnemyDefinitionId.Boulder, new(0x0128, 0x0260), 0x0050, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0080),
            new(EnemyDefinitionId.Fireflea, new(0x0156, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0520),
            new(EnemyDefinitionId.Fireflea, new(0x01c8, 0x0538), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0518),
            new(EnemyDefinitionId.Fireflea, new(0x0238, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0520),
            new(EnemyDefinitionId.Fireflea, new(0x02a8, 0x0538), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0518),
            new(EnemyDefinitionId.Fireflea, new(0x0170, 0x0476), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0720),
        ], 0x0a),
        new(nameof(MagdolliteTunnel),
        [
            new(EnemyDefinitionId.Magdollite, new(0x00f8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3a60),
            new(EnemyDefinitionId.Magdollite, new(0x00f8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Magdollite, new(0x00f8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Magdollite, new(0x01e8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3a60),
            new(EnemyDefinitionId.Magdollite, new(0x01e8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Magdollite, new(0x01e8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Magdollite, new(0x0248, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3a60),
            new(EnemyDefinitionId.Magdollite, new(0x0248, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Magdollite, new(0x0248, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Multiviola, new(0x0078, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0060, 0x0002),
            new(EnemyDefinitionId.Multiviola, new(0x023c, 0x005d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0002),
        ], 0x03),
        new(nameof(UnusedRoom8FB3EEA1AD06),
        [
        ], 0x00),
        new(nameof(LavaDive),
        [
            new(EnemyDefinitionId.Namihe, new(0x01f0, 0x0108), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1001, 0x8005),
            new(EnemyDefinitionId.Namihe, new(0x0120, 0x01d0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1011, 0x8005),
            new(EnemyDefinitionId.Namihe, new(0x0120, 0x0248), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1011, 0x8005),
            new(EnemyDefinitionId.Namihe, new(0x027f, 0x0238), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1001, 0x8005),
            new(EnemyDefinitionId.Namihe, new(0x0390, 0x0118), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1001, 0x8005),
            new(EnemyDefinitionId.Namihe, new(0x0340, 0x0198), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1001, 0x8005),
        ], 0x06),
        new(nameof(Pillar),
        [
            new(EnemyDefinitionId.Puromi, new(0x0190, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x4010, 0x2001),
            new(EnemyDefinitionId.Puromi, new(0x02d0, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x4010, 0x2001),
        ], 0x00),
        new(nameof(CrocomireEscape),
        [
            new(EnemyDefinitionId.Dragon, new(0x0380, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0380, 0x01e8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0396, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0396, 0x007f), 0x0005, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x02c0, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x02c0, 0x01e8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0200, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0200, 0x01e8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0140, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0140, 0x01e8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0080, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0080, 0x01e8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x06),
        new(nameof(LNSpringBallMaze),
        [
            new(EnemyDefinitionId.Alcoon, new(0x0100, 0x0200), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x0151, 0x0200), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x0060, 0x0200), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x0198, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x03fb, 0x04c9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x05),
        new(nameof(LNEscapePowerBombs),
        [
        ], 0x00),
        new(nameof(PurpleShaft),
        [
        ], 0x00),
        new(nameof(RidleyETank),
        [
        ], 0x00),
        new(nameof(ScrewAttack),
        [
        ], 0x00),
        new(nameof(GreenBubblesMissiles),
        [
            new(EnemyDefinitionId.ShutterShootable, new(0x0128, 0x0100), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xffff), 0x4003, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0128, 0x0100), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xffff), 0x8003, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0184, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0184, 0x0060), 0x0005, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
        ], 0x01),
        new(nameof(Amphitheatre),
        [
            new(EnemyDefinitionId.PirateGoldWalking, new(0x01c0, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0010),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x02c0, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0008),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x02b8, 0x0230), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0008),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x0158, 0x0120), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0008),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x01d0, 0x0440), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0010),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x00c5, 0x0450), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x00a0),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x037d, 0x039d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0010),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x03c0, 0x0310), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0010),
            new(EnemyDefinitionId.PirateGoldWalking, new(0x0238, 0x01a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0008),
        ], 0x09),
        new(nameof(CathedralEntrance),
        [
            new(EnemyDefinitionId.Sova, new(0x0028, 0x0100), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0018, 0x0138), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x00f8, 0x0098), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x00b0, 0x00d8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0004),
            new(EnemyDefinitionId.Dessgeega, new(0x0188, 0x019f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dessgeega, new(0x022f, 0x019e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x06),
        new(nameof(MainHall),
        [
            new(EnemyDefinitionId.Elevator, new(0x0480, 0x02a2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.Dragon, new(0x02d0, 0x02f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x02d0, 0x02f8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0330, 0x02f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0330, 0x02f8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0197, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0197, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.Hibashi, new(0x0117, 0x02a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0060, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0117, 0x02a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.Hibashi, new(0x0097, 0x0288), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0060, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0097, 0x0288), 0x0000, EnemyPropertyWord.FromNativeWord(0x2100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.Hibashi, new(0x0217, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0050, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0217, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.Dragon, new(0x0238, 0x02f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0238, 0x02f8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0138, 0x02f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0138, 0x02f8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x01b8, 0x02f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x01b8, 0x02f8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x05),
        new(nameof(PostCrocJump),
        [
            new(EnemyDefinitionId.Mella, new(0x05c3, 0x0253), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x0599, 0x0268), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x05f1, 0x0272), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x0552, 0x025c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mella, new(0x057b, 0x027d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.GRipper, new(0x00d1, 0x009b), 0x0010, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00b1, 0x0111),
            new(EnemyDefinitionId.GRipper, new(0x0198, 0x0068), 0x0120, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0168, 0x01c8),
            new(EnemyDefinitionId.GRipper, new(0x0228, 0x0098), 0x0020, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0210, 0x0268),
            new(EnemyDefinitionId.GRipper, new(0x0360, 0x0058), 0x0110, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0300, 0x03c0),
            new(EnemyDefinitionId.Kamer2, new(0x00b0, 0x0298), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2800),
            new(EnemyDefinitionId.Kamer2, new(0x0110, 0x0298), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2800),
            new(EnemyDefinitionId.Kamer2, new(0x0170, 0x0298), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2800),
            new(EnemyDefinitionId.Kamer2, new(0x01d0, 0x0298), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2800),
        ], 0x04),
        new(nameof(FrogSave),
        [
        ], 0x00),
        new(nameof(CrocomireSave),
        [
        ], 0x00),
        new(nameof(CrumbleShaft),
        [
            new(EnemyDefinitionId.Sova, new(0x0078, 0x0106), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0038, 0x0138), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x00c0, 0x01d8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0080, 0x0256), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0080, 0x02f6), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x00c0, 0x0346), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
        ], 0x06),
    ];

    /// <summary>Provides the bank-$A1 placement lists starting at $B259.</summary>
    private static RoomEnemyPopulationDefinition[] BuildSegment2() =>
    [
        new(nameof(Wasteland),
        [
            new(EnemyDefinitionId.Hibashi, new(0x05c0, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0008, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x05c0, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.DessgeegaLarge, new(0x0288, 0x005a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.DessgeegaLarge, new(0x0228, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.DessgeegaLarge, new(0x02f0, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.DessgeegaLarge, new(0x0138, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0540, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0540, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.ShutterShootable, new(0x0528, 0x00e0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0x2003, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x0180, 0x025c), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x5070),
            new(EnemyDefinitionId.KzanBottom, new(0x0180, 0x0264), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x0180, 0x015c), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x8070),
            new(EnemyDefinitionId.KzanBottom, new(0x0180, 0x0164), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x04),
        new(nameof(AcidSnakesTunnel),
        [
            new(EnemyDefinitionId.Gamet, new(0x0110, 0x00e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3030),
            new(EnemyDefinitionId.Gamet, new(0x0110, 0x00e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0110, 0x00e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0110, 0x00e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x0110, 0x00e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Dragon, new(0x0200, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0200, 0x00f0), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x02f0, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x02f0, 0x00f0), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x03),
        new(nameof(GrappleTutorial1),
        [
        ], 0x00),
        new(nameof(BubbleMountainSave),
        [
        ], 0x00),
        new(nameof(HiJumpETank),
        [
            new(EnemyDefinitionId.Sova, new(0x0177, 0x00b5), 0x0002, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0004),
        ], 0x01),
        new(nameof(BubbleMountain),
        [
            new(EnemyDefinitionId.Ripper2, new(0x0178, 0x0370), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.Sova, new(0x0118, 0x0380), 0x0001, EnemyPropertyWord.FromNativeWord(0x2002), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0151, 0x02da), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0048, 0x0329), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Waver, new(0x00b9, 0x01b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x01c0, 0x0220), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Waver, new(0x0093, 0x00de), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x010c, 0x0095), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0000),
        ], 0x08),
        new(nameof(IceBeamAcid),
        [
            new(EnemyDefinitionId.Tripper, new(0x0190, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x1010),
            new(EnemyDefinitionId.Tripper, new(0x0100, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x1010),
            new(EnemyDefinitionId.Tripper, new(0x0070, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x1010),
        ], 0x03),
        new(nameof(IceBeamTutorial),
        [
            new(EnemyDefinitionId.Boyon, new(0x0078, 0x00d9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0103, 0x0040),
            new(EnemyDefinitionId.Boyon, new(0x00a8, 0x00d9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0103, 0x0040),
            new(EnemyDefinitionId.Boyon, new(0x00d8, 0x00d9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0203, 0x0040),
            new(EnemyDefinitionId.Ripper2, new(0x0158, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0018, 0x0000),
        ], 0x01),
        new(nameof(PostCrocMissile),
        [
            new(EnemyDefinitionId.Metaree, new(0x0118, 0x0038), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Metaree, new(0x00f8, 0x0040), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Gamet, new(0x02b0, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3030),
            new(EnemyDefinitionId.Gamet, new(0x02b0, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x02b0, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x02b0, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
            new(EnemyDefinitionId.Gamet, new(0x02b0, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x3000),
        ], 0x03),
        new(nameof(Cathedral),
        [
            new(EnemyDefinitionId.Geruta, new(0x00a1, 0x0053), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x00a1, 0x0053), 0x0005, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x01de, 0x0141), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x01de, 0x0141), 0x0005, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0224, 0x007d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0224, 0x007d), 0x0005, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Sova, new(0x0140, 0x00e5), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x006e, 0x0116), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x00d8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2002), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x029c, 0x00f4), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
        ], 0x07),
        new(nameof(NorfairReserveTank),
        [
            new(EnemyDefinitionId.Dragon, new(0x0158, 0x00f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0158, 0x00f8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x00d8, 0x00f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x00d8, 0x00f8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0098, 0x00f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Dragon, new(0x0098, 0x00f8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sova, new(0x0078, 0x0045), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0006, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x00f8, 0x0050), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0006, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0187, 0x003a), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0006, 0x0004),
        ], 0x06),
        new(nameof(GrappleTutorial2),
        [
            new(EnemyDefinitionId.Fune, new(0x00f0, 0x0290), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x2000, 0xa007),
            new(EnemyDefinitionId.Fune, new(0x0010, 0x01b0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x2010, 0xa007),
            new(EnemyDefinitionId.Fune, new(0x00f0, 0x0210), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x2000, 0xa007),
        ], 0x03),
        new(nameof(IceBeamSnake),
        [
            new(EnemyDefinitionId.Sova, new(0x0158, 0x01b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0198, 0x01b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0178, 0x01b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Fune, new(0x00b0, 0x01c4), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5000, 0x5007),
            new(EnemyDefinitionId.Fune, new(0x0020, 0x0100), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5010, 0x5007),
            new(EnemyDefinitionId.Fune, new(0x00b0, 0x0144), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5000, 0x5007),
            new(EnemyDefinitionId.Fune, new(0x0020, 0x0290), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5010, 0x5007),
        ], 0x07),
        new(nameof(GoldenTorizo),
        [
            new(EnemyDefinitionId.GoldenTorizo, new(0x0080, 0x0180), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(SpikyPlatformsTunnel),
        [
            new(EnemyDefinitionId.Tripper, new(0x0280, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.Tripper, new(0x0180, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0718),
            new(EnemyDefinitionId.ShutterShootable, new(0x0200, 0x0008), 0x0110, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0303), 0x4002, 0x0030),
        ], 0x02),
        new(nameof(SpeedBooster),
        [
        ], 0x00),
        new(nameof(LNFarming),
        [
            new(EnemyDefinitionId.Viola, new(0x0228, 0x0088), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x0238, 0x00d8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x0168, 0x0088), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x0168, 0x00d8), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x00d8, 0x0098), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Zebbo, new(0x0298, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0258, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0208, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x01c8, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0148, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x00f8, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x0b),
        new(nameof(SingleChamber),
        [
            new(EnemyDefinitionId.Multiviola, new(0x003c, 0x0356), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x00d0, 0x0002),
            new(EnemyDefinitionId.Multiviola, new(0x0099, 0x01e3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0058, 0x0002),
            new(EnemyDefinitionId.Multiviola, new(0x00cf, 0x00d2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0088, 0x0002),
            new(EnemyDefinitionId.Alcoon, new(0x0080, 0x0338), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x0080, 0x0238), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x00d8, 0x0138), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Alcoon, new(0x0088, 0x0138), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x04),
        new(nameof(SpeedBoosterHall),
        [
            new(EnemyDefinitionId.Geruta, new(0x0388, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0388, 0x0128), 0x0005, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0488, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x0488, 0x0128), 0x0005, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x062c, 0x0188), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Geruta, new(0x062c, 0x0188), 0x0005, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.Metaree, new(0x0120, 0x0064), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Metaree, new(0x00f0, 0x0056), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x05),
        new(nameof(BatCave),
        [
            new(EnemyDefinitionId.Gamet, new(0x0060, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2020),
            new(EnemyDefinitionId.Gamet, new(0x0060, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2000),
            new(EnemyDefinitionId.Gamet, new(0x0060, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2000),
            new(EnemyDefinitionId.Gamet, new(0x0060, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2000),
            new(EnemyDefinitionId.Gamet, new(0x0060, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2000),
            new(EnemyDefinitionId.Skree, new(0x00c8, 0x0148), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Skree, new(0x00a8, 0x0140), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Skree, new(0x0080, 0x0138), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x04),
        new(nameof(KronicBoost),
        [
            new(EnemyDefinitionId.Viola, new(0x0188, 0x00a8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x0160, 0x01c8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x01a0, 0x0208), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
            new(EnemyDefinitionId.Viola, new(0x0180, 0x0258), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0006),
        ], 0x04),
        new(nameof(BusinessCenter),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x02c2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.Sova, new(0x007b, 0x0358), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0090, 0x03e8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0061, 0x0438), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x00a0, 0x0518), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0060, 0x0548), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0004),
            new(EnemyDefinitionId.Sova, new(0x0081, 0x05e8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0004),
        ], 0x06),
        new(nameof(ThreeMusketeers),
        [
            new(EnemyDefinitionId.Hibashi, new(0x0297, 0x0280), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0297, 0x0280), 0x0000, EnemyPropertyWord.FromNativeWord(0x2100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.Hibashi, new(0x0337, 0x0280), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0000),
            new(EnemyDefinitionId.Hibashi, new(0x0337, 0x0280), 0x0000, EnemyPropertyWord.FromNativeWord(0x2100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0001),
            new(EnemyDefinitionId.KihunterRed, new(0x0155, 0x01f2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0384, 0x0000),
            new(EnemyDefinitionId.KihunterRedWings, new(0x0155, 0x01f2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterRed, new(0x0165, 0x0174), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0384, 0x0000),
            new(EnemyDefinitionId.KihunterRedWings, new(0x0165, 0x0174), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterRed, new(0x014a, 0x0108), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0384, 0x0000),
            new(EnemyDefinitionId.KihunterRedWings, new(0x014a, 0x0108), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x02e8, 0x025a), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xff00), 0x4003, 0x0000),
            new(EnemyDefinitionId.ShutterShootable, new(0x0388, 0x023a), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xff00), 0x4003, 0x0000),
        ], 0x03),
        new(nameof(Crocomire),
        [
            new(EnemyDefinitionId.Crocomire, new(0x0480, 0x0078), 0xbd2a, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.CrocomireTongue, new(0x0480, 0x0078), 0xbd2a, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(LNElevSave),
        [
        ], 0x00),
        new(nameof(Volcano),
        [
            new(EnemyDefinitionId.Fune, new(0x01d0, 0x0280), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x8005),
            new(EnemyDefinitionId.Fune, new(0x02e0, 0x0210), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0007),
            new(EnemyDefinitionId.Fune, new(0x0220, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8010, 0x0007),
            new(EnemyDefinitionId.Fune, new(0x02e0, 0x0178), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0007),
            new(EnemyDefinitionId.Fune, new(0x0220, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8010, 0x0007),
            new(EnemyDefinitionId.Fune, new(0x02e0, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0007),
            new(EnemyDefinitionId.LavaRocks, new(0x00f8, 0x02c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.LavaRocks, new(0x0080, 0x02c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.LavaRocks, new(0x0088, 0x02c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.LavaRocks, new(0x0108, 0x02c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2500), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x0a),
        new(nameof(UpperNorfairFarming),
        [
            new(EnemyDefinitionId.Fune, new(0x01f0, 0x0190), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x2000, 0x2007),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x4050),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x4000),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x4000),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x4000),
            new(EnemyDefinitionId.Gamet, new(0x0130, 0x01c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x4000),
        ], 0x02),
        new(nameof(SpongeBath1),
        [
            new(EnemyDefinitionId.Bull, new(0x00f0, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0003),
        ], 0x01),
        new(nameof(WreckedShipETank1),
        [
            new(EnemyDefinitionId.Skultera, new(0x00ad, 0x0176), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x0240, 0x012c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0110, 0x0210),
            new(EnemyDefinitionId.Kamer2, new(0x0118, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2000),
            new(EnemyDefinitionId.Kamer2, new(0x0180, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2000),
            new(EnemyDefinitionId.Kamer2, new(0x01f0, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x2000),
        ], 0x02),
        new(nameof(WreckedShipMainShaft0),
        [
            new(EnemyDefinitionId.Coven, new(0x0038, 0x03d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sbug, new(0x0442, 0x034a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x044d, 0x0351), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7c02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x04c8, 0x03cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xcc03, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x04c5, 0x03d3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd804, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x04ce, 0x03d9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd003, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x04cb, 0x04cb), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x2003, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x04cc, 0x04d6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xff02, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x04c4, 0x04d1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0a03, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x04de, 0x04be), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd003, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03a7, 0x0521), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7a02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03b9, 0x052c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7e02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x04c4, 0x027d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xfc02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x04ca, 0x028a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0803, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x04db, 0x0278), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe003, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0425, 0x0210), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8402, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0424, 0x0206), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7802, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x0145, 0x02ba), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9103, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x013d, 0x02c3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8c02, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x010d, 0x02db), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xac03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01ad, 0x04e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe804, 0x0020),
            new(EnemyDefinitionId.KzanTop, new(0x0260, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanBottom, new(0x0260, 0x0560), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x01c0, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanBottom, new(0x01c0, 0x0560), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x0120, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanBottom, new(0x0120, 0x0560), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x0080, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanBottom, new(0x0080, 0x0560), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0430, 0x07d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x04d0, 0x07d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x03),
        new(nameof(BowlingAlley0),
        [
            new(EnemyDefinitionId.Chozo, new(0x04c8, 0x018a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sbug, new(0x04cb, 0x0189), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc203, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x04cc, 0x017c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xdc04, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x04c5, 0x0184), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xde03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x04d1, 0x0191), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc402, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x04cc, 0x019d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd803, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0026, 0x01d2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x009d, 0x01ea), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8c03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x00ce, 0x01d2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x00d3, 0x01cf), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9004, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0025, 0x01b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x002d, 0x01b9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0030, 0x01bf), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9202, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01e2, 0x01da), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01dd, 0x01e3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8803, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01f8, 0x01e3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe003, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0275, 0x01e1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd003, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x035d, 0x01d2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0332, 0x01e7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd002, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x035f, 0x01d9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc804, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0324, 0x01e2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc402, 0x0080),
        ], 0x00),
        new(nameof(Attic1),
        [
            new(EnemyDefinitionId.KihunterYellow, new(0x0419, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xffff, 0x0000),
            new(EnemyDefinitionId.KihunterYellowWings, new(0x0419, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterYellow, new(0x01ff, 0x008e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KihunterYellowWings, new(0x01ff, 0x008e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.KihunterYellow, new(0x05d5, 0x0076), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KihunterYellowWings, new(0x05d5, 0x0076), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x058f, 0x005a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0008),
            new(EnemyDefinitionId.Atomic, new(0x0352, 0x0061), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0008),
            new(EnemyDefinitionId.Atomic, new(0x022c, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0008),
            new(EnemyDefinitionId.Atomic, new(0x0093, 0x007c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0008),
            new(EnemyDefinitionId.KihunterYellow, new(0x04e8, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xffff, 0x0000),
            new(EnemyDefinitionId.KihunterYellowWings, new(0x04e8, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0000),
            new(EnemyDefinitionId.Spark, new(0x00e0, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0090),
            new(EnemyDefinitionId.Spark, new(0x0130, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0050),
            new(EnemyDefinitionId.Spark, new(0x0290, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0020),
            new(EnemyDefinitionId.Spark, new(0x02f0, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0080),
            new(EnemyDefinitionId.Spark, new(0x0370, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0030),
            new(EnemyDefinitionId.Spark, new(0x0430, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0080),
            new(EnemyDefinitionId.Spark, new(0x04d0, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0070),
            new(EnemyDefinitionId.Spark, new(0x05d0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
            new(EnemyDefinitionId.Spark, new(0x0630, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0060),
        ], 0x08),
        new(nameof(Basement1),
        [
            new(EnemyDefinitionId.Robot, new(0x004d, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Robot, new(0x0370, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0098, 0x004d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0008),
            new(EnemyDefinitionId.Atomic, new(0x027e, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0008),
            new(EnemyDefinitionId.Atomic, new(0x0354, 0x0049), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0008),
            new(EnemyDefinitionId.Spark, new(0x0479, 0x006a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0020),
        ], 0x05),
        new(nameof(WreckedShipMap1),
        [
        ], 0x00),
        new(nameof(WSWestSuper0),
        [
        ], 0x00),
        new(nameof(SpikyDeath0),
        [
        ], 0x00),
        new(nameof(SpongeBath0),
        [
        ], 0x00),
        new(nameof(WreckedShipEntrance1),
        [
        ], 0x00),
        new(nameof(ElectricDeath0),
        [
        ], 0x00),
        new(nameof(BowlingAlley1),
        [
            new(EnemyDefinitionId.Chozo, new(0x04c8, 0x018a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Robot, new(0x02f0, 0x0270), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Robot, new(0x0370, 0x0270), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x02),
        new(nameof(AssemblyLine0),
        [
        ], 0x00),
        new(nameof(Phantoon1),
        [
        ], 0x00),
        new(nameof(WreckedShipETank0),
        [
        ], 0x00),
        new(nameof(GravitySuit0),
        [
        ], 0x00),
        new(nameof(WreckedShipMap0),
        [
            new(EnemyDefinitionId.Coven, new(0x0088, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sbug, new(0x0058, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0050, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc402, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x004d, 0x00cf), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xfa03, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00cd, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb004, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x00c8, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb803, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x00d0, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd003, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x008b, 0x0027), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7802, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x008d, 0x0008), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8a02, 0x0050),
        ], 0x01),
        new(nameof(GravitySuit1),
        [
        ], 0x00),
        new(nameof(Basement0),
        [
            new(EnemyDefinitionId.Sbug, new(0x036b, 0x00bb), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc203, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0370, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xdc04, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0370, 0x00bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xde03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0368, 0x00c3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc402, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0372, 0x00c9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd803, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x004e, 0x00aa), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0051, 0x00ad), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8c03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0048, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7002, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0052, 0x00c2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9004, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0029, 0x00aa), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x002d, 0x00b9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0030, 0x00bf), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9202, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01e2, 0x00da), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01dd, 0x00e3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8803, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01f8, 0x00e3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe003, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x03f3, 0x00af), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd003, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x03ef, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x03ea, 0x00cb), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd002, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x03cc, 0x00de), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc804, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x03ce, 0x00de), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc402, 0x0080),
            new(EnemyDefinitionId.RobotNoPower, new(0x004d, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.RobotNoPower, new(0x0370, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x02),
        new(nameof(WreckedShipEntrance0),
        [
            new(EnemyDefinitionId.Sbug, new(0x0074, 0x0040), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x007c, 0x00bd), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0096, 0x00c5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb803, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0073, 0x004a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7804, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x00ba, 0x0042), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x3003, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x00b4, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9803, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x0041, 0x0049), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7802, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x0082, 0x00bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd803, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0044, 0x004b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5803, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0257, 0x0041), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x011b, 0x00bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0112, 0x00c4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xf802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x012d, 0x021e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7c03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0125, 0x0226), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9403, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0143, 0x023f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8402, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x013f, 0x02bf), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xa102, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x0145, 0x02ba), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9103, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x013d, 0x02c3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8c02, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x010d, 0x02db), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xac03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x01ad, 0x04e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe804, 0x0020),
            new(EnemyDefinitionId.Sbug, new(0x0261, 0x0042), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0293, 0x0037), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x4802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x027a, 0x00b5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xcc02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x028b, 0x00be), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xdd02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0261, 0x00cb), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03e9, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03d9, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03eb, 0x00c4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03de, 0x0051), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7d02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x03ef, 0x004c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x3c02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x036b, 0x004b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0317, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xf802, 0x0050),
        ], 0x00),
        new(nameof(AssemblyLine1),
        [
            new(EnemyDefinitionId.Spark, new(0x01d0, 0x005d), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0098),
            new(EnemyDefinitionId.Spark, new(0x0270, 0x005b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0040),
            new(EnemyDefinitionId.Spark, new(0x0090, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x00a0),
            new(EnemyDefinitionId.Spark, new(0x00f0, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0030),
            new(EnemyDefinitionId.Spark, new(0x012d, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
            new(EnemyDefinitionId.Spark, new(0x0190, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0080),
            new(EnemyDefinitionId.Spark, new(0x0230, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0028),
            new(EnemyDefinitionId.Spark, new(0x02d8, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0020),
            new(EnemyDefinitionId.Robot, new(0x0250, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Robot, new(0x0110, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Robot, new(0x01b0, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x03),
        new(nameof(WreckedShipSave1),
        [
        ], 0x00),
        new(nameof(WSWestSuper1),
        [
            new(EnemyDefinitionId.Spark, new(0x009a, 0x003a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0030),
            new(EnemyDefinitionId.Spark, new(0x0034, 0x003b), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0020),
            new(EnemyDefinitionId.Spark, new(0x0097, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0028),
            new(EnemyDefinitionId.Spark, new(0x0067, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0030),
            new(EnemyDefinitionId.Spark, new(0x00b2, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
        ], 0x00),
        new(nameof(Attic0),
        [
            new(EnemyDefinitionId.Coven, new(0x02d7, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sbug, new(0x06a9, 0x00ba), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xaa02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x06af, 0x00bd), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd403, 0x0550),
            new(EnemyDefinitionId.Sbug, new(0x06ad, 0x00c5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x06b9, 0x00b2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xf002, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0028, 0x00b9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7403, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x0025, 0x00be), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8803, 0x05a0),
            new(EnemyDefinitionId.Sbug, new(0x001b, 0x00c6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xa002, 0x00a0),
            new(EnemyDefinitionId.Sbug, new(0x0033, 0x0052), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7803, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0028, 0x0043), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x5002, 0x0550),
            new(EnemyDefinitionId.Sbug, new(0x040e, 0x00e6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8203, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x011e, 0x00ea), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8402, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0114, 0x00e6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8203, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x001a, 0x0049), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9803, 0x0580),
            new(EnemyDefinitionId.Sbug, new(0x0431, 0x00e9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x043d, 0x00ed), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xba04, 0x0580),
            new(EnemyDefinitionId.Sbug, new(0x05c3, 0x00e3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc401, 0x0540),
            new(EnemyDefinitionId.Sbug, new(0x05d2, 0x00d7), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb802, 0x0040),
            new(EnemyDefinitionId.Sbug, new(0x01a7, 0x0094), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x4003, 0x0540),
            new(EnemyDefinitionId.Sbug, new(0x06d5, 0x00c5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe002, 0x0080),
            new(EnemyDefinitionId.Atomic, new(0x00e0, 0x004e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x02f0, 0x005e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0630, 0x006e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x05d0, 0x00be), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0290, 0x00be), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0130, 0x00be), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0370, 0x00de), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x04d0, 0x007e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0430, 0x007e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x0a),
        new(nameof(WSEastSuper0),
        [
            new(EnemyDefinitionId.Coven, new(0x0088, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sbug, new(0x034f, 0x009a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc203, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x00ff, 0x00e9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xfc04, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0354, 0x00a6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xde02, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0354, 0x00a4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc402, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0347, 0x00a6), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0175, 0x00d1), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0051, 0x00ad), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8c03, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0048, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7002, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x02de, 0x0099), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x3003, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x02cb, 0x008f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x002d, 0x00b9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9803, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0030, 0x00bf), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9202, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x02c9, 0x008a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x02e3, 0x0073), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xa804, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x02e7, 0x00aa), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe002, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x03f3, 0x00af), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd004, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x03ef, 0x00b3), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x03ea, 0x00cb), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xd002, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x0184, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xc802, 0x0080),
            new(EnemyDefinitionId.Sbug, new(0x00f0, 0x00e5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8202, 0x0080),
            new(EnemyDefinitionId.RobotNoPower, new(0x0178, 0x0090), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.RobotNoPower, new(0x02c4, 0x0090), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x00a0, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0120, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x01d0, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Atomic, new(0x0250, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
        ], 0x07),
        new(nameof(WreckedShipSave0),
        [
            new(EnemyDefinitionId.Coven, new(0x0088, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x6800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sbug, new(0x0083, 0x00ba), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xb002, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x007b, 0x00b5), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xca02, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x008e, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xfa02, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x0074, 0x00c4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8302, 0x0030),
            new(EnemyDefinitionId.Sbug, new(0x00b9, 0x002e), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x7b02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00c1, 0x0035), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xe002, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00ba, 0x0036), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xf802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00cb, 0x0040), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x1802, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x00c5, 0x0046), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xfd02, 0x0050),
            new(EnemyDefinitionId.Sbug, new(0x0033, 0x00ab), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8802, 0x0130),
            new(EnemyDefinitionId.Sbug, new(0x0038, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x9002, 0x0030),
        ], 0x01),
        new(nameof(ElectricDeath1),
        [
            new(EnemyDefinitionId.Spark, new(0x0068, 0x0258), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0080),
            new(EnemyDefinitionId.Spark, new(0x002a, 0x0227), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0080),
            new(EnemyDefinitionId.Spark, new(0x00c8, 0x01dd), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
            new(EnemyDefinitionId.Spark, new(0x0086, 0x018a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0090),
            new(EnemyDefinitionId.Spark, new(0x004f, 0x0033), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0090),
            new(EnemyDefinitionId.Spark, new(0x0035, 0x00da), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
            new(EnemyDefinitionId.Spark, new(0x005c, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x00a0),
        ], 0x00),
        new(nameof(SpikyDeath1),
        [
            new(EnemyDefinitionId.KzanTop, new(0x0050, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x6810),
            new(EnemyDefinitionId.KzanBottom, new(0x0050, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x00a0, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x680c),
            new(EnemyDefinitionId.KzanBottom, new(0x00a0, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x0100, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x6811),
            new(EnemyDefinitionId.KzanBottom, new(0x0100, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x0160, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x680a),
            new(EnemyDefinitionId.KzanBottom, new(0x0160, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x01b0, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x6814),
            new(EnemyDefinitionId.KzanBottom, new(0x01b0, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(WSEastSuper1),
        [
            new(EnemyDefinitionId.Robot, new(0x0178, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Robot, new(0x02c0, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Bull, new(0x00e0, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0003),
            new(EnemyDefinitionId.Bull, new(0x0210, 0x0075), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0003),
            new(EnemyDefinitionId.Spark, new(0x00a0, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0080),
            new(EnemyDefinitionId.Spark, new(0x0120, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
            new(EnemyDefinitionId.Spark, new(0x01d0, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x00a0),
            new(EnemyDefinitionId.Spark, new(0x0250, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0080),
        ], 0x04),
        new(nameof(Phantoon0),
        [
            new(EnemyDefinitionId.PhantoonBody, new(0x0080, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.PhantoonEye, new(0x0080, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0001),
            new(EnemyDefinitionId.PhantoonTentacles, new(0x0080, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0002),
            new(EnemyDefinitionId.PhantoonMouth, new(0x0080, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0003),
        ], 0x00),
        new(nameof(WreckedShipMainShaft1),
        [
            new(EnemyDefinitionId.Spark, new(0x0498, 0x02a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0030),
            new(EnemyDefinitionId.Spark, new(0x0470, 0x0359), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0028),
            new(EnemyDefinitionId.Spark, new(0x0420, 0x048c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0020),
            new(EnemyDefinitionId.Spark, new(0x04c8, 0x0508), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0020),
            new(EnemyDefinitionId.Spark, new(0x0488, 0x05bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.Spark, new(0x0469, 0x071a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Spark, new(0x0418, 0x0788), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0030),
            new(EnemyDefinitionId.Atomic, new(0x0478, 0x0252), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0008),
            new(EnemyDefinitionId.Atomic, new(0x04d5, 0x02ab), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0008),
            new(EnemyDefinitionId.Atomic, new(0x04c0, 0x0507), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0008),
            new(EnemyDefinitionId.Atomic, new(0x0426, 0x05b9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0008),
            new(EnemyDefinitionId.KzanTop, new(0x0260, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x5030),
            new(EnemyDefinitionId.KzanBottom, new(0x0260, 0x0560), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x01c0, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x5030),
            new(EnemyDefinitionId.KzanBottom, new(0x01c0, 0x0560), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x0120, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x5030),
            new(EnemyDefinitionId.KzanBottom, new(0x0120, 0x0560), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.KzanTop, new(0x0080, 0x0558), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x5030),
            new(EnemyDefinitionId.KzanBottom, new(0x0080, 0x0560), 0x0000, EnemyPropertyWord.FromNativeWord(0x0100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Spark, new(0x04d0, 0x07c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0030),
            new(EnemyDefinitionId.Spark, new(0x0430, 0x07c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0020),
        ], 0x04),
        new(nameof(MtEverest),
        [
            new(EnemyDefinitionId.Powamp, new(0x0200, 0x0180), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0070),
            new(EnemyDefinitionId.Powamp, new(0x0200, 0x0180), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Powamp, new(0x0308, 0x0200), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0120),
            new(EnemyDefinitionId.Powamp, new(0x0308, 0x0200), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Powamp, new(0x0400, 0x0230), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0030),
            new(EnemyDefinitionId.Powamp, new(0x0400, 0x0230), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x02f0, 0x0387), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0228, 0x3b60), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0168, 0x0240), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x03c0, 0x03b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0268, 0x02a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0499, 0x0358), 0x0001, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x09),
        new(nameof(HalfieClimb),
        [
            new(EnemyDefinitionId.Oum, new(0x0250, 0x0260), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.Oum, new(0x02d0, 0x0260), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.Oum, new(0x0370, 0x0260), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x00a0, 0x0238), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0050, 0x0190), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0060, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x06),
        new(nameof(WateringHole),
        [
            new(EnemyDefinitionId.Choot, new(0x0078, 0x01e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0008),
            new(EnemyDefinitionId.Zeb, new(0x0110, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zeb, new(0x0190, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x03),
        new(nameof(CrabShaft),
        [
            new(EnemyDefinitionId.Sciser, new(0x0170, 0x0367), 0x0002, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0030, 0x03b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x00e0, 0x0367), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0080, 0x00b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
        ], 0x04),
        new(nameof(MaridiaElev),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x02c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
            new(EnemyDefinitionId.Ripper, new(0x00d0, 0x0314), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x0070, 0x0384), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0001),
            new(EnemyDefinitionId.Ripper, new(0x0080, 0x03f4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x0090, 0x0454), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x0040, 0x04c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Ripper, new(0x0038, 0x02d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0000),
            new(EnemyDefinitionId.Owtch, new(0x0098, 0x05c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0300, 0x0104),
        ], 0x07),
        new(nameof(Colosseum),
        [
            new(EnemyDefinitionId.Mochtroid, new(0x0080, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0290, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0450, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0490, 0x0060), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0660, 0x0088), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x00a8, 0x01a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0168, 0x0158), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0630, 0x01a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x08),
        new(nameof(AqueductSave),
        [
        ], 0x00),
        new(nameof(ThePrecious),
        [
        ], 0x00),
        new(nameof(BotwoonETank),
        [
            new(EnemyDefinitionId.Puyo, new(0x0120, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0190, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x01f0, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0250, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x03f0, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0016),
            new(EnemyDefinitionId.Zoa, new(0x0560, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x0560, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x0560, 0x00e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x0560, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x0560, 0x00f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x0a),
        new(nameof(DraygonSave),
        [
        ], 0x00),
        new(nameof(MaridiaMissileRefill),
        [
        ], 0x00),
        new(nameof(PlasmaBeachQuicksand),
        [
            new(EnemyDefinitionId.Bull, new(0x0020, 0x0038), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Bull, new(0x00e0, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0001),
            new(EnemyDefinitionId.Bull, new(0x0058, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0007),
        ], 0x03),
        new(nameof(BotwoonQuicksand),
        [
            new(EnemyDefinitionId.Bull, new(0x0048, 0x0020), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0001),
            new(EnemyDefinitionId.Bull, new(0x0048, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0002),
            new(EnemyDefinitionId.Bull, new(0x00f0, 0x0090), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0006),
            new(EnemyDefinitionId.Bull, new(0x0048, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0007),
            new(EnemyDefinitionId.Bull, new(0x0100, 0x0038), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0002),
            new(EnemyDefinitionId.Bull, new(0x01b8, 0x0028), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0001),
            new(EnemyDefinitionId.Bull, new(0x01c8, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0002),
            new(EnemyDefinitionId.Bull, new(0x01e0, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0007),
            new(EnemyDefinitionId.Bull, new(0x0100, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0007),
        ], 0x09),
        new(nameof(Shaktool),
        [
            new(EnemyDefinitionId.Shaktool, new(0x00a8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Shaktool, new(0x00a8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0002),
            new(EnemyDefinitionId.Shaktool, new(0x00a8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0004),
            new(EnemyDefinitionId.Shaktool, new(0x00a8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0006),
            new(EnemyDefinitionId.Shaktool, new(0x00a8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0008),
            new(EnemyDefinitionId.Shaktool, new(0x00a8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x000a),
            new(EnemyDefinitionId.Shaktool, new(0x00a8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x000c),
            new(EnemyDefinitionId.Yard, new(0x0398, 0x00b8), 0x0006, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Yard, new(0x03d8, 0x0050), 0x0002, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x03),
        new(nameof(Draygon0),
        [
            new(EnemyDefinitionId.DraygonBody, new(0xffb0, 0xffb0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.DraygonEye, new(0xffb0, 0xffb0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.DraygonTail, new(0xffb0, 0xffb0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.DraygonArms, new(0xffb0, 0xffb0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(Aqueduct),
        [
            new(EnemyDefinitionId.Yard, new(0x0544, 0x01d8), 0x0007, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Yard, new(0x0220, 0x0294), 0x0006, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Yard, new(0x0420, 0x0264), 0x0004, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Yard, new(0x01d8, 0x0130), 0x0003, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0007, 0x0000),
            new(EnemyDefinitionId.Yard, new(0x0550, 0x02b8), 0x0004, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x05),
    ];

    /// <summary>Provides the final bank-$A1 placement lists starting at $D3AA.</summary>
    private static RoomEnemyPopulationDefinition[] BuildSegment3() =>
    [
        new(nameof(RedFish),
        [
            new(EnemyDefinitionId.Skultera, new(0x02c0, 0x01b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0008, 0x1001),
            new(EnemyDefinitionId.Zebbo, new(0x0088, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0088, 0x00d4), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0088, 0x00dc), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x04),
        new(nameof(Plasma),
        [
            new(EnemyDefinitionId.PirateMagentaWalking, new(0x0100, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0020),
            new(EnemyDefinitionId.PirateMagentaWalking, new(0x0080, 0x01d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0030),
            new(EnemyDefinitionId.PirateMagentaWalking, new(0x01b0, 0x01d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0030),
            new(EnemyDefinitionId.PirateMagentaWall, new(0x0030, 0x0180), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x01a0),
            new(EnemyDefinitionId.PirateMagentaWall, new(0x01d0, 0x0130), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x01a0),
            new(EnemyDefinitionId.PirateMagentaWalking, new(0x0078, 0x0280), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0080),
        ], 0x06),
        new(nameof(SpaceJump),
        [
        ], 0x00),
        new(nameof(Pants),
        [
            new(EnemyDefinitionId.Menu, new(0x0050, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Menu, new(0x0030, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Menu, new(0x00b8, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0170, 0x029c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0080, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0160, 0x01fc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0080, 0x0016),
            new(EnemyDefinitionId.Menu, new(0x0170, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Menu, new(0x0198, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Menu, new(0x0090, 0x0070), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x01c0, 0x02dc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0180, 0x032c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x01a0, 0x019c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0150, 0x03f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0008),
            new(EnemyDefinitionId.Puyo, new(0x01b0, 0x03f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0008, 0x0008),
        ], 0x07),
        new(nameof(GlassTunnelSave),
        [
        ], 0x00),
        new(nameof(GlassTunnel),
        [
            new(EnemyDefinitionId.NoobTubeCrack, new(0x0080, 0x0100), 0x0000, EnemyPropertyWord.FromNativeWord(0x2200), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(WestTunnel),
        [
        ], 0x00),
        new(nameof(EastTunnel),
        [
            new(EnemyDefinitionId.Boyon, new(0x0088, 0x00c0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0104, 0x0008),
            new(EnemyDefinitionId.Boyon, new(0x01e8, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0008),
            new(EnemyDefinitionId.Boyon, new(0x0218, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0104, 0x0020),
            new(EnemyDefinitionId.Boyon, new(0x0278, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0304, 0x0008),
            new(EnemyDefinitionId.Boyon, new(0x02d8, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0306, 0x0008),
            new(EnemyDefinitionId.Boyon, new(0x0308, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0304, 0x0008),
            new(EnemyDefinitionId.Boyon, new(0x0368, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0008),
            new(EnemyDefinitionId.Zebbo, new(0x00b8, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x02a8, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Zebbo, new(0x0248, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6900), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x03),
        new(nameof(MamaTurtle),
        [
            new(EnemyDefinitionId.MamaTurtle, new(0x01d8, 0x03cd), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.BabyTurtle, new(0x01b0, 0x03cd), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xffff, 0x0000),
            new(EnemyDefinitionId.BabyTurtle, new(0x01d8, 0x03cd), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0xffff, 0x0000),
            new(EnemyDefinitionId.BabyTurtle, new(0x01e8, 0x03cd), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.BabyTurtle, new(0x0218, 0x03cd), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x01),
        new(nameof(CrabTunnel),
        [
            new(EnemyDefinitionId.Sciser, new(0x0080, 0x0047), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0130, 0x0087), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x01c0, 0x00c8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0200, 0x0047), 0x0002, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x02b0, 0x00c8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0370, 0x0047), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x06),
        new(nameof(NWestMaridiaBug),
        [
            new(EnemyDefinitionId.Owtch, new(0x0040, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0004),
            new(EnemyDefinitionId.Owtch, new(0x00b8, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0108),
            new(EnemyDefinitionId.Owtch, new(0x0170, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0308),
            new(EnemyDefinitionId.Owtch, new(0x0198, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0301, 0x0104),
            new(EnemyDefinitionId.Owtch, new(0x0290, 0x01b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0008),
            new(EnemyDefinitionId.Owtch, new(0x0308, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0500, 0x0104),
            new(EnemyDefinitionId.Owtch, new(0x0378, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0501, 0x0108),
            new(EnemyDefinitionId.Menu, new(0x0020, 0x0110), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Menu, new(0x0018, 0x0140), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Menu, new(0x0010, 0x0170), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Menu, new(0x02f0, 0x0160), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Menu, new(0x0360, 0x0140), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x07),
        new(nameof(PseudoPlasmaSpark),
        [
            new(EnemyDefinitionId.Owtch, new(0x0180, 0x02b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0208),
            new(EnemyDefinitionId.Choot, new(0x00f0, 0x01a4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0203, 0x0002),
            new(EnemyDefinitionId.Choot, new(0x0210, 0x017c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0004),
            new(EnemyDefinitionId.Choot, new(0x02a8, 0x013c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0102, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x0310, 0x00dc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0008),
            new(EnemyDefinitionId.Choot, new(0x0368, 0x009c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0004),
            new(EnemyDefinitionId.Skultera, new(0x0070, 0x0258), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x01c0, 0x0280), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0110, 0x0210),
            new(EnemyDefinitionId.Choot, new(0x03b8, 0x01e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0008),
        ], 0x09),
        new(nameof(WestSandHallTunnel),
        [
            new(EnemyDefinitionId.Sciser, new(0x0088, 0x0067), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
        ], 0x01),
        new(nameof(PlasmaTutorial),
        [
            new(EnemyDefinitionId.Puyo, new(0x0057, 0x009c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0f00, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0062, 0x009c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0f00, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0069, 0x009c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0f00, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x005b, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0e00, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0065, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0e00, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0061, 0x0094), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0d00, 0x0000),
        ], 0x06),
        new(nameof(ThreadTheNeedle),
        [
            new(EnemyDefinitionId.Puyo, new(0x0118, 0x008c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0144, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x02c4, 0x008c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x05f8, 0x006c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0410, 0x006c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0470, 0x008c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0440, 0x005c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0016),
            new(EnemyDefinitionId.Choot, new(0x00d8, 0x008c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0201, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x01b0, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0202, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x0318, 0x006c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0201, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x0518, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0202, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x05a8, 0x008c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0102, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x0278, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x03a8, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x065c, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0202, 0x0000),
        ], 0x0f),
        new(nameof(PlasmaSpark),
        [
            new(EnemyDefinitionId.Owtch, new(0x0350, 0x04f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0304),
            new(EnemyDefinitionId.Owtch, new(0x0338, 0x04f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0204),
            new(EnemyDefinitionId.Owtch, new(0x0170, 0x0568), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0301, 0x0002),
            new(EnemyDefinitionId.Owtch, new(0x01a0, 0x05c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0501, 0x0204),
            new(EnemyDefinitionId.Choot, new(0x0100, 0x029c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x0230, 0x039c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x0300, 0x03a4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0207, 0x0000),
            new(EnemyDefinitionId.Choot, new(0x01e0, 0x00ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x0270, 0x00ec), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0010),
            new(EnemyDefinitionId.Skultera, new(0x0300, 0x0420), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x0270, 0x0488), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0110, 0x0210),
        ], 0x0b),
        new(nameof(Toilet),
        [
            new(EnemyDefinitionId.Mochtroid, new(0x00a0, 0x0388), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0064, 0x0358), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0004),
            new(EnemyDefinitionId.Yard, new(0x0030, 0x0698), 0x0006, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x03),
        new(nameof(WestSandHall),
        [
            new(EnemyDefinitionId.Evir, new(0x01f8, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0xf808),
            new(EnemyDefinitionId.Evir, new(0x01f8, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.EvirProjectile, new(0x01f8, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Evir, new(0x02e0, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0xd00c),
            new(EnemyDefinitionId.Evir, new(0x02e0, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.EvirProjectile, new(0x02e0, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Evir, new(0x0340, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0xf808),
            new(EnemyDefinitionId.Evir, new(0x0340, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.EvirProjectile, new(0x0340, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x03),
        new(nameof(Oasis),
        [
        ], 0x00),
        new(nameof(EastSandHall),
        [
            new(EnemyDefinitionId.Evir, new(0x00a8, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0xf808),
            new(EnemyDefinitionId.Evir, new(0x00a8, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.EvirProjectile, new(0x00a8, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Evir, new(0x0100, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0xd00c),
            new(EnemyDefinitionId.Evir, new(0x0100, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.EvirProjectile, new(0x0100, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Evir, new(0x0220, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0xd00c),
            new(EnemyDefinitionId.Evir, new(0x0220, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2400), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.EvirProjectile, new(0x0220, 0x0078), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x03),
        new(nameof(WestAqueductQuicksand),
        [
            new(EnemyDefinitionId.Bull, new(0x0028, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0008, 0x0001),
            new(EnemyDefinitionId.Bull, new(0x0048, 0x0138), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0008, 0x0001),
            new(EnemyDefinitionId.Bull, new(0x00b8, 0x0118), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0006, 0x0000),
            new(EnemyDefinitionId.Bull, new(0x00a8, 0x0178), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0007, 0x0002),
            new(EnemyDefinitionId.Bull, new(0x00d8, 0x0068), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0000),
            new(EnemyDefinitionId.Bull, new(0x0028, 0x0038), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0000),
            new(EnemyDefinitionId.Bull, new(0x0070, 0x01b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0007, 0x0002),
        ], 0x07),
        new(nameof(EastAqueductQuicksand),
        [
            new(EnemyDefinitionId.Bull, new(0x0038, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0009, 0x0001),
            new(EnemyDefinitionId.Bull, new(0x00d8, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0001),
            new(EnemyDefinitionId.Bull, new(0x0058, 0x0178), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0007, 0x0002),
            new(EnemyDefinitionId.Bull, new(0x00b8, 0x0138), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0008, 0x0002),
            new(EnemyDefinitionId.Bull, new(0x0028, 0x0028), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x000a, 0x0000),
            new(EnemyDefinitionId.Bull, new(0x0098, 0x01c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0007, 0x0002),
        ], 0x06),
        new(nameof(Butterfly),
        [
            new(EnemyDefinitionId.Zoa, new(0x0098, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x0068, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x0098, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x03),
        new(nameof(BotwoonHallway),
        [
            new(EnemyDefinitionId.Mochtroid, new(0x0030, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x00c0, 0x0090), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0170, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x0298, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Mochtroid, new(0x02f0, 0x0780), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Puyo, new(0x0260, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0008),
            new(EnemyDefinitionId.Puyo, new(0x0360, 0x0058), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0008),
        ], 0x07),
        new(nameof(EastPants),
        [
            new(EnemyDefinitionId.Puyo, new(0x0080, 0x022c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0030, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x00c0, 0x01dc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0020, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0070, 0x019c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0080, 0x0016),
            new(EnemyDefinitionId.Puyo, new(0x0050, 0x02f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0008),
            new(EnemyDefinitionId.Puyo, new(0x00b0, 0x02f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0008, 0x0008),
        ], 0x05),
        new(nameof(Springball),
        [
        ], 0x00),
        new(nameof(BelowBotwoonETank),
        [
            new(EnemyDefinitionId.Zoa, new(0x02b8, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x01c8, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x01c8, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x03b8, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zoa, new(0x01c8, 0x00e0), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Owtch, new(0x00c0, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x031c),
        ], 0x06),
        new(nameof(Kassiuz),
        [
            new(EnemyDefinitionId.Choot, new(0x0070, 0x00bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0203, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x0090, 0x01dc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0202, 0x0010),
            new(EnemyDefinitionId.Choot, new(0x0060, 0x02cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0104, 0x0010),
            new(EnemyDefinitionId.Puyo, new(0x00d8, 0x02bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0008),
            new(EnemyDefinitionId.Puyo, new(0x0038, 0x01cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0008),
            new(EnemyDefinitionId.Puyo, new(0x00d4, 0x037c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0008),
            new(EnemyDefinitionId.Puyo, new(0x00a8, 0x03ac), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0008),
        ], 0x07),
        new(nameof(ForgottenHighwaySave),
        [
        ], 0x00),
        new(nameof(MaridiaEnergyRefill),
        [
        ], 0x00),
        new(nameof(MaridiaMap),
        [
        ], 0x00),
        new(nameof(CrabHole),
        [
            new(EnemyDefinitionId.Sciser, new(0x0060, 0x0037), 0x0002, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0076, 0x00e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0076, 0x0100), 0x0000, EnemyPropertyWord.FromNativeWord(0x2801), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0080, 0x01b8), 0x0003, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
        ], 0x04),
        new(nameof(Botwoon),
        [
            new(EnemyDefinitionId.Botwoon, new(0x0080, 0x0080), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x01),
        new(nameof(MainStreet),
        [
            new(EnemyDefinitionId.Skultera, new(0x0100, 0x01a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x00f0, 0x03d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x0120, 0x04f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x00d0, 0x05a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x0100, 0x0724), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Sciser, new(0x0090, 0x0118), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x00f0, 0x0298), 0x0003, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x00f8, 0x0487), 0x0002, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0108, 0x0658), 0x0003, EnemyPropertyWord.FromNativeWord(0x2001), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0168, 0x0257), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0160, 0x0047), 0x0002, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Sciser, new(0x0097, 0x07a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x0c),
        new(nameof(BugSandHole),
        [
            new(EnemyDefinitionId.YappingMaw, new(0x00b0, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0001),
            new(EnemyDefinitionId.Zoa, new(0x0080, 0x00dc), 0x0000, EnemyPropertyWord.FromNativeWord(0x6100), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.YappingMaw, new(0x004d, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0040, 0x0001),
        ], 0x01),
        new(nameof(EastSandHole),
        [
            new(EnemyDefinitionId.Boulder, new(0x0190, 0x00a0), 0x0072, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0x7204),
            new(EnemyDefinitionId.Boulder, new(0x0150, 0x00c0), 0x0098, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0xa204),
            new(EnemyDefinitionId.Boulder, new(0x00d0, 0x00d0), 0x00c0, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0xa204),
        ], 0x00),
        new(nameof(WestSandHole),
        [
            new(EnemyDefinitionId.Boulder, new(0x01d0, 0x0090), 0x0050, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0x6204),
            new(EnemyDefinitionId.Boulder, new(0x00b0, 0x0140), 0x0080, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0xa004),
            new(EnemyDefinitionId.Boulder, new(0x00f0, 0x0160), 0x00f0, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0xf004),
            new(EnemyDefinitionId.Boulder, new(0x0030, 0x0090), 0x0040, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0200, 0x5204),
        ], 0x00),
        new(nameof(WestCactusAlley),
        [
            new(EnemyDefinitionId.Cacatac, new(0x0048, 0x00b4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x00b0, 0x0134), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x0068, 0x01b4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x00a0, 0x004a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x04),
        new(nameof(EastCactusAlley),
        [
            new(EnemyDefinitionId.Cacatac, new(0x0180, 0x0094), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x0290, 0x00a4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x0390, 0x0104), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0101, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x02d0, 0x01b4), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0100, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x00f0, 0x015a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Cacatac, new(0x02d0, 0x016a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0201),
        ], 0x06),
        new(nameof(FishTank),
        [
            new(EnemyDefinitionId.Skultera, new(0x0070, 0x0098), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0110, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x00b8, 0x02a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.Skultera, new(0x0190, 0x00f0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
            new(EnemyDefinitionId.PirateMagentaWalking, new(0x01a8, 0x02a0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0018),
            new(EnemyDefinitionId.PirateMagentaWalking, new(0x0190, 0x0140), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0008),
            new(EnemyDefinitionId.PirateMagentaWalking, new(0x02a0, 0x00d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8000, 0x0008),
            new(EnemyDefinitionId.PirateMagentaWalking, new(0x0378, 0x01b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x8001, 0x0028),
            new(EnemyDefinitionId.Skultera, new(0x0368, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0010, 0x0210),
        ], 0x08),
        new(nameof(TourianEscape4),
        [
            new(EnemyDefinitionId.PirateSilverWall, new(0x002b, 0x04d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0084),
            new(EnemyDefinitionId.PirateSilverWall, new(0x00b2, 0x0428), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x0084),
            new(EnemyDefinitionId.PirateSilverWall, new(0x002b, 0x0308), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0084),
            new(EnemyDefinitionId.PirateSilverWall, new(0x00b2, 0x0278), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x0084),
            new(EnemyDefinitionId.PirateSilverWall, new(0x002b, 0x01e8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0084),
            new(EnemyDefinitionId.PirateSilverWall, new(0x00b2, 0x0158), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x0084),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x0270, 0x0180), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0020),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x01d0, 0x0180), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0020),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x01c8, 0x02d0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x0140, 0x0200), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0020),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x01b8, 0x03b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x0168, 0x0500), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x0278, 0x0450), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
        ], 0x0d),
        new(nameof(MotherBrainSave),
        [
        ], 0x00),
        new(nameof(Metroids10),
        [
            new(EnemyDefinitionId.Metroid, new(0x010c, 0x006a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Metroid, new(0x027d, 0x0074), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Metroid, new(0x036d, 0x0092), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Metroid, new(0x04ad, 0x008c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Rinka, new(0x0028, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0218, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0498, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0468, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x04),
        new(nameof(DustTorizo),
        [
            new(EnemyDefinitionId.CorpseTorizo, new(0x0120, 0x0094), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(BigBoy),
        [
            new(EnemyDefinitionId.BabyMetroid, new(0x0180, 0x0040), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CorpseSidehopper, new(0x0228, 0x00a0), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CorpseSidehopper, new(0x00a0, 0x0040), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.CorpseZoomer, new(0x00f0, 0x0040), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CorpseZoomer, new(0x0298, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.CorpseZoomer, new(0x0108, 0x0036), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.CorpseRipper, new(0x0320, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CorpseRipper, new(0x00c2, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.CorpseSkree, new(0x0380, 0x0047), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CorpseSkree, new(0x0260, 0x0047), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.CorpseSkree, new(0x0180, 0x0047), 0x0000, EnemyPropertyWord.FromNativeWord(0xa000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
        ], 0x00),
        new(nameof(MotherBrain01),
        [
            new(EnemyDefinitionId.MotherBrainBody, new(0x0081, 0x006f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.MotherBrainHead, new(0x0081, 0x006f), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0000),
            new(EnemyDefinitionId.Zebetite, new(0x0000, 0x0000), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0337, 0x0036), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0337, 0x00a6), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0277, 0x001c), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
        ], 0x00),
        new(nameof(TourianEyeDoor),
        [
        ], 0x00),
        new(nameof(BlueHopper),
        [
            new(EnemyDefinitionId.SidehopperTourian, new(0x00f8, 0x0061), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x8000, 0x0000),
            new(EnemyDefinitionId.SidehopperTourian, new(0x0086, 0x00a9), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x02),
        new(nameof(TourianEscape1),
        [
            new(EnemyDefinitionId.ShutterDestroyable, new(0x01b8, 0x0040), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xff00), 0x2000, 0x0010),
            new(EnemyDefinitionId.ShutterDestroyable, new(0x01b8, 0x00c0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0x2000, 0x0010),
            new(EnemyDefinitionId.ShutterDestroyable, new(0x0168, 0x0040), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xff00), 0x2000, 0x0010),
            new(EnemyDefinitionId.ShutterDestroyable, new(0x0168, 0x00c0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0x2000, 0x0010),
            new(EnemyDefinitionId.ShutterDestroyable, new(0x0118, 0x0040), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xff00), 0x2000, 0x0018),
            new(EnemyDefinitionId.ShutterDestroyable, new(0x0118, 0x00c0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0x2000, 0x0018),
            new(EnemyDefinitionId.ShutterDestroyable, new(0x00c8, 0x0040), 0x0108, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0xff00), 0x2000, 0x0020),
            new(EnemyDefinitionId.ShutterDestroyable, new(0x00c8, 0x00c0), 0x0008, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x00ff), 0x2000, 0x0020),
        ], 0x00),
        new(nameof(TourianFirst),
        [
            new(EnemyDefinitionId.Elevator, new(0x0080, 0x02c2), 0x0000, EnemyPropertyWord.FromNativeWord(0x2c00), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0018),
        ], 0x00),
        new(nameof(Metroids20),
        [
            new(EnemyDefinitionId.Metroid, new(0x0098, 0x0115), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Rinka, new(0x0098, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00d8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00d8, 0x01b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0008, 0x0148), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Metroid, new(0x0056, 0x018a), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
        ], 0x02),
        new(nameof(Metroids40),
        [
            new(EnemyDefinitionId.Metroid, new(0x00c4, 0x0152), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Metroid, new(0x003e, 0x00fb), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Rinka, new(0x0098, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0098, 0x0188), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0068, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00f8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Metroid, new(0x0046, 0x01be), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
        ], 0x03),
        new(nameof(Metroids11),
        [
            new(EnemyDefinitionId.Rinka, new(0x0028, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0218, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0498, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0468, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(Metroids21),
        [
            new(EnemyDefinitionId.Rinka, new(0x0098, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00d8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00d8, 0x01b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0008, 0x0148), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(TourianEscape2),
        [
            new(EnemyDefinitionId.PirateSilverWall, new(0x002b, 0x00d8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x00a0),
            new(EnemyDefinitionId.PirateSilverWall, new(0x00d0, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0001, 0x00a0),
        ], 0x02),
        new(nameof(Metroids31),
        [
            new(EnemyDefinitionId.Rinka, new(0x0108, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0018, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0028, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00e8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x01b8, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0268, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0268, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x05d8, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x05b8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(Metroids41),
        [
            new(EnemyDefinitionId.Rinka, new(0x0098, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0098, 0x0188), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0068, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00f8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(TourianEscape3),
        [
            new(EnemyDefinitionId.PirateSilverWalking, new(0x00c0, 0x0160), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x0538, 0x0160), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x0488, 0x0160), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x03d8, 0x0160), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x0478, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x04e0, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
            new(EnemyDefinitionId.PirateSilverWalking, new(0x0560, 0x0050), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0004), 0x0000, 0x0010),
        ], 0x07),
        new(nameof(Seaweed),
        [
        ], 0x00),
        new(nameof(TourianRecharge),
        [
        ], 0x00),
        new(nameof(UpperTourianSave),
        [
        ], 0x00),
        new(nameof(Metroids30),
        [
            new(EnemyDefinitionId.Metroid, new(0x018a, 0x0081), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Metroid, new(0x0367, 0x0077), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Metroid, new(0x046a, 0x0073), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0005),
            new(EnemyDefinitionId.Rinka, new(0x0108, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0018, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0028, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00e8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x01b8, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0268, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0268, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x05d8, 0x0048), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x05b8, 0x00b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x03),
        new(nameof(RinkaShaft),
        [
            new(EnemyDefinitionId.Rinka, new(0x00e8, 0x01b8), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x00e8, 0x0208), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Rinka, new(0x0008, 0x0248), 0x0000, EnemyPropertyWord.FromNativeWord(0x6000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(CeresElev0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x00e8, 0x0277), 0x0002, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x00e0, 0x027f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(FallingTile0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(MagnetStairs0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x00e0, 0x017f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(DeadScientist0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(Room58Escape0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(CeresRidley0),
        [
            new(EnemyDefinitionId.RidleyCeres, new(0x00ba, 0x00ab), 0x0000, EnemyPropertyWord.FromNativeWord(0x2800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x0010, 0x007f), 0x0003, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
        ], 0x00),
        new(nameof(CeresElev1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x00e8, 0x0277), 0x0002, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x00e0, 0x027f), 0x0004, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x005e, 0x006c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00a2, 0x006c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x009e, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0062, 0x00c8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x005e, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00a2, 0x0128), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x009e, 0x0188), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0062, 0x0198), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x005e, 0x01f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00a2, 0x01f8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0072, 0x0258), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0005, 0x0000),
        ], 0x00),
        new(nameof(FallingTile1),
        [
            new(EnemyDefinitionId.Steam, new(0x00a0, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00e0, 0x00bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0120, 0x00bc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0160, 0x00cc), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x01b8, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0080, 0x0042), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00c0, 0x0042), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0100, 0x0042), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0140, 0x0042), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0180, 0x0042), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0048, 0x00a8), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(MagnetStairs1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x00e0, 0x017f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0070, 0x009c), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00c2, 0x00b0), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0078, 0x0116), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x003e, 0x0130), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0088, 0x0194), 0x0000, EnemyPropertyWord.FromNativeWord(0x2000), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(DeadScientist1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(Room58Escape1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), 0x0000, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), 0x0001, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(CeresRidley1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0010, 0x007f), 0x0003, EnemyPropertyWord.FromNativeWord(0xa800), EnemyExtraPropertyWord.FromNativeWord(0x0000), 0x0003, 0x0000),
        ], 0x00),
    ];
}
