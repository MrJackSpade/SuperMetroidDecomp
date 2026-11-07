namespace SuperMetroid.Core.Enemies;

/// <summary>The enemy graphics set of every retail room state, by name (#1527).</summary>
/// <remarks>
/// A set is known to the game only by its name, which selects the installed sheets its enemies draw from; the members
/// each set loads are the import's knowledge.
/// </remarks>
[NumberedIdentities("Each numbered member is the enemy graphics set of one numbered room or room state, named for it; no caller indexes the sets by number.")]
public static class RoomEnemyGraphicsSetDefinitions
{
    /// <summary>
    /// The enemy graphics a room draws under when it loads no set (#1392, #1476): no set's tiles or palettes loaded over
    /// the standard ones. A room whose population is empty loads no set and draws its spawned objects under this.
    /// </summary>
    public static RoomEnemyGraphicsSetDefinition SessionStart { get; } = new(nameof(SessionStart));

    /// <summary>The <c>EnemySets_Draygon1MotherBrain2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Draygon1MotherBrain2 { get; } = new(nameof(Draygon1MotherBrain2));
    /// <summary>The <c>EnemySets_EastOcean</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastOcean { get; } = new(nameof(EastOcean));
    /// <summary>The <c>EnemySets_CrateriaPowerBombs</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaPowerBombs { get; } = new(nameof(CrateriaPowerBombs));
    /// <summary>The <c>EnemySets_Terminator</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Terminator { get; } = new(nameof(Terminator));
    /// <summary>The <c>EnemySets_FinalMissileBombway1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FinalMissileBombwayWhenZebesAwake { get; } = new(nameof(FinalMissileBombwayWhenZebesAwake));
    /// <summary>The <c>EnemySets_FinalMissileBombway0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FinalMissileBombwayDefault { get; } = new(nameof(FinalMissileBombwayDefault));
    /// <summary>The <c>EnemySets_Parlor1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ParlorWhenZebesAwake { get; } = new(nameof(ParlorWhenZebesAwake));
    /// <summary>The <c>EnemySets_Flyway01</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Flyway01 { get; } = new(nameof(Flyway01));
    /// <summary>The <c>EnemySets_Pit1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PitWithMorphBallAndMissiles { get; } = new(nameof(PitWithMorphBallAndMissiles));
    /// <summary>The <c>EnemySets_GauntletEntrance</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GauntletEntrance { get; } = new(nameof(GauntletEntrance));
    /// <summary>The <c>EnemySets_BombTorizo01</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BombTorizo01 { get; } = new(nameof(BombTorizo01));
    /// <summary>The <c>EnemySets_GreenPiratesShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenPiratesShaft { get; } = new(nameof(GreenPiratesShaft));
    /// <summary>The <c>EnemySets_GreenBrinstarElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarElev { get; } = new(nameof(GreenBrinstarElev));
    /// <summary>The <c>EnemySets_FinalMissile0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FinalMissileDefault { get; } = new(nameof(FinalMissileDefault));
    /// <summary>The <c>EnemySets_CrateriaMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaMap { get; } = new(nameof(CrateriaMap));
    /// <summary>The <c>EnemySets_FinalMissile1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FinalMissileWhenZebesAwake { get; } = new(nameof(FinalMissileWhenZebesAwake));
    /// <summary>The <c>EnemySets_Climb2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ClimbWhenZebesTimebombSet { get; } = new(nameof(ClimbWhenZebesTimebombSet));
    /// <summary>The <c>EnemySets_CrateriaSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaSave { get; } = new(nameof(CrateriaSave));
    /// <summary>The <c>EnemySets_ForgottenHighwayElbow</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ForgottenHighwayElbow { get; } = new(nameof(ForgottenHighwayElbow));
    /// <summary>The <c>EnemySets_Moat</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Moat { get; } = new(nameof(Moat));
    /// <summary>The <c>EnemySets_Climb0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ClimbDefault { get; } = new(nameof(ClimbDefault));
    /// <summary>The <c>EnemySets_WestOcean</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestOcean { get; } = new(nameof(WestOcean));
    /// <summary>The <c>EnemySets_Parlor0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ParlorDefault { get; } = new(nameof(ParlorDefault));
    /// <summary>The <c>EnemySets_LandingSite012</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LandingSite012 { get; } = new(nameof(LandingSite012));
    /// <summary>The <c>EnemySets_CrateriaSuper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaSuper { get; } = new(nameof(CrateriaSuper));
    /// <summary>The <c>EnemySets_PreBowling</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PreBowling { get; } = new(nameof(PreBowling));
    /// <summary>The <c>EnemySets_Climb1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ClimbWhenZebesAwake { get; } = new(nameof(ClimbWhenZebesAwake));
    /// <summary>The <c>EnemySets_BowlingAlleyPath</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BowlingAlleyPath { get; } = new(nameof(BowlingAlleyPath));
    /// <summary>The <c>EnemySets_RedBinstarElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedBinstarElev { get; } = new(nameof(RedBinstarElev));
    /// <summary>The <c>EnemySets_GauntletETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GauntletETank { get; } = new(nameof(GauntletETank));
    /// <summary>The <c>EnemySets_Pit0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PitDefault { get; } = new(nameof(PitDefault));
    /// <summary>The <c>EnemySets_CrabMaze</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrabMaze { get; } = new(nameof(CrabMaze));
    /// <summary>The <c>EnemySets_CrateriaTube</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaTube { get; } = new(nameof(CrateriaTube));
    /// <summary>The <c>EnemySets_ForgottenHighwayKagos</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ForgottenHighwayKagos { get; } = new(nameof(ForgottenHighwayKagos));
    /// <summary>The <c>EnemySets_ElevToBlueBrinstar</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ElevToBlueBrinstar { get; } = new(nameof(ElevToBlueBrinstar));
    /// <summary>The <c>EnemySets_ForgottenHighwayElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ForgottenHighwayElev { get; } = new(nameof(ForgottenHighwayElev));
    /// <summary>The <c>EnemySets_PreMapFlyway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PreMapFlyway { get; } = new(nameof(PreMapFlyway));
    /// <summary>The <c>EnemySets_LowerMushrooms</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LowerMushrooms { get; } = new(nameof(LowerMushrooms));
    /// <summary>The <c>EnemySets_LandingSite3</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LandingSiteWhenZebesTimebombSet { get; } = new(nameof(LandingSiteWhenZebesTimebombSet));
    /// <summary>The <c>EnemySets_Parlor2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ParlorWhenZebesTimebombSet { get; } = new(nameof(ParlorWhenZebesTimebombSet));
    /// <summary>The <c>EnemySets_BombTorizo2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BombTorizoWhenZebesTimebombSet { get; } = new(nameof(BombTorizoWhenZebesTimebombSet));
    /// <summary>The <c>EnemySets_Flyway2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FlywayWhenZebesTimebombSet { get; } = new(nameof(FlywayWhenZebesTimebombSet));
    /// <summary>The <c>EnemySets_CrateriaKihunter</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrateriaKihunter { get; } = new(nameof(CrateriaKihunter));
    /// <summary>The <c>EnemySets_BrinstarPreMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BrinstarPreMap { get; } = new(nameof(BrinstarPreMap));
    /// <summary>The <c>EnemySets_GreenBrinstarMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarMap { get; } = new(nameof(GreenBrinstarMap));
    /// <summary>The <c>EnemySets_GreenBrinstarMissileRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarMissileRefill { get; } = new(nameof(GreenBrinstarMissileRefill));
    /// <summary>The <c>EnemySets_SporeSpawnKihunters</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SporeSpawnKihunters { get; } = new(nameof(SporeSpawnKihunters));
    /// <summary>The <c>EnemySets_EtecoonSuper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EtecoonSuper { get; } = new(nameof(EtecoonSuper));
    /// <summary>The <c>EnemySets_DachoraEnergyRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DachoraEnergyRefill { get; } = new(nameof(DachoraEnergyRefill));
    /// <summary>The <c>EnemySets_SporeSpawnFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SporeSpawnFarming { get; } = new(nameof(SporeSpawnFarming));
    /// <summary>The <c>EnemySets_Statues</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Statues { get; } = new(nameof(Statues));
    /// <summary>The <c>EnemySets_HopperETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition HopperETank { get; } = new(nameof(HopperETank));
    /// <summary>The <c>EnemySets_BetaPowerBomb</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BetaPowerBomb { get; } = new(nameof(BetaPowerBomb));
    /// <summary>The <c>EnemySets_WaterwayETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WaterwayETank { get; } = new(nameof(WaterwayETank));
    /// <summary>The <c>EnemySets_ConstructionZone1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ConstructionZoneWhenZebesAwake { get; } = new(nameof(ConstructionZoneWhenZebesAwake));
    /// <summary>The <c>EnemySets_BlueBrinstarETank1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BlueBrinstarETankWhenZebesAwake { get; } = new(nameof(BlueBrinstarETankWhenZebesAwake));
    /// <summary>The <c>EnemySets_NoobBridge</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition NoobBridge { get; } = new(nameof(NoobBridge));
    /// <summary>The <c>EnemySets_MorphBall1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MorphBallWhenZebesAwake { get; } = new(nameof(MorphBallWhenZebesAwake));
    /// <summary>The <c>EnemySets_WarehouseSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseSave { get; } = new(nameof(WarehouseSave));
    /// <summary>The <c>EnemySets_MorphBall0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MorphBallDefault { get; } = new(nameof(MorphBallDefault));
    /// <summary>The <c>EnemySets_WarehouseZeela</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseZeela { get; } = new(nameof(WarehouseZeela));
    /// <summary>The <c>EnemySets_RedTower</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedTower { get; } = new(nameof(RedTower));
    /// <summary>The <c>EnemySets_BlueBrinstarBoulders</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BlueBrinstarBoulders { get; } = new(nameof(BlueBrinstarBoulders));
    /// <summary>The <c>EnemySets_BillyMays</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BillyMays { get; } = new(nameof(BillyMays));
    /// <summary>The <c>EnemySets_BigPinkSaveRoom</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BigPinkSaveRoom { get; } = new(nameof(BigPinkSaveRoom));
    /// <summary>The <c>EnemySets_BigPink</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BigPink { get; } = new(nameof(BigPink));
    /// <summary>The <c>EnemySets_EtecoonSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EtecoonSave { get; } = new(nameof(EtecoonSave));
    /// <summary>The <c>EnemySets_BelowSpazer</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BelowSpazer { get; } = new(nameof(BelowSpazer));
    /// <summary>The <c>EnemySets_GreenBrinstarSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarSave { get; } = new(nameof(GreenBrinstarSave));
    /// <summary>The <c>EnemySets_Spazer</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Spazer { get; } = new(nameof(Spazer));
    /// <summary>The <c>EnemySets_PinkBrinstarPowerBombs</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PinkBrinstarPowerBombs { get; } = new(nameof(PinkBrinstarPowerBombs));
    /// <summary>The <c>EnemySets_RedTowerRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedTowerRefill { get; } = new(nameof(RedTowerRefill));
    /// <summary>The <c>EnemySets_KraidRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition KraidRefill { get; } = new(nameof(KraidRefill));
    /// <summary>The <c>EnemySets_VariaSuit</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition VariaSuit { get; } = new(nameof(VariaSuit));
    /// <summary>The <c>EnemySets_RedBrinstarSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedBrinstarSave { get; } = new(nameof(RedBrinstarSave));
    /// <summary>The <c>EnemySets_BlueBrinstarETank0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BlueBrinstarETankDefault { get; } = new(nameof(BlueBrinstarETankDefault));
    /// <summary>The <c>EnemySets_GreenBrinstarFirefleas</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarFirefleas { get; } = new(nameof(GreenBrinstarFirefleas));
    /// <summary>The <c>EnemySets_GreenBrinstarBeetoms</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarBeetoms { get; } = new(nameof(GreenBrinstarBeetoms));
    /// <summary>The <c>EnemySets_EtecoonETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EtecoonETank { get; } = new(nameof(EtecoonETank));
    /// <summary>The <c>EnemySets_PinkBrinstarHoppers</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PinkBrinstarHoppers { get; } = new(nameof(PinkBrinstarHoppers));
    /// <summary>The <c>EnemySets_WarehouseETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseETank { get; } = new(nameof(WarehouseETank));
    /// <summary>The <c>EnemySets_WarehouseEntrance</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseEntrance { get; } = new(nameof(WarehouseEntrance));
    /// <summary>The <c>EnemySets_WarehouseKihunter</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WarehouseKihunter { get; } = new(nameof(WarehouseKihunter));
    /// <summary>The <c>EnemySets_GreenBrinstarMainShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBrinstarMainShaft { get; } = new(nameof(GreenBrinstarMainShaft));
    /// <summary>The <c>EnemySets_SporeSpawnSuper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SporeSpawnSuper { get; } = new(nameof(SporeSpawnSuper));
    /// <summary>The <c>EnemySets_EarlySupers</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EarlySupers { get; } = new(nameof(EarlySupers));
    /// <summary>The <c>EnemySets_RedBrinstarFirefleas</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedBrinstarFirefleas { get; } = new(nameof(RedBrinstarFirefleas));
    /// <summary>The <c>EnemySets_ConstructionZone0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ConstructionZoneDefault { get; } = new(nameof(ConstructionZoneDefault));
    /// <summary>The <c>EnemySets_GreenHillZone</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenHillZone { get; } = new(nameof(GreenHillZone));
    /// <summary>The <c>EnemySets_Dachora</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Dachora { get; } = new(nameof(Dachora));
    /// <summary>The <c>EnemySets_Hellway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Hellway { get; } = new(nameof(Hellway));
    /// <summary>The <c>EnemySets_FirstMissile</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FirstMissile { get; } = new(nameof(FirstMissile));
    /// <summary>The <c>EnemySets_Kraid</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Kraid { get; } = new(nameof(Kraid));
    /// <summary>The <c>EnemySets_BrinstarReserveTank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BrinstarReserveTank { get; } = new(nameof(BrinstarReserveTank));
    /// <summary>The <c>EnemySets_XrayScope</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition XrayScope { get; } = new(nameof(XrayScope));
    /// <summary>The <c>EnemySets_StatuesHallway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition StatuesHallway { get; } = new(nameof(StatuesHallway));
    /// <summary>The <c>EnemySets_AlphaPowerBomb</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AlphaPowerBomb { get; } = new(nameof(AlphaPowerBomb));
    /// <summary>The <c>EnemySets_KraidEyeDoor</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition KraidEyeDoor { get; } = new(nameof(KraidEyeDoor));
    /// <summary>The <c>EnemySets_Caterpillar</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Caterpillar { get; } = new(nameof(Caterpillar));
    /// <summary>The <c>EnemySets_MiniKraid</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MiniKraid { get; } = new(nameof(MiniKraid));
    /// <summary>The <c>EnemySets_SporeSpawn</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SporeSpawn { get; } = new(nameof(SporeSpawn));
    /// <summary>The <c>EnemySets_SkreeBoost</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SkreeBoost { get; } = new(nameof(SkreeBoost));
    /// <summary>The <c>EnemySets_GrappleTutorial3</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GrappleTutorial3 { get; } = new(nameof(GrappleTutorial3));
    /// <summary>The <c>EnemySets_PostCrocShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocShaft { get; } = new(nameof(PostCrocShaft));
    /// <summary>The <c>EnemySets_MetalPirates</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MetalPirates { get; } = new(nameof(MetalPirates));
    /// <summary>The <c>EnemySets_AcidStatue</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AcidStatue { get; } = new(nameof(AcidStatue));
    /// <summary>The <c>EnemySets_PurpleFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PurpleFarming { get; } = new(nameof(PurpleFarming));
    /// <summary>The <c>EnemySets_IceBeamGate</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeamGate { get; } = new(nameof(IceBeamGate));
    /// <summary>The <c>EnemySets_SpikyAcidSnakesTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpikyAcidSnakesTunnel { get; } = new(nameof(SpikyAcidSnakesTunnel));
    /// <summary>The <c>EnemySets_RedKihunterShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedKihunterShaft { get; } = new(nameof(RedKihunterShaft));
    /// <summary>The <c>EnemySets_FastRipper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FastRipper { get; } = new(nameof(FastRipper));
    /// <summary>The <c>EnemySets_WaveBeam</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WaveBeam { get; } = new(nameof(WaveBeam));
    /// <summary>The <c>EnemySets_FastPillarsSetup</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FastPillarsSetup { get; } = new(nameof(FastPillarsSetup));
    /// <summary>The <c>EnemySets_LowerNorfairElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LowerNorfairElev { get; } = new(nameof(LowerNorfairElev));
    /// <summary>The <c>EnemySets_LNSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNSave { get; } = new(nameof(LNSave));
    /// <summary>The <c>EnemySets_UpperNorfairRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition UpperNorfairRefill { get; } = new(nameof(UpperNorfairRefill));
    /// <summary>The <c>EnemySets_NorfairMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition NorfairMap { get; } = new(nameof(NorfairMap));
    /// <summary>The <c>EnemySets_MickeyMouse</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MickeyMouse { get; } = new(nameof(MickeyMouse));
    /// <summary>The <c>EnemySets_GTEnergyRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GTEnergyRefill { get; } = new(nameof(GTEnergyRefill));
    /// <summary>The <c>EnemySets_Ridley</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Ridley { get; } = new(nameof(Ridley));
    /// <summary>The <c>EnemySets_PostCrocPowerBombs</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocPowerBombs { get; } = new(nameof(PostCrocPowerBombs));
    /// <summary>The <c>EnemySets_IceBeam</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeam { get; } = new(nameof(IceBeam));
    /// <summary>The <c>EnemySets_HiJumpBoots</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition HiJumpBoots { get; } = new(nameof(HiJumpBoots));
    /// <summary>The <c>EnemySets_PostCrocSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocSave { get; } = new(nameof(PostCrocSave));
    /// <summary>The <c>EnemySets_FrogSpeedway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FrogSpeedway { get; } = new(nameof(FrogSpeedway));
    /// <summary>The <c>EnemySets_RisingTide</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RisingTide { get; } = new(nameof(RisingTide));
    /// <summary>The <c>EnemySets_PostCrocFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocFarming { get; } = new(nameof(PostCrocFarming));
    /// <summary>The <c>EnemySets_WorstRoomInTheGame</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WorstRoomInTheGame { get; } = new(nameof(WorstRoomInTheGame));
    /// <summary>The <c>EnemySets_DoubleChamber</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DoubleChamber { get; } = new(nameof(DoubleChamber));
    /// <summary>The <c>EnemySets_GrappleBeam</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GrappleBeam { get; } = new(nameof(GrappleBeam));
    /// <summary>The <c>EnemySets_RedPirateShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedPirateShaft { get; } = new(nameof(RedPirateShaft));
    /// <summary>The <c>EnemySets_CrocomireSpeedway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrocomireSpeedway { get; } = new(nameof(CrocomireSpeedway));
    /// <summary>The <c>EnemySets_Plowerhouse</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Plowerhouse { get; } = new(nameof(Plowerhouse));
    /// <summary>The <c>EnemySets_LNFireflea</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNFireflea { get; } = new(nameof(LNFireflea));
    /// <summary>The <c>EnemySets_MagdolliteTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MagdolliteTunnel { get; } = new(nameof(MagdolliteTunnel));
    /// <summary>The <c>EnemySets_UnusedState8FB3EEB48899</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition UnusedRoom8FB3EE { get; } = new(nameof(UnusedRoom8FB3EE));
    /// <summary>The <c>EnemySets_LavaDive</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LavaDive { get; } = new(nameof(LavaDive));
    /// <summary>The <c>EnemySets_Pillar</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Pillar { get; } = new(nameof(Pillar));
    /// <summary>The <c>EnemySets_CrocomireEscape</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrocomireEscape { get; } = new(nameof(CrocomireEscape));
    /// <summary>The <c>EnemySets_LNSpringBallMaze</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNSpringBallMaze { get; } = new(nameof(LNSpringBallMaze));
    /// <summary>The <c>EnemySets_LNEscapePowerBombs</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNEscapePowerBombs { get; } = new(nameof(LNEscapePowerBombs));
    /// <summary>The <c>EnemySets_PurpleShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PurpleShaft { get; } = new(nameof(PurpleShaft));
    /// <summary>The <c>EnemySets_RidleyETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RidleyETank { get; } = new(nameof(RidleyETank));
    /// <summary>The <c>EnemySets_ScrewAttack</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ScrewAttack { get; } = new(nameof(ScrewAttack));
    /// <summary>The <c>EnemySets_GreenBubblesMissiles</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GreenBubblesMissiles { get; } = new(nameof(GreenBubblesMissiles));
    /// <summary>The <c>EnemySets_Amphitheatre</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Amphitheatre { get; } = new(nameof(Amphitheatre));
    /// <summary>The <c>EnemySets_CathedralEntrance</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CathedralEntrance { get; } = new(nameof(CathedralEntrance));
    /// <summary>The <c>EnemySets_MainHall</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MainHall { get; } = new(nameof(MainHall));
    /// <summary>The <c>EnemySets_PostCrocJump</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocJump { get; } = new(nameof(PostCrocJump));
    /// <summary>The <c>EnemySets_FrogSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FrogSave { get; } = new(nameof(FrogSave));
    /// <summary>The <c>EnemySets_CrocomireSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrocomireSave { get; } = new(nameof(CrocomireSave));
    /// <summary>The <c>EnemySets_CrumbleShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrumbleShaft { get; } = new(nameof(CrumbleShaft));
    /// <summary>The <c>EnemySets_Wasteland</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Wasteland { get; } = new(nameof(Wasteland));
    /// <summary>The <c>EnemySets_AcidSnakesTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AcidSnakesTunnel { get; } = new(nameof(AcidSnakesTunnel));
    /// <summary>The <c>EnemySets_GrappleTutorial1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GrappleTutorial1 { get; } = new(nameof(GrappleTutorial1));
    /// <summary>The <c>EnemySets_BubbleMountainSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BubbleMountainSave { get; } = new(nameof(BubbleMountainSave));
    /// <summary>The <c>EnemySets_HiJumpETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition HiJumpETank { get; } = new(nameof(HiJumpETank));
    /// <summary>The <c>EnemySets_BubbleMountain</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BubbleMountain { get; } = new(nameof(BubbleMountain));
    /// <summary>The <c>EnemySets_IceBeamAcid</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeamAcid { get; } = new(nameof(IceBeamAcid));
    /// <summary>The <c>EnemySets_IceBeamTutorial</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeamTutorial { get; } = new(nameof(IceBeamTutorial));
    /// <summary>The <c>EnemySets_PostCrocMissile</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PostCrocMissile { get; } = new(nameof(PostCrocMissile));
    /// <summary>The <c>EnemySets_Cathedral</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Cathedral { get; } = new(nameof(Cathedral));
    /// <summary>The <c>EnemySets_NorfairReserveTank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition NorfairReserveTank { get; } = new(nameof(NorfairReserveTank));
    /// <summary>The <c>EnemySets_GrappleTutorial2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GrappleTutorial2 { get; } = new(nameof(GrappleTutorial2));
    /// <summary>The <c>EnemySets_IceBeamSnake</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition IceBeamSnake { get; } = new(nameof(IceBeamSnake));
    /// <summary>The <c>EnemySets_GoldenTorizo</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GoldenTorizo { get; } = new(nameof(GoldenTorizo));
    /// <summary>The <c>EnemySets_SpikyPlatformsTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpikyPlatformsTunnel { get; } = new(nameof(SpikyPlatformsTunnel));
    /// <summary>The <c>EnemySets_SpeedBooster</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpeedBooster { get; } = new(nameof(SpeedBooster));
    /// <summary>The <c>EnemySets_LNFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNFarming { get; } = new(nameof(LNFarming));
    /// <summary>The <c>EnemySets_SingleChamber</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SingleChamber { get; } = new(nameof(SingleChamber));
    /// <summary>The <c>EnemySets_SpeedBoosterHall</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpeedBoosterHall { get; } = new(nameof(SpeedBoosterHall));
    /// <summary>The <c>EnemySets_BatCave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BatCave { get; } = new(nameof(BatCave));
    /// <summary>The <c>EnemySets_KronicBoost</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition KronicBoost { get; } = new(nameof(KronicBoost));
    /// <summary>The <c>EnemySets_BusinessCenter</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BusinessCenter { get; } = new(nameof(BusinessCenter));
    /// <summary>The <c>EnemySets_ThreeMusketeers</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ThreeMusketeers { get; } = new(nameof(ThreeMusketeers));
    /// <summary>The <c>EnemySets_Crocomire</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Crocomire { get; } = new(nameof(Crocomire));
    /// <summary>The <c>EnemySets_LNElevSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition LNElevSave { get; } = new(nameof(LNElevSave));
    /// <summary>The <c>EnemySets_Volcano</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Volcano { get; } = new(nameof(Volcano));
    /// <summary>The <c>EnemySets_UpperNorfairFarming</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition UpperNorfairFarming { get; } = new(nameof(UpperNorfairFarming));
    /// <summary>The <c>EnemySets_SpongeBath1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpongeBathWhenAreaBossDead { get; } = new(nameof(SpongeBathWhenAreaBossDead));
    /// <summary>The <c>EnemySets_WreckedShipETank1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipETankWhenAreaBossDead { get; } = new(nameof(WreckedShipETankWhenAreaBossDead));
    /// <summary>The <c>EnemySets_WreckedShipMainShaft0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipMainShaftDefault { get; } = new(nameof(WreckedShipMainShaftDefault));
    /// <summary>The <c>EnemySets_BowlingAlley0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BowlingAlleyDefault { get; } = new(nameof(BowlingAlleyDefault));
    /// <summary>The <c>EnemySets_Attic1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AtticWhenAreaBossDead { get; } = new(nameof(AtticWhenAreaBossDead));
    /// <summary>The <c>EnemySets_Basement1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BasementWhenAreaBossDead { get; } = new(nameof(BasementWhenAreaBossDead));
    /// <summary>The <c>EnemySets_WreckedShipMap1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipMapWhenAreaBossDead { get; } = new(nameof(WreckedShipMapWhenAreaBossDead));
    /// <summary>The <c>EnemySets_WSWestSuper0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WSWestSuperDefault { get; } = new(nameof(WSWestSuperDefault));
    /// <summary>The <c>EnemySets_SpikyDeath0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpikyDeathDefault { get; } = new(nameof(SpikyDeathDefault));
    /// <summary>The <c>EnemySets_SpongeBath0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpongeBathDefault { get; } = new(nameof(SpongeBathDefault));
    /// <summary>The <c>EnemySets_WreckedShipEntrance1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipEntranceWhenAreaBossDead { get; } = new(nameof(WreckedShipEntranceWhenAreaBossDead));
    /// <summary>The <c>EnemySets_ElectricDeath0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ElectricDeathDefault { get; } = new(nameof(ElectricDeathDefault));
    /// <summary>The <c>EnemySets_BowlingAlley1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BowlingAlleyWhenAreaBossDead { get; } = new(nameof(BowlingAlleyWhenAreaBossDead));
    /// <summary>The <c>EnemySets_AssemblyLine0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AssemblyLineDefault { get; } = new(nameof(AssemblyLineDefault));
    /// <summary>The <c>EnemySets_Phantoon1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PhantoonWhenAreaBossDead { get; } = new(nameof(PhantoonWhenAreaBossDead));
    /// <summary>The <c>EnemySets_WreckedShipETank0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipETankDefault { get; } = new(nameof(WreckedShipETankDefault));
    /// <summary>The <c>EnemySets_GravitySuit0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GravitySuitDefault { get; } = new(nameof(GravitySuitDefault));
    /// <summary>The <c>EnemySets_WreckedShipMap0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipMapDefault { get; } = new(nameof(WreckedShipMapDefault));
    /// <summary>The <c>EnemySets_GravitySuit1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GravitySuitWhenAreaBossDead { get; } = new(nameof(GravitySuitWhenAreaBossDead));
    /// <summary>The <c>EnemySets_Basement0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BasementDefault { get; } = new(nameof(BasementDefault));
    /// <summary>The <c>EnemySets_WreckedShipEntrance0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipEntranceDefault { get; } = new(nameof(WreckedShipEntranceDefault));
    /// <summary>The <c>EnemySets_AssemblyLine1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AssemblyLineWhenAreaBossDead { get; } = new(nameof(AssemblyLineWhenAreaBossDead));
    /// <summary>The <c>EnemySets_WreckedShipSave1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipSaveWhenAreaBossDead { get; } = new(nameof(WreckedShipSaveWhenAreaBossDead));
    /// <summary>The <c>EnemySets_WSWestSuper1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WSWestSuperWhenAreaBossDead { get; } = new(nameof(WSWestSuperWhenAreaBossDead));
    /// <summary>The <c>EnemySets_Attic0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AtticDefault { get; } = new(nameof(AtticDefault));
    /// <summary>The <c>EnemySets_WSEastSuper0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WSEastSuperDefault { get; } = new(nameof(WSEastSuperDefault));
    /// <summary>The <c>EnemySets_WreckedShipSave0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipSaveDefault { get; } = new(nameof(WreckedShipSaveDefault));
    /// <summary>The <c>EnemySets_ElectricDeath1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ElectricDeathWhenAreaBossDead { get; } = new(nameof(ElectricDeathWhenAreaBossDead));
    /// <summary>The <c>EnemySets_SpikyDeath1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpikyDeathWhenAreaBossDead { get; } = new(nameof(SpikyDeathWhenAreaBossDead));
    /// <summary>The <c>EnemySets_WSEastSuper1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WSEastSuperWhenAreaBossDead { get; } = new(nameof(WSEastSuperWhenAreaBossDead));
    /// <summary>The <c>EnemySets_Phantoon0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PhantoonDefault { get; } = new(nameof(PhantoonDefault));
    /// <summary>The <c>EnemySets_WreckedShipMainShaft1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WreckedShipMainShaftWhenAreaBossDead { get; } = new(nameof(WreckedShipMainShaftWhenAreaBossDead));
    /// <summary>The <c>EnemySets_MtEverest</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MtEverest { get; } = new(nameof(MtEverest));
    /// <summary>The <c>EnemySets_HalfieClimb</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition HalfieClimb { get; } = new(nameof(HalfieClimb));
    /// <summary>The <c>EnemySets_WateringHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WateringHole { get; } = new(nameof(WateringHole));
    /// <summary>The <c>EnemySets_CrabShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrabShaft { get; } = new(nameof(CrabShaft));
    /// <summary>The <c>EnemySets_MaridiaElev</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MaridiaElev { get; } = new(nameof(MaridiaElev));
    /// <summary>The <c>EnemySets_Colosseum</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Colosseum { get; } = new(nameof(Colosseum));
    /// <summary>The <c>EnemySets_AqueductSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition AqueductSave { get; } = new(nameof(AqueductSave));
    /// <summary>The <c>EnemySets_ThePrecious</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ThePrecious { get; } = new(nameof(ThePrecious));
    /// <summary>The <c>EnemySets_BotwoonETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BotwoonETank { get; } = new(nameof(BotwoonETank));
    /// <summary>The <c>EnemySets_DraygonSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DraygonSave { get; } = new(nameof(DraygonSave));
    /// <summary>The <c>EnemySets_MaridiaMissileRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MaridiaMissileRefill { get; } = new(nameof(MaridiaMissileRefill));
    /// <summary>The <c>EnemySets_PlasmaBeachQuicksand</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PlasmaBeachQuicksand { get; } = new(nameof(PlasmaBeachQuicksand));
    /// <summary>The <c>EnemySets_BotwoonQuicksand</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BotwoonQuicksand { get; } = new(nameof(BotwoonQuicksand));
    /// <summary>The <c>EnemySets_Shaktool</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Shaktool { get; } = new(nameof(Shaktool));
    /// <summary>The <c>EnemySets_Draygon0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DraygonDefault { get; } = new(nameof(DraygonDefault));
    /// <summary>The <c>EnemySets_Aqueduct</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Aqueduct { get; } = new(nameof(Aqueduct));
    /// <summary>The <c>EnemySets_RedFish</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RedFish { get; } = new(nameof(RedFish));
    /// <summary>The <c>EnemySets_Plasma</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Plasma { get; } = new(nameof(Plasma));
    /// <summary>The <c>EnemySets_SpaceJump</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition SpaceJump { get; } = new(nameof(SpaceJump));
    /// <summary>The <c>EnemySets_Pants</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Pants { get; } = new(nameof(Pants));
    /// <summary>The <c>EnemySets_GlassTunnelSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GlassTunnelSave { get; } = new(nameof(GlassTunnelSave));
    /// <summary>The <c>EnemySets_GlassTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition GlassTunnel { get; } = new(nameof(GlassTunnel));
    /// <summary>The <c>EnemySets_WestTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestTunnel { get; } = new(nameof(WestTunnel));
    /// <summary>The <c>EnemySets_EastTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastTunnel { get; } = new(nameof(EastTunnel));
    /// <summary>The <c>EnemySets_MamaTurtle</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MamaTurtle { get; } = new(nameof(MamaTurtle));
    /// <summary>The <c>EnemySets_CrabTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrabTunnel { get; } = new(nameof(CrabTunnel));
    /// <summary>The <c>EnemySets_NWestMaridiaBug</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition NWestMaridiaBug { get; } = new(nameof(NWestMaridiaBug));
    /// <summary>The <c>EnemySets_PseudoPlasmaSpark</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PseudoPlasmaSpark { get; } = new(nameof(PseudoPlasmaSpark));
    /// <summary>The <c>EnemySets_WestSandHallTunnel</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestSandHallTunnel { get; } = new(nameof(WestSandHallTunnel));
    /// <summary>The <c>EnemySets_PlasmaTutorial</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PlasmaTutorial { get; } = new(nameof(PlasmaTutorial));
    /// <summary>The <c>EnemySets_ThreadTheNeedle</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ThreadTheNeedle { get; } = new(nameof(ThreadTheNeedle));
    /// <summary>The <c>EnemySets_PlasmaSpark</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition PlasmaSpark { get; } = new(nameof(PlasmaSpark));
    /// <summary>The <c>EnemySets_Toilet</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Toilet { get; } = new(nameof(Toilet));
    /// <summary>The <c>EnemySets_WestSandHall</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestSandHall { get; } = new(nameof(WestSandHall));
    /// <summary>The <c>EnemySets_Oasis</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Oasis { get; } = new(nameof(Oasis));
    /// <summary>The <c>EnemySets_EastSandHall</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastSandHall { get; } = new(nameof(EastSandHall));
    /// <summary>The <c>EnemySets_WestAqueductQuicksand</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestAqueductQuicksand { get; } = new(nameof(WestAqueductQuicksand));
    /// <summary>The <c>EnemySets_EastAqueductQuicksand</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastAqueductQuicksand { get; } = new(nameof(EastAqueductQuicksand));
    /// <summary>The <c>EnemySets_Butterfly</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Butterfly { get; } = new(nameof(Butterfly));
    /// <summary>The <c>EnemySets_BotwoonHallway</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BotwoonHallway { get; } = new(nameof(BotwoonHallway));
    /// <summary>The <c>EnemySets_EastPants</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastPants { get; } = new(nameof(EastPants));
    /// <summary>The <c>EnemySets_Springball</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Springball { get; } = new(nameof(Springball));
    /// <summary>The <c>EnemySets_BelowBotwoonETank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BelowBotwoonETank { get; } = new(nameof(BelowBotwoonETank));
    /// <summary>The <c>EnemySets_Kassiuz</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Kassiuz { get; } = new(nameof(Kassiuz));
    /// <summary>The <c>EnemySets_ForgottenHighwaySave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition ForgottenHighwaySave { get; } = new(nameof(ForgottenHighwaySave));
    /// <summary>The <c>EnemySets_MaridiaEnergyRefill</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MaridiaEnergyRefill { get; } = new(nameof(MaridiaEnergyRefill));
    /// <summary>The <c>EnemySets_MaridiaMap</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MaridiaMap { get; } = new(nameof(MaridiaMap));
    /// <summary>The <c>EnemySets_CrabHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CrabHole { get; } = new(nameof(CrabHole));
    /// <summary>The <c>EnemySets_Botwoon</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Botwoon { get; } = new(nameof(Botwoon));
    /// <summary>The <c>EnemySets_MainStreet</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MainStreet { get; } = new(nameof(MainStreet));
    /// <summary>The <c>EnemySets_BugSandHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BugSandHole { get; } = new(nameof(BugSandHole));
    /// <summary>The <c>EnemySets_EastSandHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastSandHole { get; } = new(nameof(EastSandHole));
    /// <summary>The <c>EnemySets_WestSandHole</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestSandHole { get; } = new(nameof(WestSandHole));
    /// <summary>The <c>EnemySets_WestCactusAlley</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition WestCactusAlley { get; } = new(nameof(WestCactusAlley));
    /// <summary>The <c>EnemySets_EastCactusAlley</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition EastCactusAlley { get; } = new(nameof(EastCactusAlley));
    /// <summary>The <c>EnemySets_FishTank</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FishTank { get; } = new(nameof(FishTank));
    /// <summary>The <c>EnemySets_TourianEscape4</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEscape4 { get; } = new(nameof(TourianEscape4));
    /// <summary>The <c>EnemySets_MotherBrainSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MotherBrainSave { get; } = new(nameof(MotherBrainSave));
    /// <summary>The <c>EnemySets_Metroids10</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids1Default { get; } = new(nameof(Metroids1Default));
    /// <summary>The <c>EnemySets_DustTorizo</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DustTorizo { get; } = new(nameof(DustTorizo));
    /// <summary>The <c>EnemySets_BigBoy</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BigBoy { get; } = new(nameof(BigBoy));
    /// <summary>The <c>EnemySets_MotherBrain01</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MotherBrain01 { get; } = new(nameof(MotherBrain01));
    /// <summary>The <c>EnemySets_TourianEyeDoor</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEyeDoor { get; } = new(nameof(TourianEyeDoor));
    /// <summary>The <c>EnemySets_BlueHopper</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition BlueHopper { get; } = new(nameof(BlueHopper));
    /// <summary>The <c>EnemySets_TourianEscape1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEscape1 { get; } = new(nameof(TourianEscape1));
    /// <summary>The <c>EnemySets_TourianFirst</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianFirst { get; } = new(nameof(TourianFirst));
    /// <summary>The <c>EnemySets_Metroids20</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids2Default { get; } = new(nameof(Metroids2Default));
    /// <summary>The <c>EnemySets_Metroids40</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids4Default { get; } = new(nameof(Metroids4Default));
    /// <summary>The <c>EnemySets_Metroids11</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids1WhenFirstMetroidHallCleared { get; } = new(nameof(Metroids1WhenFirstMetroidHallCleared));
    /// <summary>The <c>EnemySets_Metroids21</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids2WhenFirstMetroidShaftCleared { get; } = new(nameof(Metroids2WhenFirstMetroidShaftCleared));
    /// <summary>The <c>EnemySets_TourianEscape2</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEscape2 { get; } = new(nameof(TourianEscape2));
    /// <summary>The <c>EnemySets_Metroids31</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids3WhenSecondMetroidHallCleared { get; } = new(nameof(Metroids3WhenSecondMetroidHallCleared));
    /// <summary>The <c>EnemySets_Metroids41</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids4WhenSecondMetroidShaftCleared { get; } = new(nameof(Metroids4WhenSecondMetroidShaftCleared));
    /// <summary>The <c>EnemySets_TourianEscape3</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianEscape3 { get; } = new(nameof(TourianEscape3));
    /// <summary>The <c>EnemySets_Seaweed</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Seaweed { get; } = new(nameof(Seaweed));
    /// <summary>The <c>EnemySets_TourianRecharge</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition TourianRecharge { get; } = new(nameof(TourianRecharge));
    /// <summary>The <c>EnemySets_UpperTourianSave</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition UpperTourianSave { get; } = new(nameof(UpperTourianSave));
    /// <summary>The <c>EnemySets_Metroids30</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Metroids3Default { get; } = new(nameof(Metroids3Default));
    /// <summary>The <c>EnemySets_RinkaShaft</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition RinkaShaft { get; } = new(nameof(RinkaShaft));
    /// <summary>The <c>EnemySets_CeresElev0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CeresElevDefault { get; } = new(nameof(CeresElevDefault));
    /// <summary>The <c>EnemySets_FallingTile0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FallingTileDefault { get; } = new(nameof(FallingTileDefault));
    /// <summary>The <c>EnemySets_MagnetStairs0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MagnetStairsDefault { get; } = new(nameof(MagnetStairsDefault));
    /// <summary>The <c>EnemySets_DeadScientist0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DeadScientistDefault { get; } = new(nameof(DeadScientistDefault));
    /// <summary>The <c>EnemySets_Room58Escape0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Room58EscapeDefault { get; } = new(nameof(Room58EscapeDefault));
    /// <summary>The <c>EnemySets_CeresRidley0</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CeresRidleyDefault { get; } = new(nameof(CeresRidleyDefault));
    /// <summary>The <c>EnemySets_CeresElev1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CeresElevWhenAreaBossDead { get; } = new(nameof(CeresElevWhenAreaBossDead));
    /// <summary>The <c>EnemySets_FallingTile1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition FallingTileWhenAreaBossDead { get; } = new(nameof(FallingTileWhenAreaBossDead));
    /// <summary>The <c>EnemySets_MagnetStairs1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition MagnetStairsWhenAreaBossDead { get; } = new(nameof(MagnetStairsWhenAreaBossDead));
    /// <summary>The <c>EnemySets_DeadScientist1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition DeadScientistWhenAreaBossDead { get; } = new(nameof(DeadScientistWhenAreaBossDead));
    /// <summary>The <c>EnemySets_Room58Escape1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition Room58EscapeWhenAreaBossDead { get; } = new(nameof(Room58EscapeWhenAreaBossDead));
    /// <summary>The <c>EnemySets_CeresRidley1</c> list.</summary>
    public static RoomEnemyGraphicsSetDefinition CeresRidleyWhenAreaBossDead { get; } = new(nameof(CeresRidleyWhenAreaBossDead));

}
