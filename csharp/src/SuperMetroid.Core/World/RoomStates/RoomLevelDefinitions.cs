using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// The level of every retail room state: the room's initial blocks, block types and background blocks. Room states
/// that share a level (a room's event and boss states) name the same member. Each level is named for the room of the
/// first state that uses it, in the room-state catalogs' order.
/// </summary>
[NumberedIdentities("Each numbered level is a different room's, numbered as the room is (the Metroid rooms, the Tourian escape rooms, the grapple tutorial rooms), or a room's own state number where its states load different levels; no caller indexes them.")]
public static class RoomLevelDefinitions
{
    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.LandingSite0"/> uses first.</summary>
    public static RoomLevelId LandingSite { get; } = new(nameof(LandingSite));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.GauntletEntrance0"/> uses first.</summary>
    public static RoomLevelId GauntletEntrance { get; } = new(nameof(GauntletEntrance));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.Parlor0"/> uses first.</summary>
    public static RoomLevelId Parlor { get; } = new(nameof(Parlor));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.CrateriaPowerBombs"/> uses first.</summary>
    public static RoomLevelId CrateriaPowerBombs { get; } = new(nameof(CrateriaPowerBombs));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.CrateriaSave"/> uses first.</summary>
    public static RoomLevelId CrateriaSave { get; } = new(nameof(CrateriaSave));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.WestOcean"/> uses first.</summary>
    public static RoomLevelId WestOcean { get; } = new(nameof(WestOcean));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.BowlingAlleyPath"/> uses first.</summary>
    public static RoomLevelId BowlingAlleyPath { get; } = new(nameof(BowlingAlleyPath));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.CrateriaKihunter"/> uses first.</summary>
    public static RoomLevelId CrateriaKihunter { get; } = new(nameof(CrateriaKihunter));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.ForgottenHighwayElev"/> uses first.</summary>
    public static RoomLevelId ForgottenHighwayElev { get; } = new(nameof(ForgottenHighwayElev));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.EastOcean"/> uses first.</summary>
    public static RoomLevelId EastOcean { get; } = new(nameof(EastOcean));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.ForgottenHighwayKagos"/> uses first.</summary>
    public static RoomLevelId ForgottenHighwayKagos { get; } = new(nameof(ForgottenHighwayKagos));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.CrabMaze"/> uses first.</summary>
    public static RoomLevelId CrabMaze { get; } = new(nameof(CrabMaze));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.ForgottenHighwayElbow"/> uses first.</summary>
    public static RoomLevelId ForgottenHighwayElbow { get; } = new(nameof(ForgottenHighwayElbow));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.CrateriaTube"/> uses first.</summary>
    public static RoomLevelId CrateriaTube { get; } = new(nameof(CrateriaTube));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.Moat"/> uses first.</summary>
    public static RoomLevelId Moat { get; } = new(nameof(Moat));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.RedBinstarElev"/> uses first.</summary>
    public static RoomLevelId RedBinstarElev { get; } = new(nameof(RedBinstarElev));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.GauntletETank"/> uses first.</summary>
    public static RoomLevelId GauntletETank { get; } = new(nameof(GauntletETank));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.PreBowling"/> uses first.</summary>
    public static RoomLevelId PreBowling { get; } = new(nameof(PreBowling));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.Climb0"/> uses first.</summary>
    public static RoomLevelId Climb { get; } = new(nameof(Climb));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.Pit0"/> uses first.</summary>
    public static RoomLevelId Pit { get; } = new(nameof(Pit));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.ElevToBlueBrinstar0"/> uses first.</summary>
    public static RoomLevelId ElevToBlueBrinstar { get; } = new(nameof(ElevToBlueBrinstar));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.BombTorizo0"/> uses first.</summary>
    public static RoomLevelId BombTorizo { get; } = new(nameof(BombTorizo));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.Flyway0"/> uses first.</summary>
    public static RoomLevelId Flyway { get; } = new(nameof(Flyway));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.PreMapFlyway"/> uses first.</summary>
    public static RoomLevelId PreMapFlyway { get; } = new(nameof(PreMapFlyway));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.Terminator"/> uses first.</summary>
    public static RoomLevelId Terminator { get; } = new(nameof(Terminator));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.GreenBrinstarElev"/> uses first.</summary>
    public static RoomLevelId GreenBrinstarElev { get; } = new(nameof(GreenBrinstarElev));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.LowerMushrooms"/> uses first.</summary>
    public static RoomLevelId LowerMushrooms { get; } = new(nameof(LowerMushrooms));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.CrateriaMap"/> uses first.</summary>
    public static RoomLevelId CrateriaMap { get; } = new(nameof(CrateriaMap));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.GreenPiratesShaft"/> uses first.</summary>
    public static RoomLevelId GreenPiratesShaft { get; } = new(nameof(GreenPiratesShaft));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.CrateriaSuper"/> uses first.</summary>
    public static RoomLevelId CrateriaSuper { get; } = new(nameof(CrateriaSuper));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.FinalMissileBombway0"/> uses first.</summary>
    public static RoomLevelId FinalMissileBombway { get; } = new(nameof(FinalMissileBombway));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.FinalMissile0"/> uses first.</summary>
    public static RoomLevelId FinalMissile { get; } = new(nameof(FinalMissile));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.GreenBrinstarMainShaft"/> uses first.</summary>
    public static RoomLevelId GreenBrinstarMainShaft { get; } = new(nameof(GreenBrinstarMainShaft));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.SporeSpawnSuper"/> uses first.</summary>
    public static RoomLevelId SporeSpawnSuper { get; } = new(nameof(SporeSpawnSuper));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.BrinstarPreMap"/> uses first.</summary>
    public static RoomLevelId BrinstarPreMap { get; } = new(nameof(BrinstarPreMap));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.EarlySupers"/> uses first.</summary>
    public static RoomLevelId EarlySupers { get; } = new(nameof(EarlySupers));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.BrinstarReserveTank"/> uses first.</summary>
    public static RoomLevelId BrinstarReserveTank { get; } = new(nameof(BrinstarReserveTank));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.GreenBrinstarMap"/> uses first.</summary>
    public static RoomLevelId GreenBrinstarMap { get; } = new(nameof(GreenBrinstarMap));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.GreenBrinstarFirefleas"/> uses first.</summary>
    public static RoomLevelId GreenBrinstarFirefleas { get; } = new(nameof(GreenBrinstarFirefleas));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.GreenBrinstarMissileRefill"/> uses first.</summary>
    public static RoomLevelId GreenBrinstarMissileRefill { get; } = new(nameof(GreenBrinstarMissileRefill));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.Dachora"/> uses first.</summary>
    public static RoomLevelId Dachora { get; } = new(nameof(Dachora));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.BigPink"/> uses first.</summary>
    public static RoomLevelId BigPink { get; } = new(nameof(BigPink));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.SporeSpawnKihunters"/> uses first.</summary>
    public static RoomLevelId SporeSpawnKihunters { get; } = new(nameof(SporeSpawnKihunters));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.SporeSpawn0"/> uses first.</summary>
    public static RoomLevelId SporeSpawn { get; } = new(nameof(SporeSpawn));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.PinkBrinstarPowerBombs"/> uses first.</summary>
    public static RoomLevelId PinkBrinstarPowerBombs { get; } = new(nameof(PinkBrinstarPowerBombs));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.GreenHillZone"/> uses first.</summary>
    public static RoomLevelId GreenHillZone { get; } = new(nameof(GreenHillZone));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.MorphBall0"/> uses first.</summary>
    public static RoomLevelId MorphBall { get; } = new(nameof(MorphBall));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.ConstructionZone0"/> uses first.</summary>
    public static RoomLevelId ConstructionZone { get; } = new(nameof(ConstructionZone));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.BlueBrinstarETank0"/> uses first.</summary>
    public static RoomLevelId BlueBrinstarETank { get; } = new(nameof(BlueBrinstarETank));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.NoobBridge"/> uses first.</summary>
    public static RoomLevelId NoobBridge { get; } = new(nameof(NoobBridge));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.GreenBrinstarBeetoms"/> uses first.</summary>
    public static RoomLevelId GreenBrinstarBeetoms { get; } = new(nameof(GreenBrinstarBeetoms));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.EtecoonETank"/> uses first.</summary>
    public static RoomLevelId EtecoonETank { get; } = new(nameof(EtecoonETank));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.EtecoonSuper"/> uses first.</summary>
    public static RoomLevelId EtecoonSuper { get; } = new(nameof(EtecoonSuper));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.DachoraEnergyRefill"/> uses first.</summary>
    public static RoomLevelId DachoraEnergyRefill { get; } = new(nameof(DachoraEnergyRefill));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.SporeSpawnFarming"/> uses first.</summary>
    public static RoomLevelId SporeSpawnFarming { get; } = new(nameof(SporeSpawnFarming));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.WaterwayETank"/> uses first.</summary>
    public static RoomLevelId WaterwayETank { get; } = new(nameof(WaterwayETank));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.FirstMissile"/> uses first.</summary>
    public static RoomLevelId FirstMissile { get; } = new(nameof(FirstMissile));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.PinkBrinstarHoppers"/> uses first.</summary>
    public static RoomLevelId PinkBrinstarHoppers { get; } = new(nameof(PinkBrinstarHoppers));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.HopperETank"/> uses first.</summary>
    public static RoomLevelId HopperETank { get; } = new(nameof(HopperETank));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.BlueBrinstarBoulders"/> uses first.</summary>
    public static RoomLevelId BlueBrinstarBoulders { get; } = new(nameof(BlueBrinstarBoulders));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.BillyMays"/> uses first.</summary>
    public static RoomLevelId BillyMays { get; } = new(nameof(BillyMays));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.RedTower"/> uses first.</summary>
    public static RoomLevelId RedTower { get; } = new(nameof(RedTower));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.RedBrinstarFirefleas"/> uses first.</summary>
    public static RoomLevelId RedBrinstarFirefleas { get; } = new(nameof(RedBrinstarFirefleas));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.XrayScope"/> uses first.</summary>
    public static RoomLevelId XrayScope { get; } = new(nameof(XrayScope));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.Hellway"/> uses first.</summary>
    public static RoomLevelId Hellway { get; } = new(nameof(Hellway));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.Caterpillar"/> uses first.</summary>
    public static RoomLevelId Caterpillar { get; } = new(nameof(Caterpillar));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.BetaPowerBomb"/> uses first.</summary>
    public static RoomLevelId BetaPowerBomb { get; } = new(nameof(BetaPowerBomb));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.AlphaPowerBomb"/> uses first.</summary>
    public static RoomLevelId AlphaPowerBomb { get; } = new(nameof(AlphaPowerBomb));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.SkreeBoost"/> uses first.</summary>
    public static RoomLevelId SkreeBoost { get; } = new(nameof(SkreeBoost));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.BelowSpazer"/> uses first.</summary>
    public static RoomLevelId BelowSpazer { get; } = new(nameof(BelowSpazer));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.Spazer"/> uses first.</summary>
    public static RoomLevelId Spazer { get; } = new(nameof(Spazer));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.WarehouseZeela"/> uses first.</summary>
    public static RoomLevelId WarehouseZeela { get; } = new(nameof(WarehouseZeela));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.WarehouseETank"/> uses first.</summary>
    public static RoomLevelId WarehouseETank { get; } = new(nameof(WarehouseETank));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.WarehouseKihunter"/> uses first.</summary>
    public static RoomLevelId WarehouseKihunter { get; } = new(nameof(WarehouseKihunter));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.MiniKraid0"/> uses first.</summary>
    public static RoomLevelId MiniKraid { get; } = new(nameof(MiniKraid));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.KraidEyeDoor"/> uses first.</summary>
    public static RoomLevelId KraidEyeDoor { get; } = new(nameof(KraidEyeDoor));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.Kraid0"/> uses first.</summary>
    public static RoomLevelId Kraid { get; } = new(nameof(Kraid));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.StatuesHallway"/> uses first.</summary>
    public static RoomLevelId StatuesHallway { get; } = new(nameof(StatuesHallway));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.KraidRefill"/> uses first.</summary>
    public static RoomLevelId KraidRefill { get; } = new(nameof(KraidRefill));

