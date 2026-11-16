namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Where Draygon's fight plays and how far its reaches extend (#2216): his swim line and his passes' edges, his mouth,
/// claws and eye, the goop's reach and the slowdown it stacks on Samus, the grapple shock, and his death's drift,
/// drops and smoke.
/// </summary>
internal static class DraygonArenaDefinitions
{
    /// <summary>The height of Draygon's swim line: where his goop pass sways about, his swoop starts and his intro's Evirs enter.</summary>
    internal const int SwimLineY = 384;

    /// <summary>How far above and below his swim line Draygon's goop pass sways.</summary>
    internal const int GoopSwayRadius = 32;

    /// <summary>Where a pass to the left sets up its swoop, right of the arena.</summary>
    internal const int RightSwoopSetupX = 592;

    /// <summary>How near Samus Draygon's goop pass comes, horizontally, before he turns to whip his tail.</summary>
    internal const int GoopTailRange = 208;

    /// <summary>The goops one goop pass fires.</summary>
    internal const int GoopsPerPass = 16;

    /// <summary>The updates of Draygon's intro, while its Evirs dance before he enters.</summary>
    internal const int IntroUpdates = 256;

    /// <summary>Where the intro's Evirs enter: at the arena's left edge on Draygon's swim line.</summary>
    internal static readonly Position16<RoomPixels> IntroEvirEntry = new(16, SwimLineY);

    /// <summary>The square, in pixels, the Draygon room's projectiles stay inside before they are deleted.</summary>
    internal const int ArenaSize = 512;

    /// <summary>Where Draygon's foam leaves his mouth when he faces right; facing left mirrors it.</summary>
    internal static readonly Offset16<RoomPixels> MouthFacingRight = new(32, -16);

    /// <summary>Where his breath bubbles rise from, up and left of his body.</summary>
    internal static readonly Offset16<RoomPixels> BreathBubbleOffset = new(-16, -16);

    /// <summary>How far ahead of his body, toward his facing, Draygon's claws reach for Samus.</summary>
    internal const int ClawReach = 8;

    /// <summary>How near the claws' reach Samus must be, on each axis, for Draygon to grab her.</summary>
    internal const int GrabWindow = 8;

    /// <summary>How near the spiral's target Draygon must swim, on each axis, before he flails Samus there.</summary>
    internal const int SpiralTargetTolerance = 2;

    /// <summary>The spiral's horizontal radius over its vertical one: he spins Samus in an ellipse four times as wide as tall.</summary>
    internal const int SpiralWidthToHeight = 4;

    /// <summary>The health a grapple shock takes from Draygon; a shock with no more health than this left kills him.</summary>
    internal const int GrappleShockDamage = 256;

    /// <summary>The updates a grapple shock's hurt flash lasts beyond Draygon's ordinary hurt flash.</summary>
    internal const int GrappleShockFlashExtension = 8;

    /// <summary>Where a goop leaves Draygon moving right, from his body.</summary>
    internal static readonly Offset16<RoomPixels> GoopSpawnMovingRight = new(24, -16);

    /// <summary>Where a goop leaves Draygon moving left, from his body.</summary>
    internal static readonly Offset16<RoomPixels> GoopSpawnMovingLeft = new(-28, -16);

    /// <summary>How near Samus a flying goop must come, on each axis, to stick to her.</summary>
    internal static readonly Extent16<RoomPixels> GoopTouchReach = new(16, 20);

    /// <summary>The X-speed divisor no stuck goop raises Samus's to: at most five goops slow her.</summary>
    internal const int GoopSlowdownLimit = 6;

    /// <summary>How far apart, vertically, the goops stuck to Samus stack.</summary>
    internal const int GoopStackSpacing = 4;

    /// <summary>Where the goops stuck to Samus stack from, above her position.</summary>
    internal const int GoopStackTop = -12;

    /// <summary>The pixels Draygon climbs each update as his swoop carries him out above the arena.</summary>
    internal const int SwoopClimbOutSpeed = 4;

    /// <summary>The foreground's strong shake, in updates, when Draygon's tail hits the Samus he holds.</summary>
    internal const int TailHitShakeUpdates = 32;

    /// <summary>How far below Samus the dust of a tail hit rises.</summary>
    internal const int TailHitDustBelowSamus = 16;

    /// <summary>How far, on each axis, the dying Draygon's scattered effects land from his body each way.</summary>
    internal const int DyingScatterReach = 64;

    /// <summary>How near his burial point the dying Draygon must drift, on each axis, before he sinks.</summary>
    internal const int BurialPointTolerance = 4;

    /// <summary>The drops Draygon's death scatters over the lower half of the arena.</summary>
    internal const int DeathDropCount = 16;

    /// <summary>The corner of the band the death's drops scatter over.</summary>
    internal static readonly Position16<RoomPixels> DeathDropOrigin = new(128, 352);

    /// <summary>The corner of the area his death's smoke rises in.</summary>
    internal static readonly Position16<RoomPixels> DeathSmokeOrigin = new(192, 400);
}
