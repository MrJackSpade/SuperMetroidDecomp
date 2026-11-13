namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The setup a door runs while Samus passes through it: the complete set the retail doors name, plus none. Each is named
/// for the room its doors lead into and what it does there: a scroll setup names the screens of that room it closes to
/// the camera, opens, and opens with the camera aligned to the screen's top, each as its column and row in the room
/// (in the summaries below, as (column, row)). A setup is what it does, so one member names each change in each room,
/// whichever door runs it.
/// The values are arbitrary; the import pipeline maps the doors' setups to them (#1414).
/// </summary>
public enum DoorSetupCode
{
    /// <summary>The door definition names no setup.</summary>
    None = 1,

    /// <summary>
    /// Entering the WreckedShipEntrance room from the west, starts its treadmill moving rightwards and spawns the
    /// treadmill object of its west entrance.
    /// </summary>
    WreckedShipEntranceStartsTreadmillRightwards = 2,

    /// <summary>Entering the Parlor room, opens screen (1, 1).</summary>
    ParlorOpensScreen1x1 = 3,

    /// <summary>Entering the Parlor room, opens screen (0, 0) with the camera aligned to the screen's top.</summary>
    ParlorOpensScreen0x0TopAligned = 4,

    /// <summary>
    /// Entering the LandingSite room, opens screen (1, 2) with the camera aligned to the screen's top.
    /// </summary>
    LandingSiteOpensScreen1x2TopAligned = 5,

    /// <summary>Entering the Parlor room, closes screen (4, 0); opens screen (3, 1).</summary>
    ParlorClosesScreen4x0OpensScreen3x1 = 6,

    /// <summary>Entering the BowlingAlley room, closes screens (2, 1), (3, 1), (4, 1) and (5, 1).</summary>
    BowlingAlleyClosesScreens2x1And3x1And4x1And5x1 = 7,

    /// <summary>
    /// Entering the BowlingAlley room, closes screens (2, 0), (3, 0), (4, 0), (5, 0), (5, 1), (0, 2), (1, 2) and
    /// (5, 2).
    /// </summary>
    BowlingAlleyClosesScreens2x0And3x0And4x0And5x0And5x1And0x2And1x2And5x2 = 8,

    /// <summary>Entering the CrateriaKihunter room, opens screens (1, 0) and (1, 1).</summary>
    CrateriaKihunterOpensScreens1x0And1x1 = 9,

    /// <summary>Entering the Climb room, opens screen (2, 0) with the camera aligned to the screen's top.</summary>
    ClimbOpensScreen2x0TopAligned = 10,

    /// <summary>Entering the Climb room, opens screen (2, 7) with the camera aligned to the screen's top.</summary>
    ClimbOpensScreen2x7TopAligned = 11,

    /// <summary>
    /// Entering the GauntletETank room, opens screen (4, 0) with the camera aligned to the screen's top.
    /// </summary>
    GauntletETankOpensScreen4x0TopAligned = 12,

    /// <summary>Entering the Caterpillar room, opens screen (0, 2).</summary>
    CaterpillarOpensScreen0x2 = 13,

    /// <summary>Entering the GreenPiratesShaft room, opens screen (0, 3).</summary>
    GreenPiratesShaftOpensScreen0x3 = 14,

    /// <summary>Entering the GreenBrinstarMainShaft room, opens screens (0, 6) and (0, 7).</summary>
    GreenBrinstarMainShaftOpensScreens0x6And0x7 = 15,

    /// <summary>
    /// Entering the EtecoonETank room, opens screens (0, 1) and (1, 1) with the camera aligned to the screen's top.
    /// </summary>
    EtecoonETankOpensScreens0x1And1x1TopAligned = 16,

    /// <summary>
    /// Entering the GreenBrinstarMainShaft room, opens screen (1, 7) with the camera aligned to the screen's top.
    /// </summary>
    GreenBrinstarMainShaftOpensScreen1x7TopAligned = 17,

    /// <summary>Entering the PinkBrinstarPowerBombs room, opens screens (0, 1) and (1, 1).</summary>
    PinkBrinstarPowerBombsOpensScreens0x1And1x1 = 18,

    /// <summary>Entering the WarehouseEntrance room, closes screen (0, 0); opens screen (1, 0).</summary>
    WarehouseEntranceClosesScreen0x0OpensScreen1x0 = 19,

    /// <summary>Entering the Dachora room, opens screen (4, 1).</summary>
    DachoraOpensScreen4x1 = 20,

