using static SuperMetroid.Core.DefinitionRun;
namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The scroll data room states select: 159 explicit tables and two implicit layouts.</summary>
[NumberedIdentities("Each numbered member is the scroll layout of a distinct numbered room (Metroids1-4, TourianEscape1-4), named for that room.")]
public static class RoomScrollDefinitions
{
    /// <summary>A screen closed to the camera.</summary>
    private const RoomScrollState Closed = RoomScrollState.Closed;

    /// <summary>A screen open to the camera, which aligns to the screen's top.</summary>
    private const RoomScrollState Top = RoomScrollState.OpenAlignedToTop;

    /// <summary>A screen open to the camera.</summary>
    private const RoomScrollState Open = RoomScrollState.Open;

    /// <summary>The layout of every room of two screens both open with the camera aligned to the screen's top.</summary>
    private static readonly RoomScrollState[] BothTop = [Top, Top];

    /// <summary>The layout of every room of two screens whose first is open with the camera aligned to its top and whose second is closed.</summary>
    private static readonly RoomScrollState[] TopThenClosed = [Top, Closed];

    /// <summary>The layout of every room of four screens whose first two are open with the camera aligned to the screen's top and whose last two are closed.</summary>
    private static readonly RoomScrollState[] TwoTopThenTwoClosed = [Top, Top, Closed, Closed];

    /// <summary>The layout of every room of four screens whose first two are closed and whose last two are open with the camera aligned to the screen's top.</summary>
    private static readonly RoomScrollState[] TwoClosedThenTwoTop = [Closed, Closed, Top, Top];

    /// <summary>The layout of every room of four screens whose first two are open and whose last two are open with the camera aligned to the screen's top.</summary>
    private static readonly RoomScrollState[] TwoOpenThenTwoTop = [Open, Open, Top, Top];

    /// <summary>The layout of every room of three screens whose first two are open and whose last is open with the camera aligned to its top.</summary>
    private static readonly RoomScrollState[] TwoOpenThenTop = [Open, Open, Top];

    /// <summary>The layout of every room of two screens whose first is closed and whose second is open with the camera aligned to its top.</summary>
    private static readonly RoomScrollState[] ClosedThenTop = [Closed, Top];

    /// <summary>The layout of every room of two screens both open.</summary>
    private static readonly RoomScrollState[] BothOpen = [Open, Open];

    /// <summary>The layout of every room of two screens whose first is open and whose second is open with the camera aligned to its top.</summary>
    private static readonly RoomScrollState[] OpenThenTop = [Open, Top];

    /// <summary>The implicit layout whose bottom row is open with the camera aligned to the screen's top, and every other row open.</summary>
    public static RoomScrollDefinition OpenWithTopAlignedBottomRow { get; } = RoomScrollDefinition.Implicit("OpenWithTopAlignedBottomRow", RoomScrollState.OpenAlignedToTop);

    /// <summary>The implicit layout whose every row is open.</summary>
    public static RoomScrollDefinition AllOpen { get; } = RoomScrollDefinition.Implicit("AllOpen", RoomScrollState.Open);