    /// <summary>The level the Crateria state <see cref="CrateriaRoomStateDefinitions.Statues"/> uses first.</summary>
    public static RoomLevelId Statues { get; } = new(nameof(Statues));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.WarehouseEntrance"/> uses first.</summary>
    public static RoomLevelId WarehouseEntrance { get; } = new(nameof(WarehouseEntrance));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.VariaSuit"/> uses first.</summary>
    public static RoomLevelId VariaSuit { get; } = new(nameof(VariaSuit));

    /// <summary>The level the Brinstar state <see cref="BrinstarRoomStateDefinitions.WarehouseSave"/> uses first.</summary>
    public static RoomLevelId WarehouseSave { get; } = new(nameof(WarehouseSave));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.IceBeamAcid"/> uses first.</summary>
    public static RoomLevelId IceBeamAcid { get; } = new(nameof(IceBeamAcid));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.Cathedral"/> uses first.</summary>
    public static RoomLevelId Cathedral { get; } = new(nameof(Cathedral));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.CathedralEntrance"/> uses first.</summary>
    public static RoomLevelId CathedralEntrance { get; } = new(nameof(CathedralEntrance));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.BusinessCenter"/> uses first.</summary>
    public static RoomLevelId BusinessCenter { get; } = new(nameof(BusinessCenter));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.IceBeamGate"/> uses first.</summary>
    public static RoomLevelId IceBeamGate { get; } = new(nameof(IceBeamGate));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.IceBeamTutorial"/> uses first.</summary>
    public static RoomLevelId IceBeamTutorial { get; } = new(nameof(IceBeamTutorial));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.IceBeam"/> uses first.</summary>
    public static RoomLevelId IceBeam { get; } = new(nameof(IceBeam));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.IceBeamSnake"/> uses first.</summary>
    public static RoomLevelId IceBeamSnake { get; } = new(nameof(IceBeamSnake));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.CrumbleShaft"/> uses first.</summary>
    public static RoomLevelId CrumbleShaft { get; } = new(nameof(CrumbleShaft));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.CrocomireSpeedway"/> uses first.</summary>
    public static RoomLevelId CrocomireSpeedway { get; } = new(nameof(CrocomireSpeedway));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.Crocomire0"/> uses first.</summary>
    public static RoomLevelId Crocomire { get; } = new(nameof(Crocomire));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.HiJumpBoots"/> uses first.</summary>
    public static RoomLevelId HiJumpBoots { get; } = new(nameof(HiJumpBoots));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.CrocomireEscape"/> uses first.</summary>
    public static RoomLevelId CrocomireEscape { get; } = new(nameof(CrocomireEscape));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.HiJumpETank"/> uses first.</summary>
    public static RoomLevelId HiJumpETank { get; } = new(nameof(HiJumpETank));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.PostCrocFarming"/> uses first.</summary>
    public static RoomLevelId PostCrocFarming { get; } = new(nameof(PostCrocFarming));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.PostCrocPowerBombs"/> uses first.</summary>
    public static RoomLevelId PostCrocPowerBombs { get; } = new(nameof(PostCrocPowerBombs));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.PostCrocShaft"/> uses first.</summary>
    public static RoomLevelId PostCrocShaft { get; } = new(nameof(PostCrocShaft));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.PostCrocMissile"/> uses first.</summary>
    public static RoomLevelId PostCrocMissile { get; } = new(nameof(PostCrocMissile));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.GrappleTutorial3"/> uses first.</summary>
    public static RoomLevelId GrappleTutorial3 { get; } = new(nameof(GrappleTutorial3));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.PostCrocJump"/> uses first.</summary>
    public static RoomLevelId PostCrocJump { get; } = new(nameof(PostCrocJump));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.GrappleTutorial2"/> uses first.</summary>
    public static RoomLevelId GrappleTutorial2 { get; } = new(nameof(GrappleTutorial2));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.GrappleTutorial1"/> uses first.</summary>
    public static RoomLevelId GrappleTutorial1 { get; } = new(nameof(GrappleTutorial1));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.GrappleBeam"/> uses first.</summary>
    public static RoomLevelId GrappleBeam { get; } = new(nameof(GrappleBeam));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.NorfairReserveTank"/> uses first.</summary>
    public static RoomLevelId NorfairReserveTank { get; } = new(nameof(NorfairReserveTank));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.GreenBubblesMissiles"/> uses first.</summary>
    public static RoomLevelId GreenBubblesMissiles { get; } = new(nameof(GreenBubblesMissiles));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.BubbleMountain"/> uses first.</summary>
    public static RoomLevelId BubbleMountain { get; } = new(nameof(BubbleMountain));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.SpeedBoosterHall"/> uses first.</summary>
    public static RoomLevelId SpeedBoosterHall { get; } = new(nameof(SpeedBoosterHall));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.SpeedBooster"/> uses first.</summary>
    public static RoomLevelId SpeedBooster { get; } = new(nameof(SpeedBooster));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.SingleChamber"/> uses first.</summary>
    public static RoomLevelId SingleChamber { get; } = new(nameof(SingleChamber));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.DoubleChamber"/> uses first.</summary>
    public static RoomLevelId DoubleChamber { get; } = new(nameof(DoubleChamber));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.WaveBeam"/> uses first.</summary>
    public static RoomLevelId WaveBeam { get; } = new(nameof(WaveBeam));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.SpikyPlatformsTunnel"/> uses first.</summary>
    public static RoomLevelId SpikyPlatformsTunnel { get; } = new(nameof(SpikyPlatformsTunnel));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.Volcano"/> uses first.</summary>
    public static RoomLevelId Volcano { get; } = new(nameof(Volcano));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.KronicBoost"/> uses first.</summary>
    public static RoomLevelId KronicBoost { get; } = new(nameof(KronicBoost));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.MagdolliteTunnel"/> uses first.</summary>
    public static RoomLevelId MagdolliteTunnel { get; } = new(nameof(MagdolliteTunnel));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.PurpleShaft"/> uses first.</summary>
    public static RoomLevelId PurpleShaft { get; } = new(nameof(PurpleShaft));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.LavaDive"/> uses first.</summary>
    public static RoomLevelId LavaDive { get; } = new(nameof(LavaDive));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.LowerNorfairElev"/> uses first.</summary>
    public static RoomLevelId LowerNorfairElev { get; } = new(nameof(LowerNorfairElev));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.UpperNorfairFarming"/> uses first.</summary>
    public static RoomLevelId UpperNorfairFarming { get; } = new(nameof(UpperNorfairFarming));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.RisingTide"/> uses first.</summary>
    public static RoomLevelId RisingTide { get; } = new(nameof(RisingTide));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.AcidSnakesTunnel"/> uses first.</summary>
    public static RoomLevelId AcidSnakesTunnel { get; } = new(nameof(AcidSnakesTunnel));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.SpikyAcidSnakesTunnel"/> uses first.</summary>
    public static RoomLevelId SpikyAcidSnakesTunnel { get; } = new(nameof(SpikyAcidSnakesTunnel));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.UpperNorfairRefill"/> uses first.</summary>
    public static RoomLevelId UpperNorfairRefill { get; } = new(nameof(UpperNorfairRefill));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.PurpleFarming"/> uses first.</summary>
    public static RoomLevelId PurpleFarming { get; } = new(nameof(PurpleFarming));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.BatCave"/> uses first.</summary>
    public static RoomLevelId BatCave { get; } = new(nameof(BatCave));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.FrogSpeedway"/> uses first.</summary>
    public static RoomLevelId FrogSpeedway { get; } = new(nameof(FrogSpeedway));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.RedPirateShaft"/> uses first.</summary>
    public static RoomLevelId RedPirateShaft { get; } = new(nameof(RedPirateShaft));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.FrogSave"/> uses first.</summary>
    public static RoomLevelId FrogSave { get; } = new(nameof(FrogSave));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.AcidStatue"/> uses first.</summary>
    public static RoomLevelId AcidStatue { get; } = new(nameof(AcidStatue));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.MainHall"/> uses first.</summary>
    public static RoomLevelId MainHall { get; } = new(nameof(MainHall));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.GoldenTorizo0"/> uses first.</summary>
    public static RoomLevelId GoldenTorizo { get; } = new(nameof(GoldenTorizo));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.FastRipper"/> uses first.</summary>
    public static RoomLevelId FastRipper { get; } = new(nameof(FastRipper));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.GTEnergyRefill"/> uses first.</summary>
    public static RoomLevelId GTEnergyRefill { get; } = new(nameof(GTEnergyRefill));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.Ridley0"/> uses first.</summary>
    public static RoomLevelId Ridley { get; } = new(nameof(Ridley));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.LNFarming"/> uses first.</summary>
    public static RoomLevelId LNFarming { get; } = new(nameof(LNFarming));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.FastPillarsSetup"/> uses first.</summary>
    public static RoomLevelId FastPillarsSetup { get; } = new(nameof(FastPillarsSetup));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.UnusedRoom8FB3EE"/> uses first.</summary>
    public static RoomLevelId UnusedNorfairRoom { get; } = new(nameof(UnusedNorfairRoom));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.MickeyMouse"/> uses first.</summary>
    public static RoomLevelId MickeyMouse { get; } = new(nameof(MickeyMouse));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.Pillar"/> uses first.</summary>
    public static RoomLevelId Pillar { get; } = new(nameof(Pillar));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.Plowerhouse"/> uses first.</summary>
    public static RoomLevelId Plowerhouse { get; } = new(nameof(Plowerhouse));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.WorstRoomInTheGame"/> uses first.</summary>
    public static RoomLevelId WorstRoomInTheGame { get; } = new(nameof(WorstRoomInTheGame));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.Amphitheatre"/> uses first.</summary>
    public static RoomLevelId Amphitheatre { get; } = new(nameof(Amphitheatre));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.LNSpringBallMaze"/> uses first.</summary>
    public static RoomLevelId LNSpringBallMaze { get; } = new(nameof(LNSpringBallMaze));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.LNEscapePowerBombs"/> uses first.</summary>
    public static RoomLevelId LNEscapePowerBombs { get; } = new(nameof(LNEscapePowerBombs));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.RedKihunterShaft"/> uses first.</summary>
    public static RoomLevelId RedKihunterShaft { get; } = new(nameof(RedKihunterShaft));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.Wasteland"/> uses first.</summary>
    public static RoomLevelId Wasteland { get; } = new(nameof(Wasteland));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.MetalPirates"/> uses first.</summary>
    public static RoomLevelId MetalPirates { get; } = new(nameof(MetalPirates));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.ThreeMusketeers"/> uses first.</summary>
    public static RoomLevelId ThreeMusketeers { get; } = new(nameof(ThreeMusketeers));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.RidleyETank"/> uses first.</summary>
    public static RoomLevelId RidleyETank { get; } = new(nameof(RidleyETank));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.ScrewAttack"/> uses first.</summary>
    public static RoomLevelId ScrewAttack { get; } = new(nameof(ScrewAttack));

