using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// Every retail door setup (#1721), plus none. Each is named for the room its doors lead into and what it does there: the
/// screens of that room it closes to the camera, opens, and opens with the camera aligned to the screen's top, each as
/// its column and row in the room (in the summaries below, as (column, row)), and the room event it starts. A setup is
/// what it does, so one member names each change in each room, whichever door runs it. A screen change names its screen
/// by that column and row, a row past the room's marked as below the room; a setup changes its screens in the order
/// listed.
/// </summary>
public static class DoorSetupDefinitions
{
    /// <summary>The door definition names no setup.</summary>
    public static DoorSetup None { get; } =
        new(nameof(None), []);

    /// <summary>
    /// Entering the WreckedShipEntrance room from the west, starts its treadmill moving rightwards and spawns the
    /// treadmill object of its west entrance.
    /// </summary>
    public static DoorSetup WreckedShipEntranceStartsTreadmillRightwards { get; } =
        new(nameof(WreckedShipEntranceStartsTreadmillRightwards), [], DoorRoomEvent.StartsTreadmillRightwards);

    /// <summary>Entering the Parlor room, opens screen (1, 1).</summary>
    public static DoorSetup ParlorOpensScreen1x1 { get; } =
        new(nameof(ParlorOpensScreen1x1), [new(new(1, 1), RoomScrollState.Open)]);