    /// <summary>
    /// Entering the BigPink room, closes screen (3, 5); opens screen (4, 5) with the camera aligned to the screen's
    /// top.
    /// </summary>
    BigPinkClosesScreen3x5OpensScreen4x5TopAligned = 21,

    /// <summary>Entering the Dachora room, closes screen (4, 0).</summary>
    DachoraClosesScreen4x0 = 22,

    /// <summary>Entering the BigPink room, opens screens (2, 6), (1, 7) and (2, 7).</summary>
    BigPinkOpensScreens2x6And1x7And2x7 = 23,

    /// <summary>
    /// Entering the BlueBrinstarETank room, opens screen (2, 0) with the camera aligned to the screen's top.
    /// </summary>
    BlueBrinstarETankOpensScreen2x0TopAligned = 24,

    /// <summary>Entering the KraidEyeDoor room, opens screen (0, 0).</summary>
    KraidEyeDoorOpensScreen0x0 = 25,

    /// <summary>Entering the RedTower room, opens screens (0, 6) and (0, 7).</summary>
    RedTowerOpensScreens0x6And0x7 = 26,

    /// <summary>
    /// Entering the RedFish room, closes screen (2, 0); opens screen (1, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    RedFishClosesScreen2x0OpensScreen1x0TopAligned = 27,

    /// <summary>
    /// Entering the SporeSpawnSuper room, closes screen (1, 1); opens screen (1, 0) with the camera aligned to the
    /// screen's top.
    /// </summary>
    SporeSpawnSuperClosesScreen1x1OpensScreen1x0TopAligned = 28,

    /// <summary>
    /// Entering the EastTunnel room, closes screen (0, 0); opens screen (0, 1) with the camera aligned to the screen's
    /// top.
    /// </summary>
    EastTunnelClosesScreen0x0OpensScreen0x1TopAligned = 29,

    /// <summary>
    /// Entering the WarehouseKihunter room, opens screens (2, 0) and (3, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    WarehouseKihunterOpensScreens2x0And3x0TopAligned = 30,

    /// <summary>Entering the BelowSpazer room, opens screens (0, 0) and (1, 0).</summary>
    BelowSpazerOpensScreens0x0And1x0 = 31,

    /// <summary>Entering the BigPink room, opens screen (1, 0).</summary>
    BigPinkOpensScreen1x0 = 32,

    /// <summary>Entering the Caterpillar room, opens screens (0, 5) and (0, 6).</summary>
    CaterpillarOpensScreens0x5And0x6 = 33,

    /// <summary>
    /// Entering the ConstructionZone room, opens screen (0, 0); opens screen (0, 1) with the camera aligned to the
    /// screen's top. This opens the room's two-screen vertical camera route after Samus returns from First Missile.
    /// </summary>
    ConstructionZoneOpensScreen0x0OpensScreen0x1TopAligned = 35,

    /// <summary>
    /// Entering the GreenBrinstarElev room, opens screen (0, 2), which lies below the room's own screens.
    /// </summary>
    GreenBrinstarElevOpensScreen0x2BelowTheRoom = 36,

    /// <summary>
    /// Entering the AcidStatue room, closes screens (0, 1) and (1, 1); opens screens (0, 2), (1, 2) and (2, 2) with the
    /// camera aligned to the screen's top.
    /// </summary>
    AcidStatueClosesScreens0x1And1x1OpensScreens0x2And1x2And2x2TopAligned = 37,

    /// <summary>
    /// Entering the LNSpringBallMaze room, closes screen (1, 1); opens screen (4, 0); opens screens (1, 0), (2, 0) and
    /// (3, 0) with the camera aligned to the screen's top.
    /// </summary>
    LNSpringBallMazeClosesScreen1x1OpensScreen4x0OpensScreens1x0And2x0And3x0TopAligned = 38,

    /// <summary>
    /// Entering the Crocomire room, opens screens (0, 0) and (1, 0) with the camera aligned to the screen's top.
    /// </summary>
    CrocomireOpensScreens0x0And1x0TopAligned = 39,

    /// <summary>
    /// Entering the BatCave room, closes screen (0, 1); opens screen (0, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    BatCaveClosesScreen0x1OpensScreen0x0TopAligned = 40,

    /// <summary>Entering the IceBeamGate room, opens screen (3, 1).</summary>
    IceBeamGateOpensScreen3x1 = 41,

