namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Where Spore Spawn's fight takes place in his room and how his sway grows (#2171): the arena's centre he descends to,
/// hardens at and drifts back to as he dies, the ceiling his stalk hangs from, and the widths of his sway.
/// </summary>
internal static class SporeSpawnArenaDefinitions
{
    /// <summary>
    /// The arena centre's X in room pixels: where he hardens, where his death drift ends and where his stalk leaves the
    /// ceiling.
    /// </summary>
    internal const int CentreX = 128;

    /// <summary>The arena centre's Y in room pixels: where his descent ends, where he hardens and where his death drift ends.</summary>
    internal const int CentreY = 624;

    /// <summary>How far above his spawn the live fight raises his body before he descends, in pixels.</summary>
    internal const int FightStartRise = 128;

    /// <summary>The ceiling's Y in room pixels, where his stalk's first link hangs.</summary>
    internal const int CeilingY = 560;

    /// <summary>How far above his body's centre his stalk's last link reaches, in pixels.</summary>
    internal const int StalkBodyGap = 40;

    /// <summary>The horizontal sway width, in pixels either side of his sway's centre, his descent leaves for the fight's first sway.</summary>
    internal const int DescentSwayRadius = 48;

    /// <summary>The horizontal sway width, in pixels, below which each closing of his shell widens the sway.</summary>
    internal const int WidenedSwayLimit = 40;

    /// <summary>The pixels each closing of his shell adds to a sway narrower than <see cref="WidenedSwayLimit"/>.</summary>
    internal const int SwayWidening = 8;

    /// <summary>How much narrower his vertical sway is than his horizontal sway, in pixels.</summary>
    internal const int VerticalSwayNarrowing = 16;

    /// <summary>How close on each axis, in pixels and exclusive, his death drift brings him to the arena centre before he settles.</summary>
    internal const int DeathDriftSettleDistance = 8;

    /// <summary>The left edge of the strip his idle dust clouds rise across as he dies, in room pixels.</summary>
    internal const int IdleDustLeft = 96;

    /// <summary>The top of the band his idle dust clouds rise within as he dies, in room pixels.</summary>
    internal const int IdleDustTop = 480;

    /// <summary>
    /// The lowest Y, in room pixels, the camera's top edge may take while his live fight goes on: the camera never looks
    /// above his arena until he dies.
    /// </summary>
    internal const int LowestCameraY = 464;

    /// <summary>Where his stalk hangs from as he is placed, above his body (#2232).</summary>
    internal static readonly Offset16<RoomPixels> StalkAnchorAboveBody = new(0, -72);

    /// <summary>The pixels his descent lowers him each update.</summary>
    internal const int DescentSpeed = 1;

    /// <summary>The angle step his sway takes each update once a hit low on health quickens it, either way.</summary>
    internal const int FastSwingStep = 2;

    /// <summary>How far left of and above his body the box his hardening's dust bursts scatter over starts.</summary>
    internal static readonly Offset16<RoomPixels> HardeningDustCentring = new(64, 64);

    /// <summary>How far left of and above his body the box his dying explosions scatter over starts.</summary>
    internal static readonly Offset16<RoomPixels> DyingExplosionCentring = new(64, 32);

    /// <summary>The drops his death scatters.</summary>
    internal const int DeathDrops = 16;

    /// <summary>The top-left corner of the stretch of his room his death's drops scatter over.</summary>
    internal static readonly Position16<RoomPixels> DeathDropCorner = new(64, 528);

    /// <summary>The bottom of his room, which a falling spore is deleted once it reaches.</summary>
    internal const int SporeFloorY = 768;
}