    /// <summary>The level the Norfair state <see cref="NorfairRoomStateDefinitions.LNFireflea"/> uses first.</summary>
    public static RoomLevelId LNFireflea { get; } = new(nameof(LNFireflea));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.BowlingAlley0"/> uses first.</summary>
    public static RoomLevelId BowlingAlley { get; } = new(nameof(BowlingAlley));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.WreckedShipEntrance0"/> uses first.</summary>
    public static RoomLevelId WreckedShipEntrance { get; } = new(nameof(WreckedShipEntrance));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.Attic0"/> uses first.</summary>
    public static RoomLevelId Attic0 { get; } = new(nameof(Attic0));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.Attic1"/> uses first.</summary>
    public static RoomLevelId Attic1 { get; } = new(nameof(Attic1));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.AssemblyLine0"/> uses first.</summary>
    public static RoomLevelId AssemblyLine { get; } = new(nameof(AssemblyLine));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.WreckedShipMainShaft0"/> uses first.</summary>
    public static RoomLevelId WreckedShipMainShaft0 { get; } = new(nameof(WreckedShipMainShaft0));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.WreckedShipMainShaft1"/> uses first.</summary>
    public static RoomLevelId WreckedShipMainShaft1 { get; } = new(nameof(WreckedShipMainShaft1));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.SpikyDeath0"/> uses first.</summary>
    public static RoomLevelId SpikyDeath { get; } = new(nameof(SpikyDeath));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.ElectricDeath0"/> uses first.</summary>
    public static RoomLevelId ElectricDeath { get; } = new(nameof(ElectricDeath));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.WreckedShipETank0"/> uses first.</summary>
    public static RoomLevelId WreckedShipETank { get; } = new(nameof(WreckedShipETank));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.Basement0"/> uses first.</summary>
    public static RoomLevelId Basement { get; } = new(nameof(Basement));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.Phantoon0"/> uses first.</summary>
    public static RoomLevelId Phantoon { get; } = new(nameof(Phantoon));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.SpongeBath0"/> uses first.</summary>
    public static RoomLevelId SpongeBath { get; } = new(nameof(SpongeBath));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.WSWestSuper0"/> uses first.</summary>
    public static RoomLevelId WSWestSuper { get; } = new(nameof(WSWestSuper));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.WSEastSuper0"/> uses first.</summary>
    public static RoomLevelId WSEastSuper0 { get; } = new(nameof(WSEastSuper0));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.WSEastSuper1"/> uses first.</summary>
    public static RoomLevelId WSEastSuper1 { get; } = new(nameof(WSEastSuper1));

