namespace SuperMetroid.Core.Enemies;

/// <summary>Compiled graphics-set membership and native staging order for all retail room states.</summary>
/// <remarks>
/// The 302 bank-$B4 lists contain 425 records in the pinned NTSC J/U v1.0
/// cartridge. Palette rows and staging blocks are fixed engine staging metadata;
/// editable pixel/color content is loaded separately through enemy artwork assets.
/// </remarks>
public static class RoomEnemyGraphicsSetDefinitions
{
    /// <summary>Retail sets in the cartridge's list order.</summary>
    private static readonly RoomEnemyGraphicsSetDefinition[] Definitions =
    [
        ..BuildSegment0(),
        ..BuildSegment1(),
    ];

    /// <summary>Number of distinct retail bank-$B4 graphics-set identities in the compiled catalog, including empty sets and separately addressed sets with matching content.</summary>
    public const int ListCount = 302;
    /// <summary>Total four-byte member records across all compiled retail lists, excluding terminators and trailing metadata; not a count of unique enemy definitions or artwork images.</summary>
    public const int RecordCount = 425;

    /// <summary>Rejects a compiled catalog with missing, duplicate or misordered retail records.</summary>
    static RoomEnemyGraphicsSetDefinitions()
    {
        if (Definitions.Length != ListCount || Definitions.Sum(list => list.Records.Length) != RecordCount)
            throw new InvalidDataException("Compiled enemy graphics-set counts changed.");
        if (Definitions.Select(list => list.Name).Distinct().Count() != ListCount)
            throw new InvalidDataException("Compiled enemy graphics sets repeat a name.");
    }

    /// <summary>Every retail graphics set, in the cartridge's list order.</summary>
    public static IReadOnlyList<RoomEnemyGraphicsSetDefinition> All => Definitions;

    /// <summary>The compiled graphics set named <paramref name="name"/>.</summary>
    private static RoomEnemyGraphicsSetDefinition Find(string name) =>
        Definitions.Single(list => list.Name == name);

