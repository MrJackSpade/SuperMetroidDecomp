namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The scroll data retail room states select: 159 explicit tables and two implicit layouts.</summary>
[NumberedIdentities("Each numbered member is the scroll layout of a distinct numbered room (Metroids1-4, TourianEscape1-4), named for that room.")]
public static class RoomScrollDefinitions
{
    /// <summary>A red cell: a camera boundary.</summary>
    private const RoomScrollState Red = RoomScrollState.RedBoundary;

    /// <summary>A blue cell.</summary>
    private const RoomScrollState Blue = RoomScrollState.Blue;

    /// <summary>A green cell.</summary>
    private const RoomScrollState Green = RoomScrollState.Green;

    /// <summary>
    /// Retail explicit tables in their authored order, each the fifty cells its load writes: the room's own screens,
    /// then the cells after them, which past a short table hold the data stored after it read as cells.
    /// </summary>
    private static readonly RoomScrollDefinition[] Definitions =
    [
        new(nameof(LandingSite),
        [
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Blue, Red,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Green,
            Green, Green, Green, Green, Green, Green, Blue, Blue, Blue, Blue,
            Blue, Blue, Blue, Blue, Blue, Green, Blue, Green, Blue, Red,
        ]),
        new(nameof(Parlor),
        [
            Red, Blue, Blue, Blue, Blue, Red, Red, Red, Red, Red,
            Red, Green, Red, Blue, Red, Red, Green, Red, Red, Red,
            Red, Green, Red, Red, Red, Green, Green, Green, Green, Red,
            Green, Red, Red, Green, Red, Green, Green, Green, Green, Red,
            Green, Red, Blue, Green, Green, Blue, Green, Green, Blue, Green,
        ]),
        new(nameof(CrateriaPowerBomb),
        [
            Blue, Blue, Green, Red, Green, Green, Blue, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Red,
            Red, Red, Red, Red, Red, Green, Green, Red, Red, Green,
            Green, Green, Green, Green, Red, Green, Red, Green, Green, Green,
        ]),
        new(nameof(WestOcean),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Red, Red, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Blue, Blue, Blue, Blue, Blue, Blue, Blue, Blue, Green, Red,
        ]),
        new(nameof(CrateriaKihunter),
        [
            Blue, Blue, Blue, Red, Red, Red, Red, Green, Red, Blue,
            Green, Green, Green, Green, Blue, Blue, Green, Red, Green, Green,
            Red, Green, Green, Blue, Blue, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
        ]),
        new(nameof(ForgottenHighwayElev),
        [
            Blue, Red, Green, Green, Green, Red, Green, Red, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Blue, Green, Green, Red, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Red,
        ]),
        new(nameof(EastOcean),
        [
            Red, Red, Red, Red, Red, Red, Red, Red, Red, Red,
            Red, Red, Red, Red, Red, Red, Red, Red, Red, Red,
            Red, Red, Red, Red, Red, Red, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Blue, Blue, Blue, Blue, Blue,
            Blue, Blue, Green, Red, Green, Green, Blue, Green, Green, Green,
        ]),
        new(nameof(ForgottenHighwayElbow),
        [
            Green, Green, Red, Green, Green, Blue, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Green, Green,
        ]),
        new(nameof(RedBinstarElev),
        [
            Blue, Red, Green, Green, Green, Red, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Blue, Green, Green,
            Red, Red, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Blue, Blue, Blue,
        ]),
        new(nameof(GauntletETank),
        [
            Blue, Blue, Blue, Blue, Red, Blue, Green, Blue, Green, Green,
            Red, Green, Green, Blue, Blue, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Blue, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Climb),
        [
            Red, Green, Red, Red, Green, Red, Red, Green, Red, Red,
            Green, Red, Red, Green, Red, Red, Green, Red, Red, Green,
            Red, Red, Green, Red, Red, Blue, Red, Green, Blue, Green,
            Green, Red, Green, Green, Blue, Green, Green, Red, Green, Green,
            Blue, Green, Green, Red, Green, Green, Blue, Green, Green, Red,
        ]),
        new(nameof(Pit),
        [
            Blue, Blue, Blue, Red, Red, Red, Red, Green, Green, Green,
            Green, Red, Blue, Green, Red, Green, Green, Red, Green, Green,
            Blue, Blue, Green, Green, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Red, Green, Red,
        ]),
        new(nameof(ElevToBlueBrinstar),
        [
            Blue, Red, Green, Green, Green, Red, Green, Green, Blue, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Red, Red, Green, Green, Green,
        ]),
        new(nameof(GreenBrinstarElev),
        [
            Blue, Red, Green, Green, Green, Red, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Green,
        ]),
        new(nameof(GreenPiratesShaft),
        [
            Green, Green, Green, Red, Green, Green, Blue, Green, Green, Green,
            Green, Red, Green, Green, Red, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Red, Red, Red, Green, Green, Green, Green,
        ]),
        new(nameof(CrateriaSuper),
        [
            Blue, Blue, Blue, Green, Red, Red, Red, Green, Red, Red,
            Red, Green, Red, Red, Red, Green, Red, Red, Red, Green,
            Red, Red, Red, Green, Red, Red, Red, Green, Blue, Blue,
            Blue, Blue, Green, Red, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Red, Green, Green, Green, Green,
        ]),
        new(nameof(FinalMissileBombway),
        [
            Blue, Blue, Green, Red, Green, Green, Blue, Blue, Green, Green,
            Red, Green, Green, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(FinalMissile),
        [
            Blue, Red, Blue, Green, Red, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Green, Green, Green,
            Red, Red, Red, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(GreenBrinstarMainShaft),
        [
            Green, Red, Red, Red, Green, Red, Red, Red, Green, Red,
            Red, Red, Green, Red, Red, Red, Green, Red, Red, Red,
            Green, Red, Red, Red, Green, Red, Red, Red, Red, Red,
            Green, Blue, Green, Red, Green, Red, Green, Red, Green, Red,
            Green, Red, Green, Red, Red, Red, Green, Red, Green, Green,
        ]),
        new(nameof(SporeSpawnSuper),
        [
            Blue, Green, Red, Green, Red, Green, Red, Green, Red, Green,
            Red, Green, Red, Green, Red, Green, Blue, Blue, Blue, Green,
            Green, Green, Green, Green, Blue, Green, Green, Green, Blue, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(EarlySupers),
        [
            Red, Red, Red, Blue, Blue, Blue, Red, Green, Blue, Green,
            Green, Green, Green, Red, Red, Blue, Red, Green, Red, Green,
            Green, Blue, Green, Green, Green, Blue, Green, Green, Red, Green,
            Green, Green, Green, Red, Green, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
        ]),
        new(nameof(BrinstarReserveTank),
        [
            Blue, Red, Blue, Blue, Green, Green, Blue, Green, Green, Blue,
            Blue, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
            Red, Red, Red, Red, Red, Red, Red, Red, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Blue, Green, Green,
        ]),
        new(nameof(GreenBrinstarMissileRefill),
        [
            Blue, Green, Blue, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Red, Red, Green, Green, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Blue, Blue, Blue, Blue,
        ]),
        new(nameof(Dachora),
        [
            Blue, Blue, Blue, Blue, Blue, Blue, Blue, Red, Red, Red,
            Red, Red, Red, Red, Red, Red, Red, Red, Green, Red,
            Red, Red, Red, Red, Red, Green, Red, Red, Red, Red,
            Red, Red, Green, Red, Red, Red, Red, Red, Red, Green,
            Red, Red, Green, Green, Green, Green, Green, Red, Red, Green,
        ]),
        new(nameof(BigPink),
        [
            Red, Red, Green, Green, Red, Red, Red, Green, Green, Red,
            Red, Red, Green, Green, Red, Red, Red, Green, Green, Red,
            Red, Red, Green, Green, Red, Red, Red, Green, Green, Red,
            Red, Red, Blue, Blue, Red, Green, Red, Red, Red, Red,
            Green, Red, Red, Red, Red, Green, Red, Red, Red, Red,
        ]),
        new(nameof(PinkBrinstarPowerBombs),
        [
            Blue, Blue, Red, Red, Red, Green, Blue, Red, Green, Blue,
            Green, Blue, Green, Red, Blue, Blue, Blue, Green, Red, Green,
            Red, Green, Green, Blue, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(GreenHillZone),
        [
            Green, Green, Red, Red, Red, Red, Red, Red, Green, Green,
            Green, Green, Red, Red, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Blue, Blue, Blue, Blue, Blue, Blue,
            Blue, Blue, Green, Blue, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Red, Green, Green, Green, Green,
        ]),
        new(nameof(MorphBall),
        [
            Red, Red, Red, Red, Red, Green, Red, Red, Red, Red,
            Red, Red, Red, Red, Red, Red, Blue, Blue, Blue, Blue,
            Blue, Blue, Blue, Blue, Green, Blue, Green, Green, Red, Green,
            Green, Green, Green, Green, Red, Green, Green, Blue, Green, Green,
            Blue, Green, Green, Green, Red, Green, Green, Green, Green, Red,
        ]),
        new(nameof(ConstructionZone),
        [
            Blue, Red, Red, Green, Blue, Blue, Green, Green, Blue, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Green, Green, Green, Green, Green,
        ]),
        new(nameof(BlueBrinstarETank),
        [
            Red, Red, Green, Red, Red, Red, Blue, Blue, Blue, Green,
            Green, Green, Green, Blue, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Red, Red, Red, Red, Green, Green, Green, Green, Green,
        ]),
        new(nameof(GreenBrinstarBeetoms),
        [
            Blue, Green, Blue, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Red, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Blue, Blue,
        ]),
        new(nameof(EtecoonETank),
        [
            Blue, Blue, Red, Red, Red, Red, Red, Blue, Blue, Blue,
            Blue, Green, Green, Blue, Green, Blue, Green, Green, Blue, Green,
            Green, Blue, Blue, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Blue, Blue, Green, Green, Red, Red, Red, Red,
        ]),
        new(nameof(EtecoonSuper),
        [
            Blue, Green, Blue, Green, Green, Blue, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Blue, Green, Green, Green, Blue, Green, Green,
        ]),
        new(nameof(SporeSpawnFarming),
        [
            Blue, Blue, Blue, Green, Blue, Green, Green, Green, Blue, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Blue,
            Green, Green, Red, Red, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Blue, Blue, Blue,
        ]),
        new(nameof(WaterwayETank),
        [
            Red, Blue, Blue, Blue, Blue, Blue, Blue, Red, Blue, Green,
            Green, Blue, Green, Green, Blue, Blue, Green, Green, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Green,
            Green, Green, Green, Green, Green, Blue, Blue, Red, Red, Red,
            Red, Red, Red, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(RedTower),
        [
            Green, Green, Green, Green, Green, Green, Blue, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Blue, Green, Green, Green,
            Green, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Red, Red, Red, Red, Green, Green,
        ]),
        new(nameof(RedBrinstarFirefleas),
        [
            Green, Green, Blue, Blue, Green, Green, Blue, Blue, Blue, Blue,
            Red, Red, Blue, Blue, Red, Red, Green, Blue, Green, Green,
            Green, Blue, Green, Green, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Red, Red, Red, Red, Green,
        ]),
        new(nameof(Caterpillar),
        [
            Green, Red, Red, Green, Red, Red, Green, Red, Red, Green,
            Red, Blue, Green, Red, Red, Blue, Red, Red, Red, Red,
            Red, Green, Red, Red, Green, Green, Green, Green, Green, Green,
            Blue, Green, Green, Green, Green, Red, Green, Green, Blue, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Green, Green,
        ]),
        new(nameof(BetaPowerBomb),
        [
            Blue, Blue, Red, Red, Red, Green, Green, Blue, Green, Green,
            Blue, Green, Green, Green, Blue, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(AlphaPowerBomb),
        [
            Red, Blue, Blue, Red, Blue, Green, Green, Blue, Green, Green,
            Green, Blue, Green, Green, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Red, Red, Red, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(BelowSpazer),
        [
            Red, Red, Blue, Blue, Red, Green, Blue, Green, Green, Red,
            Red, Blue, Red, Green, Blue, Green, Blue, Green, Green, Blue,
            Green, Green, Blue, Blue, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Red,
        ]),
        new(nameof(Spazer),
        [
            Blue, Green, Blue, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Blue, Red, Red, Blue,
        ]),
        new(nameof(WarehouseZeela),
        [
            Blue, Red, Red, Blue, Red, Green, Green, Blue, Green, Red,
            Green, Red, Green, Green, Blue, Green, Green, Blue, Green, Green,
            Blue, Green, Green, Blue, Blue, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Red, Red, Red,
        ]),
        new(nameof(WarehouseKihunter),
        [
            Green, Blue, Blue, Red, Red, Blue, Red, Red, Blue, Blue,
            Green, Red, Green, Blue, Green, Green, Blue, Green, Green, Red,
            Green, Green, Blue, Green, Red, Green, Green, Blue, Green, Green,
            Green, Blue, Green, Green, Red, Green, Green, Green, Green, Blue,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(KraidEyeDoor),
        [
            Red, Red, Blue, Blue, Red, Green, Green, Green, Blue, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Blue, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Blue, Blue, Green,
            Green, Red, Red, Red, Red, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Kraid),
        [
            Green, Green, Blue, Blue, Green, Red, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Blue, Green,
        ]),
        new(nameof(Statues),
        [
            Blue, Red, Red, Green, Blue, Blue, Green, Blue, Green, Green,
            Green, Blue, Green, Green, Green, Green, Green, Green, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Red, Red, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(WarehouseEntrance),
        [
            Blue, Red, Green, Red, Blue, Blue, Red, Green, Green, Blue,
            Green, Green, Red, Blue, Green, Red, Red, Green, Green, Blue,
            Green, Green, Blue, Blue, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Red, Red, Red, Red, Red,
        ]),
        new(nameof(IceBeamGate),
        [
            Red, Red, Red, Green, Red, Red, Red, Red, Red, Red,
            Red, Red, Red, Red, Red, Red, Red, Blue, Blue, Blue,
            Blue, Blue, Blue, Blue, Red, Red, Red, Red, Green, Green,
            Green, Blue, Green, Green, Green, Green, Blue, Green, Blue, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(IceBeamSnake),
        [
            Green, Red, Green, Red, Blue, Red, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Blue, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Red, Red, Red,
        ]),
        new(nameof(CrocomireSpeedway),
        [
            Blue, Blue, Blue, Green, Green, Red, Red, Red, Red, Red,
            Red, Red, Green, Red, Red, Red, Green, Green, Green, Green,
            Red, Red, Red, Red, Red, Green, Red, Red, Red, Red,
            Red, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Crocomire),
        [
            Red, Red, Blue, Blue, Blue, Blue, Blue, Blue, Blue, Blue,
            Green, Red, Blue, Green, Green, Green, Green, Green, Blue, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Red, Red, Green, Green, Green,
        ]),
        new(nameof(CrocomireEscape),
        [
            Green, Green, Green, Green, Blue, Blue, Blue, Blue, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Green, Red, Green, Green,
            Green, Green, Green, Green, Red, Green, Green, Red, Red, Red,
            Red, Green, Green, Red, Red, Green, Green, Green, Green, Green,
        ]),
        new(nameof(HiJumpETank),
        [
            Red, Blue, Red, Red, Red, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Green,
        ]),
        new(nameof(PostCrocFarming),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Blue, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Red, Red, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Blue,
        ]),
        new(nameof(PostCrocShaft),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Blue, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Red, Red, Red, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(PostCrocJump),
        [
            Green, Green, Green, Green, Green, Red, Red, Red, Green, Green,
            Green, Green, Green, Red, Red, Red, Blue, Blue, Blue, Blue,
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Blue, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(GrappleTutorial2),
        [
            Green, Green, Blue, Green, Green, Green, Green, Green, Blue, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Red, Red, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(GrappleBeam),
        [
            Green, Green, Blue, Red, Green, Green, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(GreenBubblesMissiles),
        [
            Red, Blue, Red, Blue, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(BubbleMountain),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Blue, Green, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Red, Red, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(SingleChamber),
        [
            Green, Red, Blue, Blue, Blue, Blue, Green, Red, Red, Red,
            Red, Red, Green, Red, Red, Red, Red, Red, Blue, Red,
            Red, Red, Red, Red, Blue, Red, Green, Blue, Blue, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
        ]),
        new(nameof(DoubleChamber),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Blue, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Volcano),
        [
            Red, Red, Green, Red, Red, Green, Red, Red, Blue, Green,
            Blue, Green, Blue, Green, Green, Blue, Green, Green, Red, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Green, Red, Red,
        ]),
        new(nameof(KronicBoost),
        [
            Red, Green, Red, Green, Red, Blue, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Blue, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Red, Red, Red, Red,
        ]),
        new(nameof(PurpleShaft),
        [
            Green, Green, Blue, Red, Green, Blue, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Red, Red, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(LowerNorfairElev),
        [
            Blue, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(UpperNorfairFarming),
        [
            Green, Green, Blue, Blue, Green, Green, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(BatCave),
        [
            Red, Blue, Red, Green, Blue, Green, Green, Red, Green, Blue,
            Blue, Green, Red, Red, Green, Green, Green, Green, Green, Blue,
            Blue, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
            Red, Red, Red, Red, Red, Red, Red, Red, Green, Green,
        ]),
        new(nameof(FrogSpeedway),
        [
            Blue, Blue, Blue, Blue, Blue, Blue, Blue, Blue, Green, Green,
            Green, Green, Blue, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Green, Green, Green,
            Green, Green, Green, Blue, Green, Green, Green, Red, Red, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(RedPirateShaft),
        [
            Green, Green, Green, Green, Green, Green, Green, Blue, Blue, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Red, Red, Red, Red, Red, Red, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(LNElevSave),
        [
            Blue, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Red, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Blue, Blue, Red,
        ]),
        new(nameof(AcidStatue),
        [
            Green, Green, Red, Blue, Blue, Red, Red, Red, Red, Green,
            Blue, Green, Blue, Green, Red, Green, Red, Green, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Blue, Green, Blue, Green, Blue, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Green, Green, Green,
        ]),
        new(nameof(MainHall),
        [
            Red, Red, Red, Red, Green, Red, Red, Red, Red, Red,
            Red, Red, Red, Red, Red, Red, Blue, Blue, Blue, Blue,
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(GoldenTorizo),
        [
            Green, Green, Blue, Blue, Red, Green, Blue, Green, Green, Blue,
            Green, Blue, Green, Green, Green, Green, Green, Green, Blue, Green,
            Green, Red, Blue, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Red, Red, Red, Red, Green, Green, Green, Green,
        ]),
        new(nameof(Ridley),
        [
            Green, Green, Green, Green, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Red, Red, Red, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Blue,
        ]),
        new(nameof(FastPillarsSetup),
        [
            Green, Blue, Red, Blue, Green, Green, Blue, Green, Green, Blue,
            Green, Green, Green, Green, Green, Blue, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Red, Red, Red, Red, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(MickeyMouse),
        [
            Red, Red, Red, Green, Red, Red, Red, Green, Red, Red,
            Red, Green, Blue, Blue, Blue, Blue, Green, Blue, Green, Green,
            Red, Green, Green, Blue, Green, Green, Red, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(WorstRoomInTheGame),
        [
            Red, Green, Green, Green, Green, Blue, Red, Green, Blue, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Red, Red, Red, Red, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(LNSpringBallMaze),
        [
            Green, Red, Red, Red, Red, Blue, Blue, Red, Red, Red,
            Blue, Blue, Green, Blue, Green, Red, Green, Blue, Red, Green,
            Red, Green, Blue, Green, Green, Blue, Green, Green, Green, Green,
            Green, Green, Green, Blue, Blue, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
        ]),
        new(nameof(RedKihunterShaft),
        [
            Green, Red, Red, Green, Red, Red, Green, Red, Red, Green,
            Red, Red, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Red, Green, Red, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Wasteland),
        [
            Red, Blue, Blue, Blue, Red, Blue, Red, Red, Red, Red,
            Red, Red, Red, Green, Red, Red, Red, Red, Green, Green,
            Green, Blue, Blue, Green, Red, Green, Green, Blue, Green, Red,
            Blue, Blue, Blue, Green, Green, Red, Green, Red, Red, Green,
            Blue, Red, Green, Green, Green, Green, Green, Green, Blue, Green,
        ]),
        new(nameof(ThreeMusketeers),
        [
            Red, Green, Red, Red, Red, Green, Red, Red, Red, Blue,
            Blue, Blue, Green, Red, Green, Green, Blue, Green, Blue, Green,
            Green, Red, Green, Green, Green, Green, Green, Blue, Blue, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Green, Blue, Blue,
        ]),
        new(nameof(LNFireflea),
        [
            Red, Green, Red, Red, Green, Red, Red, Green, Red, Red,
            Blue, Red, Red, Red, Red, Red, Blue, Blue, Green, Blue,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Blue,
            Green, Red, Green, Blue, Green, Green, Green, Green, Green, Green,
            Green, Green, Blue, Blue, Green, Green, Red, Green, Green, Green,
        ]),
        new(nameof(BowlingAlley),
        [
            Red, Red, Blue, Blue, Blue, Green, Blue, Blue, Blue, Blue,
            Blue, Green, Red, Blue, Red, Red, Red, Green, Green, Blue,
            Green, Blue, Green, Green, Red, Green, Green, Green, Green, Green,
            Blue, Green, Blue, Green, Green, Blue, Green, Green, Blue, Green,
            Green, Red, Green, Green, Red, Green, Blue, Green, Green, Green,
        ]),
        new(nameof(Attic),
        [
            Green, Green, Green, Green, Green, Green, Green, Red, Blue, Green,
            Green, Blue, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Blue, Green, Green, Red, Green, Green, Green, Green, Blue,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
        ]),
        new(nameof(WreckedShipMainShaft),
        [
            Red, Red, Red, Red, Green, Red, Red, Red, Red, Red,
            Green, Red, Red, Red, Red, Red, Green, Red, Red, Red,
            Red, Red, Green, Red, Red, Red, Red, Red, Green, Red,
            Blue, Blue, Blue, Red, Green, Red, Red, Red, Red, Red,
            Blue, Red, Red, Red, Red, Red, Red, Red, Green, Blue,
        ]),
        new(nameof(ElectricDeath),
        [
            Red, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Green, Green, Green, Green, Blue,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Red,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
        ]),
        new(nameof(Basement),
        [
            Blue, Blue, Blue, Blue, Red, Green, Blue, Green, Blue, Green,
            Green, Red, Green, Green, Red, Green, Green, Green, Green, Green,
            Blue, Blue, Green, Green, Red, Green, Green, Green, Green, Blue,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
        ]),
        new(nameof(Phantoon),
        [
            Blue, Green, Green, Green, Green, Green, Blue, Green, Green, Red,
            Green, Green, Green, Green, Blue, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Red, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
        ]),
        new(nameof(SpongeBath),
        [
            Blue, Blue, Green, Green, Green, Green, Blue, Blue, Green, Green,
            Red, Green, Green, Green, Green, Blue, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(WSWestSuper),
        [
            Blue, Green, Green, Green, Green, Green, Blue, Green, Green, Red,
            Green, Green, Green, Green, Blue, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Red, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
        ]),
        new(nameof(WSEastSuper),
        [
            Blue, Blue, Blue, Red, Green, Blue, Green, Green, Green, Green,
            Green, Blue, Blue, Green, Green, Red, Green, Green, Green, Green,
            Blue, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Blue, Blue, Red,
            Red, Red, Red, Red, Red, Green, Green, Green, Green, Green,
        ]),
        new(nameof(GlassTunnel),
        [
            Green, Green, Green, Red, Green, Green, Blue, Green, Green, Green,
            Green, Green, Green, Green, Green, Blue, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
        ]),
        new(nameof(WestTunnel),
        [
            Blue, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Blue, Blue, Blue, Blue,
        ]),
        new(nameof(EastTunnel),
        [
            Blue, Blue, Blue, Blue, Red, Red, Red, Red, Red, Blue,
            Blue, Red, Green, Red, Green, Red, Green, Green, Blue, Green,
            Red, Red, Green, Blue, Green, Blue, Blue, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(MainStreet),
        [
            Green, Green, Red, Green, Green, Red, Green, Green, Blue, Green,
            Green, Red, Green, Green, Red, Green, Green, Red, Green, Green,
            Green, Green, Green, Red, Green, Blue, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
        ]),
        new(nameof(FishTank),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Red, Red, Red, Red, Green, Green,
        ]),
        new(nameof(MamaTurtle),
        [
            Red, Green, Green, Red, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Green, Green, Green, Green, Green,
        ]),
        new(nameof(CrabTunnel),
        [
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(MtEverest),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Red, Red, Green,
            Green, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
        ]),
        new(nameof(RedFish),
        [
            Blue, Red, Green, Red, Red, Green, Green, Green, Green, Blue,
            Blue, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Red, Green,
            Green, Red, Red, Red, Red, Green, Green, Red, Red, Green,
        ]),
        new(nameof(WateringHole),
        [
            Green, Blue, Green, Red, Red, Red, Green, Blue, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Red, Green, Green, Red, Red,
            Red, Red, Green, Green, Red, Red, Green, Green, Green, Green,
        ]),
        new(nameof(NWestMaridiaBug),
        [
            Green, Red, Red, Red, Blue, Red, Blue, Blue, Green, Blue,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Green, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
        ]),
        new(nameof(CrabShaft),
        [
            Green, Red, Green, Red, Blue, Red, Red, Blue, Green, Green,
            Green, Blue, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Red, Red, Red, Red, Green, Green, Red, Red,
        ]),
        new(nameof(PseudoPlasmaSpark),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Blue, Green, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Green, Green, Red, Red, Red,
        ]),
        new(nameof(CrabHole),
        [
            Blue, Red, Red, Green, Blue, Blue, Green, Green, Green, Green,
            Green, Blue, Blue, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Red, Green, Green, Red, Red, Red, Red,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(WestSandHallTunnel),
        [
            Blue, Green, Green, Green, Red, Blue, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Blue, Green, Green, Green, Red, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Blue, Green, Green, Green, Red, Green,
        ]),
        new(nameof(PlasmaTutorial),
        [
            Blue, Green, Green, Green, Red, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Blue, Blue, Green, Green,
        ]),
        new(nameof(Plasma),
        [
            Green, Green, Green, Green, Blue, Blue, Green, Green, Green, Green,
            Green, Blue, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Red, Red, Red, Red, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Blue,
        ]),
        new(nameof(ThreadTheNeedle),
        [
            Blue, Blue, Blue, Blue, Blue, Blue, Blue, Green, Green, Green,
            Red, Blue, Green, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Red, Red, Red, Red,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(MaridiaElev),
        [
            Green, Green, Green, Green, Green, Blue, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Red, Red, Red, Red, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(PlasmaSpark),
        [
            Red, Green, Green, Red, Red, Green, Green, Red, Green, Green,
            Green, Green, Red, Green, Green, Green, Red, Green, Green, Green,
            Red, Blue, Blue, Blue, Green, Green, Green, Red, Blue, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Red,
        ]),
        new(nameof(Kassiuz),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Blue, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Red,
            Red, Red, Red, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Blue,
        ]),
        new(nameof(BugSandHole),
        [
            Green, Green, Green, Green, Green, Green, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Blue, Red, Red,
            Red, Red, Red, Red, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Oasis),
        [
            Red, Blue, Red, Green, Blue, Green, Green, Green, Green, Green,
            Green, Green, Blue, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Blue, Red, Red, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(WestSandHole),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(EastSandHole),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Blue, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(WestAqueductQuicksand),
        [
            Green, Green, Green, Green, Green, Green, Blue, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Red, Red, Green,
            Green, Red, Red, Red, Red, Green, Green, Red, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(EastAqueductQuicksand),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Green,
            Green, Red, Red, Red, Red, Green, Green, Red, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Aqueduct),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Blue, Blue, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Red, Red, Red, Red,
        ]),
        new(nameof(BotwoonHallway),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Pants),
        [
            Green, Green, Green, Green, Green, Blue, Blue, Red, Green, Green,
            Green, Red, Green, Green, Green, Red, Green, Blue, Green, Red,
            Green, Green, Red, Green, Green, Green, Red, Green, Blue, Green,
            Red, Green, Green, Green, Green, Blue, Green, Green, Green, Green,
            Green, Blue, Green, Green, Green, Red, Green, Green, Green, Green,
        ]),
        new(nameof(EastPants),
        [
            Green, Red, Blue, Blue, Green, Green, Blue, Green, Green, Blue,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
        ]),
        new(nameof(Springball),
        [
            Green, Red, Blue, Blue, Green, Green, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Blue, Blue, Blue, Blue, Green,
        ]),
        new(nameof(BelowBotwoonETank),
        [
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Colosseum),
        [
            Green, Green, Green, Green, Green, Green, Green, Blue, Blue, Blue,
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Blue, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Red, Red, Red, Red, Green, Green, Red,
        ]),
        new(nameof(AqueductSave),
        [
            Blue, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Blue, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Green, Green, Green, Green, Green,
            Green, Red, Green, Green, Green, Red, Red, Red, Red, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(ThePrecious),
        [
            Blue, Blue, Red, Red, Blue, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
        ]),
        new(nameof(BotwoonETank),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Blue, Blue, Green, Green, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Red, Red, Red, Red, Red, Red, Red,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(PlasmaBeachQuicksand),
        [
            Green, Green, Green, Green, Green, Green, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(BotwoonQuicksand),
        [
            Green, Green, Green, Green, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Red, Green, Green, Red, Red, Red, Red,
            Green, Green, Red, Red, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Shaktool),
        [
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Green, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Red, Red, Red, Red, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
        ]),
        new(nameof(HalfieClimb),
        [
            Green, Red, Red, Red, Red, Green, Red, Red, Red, Red,
            Blue, Red, Blue, Blue, Blue, Green, Red, Green, Blue, Green,
            Green, Blue, Green, Red, Green, Green, Blue, Green, Green, Green,
            Green, Green, Green, Blue, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Botwoon),
        [
            Blue, Red, Green, Green, Green, Green, Blue, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Green,
            Green, Red, Red, Red, Red, Green, Green, Red, Red, Green,
            Green, Green, Green, Blue, Green, Green, Green, Green, Blue, Blue,
        ]),
        new(nameof(SpaceJump),
        [
            Blue, Green, Green, Green, Green, Blue, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Red, Red,
            Green, Green, Blue, Green, Green, Green, Green, Blue, Green, Green,
        ]),
        new(nameof(MaridiaEnergyRefill),
        [
            Blue, Green, Green, Green, Green, Blue, Green, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Red, Green, Green, Green,
            Red, Red, Red, Red, Green, Green, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Blue, Green, Green, Green, Green,
        ]),
        new(nameof(WestCactusAlley),
        [
            Green, Blue, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Red, Red, Green,
            Green, Red, Red, Red, Red, Green, Green, Red, Red, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(EastCactusAlley),
        [
            Green, Green, Green, Green, Green, Blue, Blue, Blue, Blue, Blue,
            Green, Green, Green, Green, Green, Green, Green, Green, Blue, Green,
            Green, Green, Green, Blue, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Blue, Blue, Green, Green, Red, Red, Red, Red, Green, Green,
        ]),
        new(nameof(Draygon),
        [
            Green, Green, Blue, Blue, Red, Green, Green, Green, Blue, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Blue,
            Green, Green, Green, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(TourianFirst),
        [
            Green, Green, Green, Blue, Blue, Green, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Metroids1),
        [
            Blue, Blue, Blue, Blue, Blue, Blue, Green, Green, Green, Green,
            Blue, Green, Green, Green, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Red, Red, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Metroids2),
        [
            Green, Green, Green, Green, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Metroids3),
        [
            Blue, Blue, Blue, Blue, Blue, Blue, Green, Green, Green, Green,
            Blue, Green, Green, Green, Red, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Red, Red, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Metroids4),
        [
            Green, Green, Green, Green, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(BlueHopper),
        [
            Blue, Blue, Green, Green, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(DustTorizo),
        [
            Blue, Blue, Green, Green, Green, Green, Green, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Red, Red, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Red, Red, Red, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(BigBoy),
        [
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Blue, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(Seaweed),
        [
            Green, Green, Green, Green, Green, Green, Blue, Blue, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Red, Red, Green,
            Green, Red, Red, Red, Red, Green, Green, Red, Red, Green,
            Green, Green, Green, Blue, Green, Green, Green, Green, Green, Blue,
        ]),
        new(nameof(TourianRecharge),
        [
            Blue, Green, Green, Green, Green, Green, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Red, Red, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Red, Red, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(MotherBrain),
        [
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Green, Blue,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Blue, Blue, Blue,
        ]),
        new(nameof(TourianEyeDoor),
        [
            Blue, Blue, Blue, Blue, Green, Green, Green, Green, Blue, Green,
            Green, Green, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(RinkaShaft),
        [
            Green, Green, Green, Green, Green, Green, Green, Blue, Blue, Green,
            Green, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Red, Red,
            Green, Green, Red, Red, Red, Red, Green, Green, Red, Red,
            Green, Green, Green, Green, Blue, Green, Green, Green, Green, Green,
        ]),
        new(nameof(MotherBrainSave),
        [
            Blue, Green, Green, Green, Green, Green, Blue, Green, Green, Red,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Red, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(TourianEscape1),
        [
            Green, Green, Green, Green, Green, Green, Blue, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Blue, Green, Green, Green,
        ]),
        new(nameof(TourianEscape2),
        [
            Green, Blue, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(TourianEscape3),
        [
            Green, Green, Green, Green, Green, Green, Blue, Blue, Blue, Blue,
            Blue, Blue, Green, Green, Green, Green, Green, Green, Green, Green,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Red,
            Red, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Green, Green, Green, Green, Green, Green, Green,
        ]),
        new(nameof(TourianEscape4),
        [
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Blue, Blue, Blue, Green, Green,
            Green, Green, Blue, Blue, Green, Green, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Green, Green, Red, Red, Green, Green, Red, Red, Red,
        ]),
        new(nameof(UpperTourianSave),
        [
            Blue, Red, Green, Green, Green, Blue, Green, Green, Green, Green,
            Green, Green, Green, Green, Blue, Green, Green, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Green,
            Green, Red, Red, Red, Red, Red, Red, Green, Green, Green,
            Green, Green, Green, Green, Green, Green, Green, Green, Green, Red,
        ]),
    ];

    /// <summary>Every explicit table by its name.</summary>
    private static readonly Dictionary<string, RoomScrollDefinition> ByName = Definitions.ToDictionary(scroll => scroll.Name);

    /// <summary>The explicit table named <paramref name="name"/>.</summary>
    private static RoomScrollDefinition Find(string name) => ByName[name];

    /// <summary>The implicit layout whose bottom row is blue and every other row green.</summary>
    public static RoomScrollDefinition GreenWithBlueBottomRow { get; } = RoomScrollDefinition.Implicit(nameof(GreenWithBlueBottomRow), RoomScrollState.Blue);

    /// <summary>The implicit layout whose every row is green.</summary>
    public static RoomScrollDefinition AllGreen { get; } = RoomScrollDefinition.Implicit(nameof(AllGreen), RoomScrollState.Green);

    /// <summary>The Landing Site room's screen scroll layout.</summary>
    public static RoomScrollDefinition LandingSite { get; } = Find(nameof(LandingSite));
    /// <summary>The Parlor room's screen scroll layout.</summary>
    public static RoomScrollDefinition Parlor { get; } = Find(nameof(Parlor));
    /// <summary>The Crateria Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaPowerBomb { get; } = Find(nameof(CrateriaPowerBomb));
    /// <summary>The West Ocean room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestOcean { get; } = Find(nameof(WestOcean));
    /// <summary>The Crateria Kihunter room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaKihunter { get; } = Find(nameof(CrateriaKihunter));
    /// <summary>The Forgotten Highway Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition ForgottenHighwayElev { get; } = Find(nameof(ForgottenHighwayElev));
    /// <summary>The East Ocean room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastOcean { get; } = Find(nameof(EastOcean));
    /// <summary>The Forgotten Highway Elbow room's screen scroll layout.</summary>
    public static RoomScrollDefinition ForgottenHighwayElbow { get; } = Find(nameof(ForgottenHighwayElbow));
    /// <summary>The Red Brinstar Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedBinstarElev { get; } = Find(nameof(RedBinstarElev));
    /// <summary>The Gauntlet Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition GauntletETank { get; } = Find(nameof(GauntletETank));
    /// <summary>The Climb room's screen scroll layout.</summary>
    public static RoomScrollDefinition Climb { get; } = Find(nameof(Climb));
    /// <summary>The Pit room's screen scroll layout.</summary>
    public static RoomScrollDefinition Pit { get; } = Find(nameof(Pit));
    /// <summary>The Elevator to Blue Brinstar room's screen scroll layout.</summary>
    public static RoomScrollDefinition ElevToBlueBrinstar { get; } = Find(nameof(ElevToBlueBrinstar));
    /// <summary>The Green Brinstar Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarElev { get; } = Find(nameof(GreenBrinstarElev));
    /// <summary>The Green Pirates Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenPiratesShaft { get; } = Find(nameof(GreenPiratesShaft));
    /// <summary>The Crateria Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrateriaSuper { get; } = Find(nameof(CrateriaSuper));
    /// <summary>The Final Missile Bombway room's screen scroll layout.</summary>
    public static RoomScrollDefinition FinalMissileBombway { get; } = Find(nameof(FinalMissileBombway));
    /// <summary>The Final Missile room's screen scroll layout.</summary>
    public static RoomScrollDefinition FinalMissile { get; } = Find(nameof(FinalMissile));
    /// <summary>The Green Brinstar Main Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarMainShaft { get; } = Find(nameof(GreenBrinstarMainShaft));
    /// <summary>The Spore Spawn Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition SporeSpawnSuper { get; } = Find(nameof(SporeSpawnSuper));
    /// <summary>The Early Supers room's screen scroll layout.</summary>
    public static RoomScrollDefinition EarlySupers { get; } = Find(nameof(EarlySupers));
    /// <summary>The Brinstar Reserve Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BrinstarReserveTank { get; } = Find(nameof(BrinstarReserveTank));
    /// <summary>The Green Brinstar Missile Refill room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarMissileRefill { get; } = Find(nameof(GreenBrinstarMissileRefill));
    /// <summary>The Dachora room's screen scroll layout.</summary>
    public static RoomScrollDefinition Dachora { get; } = Find(nameof(Dachora));
    /// <summary>The Big Pink room's screen scroll layout.</summary>
    public static RoomScrollDefinition BigPink { get; } = Find(nameof(BigPink));
    /// <summary>The Pink Brinstar Power Bombs room's screen scroll layout.</summary>
    public static RoomScrollDefinition PinkBrinstarPowerBombs { get; } = Find(nameof(PinkBrinstarPowerBombs));
    /// <summary>The Green Hill Zone room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenHillZone { get; } = Find(nameof(GreenHillZone));
    /// <summary>The Morph Ball room's screen scroll layout.</summary>
    public static RoomScrollDefinition MorphBall { get; } = Find(nameof(MorphBall));
    /// <summary>The Construction Zone room's screen scroll layout.</summary>
    public static RoomScrollDefinition ConstructionZone { get; } = Find(nameof(ConstructionZone));
    /// <summary>The Blue Brinstar Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BlueBrinstarETank { get; } = Find(nameof(BlueBrinstarETank));
    /// <summary>The Green Brinstar Beetoms room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBrinstarBeetoms { get; } = Find(nameof(GreenBrinstarBeetoms));
    /// <summary>The Etecoon Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition EtecoonETank { get; } = Find(nameof(EtecoonETank));
    /// <summary>The Etecoon Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition EtecoonSuper { get; } = Find(nameof(EtecoonSuper));
    /// <summary>The Spore Spawn Farming room's screen scroll layout.</summary>
    public static RoomScrollDefinition SporeSpawnFarming { get; } = Find(nameof(SporeSpawnFarming));
    /// <summary>The Waterway Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition WaterwayETank { get; } = Find(nameof(WaterwayETank));
    /// <summary>The Red Tower room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedTower { get; } = Find(nameof(RedTower));
    /// <summary>The Red Brinstar Firefleas room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedBrinstarFirefleas { get; } = Find(nameof(RedBrinstarFirefleas));
    /// <summary>The Caterpillar room's screen scroll layout.</summary>
    public static RoomScrollDefinition Caterpillar { get; } = Find(nameof(Caterpillar));
    /// <summary>The Beta Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition BetaPowerBomb { get; } = Find(nameof(BetaPowerBomb));
    /// <summary>The Alpha Power Bomb room's screen scroll layout.</summary>
    public static RoomScrollDefinition AlphaPowerBomb { get; } = Find(nameof(AlphaPowerBomb));
    /// <summary>The Below Spazer room's screen scroll layout.</summary>
    public static RoomScrollDefinition BelowSpazer { get; } = Find(nameof(BelowSpazer));
    /// <summary>The Spazer room's screen scroll layout.</summary>
    public static RoomScrollDefinition Spazer { get; } = Find(nameof(Spazer));
    /// <summary>The Warehouse Zeela room's screen scroll layout.</summary>
    public static RoomScrollDefinition WarehouseZeela { get; } = Find(nameof(WarehouseZeela));
    /// <summary>The Warehouse Kihunter room's screen scroll layout.</summary>
    public static RoomScrollDefinition WarehouseKihunter { get; } = Find(nameof(WarehouseKihunter));
    /// <summary>The Kraid Eye Door room's screen scroll layout.</summary>
    public static RoomScrollDefinition KraidEyeDoor { get; } = Find(nameof(KraidEyeDoor));
    /// <summary>The Kraid room's screen scroll layout.</summary>
    public static RoomScrollDefinition Kraid { get; } = Find(nameof(Kraid));
    /// <summary>The Statues room's screen scroll layout.</summary>
    public static RoomScrollDefinition Statues { get; } = Find(nameof(Statues));
    /// <summary>The Warehouse Entrance room's screen scroll layout.</summary>
    public static RoomScrollDefinition WarehouseEntrance { get; } = Find(nameof(WarehouseEntrance));
    /// <summary>The Ice Beam Gate room's screen scroll layout.</summary>
    public static RoomScrollDefinition IceBeamGate { get; } = Find(nameof(IceBeamGate));
    /// <summary>The Ice Beam Snake room's screen scroll layout.</summary>
    public static RoomScrollDefinition IceBeamSnake { get; } = Find(nameof(IceBeamSnake));
    /// <summary>The Crocomire Speedway room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrocomireSpeedway { get; } = Find(nameof(CrocomireSpeedway));
    /// <summary>The Crocomire room's screen scroll layout.</summary>
    public static RoomScrollDefinition Crocomire { get; } = Find(nameof(Crocomire));
    /// <summary>The Crocomire Escape room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrocomireEscape { get; } = Find(nameof(CrocomireEscape));
    /// <summary>The Hi-Jump Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition HiJumpETank { get; } = Find(nameof(HiJumpETank));
    /// <summary>The Post-Crocomire Farming room's screen scroll layout.</summary>
    public static RoomScrollDefinition PostCrocFarming { get; } = Find(nameof(PostCrocFarming));
    /// <summary>The Post-Crocomire Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition PostCrocShaft { get; } = Find(nameof(PostCrocShaft));
    /// <summary>The Post-Crocomire Jump room's screen scroll layout.</summary>
    public static RoomScrollDefinition PostCrocJump { get; } = Find(nameof(PostCrocJump));
    /// <summary>The Grapple Tutorial 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition GrappleTutorial2 { get; } = Find(nameof(GrappleTutorial2));
    /// <summary>The Grapple Beam room's screen scroll layout.</summary>
    public static RoomScrollDefinition GrappleBeam { get; } = Find(nameof(GrappleBeam));
    /// <summary>The Green Bubbles Missiles room's screen scroll layout.</summary>
    public static RoomScrollDefinition GreenBubblesMissiles { get; } = Find(nameof(GreenBubblesMissiles));
    /// <summary>The Bubble Mountain room's screen scroll layout.</summary>
    public static RoomScrollDefinition BubbleMountain { get; } = Find(nameof(BubbleMountain));
    /// <summary>The Single Chamber room's screen scroll layout.</summary>
    public static RoomScrollDefinition SingleChamber { get; } = Find(nameof(SingleChamber));
    /// <summary>The Double Chamber room's screen scroll layout.</summary>
    public static RoomScrollDefinition DoubleChamber { get; } = Find(nameof(DoubleChamber));
    /// <summary>The Volcano room's screen scroll layout.</summary>
    public static RoomScrollDefinition Volcano { get; } = Find(nameof(Volcano));
    /// <summary>The Kronic Boost room's screen scroll layout.</summary>
    public static RoomScrollDefinition KronicBoost { get; } = Find(nameof(KronicBoost));
    /// <summary>The Purple Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition PurpleShaft { get; } = Find(nameof(PurpleShaft));
    /// <summary>The Lower Norfair Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition LowerNorfairElev { get; } = Find(nameof(LowerNorfairElev));
    /// <summary>The Upper Norfair Farming room's screen scroll layout.</summary>
    public static RoomScrollDefinition UpperNorfairFarming { get; } = Find(nameof(UpperNorfairFarming));
    /// <summary>The Bat Cave room's screen scroll layout.</summary>
    public static RoomScrollDefinition BatCave { get; } = Find(nameof(BatCave));
    /// <summary>The Frog Speedway room's screen scroll layout.</summary>
    public static RoomScrollDefinition FrogSpeedway { get; } = Find(nameof(FrogSpeedway));
    /// <summary>The Red Pirate Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedPirateShaft { get; } = Find(nameof(RedPirateShaft));
    /// <summary>The Lower Norfair Elevator Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition LNElevSave { get; } = Find(nameof(LNElevSave));
    /// <summary>The Acid Statue room's screen scroll layout.</summary>
    public static RoomScrollDefinition AcidStatue { get; } = Find(nameof(AcidStatue));
    /// <summary>The Main Hall room's screen scroll layout.</summary>
    public static RoomScrollDefinition MainHall { get; } = Find(nameof(MainHall));
    /// <summary>The Golden Torizo room's screen scroll layout.</summary>
    public static RoomScrollDefinition GoldenTorizo { get; } = Find(nameof(GoldenTorizo));
    /// <summary>The Ridley room's screen scroll layout.</summary>
    public static RoomScrollDefinition Ridley { get; } = Find(nameof(Ridley));
    /// <summary>The Fast Pillars Setup room's screen scroll layout.</summary>
    public static RoomScrollDefinition FastPillarsSetup { get; } = Find(nameof(FastPillarsSetup));
    /// <summary>The Mickey Mouse room's screen scroll layout.</summary>
    public static RoomScrollDefinition MickeyMouse { get; } = Find(nameof(MickeyMouse));
    /// <summary>The Worst Room in the Game's screen scroll layout.</summary>
    public static RoomScrollDefinition WorstRoomInTheGame { get; } = Find(nameof(WorstRoomInTheGame));
    /// <summary>The Lower Norfair Spring Ball Maze room's screen scroll layout.</summary>
    public static RoomScrollDefinition LNSpringBallMaze { get; } = Find(nameof(LNSpringBallMaze));
    /// <summary>The Red Kihunter Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedKihunterShaft { get; } = Find(nameof(RedKihunterShaft));
    /// <summary>The Wasteland room's screen scroll layout.</summary>
    public static RoomScrollDefinition Wasteland { get; } = Find(nameof(Wasteland));
    /// <summary>The Three Musketeers room's screen scroll layout.</summary>
    public static RoomScrollDefinition ThreeMusketeers { get; } = Find(nameof(ThreeMusketeers));
    /// <summary>The Lower Norfair Fireflea room's screen scroll layout.</summary>
    public static RoomScrollDefinition LNFireflea { get; } = Find(nameof(LNFireflea));
    /// <summary>The Bowling Alley room's screen scroll layout.</summary>
    public static RoomScrollDefinition BowlingAlley { get; } = Find(nameof(BowlingAlley));
    /// <summary>The Attic room's screen scroll layout.</summary>
    public static RoomScrollDefinition Attic { get; } = Find(nameof(Attic));
    /// <summary>The Wrecked Ship Main Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition WreckedShipMainShaft { get; } = Find(nameof(WreckedShipMainShaft));
    /// <summary>The Electric Death room's screen scroll layout.</summary>
    public static RoomScrollDefinition ElectricDeath { get; } = Find(nameof(ElectricDeath));
    /// <summary>The Basement room's screen scroll layout.</summary>
    public static RoomScrollDefinition Basement { get; } = Find(nameof(Basement));
    /// <summary>The Phantoon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Phantoon { get; } = Find(nameof(Phantoon));
    /// <summary>The Sponge Bath room's screen scroll layout.</summary>
    public static RoomScrollDefinition SpongeBath { get; } = Find(nameof(SpongeBath));
    /// <summary>The Wrecked Ship West Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition WSWestSuper { get; } = Find(nameof(WSWestSuper));
    /// <summary>The Wrecked Ship East Super room's screen scroll layout.</summary>
    public static RoomScrollDefinition WSEastSuper { get; } = Find(nameof(WSEastSuper));
    /// <summary>The Glass Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition GlassTunnel { get; } = Find(nameof(GlassTunnel));
    /// <summary>The West Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestTunnel { get; } = Find(nameof(WestTunnel));
    /// <summary>The East Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastTunnel { get; } = Find(nameof(EastTunnel));
    /// <summary>The Main Street room's screen scroll layout.</summary>
    public static RoomScrollDefinition MainStreet { get; } = Find(nameof(MainStreet));
    /// <summary>The Fish Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition FishTank { get; } = Find(nameof(FishTank));
    /// <summary>The Mama Turtle room's screen scroll layout.</summary>
    public static RoomScrollDefinition MamaTurtle { get; } = Find(nameof(MamaTurtle));
    /// <summary>The Crab Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrabTunnel { get; } = Find(nameof(CrabTunnel));
    /// <summary>The Mt. Everest room's screen scroll layout.</summary>
    public static RoomScrollDefinition MtEverest { get; } = Find(nameof(MtEverest));
    /// <summary>The Red Fish room's screen scroll layout.</summary>
    public static RoomScrollDefinition RedFish { get; } = Find(nameof(RedFish));
    /// <summary>The Watering Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition WateringHole { get; } = Find(nameof(WateringHole));
    /// <summary>The Northwest Maridia Bug room's screen scroll layout.</summary>
    public static RoomScrollDefinition NWestMaridiaBug { get; } = Find(nameof(NWestMaridiaBug));
    /// <summary>The Crab Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrabShaft { get; } = Find(nameof(CrabShaft));
    /// <summary>The Pseudo Plasma Spark room's screen scroll layout.</summary>
    public static RoomScrollDefinition PseudoPlasmaSpark { get; } = Find(nameof(PseudoPlasmaSpark));
    /// <summary>The Crab Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition CrabHole { get; } = Find(nameof(CrabHole));
    /// <summary>The West Sand Hall Tunnel room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestSandHallTunnel { get; } = Find(nameof(WestSandHallTunnel));
    /// <summary>The Plasma Tutorial room's screen scroll layout.</summary>
    public static RoomScrollDefinition PlasmaTutorial { get; } = Find(nameof(PlasmaTutorial));
    /// <summary>The Plasma room's screen scroll layout.</summary>
    public static RoomScrollDefinition Plasma { get; } = Find(nameof(Plasma));
    /// <summary>The Thread the Needle room's screen scroll layout.</summary>
    public static RoomScrollDefinition ThreadTheNeedle { get; } = Find(nameof(ThreadTheNeedle));
    /// <summary>The Maridia Elevator room's screen scroll layout.</summary>
    public static RoomScrollDefinition MaridiaElev { get; } = Find(nameof(MaridiaElev));
    /// <summary>The Plasma Spark room's screen scroll layout.</summary>
    public static RoomScrollDefinition PlasmaSpark { get; } = Find(nameof(PlasmaSpark));
    /// <summary>The Kassiuz room's screen scroll layout.</summary>
    public static RoomScrollDefinition Kassiuz { get; } = Find(nameof(Kassiuz));
    /// <summary>The Bug Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition BugSandHole { get; } = Find(nameof(BugSandHole));
    /// <summary>The Oasis room's screen scroll layout.</summary>
    public static RoomScrollDefinition Oasis { get; } = Find(nameof(Oasis));
    /// <summary>The West Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestSandHole { get; } = Find(nameof(WestSandHole));
    /// <summary>The East Sand Hole room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastSandHole { get; } = Find(nameof(EastSandHole));
    /// <summary>The West Aqueduct Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestAqueductQuicksand { get; } = Find(nameof(WestAqueductQuicksand));
    /// <summary>The East Aqueduct Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastAqueductQuicksand { get; } = Find(nameof(EastAqueductQuicksand));
    /// <summary>The Aqueduct room's screen scroll layout.</summary>
    public static RoomScrollDefinition Aqueduct { get; } = Find(nameof(Aqueduct));
    /// <summary>The Botwoon Hallway room's screen scroll layout.</summary>
    public static RoomScrollDefinition BotwoonHallway { get; } = Find(nameof(BotwoonHallway));
    /// <summary>The Pants room's screen scroll layout.</summary>
    public static RoomScrollDefinition Pants { get; } = Find(nameof(Pants));
    /// <summary>The East Pants room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastPants { get; } = Find(nameof(EastPants));
    /// <summary>The Springball room's screen scroll layout.</summary>
    public static RoomScrollDefinition Springball { get; } = Find(nameof(Springball));
    /// <summary>The Below Botwoon Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BelowBotwoonETank { get; } = Find(nameof(BelowBotwoonETank));
    /// <summary>The Colosseum room's screen scroll layout.</summary>
    public static RoomScrollDefinition Colosseum { get; } = Find(nameof(Colosseum));
    /// <summary>The Aqueduct Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition AqueductSave { get; } = Find(nameof(AqueductSave));
    /// <summary>The Precious room's screen scroll layout.</summary>
    public static RoomScrollDefinition ThePrecious { get; } = Find(nameof(ThePrecious));
    /// <summary>The Botwoon Energy Tank room's screen scroll layout.</summary>
    public static RoomScrollDefinition BotwoonETank { get; } = Find(nameof(BotwoonETank));
    /// <summary>The Plasma Beach Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition PlasmaBeachQuicksand { get; } = Find(nameof(PlasmaBeachQuicksand));
    /// <summary>The Botwoon Quicksand room's screen scroll layout.</summary>
    public static RoomScrollDefinition BotwoonQuicksand { get; } = Find(nameof(BotwoonQuicksand));
    /// <summary>The Shaktool room's screen scroll layout.</summary>
    public static RoomScrollDefinition Shaktool { get; } = Find(nameof(Shaktool));
    /// <summary>The Halfie Climb room's screen scroll layout.</summary>
    public static RoomScrollDefinition HalfieClimb { get; } = Find(nameof(HalfieClimb));
    /// <summary>The Botwoon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Botwoon { get; } = Find(nameof(Botwoon));
    /// <summary>The Space Jump room's screen scroll layout.</summary>
    public static RoomScrollDefinition SpaceJump { get; } = Find(nameof(SpaceJump));
    /// <summary>The Maridia Energy Refill room's screen scroll layout.</summary>
    public static RoomScrollDefinition MaridiaEnergyRefill { get; } = Find(nameof(MaridiaEnergyRefill));
    /// <summary>The West Cactus Alley room's screen scroll layout.</summary>
    public static RoomScrollDefinition WestCactusAlley { get; } = Find(nameof(WestCactusAlley));
    /// <summary>The East Cactus Alley room's screen scroll layout.</summary>
    public static RoomScrollDefinition EastCactusAlley { get; } = Find(nameof(EastCactusAlley));
    /// <summary>The Draygon room's screen scroll layout.</summary>
    public static RoomScrollDefinition Draygon { get; } = Find(nameof(Draygon));
    /// <summary>The Tourian First room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianFirst { get; } = Find(nameof(TourianFirst));
    /// <summary>The Metroids 1 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids1 { get; } = Find(nameof(Metroids1));
    /// <summary>The Metroids 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids2 { get; } = Find(nameof(Metroids2));
    /// <summary>The Metroids 3 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids3 { get; } = Find(nameof(Metroids3));
    /// <summary>The Metroids 4 room's screen scroll layout.</summary>
    public static RoomScrollDefinition Metroids4 { get; } = Find(nameof(Metroids4));
    /// <summary>The Blue Hopper room's screen scroll layout.</summary>
    public static RoomScrollDefinition BlueHopper { get; } = Find(nameof(BlueHopper));
    /// <summary>The Dust Torizo room's screen scroll layout.</summary>
    public static RoomScrollDefinition DustTorizo { get; } = Find(nameof(DustTorizo));
    /// <summary>The Big Boy room's screen scroll layout.</summary>
    public static RoomScrollDefinition BigBoy { get; } = Find(nameof(BigBoy));
    /// <summary>The Seaweed room's screen scroll layout.</summary>
    public static RoomScrollDefinition Seaweed { get; } = Find(nameof(Seaweed));
    /// <summary>The Tourian Recharge room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianRecharge { get; } = Find(nameof(TourianRecharge));
    /// <summary>The Mother Brain room's screen scroll layout.</summary>
    public static RoomScrollDefinition MotherBrain { get; } = Find(nameof(MotherBrain));
    /// <summary>The Tourian Eye Door room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEyeDoor { get; } = Find(nameof(TourianEyeDoor));
    /// <summary>The Rinka Shaft room's screen scroll layout.</summary>
    public static RoomScrollDefinition RinkaShaft { get; } = Find(nameof(RinkaShaft));
    /// <summary>The Mother Brain Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition MotherBrainSave { get; } = Find(nameof(MotherBrainSave));
    /// <summary>The Tourian Escape 1 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape1 { get; } = Find(nameof(TourianEscape1));
    /// <summary>The Tourian Escape 2 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape2 { get; } = Find(nameof(TourianEscape2));
    /// <summary>The Tourian Escape 3 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape3 { get; } = Find(nameof(TourianEscape3));
    /// <summary>The Tourian Escape 4 room's screen scroll layout.</summary>
    public static RoomScrollDefinition TourianEscape4 { get; } = Find(nameof(TourianEscape4));
    /// <summary>The Upper Tourian Save room's screen scroll layout.</summary>
    public static RoomScrollDefinition UpperTourianSave { get; } = Find(nameof(UpperTourianSave));
}