    /// <summary>The level the WreckedShip state <see cref="WreckedShipRoomStateDefinitions.GravitySuit0"/> uses first.</summary>
    public static RoomLevelId GravitySuit { get; } = new(nameof(GravitySuit));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.GlassTunnel0"/> uses first.</summary>
    public static RoomLevelId GlassTunnel0 { get; } = new(nameof(GlassTunnel0));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.GlassTunnel1"/> uses first.</summary>
    public static RoomLevelId GlassTunnel1 { get; } = new(nameof(GlassTunnel1));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.WestTunnel"/> uses first.</summary>
    public static RoomLevelId WestTunnel { get; } = new(nameof(WestTunnel));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.EastTunnel"/> uses first.</summary>
    public static RoomLevelId EastTunnel { get; } = new(nameof(EastTunnel));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.MainStreet"/> uses first.</summary>
    public static RoomLevelId MainStreet { get; } = new(nameof(MainStreet));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.FishTank"/> uses first.</summary>
    public static RoomLevelId FishTank { get; } = new(nameof(FishTank));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.MamaTurtle"/> uses first.</summary>
    public static RoomLevelId MamaTurtle { get; } = new(nameof(MamaTurtle));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.CrabTunnel"/> uses first.</summary>
    public static RoomLevelId CrabTunnel { get; } = new(nameof(CrabTunnel));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.MtEverest"/> uses first.</summary>
    public static RoomLevelId MtEverest { get; } = new(nameof(MtEverest));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.RedFish"/> uses first.</summary>
    public static RoomLevelId RedFish { get; } = new(nameof(RedFish));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.WateringHole"/> uses first.</summary>
    public static RoomLevelId WateringHole { get; } = new(nameof(WateringHole));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.NWestMaridiaBug"/> uses first.</summary>
    public static RoomLevelId NWestMaridiaBug { get; } = new(nameof(NWestMaridiaBug));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.CrabShaft"/> uses first.</summary>
    public static RoomLevelId CrabShaft { get; } = new(nameof(CrabShaft));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.PseudoPlasmaSpark"/> uses first.</summary>
    public static RoomLevelId PseudoPlasmaSpark { get; } = new(nameof(PseudoPlasmaSpark));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.CrabHole"/> uses first.</summary>
    public static RoomLevelId CrabHole { get; } = new(nameof(CrabHole));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.WestSandHallTunnel"/> uses first.</summary>
    public static RoomLevelId WestSandHallTunnel { get; } = new(nameof(WestSandHallTunnel));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.PlasmaTutorial"/> uses first.</summary>
    public static RoomLevelId PlasmaTutorial { get; } = new(nameof(PlasmaTutorial));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Plasma"/> uses first.</summary>
    public static RoomLevelId Plasma { get; } = new(nameof(Plasma));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.ThreadTheNeedle"/> uses first.</summary>
    public static RoomLevelId ThreadTheNeedle { get; } = new(nameof(ThreadTheNeedle));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.MaridiaElev"/> uses first.</summary>
    public static RoomLevelId MaridiaElev { get; } = new(nameof(MaridiaElev));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.PlasmaSpark"/> uses first.</summary>
    public static RoomLevelId PlasmaSpark { get; } = new(nameof(PlasmaSpark));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Kassiuz"/> uses first.</summary>
    public static RoomLevelId Kassiuz { get; } = new(nameof(Kassiuz));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Toilet"/> uses first.</summary>
    public static RoomLevelId Toilet { get; } = new(nameof(Toilet));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.BugSandHole"/> uses first.</summary>
    public static RoomLevelId BugSandHole { get; } = new(nameof(BugSandHole));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.WestSandHall"/> uses first.</summary>
    public static RoomLevelId WestSandHall { get; } = new(nameof(WestSandHall));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Oasis"/> uses first.</summary>
    public static RoomLevelId Oasis { get; } = new(nameof(Oasis));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.EastSandHall"/> uses first.</summary>
    public static RoomLevelId EastSandHall { get; } = new(nameof(EastSandHall));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.WestSandHole"/> uses first.</summary>
    public static RoomLevelId WestSandHole { get; } = new(nameof(WestSandHole));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.EastSandHole"/> uses first.</summary>
    public static RoomLevelId EastSandHole { get; } = new(nameof(EastSandHole));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.WestAqueductQuicksand"/> uses first.</summary>
    public static RoomLevelId WestAqueductQuicksand { get; } = new(nameof(WestAqueductQuicksand));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.EastAqueductQuicksand"/> uses first.</summary>
    public static RoomLevelId EastAqueductQuicksand { get; } = new(nameof(EastAqueductQuicksand));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Aqueduct"/> uses first.</summary>
    public static RoomLevelId Aqueduct { get; } = new(nameof(Aqueduct));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Butterfly"/> uses first.</summary>
    public static RoomLevelId Butterfly { get; } = new(nameof(Butterfly));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.BotwoonHallway"/> uses first.</summary>
    public static RoomLevelId BotwoonHallway { get; } = new(nameof(BotwoonHallway));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Pants"/> uses first.</summary>
    public static RoomLevelId Pants { get; } = new(nameof(Pants));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.EastPants"/> uses first.</summary>
    public static RoomLevelId EastPants { get; } = new(nameof(EastPants));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Springball"/> uses first.</summary>
    public static RoomLevelId Springball { get; } = new(nameof(Springball));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.BelowBotwoonETank"/> uses first.</summary>
    public static RoomLevelId BelowBotwoonETank { get; } = new(nameof(BelowBotwoonETank));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Colosseum"/> uses first.</summary>
    public static RoomLevelId Colosseum { get; } = new(nameof(Colosseum));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.ThePrecious0"/> uses first.</summary>
    public static RoomLevelId ThePrecious { get; } = new(nameof(ThePrecious));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.BotwoonETank"/> uses first.</summary>
    public static RoomLevelId BotwoonETank { get; } = new(nameof(BotwoonETank));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.MaridiaMissileRefill"/> uses first.</summary>
    public static RoomLevelId MaridiaMissileRefill { get; } = new(nameof(MaridiaMissileRefill));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.PlasmaBeachQuicksand"/> uses first.</summary>
    public static RoomLevelId PlasmaBeachQuicksand { get; } = new(nameof(PlasmaBeachQuicksand));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.BotwoonQuicksand"/> uses first.</summary>
    public static RoomLevelId BotwoonQuicksand { get; } = new(nameof(BotwoonQuicksand));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Shaktool0"/> uses first.</summary>
    public static RoomLevelId Shaktool0 { get; } = new(nameof(Shaktool0));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Shaktool1"/> uses first.</summary>
    public static RoomLevelId Shaktool1 { get; } = new(nameof(Shaktool1));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.HalfieClimb"/> uses first.</summary>
    public static RoomLevelId HalfieClimb { get; } = new(nameof(HalfieClimb));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Botwoon0"/> uses first.</summary>
    public static RoomLevelId Botwoon { get; } = new(nameof(Botwoon));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.SpaceJump"/> uses first.</summary>
    public static RoomLevelId SpaceJump { get; } = new(nameof(SpaceJump));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.WestCactusAlley"/> uses first.</summary>
    public static RoomLevelId WestCactusAlley { get; } = new(nameof(WestCactusAlley));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.EastCactusAlley"/> uses first.</summary>
    public static RoomLevelId EastCactusAlley { get; } = new(nameof(EastCactusAlley));

