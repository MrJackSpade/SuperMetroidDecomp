using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where Lower Norfair Ridley's fight plays in his arena (#2232): where he starts and may fly, the heights his flight,
/// pogo, carry and death turn on, how near his claws must come to grab Samus, the pogo's bounce and his death's drops.
/// The targets that depend on the way he faces are <see cref="RidleyMovementTargets"/>'.
/// </summary>
internal static class NorfairRidleyArenaDefinitions
{
    /// <summary>Where Ridley is placed, below his arena's visible floor.</summary>
    internal static readonly Position16<RoomPixels> Spawn = new(96, 394);

    /// <summary>The top-left corner of the box his movement keeps him in.</summary>
    internal static readonly Position16<RoomPixels> MovementMinimum = new(64, 64);

    /// <summary>The bottom-right corner of the box his movement keeps him in.</summary>
    internal static readonly Position16<RoomPixels> MovementMaximum = new(224, 416);

    /// <summary>The point he flies to as he enters the arena, before his first attack.</summary>
    internal static readonly Position16<RoomPixels> ArenaEntry = new(64, 256);

    /// <summary>How near his entry point, hover point or swoop start he must be, on each axis, to go on at once.</summary>
    internal static readonly Extent16<RoomPixels> FlightTargetReach = new(8, 8);

    /// <summary>The height he enters the arena at, hovers at and lifts a carried Samus to.</summary>
    internal const int FlightLineY = 256;

    /// <summary>The height his swoop starts from.</summary>
    internal const int SwoopStartY = 128;

    /// <summary>
    /// The height from which Samus stands in the pogo zone: while he is healthy her standing there starts his pogo, his
    /// pogo goes on only while she stays there, and his wall sweep never follows her below it.
    /// </summary>
    internal const int PogoZoneTopY = 352;

    /// <summary>The height his pogo starts from: he descends to it before his first bounce.</summary>
    internal const int PogoHeightY = 288;

    /// <summary>How near his claw Samus must come, past her own reach on each axis, for his pogo to grab her.</summary>
    internal static readonly Extent16<RoomPixels> PogoClawReach = new(4, 4);

    /// <summary>How near his claw Samus must come, past her own reach on each axis, for his lunge to grab her.</summary>
    internal static readonly Extent16<RoomPixels> LungeClawReach = new(8, 12);

    /// <summary>Where his lunge aims beside Samus while he faces right or the screen; facing left mirrors it.</summary>
    internal static readonly Offset16<RoomPixels> LungeAimBesideSamus = new(16, -4);

    /// <summary>Where a pogo bounce's dust rises, below the tail's tip.</summary>
    internal static readonly Offset16<RoomPixels> PogoDustBelowTip = new(0, 12);

    /// <summary>The medium vertical earthquake a pogo bounce shakes the foreground and background with.</summary>
    internal static readonly EarthquakeShake PogoBounceQuake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.ForegroundAndBackground);

    /// <summary>The updates a pogo bounce's earthquake lasts.</summary>
    internal const int PogoBounceQuakeUpdates = 4;

    /// <summary>The height above which his carry lifts Samus to his flight line; from lower it lifts her by <see cref="CarryLift"/>.</summary>
    internal const int CarryHighLineY = 320;

    /// <summary>How far his carry lifts Samus from low in the arena.</summary>
    internal const int CarryLift = 64;

    /// <summary>The height his carry flies to as he releases Samus.</summary>
    internal const int CarryReleaseY = 224;

    /// <summary>The spot he carries Samus to as he dies, roars at and breaks up over.</summary>
    internal static readonly Position16<RoomPixels> DeathSpot = new(128, 328);

    /// <summary>How near the death spot he must be, on each axis, to begin his death roar.</summary>
    internal static readonly Extent16<RoomPixels> DeathSpotReach = new(4, 4);

    /// <summary>The drops his death scatters.</summary>
    internal const int DeathDrops = 16;

    /// <summary>The top-left corner of the stretch of the arena his death's drops scatter over.</summary>
    internal static readonly Position16<RoomPixels> DeathDropCorner = new(64, 320);
}
