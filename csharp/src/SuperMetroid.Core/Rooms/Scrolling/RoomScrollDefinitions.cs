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

    /// <summary>The implicit layout whose bottom row is open with the camera aligned to the screen's top, and every other row open.</summary>
    public static RoomScrollDefinition OpenWithTopAlignedBottomRow { get; } = RoomScrollDefinition.Implicit("OpenWithTopAlignedBottomRow", RoomScrollState.OpenAlignedToTop);

    /// <summary>The implicit layout whose every row is open.</summary>
    public static RoomScrollDefinition AllOpen { get; } = RoomScrollDefinition.Implicit("AllOpen", RoomScrollState.Open);

    /// <summary>The Landing Site room's screen scroll layout.</summary>
    public static RoomScrollDefinition LandingSite { get; } = new("LandingSite",
    [
        Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        Top, Closed, Open, Open, Open, Open, Open, Open, Open,
        Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        Top, Top, Top, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Parlor room's screen scroll layout.</summary>
    public static RoomScrollDefinition Parlor { get; } = new("Parlor",
    [
        Closed, Top, Top, Top, Top,
        Closed, Closed, Closed, Closed, Closed,
        Closed, Open, Closed, Top, Closed,
        Closed, Open, Closed, Closed, Closed,
        Closed, Open, Closed, Closed, Closed,
    ]);
    /// <summary>The Crateria Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaPowerBombs { get; } = new("CrateriaPowerBombs",
    [
        Top, Top,
    ]);
    /// <summary>The West Ocean room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestOcean { get; } = new("WestOcean",
    [
        Open, Open, Open, Open, Open, Open, Open, Open,
        Open, Open, Open, Open, Open, Open, Open, Open,
        Open, Open, Open, Closed, Closed, Open, Closed, Closed,
        Open, Open, Open, Open, Open, Open, Closed, Closed,
        Open, Open, Open, Open, Open, Open, Open, Open,
        Top, Top, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Crateria Kihunter room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaKihunter { get; } = new("CrateriaKihunter",
    [
        Top, Top, Top,
        Closed, Closed, Closed,
        Closed, Open, Closed,
    ]);
    /// <summary>The Forgotten Highway Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition ForgottenHighwayElevator { get; } = new("ForgottenHighwayElevator",
    [
        Top,
    ]);
    /// <summary>The East Ocean room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastOcean { get; } = new("EastOcean",
    [
        Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        Open, Open, Open, Open, Open, Open, Open,
        Top, Top, Top, Top, Top, Top, Top,
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
        Top, Top, Top, Top, Closed, Top,
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
        Top, Top, Top,
        Closed, Closed, Closed,
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
        Open,
        Open,
        Open,
        Closed,
        Open,
        Open,
        Top,
    ]);
    /// <summary>The Crateria Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaSuper { get; } = new("CrateriaSuper",
    [
        Top, Top, Top, Open,
        Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Open,
        Top, Top, Top, Top,
    ]);
    /// <summary>The Final Missile Bombway room's screen scroll layout.</summary>
    public static RoomScrollDefinition FinalMissileBombway { get; } = new("FinalMissileBombway",
    [
        Top, Top,
    ]);
    /// <summary>The Final Missile room's screen scroll layout.</summary>
    public static RoomScrollDefinition FinalMissile { get; } = new("FinalMissile",
    [
        Top,
    ]);
    /// <summary>The Green Brinstar Main Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarMainShaft { get; } = new("GreenBrinstarMainShaft",
    [
        Open, Closed, Closed, Closed,
        Open, Closed, Closed, Closed,
        Open, Closed, Closed, Closed,
        Open, Closed, Closed, Closed,
        Open, Closed, Closed, Closed,
        Open, Closed, Closed, Closed,
        Open, Closed, Closed, Closed,
        Closed, Closed, Open, Top,
        Open, Closed, Open, Closed,
        Open, Closed, Open, Closed,
        Open, Closed, Open, Closed,
        Closed, Closed, Open, Closed,
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
        Closed, Closed, Closed,
        Top, Top, Top,
    ]);
    /// <summary>The Brinstar Reserve Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BrinstarReserveTank { get; } = new("BrinstarReserveTank",
    [
        Top, Closed,
    ]);
    /// <summary>The Green Brinstar Missile Refill room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarMissileRefill { get; } = new("GreenBrinstarMissileRefill",
    [
        Top,
    ]);
    /// <summary>The Dachora room's screen scroll layout.</summary>
    public static RoomScrollDefinition Dachora { get; } = new("Dachora",
    [
        Top, Top, Top, Top, Top, Top, Top,
        Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        Closed, Closed, Closed, Closed, Open, Closed, Closed,
        Closed, Closed, Closed, Closed, Open, Closed, Closed,
        Closed, Closed, Closed, Closed, Open, Closed, Closed,
        Closed, Closed, Closed, Closed, Open, Closed, Closed,
        Open, Open, Open, Open, Open, Closed, Closed,
    ]);
    /// <summary>The Big Pink room's screen scroll layout.</summary>
    public static RoomScrollDefinition BigPink { get; } = new("BigPink",
    [
        Closed, Closed, Open, Open, Closed,
        Closed, Closed, Open, Open, Closed,
        Closed, Closed, Open, Open, Closed,
        Closed, Closed, Open, Open, Closed,
        Closed, Closed, Open, Open, Closed,
        Closed, Closed, Open, Open, Closed,
        Closed, Closed, Top, Top, Closed,
        Open, Closed, Closed, Closed, Closed,
        Open, Closed, Closed, Closed, Closed,
        Open, Closed, Closed, Closed, Closed,
    ]);
    /// <summary>The Pink Brinstar Power Bombs room's screen scroll layout.</summary>
    public static RoomScrollDefinition PinkBrinstarPowerBombs { get; } = new("PinkBrinstarPowerBombs",
    [
        Top, Top,
        Closed, Closed,
    ]);
    /// <summary>The Green Hill Zone room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenHillZone { get; } = new("GreenHillZone",
    [
        Open, Open, Closed, Closed, Closed, Closed, Closed, Closed,
        Open, Open, Open, Open, Closed, Closed, Closed, Closed,
        Open, Open, Open, Open, Open, Open, Open, Open,
        Top, Top, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Morph Ball room's screen scroll layout.</summary>
    public static RoomScrollDefinition MorphBall { get; } = new("MorphBall",
    [
        Closed, Closed, Closed, Closed, Closed, Open, Closed, Closed,
        Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        Top, Top, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Construction Zone room's screen scroll layout.</summary>
    public static RoomScrollDefinition ConstructionZone { get; } = new("ConstructionZone",
    [
        Top,
        Closed,
    ]);
    /// <summary>The Blue Brinstar Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BlueBrinstarEnergyTank { get; } = new("BlueBrinstarEnergyTank",
    [
        Closed, Closed, Open,
        Closed, Closed, Closed,
        Top, Top, Top,
    ]);
    /// <summary>The Green Brinstar Beetoms room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarBeetoms { get; } = new("GreenBrinstarBeetoms",
    [
        Top,
    ]);
    /// <summary>The Etecoon Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition EtecoonEnergyTank { get; } = new("EtecoonEnergyTank",
    [
        Top, Top, Closed, Closed, Closed,
        Closed, Closed, Top, Top, Top,
    ]);
    /// <summary>The Etecoon Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition EtecoonSuper { get; } = new("EtecoonSuper",
    [
        Top,
    ]);
    /// <summary>The Spore Spawn Farming room's screen scroll layout.</summary>
    public static RoomScrollDefinition SporeSpawnFarming { get; } = new("SporeSpawnFarming",
    [
        Top, Top, Top,
    ]);
    /// <summary>The Waterway Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition WaterwayEnergyTank { get; } = new("WaterwayEnergyTank",
    [
        Closed, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Red Tower room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedTower { get; } = new("RedTower",
    [
        Open,
        Open,
        Open,
        Open,
        Open,
        Open,
        Top,
        Closed,
        Open,
        Open,
    ]);
    /// <summary>The Red Brinstar Firefleas room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedBrinstarFirefleas { get; } = new("RedBrinstarFirefleas",
    [
        Open, Open, Top, Top, Open, Open, Top, Top,
        Top, Top, Closed, Closed, Top, Top, Closed, Closed,
    ]);
    /// <summary>The Caterpillar room's screen scroll layout.</summary>
    public static RoomScrollDefinition Caterpillar { get; } = new("Caterpillar",
    [
        Open, Closed, Closed,
        Open, Closed, Closed,
        Open, Closed, Closed,
        Open, Closed, Top,
        Open, Closed, Closed,
        Top, Closed, Closed,
        Closed, Closed, Closed,
        Open, Closed, Closed,
    ]);
    /// <summary>The Beta Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition BetaPowerBomb { get; } = new("BetaPowerBomb",
    [
        Top, Top,
        Closed, Closed,
    ]);
    /// <summary>The Alpha Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition AlphaPowerBomb { get; } = new("AlphaPowerBomb",
    [
        Closed, Top, Top,
    ]);
    /// <summary>The Below Spazer room's screen scroll layout.</summary>
    public static RoomScrollDefinition BelowSpazer { get; } = new("BelowSpazer",
    [
        Closed, Closed,
        Top, Top,
    ]);
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
    public static RoomScrollDefinition KraidEyeDoor { get; } = new("KraidEyeDoor",
    [
        Closed, Closed,
        Top, Top,
    ]);
    /// <summary>The Kraid room's screen scroll layout.</summary>
    public static RoomScrollDefinition Kraid { get; } = new("Kraid",
    [
        Open, Open,
        Top, Top,
    ]);
    /// <summary>The Statues room's screen scroll layout.</summary>
    public static RoomScrollDefinition Statues { get; } = new("Statues",
    [
        Top,
        Closed,
    ]);
    /// <summary>The Warehouse Entrance room's screen scroll layout.</summary>
    public static RoomScrollDefinition WarehouseEntrance { get; } = new("WarehouseEntrance",
    [
        Top, Closed, Open,
        Closed, Top, Top,
    ]);
    /// <summary>The Ice Beam Gate room's screen scroll layout.</summary>
    public static RoomScrollDefinition IceBeamGate { get; } = new("IceBeamGate",
    [
        Closed, Closed, Closed, Open, Closed, Closed, Closed,
        Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        Closed, Closed, Closed, Top, Top, Top, Top,
        Top, Top, Top, Closed, Closed, Closed, Closed,
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
        Top, Top, Top, Open, Open, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Open, Open, Open, Open, Closed, Closed, Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open, Closed, Open,
    ]);
    /// <summary>The Crocomire room's screen scroll layout.</summary>
    public static RoomScrollDefinition Crocomire { get; } = new("Crocomire",
    [
        Closed, Closed, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Crocomire Escape room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrocomireEscape { get; } = new("CrocomireEscape",
    [
        Open, Open, Open, Open,
        Top, Top, Top, Top,
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
        Open, Open,
        Open, Open,
    ]);
    /// <summary>The Post-Crocomire Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition PostCrocomireShaft { get; } = new("PostCrocomireShaft",
    [
        Open,
        Open,
        Open,
        Open,
        Open,
    ]);
    /// <summary>The Post-Crocomire Jump room's screen scroll layout.</summary>
    public static RoomScrollDefinition PostCrocomireJump { get; } = new("PostCrocomireJump",
    [
        Open, Open, Open, Open, Open, Closed, Closed, Closed,
        Open, Open, Open, Open, Open, Closed, Closed, Closed,
        Top, Top, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Grapple Tutorial 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition GrappleTutorial2 { get; } = new("GrappleTutorial2",
    [
        Open,
        Open,
        Top,
    ]);
    /// <summary>The Grapple Beam room's screen scroll layout.</summary>
    public static RoomScrollDefinition GrappleBeam { get; } = new("GrappleBeam",
    [
        Open,
        Open,
        Top,
        Closed,
    ]);
    /// <summary>The Green Bubbles Missiles room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBubblesMissiles { get; } = new("GreenBubblesMissiles",
    [
        Closed, Top,
    ]);
    /// <summary>The Bubble Mountain room's screen scroll layout.</summary>
    public static RoomScrollDefinition BubbleMountain { get; } = new("BubbleMountain",
    [
        Open, Open,
        Open, Open,
        Open, Open,
        Open, Open,
    ]);
    /// <summary>The Single Chamber room's screen scroll layout.</summary>
    public static RoomScrollDefinition SingleChamber { get; } = new("SingleChamber",
    [
        Open, Closed, Top, Top, Top, Top,
        Open, Closed, Closed, Closed, Closed, Closed,
        Open, Closed, Closed, Closed, Closed, Closed,
        Top, Closed, Closed, Closed, Closed, Closed,
    ]);
    /// <summary>The Double Chamber room's screen scroll layout.</summary>
    public static RoomScrollDefinition DoubleChamber { get; } = new("DoubleChamber",
    [
        Open, Open, Open, Open,
        Open, Open, Open, Open,
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
    public static RoomScrollDefinition PurpleShaft { get; } = new("PurpleShaft",
    [
        Open,
        Open,
        Top,
    ]);
    /// <summary>The Lower Norfair Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition LowerNorfairElevator { get; } = new("LowerNorfairElevator",
    [
        Top,
    ]);
    /// <summary>The Upper Norfair Farming room's screen scroll layout.</summary>
    public static RoomScrollDefinition UpperNorfairFarming { get; } = new("UpperNorfairFarming",
    [
        Open, Open,
        Top, Top,
    ]);
    /// <summary>The Bat Cave room's screen scroll layout.</summary>
    public static RoomScrollDefinition BatCave { get; } = new("BatCave",
    [
        Closed,
        Top,
    ]);
    /// <summary>The Frog Speedway room's screen scroll layout.</summary>
    public static RoomScrollDefinition FrogSpeedway { get; } = new("FrogSpeedway",
    [
        Top, Top, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Red Pirate Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedPirateShaft { get; } = new("RedPirateShaft",
    [
        Open,
        Open,
        Open,
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
        Top, Top, Closed,
        Closed, Closed, Closed,
    ]);
    /// <summary>The Main Hall room's screen scroll layout.</summary>
    public static RoomScrollDefinition MainHall { get; } = new("MainHall",
    [
        Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed,
        Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        Top, Top, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Golden Torizo room's screen scroll layout.</summary>
    public static RoomScrollDefinition GoldenTorizo { get; } = new("GoldenTorizo",
    [
        Open, Open,
        Top, Top,
    ]);
    /// <summary>The Ridley room's screen scroll layout.</summary>
    public static RoomScrollDefinition Ridley { get; } = new("Ridley",
    [
        Open,
        Open,
    ]);
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
        Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Open,
        Closed, Closed, Closed, Open,
        Top, Top, Top, Top,
    ]);
    /// <summary>The Worst Room in the Game's screen scroll layout.</summary>
    public static RoomScrollDefinition WorstRoomInTheGame { get; } = new("WorstRoomInTheGame",
    [
        Closed,
        Open,
        Open,
        Open,
        Open,
        Top,
    ]);
    /// <summary>The Lower Norfair Spring Ball Maze room's screen scroll layout.</summary>
    public static RoomScrollDefinition LowerNorfairSpringBallMaze { get; } = new("LowerNorfairSpringBallMaze",
    [
        Open, Closed, Closed, Closed, Closed,
        Top, Top, Closed, Closed, Closed,
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
        Closed, Top, Top, Top, Closed, Top,
        Closed, Closed, Closed, Closed, Closed, Closed,
        Closed, Open, Closed, Closed, Closed, Closed,
    ]);
    /// <summary>The Three Musketeers room's screen scroll layout.</summary>
    public static RoomScrollDefinition ThreeMusketeers { get; } = new("ThreeMusketeers",
    [
        Closed, Open, Closed, Closed,
        Closed, Open, Closed, Closed,
        Closed, Top, Top, Top,
    ]);
    /// <summary>The Lower Norfair Fireflea room's screen scroll layout.</summary>
    public static RoomScrollDefinition LowerNorfairFireflea { get; } = new("LowerNorfairFireflea",
    [
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Open, Closed,
        Closed, Top, Closed,
        Closed, Closed, Closed,
        Closed, Top, Top,
    ]);
    /// <summary>The Bowling Alley room's screen scroll layout.</summary>
    public static RoomScrollDefinition BowlingAlley { get; } = new("BowlingAlley",
    [
        Closed, Closed, Top, Top, Top, Open,
        Top, Top, Top, Top, Top, Open,
        Closed, Top, Closed, Closed, Closed, Open,
    ]);
    /// <summary>The Attic room's screen scroll layout.</summary>
    public static RoomScrollDefinition Attic { get; } = new("Attic",
    [
        Open, Open, Open, Open, Open, Open, Open,
    ]);
    /// <summary>The Wrecked Ship Main Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition WreckedShipMainShaft { get; } = new("WreckedShipMainShaft",
    [
        Closed, Closed, Closed, Closed, Open, Closed,
        Closed, Closed, Closed, Closed, Open, Closed,
        Closed, Closed, Closed, Closed, Open, Closed,
        Closed, Closed, Closed, Closed, Open, Closed,
        Closed, Closed, Closed, Closed, Open, Closed,
        Top, Top, Top, Closed, Open, Closed,
        Closed, Closed, Closed, Closed, Top, Closed,
        Closed, Closed, Closed, Closed, Closed, Closed,
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
        Top, Top, Top, Top, Closed,
    ]);
    /// <summary>The Phantoon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Phantoon { get; } = new("Phantoon",
    [
        Top,
    ]);
    /// <summary>The Sponge Bath room's screen scroll layout.</summary>
    public static RoomScrollDefinition SpongeBath { get; } = new("SpongeBath",
    [
        Top, Top,
    ]);
    /// <summary>The Wrecked Ship West Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition WreckedShipWestSuper { get; } = new("WreckedShipWestSuper",
    [
        Top,
    ]);
    /// <summary>The Wrecked Ship East Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition WreckedShipEastSuper { get; } = new("WreckedShipEastSuper",
    [
        Top, Top, Top, Closed,
    ]);
    /// <summary>The Glass Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition GlassTunnel { get; } = new("GlassTunnel",
    [
        Open,
        Open,
        Open,
    ]);
    /// <summary>The West Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestTunnel { get; } = new("WestTunnel",
    [
        Top,
    ]);
    /// <summary>The East Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastTunnel { get; } = new("EastTunnel",
    [
        Top, Top, Top, Top,
        Closed, Closed, Closed, Closed,
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
        Open, Open, Open,
        Open, Open, Closed,
    ]);
    /// <summary>The Fish Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition FishTank { get; } = new("FishTank",
    [
        Open, Open, Open, Open,
        Open, Open, Open, Open,
        Open, Open, Closed, Open,
    ]);
    /// <summary>The Mama Turtle room's screen scroll layout.</summary>
    public static RoomScrollDefinition MamaTurtle { get; } = new("MamaTurtle",
    [
        Closed, Open, Open,
        Closed, Open, Open,
        Closed, Open, Open,
        Open, Open, Open,
    ]);
    /// <summary>The Crab Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrabTunnel { get; } = new("CrabTunnel",
    [
        Top, Top, Top, Top,
    ]);
    /// <summary>The Mt. Everest room's screen scroll layout.</summary>
    public static RoomScrollDefinition MtEverest { get; } = new("MtEverest",
    [
        Open, Open, Open, Open, Open, Open,
        Open, Open, Open, Open, Open, Open,
        Closed, Open, Open, Open, Open, Closed,
        Closed, Open, Open, Open, Open, Closed,
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
        Open, Closed,
        Closed, Closed,
    ]);
    /// <summary>The Northwest Maridia Bug room's screen scroll layout.</summary>
    public static RoomScrollDefinition NorthwestMaridiaBug { get; } = new("NorthwestMaridiaBug",
    [
        Open, Closed, Closed, Closed,
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
        Open, Open, Open, Open,
        Open, Open, Open, Open,
        Open, Open, Closed, Closed,
    ]);
    /// <summary>The Crab Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrabHole { get; } = new("CrabHole",
    [
        Top,
        Closed,
    ]);
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
        Open, Open,
        Open, Open,
        Top, Top,
    ]);
    /// <summary>The Thread the Needle room's screen scroll layout.</summary>
    public static RoomScrollDefinition ThreadTheNeedle { get; } = new("ThreadTheNeedle",
    [
        Top, Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Maridia Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition MaridiaElevator { get; } = new("MaridiaElevator",
    [
        Open,
        Open,
        Open,
        Open,
        Open,
        Top,
    ]);
    /// <summary>The Plasma Spark room's screen scroll layout.</summary>
    public static RoomScrollDefinition PlasmaSpark { get; } = new("PlasmaSpark",
    [
        Closed, Open, Open, Closed,
        Closed, Open, Open, Closed,
        Open, Open, Open, Open,
        Closed, Open, Open, Open,
        Closed, Open, Open, Open,
        Closed, Top, Top, Top,
    ]);
    /// <summary>The Kassiuz room's screen scroll layout.</summary>
    public static RoomScrollDefinition Kassiuz { get; } = new("Kassiuz",
    [
        Open,
        Open,
        Open,
        Open,
    ]);
    /// <summary>The Bug Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition BugSandHole { get; } = new("BugSandHole",
    [
        Open,
    ]);
    /// <summary>The Oasis room's screen scroll layout.</summary>
    public static RoomScrollDefinition Oasis { get; } = new("Oasis",
    [
        Closed,
        Top,
    ]);
    /// <summary>The West Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestSandHole { get; } = new("WestSandHole",
    [
        Open, Open,
        Open, Open,
    ]);
    /// <summary>The East Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastSandHole { get; } = new("EastSandHole",
    [
        Open, Open,
        Open, Open,
    ]);
    /// <summary>The West Aqueduct Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestAqueductQuicksand { get; } = new("WestAqueductQuicksand",
    [
        Open,
        Open,
    ]);
    /// <summary>The East Aqueduct Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastAqueductQuicksand { get; } = new("EastAqueductQuicksand",
    [
        Open,
        Open,
    ]);
    /// <summary>The Aqueduct room's screen scroll layout.</summary>
    public static RoomScrollDefinition Aqueduct { get; } = new("Aqueduct",
    [
        Open, Open, Open, Open, Open, Open,
        Open, Open, Open, Open, Open, Open,
        Open, Open, Open, Open, Open, Open,
    ]);
    /// <summary>The Botwoon Hallway room's screen scroll layout.</summary>
    public static RoomScrollDefinition BotwoonHallway { get; } = new("BotwoonHallway",
    [
        Open, Open, Open, Open,
    ]);
    /// <summary>The Pants room's screen scroll layout.</summary>
    public static RoomScrollDefinition Pants { get; } = new("Pants",
    [
        Open, Open,
        Open, Open,
        Open, Top,
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
        Top, Top, Top, Top,
    ]);
    /// <summary>The Colosseum room's screen scroll layout.</summary>
    public static RoomScrollDefinition Colosseum { get; } = new("Colosseum",
    [
        Open, Open, Open, Open, Open, Open, Open,
        Top, Top, Top, Top, Top, Top, Top,
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
        Open, Open, Open, Open, Open, Open, Open,
    ]);
    /// <summary>The Plasma Beach Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition PlasmaBeachQuicksand { get; } = new("PlasmaBeachQuicksand",
    [
        Open,
    ]);
    /// <summary>The Botwoon Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition BotwoonQuicksand { get; } = new("BotwoonQuicksand",
    [
        Open, Open,
    ]);
    /// <summary>The Shaktool room's screen scroll layout.</summary>
    public static RoomScrollDefinition Shaktool { get; } = new("Shaktool",
    [
        Top, Top, Top, Top,
    ]);
    /// <summary>The Halfie Climb room's screen scroll layout.</summary>
    public static RoomScrollDefinition HalfieClimb { get; } = new("HalfieClimb",
    [
        Open, Closed, Closed, Closed, Closed,
        Open, Closed, Closed, Closed, Closed,
        Top, Closed, Top, Top, Top,
    ]);
    /// <summary>The Botwoon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Botwoon { get; } = new("Botwoon",
    [
        Top, Closed,
    ]);
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
    public static RoomScrollDefinition WestCactusAlley { get; } = new("WestCactusAlley",
    [
        Open,
        Top,
    ]);
    /// <summary>The East Cactus Alley room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastCactusAlley { get; } = new("EastCactusAlley",
    [
        Open, Open, Open, Open, Open,
        Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Draygon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Draygon { get; } = new("Draygon",
    [
        Open, Open,
        Top, Top,
    ]);
    /// <summary>The Tourian First room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianFirst { get; } = new("TourianFirst",
    [
        Open,
        Open,
        Open,
        Top,
    ]);
    /// <summary>The Metroids 1 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids1 { get; } = new("Metroids1",
    [
        Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Metroids 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids2 { get; } = new("Metroids2",
    [
        Open,
        Open,
    ]);
    /// <summary>The Metroids 3 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids3 { get; } = new("Metroids3",
    [
        Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Metroids 4 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids4 { get; } = new("Metroids4",
    [
        Open,
        Open,
    ]);
    /// <summary>The Blue Hopper room's screen scroll layout.</summary>
    public static RoomScrollDefinition BlueHopper { get; } = new("BlueHopper",
    [
        Top, Top,
    ]);
    /// <summary>The Dust Torizo room's screen scroll layout.</summary>
    public static RoomScrollDefinition DustTorizo { get; } = new("DustTorizo",
    [
        Top, Top,
    ]);
    /// <summary>The Big Boy room's screen scroll layout.</summary>
    public static RoomScrollDefinition BigBoy { get; } = new("BigBoy",
    [
        Top, Top, Top, Top,
    ]);
    /// <summary>The Seaweed room's screen scroll layout.</summary>
    public static RoomScrollDefinition Seaweed { get; } = new("Seaweed",
    [
        Open,
        Open,
    ]);
    /// <summary>The Tourian Recharge room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianRecharge { get; } = new("TourianRecharge",
    [
        Top,
    ]);
    /// <summary>The Mother Brain room's screen scroll layout.</summary>
    public static RoomScrollDefinition MotherBrain { get; } = new("MotherBrain",
    [
        Top, Top, Top, Top,
    ]);
    /// <summary>The Tourian Eye Door room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEyeDoor { get; } = new("TourianEyeDoor",
    [
        Top, Top, Top, Top,
    ]);
    /// <summary>The Rinka Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition RinkaShaft { get; } = new("RinkaShaft",
    [
        Open,
        Open,
        Open,
    ]);
    /// <summary>The Mother Brain Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition MotherBrainSave { get; } = new("MotherBrainSave",
    [
        Top,
    ]);
    /// <summary>The Tourian Escape 1 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape1 { get; } = new("TourianEscape1",
    [
        Open, Open,
    ]);
    /// <summary>The Tourian Escape 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape2 { get; } = new("TourianEscape2",
    [
        Open,
        Top,
    ]);
    /// <summary>The Tourian Escape 3 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape3 { get; } = new("TourianEscape3",
    [
        Open, Open, Open, Open, Open, Open,
        Top, Top, Top, Top, Top, Top,
    ]);
    /// <summary>The Tourian Escape 4 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape4 { get; } = new("TourianEscape4",
    [
        Open, Open, Open,
        Open, Open, Open,
        Open, Open, Open,
        Open, Open, Open,
        Open, Open, Open,
        Top, Top, Top,
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