    /// <summary>The <c>EnemySets_Draygon1MotherBrain2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Draygon1MotherBrain2 { get; } = Find(nameof(Draygon1MotherBrain2));
    /// <summary>The <c>EnemySets_EastOcean</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastOcean { get; } = Find(nameof(EastOcean));
    /// <summary>The <c>EnemySets_CrateriaPowerBombs</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaPowerBombs { get; } = Find(nameof(CrateriaPowerBombs));
    /// <summary>The <c>EnemySets_Terminator</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Terminator { get; } = Find(nameof(Terminator));
    /// <summary>The <c>EnemySets_FinalMissileBombway1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FinalMissileBombway1 { get; } = Find(nameof(FinalMissileBombway1));
    /// <summary>The <c>EnemySets_FinalMissileBombway0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FinalMissileBombway0 { get; } = Find(nameof(FinalMissileBombway0));
    /// <summary>The <c>EnemySets_Parlor1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Parlor1 { get; } = Find(nameof(Parlor1));
    /// <summary>The <c>EnemySets_Flyway01</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Flyway01 { get; } = Find(nameof(Flyway01));
    /// <summary>The <c>EnemySets_Pit1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Pit1 { get; } = Find(nameof(Pit1));
    /// <summary>The <c>EnemySets_GauntletEntrance</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GauntletEntrance { get; } = Find(nameof(GauntletEntrance));
    /// <summary>The <c>EnemySets_BombTorizo01</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BombTorizo01 { get; } = Find(nameof(BombTorizo01));
    /// <summary>The <c>EnemySets_GreenPiratesShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenPiratesShaft { get; } = Find(nameof(GreenPiratesShaft));
    /// <summary>The <c>EnemySets_GreenBrinstarElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarElev { get; } = Find(nameof(GreenBrinstarElev));
    /// <summary>The <c>EnemySets_FinalMissile0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FinalMissile0 { get; } = Find(nameof(FinalMissile0));
    /// <summary>The <c>EnemySets_CrateriaMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaMap { get; } = Find(nameof(CrateriaMap));
    /// <summary>The <c>EnemySets_FinalMissile1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FinalMissile1 { get; } = Find(nameof(FinalMissile1));
    /// <summary>The <c>EnemySets_Climb2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Climb2 { get; } = Find(nameof(Climb2));
    /// <summary>The <c>EnemySets_CrateriaSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaSave { get; } = Find(nameof(CrateriaSave));
    /// <summary>The <c>EnemySets_ForgottenHighwayElbow</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ForgottenHighwayElbow { get; } = Find(nameof(ForgottenHighwayElbow));
    /// <summary>The <c>EnemySets_Moat</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Moat { get; } = Find(nameof(Moat));
    /// <summary>The <c>EnemySets_Climb0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Climb0 { get; } = Find(nameof(Climb0));
    /// <summary>The <c>EnemySets_WestOcean</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestOcean { get; } = Find(nameof(WestOcean));
    /// <summary>The <c>EnemySets_Parlor0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Parlor0 { get; } = Find(nameof(Parlor0));
    /// <summary>The <c>EnemySets_LandingSite012</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LandingSite012 { get; } = Find(nameof(LandingSite012));
    /// <summary>The <c>EnemySets_CrateriaSuper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaSuper { get; } = Find(nameof(CrateriaSuper));
    /// <summary>The <c>EnemySets_PreBowling</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PreBowling { get; } = Find(nameof(PreBowling));
    /// <summary>The <c>EnemySets_Climb1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Climb1 { get; } = Find(nameof(Climb1));
    /// <summary>The <c>EnemySets_BowlingAlleyPath</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BowlingAlleyPath { get; } = Find(nameof(BowlingAlleyPath));
    /// <summary>The <c>EnemySets_RedBinstarElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedBinstarElev { get; } = Find(nameof(RedBinstarElev));
    /// <summary>The <c>EnemySets_GauntletETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GauntletETank { get; } = Find(nameof(GauntletETank));
    /// <summary>The <c>EnemySets_Pit0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Pit0 { get; } = Find(nameof(Pit0));
    /// <summary>The <c>EnemySets_CrabMaze</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrabMaze { get; } = Find(nameof(CrabMaze));
    /// <summary>The <c>EnemySets_CrateriaTube</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaTube { get; } = Find(nameof(CrateriaTube));
    /// <summary>The <c>EnemySets_ForgottenHighwayKagos</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ForgottenHighwayKagos { get; } = Find(nameof(ForgottenHighwayKagos));
    /// <summary>The <c>EnemySets_ElevToBlueBrinstar</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ElevToBlueBrinstar { get; } = Find(nameof(ElevToBlueBrinstar));
    /// <summary>The <c>EnemySets_ForgottenHighwayElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ForgottenHighwayElev { get; } = Find(nameof(ForgottenHighwayElev));
    /// <summary>The <c>EnemySets_PreMapFlyway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PreMapFlyway { get; } = Find(nameof(PreMapFlyway));
    /// <summary>The <c>EnemySets_LowerMushrooms</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LowerMushrooms { get; } = Find(nameof(LowerMushrooms));
    /// <summary>The <c>EnemySets_LandingSite3</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LandingSite3 { get; } = Find(nameof(LandingSite3));
    /// <summary>The <c>EnemySets_Parlor2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Parlor2 { get; } = Find(nameof(Parlor2));
    /// <summary>The <c>EnemySets_BombTorizo2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BombTorizo2 { get; } = Find(nameof(BombTorizo2));
    /// <summary>The <c>EnemySets_Flyway2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Flyway2 { get; } = Find(nameof(Flyway2));
    /// <summary>The <c>EnemySets_CrateriaKihunter</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaKihunter { get; } = Find(nameof(CrateriaKihunter));
    /// <summary>The <c>EnemySets_BrinstarPreMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BrinstarPreMap { get; } = Find(nameof(BrinstarPreMap));
    /// <summary>The <c>EnemySets_GreenBrinstarMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarMap { get; } = Find(nameof(GreenBrinstarMap));
    /// <summary>The <c>EnemySets_GreenBrinstarMissileRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarMissileRefill { get; } = Find(nameof(GreenBrinstarMissileRefill));
    /// <summary>The <c>EnemySets_SporeSpawnKihunters</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SporeSpawnKihunters { get; } = Find(nameof(SporeSpawnKihunters));
    /// <summary>The <c>EnemySets_EtecoonSuper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EtecoonSuper { get; } = Find(nameof(EtecoonSuper));
    /// <summary>The <c>EnemySets_DachoraEnergyRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DachoraEnergyRefill { get; } = Find(nameof(DachoraEnergyRefill));
    /// <summary>The <c>EnemySets_SporeSpawnFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SporeSpawnFarming { get; } = Find(nameof(SporeSpawnFarming));
    /// <summary>The <c>EnemySets_Statues</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Statues { get; } = Find(nameof(Statues));
    /// <summary>The <c>EnemySets_HopperETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition HopperETank { get; } = Find(nameof(HopperETank));
    /// <summary>The <c>EnemySets_BetaPowerBomb</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BetaPowerBomb { get; } = Find(nameof(BetaPowerBomb));
    /// <summary>The <c>EnemySets_WaterwayETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WaterwayETank { get; } = Find(nameof(WaterwayETank));
    /// <summary>The <c>EnemySets_ConstructionZone1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ConstructionZone1 { get; } = Find(nameof(ConstructionZone1));
    /// <summary>The <c>EnemySets_BlueBrinstarETank1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BlueBrinstarETank1 { get; } = Find(nameof(BlueBrinstarETank1));
    /// <summary>The <c>EnemySets_NoobBridge</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition NoobBridge { get; } = Find(nameof(NoobBridge));
    /// <summary>The <c>EnemySets_MorphBall1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MorphBall1 { get; } = Find(nameof(MorphBall1));
    /// <summary>The <c>EnemySets_WarehouseSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseSave { get; } = Find(nameof(WarehouseSave));
    /// <summary>The <c>EnemySets_MorphBall0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MorphBall0 { get; } = Find(nameof(MorphBall0));
    /// <summary>The <c>EnemySets_WarehouseZeela</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseZeela { get; } = Find(nameof(WarehouseZeela));
    /// <summary>The <c>EnemySets_RedTower</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedTower { get; } = Find(nameof(RedTower));
    /// <summary>The <c>EnemySets_BlueBrinstarBoulders</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BlueBrinstarBoulders { get; } = Find(nameof(BlueBrinstarBoulders));
    /// <summary>The <c>EnemySets_BillyMays</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BillyMays { get; } = Find(nameof(BillyMays));
    /// <summary>The <c>EnemySets_BigPinkSaveRoom</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BigPinkSaveRoom { get; } = Find(nameof(BigPinkSaveRoom));
    /// <summary>The <c>EnemySets_BigPink</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BigPink { get; } = Find(nameof(BigPink));
    /// <summary>The <c>EnemySets_EtecoonSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EtecoonSave { get; } = Find(nameof(EtecoonSave));
    /// <summary>The <c>EnemySets_BelowSpazer</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BelowSpazer { get; } = Find(nameof(BelowSpazer));
    /// <summary>The <c>EnemySets_GreenBrinstarSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarSave { get; } = Find(nameof(GreenBrinstarSave));
    /// <summary>The <c>EnemySets_Spazer</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Spazer { get; } = Find(nameof(Spazer));
    /// <summary>The <c>EnemySets_PinkBrinstarPowerBombs</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PinkBrinstarPowerBombs { get; } = Find(nameof(PinkBrinstarPowerBombs));
    /// <summary>The <c>EnemySets_RedTowerRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedTowerRefill { get; } = Find(nameof(RedTowerRefill));
    /// <summary>The <c>EnemySets_KraidRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition KraidRefill { get; } = Find(nameof(KraidRefill));
    /// <summary>The <c>EnemySets_VariaSuit</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition VariaSuit { get; } = Find(nameof(VariaSuit));
    /// <summary>The <c>EnemySets_RedBrinstarSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedBrinstarSave { get; } = Find(nameof(RedBrinstarSave));
    /// <summary>The <c>EnemySets_BlueBrinstarETank0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BlueBrinstarETank0 { get; } = Find(nameof(BlueBrinstarETank0));
    /// <summary>The <c>EnemySets_GreenBrinstarFirefleas</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarFirefleas { get; } = Find(nameof(GreenBrinstarFirefleas));
    /// <summary>The <c>EnemySets_GreenBrinstarBeetoms</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarBeetoms { get; } = Find(nameof(GreenBrinstarBeetoms));
    /// <summary>The <c>EnemySets_EtecoonETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EtecoonETank { get; } = Find(nameof(EtecoonETank));
    /// <summary>The <c>EnemySets_PinkBrinstarHoppers</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PinkBrinstarHoppers { get; } = Find(nameof(PinkBrinstarHoppers));
    /// <summary>The <c>EnemySets_WarehouseETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseETank { get; } = Find(nameof(WarehouseETank));
    /// <summary>The <c>EnemySets_WarehouseEntrance</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseEntrance { get; } = Find(nameof(WarehouseEntrance));
    /// <summary>The <c>EnemySets_WarehouseKihunter</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseKihunter { get; } = Find(nameof(WarehouseKihunter));
    /// <summary>The <c>EnemySets_GreenBrinstarMainShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarMainShaft { get; } = Find(nameof(GreenBrinstarMainShaft));
    /// <summary>The <c>EnemySets_SporeSpawnSuper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SporeSpawnSuper { get; } = Find(nameof(SporeSpawnSuper));
    /// <summary>The <c>EnemySets_EarlySupers</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EarlySupers { get; } = Find(nameof(EarlySupers));
    /// <summary>The <c>EnemySets_RedBrinstarFirefleas</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedBrinstarFirefleas { get; } = Find(nameof(RedBrinstarFirefleas));
    /// <summary>The <c>EnemySets_ConstructionZone0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ConstructionZone0 { get; } = Find(nameof(ConstructionZone0));
    /// <summary>The <c>EnemySets_GreenHillZone</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenHillZone { get; } = Find(nameof(GreenHillZone));
    /// <summary>The <c>EnemySets_Dachora</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Dachora { get; } = Find(nameof(Dachora));
    /// <summary>The <c>EnemySets_Hellway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Hellway { get; } = Find(nameof(Hellway));
    /// <summary>The <c>EnemySets_FirstMissile</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FirstMissile { get; } = Find(nameof(FirstMissile));
    /// <summary>The <c>EnemySets_Kraid</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Kraid { get; } = Find(nameof(Kraid));
    /// <summary>The <c>EnemySets_BrinstarReserveTank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BrinstarReserveTank { get; } = Find(nameof(BrinstarReserveTank));
    /// <summary>The <c>EnemySets_XrayScope</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition XrayScope { get; } = Find(nameof(XrayScope));
    /// <summary>The <c>EnemySets_StatuesHallway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition StatuesHallway { get; } = Find(nameof(StatuesHallway));
    /// <summary>The <c>EnemySets_AlphaPowerBomb</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AlphaPowerBomb { get; } = Find(nameof(AlphaPowerBomb));
    /// <summary>The <c>EnemySets_KraidEyeDoor</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition KraidEyeDoor { get; } = Find(nameof(KraidEyeDoor));
    /// <summary>The <c>EnemySets_Caterpillar</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Caterpillar { get; } = Find(nameof(Caterpillar));
    /// <summary>The <c>EnemySets_MiniKraid</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MiniKraid { get; } = Find(nameof(MiniKraid));
    /// <summary>The <c>EnemySets_SporeSpawn</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SporeSpawn { get; } = Find(nameof(SporeSpawn));
    /// <summary>The <c>EnemySets_SkreeBoost</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SkreeBoost { get; } = Find(nameof(SkreeBoost));
    /// <summary>The <c>EnemySets_GrappleTutorial3</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GrappleTutorial3 { get; } = Find(nameof(GrappleTutorial3));
    /// <summary>The <c>EnemySets_PostCrocShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocShaft { get; } = Find(nameof(PostCrocShaft));
    /// <summary>The <c>EnemySets_MetalPirates</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MetalPirates { get; } = Find(nameof(MetalPirates));
    /// <summary>The <c>EnemySets_AcidStatue</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AcidStatue { get; } = Find(nameof(AcidStatue));
    /// <summary>The <c>EnemySets_PurpleFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PurpleFarming { get; } = Find(nameof(PurpleFarming));
    /// <summary>The <c>EnemySets_IceBeamGate</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeamGate { get; } = Find(nameof(IceBeamGate));
    /// <summary>The <c>EnemySets_SpikyAcidSnakesTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpikyAcidSnakesTunnel { get; } = Find(nameof(SpikyAcidSnakesTunnel));
    /// <summary>The <c>EnemySets_RedKihunterShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedKihunterShaft { get; } = Find(nameof(RedKihunterShaft));
    /// <summary>The <c>EnemySets_FastRipper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FastRipper { get; } = Find(nameof(FastRipper));
    /// <summary>The <c>EnemySets_WaveBeam</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WaveBeam { get; } = Find(nameof(WaveBeam));
    /// <summary>The <c>EnemySets_FastPillarsSetup</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FastPillarsSetup { get; } = Find(nameof(FastPillarsSetup));
    /// <summary>The <c>EnemySets_LowerNorfairElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LowerNorfairElev { get; } = Find(nameof(LowerNorfairElev));
    /// <summary>The <c>EnemySets_LNSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNSave { get; } = Find(nameof(LNSave));
    /// <summary>The <c>EnemySets_UpperNorfairRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition UpperNorfairRefill { get; } = Find(nameof(UpperNorfairRefill));
    /// <summary>The <c>EnemySets_NorfairMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition NorfairMap { get; } = Find(nameof(NorfairMap));
    /// <summary>The <c>EnemySets_MickeyMouse</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MickeyMouse { get; } = Find(nameof(MickeyMouse));
    /// <summary>The <c>EnemySets_GTEnergyRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GTEnergyRefill { get; } = Find(nameof(GTEnergyRefill));
    /// <summary>The <c>EnemySets_Ridley</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Ridley { get; } = Find(nameof(Ridley));
    /// <summary>The <c>EnemySets_PostCrocPowerBombs</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocPowerBombs { get; } = Find(nameof(PostCrocPowerBombs));
    /// <summary>The <c>EnemySets_IceBeam</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeam { get; } = Find(nameof(IceBeam));
    /// <summary>The <c>EnemySets_HiJumpBoots</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition HiJumpBoots { get; } = Find(nameof(HiJumpBoots));
    /// <summary>The <c>EnemySets_PostCrocSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocSave { get; } = Find(nameof(PostCrocSave));
    /// <summary>The <c>EnemySets_FrogSpeedway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FrogSpeedway { get; } = Find(nameof(FrogSpeedway));
    /// <summary>The <c>EnemySets_RisingTide</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RisingTide { get; } = Find(nameof(RisingTide));
    /// <summary>The <c>EnemySets_PostCrocFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocFarming { get; } = Find(nameof(PostCrocFarming));
    /// <summary>The <c>EnemySets_WorstRoomInTheGame</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WorstRoomInTheGame { get; } = Find(nameof(WorstRoomInTheGame));
    /// <summary>The <c>EnemySets_DoubleChamber</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DoubleChamber { get; } = Find(nameof(DoubleChamber));
    /// <summary>The <c>EnemySets_GrappleBeam</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GrappleBeam { get; } = Find(nameof(GrappleBeam));
    /// <summary>The <c>EnemySets_RedPirateShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedPirateShaft { get; } = Find(nameof(RedPirateShaft));
    /// <summary>The <c>EnemySets_CrocomireSpeedway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrocomireSpeedway { get; } = Find(nameof(CrocomireSpeedway));
    /// <summary>The <c>EnemySets_Plowerhouse</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Plowerhouse { get; } = Find(nameof(Plowerhouse));
    /// <summary>The <c>EnemySets_LNFireflea</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNFireflea { get; } = Find(nameof(LNFireflea));
    /// <summary>The <c>EnemySets_MagdolliteTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MagdolliteTunnel { get; } = Find(nameof(MagdolliteTunnel));
    /// <summary>The <c>EnemySets_UnusedState8FB3EEB48899</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition UnusedState8FB3EEB48899 { get; } = Find(nameof(UnusedState8FB3EEB48899));
    /// <summary>The <c>EnemySets_LavaDive</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LavaDive { get; } = Find(nameof(LavaDive));
    /// <summary>The <c>EnemySets_Pillar</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Pillar { get; } = Find(nameof(Pillar));
    /// <summary>The <c>EnemySets_CrocomireEscape</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrocomireEscape { get; } = Find(nameof(CrocomireEscape));
    /// <summary>The <c>EnemySets_LNSpringBallMaze</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNSpringBallMaze { get; } = Find(nameof(LNSpringBallMaze));
    /// <summary>The <c>EnemySets_LNEscapePowerBombs</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNEscapePowerBombs { get; } = Find(nameof(LNEscapePowerBombs));
    /// <summary>The <c>EnemySets_PurpleShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PurpleShaft { get; } = Find(nameof(PurpleShaft));
    /// <summary>The <c>EnemySets_RidleyETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RidleyETank { get; } = Find(nameof(RidleyETank));
    /// <summary>The <c>EnemySets_ScrewAttack</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ScrewAttack { get; } = Find(nameof(ScrewAttack));
    /// <summary>The <c>EnemySets_GreenBubblesMissiles</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBubblesMissiles { get; } = Find(nameof(GreenBubblesMissiles));
    /// <summary>The <c>EnemySets_Amphitheatre</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Amphitheatre { get; } = Find(nameof(Amphitheatre));
    /// <summary>The <c>EnemySets_CathedralEntrance</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CathedralEntrance { get; } = Find(nameof(CathedralEntrance));
    /// <summary>The <c>EnemySets_MainHall</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MainHall { get; } = Find(nameof(MainHall));
    /// <summary>The <c>EnemySets_PostCrocJump</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocJump { get; } = Find(nameof(PostCrocJump));
    /// <summary>The <c>EnemySets_FrogSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FrogSave { get; } = Find(nameof(FrogSave));
    /// <summary>The <c>EnemySets_CrocomireSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrocomireSave { get; } = Find(nameof(CrocomireSave));
    /// <summary>The <c>EnemySets_CrumbleShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrumbleShaft { get; } = Find(nameof(CrumbleShaft));
    /// <summary>The <c>EnemySets_Wasteland</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Wasteland { get; } = Find(nameof(Wasteland));
    /// <summary>The <c>EnemySets_AcidSnakesTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AcidSnakesTunnel { get; } = Find(nameof(AcidSnakesTunnel));
    /// <summary>The <c>EnemySets_GrappleTutorial1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GrappleTutorial1 { get; } = Find(nameof(GrappleTutorial1));
    /// <summary>The <c>EnemySets_BubbleMountainSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BubbleMountainSave { get; } = Find(nameof(BubbleMountainSave));
    /// <summary>The <c>EnemySets_HiJumpETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition HiJumpETank { get; } = Find(nameof(HiJumpETank));
    /// <summary>The <c>EnemySets_BubbleMountain</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BubbleMountain { get; } = Find(nameof(BubbleMountain));
    /// <summary>The <c>EnemySets_IceBeamAcid</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeamAcid { get; } = Find(nameof(IceBeamAcid));
    /// <summary>The <c>EnemySets_IceBeamTutorial</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeamTutorial { get; } = Find(nameof(IceBeamTutorial));
    /// <summary>The <c>EnemySets_PostCrocMissile</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocMissile { get; } = Find(nameof(PostCrocMissile));
    /// <summary>The <c>EnemySets_Cathedral</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Cathedral { get; } = Find(nameof(Cathedral));
    /// <summary>The <c>EnemySets_NorfairReserveTank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition NorfairReserveTank { get; } = Find(nameof(NorfairReserveTank));
    /// <summary>The <c>EnemySets_GrappleTutorial2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GrappleTutorial2 { get; } = Find(nameof(GrappleTutorial2));
    /// <summary>The <c>EnemySets_IceBeamSnake</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeamSnake { get; } = Find(nameof(IceBeamSnake));
    /// <summary>The <c>EnemySets_GoldenTorizo</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GoldenTorizo { get; } = Find(nameof(GoldenTorizo));
    /// <summary>The <c>EnemySets_SpikyPlatformsTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpikyPlatformsTunnel { get; } = Find(nameof(SpikyPlatformsTunnel));
    /// <summary>The <c>EnemySets_SpeedBooster</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpeedBooster { get; } = Find(nameof(SpeedBooster));
    /// <summary>The <c>EnemySets_LNFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNFarming { get; } = Find(nameof(LNFarming));
    /// <summary>The <c>EnemySets_SingleChamber</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SingleChamber { get; } = Find(nameof(SingleChamber));
    /// <summary>The <c>EnemySets_SpeedBoosterHall</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpeedBoosterHall { get; } = Find(nameof(SpeedBoosterHall));
    /// <summary>The <c>EnemySets_BatCave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BatCave { get; } = Find(nameof(BatCave));
    /// <summary>The <c>EnemySets_KronicBoost</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition KronicBoost { get; } = Find(nameof(KronicBoost));
    /// <summary>The <c>EnemySets_BusinessCenter</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BusinessCenter { get; } = Find(nameof(BusinessCenter));
    /// <summary>The <c>EnemySets_ThreeMusketeers</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ThreeMusketeers { get; } = Find(nameof(ThreeMusketeers));
    /// <summary>The <c>EnemySets_Crocomire</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Crocomire { get; } = Find(nameof(Crocomire));
    /// <summary>The <c>EnemySets_LNElevSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNElevSave { get; } = Find(nameof(LNElevSave));
    /// <summary>The <c>EnemySets_Volcano</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Volcano { get; } = Find(nameof(Volcano));
    /// <summary>The <c>EnemySets_UpperNorfairFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition UpperNorfairFarming { get; } = Find(nameof(UpperNorfairFarming));
    /// <summary>The <c>EnemySets_SpongeBath1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpongeBath1 { get; } = Find(nameof(SpongeBath1));
    /// <summary>The <c>EnemySets_WreckedShipETank1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipETank1 { get; } = Find(nameof(WreckedShipETank1));
    /// <summary>The <c>EnemySets_WreckedShipMainShaft0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipMainShaft0 { get; } = Find(nameof(WreckedShipMainShaft0));
    /// <summary>The <c>EnemySets_BowlingAlley0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BowlingAlley0 { get; } = Find(nameof(BowlingAlley0));
    /// <summary>The <c>EnemySets_Attic1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Attic1 { get; } = Find(nameof(Attic1));
    /// <summary>The <c>EnemySets_Basement1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Basement1 { get; } = Find(nameof(Basement1));
    /// <summary>The <c>EnemySets_WreckedShipMap1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipMap1 { get; } = Find(nameof(WreckedShipMap1));
    /// <summary>The <c>EnemySets_WSWestSuper0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WSWestSuper0 { get; } = Find(nameof(WSWestSuper0));
    /// <summary>The <c>EnemySets_SpikyDeath0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpikyDeath0 { get; } = Find(nameof(SpikyDeath0));
    /// <summary>The <c>EnemySets_SpongeBath0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpongeBath0 { get; } = Find(nameof(SpongeBath0));
    /// <summary>The <c>EnemySets_WreckedShipEntrance1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipEntrance1 { get; } = Find(nameof(WreckedShipEntrance1));
    /// <summary>The <c>EnemySets_ElectricDeath0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ElectricDeath0 { get; } = Find(nameof(ElectricDeath0));
    /// <summary>The <c>EnemySets_BowlingAlley1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BowlingAlley1 { get; } = Find(nameof(BowlingAlley1));
    /// <summary>The <c>EnemySets_AssemblyLine0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AssemblyLine0 { get; } = Find(nameof(AssemblyLine0));
    /// <summary>The <c>EnemySets_Phantoon1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Phantoon1 { get; } = Find(nameof(Phantoon1));
    /// <summary>The <c>EnemySets_WreckedShipETank0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipETank0 { get; } = Find(nameof(WreckedShipETank0));
    /// <summary>The <c>EnemySets_GravitySuit0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GravitySuit0 { get; } = Find(nameof(GravitySuit0));
    /// <summary>The <c>EnemySets_WreckedShipMap0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipMap0 { get; } = Find(nameof(WreckedShipMap0));
    /// <summary>The <c>EnemySets_GravitySuit1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GravitySuit1 { get; } = Find(nameof(GravitySuit1));
    /// <summary>The <c>EnemySets_Basement0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Basement0 { get; } = Find(nameof(Basement0));
    /// <summary>The <c>EnemySets_WreckedShipEntrance0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipEntrance0 { get; } = Find(nameof(WreckedShipEntrance0));
    /// <summary>The <c>EnemySets_AssemblyLine1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AssemblyLine1 { get; } = Find(nameof(AssemblyLine1));
    /// <summary>The <c>EnemySets_WreckedShipSave1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipSave1 { get; } = Find(nameof(WreckedShipSave1));
    /// <summary>The <c>EnemySets_WSWestSuper1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WSWestSuper1 { get; } = Find(nameof(WSWestSuper1));
    /// <summary>The <c>EnemySets_Attic0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Attic0 { get; } = Find(nameof(Attic0));
    /// <summary>The <c>EnemySets_WSEastSuper0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WSEastSuper0 { get; } = Find(nameof(WSEastSuper0));
    /// <summary>The <c>EnemySets_WreckedShipSave0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipSave0 { get; } = Find(nameof(WreckedShipSave0));
    /// <summary>The <c>EnemySets_ElectricDeath1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ElectricDeath1 { get; } = Find(nameof(ElectricDeath1));
    /// <summary>The <c>EnemySets_SpikyDeath1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpikyDeath1 { get; } = Find(nameof(SpikyDeath1));
    /// <summary>The <c>EnemySets_WSEastSuper1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WSEastSuper1 { get; } = Find(nameof(WSEastSuper1));
    /// <summary>The <c>EnemySets_Phantoon0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Phantoon0 { get; } = Find(nameof(Phantoon0));
    /// <summary>The <c>EnemySets_WreckedShipMainShaft1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipMainShaft1 { get; } = Find(nameof(WreckedShipMainShaft1));
    /// <summary>The <c>EnemySets_MtEverest</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MtEverest { get; } = Find(nameof(MtEverest));
    /// <summary>The <c>EnemySets_HalfieClimb</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition HalfieClimb { get; } = Find(nameof(HalfieClimb));
    /// <summary>The <c>EnemySets_WateringHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WateringHole { get; } = Find(nameof(WateringHole));
    /// <summary>The <c>EnemySets_CrabShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrabShaft { get; } = Find(nameof(CrabShaft));
    /// <summary>The <c>EnemySets_MaridiaElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MaridiaElev { get; } = Find(nameof(MaridiaElev));
    /// <summary>The <c>EnemySets_Colosseum</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Colosseum { get; } = Find(nameof(Colosseum));
    /// <summary>The <c>EnemySets_AqueductSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AqueductSave { get; } = Find(nameof(AqueductSave));
    /// <summary>The <c>EnemySets_ThePrecious</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ThePrecious { get; } = Find(nameof(ThePrecious));
    /// <summary>The <c>EnemySets_BotwoonETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BotwoonETank { get; } = Find(nameof(BotwoonETank));
    /// <summary>The <c>EnemySets_DraygonSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DraygonSave { get; } = Find(nameof(DraygonSave));
    /// <summary>The <c>EnemySets_MaridiaMissileRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MaridiaMissileRefill { get; } = Find(nameof(MaridiaMissileRefill));
    /// <summary>The <c>EnemySets_PlasmaBeachQuicksand</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PlasmaBeachQuicksand { get; } = Find(nameof(PlasmaBeachQuicksand));
    /// <summary>The <c>EnemySets_BotwoonQuicksand</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BotwoonQuicksand { get; } = Find(nameof(BotwoonQuicksand));
    /// <summary>The <c>EnemySets_Shaktool</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Shaktool { get; } = Find(nameof(Shaktool));
    /// <summary>The <c>EnemySets_Draygon0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Draygon0 { get; } = Find(nameof(Draygon0));
    /// <summary>The <c>EnemySets_Aqueduct</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Aqueduct { get; } = Find(nameof(Aqueduct));
    /// <summary>The <c>EnemySets_RedFish</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedFish { get; } = Find(nameof(RedFish));
    /// <summary>The <c>EnemySets_Plasma</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Plasma { get; } = Find(nameof(Plasma));
    /// <summary>The <c>EnemySets_SpaceJump</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpaceJump { get; } = Find(nameof(SpaceJump));
    /// <summary>The <c>EnemySets_Pants</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Pants { get; } = Find(nameof(Pants));
    /// <summary>The <c>EnemySets_GlassTunnelSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GlassTunnelSave { get; } = Find(nameof(GlassTunnelSave));
    /// <summary>The <c>EnemySets_GlassTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GlassTunnel { get; } = Find(nameof(GlassTunnel));
    /// <summary>The <c>EnemySets_WestTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestTunnel { get; } = Find(nameof(WestTunnel));
    /// <summary>The <c>EnemySets_EastTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastTunnel { get; } = Find(nameof(EastTunnel));
    /// <summary>The <c>EnemySets_MamaTurtle</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MamaTurtle { get; } = Find(nameof(MamaTurtle));
    /// <summary>The <c>EnemySets_CrabTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrabTunnel { get; } = Find(nameof(CrabTunnel));
    /// <summary>The <c>EnemySets_NWestMaridiaBug</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition NWestMaridiaBug { get; } = Find(nameof(NWestMaridiaBug));
    /// <summary>The <c>EnemySets_PseudoPlasmaSpark</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PseudoPlasmaSpark { get; } = Find(nameof(PseudoPlasmaSpark));
    /// <summary>The <c>EnemySets_WestSandHallTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestSandHallTunnel { get; } = Find(nameof(WestSandHallTunnel));
    /// <summary>The <c>EnemySets_PlasmaTutorial</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PlasmaTutorial { get; } = Find(nameof(PlasmaTutorial));
    /// <summary>The <c>EnemySets_ThreadTheNeedle</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ThreadTheNeedle { get; } = Find(nameof(ThreadTheNeedle));
    /// <summary>The <c>EnemySets_PlasmaSpark</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PlasmaSpark { get; } = Find(nameof(PlasmaSpark));
    /// <summary>The <c>EnemySets_Toilet</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Toilet { get; } = Find(nameof(Toilet));
    /// <summary>The <c>EnemySets_WestSandHall</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestSandHall { get; } = Find(nameof(WestSandHall));
    /// <summary>The <c>EnemySets_Oasis</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Oasis { get; } = Find(nameof(Oasis));
    /// <summary>The <c>EnemySets_EastSandHall</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastSandHall { get; } = Find(nameof(EastSandHall));
    /// <summary>The <c>EnemySets_WestAqueductQuicksand</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestAqueductQuicksand { get; } = Find(nameof(WestAqueductQuicksand));
    /// <summary>The <c>EnemySets_EastAqueductQuicksand</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastAqueductQuicksand { get; } = Find(nameof(EastAqueductQuicksand));
    /// <summary>The <c>EnemySets_Butterfly</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Butterfly { get; } = Find(nameof(Butterfly));
    /// <summary>The <c>EnemySets_BotwoonHallway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BotwoonHallway { get; } = Find(nameof(BotwoonHallway));
    /// <summary>The <c>EnemySets_EastPants</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastPants { get; } = Find(nameof(EastPants));
    /// <summary>The <c>EnemySets_Springball</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Springball { get; } = Find(nameof(Springball));
    /// <summary>The <c>EnemySets_BelowBotwoonETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BelowBotwoonETank { get; } = Find(nameof(BelowBotwoonETank));
    /// <summary>The <c>EnemySets_Kassiuz</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Kassiuz { get; } = Find(nameof(Kassiuz));
    /// <summary>The <c>EnemySets_ForgottenHighwaySave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ForgottenHighwaySave { get; } = Find(nameof(ForgottenHighwaySave));
    /// <summary>The <c>EnemySets_MaridiaEnergyRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MaridiaEnergyRefill { get; } = Find(nameof(MaridiaEnergyRefill));
    /// <summary>The <c>EnemySets_MaridiaMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MaridiaMap { get; } = Find(nameof(MaridiaMap));
    /// <summary>The <c>EnemySets_CrabHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrabHole { get; } = Find(nameof(CrabHole));
    /// <summary>The <c>EnemySets_Botwoon</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Botwoon { get; } = Find(nameof(Botwoon));
    /// <summary>The <c>EnemySets_MainStreet</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MainStreet { get; } = Find(nameof(MainStreet));
    /// <summary>The <c>EnemySets_BugSandHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BugSandHole { get; } = Find(nameof(BugSandHole));
    /// <summary>The <c>EnemySets_EastSandHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastSandHole { get; } = Find(nameof(EastSandHole));
    /// <summary>The <c>EnemySets_WestSandHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestSandHole { get; } = Find(nameof(WestSandHole));
    /// <summary>The <c>EnemySets_WestCactusAlley</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestCactusAlley { get; } = Find(nameof(WestCactusAlley));
    /// <summary>The <c>EnemySets_EastCactusAlley</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastCactusAlley { get; } = Find(nameof(EastCactusAlley));
    /// <summary>The <c>EnemySets_FishTank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FishTank { get; } = Find(nameof(FishTank));
    /// <summary>The <c>EnemySets_TourianEscape4</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEscape4 { get; } = Find(nameof(TourianEscape4));
    /// <summary>The <c>EnemySets_MotherBrainSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MotherBrainSave { get; } = Find(nameof(MotherBrainSave));
    /// <summary>The <c>EnemySets_Metroids10</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids10 { get; } = Find(nameof(Metroids10));
    /// <summary>The <c>EnemySets_DustTorizo</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DustTorizo { get; } = Find(nameof(DustTorizo));
    /// <summary>The <c>EnemySets_BigBoy</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BigBoy { get; } = Find(nameof(BigBoy));
    /// <summary>The <c>EnemySets_MotherBrain01</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MotherBrain01 { get; } = Find(nameof(MotherBrain01));
    /// <summary>The <c>EnemySets_TourianEyeDoor</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEyeDoor { get; } = Find(nameof(TourianEyeDoor));
    /// <summary>The <c>EnemySets_BlueHopper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BlueHopper { get; } = Find(nameof(BlueHopper));
    /// <summary>The <c>EnemySets_TourianEscape1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEscape1 { get; } = Find(nameof(TourianEscape1));
    /// <summary>The <c>EnemySets_TourianFirst</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianFirst { get; } = Find(nameof(TourianFirst));
    /// <summary>The <c>EnemySets_Metroids20</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids20 { get; } = Find(nameof(Metroids20));
    /// <summary>The <c>EnemySets_Metroids40</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids40 { get; } = Find(nameof(Metroids40));
    /// <summary>The <c>EnemySets_Metroids11</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids11 { get; } = Find(nameof(Metroids11));
    /// <summary>The <c>EnemySets_Metroids21</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids21 { get; } = Find(nameof(Metroids21));
    /// <summary>The <c>EnemySets_TourianEscape2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEscape2 { get; } = Find(nameof(TourianEscape2));
    /// <summary>The <c>EnemySets_Metroids31</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids31 { get; } = Find(nameof(Metroids31));
    /// <summary>The <c>EnemySets_Metroids41</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids41 { get; } = Find(nameof(Metroids41));
    /// <summary>The <c>EnemySets_TourianEscape3</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEscape3 { get; } = Find(nameof(TourianEscape3));
    /// <summary>The <c>EnemySets_Seaweed</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Seaweed { get; } = Find(nameof(Seaweed));
    /// <summary>The <c>EnemySets_TourianRecharge</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianRecharge { get; } = Find(nameof(TourianRecharge));
    /// <summary>The <c>EnemySets_UpperTourianSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition UpperTourianSave { get; } = Find(nameof(UpperTourianSave));
    /// <summary>The <c>EnemySets_Metroids30</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids30 { get; } = Find(nameof(Metroids30));
    /// <summary>The <c>EnemySets_RinkaShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RinkaShaft { get; } = Find(nameof(RinkaShaft));
    /// <summary>The <c>EnemySets_CeresElev0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CeresElev0 { get; } = Find(nameof(CeresElev0));
    /// <summary>The <c>EnemySets_FallingTile0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FallingTile0 { get; } = Find(nameof(FallingTile0));
    /// <summary>The <c>EnemySets_MagnetStairs0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MagnetStairs0 { get; } = Find(nameof(MagnetStairs0));
    /// <summary>The <c>EnemySets_DeadScientist0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DeadScientist0 { get; } = Find(nameof(DeadScientist0));
    /// <summary>The <c>EnemySets_Room58Escape0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Room58Escape0 { get; } = Find(nameof(Room58Escape0));
    /// <summary>The <c>EnemySets_CeresRidley0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CeresRidley0 { get; } = Find(nameof(CeresRidley0));
    /// <summary>The <c>EnemySets_CeresElev1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CeresElev1 { get; } = Find(nameof(CeresElev1));
    /// <summary>The <c>EnemySets_FallingTile1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FallingTile1 { get; } = Find(nameof(FallingTile1));
    /// <summary>The <c>EnemySets_MagnetStairs1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MagnetStairs1 { get; } = Find(nameof(MagnetStairs1));
    /// <summary>The <c>EnemySets_DeadScientist1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DeadScientist1 { get; } = Find(nameof(DeadScientist1));
    /// <summary>The <c>EnemySets_Room58Escape1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Room58Escape1 { get; } = Find(nameof(Room58Escape1));
    /// <summary>The <c>EnemySets_CeresRidley1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CeresRidley1 { get; } = Find(nameof(CeresRidley1));