    /// <summary>The level the Maridia state <see cref="MaridiaRoomStateDefinitions.Draygon0"/> uses first.</summary>
    public static RoomLevelId Draygon { get; } = new(nameof(Draygon));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.TourianFirst"/> uses first.</summary>
    public static RoomLevelId TourianFirst { get; } = new(nameof(TourianFirst));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.Metroids10"/> uses first.</summary>
    public static RoomLevelId Metroids1 { get; } = new(nameof(Metroids1));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.Metroids20"/> uses first.</summary>
    public static RoomLevelId Metroids2 { get; } = new(nameof(Metroids2));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.Metroids30"/> uses first.</summary>
    public static RoomLevelId Metroids3 { get; } = new(nameof(Metroids3));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.Metroids40"/> uses first.</summary>
    public static RoomLevelId Metroids4 { get; } = new(nameof(Metroids4));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.BlueHopper0"/> uses first.</summary>
    public static RoomLevelId BlueHopper { get; } = new(nameof(BlueHopper));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.DustTorizo0"/> uses first.</summary>
    public static RoomLevelId DustTorizo { get; } = new(nameof(DustTorizo));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.BigBoy0"/> uses first.</summary>
    public static RoomLevelId BigBoy { get; } = new(nameof(BigBoy));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.Seaweed"/> uses first.</summary>
    public static RoomLevelId Seaweed { get; } = new(nameof(Seaweed));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.TourianRecharge"/> uses first.</summary>
    public static RoomLevelId TourianRecharge { get; } = new(nameof(TourianRecharge));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.MotherBrain0"/> uses first.</summary>
    public static RoomLevelId MotherBrain { get; } = new(nameof(MotherBrain));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.TourianEyeDoor"/> uses first.</summary>
    public static RoomLevelId TourianEyeDoor { get; } = new(nameof(TourianEyeDoor));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.RinkaShaft"/> uses first.</summary>
    public static RoomLevelId RinkaShaft { get; } = new(nameof(RinkaShaft));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.TourianEscape1"/> uses first.</summary>
    public static RoomLevelId TourianEscape1 { get; } = new(nameof(TourianEscape1));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.TourianEscape2"/> uses first.</summary>
    public static RoomLevelId TourianEscape2 { get; } = new(nameof(TourianEscape2));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.TourianEscape3"/> uses first.</summary>
    public static RoomLevelId TourianEscape3 { get; } = new(nameof(TourianEscape3));