    /// <summary>Entering the HiJumpETank room, opens screens (0, 0) and (0, 1).</summary>
    HiJumpETankOpensScreens0x0And0x1 = 42,

    /// <summary>
    /// Entering the Volcano room, closes screen (2, 2); opens screens (0, 2) and (1, 2) with the camera aligned to the
    /// screen's top.
    /// </summary>
    VolcanoClosesScreen2x2OpensScreens0x2And1x2TopAligned = 43,

    /// <summary>
    /// Entering the IceBeamSnake room, closes screen (0, 1); opens screen (1, 1) with the camera aligned to the
    /// screen's top.
    /// </summary>
    IceBeamSnakeClosesScreen0x1OpensScreen1x1TopAligned = 44,

    /// <summary>Entering the Wasteland room, opens screen (1, 1).</summary>
    WastelandOpensScreen1x1 = 45,

    /// <summary>
    /// Entering the FastPillarsSetup room, closes screen (0, 1); opens screen (0, 2) with the camera aligned to the
    /// screen's top.
    /// </summary>
    FastPillarsSetupClosesScreen0x1OpensScreen0x2TopAligned = 46,

    /// <summary>
    /// Entering the RedKihunterShaft room, closes screen (0, 1); opens screen (0, 0) with the camera aligned to the
    /// screen's top.
    /// </summary>
    RedKihunterShaftClosesScreen0x1OpensScreen0x0TopAligned = 47,

    /// <summary>
    /// Entering the LNFireflea room, closes screen (1, 1); opens screen (1, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    LNFirefleaClosesScreen1x1OpensScreen1x0TopAligned = 48,

    /// <summary>
    /// Entering the GoldenTorizo room, closes screens (1, 0), (0, 1) and (1, 1); opens screen (0, 0) with the camera
    /// aligned to the screen's top.
    /// </summary>
    GoldenTorizoClosesScreens1x0And0x1And1x1OpensScreen0x0TopAligned = 49,

    /// <summary>Entering the WorstRoomInTheGame room, opens screen (0, 0).</summary>
    WorstRoomInTheGameOpensScreen0x0 = 50,

    /// <summary>
    /// Entering the LNFireflea room, closes screen (1, 1); opens screens (0, 0) and (1, 0) with the camera aligned to
    /// the screen's top.
    /// </summary>
    LNFirefleaClosesScreen1x1OpensScreens0x0And1x0TopAligned = 51,

    /// <summary>
    /// Entering the GreenBubblesMissiles room, opens screen (0, 0) with the camera aligned to the screen's top.
    /// </summary>
    GreenBubblesMissilesOpensScreen0x0TopAligned = 53,

    /// <summary>
    /// Entering the PurpleShaft room, closes screen (0, 1); opens screen (0, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    PurpleShaftClosesScreen0x1OpensScreen0x0TopAligned = 54,

    /// <summary>
    /// Entering the IceBeamGate room, opens screen (3, 3) with the camera aligned to the screen's top.
    /// </summary>
    IceBeamGateOpensScreen3x3TopAligned = 55,

    /// <summary>
    /// Entering the KronicBoost room, closes screen (1, 1); opens screen (0, 1) with the camera aligned to the screen's
    /// top.
    /// </summary>
    KronicBoostClosesScreen1x1OpensScreen0x1TopAligned = 56,

    /// <summary>Entering the MickeyMouse room, closes screen (2, 3).</summary>
    MickeyMouseClosesScreen2x3 = 57,

    /// <summary>
    /// Entering the WreckedShipEntrance room from the east, starts its treadmill moving leftwards and spawns the
    /// treadmill object of its east entrance.
    /// </summary>
    WreckedShipEntranceStartsTreadmillLeftwards = 58,

    /// <summary>
    /// Entering the SpongeBath room, opens screen (1, 0) with the camera aligned to the screen's top.
    /// </summary>
    SpongeBathOpensScreen1x0TopAligned = 59,

    /// <summary>Entering the ElectricDeath room, opens screen (0, 0).</summary>
    ElectricDeathOpensScreen0x0 = 60,

    /// <summary>
    /// Entering the Basement room, closes screen (3, 0); opens screen (4, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    BasementClosesScreen3x0OpensScreen4x0TopAligned = 61,

    /// <summary>
    /// Entering the WreckedShipMainShaft room, opens screen (5, 6) with the camera aligned to the screen's top.
    /// </summary>
    WreckedShipMainShaftOpensScreen5x6TopAligned = 62,