    /// <summary>The Landing Site room's screen scroll layout.</summary>
    public static RoomScrollDefinition LandingSite { get; } = new("LandingSite",
    [
        Closed, Closed, .. Repeated(Open, 7),
        Closed, Closed, .. Repeated(Open, 7),
        Top, Closed, .. Repeated(Open, 7),
        Closed, Closed, .. Repeated(Open, 7),
        .. Repeated(Top, 9),
    ]);
    /// <summary>The Parlor room's screen scroll layout.</summary>
    public static RoomScrollDefinition Parlor { get; } = new("Parlor",
    [
        Closed, .. Repeated(Top, 4),
        .. Repeated(Closed, 6), Open, Closed, Top, Closed,
        Closed, Open, .. Repeated(Closed, 4), Open, .. Repeated(Closed, 3),
    ]);
    /// <summary>The Crateria Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaPowerBombs { get; } = new("CrateriaPowerBombs", BothTop);
    /// <summary>The West Ocean room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestOcean { get; } = new("WestOcean",
    [
        .. Repeated(Open, 19), Closed, Closed, Open, Closed, Closed,
        .. Repeated(Open, 6), Closed, Closed,
        .. Repeated(Open, 8),
        .. Repeated(Top, 8),
    ]);
    /// <summary>The Crateria Kihunter room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaKihunter { get; } = new("CrateriaKihunter",
    [
        .. Repeated(Top, 3),
        .. Repeated(Closed, 4), Open, Closed,
    ]);
    /// <summary>The Forgotten Highway Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition ForgottenHighwayElevator { get; } = new("ForgottenHighwayElevator",
    [
        Top,
    ]);
    /// <summary>The East Ocean room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastOcean { get; } = new("EastOcean",
    [
        .. Repeated(Closed, 28),
        .. Repeated(Open, 7),
        .. Repeated(Top, 7),
    ]);
    /// <summary>The Forgotten Highway Elbow room's screen scroll layout.</summary>
    public static RoomScrollDefinition ForgottenHighwayElbow { get; } = new("ForgottenHighwayElbow",
    [
        Open,
    ]);
    /// <summary>The Red Brinstar Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedBrinstarElevator { get; } = new("RedBrinstarElevator",
    [
        Top,
    ]);
    /// <summary>The Gauntlet Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition GauntletEnergyTank { get; } = new("GauntletEnergyTank",
    [
        .. Repeated(Top, 4), Closed, Top,
    ]);
    /// <summary>The Climb room's screen scroll layout.</summary>
    public static RoomScrollDefinition Climb { get; } = new("Climb",
    [
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Top, Closed,
    ]);
    /// <summary>The Pit room's screen scroll layout.</summary>
    public static RoomScrollDefinition Pit { get; } = new("Pit",
    [
        .. Repeated(Top, 3),
        .. Repeated(Closed, 3),
    ]);
    /// <summary>The Elevator to Blue Brinstar room's screen scroll layout.</summary>
    public static RoomScrollDefinition ElevatorToBlueBrinstar { get; } = new("ElevatorToBlueBrinstar",
    [
        Top,
    ]);
    /// <summary>The Green Brinstar Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarElevator { get; } = new("GreenBrinstarElevator",
    [
        Top,
    ]);
    /// <summary>The Green Pirates Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenPiratesShaft { get; } = new("GreenPiratesShaft",
    [
        .. Repeated(Open, 3),
        Closed,
        Open,
        Open,
        Top,
    ]);
    /// <summary>The Crateria Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaSuper { get; } = new("CrateriaSuper",
    [
        .. Repeated(Top, 3), Open,
        .. Repeated(Closed, 3), Open,
        .. Repeated(Closed, 3), Open,
        .. Repeated(Closed, 3), Open,
        .. Repeated(Closed, 3), Open,
        .. Repeated(Closed, 3), Open,
        .. Repeated(Closed, 3), Open,
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Final Missile Bombway room's screen scroll layout.</summary>
    public static RoomScrollDefinition FinalMissileBombway { get; } = new("FinalMissileBombway", BothTop);
    /// <summary>The Final Missile room's screen scroll layout.</summary>
    public static RoomScrollDefinition FinalMissile { get; } = new("FinalMissile",
    [
        Top,
    ]);
    /// <summary>The Green Brinstar Main Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarMainShaft { get; } = new("GreenBrinstarMainShaft",
    [
        Open, .. Repeated(Closed, 3),
        Open, .. Repeated(Closed, 3),
        Open, .. Repeated(Closed, 3),
        Open, .. Repeated(Closed, 3),
        Open, .. Repeated(Closed, 3),
        Open, .. Repeated(Closed, 3),
        Open, .. Repeated(Closed, 5), Open, Top,
        Open, Closed, Open, Closed,
        Open, Closed, Open, Closed,
        Open, Closed, Open, .. Repeated(Closed, 3), Open, Closed,
    ]);
    /// <summary>The Spore Spawn Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition SporeSpawnSuper { get; } = new("SporeSpawnSuper",
    [
        Top, Open,
        Closed, Open,
        Closed, Open,
        Closed, Open,
        Closed, Open,
        Closed, Open,
        Closed, Open,
        Closed, Open,
        Top, Top,
    ]);
    /// <summary>The Early Supers room's screen scroll layout.</summary>
    public static RoomScrollDefinition EarlySupers { get; } = new("EarlySupers",
    [
        .. Repeated(Closed, 3),
        .. Repeated(Top, 3),
    ]);
    /// <summary>The Brinstar Reserve Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BrinstarReserveTank { get; } = new("BrinstarReserveTank", TopThenClosed);
    /// <summary>The Green Brinstar Missile Refill room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarMissileRefill { get; } = new("GreenBrinstarMissileRefill",
    [
        Top,
    ]);
    /// <summary>The Dachora room's screen scroll layout.</summary>
    public static RoomScrollDefinition Dachora { get; } = new("Dachora",
    [
        .. Repeated(Top, 7),
        .. Repeated(Closed, 11), Open, .. Repeated(Closed, 6), Open, .. Repeated(Closed, 6), Open, .. Repeated(Closed, 6), Open, Closed, Closed,
        .. Repeated(Open, 5), Closed, Closed,
    ]);
    /// <summary>The Big Pink room's screen scroll layout.</summary>
    public static RoomScrollDefinition BigPink { get; } = new("BigPink",
    [
        Closed, Closed, Open, Open, .. Repeated(Closed, 3), Open, Open, .. Repeated(Closed, 3), Open, Open, .. Repeated(Closed, 3), Open, Open, .. Repeated(Closed, 3), Open, Open, .. Repeated(Closed, 3), Open, Open, .. Repeated(Closed, 3), Top, Top, Closed,
        Open, .. Repeated(Closed, 4),
        Open, .. Repeated(Closed, 4),
        Open, .. Repeated(Closed, 4),
    ]);
    /// <summary>The Pink Brinstar Power Bombs room's screen scroll layout.</summary>
    public static RoomScrollDefinition PinkBrinstarPowerBombs { get; } = new("PinkBrinstarPowerBombs", TwoTopThenTwoClosed);
    /// <summary>The Green Hill Zone room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenHillZone { get; } = new("GreenHillZone",
    [
        Open, Open, .. Repeated(Closed, 6),
        .. Repeated(Open, 4), .. Repeated(Closed, 4),
        .. Repeated(Open, 8),
        .. Repeated(Top, 8),
    ]);
    /// <summary>The Morph Ball room's screen scroll layout.</summary>
    public static RoomScrollDefinition MorphBall { get; } = new("MorphBall",
    [
        .. Repeated(Closed, 5), Open, .. Repeated(Closed, 10),
        .. Repeated(Top, 8),
    ]);
    /// <summary>The Construction Zone room's screen scroll layout.</summary>
    public static RoomScrollDefinition ConstructionZone { get; } = new("ConstructionZone", TopThenClosed);
    /// <summary>The Blue Brinstar Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BlueBrinstarEnergyTank { get; } = new("BlueBrinstarEnergyTank",
    [
        Closed, Closed, Open,
        .. Repeated(Closed, 3),
        .. Repeated(Top, 3),
    ]);
    /// <summary>The Green Brinstar Beetoms room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarBeetoms { get; } = new("GreenBrinstarBeetoms",
    [
        Top,
    ]);
    /// <summary>The Etecoon Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition EtecoonEnergyTank { get; } = new("EtecoonEnergyTank",
    [
        Top, Top, .. Repeated(Closed, 5), .. Repeated(Top, 3),
    ]);
    /// <summary>The Etecoon Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition EtecoonSuper { get; } = new("EtecoonSuper",
    [
        Top,
    ]);
    /// <summary>The Spore Spawn Farming room's screen scroll layout.</summary>
    public static RoomScrollDefinition SporeSpawnFarming { get; } = new("SporeSpawnFarming",
    [
        .. Repeated(Top, 3),
    ]);
    /// <summary>The Waterway Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition WaterwayEnergyTank { get; } = new("WaterwayEnergyTank",
    [
        Closed, .. Repeated(Top, 6),
    ]);
    /// <summary>The Red Tower room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedTower { get; } = new("RedTower",
    [
        .. Repeated(Open, 6),
        Top,
        Closed,
        Open,
        Open,
    ]);
    /// <summary>The Red Brinstar Firefleas room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedBrinstarFirefleas { get; } = new("RedBrinstarFirefleas",
    [
        Open, Open, Top, Top, Open, Open, .. Repeated(Top, 4), Closed, Closed, Top, Top, Closed, Closed,
    ]);
    /// <summary>The Caterpillar room's screen scroll layout.</summary>
    public static RoomScrollDefinition Caterpillar { get; } = new("Caterpillar",
    [
        Open, Closed, Closed,
        Open, Closed, Closed,
        Open, Closed, Closed,
        Open, Closed, Top,
        Open, Closed, Closed,
        Top, .. Repeated(Closed, 5),
        Open, Closed, Closed,
    ]);
    /// <summary>The Beta Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition BetaPowerBomb { get; } = new("BetaPowerBomb", TwoTopThenTwoClosed);
    /// <summary>The Alpha Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition AlphaPowerBomb { get; } = new("AlphaPowerBomb",
    [
        Closed, Top, Top,
    ]);
    /// <summary>The Below Spazer room's screen scroll layout.</summary>
    public static RoomScrollDefinition BelowSpazer { get; } = new("BelowSpazer", TwoClosedThenTwoTop);
    /// <summary>The Spazer room's screen scroll layout.</summary>
    public static RoomScrollDefinition Spazer { get; } = new("Spazer",
    [
        Top,
    ]);
    /// <summary>The Warehouse Zeela room's screen scroll layout.</summary>
    public static RoomScrollDefinition WarehouseZeela { get; } = new("WarehouseZeela",
    [
        Top, Closed,
        Closed, Top,
    ]);
    /// <summary>The Warehouse Kihunter room's screen scroll layout.</summary>
    public static RoomScrollDefinition WarehouseKihunter { get; } = new("WarehouseKihunter",
    [
        Open, Top, Top, Closed,
        Closed, Top, Closed, Closed,
    ]);
    /// <summary>The Kraid Eye Door room's screen scroll layout.</summary>
    public static RoomScrollDefinition KraidEyeDoor { get; } = new("KraidEyeDoor", TwoClosedThenTwoTop);
    /// <summary>The Kraid room's screen scroll layout.</summary>
    public static RoomScrollDefinition Kraid { get; } = new("Kraid", TwoOpenThenTwoTop);
    /// <summary>The Statues room's screen scroll layout.</summary>
    public static RoomScrollDefinition Statues { get; } = new("Statues", TopThenClosed);
    /// <summary>The Warehouse Entrance room's screen scroll layout.</summary>
    public static RoomScrollDefinition WarehouseEntrance { get; } = new("WarehouseEntrance",
    [
        Top, Closed, Open,
        Closed, Top, Top,
    ]);
    /// <summary>The Ice Beam Gate room's screen scroll layout.</summary>
    public static RoomScrollDefinition IceBeamGate { get; } = new("IceBeamGate",
    [
        .. Repeated(Closed, 3), Open, .. Repeated(Closed, 13), .. Repeated(Top, 7), .. Repeated(Closed, 4),
    ]);
    /// <summary>The Ice Beam Snake room's screen scroll layout.</summary>
    public static RoomScrollDefinition IceBeamSnake { get; } = new("IceBeamSnake",
    [
        Open, Closed,
        Open, Closed,
        Top, Closed,
    ]);
    /// <summary>The Crocomire Speedway room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrocomireSpeedway { get; } = new("CrocomireSpeedway",
    [
        .. Repeated(Top, 3), Open, Open, .. Repeated(Closed, 7), Open,
        .. Repeated(Closed, 3), .. Repeated(Open, 4), .. Repeated(Closed, 5), Open,
        .. Repeated(Closed, 5), .. Repeated(Open, 6), Closed, Open,
    ]);
    /// <summary>The Crocomire room's screen scroll layout.</summary>
    public static RoomScrollDefinition Crocomire { get; } = new("Crocomire",
    [
        Closed, Closed, .. Repeated(Top, 6),
    ]);
    /// <summary>The Crocomire Escape room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrocomireEscape { get; } = new("CrocomireEscape",
    [
        .. Repeated(Open, 4),
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Hi-Jump Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition HiJumpEnergyTank { get; } = new("HiJumpEnergyTank",
    [
        Closed, Top,
        Closed, Closed,
    ]);
    /// <summary>The Post-Crocomire Farming room's screen scroll layout.</summary>
    public static RoomScrollDefinition PostCrocomireFarming { get; } = new("PostCrocomireFarming",
    [
        .. Repeated(Open, 4),
    ]);
    /// <summary>The Post-Crocomire Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition PostCrocomireShaft { get; } = new("PostCrocomireShaft",
    [
        .. Repeated(Open, 5),
    ]);
    /// <summary>The Post-Crocomire Jump room's screen scroll layout.</summary>
    public static RoomScrollDefinition PostCrocomireJump { get; } = new("PostCrocomireJump",
    [
        .. Repeated(Open, 5), .. Repeated(Closed, 3),
        .. Repeated(Open, 5), .. Repeated(Closed, 3),
        .. Repeated(Top, 8),
    ]);
    /// <summary>The Grapple Tutorial 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition GrappleTutorial2 { get; } = new("GrappleTutorial2", TwoOpenThenTop);
    /// <summary>The Grapple Beam room's screen scroll layout.</summary>
    public static RoomScrollDefinition GrappleBeam { get; } = new("GrappleBeam",
    [
        Open,
        Open,
        Top,
        Closed,
    ]);
    /// <summary>The Green Bubbles Missiles room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBubblesMissiles { get; } = new("GreenBubblesMissiles", ClosedThenTop);
    /// <summary>The Bubble Mountain room's screen scroll layout.</summary>
    public static RoomScrollDefinition BubbleMountain { get; } = new("BubbleMountain",
    [
        .. Repeated(Open, 8),
    ]);
    /// <summary>The Single Chamber room's screen scroll layout.</summary>
    public static RoomScrollDefinition SingleChamber { get; } = new("SingleChamber",
    [
        Open, Closed, .. Repeated(Top, 4),
        Open, .. Repeated(Closed, 5),
        Open, .. Repeated(Closed, 5),
        Top, .. Repeated(Closed, 5),
    ]);
    /// <summary>The Double Chamber room's screen scroll layout.</summary>
    public static RoomScrollDefinition DoubleChamber { get; } = new("DoubleChamber",
    [
        .. Repeated(Open, 8),
    ]);
    /// <summary>The Volcano room's screen scroll layout.</summary>
    public static RoomScrollDefinition Volcano { get; } = new("Volcano",
    [
        Closed, Closed, Open,
        Closed, Closed, Open,
        Closed, Closed, Top,
    ]);
    /// <summary>The Kronic Boost room's screen scroll layout.</summary>
    public static RoomScrollDefinition KronicBoost { get; } = new("KronicBoost",
    [
        Closed, Open,
        Closed, Open,
        Closed, Top,
    ]);
    /// <summary>The Purple Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition PurpleShaft { get; } = new("PurpleShaft", TwoOpenThenTop);
    /// <summary>The Lower Norfair Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition LowerNorfairElevator { get; } = new("LowerNorfairElevator",
    [
        Top,
    ]);
    /// <summary>The Upper Norfair Farming room's screen scroll layout.</summary>
    public static RoomScrollDefinition UpperNorfairFarming { get; } = new("UpperNorfairFarming", TwoOpenThenTwoTop);
    /// <summary>The Bat Cave room's screen scroll layout.</summary>
    public static RoomScrollDefinition BatCave { get; } = new("BatCave", ClosedThenTop);
    /// <summary>The Frog Speedway room's screen scroll layout.</summary>
    public static RoomScrollDefinition FrogSpeedway { get; } = new("FrogSpeedway",
    [
        .. Repeated(Top, 8),
    ]);
    /// <summary>The Red Pirate Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedPirateShaft { get; } = new("RedPirateShaft",
    [
        .. Repeated(Open, 3),
    ]);
    /// <summary>The Lower Norfair Elevator Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition LowerNorfairElevatorSave { get; } = new("LowerNorfairElevatorSave",
    [
        Top,
    ]);
    /// <summary>The Acid Statue room's screen scroll layout.</summary>
    public static RoomScrollDefinition AcidStatue { get; } = new("AcidStatue",
    [
        Open, Open, Closed,
        Top, Top, .. Repeated(Closed, 4),
    ]);
    /// <summary>The Main Hall room's screen scroll layout.</summary>
    public static RoomScrollDefinition MainHall { get; } = new("MainHall",
    [
        .. Repeated(Closed, 4), Open, .. Repeated(Closed, 11),
        .. Repeated(Top, 8),
    ]);
    /// <summary>The Golden Torizo room's screen scroll layout.</summary>
    public static RoomScrollDefinition GoldenTorizo { get; } = new("GoldenTorizo", TwoOpenThenTwoTop);
    /// <summary>The Ridley room's screen scroll layout.</summary>
    public static RoomScrollDefinition Ridley { get; } = new("Ridley", BothOpen);
    /// <summary>The Fast Pillars Setup room's screen scroll layout.</summary>
    public static RoomScrollDefinition FastPillarsSetup { get; } = new("FastPillarsSetup",
    [
        Open,
        Top,
        Closed,
    ]);
    /// <summary>The Mickey Mouse room's screen scroll layout.</summary>
    public static RoomScrollDefinition MickeyMouse { get; } = new("MickeyMouse",
    [
        .. Repeated(Closed, 3), Open,
        .. Repeated(Closed, 3), Open,
        .. Repeated(Closed, 3), Open,
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Worst Room in the Game's screen scroll layout.</summary>
    public static RoomScrollDefinition WorstRoomInTheGame { get; } = new("WorstRoomInTheGame",
    [
        Closed,
        .. Repeated(Open, 4),
        Top,
    ]);
    /// <summary>The Lower Norfair Spring Ball Maze room's screen scroll layout.</summary>
    public static RoomScrollDefinition LowerNorfairSpringBallMaze { get; } = new("LowerNorfairSpringBallMaze",
    [
        Open, .. Repeated(Closed, 4),
        Top, Top, .. Repeated(Closed, 3),
    ]);
    /// <summary>The Red Kihunter Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedKihunterShaft { get; } = new("RedKihunterShaft",
    [
        Open, Closed, Closed,
        Open, Closed, Closed,
        Open, Closed, Closed,
        Open, Closed, Closed,
        Open, Closed, Open,
    ]);
    /// <summary>The Wasteland room's screen scroll layout.</summary>
    public static RoomScrollDefinition Wasteland { get; } = new("Wasteland",
    [
        Closed, .. Repeated(Top, 3), Closed, Top,
        .. Repeated(Closed, 7), Open, .. Repeated(Closed, 4),
    ]);
    /// <summary>The Three Musketeers room's screen scroll layout.</summary>
    public static RoomScrollDefinition ThreeMusketeers { get; } = new("ThreeMusketeers",
    [
        Closed, Open, .. Repeated(Closed, 3), Open, .. Repeated(Closed, 3), .. Repeated(Top, 3),
    ]);
    /// <summary>The Lower Norfair Fireflea room's screen scroll layout.</summary>
    public static RoomScrollDefinition LowerNorfairFireflea { get; } = new("LowerNorfairFireflea",
    [
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Top, .. Repeated(Closed, 5), Top, Top,
    ]);
    /// <summary>The Bowling Alley room's screen scroll layout.</summary>
    public static RoomScrollDefinition BowlingAlley { get; } = new("BowlingAlley",
    [
        Closed, Closed, .. Repeated(Top, 3), Open,
        .. Repeated(Top, 5), Open,
        Closed, Top, .. Repeated(Closed, 3), Open,
    ]);
    /// <summary>The Attic room's screen scroll layout.</summary>
    public static RoomScrollDefinition Attic { get; } = new("Attic",
    [
        .. Repeated(Open, 7),
    ]);
    /// <summary>The Wrecked Ship Main Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition WreckedShipMainShaft { get; } = new("WreckedShipMainShaft",
    [
        .. Repeated(Closed, 4), Open, .. Repeated(Closed, 5), Open, .. Repeated(Closed, 5), Open, .. Repeated(Closed, 5), Open, .. Repeated(Closed, 5), Open, Closed,
        .. Repeated(Top, 3), Closed, Open, .. Repeated(Closed, 5), Top, .. Repeated(Closed, 7),
    ]);
    /// <summary>The Electric Death room's screen scroll layout.</summary>
    public static RoomScrollDefinition ElectricDeath { get; } = new("ElectricDeath",
    [
        Closed,
        Open,
        Open,
    ]);
    /// <summary>The Basement room's screen scroll layout.</summary>
    public static RoomScrollDefinition Basement { get; } = new("Basement",
    [
        .. Repeated(Top, 4), Closed,
    ]);
    /// <summary>The Phantoon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Phantoon { get; } = new("Phantoon",
    [
        Top,
    ]);
    /// <summary>The Sponge Bath room's screen scroll layout.</summary>
    public static RoomScrollDefinition SpongeBath { get; } = new("SpongeBath", BothTop);
    /// <summary>The Wrecked Ship West Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition WreckedShipWestSuper { get; } = new("WreckedShipWestSuper",
    [
        Top,
    ]);
    /// <summary>The Wrecked Ship East Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition WreckedShipEastSuper { get; } = new("WreckedShipEastSuper",
    [
        .. Repeated(Top, 3), Closed,
    ]);
    /// <summary>The Glass Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition GlassTunnel { get; } = new("GlassTunnel",
    [
        .. Repeated(Open, 3),
    ]);
    /// <summary>The West Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestTunnel { get; } = new("WestTunnel",
    [
        Top,
    ]);
    /// <summary>The East Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastTunnel { get; } = new("EastTunnel",
    [
        .. Repeated(Top, 4),
        .. Repeated(Closed, 4),
    ]);
    /// <summary>The Main Street room's screen scroll layout.</summary>
    public static RoomScrollDefinition MainStreet { get; } = new("MainStreet",
    [
        Open, Open, Closed,
        Open, Open, Closed,
        Open, Open, Top,
        Open, Open, Closed,
        Open, Open, Closed,
        Open, Open, Closed,
        .. Repeated(Open, 5), Closed,
    ]);
    /// <summary>The Fish Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition FishTank { get; } = new("FishTank",
    [
        .. Repeated(Open, 10), Closed, Open,
    ]);
    /// <summary>The Mama Turtle room's screen scroll layout.</summary>
    public static RoomScrollDefinition MamaTurtle { get; } = new("MamaTurtle",
    [
        Closed, Open, Open,
        Closed, Open, Open,
        Closed, .. Repeated(Open, 5),
    ]);
    /// <summary>The Crab Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrabTunnel { get; } = new("CrabTunnel",
    [
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Mt. Everest room's screen scroll layout.</summary>
    public static RoomScrollDefinition MtEverest { get; } = new("MtEverest",
    [
        .. Repeated(Open, 12),
        Closed, .. Repeated(Open, 4), Closed,
        Closed, .. Repeated(Open, 4), Closed,
    ]);
    /// <summary>The Red Fish room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedFish { get; } = new("RedFish",
    [
        Top, Closed, Open,
        Closed, Closed, Open,
    ]);
    /// <summary>The Watering Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition WateringHole { get; } = new("WateringHole",
    [
        Open, Top,
        Open, .. Repeated(Closed, 3),
    ]);
    /// <summary>The Northwest Maridia Bug room's screen scroll layout.</summary>
    public static RoomScrollDefinition NorthwestMaridiaBug { get; } = new("NorthwestMaridiaBug",
    [
        Open, .. Repeated(Closed, 3),
        Top, Closed, Top, Top,
    ]);
    /// <summary>The Crab Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrabShaft { get; } = new("CrabShaft",
    [
        Open, Closed,
        Open, Closed,
        Top, Closed,
        Closed, Top,
    ]);
    /// <summary>The Pseudo Plasma Spark room's screen scroll layout.</summary>
    public static RoomScrollDefinition PseudoPlasmaSpark { get; } = new("PseudoPlasmaSpark",
    [
        .. Repeated(Open, 10), Closed, Closed,
    ]);
    /// <summary>The Crab Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrabHole { get; } = new("CrabHole", TopThenClosed);
    /// <summary>The West Sand Hall Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestSandHallTunnel { get; } = new("WestSandHallTunnel",
    [
        Top,
    ]);
    /// <summary>The Plasma Tutorial room's screen scroll layout.</summary>
    public static RoomScrollDefinition PlasmaTutorial { get; } = new("PlasmaTutorial",
    [
        Top,
    ]);
    /// <summary>The Plasma room's screen scroll layout.</summary>
    public static RoomScrollDefinition Plasma { get; } = new("Plasma",
    [
        .. Repeated(Open, 4),
        Top, Top,
    ]);
    /// <summary>The Thread the Needle room's screen scroll layout.</summary>
    public static RoomScrollDefinition ThreadTheNeedle { get; } = new("ThreadTheNeedle",
    [
        .. Repeated(Top, 7),
    ]);
    /// <summary>The Maridia Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition MaridiaElevator { get; } = new("MaridiaElevator",
    [
        .. Repeated(Open, 5),
        Top,
    ]);
    /// <summary>The Plasma Spark room's screen scroll layout.</summary>
    public static RoomScrollDefinition PlasmaSpark { get; } = new("PlasmaSpark",
    [
        Closed, Open, Open, Closed,
        Closed, Open, Open, Closed,
        .. Repeated(Open, 4),
        Closed, .. Repeated(Open, 3),
        Closed, .. Repeated(Open, 3),
        Closed, .. Repeated(Top, 3),
    ]);
    /// <summary>The Kassiuz room's screen scroll layout.</summary>
    public static RoomScrollDefinition Kassiuz { get; } = new("Kassiuz",
    [
        .. Repeated(Open, 4),
    ]);
    /// <summary>The Bug Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition BugSandHole { get; } = new("BugSandHole",
    [
        Open,
    ]);
    /// <summary>The Oasis room's screen scroll layout.</summary>
    public static RoomScrollDefinition Oasis { get; } = new("Oasis", ClosedThenTop);
    /// <summary>The West Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestSandHole { get; } = new("WestSandHole",
    [
        .. Repeated(Open, 4),
    ]);
    /// <summary>The East Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastSandHole { get; } = new("EastSandHole",
    [
        .. Repeated(Open, 4),
    ]);
    /// <summary>The West Aqueduct Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestAqueductQuicksand { get; } = new("WestAqueductQuicksand", BothOpen);
    /// <summary>The East Aqueduct Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastAqueductQuicksand { get; } = new("EastAqueductQuicksand", BothOpen);
    /// <summary>The Aqueduct room's screen scroll layout.</summary>
    public static RoomScrollDefinition Aqueduct { get; } = new("Aqueduct",
    [
        .. Repeated(Open, 18),
    ]);
    /// <summary>The Botwoon Hallway room's screen scroll layout.</summary>
    public static RoomScrollDefinition BotwoonHallway { get; } = new("BotwoonHallway",
    [
        .. Repeated(Open, 4),
    ]);
    /// <summary>The Pants room's screen scroll layout.</summary>
    public static RoomScrollDefinition Pants { get; } = new("Pants",
    [
        .. Repeated(Open, 5), Top,
        Top, Closed,
    ]);
    /// <summary>The East Pants room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastPants { get; } = new("EastPants",
    [
        Open,
        Closed,
        Top,
    ]);
    /// <summary>The Springball room's screen scroll layout.</summary>
    public static RoomScrollDefinition Springball { get; } = new("Springball",
    [
        Open, Closed,
        Top, Top,
    ]);
    /// <summary>The Below Botwoon Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BelowBotwoonEnergyTank { get; } = new("BelowBotwoonEnergyTank",
    [
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Colosseum room's screen scroll layout.</summary>
    public static RoomScrollDefinition Colosseum { get; } = new("Colosseum",
    [
        .. Repeated(Open, 7),
        .. Repeated(Top, 7),
    ]);
    /// <summary>The Aqueduct Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition AqueductSave { get; } = new("AqueductSave",
    [
        Top,
    ]);
    /// <summary>The Precious room's screen scroll layout.</summary>
    public static RoomScrollDefinition ThePrecious { get; } = new("ThePrecious",
    [
        Top, Top,
        Closed, Closed,
        Top, Closed,
    ]);
    /// <summary>The Botwoon Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BotwoonEnergyTank { get; } = new("BotwoonEnergyTank",
    [
        .. Repeated(Open, 7),
    ]);
    /// <summary>The Plasma Beach Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition PlasmaBeachQuicksand { get; } = new("PlasmaBeachQuicksand",
    [
        Open,
    ]);
    /// <summary>The Botwoon Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition BotwoonQuicksand { get; } = new("BotwoonQuicksand", BothOpen);
    /// <summary>The Shaktool room's screen scroll layout.</summary>
    public static RoomScrollDefinition Shaktool { get; } = new("Shaktool",
    [
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Halfie Climb room's screen scroll layout.</summary>
    public static RoomScrollDefinition HalfieClimb { get; } = new("HalfieClimb",
    [
        Open, .. Repeated(Closed, 4),
        Open, .. Repeated(Closed, 4),
        Top, Closed, .. Repeated(Top, 3),
    ]);
    /// <summary>The Botwoon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Botwoon { get; } = new("Botwoon", TopThenClosed);
    /// <summary>The Space Jump room's screen scroll layout.</summary>
    public static RoomScrollDefinition SpaceJump { get; } = new("SpaceJump",
    [
        Top,
    ]);
    /// <summary>The Maridia Energy Refill room's screen scroll layout.</summary>
    public static RoomScrollDefinition MaridiaEnergyRefill { get; } = new("MaridiaEnergyRefill",
    [
        Top,
    ]);
    /// <summary>The West Cactus Alley room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestCactusAlley { get; } = new("WestCactusAlley", OpenThenTop);
    /// <summary>The East Cactus Alley room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastCactusAlley { get; } = new("EastCactusAlley",
    [
        .. Repeated(Open, 5),
        .. Repeated(Top, 5),
    ]);
    /// <summary>The Draygon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Draygon { get; } = new("Draygon", TwoOpenThenTwoTop);
    /// <summary>The Tourian First room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianFirst { get; } = new("TourianFirst",
    [
        .. Repeated(Open, 3),
        Top,
    ]);
    /// <summary>The Metroids 1 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids1 { get; } = new("Metroids1",
    [
        .. Repeated(Top, 6),
    ]);
    /// <summary>The Metroids 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids2 { get; } = new("Metroids2", BothOpen);
    /// <summary>The Metroids 3 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids3 { get; } = new("Metroids3",
    [
        .. Repeated(Top, 6),
    ]);
    /// <summary>The Metroids 4 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids4 { get; } = new("Metroids4", BothOpen);
    /// <summary>The Blue Hopper room's screen scroll layout.</summary>
    public static RoomScrollDefinition BlueHopper { get; } = new("BlueHopper", BothTop);
    /// <summary>The Dust Torizo room's screen scroll layout.</summary>
    public static RoomScrollDefinition DustTorizo { get; } = new("DustTorizo", BothTop);
    /// <summary>The Big Boy room's screen scroll layout.</summary>
    public static RoomScrollDefinition BigBoy { get; } = new("BigBoy",
    [
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Seaweed room's screen scroll layout.</summary>
    public static RoomScrollDefinition Seaweed { get; } = new("Seaweed", BothOpen);
    /// <summary>The Tourian Recharge room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianRecharge { get; } = new("TourianRecharge",
    [
        Top,
    ]);
    /// <summary>The Mother Brain room's screen scroll layout.</summary>
    public static RoomScrollDefinition MotherBrain { get; } = new("MotherBrain",
    [
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Tourian Eye Door room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEyeDoor { get; } = new("TourianEyeDoor",
    [
        .. Repeated(Top, 4),
    ]);
    /// <summary>The Rinka Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition RinkaShaft { get; } = new("RinkaShaft",
    [
        .. Repeated(Open, 3),
    ]);
    /// <summary>The Mother Brain Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition MotherBrainSave { get; } = new("MotherBrainSave",
    [
        Top,
    ]);
    /// <summary>The Tourian Escape 1 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape1 { get; } = new("TourianEscape1", BothOpen);
    /// <summary>The Tourian Escape 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape2 { get; } = new("TourianEscape2", OpenThenTop);
    /// <summary>The Tourian Escape 3 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape3 { get; } = new("TourianEscape3",
    [
        .. Repeated(Open, 6),
        .. Repeated(Top, 6),
    ]);
    /// <summary>The Tourian Escape 4 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape4 { get; } = new("TourianEscape4",
    [
        .. Repeated(Open, 15),
        .. Repeated(Top, 3),
    ]);
    /// <summary>The Upper Tourian Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition UpperTourianSave { get; } = new("UpperTourianSave",
    [
        Top,
    ]);

    /// <summary>
    /// Explicit tables in their authored order, each the room's own screens, row by row at the room's width. What a
    /// room's load writes past its own screens is <see cref="ScrollCellsPastRoomScreens{TCell}"/>.
    /// </summary>
    private static readonly RoomScrollDefinition[] Definitions =
    [
        LandingSite,
        Parlor,
        CrateriaPowerBombs,
        WestOcean,
        CrateriaKihunter,
        ForgottenHighwayElevator,
        EastOcean,
        ForgottenHighwayElbow,
        RedBrinstarElevator,
        GauntletEnergyTank,
        Climb,
        Pit,
        ElevatorToBlueBrinstar,
        GreenBrinstarElevator,
        GreenPiratesShaft,
        CrateriaSuper,
        FinalMissileBombway,
        FinalMissile,
        GreenBrinstarMainShaft,
        SporeSpawnSuper,
        EarlySupers,
        BrinstarReserveTank,
        GreenBrinstarMissileRefill,
        Dachora,
        BigPink,
        PinkBrinstarPowerBombs,
        GreenHillZone,
        MorphBall,
        ConstructionZone,
        BlueBrinstarEnergyTank,
        GreenBrinstarBeetoms,
        EtecoonEnergyTank,
        EtecoonSuper,
        SporeSpawnFarming,
        WaterwayEnergyTank,
        RedTower,
        RedBrinstarFirefleas,
        Caterpillar,
        BetaPowerBomb,
        AlphaPowerBomb,
        BelowSpazer,
        Spazer,
        WarehouseZeela,
        WarehouseKihunter,
        KraidEyeDoor,
        Kraid,
        Statues,
        WarehouseEntrance,
        IceBeamGate,
        IceBeamSnake,
        CrocomireSpeedway,
        Crocomire,
        CrocomireEscape,
        HiJumpEnergyTank,
        PostCrocomireFarming,
        PostCrocomireShaft,
        PostCrocomireJump,
        GrappleTutorial2,
        GrappleBeam,
        GreenBubblesMissiles,
        BubbleMountain,
        SingleChamber,
        DoubleChamber,
        Volcano,
        KronicBoost,
        PurpleShaft,
        LowerNorfairElevator,
        UpperNorfairFarming,
        BatCave,
        FrogSpeedway,
        RedPirateShaft,
        LowerNorfairElevatorSave,
        AcidStatue,
        MainHall,
        GoldenTorizo,
        Ridley,
        FastPillarsSetup,
        MickeyMouse,
        WorstRoomInTheGame,
        LowerNorfairSpringBallMaze,
        RedKihunterShaft,
        Wasteland,
        ThreeMusketeers,
        LowerNorfairFireflea,
        BowlingAlley,
        Attic,
        WreckedShipMainShaft,
        ElectricDeath,
        Basement,
        Phantoon,
        SpongeBath,
        WreckedShipWestSuper,
        WreckedShipEastSuper,
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
        BugSandHole,
        Oasis,
        WestSandHole,
        EastSandHole,
        WestAqueductQuicksand,
        EastAqueductQuicksand,
        Aqueduct,
        BotwoonHallway,
        Pants,
        EastPants,
        Springball,
        BelowBotwoonEnergyTank,
        Colosseum,
        AqueductSave,
        ThePrecious,
        BotwoonEnergyTank,
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
    ];

    /// <summary>Every explicit table, in authored order, which the import decodes each one's cells past its screens for.</summary>
    internal static IReadOnlyList<RoomScrollDefinition> Explicit => Definitions;
}
