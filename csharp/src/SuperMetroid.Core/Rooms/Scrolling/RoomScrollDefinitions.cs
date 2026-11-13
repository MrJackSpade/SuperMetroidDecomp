namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>The scroll data retail room states select: 159 explicit tables and two implicit layouts.</summary>
[NumberedIdentities("Each numbered member is the scroll layout of a distinct numbered room (Metroids1-4, TourianEscape1-4), named for that room.")]
[ArchitectureReview("large", "Data only: every room state's fifty scroll cells as the camera reads them; splitting it by area would only scatter one table the room loader reads as a whole.")]
public static class RoomScrollDefinitions
{
    /// <summary>A screen closed to the camera.</summary>
    private const RoomScrollState Closed = RoomScrollState.Closed;

    /// <summary>A screen open to the camera, which aligns to the screen's top.</summary>
    private const RoomScrollState Top = RoomScrollState.OpenAlignedToTop;

    /// <summary>A screen open to the camera.</summary>
    private const RoomScrollState Open = RoomScrollState.Open;

    /// <summary>
    /// Retail explicit tables in their authored order, each the fifty cells its load writes: the room's own screens,
    /// then the cells after them, which past a short table hold the data stored after it read as cells.
    /// </summary>
    private static readonly RoomScrollDefinition[] Definitions =
    [
        new(nameof(LandingSite),
        [
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Top, Closed,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Open, Open, Open, Top, Top, Top, Top,
            Top, Top, Top, Top, Top, Open, Top, Open, Top, Closed,
        ]),
        new(nameof(Parlor),
        [
            Closed, Top, Top, Top, Top, Closed, Closed, Closed, Closed, Closed,
            Closed, Open, Closed, Top, Closed, Closed, Open, Closed, Closed, Closed,
            Closed, Open, Closed, Closed, Closed, Open, Open, Open, Open, Closed,
            Open, Closed, Closed, Open, Closed, Open, Open, Open, Open, Closed,
            Open, Closed, Top, Open, Open, Top, Open, Open, Top, Open,
        ]),
        new(nameof(CrateriaPowerBomb),
        [
            Top, Top, Open, Closed, Open, Open, Top, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Open, Closed, Open, Closed, Open, Open, Open,
        ]),
        new(nameof(WestOcean),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Top, Top, Top, Top, Top, Top, Top, Top, Open, Closed,
        ]),
        new(nameof(CrateriaKihunter),
        [
            Top, Top, Top, Closed, Closed, Closed, Closed, Open, Closed, Top,
            Open, Open, Open, Open, Top, Top, Open, Closed, Open, Open,
            Closed, Open, Open, Top, Top, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
        ]),
        new(nameof(ForgottenHighwayElev),
        [
            Top, Closed, Open, Open, Open, Closed, Open, Closed, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Top, Open, Open, Closed, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Closed,
        ]),
        new(nameof(EastOcean),
        [
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Top, Top, Top, Top, Top,
            Top, Top, Open, Closed, Open, Open, Top, Open, Open, Open,
        ]),
        new(nameof(ForgottenHighwayElbow),
        [
            Open, Open, Closed, Open, Open, Top, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
        ]),
        new(nameof(RedBinstarElev),
        [
            Top, Closed, Open, Open, Open, Closed, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Top, Open, Open,
            Closed, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Top, Top, Top,
        ]),
        new(nameof(GauntletETank),
        [
            Top, Top, Top, Top, Closed, Top, Open, Top, Open, Open,
            Closed, Open, Open, Top, Top, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Top, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Climb),
        [
            Closed, Open, Closed, Closed, Open, Closed, Closed, Open, Closed, Closed,
            Open, Closed, Closed, Open, Closed, Closed, Open, Closed, Closed, Open,
            Closed, Closed, Open, Closed, Closed, Top, Closed, Open, Top, Open,
            Open, Closed, Open, Open, Top, Open, Open, Closed, Open, Open,
            Top, Open, Open, Closed, Open, Open, Top, Open, Open, Closed,
        ]),
        new(nameof(Pit),
        [
            Top, Top, Top, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Closed, Top, Open, Closed, Open, Open, Closed, Open, Open,
            Top, Top, Open, Open, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Closed, Open, Closed,
        ]),
        new(nameof(ElevToBlueBrinstar),
        [
            Top, Closed, Open, Open, Open, Closed, Open, Open, Top, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open,
        ]),
        new(nameof(GreenBrinstarElev),
        [
            Top, Closed, Open, Open, Open, Closed, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Open,
        ]),
        new(nameof(GreenPiratesShaft),
        [
            Open, Open, Open, Closed, Open, Open, Top, Open, Open, Open,
            Open, Closed, Open, Open, Closed, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Closed, Closed, Closed, Open, Open, Open, Open,
        ]),
        new(nameof(CrateriaSuper),
        [
            Top, Top, Top, Open, Closed, Closed, Closed, Open, Closed, Closed,
            Closed, Open, Closed, Closed, Closed, Open, Closed, Closed, Closed, Open,
            Closed, Closed, Closed, Open, Closed, Closed, Closed, Open, Top, Top,
            Top, Top, Open, Closed, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
        ]),
        new(nameof(FinalMissileBombway),
        [
            Top, Top, Open, Closed, Open, Open, Top, Top, Open, Open,
            Closed, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(FinalMissile),
        [
            Top, Closed, Top, Open, Closed, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(GreenBrinstarMainShaft),
        [
            Open, Closed, Closed, Closed, Open, Closed, Closed, Closed, Open, Closed,
            Closed, Closed, Open, Closed, Closed, Closed, Open, Closed, Closed, Closed,
            Open, Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed, Closed,
            Open, Top, Open, Closed, Open, Closed, Open, Closed, Open, Closed,
            Open, Closed, Open, Closed, Closed, Closed, Open, Closed, Open, Open,
        ]),
        new(nameof(SporeSpawnSuper),
        [
            Top, Open, Closed, Open, Closed, Open, Closed, Open, Closed, Open,
            Closed, Open, Closed, Open, Closed, Open, Top, Top, Top, Open,
            Open, Open, Open, Open, Top, Open, Open, Open, Top, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(EarlySupers),
        [
            Closed, Closed, Closed, Top, Top, Top, Closed, Open, Top, Open,
            Open, Open, Open, Closed, Closed, Top, Closed, Open, Closed, Open,
            Open, Top, Open, Open, Open, Top, Open, Open, Closed, Open,
            Open, Open, Open, Closed, Open, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
        ]),
        new(nameof(BrinstarReserveTank),
        [
            Top, Closed, Top, Top, Open, Open, Top, Open, Open, Top,
            Top, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Top, Open, Open,
        ]),
        new(nameof(GreenBrinstarMissileRefill),
        [
            Top, Open, Top, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Top, Top, Top, Top,
        ]),
        new(nameof(Dachora),
        [
            Top, Top, Top, Top, Top, Top, Top, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Open, Closed,
            Closed, Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed,
            Closed, Closed, Open, Closed, Closed, Closed, Closed, Closed, Closed, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Closed, Closed, Open,
        ]),
        new(nameof(BigPink),
        [
            Closed, Closed, Open, Open, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Closed, Open, Open, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Closed, Open, Open, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Closed, Top, Top, Closed, Open, Closed, Closed, Closed, Closed,
            Open, Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed,
        ]),
        new(nameof(PinkBrinstarPowerBombs),
        [
            Top, Top, Closed, Closed, Closed, Open, Top, Closed, Open, Top,
            Open, Top, Open, Closed, Top, Top, Top, Open, Closed, Open,
            Closed, Open, Open, Top, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(GreenHillZone),
        [
            Open, Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open,
            Open, Open, Closed, Closed, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Top, Top, Top, Top, Top, Top,
            Top, Top, Open, Top, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
        ]),
        new(nameof(MorphBall),
        [
            Closed, Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Top, Top, Top, Top,
            Top, Top, Top, Top, Open, Top, Open, Open, Closed, Open,
            Open, Open, Open, Open, Closed, Open, Open, Top, Open, Open,
            Top, Open, Open, Open, Closed, Open, Open, Open, Open, Closed,
        ]),
        new(nameof(ConstructionZone),
        [
            Top, Closed, Closed, Open, Top, Top, Open, Open, Top, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open,
        ]),
        new(nameof(BlueBrinstarETank),
        [
            Closed, Closed, Open, Closed, Closed, Closed, Top, Top, Top, Open,
            Open, Open, Open, Top, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open,
        ]),
        new(nameof(GreenBrinstarBeetoms),
        [
            Top, Open, Top, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Top, Top,
        ]),
        new(nameof(EtecoonETank),
        [
            Top, Top, Closed, Closed, Closed, Closed, Closed, Top, Top, Top,
            Top, Open, Open, Top, Open, Top, Open, Open, Top, Open,
            Open, Top, Top, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Top, Top, Open, Open, Closed, Closed, Closed, Closed,
        ]),
        new(nameof(EtecoonSuper),
        [
            Top, Open, Top, Open, Open, Top, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Top, Open, Open, Open, Top, Open, Open,
        ]),
        new(nameof(SporeSpawnFarming),
        [
            Top, Top, Top, Open, Top, Open, Open, Open, Top, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Top,
            Open, Open, Closed, Closed, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Top, Top, Top,
        ]),
        new(nameof(WaterwayETank),
        [
            Closed, Top, Top, Top, Top, Top, Top, Closed, Top, Open,
            Open, Top, Open, Open, Top, Top, Open, Open, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Open, Open, Top, Top, Closed, Closed, Closed,
            Closed, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(RedTower),
        [
            Open, Open, Open, Open, Open, Open, Top, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Top, Open, Open, Open,
            Open, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Closed, Closed, Closed, Closed, Open, Open,
        ]),
        new(nameof(RedBrinstarFirefleas),
        [
            Open, Open, Top, Top, Open, Open, Top, Top, Top, Top,
            Closed, Closed, Top, Top, Closed, Closed, Open, Top, Open, Open,
            Open, Top, Open, Open, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Closed, Closed, Closed, Open,
        ]),
        new(nameof(Caterpillar),
        [
            Open, Closed, Closed, Open, Closed, Closed, Open, Closed, Closed, Open,
            Closed, Top, Open, Closed, Closed, Top, Closed, Closed, Closed, Closed,
            Closed, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Top, Open, Open, Open, Open, Closed, Open, Open, Top, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
        ]),
        new(nameof(BetaPowerBomb),
        [
            Top, Top, Closed, Closed, Closed, Open, Open, Top, Open, Open,
            Top, Open, Open, Open, Top, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(AlphaPowerBomb),
        [
            Closed, Top, Top, Closed, Top, Open, Open, Top, Open, Open,
            Open, Top, Open, Open, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Closed, Closed, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(BelowSpazer),
        [
            Closed, Closed, Top, Top, Closed, Open, Top, Open, Open, Closed,
            Closed, Top, Closed, Open, Top, Open, Top, Open, Open, Top,
            Open, Open, Top, Top, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Closed,
        ]),
        new(nameof(Spazer),
        [
            Top, Open, Top, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Top, Closed, Closed, Top,
        ]),
        new(nameof(WarehouseZeela),
        [
            Top, Closed, Closed, Top, Closed, Open, Open, Top, Open, Closed,
            Open, Closed, Open, Open, Top, Open, Open, Top, Open, Open,
            Top, Open, Open, Top, Top, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed, Closed,
        ]),
        new(nameof(WarehouseKihunter),
        [
            Open, Top, Top, Closed, Closed, Top, Closed, Closed, Top, Top,
            Open, Closed, Open, Top, Open, Open, Top, Open, Open, Closed,
            Open, Open, Top, Open, Closed, Open, Open, Top, Open, Open,
            Open, Top, Open, Open, Closed, Open, Open, Open, Open, Top,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(KraidEyeDoor),
        [
            Closed, Closed, Top, Top, Closed, Open, Open, Open, Top, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Top, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Top, Top, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Kraid),
        [
            Open, Open, Top, Top, Open, Closed, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Top, Open,
        ]),
        new(nameof(Statues),
        [
            Top, Closed, Closed, Open, Top, Top, Open, Top, Open, Open,
            Open, Top, Open, Open, Open, Open, Open, Open, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(WarehouseEntrance),
        [
            Top, Closed, Open, Closed, Top, Top, Closed, Open, Open, Top,
            Open, Open, Closed, Top, Open, Closed, Closed, Open, Open, Top,
            Open, Open, Top, Top, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Closed, Closed, Closed, Closed,
        ]),
        new(nameof(IceBeamGate),
        [
            Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Top, Top, Top,
            Top, Top, Top, Top, Closed, Closed, Closed, Closed, Open, Open,
            Open, Top, Open, Open, Open, Open, Top, Open, Top, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(IceBeamSnake),
        [
            Open, Closed, Open, Closed, Top, Closed, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Top, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed, Closed,
        ]),
        new(nameof(CrocomireSpeedway),
        [
            Top, Top, Top, Open, Open, Closed, Closed, Closed, Closed, Closed,
            Closed, Closed, Open, Closed, Closed, Closed, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Crocomire),
        [
            Closed, Closed, Top, Top, Top, Top, Top, Top, Top, Top,
            Open, Closed, Top, Open, Open, Open, Open, Open, Top, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open,
        ]),
        new(nameof(CrocomireEscape),
        [
            Open, Open, Open, Open, Top, Top, Top, Top, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Open, Closed, Open, Open,
            Open, Open, Open, Open, Closed, Open, Open, Closed, Closed, Closed,
            Closed, Open, Open, Closed, Closed, Open, Open, Open, Open, Open,
        ]),
        new(nameof(HiJumpETank),
        [
            Closed, Top, Closed, Closed, Closed, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Open,
        ]),
        new(nameof(PostCrocFarming),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Top, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Top,
        ]),
        new(nameof(PostCrocShaft),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Top, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(PostCrocJump),
        [
            Open, Open, Open, Open, Open, Closed, Closed, Closed, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Top, Top, Top, Top,
            Top, Top, Top, Top, Open, Open, Open, Open, Top, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(GrappleTutorial2),
        [
            Open, Open, Top, Open, Open, Open, Open, Open, Top, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(GrappleBeam),
        [
            Open, Open, Top, Closed, Open, Open, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(GreenBubblesMissiles),
        [
            Closed, Top, Closed, Top, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(BubbleMountain),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Top, Open, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(SingleChamber),
        [
            Open, Closed, Top, Top, Top, Top, Open, Closed, Closed, Closed,
            Closed, Closed, Open, Closed, Closed, Closed, Closed, Closed, Top, Closed,
            Closed, Closed, Closed, Closed, Top, Closed, Open, Top, Top, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
        ]),
        new(nameof(DoubleChamber),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Top, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Volcano),
        [
            Closed, Closed, Open, Closed, Closed, Open, Closed, Closed, Top, Open,
            Top, Open, Top, Open, Open, Top, Open, Open, Closed, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Closed, Closed,
        ]),
        new(nameof(KronicBoost),
        [
            Closed, Open, Closed, Open, Closed, Top, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Top, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Closed, Closed, Closed, Closed,
        ]),
        new(nameof(PurpleShaft),
        [
            Open, Open, Top, Closed, Open, Top, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(LowerNorfairElev),
        [
            Top, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(UpperNorfairFarming),
        [
            Open, Open, Top, Top, Open, Open, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(BatCave),
        [
            Closed, Top, Closed, Open, Top, Open, Open, Closed, Open, Top,
            Top, Open, Closed, Closed, Open, Open, Open, Open, Open, Top,
            Top, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open,
        ]),
        new(nameof(FrogSpeedway),
        [
            Top, Top, Top, Top, Top, Top, Top, Top, Open, Open,
            Open, Open, Top, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Top, Open, Open, Open, Closed, Closed, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(RedPirateShaft),
        [
            Open, Open, Open, Open, Open, Open, Open, Top, Top, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(LNElevSave),
        [
            Top, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Top, Top, Closed,
        ]),
        new(nameof(AcidStatue),
        [
            Open, Open, Closed, Top, Top, Closed, Closed, Closed, Closed, Open,
            Top, Open, Top, Open, Closed, Open, Closed, Open, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Top, Open, Top, Open, Top, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
        ]),
        new(nameof(MainHall),
        [
            Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Top, Top, Top, Top,
            Top, Top, Top, Top, Open, Open, Open, Open, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(GoldenTorizo),
        [
            Open, Open, Top, Top, Closed, Open, Top, Open, Open, Top,
            Open, Top, Open, Open, Open, Open, Open, Open, Top, Open,
            Open, Closed, Top, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open, Open,
        ]),
        new(nameof(Ridley),
        [
            Open, Open, Open, Open, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Top,
        ]),
        new(nameof(FastPillarsSetup),
        [
            Open, Top, Closed, Top, Open, Open, Top, Open, Open, Top,
            Open, Open, Open, Open, Open, Top, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(MickeyMouse),
        [
            Closed, Closed, Closed, Open, Closed, Closed, Closed, Open, Closed, Closed,
            Closed, Open, Top, Top, Top, Top, Open, Top, Open, Open,
            Closed, Open, Open, Top, Open, Open, Closed, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(WorstRoomInTheGame),
        [
            Closed, Open, Open, Open, Open, Top, Closed, Open, Top, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(LNSpringBallMaze),
        [
            Open, Closed, Closed, Closed, Closed, Top, Top, Closed, Closed, Closed,
            Top, Top, Open, Top, Open, Closed, Open, Top, Closed, Open,
            Closed, Open, Top, Open, Open, Top, Open, Open, Open, Open,
            Open, Open, Open, Top, Top, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
        ]),
        new(nameof(RedKihunterShaft),
        [
            Open, Closed, Closed, Open, Closed, Closed, Open, Closed, Closed, Open,
            Closed, Closed, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Closed, Open, Closed, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Wasteland),
        [
            Closed, Top, Top, Top, Closed, Top, Closed, Closed, Closed, Closed,
            Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed, Open, Open,
            Open, Top, Top, Open, Closed, Open, Open, Top, Open, Closed,
            Top, Top, Top, Open, Open, Closed, Open, Closed, Closed, Open,
            Top, Closed, Open, Open, Open, Open, Open, Open, Top, Open,
        ]),
        new(nameof(ThreeMusketeers),
        [
            Closed, Open, Closed, Closed, Closed, Open, Closed, Closed, Closed, Top,
            Top, Top, Open, Closed, Open, Open, Top, Open, Top, Open,
            Open, Closed, Open, Open, Open, Open, Open, Top, Top, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Top, Top,
        ]),
        new(nameof(LNFireflea),
        [
            Closed, Open, Closed, Closed, Open, Closed, Closed, Open, Closed, Closed,
            Top, Closed, Closed, Closed, Closed, Closed, Top, Top, Open, Top,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Top,
            Open, Closed, Open, Top, Open, Open, Open, Open, Open, Open,
            Open, Open, Top, Top, Open, Open, Closed, Open, Open, Open,
        ]),
        new(nameof(BowlingAlley),
        [
            Closed, Closed, Top, Top, Top, Open, Top, Top, Top, Top,
            Top, Open, Closed, Top, Closed, Closed, Closed, Open, Open, Top,
            Open, Top, Open, Open, Closed, Open, Open, Open, Open, Open,
            Top, Open, Top, Open, Open, Top, Open, Open, Top, Open,
            Open, Closed, Open, Open, Closed, Open, Top, Open, Open, Open,
        ]),
        new(nameof(Attic),
        [
            Open, Open, Open, Open, Open, Open, Open, Closed, Top, Open,
            Open, Top, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Top, Open, Open, Closed, Open, Open, Open, Open, Top,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
        ]),
        new(nameof(WreckedShipMainShaft),
        [
            Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed, Closed,
            Open, Closed, Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed,
            Closed, Closed, Open, Closed, Closed, Closed, Closed, Closed, Open, Closed,
            Top, Top, Top, Closed, Open, Closed, Closed, Closed, Closed, Closed,
            Top, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Open, Top,
        ]),
        new(nameof(ElectricDeath),
        [
            Closed, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Open, Open, Open, Open, Top,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Closed,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
        ]),
        new(nameof(Basement),
        [
            Top, Top, Top, Top, Closed, Open, Top, Open, Top, Open,
            Open, Closed, Open, Open, Closed, Open, Open, Open, Open, Open,
            Top, Top, Open, Open, Closed, Open, Open, Open, Open, Top,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
        ]),
        new(nameof(Phantoon),
        [
            Top, Open, Open, Open, Open, Open, Top, Open, Open, Closed,
            Open, Open, Open, Open, Top, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
        ]),
        new(nameof(SpongeBath),
        [
            Top, Top, Open, Open, Open, Open, Top, Top, Open, Open,
            Closed, Open, Open, Open, Open, Top, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(WSWestSuper),
        [
            Top, Open, Open, Open, Open, Open, Top, Open, Open, Closed,
            Open, Open, Open, Open, Top, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
        ]),
        new(nameof(WSEastSuper),
        [
            Top, Top, Top, Closed, Open, Top, Open, Open, Open, Open,
            Open, Top, Top, Open, Open, Closed, Open, Open, Open, Open,
            Top, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Top, Top, Closed,
            Closed, Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open,
        ]),
        new(nameof(GlassTunnel),
        [
            Open, Open, Open, Closed, Open, Open, Top, Open, Open, Open,
            Open, Open, Open, Open, Open, Top, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
        ]),
        new(nameof(WestTunnel),
        [
            Top, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Top, Top, Top, Top,
        ]),
        new(nameof(EastTunnel),
        [
            Top, Top, Top, Top, Closed, Closed, Closed, Closed, Closed, Top,
            Top, Closed, Open, Closed, Open, Closed, Open, Open, Top, Open,
            Closed, Closed, Open, Top, Open, Top, Top, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(MainStreet),
        [
            Open, Open, Closed, Open, Open, Closed, Open, Open, Top, Open,
            Open, Closed, Open, Open, Closed, Open, Open, Closed, Open, Open,
            Open, Open, Open, Closed, Open, Top, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
        ]),
        new(nameof(FishTank),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open,
        ]),
        new(nameof(MamaTurtle),
        [
            Closed, Open, Open, Closed, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open,
        ]),
        new(nameof(CrabTunnel),
        [
            Top, Top, Top, Top, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(MtEverest),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
        ]),
        new(nameof(RedFish),
        [
            Top, Closed, Open, Closed, Closed, Open, Open, Open, Open, Top,
            Top, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Closed, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open,
        ]),
        new(nameof(WateringHole),
        [
            Open, Top, Open, Closed, Closed, Closed, Open, Top, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Closed, Open, Open, Closed, Closed,
            Closed, Closed, Open, Open, Closed, Closed, Open, Open, Open, Open,
        ]),
        new(nameof(NWestMaridiaBug),
        [
            Open, Closed, Closed, Closed, Top, Closed, Top, Top, Open, Top,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
        ]),
        new(nameof(CrabShaft),
        [
            Open, Closed, Open, Closed, Top, Closed, Closed, Top, Open, Open,
            Open, Top, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed,
        ]),
        new(nameof(PseudoPlasmaSpark),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Top, Open, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Open, Open, Closed, Closed, Closed,
        ]),
        new(nameof(CrabHole),
        [
            Top, Closed, Closed, Open, Top, Top, Open, Open, Open, Open,
            Open, Top, Top, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Closed, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(WestSandHallTunnel),
        [
            Top, Open, Open, Open, Closed, Top, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Top, Open, Open, Open, Closed, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Top, Open, Open, Open, Closed, Open,
        ]),
        new(nameof(PlasmaTutorial),
        [
            Top, Open, Open, Open, Closed, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Top, Top, Open, Open,
        ]),
        new(nameof(Plasma),
        [
            Open, Open, Open, Open, Top, Top, Open, Open, Open, Open,
            Open, Top, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Closed, Closed, Closed, Closed, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Top,
        ]),
        new(nameof(ThreadTheNeedle),
        [
            Top, Top, Top, Top, Top, Top, Top, Open, Open, Open,
            Closed, Top, Open, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(MaridiaElev),
        [
            Open, Open, Open, Open, Open, Top, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Closed, Closed, Closed, Closed, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(PlasmaSpark),
        [
            Closed, Open, Open, Closed, Closed, Open, Open, Closed, Open, Open,
            Open, Open, Closed, Open, Open, Open, Closed, Open, Open, Open,
            Closed, Top, Top, Top, Open, Open, Open, Closed, Top, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Closed,
        ]),
        new(nameof(Kassiuz),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Top, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Top,
        ]),
        new(nameof(BugSandHole),
        [
            Open, Open, Open, Open, Open, Open, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Top, Closed, Closed,
            Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Oasis),
        [
            Closed, Top, Closed, Open, Top, Open, Open, Open, Open, Open,
            Open, Open, Top, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Top, Closed, Closed, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(WestSandHole),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(EastSandHole),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Top, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(WestAqueductQuicksand),
        [
            Open, Open, Open, Open, Open, Open, Top, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Closed, Closed, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(EastAqueductQuicksand),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Aqueduct),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Top, Top, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Closed, Closed, Closed, Closed,
        ]),
        new(nameof(BotwoonHallway),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Pants),
        [
            Open, Open, Open, Open, Open, Top, Top, Closed, Open, Open,
            Open, Closed, Open, Open, Open, Closed, Open, Top, Open, Closed,
            Open, Open, Closed, Open, Open, Open, Closed, Open, Top, Open,
            Closed, Open, Open, Open, Open, Top, Open, Open, Open, Open,
            Open, Top, Open, Open, Open, Closed, Open, Open, Open, Open,
        ]),
        new(nameof(EastPants),
        [
            Open, Closed, Top, Top, Open, Open, Top, Open, Open, Top,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
        ]),
        new(nameof(Springball),
        [
            Open, Closed, Top, Top, Open, Open, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Top, Top, Top, Top, Open,
        ]),
        new(nameof(BelowBotwoonETank),
        [
            Top, Top, Top, Top, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Colosseum),
        [
            Open, Open, Open, Open, Open, Open, Open, Top, Top, Top,
            Top, Top, Top, Top, Open, Open, Open, Open, Top, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
        ]),
        new(nameof(AqueductSave),
        [
            Top, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Top, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Open, Open, Open, Open, Open,
            Open, Closed, Open, Open, Open, Closed, Closed, Closed, Closed, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(ThePrecious),
        [
            Top, Top, Closed, Closed, Top, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
        ]),
        new(nameof(BotwoonETank),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Top, Top, Open, Open, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Closed, Closed, Closed, Closed, Closed, Closed, Closed,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(PlasmaBeachQuicksand),
        [
            Open, Open, Open, Open, Open, Open, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(BotwoonQuicksand),
        [
            Open, Open, Open, Open, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Closed, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Closed, Closed, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Shaktool),
        [
            Top, Top, Top, Top, Open, Open, Open, Open, Open, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
        ]),
        new(nameof(HalfieClimb),
        [
            Open, Closed, Closed, Closed, Closed, Open, Closed, Closed, Closed, Closed,
            Top, Closed, Top, Top, Top, Open, Closed, Open, Top, Open,
            Open, Top, Open, Closed, Open, Open, Top, Open, Open, Open,
            Open, Open, Open, Top, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Botwoon),
        [
            Top, Closed, Open, Open, Open, Open, Top, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Top, Open, Open, Open, Open, Top, Top,
        ]),
        new(nameof(SpaceJump),
        [
            Top, Open, Open, Open, Open, Top, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Top, Open, Open, Open, Open, Top, Open, Open,
        ]),
        new(nameof(MaridiaEnergyRefill),
        [
            Top, Open, Open, Open, Open, Top, Open, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Closed, Open, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Top, Open, Open, Open, Open,
        ]),
        new(nameof(WestCactusAlley),
        [
            Open, Top, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Closed, Closed, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(EastCactusAlley),
        [
            Open, Open, Open, Open, Open, Top, Top, Top, Top, Top,
            Open, Open, Open, Open, Open, Open, Open, Open, Top, Open,
            Open, Open, Open, Top, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Top, Top, Open, Open, Closed, Closed, Closed, Closed, Open, Open,
        ]),
        new(nameof(Draygon),
        [
            Open, Open, Top, Top, Closed, Open, Open, Open, Top, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Top,
            Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(TourianFirst),
        [
            Open, Open, Open, Top, Top, Open, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Metroids1),
        [
            Top, Top, Top, Top, Top, Top, Open, Open, Open, Open,
            Top, Open, Open, Open, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Metroids2),
        [
            Open, Open, Open, Open, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Metroids3),
        [
            Top, Top, Top, Top, Top, Top, Open, Open, Open, Open,
            Top, Open, Open, Open, Closed, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Closed, Closed, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Metroids4),
        [
            Open, Open, Open, Open, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(BlueHopper),
        [
            Top, Top, Open, Open, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(DustTorizo),
        [
            Top, Top, Open, Open, Open, Open, Open, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Closed, Closed, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Closed, Closed, Closed, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(BigBoy),
        [
            Top, Top, Top, Top, Open, Open, Open, Open, Top, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(Seaweed),
        [
            Open, Open, Open, Open, Open, Open, Top, Top, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Closed, Closed, Open,
            Open, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Top, Open, Open, Open, Open, Open, Top,
        ]),
        new(nameof(TourianRecharge),
        [
            Top, Open, Open, Open, Open, Open, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Closed, Closed, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(MotherBrain),
        [
            Top, Top, Top, Top, Open, Open, Open, Open, Open, Top,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Top, Top, Top,
        ]),
        new(nameof(TourianEyeDoor),
        [
            Top, Top, Top, Top, Open, Open, Open, Open, Top, Open,
            Open, Open, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(RinkaShaft),
        [
            Open, Open, Open, Open, Open, Open, Open, Top, Top, Open,
            Open, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Closed, Closed,
            Open, Open, Closed, Closed, Closed, Closed, Open, Open, Closed, Closed,
            Open, Open, Open, Open, Top, Open, Open, Open, Open, Open,
        ]),
        new(nameof(MotherBrainSave),
        [
            Top, Open, Open, Open, Open, Open, Top, Open, Open, Closed,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Closed, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(TourianEscape1),
        [
            Open, Open, Open, Open, Open, Open, Top, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Top, Open, Open, Open,
        ]),
        new(nameof(TourianEscape2),
        [
            Open, Top, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(TourianEscape3),
        [
            Open, Open, Open, Open, Open, Open, Top, Top, Top, Top,
            Top, Top, Open, Open, Open, Open, Open, Open, Open, Open,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
            Closed, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Open, Open, Open, Open, Open, Open, Open,
        ]),
        new(nameof(TourianEscape4),
        [
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Top, Top, Top, Open, Open,
            Open, Open, Top, Top, Open, Open, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Open, Open, Closed, Closed, Open, Open, Closed, Closed, Closed,
        ]),
        new(nameof(UpperTourianSave),
        [
            Top, Closed, Open, Open, Open, Top, Open, Open, Open, Open,
            Open, Open, Open, Open, Top, Open, Open, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Open,
            Open, Closed, Closed, Closed, Closed, Closed, Closed, Open, Open, Open,
            Open, Open, Open, Open, Open, Open, Open, Open, Open, Closed,
        ]),
    ];

    /// <summary>Every explicit table by its name.</summary>
    private static readonly Dictionary<string, RoomScrollDefinition> ByName = Definitions.ToDictionary(scroll => scroll.Name);

    /// <summary>The explicit table named <paramref name="name"/>.</summary>
    private static RoomScrollDefinition Find(string name) => ByName[name];

    /// <summary>The implicit layout whose bottom row is open with the camera aligned to the screen's top, and every other row open.</summary>
    public static RoomScrollDefinition OpenWithTopAlignedBottomRow { get; } = RoomScrollDefinition.Implicit(nameof(OpenWithTopAlignedBottomRow), RoomScrollState.OpenAlignedToTop);

    /// <summary>The implicit layout whose every row is open.</summary>
    public static RoomScrollDefinition AllOpen { get; } = RoomScrollDefinition.Implicit(nameof(AllOpen), RoomScrollState.Open);

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