    /// <summary>First pointer-ordered portion of the compiled bank-$B4 graphics-set catalog.</summary>
    private static RoomEnemyGraphicsSetDefinition[] BuildSegment0() =>
    [
        new(nameof(Draygon1MotherBrain2), [
        ]),
        new(nameof(EastOcean), [
            new(EnemyDefinitionId.Choot, PaletteRow: 1),
            new(EnemyDefinitionId.Skultera, PaletteRow: 2),
            new(EnemyDefinitionId.Kamer2, PaletteRow: 3),
        ]),
        new(nameof(CrateriaPowerBombs), [
            new(EnemyDefinitionId.Alcoon, PaletteRow: 1),
            new(EnemyDefinitionId.Zero, PaletteRow: 1),
        ]),
        new(nameof(Terminator), [
            new(EnemyDefinitionId.Waver, PaletteRow: 1),
            new(EnemyDefinitionId.Zoomer, PaletteRow: 2),
        ]),
        new(nameof(FinalMissileBombway1), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
            new(EnemyDefinitionId.Stoke, PaletteRow: 7),
        ]),
        new(nameof(FinalMissileBombway0), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
            new(EnemyDefinitionId.Stoke, PaletteRow: 7),
        ]),
        new(nameof(Parlor1), [
            new(EnemyDefinitionId.Zoomer, PaletteRow: 1),
            new(EnemyDefinitionId.Skree, PaletteRow: 2),
            new(EnemyDefinitionId.Ripper, PaletteRow: 3),
        ]),
        new(nameof(Flyway01), [
            new(EnemyDefinitionId.Waver, PaletteRow: 1),
            new(EnemyDefinitionId.Mellow, PaletteRow: 2),
        ]),
        new(nameof(Pit1), [
            new(EnemyDefinitionId.PirateGreyWalking, PaletteRow: 1),
            new(EnemyDefinitionId.PirateGreyWall, PaletteRow: 2),
        ]),
        new(nameof(GauntletEntrance), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.Waver, PaletteRow: 2),
        ]),
        new(nameof(BombTorizo01), [
            new(EnemyDefinitionId.BombTorizo, PaletteRow: 1),
        ]),
        new(nameof(GreenPiratesShaft), [
            new(EnemyDefinitionId.PirateGreenWalking, PaletteRow: 1),
            new(EnemyDefinitionId.Beetom, PaletteRow: 2),
        ]),
        new(nameof(GreenBrinstarElev), [
        ]),
        new(nameof(FinalMissile0), [
            new(EnemyDefinitionId.Eye, PaletteRow: 1),
        ]),
        new(nameof(CrateriaMap), [
            new(EnemyDefinitionId.BombTorizo, PaletteRow: 1),
        ]),
        new(nameof(FinalMissile1), [
        ]),
        new(nameof(Climb2), [
            new(EnemyDefinitionId.PirateSilverNinja, PaletteRow: 2),
        ]),
        new(nameof(CrateriaSave), [
        ]),
        new(nameof(ForgottenHighwayElbow), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 1),
        ]),
        new(nameof(Moat), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 1),
        ]),
        new(nameof(Climb0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(WestOcean), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.Zeb, PaletteRow: 2),
            new(EnemyDefinitionId.GRipper, PaletteRow: 3),
            new(EnemyDefinitionId.Tripper, PaletteRow: 7),
        ]),
        new(nameof(Parlor0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
        ]),
        new(nameof(LandingSite012), [
            new(EnemyDefinitionId.ShipTop, PaletteRow: 2),
            new(EnemyDefinitionId.ShipBottomEntrance, PaletteRow: 7),
        ]),
        new(nameof(CrateriaSuper), [
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 2),
        ]),
        new(nameof(PreBowling), [
            new(EnemyDefinitionId.HZoomer, PaletteRow: 1),
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
        ]),
        new(nameof(Climb1), [
            new(EnemyDefinitionId.PirateGreyWall, PaletteRow: 1),
        ]),
        new(nameof(BowlingAlleyPath), [
            new(EnemyDefinitionId.Waver, PaletteRow: 1),
            new(EnemyDefinitionId.Choot, PaletteRow: 7),
        ]),
        new(nameof(RedBinstarElev), [
        ]),
        new(nameof(GauntletETank), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 2),
        ]),
        new(nameof(Pit0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(CrabMaze), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 7),
            new(EnemyDefinitionId.Puyo, PaletteRow: 7),
        ]),
        new(nameof(CrateriaTube), [
        ]),
        new(nameof(ForgottenHighwayKagos), [
            new(EnemyDefinitionId.Kago, PaletteRow: 1),
        ]),
        new(nameof(ElevToBlueBrinstar), [
        ]),
        new(nameof(ForgottenHighwayElev), [
        ]),
        new(nameof(PreMapFlyway), [
            new(EnemyDefinitionId.Mellow, PaletteRow: 1),
            new(EnemyDefinitionId.Rio, PaletteRow: 2),
        ]),
        new(nameof(LowerMushrooms), [
            new(EnemyDefinitionId.Kago, PaletteRow: 2),
            new(EnemyDefinitionId.MZoomer, PaletteRow: 3),
        ]),
        new(nameof(LandingSite3), [
            new(EnemyDefinitionId.ShipTop, PaletteRow: 2),
            new(EnemyDefinitionId.ShipBottomEntrance, PaletteRow: 7),
        ]),
        new(nameof(Parlor2), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
        ]),
        new(nameof(BombTorizo2), [
            new(EnemyDefinitionId.EtecoonEscape, PaletteRow: 1),
            new(EnemyDefinitionId.DachoraEscape, PaletteRow: 7),
        ]),
        new(nameof(Flyway2), [
        ]),
        new(nameof(CrateriaKihunter), [
            new(EnemyDefinitionId.KihunterGreen, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 2),
            new(EnemyDefinitionId.Zero, PaletteRow: 2),
        ]),
        new(nameof(BrinstarPreMap), [
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 1),
            new(EnemyDefinitionId.Zeela, PaletteRow: 2),
        ]),
        new(nameof(GreenBrinstarMap), [
        ]),
        new(nameof(GreenBrinstarMissileRefill), [
        ]),
        new(nameof(SporeSpawnKihunters), [
            new(EnemyDefinitionId.KihunterGreen, PaletteRow: 7),
        ]),
        new(nameof(EtecoonSuper), [
        ]),
        new(nameof(DachoraEnergyRefill), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 1),
        ]),
        new(nameof(SporeSpawnFarming), [
            new(EnemyDefinitionId.Zeb, PaletteRow: 1),
            new(EnemyDefinitionId.Zeela, PaletteRow: 2),
        ]),
        new(nameof(Statues), [
            new(EnemyDefinitionId.TourianStatue, PaletteRow: 1),
            new(EnemyDefinitionId.TourianStatueGhost, PaletteRow: 3, StagingBlock: 1),
        ]),
        new(nameof(HopperETank), [
        ]),
        new(nameof(BetaPowerBomb), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.SidehopperLarge, PaletteRow: 2),
        ]),
        new(nameof(WaterwayETank), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Zero, PaletteRow: 3),
        ]),
        new(nameof(ConstructionZone1), [
            new(EnemyDefinitionId.Zoomer, PaletteRow: 1),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 2),
        ]),
        new(nameof(BlueBrinstarETank1), [
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 1),
            new(EnemyDefinitionId.Zoomer, PaletteRow: 2),
            new(EnemyDefinitionId.Skree, PaletteRow: 3),
            new(EnemyDefinitionId.Rio, PaletteRow: 7),
        ]),
        new(nameof(NoobBridge), [
            new(EnemyDefinitionId.Cacatac, PaletteRow: 1),
            new(EnemyDefinitionId.Zeela, PaletteRow: 2),
        ]),
        new(nameof(MorphBall1), [
            new(EnemyDefinitionId.SidehopperLarge, PaletteRow: 2),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(WarehouseSave), [
        ]),
        new(nameof(MorphBall0), [
            new(EnemyDefinitionId.Eye, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(WarehouseZeela), [
            new(EnemyDefinitionId.Zeela, PaletteRow: 1),
        ]),
        new(nameof(RedTower), [
            new(EnemyDefinitionId.Ripper, PaletteRow: 1),
            new(EnemyDefinitionId.Beetom, PaletteRow: 2),
            new(EnemyDefinitionId.Geega, PaletteRow: 3),
        ]),
        new(nameof(BlueBrinstarBoulders), [
            new(EnemyDefinitionId.Boulder, PaletteRow: 1),
        ]),
        new(nameof(BillyMays), [
        ]),
        new(nameof(BigPinkSaveRoom), [
        ]),
        new(nameof(BigPink), [
            new(EnemyDefinitionId.Rio, PaletteRow: 1),
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 2),
            new(EnemyDefinitionId.Zeb, PaletteRow: 3),
        ]),
        new(nameof(EtecoonSave), [
        ]),
        new(nameof(BelowSpazer), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.Cacatac, PaletteRow: 2),
        ]),
        new(nameof(GreenBrinstarSave), [
        ]),
        new(nameof(Spazer), [
        ]),
        new(nameof(PinkBrinstarPowerBombs), [
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 1),
            new(EnemyDefinitionId.Metaree, PaletteRow: 1),
            new(EnemyDefinitionId.SidehopperLarge, PaletteRow: 2),
        ]),
        new(nameof(RedTowerRefill), [
        ]),
        new(nameof(KraidRefill), [
        ]),
        new(nameof(VariaSuit), [
        ]),
        new(nameof(RedBrinstarSave), [
        ]),
        new(nameof(BlueBrinstarETank0), [
            new(EnemyDefinitionId.Eye, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(GreenBrinstarFirefleas), [
            new(EnemyDefinitionId.Fireflea, PaletteRow: 2),
        ]),
        new(nameof(GreenBrinstarBeetoms), [
            new(EnemyDefinitionId.Beetom, PaletteRow: 1),
        ]),
        new(nameof(EtecoonETank), [
            new(EnemyDefinitionId.Zebbo, PaletteRow: 1),
            new(EnemyDefinitionId.Beetom, PaletteRow: 2),
        ]),
        new(nameof(PinkBrinstarHoppers), [
            new(EnemyDefinitionId.SidehopperLarge, PaletteRow: 1),
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 2),
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 3),
        ]),
        new(nameof(WarehouseETank), [
            new(EnemyDefinitionId.Reflec, PaletteRow: 1),
            new(EnemyDefinitionId.Beetom, PaletteRow: 2),
        ]),
        new(nameof(WarehouseEntrance), [
        ]),
        new(nameof(WarehouseKihunter), [
            new(EnemyDefinitionId.KihunterGreen, PaletteRow: 1),
        ]),
        new(nameof(GreenBrinstarMainShaft), [
            new(EnemyDefinitionId.Etecoon, PaletteRow: 1),
            new(EnemyDefinitionId.Zeela, PaletteRow: 2),
            new(EnemyDefinitionId.Ripper2, PaletteRow: 3),
        ]),
        new(nameof(SporeSpawnSuper), [
            new(EnemyDefinitionId.Zeb, PaletteRow: 1),
        ]),
        new(nameof(EarlySupers), [
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 1),
            new(EnemyDefinitionId.ShutterGrowing, PaletteRow: 2),
            new(EnemyDefinitionId.Waver, PaletteRow: 3),
            new(EnemyDefinitionId.Zeb, PaletteRow: 7),
        ]),
        new(nameof(RedBrinstarFirefleas), [
            new(EnemyDefinitionId.Fireflea, PaletteRow: 1),
            new(EnemyDefinitionId.Waver, PaletteRow: 2),
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 3),
        ]),
        new(nameof(ConstructionZone0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
            new(EnemyDefinitionId.FaceBlock, PaletteRow: 7),
        ]),
        new(nameof(GreenHillZone), [
            new(EnemyDefinitionId.Sidehopper, PaletteRow: 1),
            new(EnemyDefinitionId.Geega, PaletteRow: 2),
        ]),
        new(nameof(Dachora), [
            new(EnemyDefinitionId.Zeela, PaletteRow: 1),
            new(EnemyDefinitionId.Metaree, PaletteRow: 2),
            new(EnemyDefinitionId.Rio, PaletteRow: 3),
            new(EnemyDefinitionId.Dachora, PaletteRow: 7),
        ]),
        new(nameof(Hellway), [
            new(EnemyDefinitionId.Zeela, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 2),
        ]),
        new(nameof(FirstMissile), [
        ]),
        new(nameof(Kraid), [
            new(EnemyDefinitionId.Kraid, PaletteRow: 7),
        ]),
        new(nameof(BrinstarReserveTank), [
        ]),
        new(nameof(XrayScope), [
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 1),
        ]),
        new(nameof(StatuesHallway), [
        ]),
        new(nameof(AlphaPowerBomb), [
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
        ]),
        new(nameof(KraidEyeDoor), [
            new(EnemyDefinitionId.Zeb, PaletteRow: 1),
            new(EnemyDefinitionId.Sbug, PaletteRow: 2),
        ]),
        new(nameof(Caterpillar), [
            new(EnemyDefinitionId.Zero, PaletteRow: 2),
            new(EnemyDefinitionId.Cacatac, PaletteRow: 3),
        ]),
        new(nameof(MiniKraid), [
            new(EnemyDefinitionId.PirateGreenWalking, PaletteRow: 1),
            new(EnemyDefinitionId.MiniKraid, PaletteRow: 3),
        ]),
        new(nameof(SporeSpawn), [
            new(EnemyDefinitionId.SporeSpawn, PaletteRow: 1),
        ]),
        new(nameof(SkreeBoost), [
            new(EnemyDefinitionId.Skree, PaletteRow: 1),
            new(EnemyDefinitionId.Boyon, PaletteRow: 7),
        ]),
        new(nameof(GrappleTutorial3), [
            new(EnemyDefinitionId.Gamet, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
        ]),
        new(nameof(PostCrocShaft), [
            new(EnemyDefinitionId.Viola, PaletteRow: 1),
        ]),
        new(nameof(MetalPirates), [
            new(EnemyDefinitionId.PirateGoldNinja, PaletteRow: 1),
        ]),
        new(nameof(AcidStatue), [
            new(EnemyDefinitionId.Chozo, PaletteRow: 1),
            new(EnemyDefinitionId.Magdollite, PaletteRow: 3),
            new(EnemyDefinitionId.Holtz, PaletteRow: 7),
        ]),
        new(nameof(PurpleFarming), [
            new(EnemyDefinitionId.Gamet, PaletteRow: 1),
        ]),
        new(nameof(IceBeamGate), [
            new(EnemyDefinitionId.ShutterGrowing, PaletteRow: 1),
            new(EnemyDefinitionId.Dessgeega, PaletteRow: 2),
            new(EnemyDefinitionId.Mella, PaletteRow: 3),
            new(EnemyDefinitionId.Sova, PaletteRow: 7),
        ]),
        new(nameof(SpikyAcidSnakesTunnel), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
        ]),
        new(nameof(RedKihunterShaft), [
            new(EnemyDefinitionId.KihunterRed, PaletteRow: 1),
            new(EnemyDefinitionId.Reflec, PaletteRow: 2),
        ]),
        new(nameof(FastRipper), [
            new(EnemyDefinitionId.Ripper2, PaletteRow: 1),
        ]),
        new(nameof(WaveBeam), [
        ]),
        new(nameof(FastPillarsSetup), [
            new(EnemyDefinitionId.PirateGoldWall, PaletteRow: 1),
            new(EnemyDefinitionId.PirateGoldWalking, PaletteRow: 2),
            new(EnemyDefinitionId.Viola, PaletteRow: 3),
        ]),
        new(nameof(LowerNorfairElev), [
        ]),
        new(nameof(LNSave), [
        ]),
        new(nameof(UpperNorfairRefill), [
        ]),
        new(nameof(NorfairMap), [
        ]),
        new(nameof(MickeyMouse), [
            new(EnemyDefinitionId.DessgeegaLarge, PaletteRow: 1),
            new(EnemyDefinitionId.Multiviola, PaletteRow: 2),
        ]),
        new(nameof(GTEnergyRefill), [
        ]),
        new(nameof(Ridley), [
            new(EnemyDefinitionId.Ridley, PaletteRow: 1),
            new(EnemyDefinitionId.RidleyExplosion, PaletteRow: 1, StagingBlock: 2),
        ]),
        new(nameof(PostCrocPowerBombs), [
        ]),
        new(nameof(IceBeam), [
        ]),
        new(nameof(HiJumpBoots), [
        ]),
        new(nameof(PostCrocSave), [
        ]),
        new(nameof(FrogSpeedway), [
            new(EnemyDefinitionId.Beetom, PaletteRow: 1),
        ]),
        new(nameof(RisingTide), [
            new(EnemyDefinitionId.Squeept, PaletteRow: 1),
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
            new(EnemyDefinitionId.Dragon, PaletteRow: 3),
        ]),
        new(nameof(PostCrocFarming), [
            new(EnemyDefinitionId.Gamet, PaletteRow: 1),
            new(EnemyDefinitionId.GRipper, PaletteRow: 2),
            new(EnemyDefinitionId.Kamer2, PaletteRow: 3),
        ]),
        new(nameof(WorstRoomInTheGame), [
            new(EnemyDefinitionId.Namihe, PaletteRow: 1),
            new(EnemyDefinitionId.PirateGoldWall, PaletteRow: 2),
            new(EnemyDefinitionId.PirateGoldWalking, PaletteRow: 3),
        ]),
        new(nameof(DoubleChamber), [
            new(EnemyDefinitionId.Fune, PaletteRow: 1),
            new(EnemyDefinitionId.Kamer, PaletteRow: 2),
            new(EnemyDefinitionId.Kago, PaletteRow: 3),
            new(EnemyDefinitionId.GRipper, PaletteRow: 7),
        ]),
        new(nameof(GrappleBeam), [
            new(EnemyDefinitionId.Multiviola, PaletteRow: 1),
        ]),
        new(nameof(RedPirateShaft), [
            new(EnemyDefinitionId.PirateRedWalking, PaletteRow: 1),
            new(EnemyDefinitionId.MZoomer, PaletteRow: 3),
        ]),
        new(nameof(CrocomireSpeedway), [
            new(EnemyDefinitionId.PirateRedWalking, PaletteRow: 1),
            new(EnemyDefinitionId.Multiviola, PaletteRow: 2),
            new(EnemyDefinitionId.Cacatac, PaletteRow: 3),
            new(EnemyDefinitionId.Sova, PaletteRow: 3),
        ]),
        new(nameof(Plowerhouse), [
            new(EnemyDefinitionId.Zebbo, PaletteRow: 1),
            new(EnemyDefinitionId.Holtz, PaletteRow: 7),
        ]),
        new(nameof(LNFireflea), [
            new(EnemyDefinitionId.Fireflea, PaletteRow: 1),
            new(EnemyDefinitionId.Boulder, PaletteRow: 2),
            new(EnemyDefinitionId.Fune, PaletteRow: 3),
        ]),
        new(nameof(MagdolliteTunnel), [
            new(EnemyDefinitionId.Multiviola, PaletteRow: 1),
            new(EnemyDefinitionId.Magdollite, PaletteRow: 7),
        ]),
        new(nameof(UnusedState8FB3EEB48899), [
        ]),
        new(nameof(LavaDive), [
            new(EnemyDefinitionId.Namihe, PaletteRow: 1),
        ]),
        new(nameof(Pillar), [
            new(EnemyDefinitionId.Puromi, PaletteRow: 7),
        ]),
        new(nameof(CrocomireEscape), [
            new(EnemyDefinitionId.Dragon, PaletteRow: 1),
            new(EnemyDefinitionId.Geruta, PaletteRow: 2),
        ]),
        new(nameof(LNSpringBallMaze), [
            new(EnemyDefinitionId.Alcoon, PaletteRow: 1),
        ]),
        new(nameof(LNEscapePowerBombs), [
        ]),
        new(nameof(PurpleShaft), [
        ]),
        new(nameof(RidleyETank), [
        ]),
        new(nameof(ScrewAttack), [
        ]),
        new(nameof(GreenBubblesMissiles), [
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 7),
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
            new(EnemyDefinitionId.Geruta, PaletteRow: 3),
        ]),
        new(nameof(Amphitheatre), [
            new(EnemyDefinitionId.PirateGoldWalking, PaletteRow: 7),
        ]),
        new(nameof(CathedralEntrance), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
            new(EnemyDefinitionId.Dessgeega, PaletteRow: 2),
        ]),
        new(nameof(MainHall), [
            new(EnemyDefinitionId.Hibashi, PaletteRow: 1),
            new(EnemyDefinitionId.Dragon, PaletteRow: 7),
            new(EnemyDefinitionId.Gamet, PaletteRow: 7),
        ]),
        new(nameof(PostCrocJump), [
            new(EnemyDefinitionId.Mella, PaletteRow: 1),
            new(EnemyDefinitionId.GRipper, PaletteRow: 2),
            new(EnemyDefinitionId.Kamer2, PaletteRow: 3),
        ]),
        new(nameof(FrogSave), [
        ]),
        new(nameof(CrocomireSave), [
        ]),
    ];

    /// <summary>Remaining pointer-ordered portion of the compiled bank-$B4 graphics-set catalog.</summary>
    private static RoomEnemyGraphicsSetDefinition[] BuildSegment1() =>
    [
        new(nameof(CrumbleShaft), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
        ]),
        new(nameof(Wasteland), [
            new(EnemyDefinitionId.Hibashi, PaletteRow: 1),
            new(EnemyDefinitionId.DessgeegaLarge, PaletteRow: 2),
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 3),
            new(EnemyDefinitionId.KzanTop, PaletteRow: 7),
        ]),
        new(nameof(AcidSnakesTunnel), [
            new(EnemyDefinitionId.Dragon, PaletteRow: 1),
            new(EnemyDefinitionId.Gamet, PaletteRow: 2),
        ]),
        new(nameof(GrappleTutorial1), [
        ]),
        new(nameof(BubbleMountainSave), [
        ]),
        new(nameof(HiJumpETank), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
        ]),
        new(nameof(BubbleMountain), [
            new(EnemyDefinitionId.Waver, PaletteRow: 1),
            new(EnemyDefinitionId.Ripper2, PaletteRow: 1),
            new(EnemyDefinitionId.Cacatac, PaletteRow: 2),
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
        ]),
        new(nameof(IceBeamAcid), [
            new(EnemyDefinitionId.Tripper, PaletteRow: 7),
        ]),
        new(nameof(IceBeamTutorial), [
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
            new(EnemyDefinitionId.Ripper2, PaletteRow: 2),
        ]),
        new(nameof(PostCrocMissile), [
            new(EnemyDefinitionId.Metaree, PaletteRow: 1),
            new(EnemyDefinitionId.Gamet, PaletteRow: 2),
        ]),
        new(nameof(Cathedral), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
            new(EnemyDefinitionId.Geruta, PaletteRow: 2),
        ]),
        new(nameof(NorfairReserveTank), [
            new(EnemyDefinitionId.Dragon, PaletteRow: 1),
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
        ]),
        new(nameof(GrappleTutorial2), [
            new(EnemyDefinitionId.Fune, PaletteRow: 1),
            new(EnemyDefinitionId.Multiviola, PaletteRow: 2),
        ]),
        new(nameof(IceBeamSnake), [
            new(EnemyDefinitionId.Sova, PaletteRow: 1),
            new(EnemyDefinitionId.Fune, PaletteRow: 2),
        ]),
        new(nameof(GoldenTorizo), [
            new(EnemyDefinitionId.GoldenTorizo, PaletteRow: 1),
        ]),
        new(nameof(SpikyPlatformsTunnel), [
            new(EnemyDefinitionId.Tripper, PaletteRow: 7),
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 2),
        ]),
        new(nameof(SpeedBooster), [
        ]),
        new(nameof(LNFarming), [
            new(EnemyDefinitionId.Viola, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 7),
        ]),
        new(nameof(SingleChamber), [
            new(EnemyDefinitionId.Multiviola, PaletteRow: 1),
            new(EnemyDefinitionId.Alcoon, PaletteRow: 2),
        ]),
        new(nameof(SpeedBoosterHall), [
            new(EnemyDefinitionId.Geruta, PaletteRow: 1),
            new(EnemyDefinitionId.Metaree, PaletteRow: 2),
        ]),
        new(nameof(BatCave), [
            new(EnemyDefinitionId.Gamet, PaletteRow: 1),
            new(EnemyDefinitionId.Skree, PaletteRow: 2),
        ]),
        new(nameof(KronicBoost), [
            new(EnemyDefinitionId.Viola, PaletteRow: 1),
        ]),
        new(nameof(BusinessCenter), [
            new(EnemyDefinitionId.Sova, PaletteRow: 2),
        ]),
        new(nameof(ThreeMusketeers), [
            new(EnemyDefinitionId.Hibashi, PaletteRow: 1),
            new(EnemyDefinitionId.KihunterRed, PaletteRow: 2),
            new(EnemyDefinitionId.ShutterShootable, PaletteRow: 3),
        ]),
        new(nameof(Crocomire), [
            new(EnemyDefinitionId.Crocomire, PaletteRow: 7, StagingBlock: 1),
        ]),
        new(nameof(LNElevSave), [
        ]),
        new(nameof(Volcano), [
            new(EnemyDefinitionId.Fune, PaletteRow: 1),
            new(EnemyDefinitionId.LavaRocks, PaletteRow: 2),
        ]),
        new(nameof(UpperNorfairFarming), [
            new(EnemyDefinitionId.Fune, PaletteRow: 1),
            new(EnemyDefinitionId.Gamet, PaletteRow: 2),
        ]),
        new(nameof(SpongeBath1), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(WreckedShipETank1), [
            new(EnemyDefinitionId.Kamer2, PaletteRow: 1),
            new(EnemyDefinitionId.Skultera, PaletteRow: 2),
        ]),
        new(nameof(WreckedShipMainShaft0), [
            new(EnemyDefinitionId.Coven, PaletteRow: 1),
            new(EnemyDefinitionId.KzanTop, PaletteRow: 2),
            new(EnemyDefinitionId.Atomic, PaletteRow: 3),
            new(EnemyDefinitionId.Sbug, PaletteRow: 3),
        ]),
        new(nameof(BowlingAlley0), [
            new(EnemyDefinitionId.Chozo, PaletteRow: 1),
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
        ]),
        new(nameof(Attic1), [
            new(EnemyDefinitionId.KihunterYellow, PaletteRow: 1),
            new(EnemyDefinitionId.Atomic, PaletteRow: 2),
            new(EnemyDefinitionId.Spark, PaletteRow: 3),
        ]),
        new(nameof(Basement1), [
            new(EnemyDefinitionId.Robot, PaletteRow: 1),
            new(EnemyDefinitionId.Atomic, PaletteRow: 2),
            new(EnemyDefinitionId.Spark, PaletteRow: 3),
        ]),
        new(nameof(WreckedShipMap1), [
        ]),
        new(nameof(WSWestSuper0), [
        ]),
        new(nameof(SpikyDeath0), [
        ]),
        new(nameof(SpongeBath0), [
        ]),
        new(nameof(WreckedShipEntrance1), [
        ]),
        new(nameof(ElectricDeath0), [
        ]),
        new(nameof(BowlingAlley1), [
            new(EnemyDefinitionId.Chozo, PaletteRow: 1),
            new(EnemyDefinitionId.Robot, PaletteRow: 3),
        ]),
        new(nameof(AssemblyLine0), [
        ]),
        new(nameof(Phantoon1), [
        ]),
        new(nameof(WreckedShipETank0), [
        ]),
        new(nameof(GravitySuit0), [
        ]),
        new(nameof(WreckedShipMap0), [
            new(EnemyDefinitionId.Coven, PaletteRow: 1),
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
            new(EnemyDefinitionId.Bull, PaletteRow: 7),
        ]),
        new(nameof(GravitySuit1), [
        ]),
        new(nameof(Basement0), [
            new(EnemyDefinitionId.RobotNoPower, PaletteRow: 7),
            new(EnemyDefinitionId.Sbug, PaletteRow: 7),
        ]),
        new(nameof(WreckedShipEntrance0), [
            new(EnemyDefinitionId.Sbug, PaletteRow: 1),
        ]),
        new(nameof(AssemblyLine1), [
            new(EnemyDefinitionId.Spark, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Robot, PaletteRow: 3),
        ]),
        new(nameof(WreckedShipSave1), [
        ]),
        new(nameof(WSWestSuper1), [
            new(EnemyDefinitionId.Spark, PaletteRow: 1),
        ]),
        new(nameof(Attic0), [
            new(EnemyDefinitionId.Coven, PaletteRow: 1),
            new(EnemyDefinitionId.Atomic, PaletteRow: 2),
            new(EnemyDefinitionId.Sbug, PaletteRow: 2),
        ]),
        new(nameof(WSEastSuper0), [
            new(EnemyDefinitionId.Coven, PaletteRow: 1),
            new(EnemyDefinitionId.Atomic, PaletteRow: 2),
            new(EnemyDefinitionId.Sbug, PaletteRow: 2),
            new(EnemyDefinitionId.RobotNoPower, PaletteRow: 7),
        ]),
        new(nameof(WreckedShipSave0), [
            new(EnemyDefinitionId.Coven, PaletteRow: 1),
            new(EnemyDefinitionId.Sbug, PaletteRow: 2),
        ]),
        new(nameof(ElectricDeath1), [
            new(EnemyDefinitionId.Spark, PaletteRow: 1),
        ]),
        new(nameof(SpikyDeath1), [
            new(EnemyDefinitionId.KzanTop, PaletteRow: 7),
        ]),
        new(nameof(WSEastSuper1), [
            new(EnemyDefinitionId.Robot, PaletteRow: 1),
            new(EnemyDefinitionId.Bull, PaletteRow: 2),
            new(EnemyDefinitionId.Spark, PaletteRow: 3),
        ]),
        new(nameof(Phantoon0), [
            new(EnemyDefinitionId.PhantoonBody, PaletteRow: 7),
        ]),
        new(nameof(WreckedShipMainShaft1), [
            new(EnemyDefinitionId.Spark, PaletteRow: 1),
            new(EnemyDefinitionId.Atomic, PaletteRow: 2),
            new(EnemyDefinitionId.KzanTop, PaletteRow: 3),
        ]),
        new(nameof(MtEverest), [
            new(EnemyDefinitionId.Powamp, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 3),
        ]),
        new(nameof(HalfieClimb), [
            new(EnemyDefinitionId.Oum, PaletteRow: 1),
            new(EnemyDefinitionId.Mochtroid, PaletteRow: 2),
        ]),
        new(nameof(WateringHole), [
            new(EnemyDefinitionId.Choot, PaletteRow: 1),
            new(EnemyDefinitionId.Zeb, PaletteRow: 2),
        ]),
        new(nameof(CrabShaft), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
        ]),
        new(nameof(MaridiaElev), [
            new(EnemyDefinitionId.Ripper, PaletteRow: 3),
            new(EnemyDefinitionId.Owtch, PaletteRow: 7),
        ]),
        new(nameof(Colosseum), [
            new(EnemyDefinitionId.Mochtroid, PaletteRow: 1),
        ]),
        new(nameof(AqueductSave), [
        ]),
        new(nameof(ThePrecious), [
        ]),
        new(nameof(BotwoonETank), [
            new(EnemyDefinitionId.Puyo, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
            new(EnemyDefinitionId.Zoa, PaletteRow: 2),
        ]),
        new(nameof(DraygonSave), [
        ]),
        new(nameof(MaridiaMissileRefill), [
        ]),
        new(nameof(PlasmaBeachQuicksand), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(BotwoonQuicksand), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(Shaktool), [
            new(EnemyDefinitionId.Shaktool, PaletteRow: 1),
            new(EnemyDefinitionId.Yard, PaletteRow: 2),
        ]),
        new(nameof(Draygon0), [
            new(EnemyDefinitionId.DraygonBody, PaletteRow: 7),
        ]),
        new(nameof(Aqueduct), [
            new(EnemyDefinitionId.Yard, PaletteRow: 1),
        ]),
        new(nameof(RedFish), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 2),
        ]),
        new(nameof(Plasma), [
            new(EnemyDefinitionId.PirateMagentaWalking, PaletteRow: 1),
            new(EnemyDefinitionId.PirateMagentaWall, PaletteRow: 2),
        ]),
        new(nameof(SpaceJump), [
        ]),
        new(nameof(Pants), [
            new(EnemyDefinitionId.Menu, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Tripper, PaletteRow: 2),
        ]),
        new(nameof(GlassTunnelSave), [
        ]),
        new(nameof(GlassTunnel), [
            new(EnemyDefinitionId.NoobTubeCrack, PaletteRow: 7),
        ]),
        new(nameof(WestTunnel), [
        ]),
        new(nameof(EastTunnel), [
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
            new(EnemyDefinitionId.Zebbo, PaletteRow: 2),
        ]),
        new(nameof(MamaTurtle), [
            new(EnemyDefinitionId.MamaTurtle, PaletteRow: 1),
        ]),
        new(nameof(CrabTunnel), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
        ]),
        new(nameof(NWestMaridiaBug), [
            new(EnemyDefinitionId.Owtch, PaletteRow: 1),
            new(EnemyDefinitionId.Menu, PaletteRow: 2),
        ]),
        new(nameof(PseudoPlasmaSpark), [
            new(EnemyDefinitionId.Owtch, PaletteRow: 1),
            new(EnemyDefinitionId.Choot, PaletteRow: 2),
            new(EnemyDefinitionId.Skultera, PaletteRow: 3),
        ]),
        new(nameof(WestSandHallTunnel), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
        ]),
        new(nameof(PlasmaTutorial), [
            new(EnemyDefinitionId.Puyo, PaletteRow: 1),
            new(EnemyDefinitionId.MZoomer, PaletteRow: 1),
        ]),
        new(nameof(ThreadTheNeedle), [
            new(EnemyDefinitionId.Puyo, PaletteRow: 1),
            new(EnemyDefinitionId.Boyon, PaletteRow: 1),
            new(EnemyDefinitionId.Choot, PaletteRow: 2),
        ]),
        new(nameof(PlasmaSpark), [
            new(EnemyDefinitionId.Owtch, PaletteRow: 1),
            new(EnemyDefinitionId.Choot, PaletteRow: 2),
            new(EnemyDefinitionId.Skultera, PaletteRow: 3),
        ]),
        new(nameof(Toilet), [
            new(EnemyDefinitionId.Mochtroid, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Sciser, PaletteRow: 2),
            new(EnemyDefinitionId.Yard, PaletteRow: 3),
        ]),
        new(nameof(WestSandHall), [
            new(EnemyDefinitionId.Evir, PaletteRow: 1),
        ]),
        new(nameof(Oasis), [
        ]),
        new(nameof(EastSandHall), [
            new(EnemyDefinitionId.Evir, PaletteRow: 1),
        ]),
        new(nameof(WestAqueductQuicksand), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(EastAqueductQuicksand), [
            new(EnemyDefinitionId.Bull, PaletteRow: 1),
        ]),
        new(nameof(Butterfly), [
            new(EnemyDefinitionId.Zoa, PaletteRow: 1),
        ]),
        new(nameof(BotwoonHallway), [
            new(EnemyDefinitionId.Mochtroid, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Sciser, PaletteRow: 2),
        ]),
        new(nameof(EastPants), [
            new(EnemyDefinitionId.Puyo, PaletteRow: 1),
            new(EnemyDefinitionId.Tripper, PaletteRow: 1),
        ]),
        new(nameof(Springball), [
        ]),
        new(nameof(BelowBotwoonETank), [
            new(EnemyDefinitionId.Zoa, PaletteRow: 1),
            new(EnemyDefinitionId.Owtch, PaletteRow: 2),
        ]),
        new(nameof(Kassiuz), [
            new(EnemyDefinitionId.Choot, PaletteRow: 1),
            new(EnemyDefinitionId.Puyo, PaletteRow: 2),
            new(EnemyDefinitionId.Boyon, PaletteRow: 2),
        ]),
        new(nameof(ForgottenHighwaySave), [
        ]),
        new(nameof(MaridiaEnergyRefill), [
        ]),
        new(nameof(MaridiaMap), [
        ]),
        new(nameof(CrabHole), [
            new(EnemyDefinitionId.Sciser, PaletteRow: 1),
        ]),
        new(nameof(Botwoon), [
            new(EnemyDefinitionId.Owtch, PaletteRow: 1),
            new(EnemyDefinitionId.Botwoon, PaletteRow: 7),
        ]),
        new(nameof(MainStreet), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.Sciser, PaletteRow: 2),
        ]),
        new(nameof(BugSandHole), [
            new(EnemyDefinitionId.YappingMaw, PaletteRow: 1),
            new(EnemyDefinitionId.Zoa, PaletteRow: 7),
        ]),
        new(nameof(EastSandHole), [
            new(EnemyDefinitionId.Boulder, PaletteRow: 1),
        ]),
        new(nameof(WestSandHole), [
            new(EnemyDefinitionId.Boulder, PaletteRow: 1),
        ]),
        new(nameof(WestCactusAlley), [
            new(EnemyDefinitionId.Cacatac, PaletteRow: 7),
            new(EnemyDefinitionId.Zoa, PaletteRow: 7),
        ]),
        new(nameof(EastCactusAlley), [
            new(EnemyDefinitionId.Cacatac, PaletteRow: 1),
            new(EnemyDefinitionId.Zoa, PaletteRow: 1),
        ]),
        new(nameof(FishTank), [
            new(EnemyDefinitionId.Skultera, PaletteRow: 1),
            new(EnemyDefinitionId.PirateMagentaWalking, PaletteRow: 2),
        ]),
        new(nameof(TourianEscape4), [
            new(EnemyDefinitionId.PirateSilverWall, PaletteRow: 1),
            new(EnemyDefinitionId.PirateSilverWalking, PaletteRow: 2),
        ]),
        new(nameof(MotherBrainSave), [
        ]),
        new(nameof(Metroids10), [
            new(EnemyDefinitionId.Metroid, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(DustTorizo), [
            new(EnemyDefinitionId.CorpseTorizo, PaletteRow: 1),
        ]),
        new(nameof(BigBoy), [
            new(EnemyDefinitionId.CorpseSidehopper, PaletteRow: 1),
            new(EnemyDefinitionId.CorpseSidehopper2, PaletteRow: 1),
            new(EnemyDefinitionId.BabyMetroid, PaletteRow: 1),
        ]),
        new(nameof(MotherBrain01), [
            new(EnemyDefinitionId.MotherBrainHead, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
            new(EnemyDefinitionId.MotherBrainBody, PaletteRow: 1, StagingBlock: 1),
        ]),
        new(nameof(TourianEyeDoor), [
        ]),
        new(nameof(BlueHopper), [
            new(EnemyDefinitionId.SidehopperTourian, PaletteRow: 1),
        ]),
        new(nameof(TourianEscape1), [
            new(EnemyDefinitionId.ShutterDestroyable, PaletteRow: 1),
        ]),
        new(nameof(TourianFirst), [
        ]),
        new(nameof(Metroids20), [
            new(EnemyDefinitionId.Metroid, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(Metroids40), [
            new(EnemyDefinitionId.Metroid, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(Metroids11), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(Metroids21), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(TourianEscape2), [
            new(EnemyDefinitionId.PirateSilverWall, PaletteRow: 1),
        ]),
        new(nameof(Metroids31), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(Metroids41), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(TourianEscape3), [
            new(EnemyDefinitionId.PirateSilverWalking, PaletteRow: 1),
            new(EnemyDefinitionId.ShutterGrowing, PaletteRow: 2),
        ]),
        new(nameof(Seaweed), [
        ]),
        new(nameof(TourianRecharge), [
        ]),
        new(nameof(UpperTourianSave), [
        ]),
        new(nameof(Metroids30), [
            new(EnemyDefinitionId.Metroid, PaletteRow: 1),
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(RinkaShaft), [
            new(EnemyDefinitionId.Rinka, PaletteRow: 2),
        ]),
        new(nameof(CeresElev0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(FallingTile0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(MagnetStairs0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(DeadScientist0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(Room58Escape0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(CeresRidley0), [
            new(EnemyDefinitionId.RidleyCeres, PaletteRow: 1),
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(CeresElev1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(FallingTile1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(MagnetStairs1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(DeadScientist1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(Room58Escape1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(CeresRidley1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
    ];
}
