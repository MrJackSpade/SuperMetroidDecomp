using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and physical spawn schedule for the Ceres destruction blasts.</summary>
internal static class CeresExplosionDefinitions
{

    /// <summary>Number of actors spawned by the first wave.</summary>
    public const int InitialExplosionCount = 5;

    /// <summary>Number of cyclic offsets consumed by the repeating wave's frame behaviour.</summary>
    public const int RepeatingExplosionCount = 8;

    /// <summary>Number of actors spawned by the final wave.</summary>
    public const int FinalExplosionCount = 4;

    /// <summary>Invisible wait before the first wave.</summary>
    public const int InitialWaitFrames = 128;

    /// <summary>Wait before installing the repeating wave's frame behaviour.</summary>
    public const int RepeatingStartWaitFrames = 80;

    /// <summary>Final spawner lifetime before the final wave.</summary>
    public const int RepeatingLifetimeFrames = 64;

    /// <summary>Frames between repeating second-wave explosions.</summary>
    public const int RepeatingPeriodFrames = 12;

    /// <summary>First host frame on which the initial wait has expired.</summary>
    public const int InitialSpawnFrame = InitialWaitFrames + 1;

    /// <summary>First host frame on which the newly installed repeating behaviour runs.</summary>
    public const int RepeatingFirstFrame = InitialWaitFrames + RepeatingStartWaitFrames + 2;

    /// <summary>Last host frame owned by the spawner and its final-wave cue.</summary>
    public const int SpawnerFinalFrame =
        InitialWaitFrames + RepeatingStartWaitFrames + RepeatingLifetimeFrames + 1;

    /// <summary>The cinematic phase's countdown seed when the initial fade completes.</summary>
    internal const int RepeatingCountdownSeed = 1;
    /// <summary>First delayed small-explosion actor.</summary>
    /// <remarks>Its animation displays six duration-3 small-explosion frames, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition InitialActor =>
        new(CinematicSpriteFrameBehavior.CeresExplosion, CinematicSpriteSegment.CeresSmallExplosion);

    /// <summary>Cyclic repeating small-explosion actor.</summary>
    /// <remarks>Its animation repeats six duration-3 frames and a duration-16 blank six times, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition RepeatingActor =>
        new(CinematicSpriteFrameBehavior.CeresExplosion, CinematicSpriteSegment.CeresRepeatingSmallExplosion);

    /// <summary>Final delayed large-explosion actor.</summary>
    /// <remarks>Its animation repeats four duration-5 frames and a duration-8 blank seven times, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition FinalWaveActor =>
        new(CinematicSpriteFrameBehavior.CeresExplosion, CinematicSpriteSegment.CeresLargeExplosion);

    /// <summary>Final station blast actor created as the station explodes.</summary>
    /// <remarks>Its animation displays six duration-5 station-blast frames, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition StationBlastActor =>
        new(CinematicSpriteFrameBehavior.CeresExplosion, CinematicSpriteSegment.CeresStationBlast);

    /// <summary>The repeating burst's eight chosen screen-space positions.</summary>
    /// <remarks>
    /// Retained under #1165's drawing/choreography exception. Each ordinal creates the
    /// same small blast, then advances modulo eight even if allocation fails. These
    /// points describe the particular scattered sequence around the station, not sampled
    /// motion, actor dispatch, or hull attachment points: the offsets stay in screen space
    /// while the underlying turning-floor image changes scale. A per-index switch or fitted curve
    /// would just encode this same layout. The initial square/center layout, final X rule,
    /// animation delays and shared actor motion are calculated separately.
    /// </remarks>
    internal static readonly Offset16<SpritePixels>[] RepeatingBurstLayout =
    [
        new Offset16<SpritePixels>(14, -8), new Offset16<SpritePixels>(8, 12), new Offset16<SpritePixels>(-16, 12), new Offset16<SpritePixels>(-8, -14),
        new Offset16<SpritePixels>(0, 0), new Offset16<SpritePixels>(16, 14), new Offset16<SpritePixels>(-12, 4), new Offset16<SpritePixels>(-8, -16),
    ];

    /// <summary>Chosen heights of the four final large bursts.</summary>
    /// <remarks>
    /// Retained as the final burst's spatial choreography under #1165. These are absolute
    /// offsets from the scrolling anchor, not successive displacements or a velocity curve.
    /// The alternating upper/lower placements have individually chosen heights; turning
    /// their ordinal-to-height list into cases would merely recite the layout. Only this
    /// Y content is retained: the independent X and delay fields remain calculated.
    /// </remarks>
    internal static ReadOnlySpan<short> FinalBurstHeights => [-4, 8, -10, 12];
}