    /// <summary>Entering the WreckedShipMainShaft room, opens screens (4, 6) and (4, 7).</summary>
    WreckedShipMainShaftOpensScreens4x6And4x7 = 63,

    /// <summary>
    /// Entering the BowlingAlley room, closes screens (0, 1), (1, 1), (2, 1), (3, 1), (4, 1) and (5, 1).
    /// </summary>
    BowlingAlleyClosesScreens0x1And1x1And2x1And3x1And4x1And5x1 = 64,

    /// <summary>
    /// Entering the Toilet room from the south, starts the elevatube ride: holds Samus still and sets the tube's
    /// starting height, speed and acceleration for a ride from the bottom.
    /// </summary>
    ToiletStartsElevatubeRideFromSouth = 65,

    /// <summary>
    /// Entering the Toilet room from the north, starts the elevatube ride: holds Samus still and sets the tube's
    /// starting height, speed and acceleration for a ride from the top.
    /// </summary>
    ToiletStartsElevatubeRideFromNorth = 66,

    /// <summary>
    /// Entering the PlasmaSpark room from the elevatube, ends the ride: releases Samus and stops the tube.
    /// </summary>
    PlasmaSparkEndsElevatubeRide = 67,

    /// <summary>
    /// Entering the Oasis room from the elevatube, ends the ride: releases Samus and stops the tube; opens screens
    /// (0, 0) and (0, 1).
    /// </summary>
    OasisEndsElevatubeRideOpensScreens0x0And0x1 = 68,

    /// <summary>
    /// Entering the HalfieClimb room, closes screen (0, 2); opens screen (1, 2) with the camera aligned to the screen's
    /// top.
    /// </summary>
    HalfieClimbClosesScreen0x2OpensScreen1x2TopAligned = 69,

    /// <summary>
    /// Entering the CrabHole room, closes screen (0, 0); opens screen (0, 1) with the camera aligned to the screen's
    /// top.
    /// </summary>
    CrabHoleClosesScreen0x0OpensScreen0x1TopAligned = 71,

    /// <summary>
    /// Entering the Caterpillar room, closes screen (0, 3); opens screen (1, 3) with the camera aligned to the screen's
    /// top.
    /// </summary>
    CaterpillarClosesScreen0x3OpensScreen1x3TopAligned = 72,

    /// <summary>
    /// Entering the GlassTunnel room, closes screens (0, 0) and (0, 2); opens screen (0, 1) with the camera aligned to
    /// the screen's top.
    /// </summary>
    GlassTunnelClosesScreens0x0And0x2OpensScreen0x1TopAligned = 73,

    /// <summary>Entering the Botwoon room, opens screen (1, 0) with the camera aligned to the screen's top.</summary>
    BotwoonOpensScreen1x0TopAligned = 74,

    /// <summary>Entering the CrabShaft room, opens screen (0, 3) with the camera aligned to the screen's top.</summary>
    CrabShaftOpensScreen0x3TopAligned = 75,

    /// <summary>Entering the Pants room, closes screen (0, 2).</summary>
    PantsClosesScreen0x2 = 76,

    /// <summary>Entering the Pants room, closes screens (0, 2) and (1, 3).</summary>
    PantsClosesScreens0x2And1x3 = 77,

    /// <summary>
    /// Entering the EastPants room, closes screen (0, 2); opens screen (0, 1) with the camera aligned to the screen's
    /// top.
    /// </summary>
    EastPantsClosesScreen0x2OpensScreen0x1TopAligned = 78,

    /// <summary>Entering the ThePrecious room, opens screens (0, 0) and (0, 1).</summary>
    ThePreciousOpensScreens0x0And0x1 = 79,

    /// <summary>Entering the Statues room, opens screens (0, 0) and (0, 1).</summary>
    StatuesOpensScreens0x0And0x1 = 80,

    /// <summary>
    /// Entering the Climb room, closes screen (1, 8); opens screen (0, 8) with the camera aligned to the screen's top.
    /// </summary>
    ClimbClosesScreen1x8OpensScreen0x8TopAligned = 81,

    /// <summary>
    /// Entering the CeresElev room, shows the room as a turning floor, unrotated, about room point (128, 1008).
    /// </summary>
    CeresElevShowsTurningFloor = 82,

    /// <summary>
    /// Entering the FallingTile room from the Ceres elevator shaft, returns the screen from the turning floor to the
    /// ordinary room display.
    /// </summary>
    FallingTileEndsTurningFloor = 83,
}