    /// <summary>Entering the Parlor room, opens screen (0, 0) with the camera aligned to the screen's top.</summary>
    public static DoorSetup ParlorOpensScreen0x0TopAligned { get; } =
        new(nameof(ParlorOpensScreen0x0TopAligned), [new(new(0, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the LandingSite room, opens screen (1, 2) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup LandingSiteOpensScreen1x2TopAligned { get; } =
        new(nameof(LandingSiteOpensScreen1x2TopAligned), [new(new(1, 2), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the Parlor room, closes screen (4, 0); opens screen (3, 1).</summary>
    public static DoorSetup ParlorClosesScreen4x0OpensScreen3x1 { get; } =
        new(nameof(ParlorClosesScreen4x0OpensScreen3x1), [new(new(4, 0), RoomScrollState.Closed), new(new(3, 1), RoomScrollState.Open)]);

    /// <summary>Entering the BowlingAlley room, closes screens (2, 1), (3, 1), (4, 1) and (5, 1).</summary>
    public static DoorSetup BowlingAlleyClosesScreens2x1And3x1And4x1And5x1 { get; } =
        new(nameof(BowlingAlleyClosesScreens2x1And3x1And4x1And5x1), [new(new(2, 1), RoomScrollState.Closed), new(new(3, 1), RoomScrollState.Closed), new(new(4, 1), RoomScrollState.Closed), new(new(5, 1), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the BowlingAlley room, closes screens (2, 0), (3, 0), (4, 0), (5, 0), (5, 1), (0, 2), (1, 2) and
    /// (5, 2).
    /// </summary>
    public static DoorSetup BowlingAlleyClosesScreens2x0And3x0And4x0And5x0And5x1And0x2And1x2And5x2 { get; } =
        new(nameof(BowlingAlleyClosesScreens2x0And3x0And4x0And5x0And5x1And0x2And1x2And5x2), [new(new(2, 0), RoomScrollState.Closed), new(new(3, 0), RoomScrollState.Closed), new(new(4, 0), RoomScrollState.Closed), new(new(5, 0), RoomScrollState.Closed), new(new(5, 1), RoomScrollState.Closed), new(new(0, 2), RoomScrollState.Closed), new(new(1, 2), RoomScrollState.Closed), new(new(5, 2), RoomScrollState.Closed)]);

    /// <summary>Entering the CrateriaKihunter room, opens screens (1, 0) and (1, 1).</summary>
    public static DoorSetup CrateriaKihunterOpensScreens1x0And1x1 { get; } =
        new(nameof(CrateriaKihunterOpensScreens1x0And1x1), [new(new(1, 0), RoomScrollState.Open), new(new(1, 1), RoomScrollState.Open)]);

    /// <summary>Entering the Climb room, opens screen (2, 0) with the camera aligned to the screen's top.</summary>
    public static DoorSetup ClimbOpensScreen2x0TopAligned { get; } =
        new(nameof(ClimbOpensScreen2x0TopAligned), [new(new(2, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the Climb room, opens screen (2, 7) with the camera aligned to the screen's top.</summary>
    public static DoorSetup ClimbOpensScreen2x7TopAligned { get; } =
        new(nameof(ClimbOpensScreen2x7TopAligned), [new(new(2, 7), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the GauntletETank room, opens screen (4, 0) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup GauntletETankOpensScreen4x0TopAligned { get; } =
        new(nameof(GauntletETankOpensScreen4x0TopAligned), [new(new(4, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the Caterpillar room, opens screen (0, 2).</summary>
    public static DoorSetup CaterpillarOpensScreen0x2 { get; } =
        new(nameof(CaterpillarOpensScreen0x2), [new(new(0, 2), RoomScrollState.Open)]);

    /// <summary>Entering the GreenPiratesShaft room, opens screen (0, 3).</summary>
    public static DoorSetup GreenPiratesShaftOpensScreen0x3 { get; } =
        new(nameof(GreenPiratesShaftOpensScreen0x3), [new(new(0, 3), RoomScrollState.Open)]);

    /// <summary>Entering the GreenBrinstarMainShaft room, opens screens (0, 6) and (0, 7).</summary>
    public static DoorSetup GreenBrinstarMainShaftOpensScreens0x6And0x7 { get; } =
        new(nameof(GreenBrinstarMainShaftOpensScreens0x6And0x7), [new(new(0, 6), RoomScrollState.Open), new(new(0, 7), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the EtecoonETank room, opens screens (0, 1) and (1, 1) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup EtecoonETankOpensScreens0x1And1x1TopAligned { get; } =
        new(nameof(EtecoonETankOpensScreens0x1And1x1TopAligned), [new(new(0, 1), RoomScrollState.OpenAlignedToTop), new(new(1, 1), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the GreenBrinstarMainShaft room, opens screen (1, 7) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup GreenBrinstarMainShaftOpensScreen1x7TopAligned { get; } =
        new(nameof(GreenBrinstarMainShaftOpensScreen1x7TopAligned), [new(new(1, 7), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the PinkBrinstarPowerBombs room, opens screens (0, 1) and (1, 1).</summary>
    public static DoorSetup PinkBrinstarPowerBombsOpensScreens0x1And1x1 { get; } =
        new(nameof(PinkBrinstarPowerBombsOpensScreens0x1And1x1), [new(new(0, 1), RoomScrollState.Open), new(new(1, 1), RoomScrollState.Open)]);

    /// <summary>Entering the WarehouseEntrance room, closes screen (0, 0); opens screen (1, 0).</summary>
    public static DoorSetup WarehouseEntranceClosesScreen0x0OpensScreen1x0 { get; } =
        new(nameof(WarehouseEntranceClosesScreen0x0OpensScreen1x0), [new(new(0, 0), RoomScrollState.Closed), new(new(1, 0), RoomScrollState.Open)]);

    /// <summary>Entering the Dachora room, opens screen (4, 1).</summary>
    public static DoorSetup DachoraOpensScreen4x1 { get; } =
        new(nameof(DachoraOpensScreen4x1), [new(new(4, 1), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the BigPink room, closes screen (3, 5); opens screen (4, 5) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup BigPinkClosesScreen3x5OpensScreen4x5TopAligned { get; } =
        new(nameof(BigPinkClosesScreen3x5OpensScreen4x5TopAligned), [new(new(3, 5), RoomScrollState.Closed), new(new(4, 5), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the Dachora room, closes screen (4, 0).</summary>
    public static DoorSetup DachoraClosesScreen4x0 { get; } =
        new(nameof(DachoraClosesScreen4x0), [new(new(4, 0), RoomScrollState.Closed)]);

    /// <summary>Entering the BigPink room, opens screens (2, 6), (1, 7) and (2, 7).</summary>
    public static DoorSetup BigPinkOpensScreens2x6And1x7And2x7 { get; } =
        new(nameof(BigPinkOpensScreens2x6And1x7And2x7), [new(new(2, 6), RoomScrollState.Open), new(new(1, 7), RoomScrollState.Open), new(new(2, 7), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the BlueBrinstarETank room, opens screen (2, 0) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup BlueBrinstarETankOpensScreen2x0TopAligned { get; } =
        new(nameof(BlueBrinstarETankOpensScreen2x0TopAligned), [new(new(2, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the KraidEyeDoor room, opens screen (0, 0).</summary>
    public static DoorSetup KraidEyeDoorOpensScreen0x0 { get; } =
        new(nameof(KraidEyeDoorOpensScreen0x0), [new(new(0, 0), RoomScrollState.Open)]);

    /// <summary>Entering the RedTower room, opens screens (0, 6) and (0, 7).</summary>
    public static DoorSetup RedTowerOpensScreens0x6And0x7 { get; } =
        new(nameof(RedTowerOpensScreens0x6And0x7), [new(new(0, 6), RoomScrollState.Open), new(new(0, 7), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the RedFish room, closes screen (2, 0); opens screen (1, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup RedFishClosesScreen2x0OpensScreen1x0TopAligned { get; } =
        new(nameof(RedFishClosesScreen2x0OpensScreen1x0TopAligned), [new(new(1, 0), RoomScrollState.OpenAlignedToTop), new(new(2, 0), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the SporeSpawnSuper room, closes screen (1, 1); opens screen (1, 0) with the camera aligned to the
    /// screen's top.
    /// </summary>
    public static DoorSetup SporeSpawnSuperClosesScreen1x1OpensScreen1x0TopAligned { get; } =
        new(nameof(SporeSpawnSuperClosesScreen1x1OpensScreen1x0TopAligned), [new(new(1, 0), RoomScrollState.OpenAlignedToTop), new(new(1, 1), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the EastTunnel room, closes screen (0, 0); opens screen (0, 1) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup EastTunnelClosesScreen0x0OpensScreen0x1TopAligned { get; } =
        new(nameof(EastTunnelClosesScreen0x0OpensScreen0x1TopAligned), [new(new(0, 0), RoomScrollState.Closed), new(new(0, 1), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the WarehouseKihunter room, opens screens (2, 0) and (3, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup WarehouseKihunterOpensScreens2x0And3x0TopAligned { get; } =
        new(nameof(WarehouseKihunterOpensScreens2x0And3x0TopAligned), [new(new(2, 0), RoomScrollState.OpenAlignedToTop), new(new(3, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the BelowSpazer room, opens screens (0, 0) and (1, 0).</summary>
    public static DoorSetup BelowSpazerOpensScreens0x0And1x0 { get; } =
        new(nameof(BelowSpazerOpensScreens0x0And1x0), [new(new(0, 0), RoomScrollState.Open), new(new(1, 0), RoomScrollState.Open)]);

    /// <summary>Entering the BigPink room, opens screen (1, 0).</summary>
    public static DoorSetup BigPinkOpensScreen1x0 { get; } =
        new(nameof(BigPinkOpensScreen1x0), [new(new(1, 0), RoomScrollState.Open)]);

    /// <summary>Entering the Caterpillar room, opens screens (0, 5) and (0, 6).</summary>
    public static DoorSetup CaterpillarOpensScreens0x5And0x6 { get; } =
        new(nameof(CaterpillarOpensScreens0x5And0x6), [new(new(0, 5), RoomScrollState.Open), new(new(0, 6), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the ConstructionZone room, opens screen (0, 0); opens screen (0, 1) with the camera aligned to the
    /// screen's top. This opens the room's two-screen vertical camera route after Samus returns from First Missile.
    /// </summary>
    public static DoorSetup ConstructionZoneOpensScreen0x0OpensScreen0x1TopAligned { get; } =
        new(nameof(ConstructionZoneOpensScreen0x0OpensScreen0x1TopAligned), [new(new(0, 0), RoomScrollState.Open), new(new(0, 1), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the GreenBrinstarElev room, opens screen (0, 2), which lies below the room's own screens.
    /// </summary>
    public static DoorSetup GreenBrinstarElevOpensScreen0x2BelowTheRoom { get; } =
        new(nameof(GreenBrinstarElevOpensScreen0x2BelowTheRoom), [new(new(0, 2), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the AcidStatue room, closes screens (0, 1) and (1, 1); opens screens (0, 2), (1, 2) and (2, 2) with the
    /// camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup AcidStatueClosesScreens0x1And1x1OpensScreens0x2And1x2And2x2TopAligned { get; } =
        new(nameof(AcidStatueClosesScreens0x1And1x1OpensScreens0x2And1x2And2x2TopAligned), [new(new(0, 1), RoomScrollState.Closed), new(new(1, 1), RoomScrollState.Closed), new(new(0, 2), RoomScrollState.OpenAlignedToTop), new(new(1, 2), RoomScrollState.OpenAlignedToTop), new(new(2, 2), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the LNSpringBallMaze room, closes screen (1, 1); opens screen (4, 0); opens screens (1, 0), (2, 0) and
    /// (3, 0) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup LNSpringBallMazeClosesScreen1x1OpensScreen4x0OpensScreens1x0And2x0And3x0TopAligned { get; } =
        new(nameof(LNSpringBallMazeClosesScreen1x1OpensScreen4x0OpensScreens1x0And2x0And3x0TopAligned), [new(new(1, 0), RoomScrollState.OpenAlignedToTop), new(new(2, 0), RoomScrollState.OpenAlignedToTop), new(new(3, 0), RoomScrollState.OpenAlignedToTop), new(new(4, 0), RoomScrollState.Open), new(new(1, 1), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the Crocomire room, opens screens (0, 0) and (1, 0) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup CrocomireOpensScreens0x0And1x0TopAligned { get; } =
        new(nameof(CrocomireOpensScreens0x0And1x0TopAligned), [new(new(0, 0), RoomScrollState.OpenAlignedToTop), new(new(1, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the BatCave room, closes screen (0, 1); opens screen (0, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup BatCaveClosesScreen0x1OpensScreen0x0TopAligned { get; } =
        new(nameof(BatCaveClosesScreen0x1OpensScreen0x0TopAligned), [new(new(0, 0), RoomScrollState.OpenAlignedToTop), new(new(0, 1), RoomScrollState.Closed)]);

    /// <summary>Entering the IceBeamGate room, opens screen (3, 1).</summary>
    public static DoorSetup IceBeamGateOpensScreen3x1 { get; } =
        new(nameof(IceBeamGateOpensScreen3x1), [new(new(3, 1), RoomScrollState.Open)]);

    /// <summary>Entering the HiJumpETank room, opens screens (0, 0) and (0, 1).</summary>
    public static DoorSetup HiJumpETankOpensScreens0x0And0x1 { get; } =
        new(nameof(HiJumpETankOpensScreens0x0And0x1), [new(new(0, 0), RoomScrollState.Open), new(new(0, 1), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the Volcano room, closes screen (2, 2); opens screens (0, 2) and (1, 2) with the camera aligned to the
    /// screen's top.
    /// </summary>
    public static DoorSetup VolcanoClosesScreen2x2OpensScreens0x2And1x2TopAligned { get; } =
        new(nameof(VolcanoClosesScreen2x2OpensScreens0x2And1x2TopAligned), [new(new(0, 2), RoomScrollState.OpenAlignedToTop), new(new(1, 2), RoomScrollState.OpenAlignedToTop), new(new(2, 2), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the IceBeamSnake room, closes screen (0, 1); opens screen (1, 1) with the camera aligned to the
    /// screen's top.
    /// </summary>
    public static DoorSetup IceBeamSnakeClosesScreen0x1OpensScreen1x1TopAligned { get; } =
        new(nameof(IceBeamSnakeClosesScreen0x1OpensScreen1x1TopAligned), [new(new(0, 1), RoomScrollState.Closed), new(new(1, 1), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the Wasteland room, opens screen (1, 1).</summary>
    public static DoorSetup WastelandOpensScreen1x1 { get; } =
        new(nameof(WastelandOpensScreen1x1), [new(new(1, 1), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the FastPillarsSetup room, closes screen (0, 1); opens screen (0, 2) with the camera aligned to the
    /// screen's top.
    /// </summary>
    public static DoorSetup FastPillarsSetupClosesScreen0x1OpensScreen0x2TopAligned { get; } =
        new(nameof(FastPillarsSetupClosesScreen0x1OpensScreen0x2TopAligned), [new(new(0, 1), RoomScrollState.Closed), new(new(0, 2), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the RedKihunterShaft room, closes screen (0, 1); opens screen (0, 0) with the camera aligned to the
    /// screen's top.
    /// </summary>
    public static DoorSetup RedKihunterShaftClosesScreen0x1OpensScreen0x0TopAligned { get; } =
        new(nameof(RedKihunterShaftClosesScreen0x1OpensScreen0x0TopAligned), [new(new(0, 0), RoomScrollState.OpenAlignedToTop), new(new(0, 1), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the LNFireflea room, closes screen (1, 1); opens screen (1, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup LNFirefleaClosesScreen1x1OpensScreen1x0TopAligned { get; } =
        new(nameof(LNFirefleaClosesScreen1x1OpensScreen1x0TopAligned), [new(new(1, 0), RoomScrollState.OpenAlignedToTop), new(new(1, 1), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the GoldenTorizo room, closes screens (1, 0), (0, 1) and (1, 1); opens screen (0, 0) with the camera
    /// aligned to the screen's top.
    /// </summary>
    public static DoorSetup GoldenTorizoClosesScreens1x0And0x1And1x1OpensScreen0x0TopAligned { get; } =
        new(nameof(GoldenTorizoClosesScreens1x0And0x1And1x1OpensScreen0x0TopAligned), [new(new(0, 0), RoomScrollState.OpenAlignedToTop), new(new(1, 0), RoomScrollState.Closed), new(new(0, 1), RoomScrollState.Closed), new(new(1, 1), RoomScrollState.Closed)]);

    /// <summary>Entering the WorstRoomInTheGame room, opens screen (0, 0).</summary>
    public static DoorSetup WorstRoomInTheGameOpensScreen0x0 { get; } =
        new(nameof(WorstRoomInTheGameOpensScreen0x0), [new(new(0, 0), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the LNFireflea room, closes screen (1, 1); opens screens (0, 0) and (1, 0) with the camera aligned to
    /// the screen's top.
    /// </summary>
    public static DoorSetup LNFirefleaClosesScreen1x1OpensScreens0x0And1x0TopAligned { get; } =
        new(nameof(LNFirefleaClosesScreen1x1OpensScreens0x0And1x0TopAligned), [new(new(0, 0), RoomScrollState.OpenAlignedToTop), new(new(1, 0), RoomScrollState.OpenAlignedToTop), new(new(1, 1), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the GreenBubblesMissiles room, opens screen (0, 0) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup GreenBubblesMissilesOpensScreen0x0TopAligned { get; } =
        new(nameof(GreenBubblesMissilesOpensScreen0x0TopAligned), [new(new(0, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the PurpleShaft room, closes screen (0, 1); opens screen (0, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup PurpleShaftClosesScreen0x1OpensScreen0x0TopAligned { get; } =
        new(nameof(PurpleShaftClosesScreen0x1OpensScreen0x0TopAligned), [new(new(0, 0), RoomScrollState.OpenAlignedToTop), new(new(0, 1), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the IceBeamGate room, opens screen (3, 3) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup IceBeamGateOpensScreen3x3TopAligned { get; } =
        new(nameof(IceBeamGateOpensScreen3x3TopAligned), [new(new(3, 3), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the KronicBoost room, closes screen (1, 1); opens screen (0, 1) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup KronicBoostClosesScreen1x1OpensScreen0x1TopAligned { get; } =
        new(nameof(KronicBoostClosesScreen1x1OpensScreen0x1TopAligned), [new(new(0, 1), RoomScrollState.OpenAlignedToTop), new(new(1, 1), RoomScrollState.Closed)]);

    /// <summary>Entering the MickeyMouse room, closes screen (2, 3).</summary>
    public static DoorSetup MickeyMouseClosesScreen2x3 { get; } =
        new(nameof(MickeyMouseClosesScreen2x3), [new(new(2, 3), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the WreckedShipEntrance room from the east, starts its treadmill moving leftwards and spawns the
    /// treadmill object of its east entrance.
    /// </summary>
    public static DoorSetup WreckedShipEntranceStartsTreadmillLeftwards { get; } =
        new(nameof(WreckedShipEntranceStartsTreadmillLeftwards), [], DoorRoomEvent.StartsTreadmillLeftwards);

    /// <summary>
    /// Entering the SpongeBath room, opens screen (1, 0) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup SpongeBathOpensScreen1x0TopAligned { get; } =
        new(nameof(SpongeBathOpensScreen1x0TopAligned), [new(new(1, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the ElectricDeath room, opens screen (0, 0).</summary>
    public static DoorSetup ElectricDeathOpensScreen0x0 { get; } =
        new(nameof(ElectricDeathOpensScreen0x0), [new(new(0, 0), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the Basement room, closes screen (3, 0); opens screen (4, 0) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup BasementClosesScreen3x0OpensScreen4x0TopAligned { get; } =
        new(nameof(BasementClosesScreen3x0OpensScreen4x0TopAligned), [new(new(3, 0), RoomScrollState.Closed), new(new(4, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the WreckedShipMainShaft room, opens screen (5, 6) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup WreckedShipMainShaftOpensScreen5x6TopAligned { get; } =
        new(nameof(WreckedShipMainShaftOpensScreen5x6TopAligned), [new(new(5, 6), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the WreckedShipMainShaft room, opens screens (4, 6) and (4, 7).</summary>
    public static DoorSetup WreckedShipMainShaftOpensScreens4x6And4x7 { get; } =
        new(nameof(WreckedShipMainShaftOpensScreens4x6And4x7), [new(new(4, 6), RoomScrollState.Open), new(new(4, 7), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the BowlingAlley room, closes screens (0, 1), (1, 1), (2, 1), (3, 1), (4, 1) and (5, 1).
    /// </summary>
    public static DoorSetup BowlingAlleyClosesScreens0x1And1x1And2x1And3x1And4x1And5x1 { get; } =
        new(nameof(BowlingAlleyClosesScreens0x1And1x1And2x1And3x1And4x1And5x1), [new(new(0, 1), RoomScrollState.Closed), new(new(1, 1), RoomScrollState.Closed), new(new(2, 1), RoomScrollState.Closed), new(new(3, 1), RoomScrollState.Closed), new(new(4, 1), RoomScrollState.Closed), new(new(5, 1), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the Toilet room from the south, starts the elevatube ride: holds Samus still and sets the tube's
    /// starting height, speed and acceleration for a ride from the bottom.
    /// </summary>
    public static DoorSetup ToiletStartsElevatubeRideFromSouth { get; } =
        new(nameof(ToiletStartsElevatubeRideFromSouth), [], DoorRoomEvent.StartsElevatubeRideFromSouth);

    /// <summary>
    /// Entering the Toilet room from the north, starts the elevatube ride: holds Samus still and sets the tube's
    /// starting height, speed and acceleration for a ride from the top.
    /// </summary>
    public static DoorSetup ToiletStartsElevatubeRideFromNorth { get; } =
        new(nameof(ToiletStartsElevatubeRideFromNorth), [], DoorRoomEvent.StartsElevatubeRideFromNorth);

    /// <summary>
    /// Entering the PlasmaSpark room from the elevatube, ends the ride: releases Samus and stops the tube.
    /// </summary>
    public static DoorSetup PlasmaSparkEndsElevatubeRide { get; } =
        new(nameof(PlasmaSparkEndsElevatubeRide), [], DoorRoomEvent.EndsElevatubeRide);

    /// <summary>
    /// Entering the Oasis room from the elevatube, ends the ride: releases Samus and stops the tube; opens screens
    /// (0, 0) and (0, 1).
    /// </summary>
    public static DoorSetup OasisEndsElevatubeRideOpensScreens0x0And0x1 { get; } =
        new(nameof(OasisEndsElevatubeRideOpensScreens0x0And0x1), [new(new(0, 0), RoomScrollState.Open), new(new(0, 1), RoomScrollState.Open)], DoorRoomEvent.EndsElevatubeRide);

    /// <summary>
    /// Entering the HalfieClimb room, closes screen (0, 2); opens screen (1, 2) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup HalfieClimbClosesScreen0x2OpensScreen1x2TopAligned { get; } =
        new(nameof(HalfieClimbClosesScreen0x2OpensScreen1x2TopAligned), [new(new(0, 2), RoomScrollState.Closed), new(new(1, 2), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the CrabHole room, closes screen (0, 0); opens screen (0, 1) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup CrabHoleClosesScreen0x0OpensScreen0x1TopAligned { get; } =
        new(nameof(CrabHoleClosesScreen0x0OpensScreen0x1TopAligned), [new(new(0, 0), RoomScrollState.Closed), new(new(0, 1), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the Caterpillar room, closes screen (0, 3); opens screen (1, 3) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup CaterpillarClosesScreen0x3OpensScreen1x3TopAligned { get; } =
        new(nameof(CaterpillarClosesScreen0x3OpensScreen1x3TopAligned), [new(new(0, 3), RoomScrollState.Closed), new(new(1, 3), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>
    /// Entering the GlassTunnel room, closes screens (0, 0) and (0, 2); opens screen (0, 1) with the camera aligned to
    /// the screen's top.
    /// </summary>
    public static DoorSetup GlassTunnelClosesScreens0x0And0x2OpensScreen0x1TopAligned { get; } =
        new(nameof(GlassTunnelClosesScreens0x0And0x2OpensScreen0x1TopAligned), [new(new(0, 0), RoomScrollState.Closed), new(new(0, 2), RoomScrollState.Closed), new(new(0, 1), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the Botwoon room, opens screen (1, 0) with the camera aligned to the screen's top.</summary>
    public static DoorSetup BotwoonOpensScreen1x0TopAligned { get; } =
        new(nameof(BotwoonOpensScreen1x0TopAligned), [new(new(1, 0), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the CrabShaft room, opens screen (0, 3) with the camera aligned to the screen's top.</summary>
    public static DoorSetup CrabShaftOpensScreen0x3TopAligned { get; } =
        new(nameof(CrabShaftOpensScreen0x3TopAligned), [new(new(0, 3), RoomScrollState.OpenAlignedToTop)]);

    /// <summary>Entering the Pants room, closes screen (0, 2).</summary>
    public static DoorSetup PantsClosesScreen0x2 { get; } =
        new(nameof(PantsClosesScreen0x2), [new(new(0, 2), RoomScrollState.Closed)]);

    /// <summary>Entering the Pants room, closes screens (0, 2) and (1, 3).</summary>
    public static DoorSetup PantsClosesScreens0x2And1x3 { get; } =
        new(nameof(PantsClosesScreens0x2And1x3), [new(new(0, 2), RoomScrollState.Closed), new(new(1, 3), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the EastPants room, closes screen (0, 2); opens screen (0, 1) with the camera aligned to the screen's
    /// top.
    /// </summary>
    public static DoorSetup EastPantsClosesScreen0x2OpensScreen0x1TopAligned { get; } =
        new(nameof(EastPantsClosesScreen0x2OpensScreen0x1TopAligned), [new(new(0, 1), RoomScrollState.OpenAlignedToTop), new(new(0, 2), RoomScrollState.Closed)]);

    /// <summary>Entering the ThePrecious room, opens screens (0, 0) and (0, 1).</summary>
    public static DoorSetup ThePreciousOpensScreens0x0And0x1 { get; } =
        new(nameof(ThePreciousOpensScreens0x0And0x1), [new(new(0, 0), RoomScrollState.Open), new(new(0, 1), RoomScrollState.Open)]);

    /// <summary>Entering the Statues room, opens screens (0, 0) and (0, 1).</summary>
    public static DoorSetup StatuesOpensScreens0x0And0x1 { get; } =
        new(nameof(StatuesOpensScreens0x0And0x1), [new(new(0, 0), RoomScrollState.Open), new(new(0, 1), RoomScrollState.Open)]);

    /// <summary>
    /// Entering the Climb room, closes screen (1, 8); opens screen (0, 8) with the camera aligned to the screen's top.
    /// </summary>
    public static DoorSetup ClimbClosesScreen1x8OpensScreen0x8TopAligned { get; } =
        new(nameof(ClimbClosesScreen1x8OpensScreen0x8TopAligned), [new(new(0, 8), RoomScrollState.OpenAlignedToTop), new(new(1, 8), RoomScrollState.Closed)]);

    /// <summary>
    /// Entering the CeresElev room, shows the room as a turning floor, unrotated, about room point (128, 1008).
    /// </summary>
    public static DoorSetup CeresElevShowsTurningFloor { get; } =
        new(nameof(CeresElevShowsTurningFloor), [], DoorRoomEvent.ShowsTurningFloor);

    /// <summary>
    /// Entering the FallingTile room from the Ceres elevator shaft, returns the screen from the turning floor to the
    /// ordinary room display.
    /// </summary>
    public static DoorSetup FallingTileEndsTurningFloor { get; } =
        new(nameof(FallingTileEndsTurningFloor), [], DoorRoomEvent.EndsTurningFloor);
}