    /// <summary>The level the Tourian state <see cref="TourianRoomStateDefinitions.TourianEscape4"/> uses first.</summary>
    public static RoomLevelId TourianEscape4 { get; } = new(nameof(TourianEscape4));

    /// <summary>The level the Ceres state <see cref="CeresRoomStateDefinitions.CeresElev0"/> uses first.</summary>
    public static RoomLevelId CeresElev { get; } = new(nameof(CeresElev));

    /// <summary>The level the Ceres state <see cref="CeresRoomStateDefinitions.FallingTile0"/> uses first.</summary>
    public static RoomLevelId FallingTile { get; } = new(nameof(FallingTile));

    /// <summary>The level the Ceres state <see cref="CeresRoomStateDefinitions.MagnetStairs0"/> uses first.</summary>
    public static RoomLevelId MagnetStairs { get; } = new(nameof(MagnetStairs));

    /// <summary>The level the Ceres state <see cref="CeresRoomStateDefinitions.DeadScientist0"/> uses first.</summary>
    public static RoomLevelId DeadScientist { get; } = new(nameof(DeadScientist));

    /// <summary>The level the Ceres state <see cref="CeresRoomStateDefinitions.Room58Escape0"/> uses first.</summary>
    public static RoomLevelId Room58Escape { get; } = new(nameof(Room58Escape));

    /// <summary>The level the Ceres state <see cref="CeresRoomStateDefinitions.CeresRidley0"/> uses first.</summary>
    public static RoomLevelId CeresRidley { get; } = new(nameof(CeresRidley));
}
