using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and physical spawn schedule for the Ceres destruction blasts.</summary>
internal static class CeresExplosionDefinitions
{

    /// <summary>Invisible wait before the first wave.</summary>
    public const int InitialWaitFrames = 128;

    /// <summary>Wait before the repeating wave starts.</summary>
    public const int RepeatingStartWaitFrames = 80;

    /// <summary>Final spawner lifetime before the final wave.</summary>
    public const int RepeatingLifetimeFrames = 64;

    /// <summary>Frames between repeating second-wave explosions.</summary>
    public const int RepeatingPeriodFrames = 12;

    /// <summary>The scene's first frame after the initial wait has run out.</summary>
    public const int InitialSpawnFrame = InitialWaitFrames + 1;

    /// <summary>The scene's first frame of the repeating wave.</summary>
    public const int RepeatingFirstFrame = InitialWaitFrames + RepeatingStartWaitFrames + 2;

    /// <summary>The scene's last frame of the spawner and its final wave.</summary>
    public const int SpawnerFinalFrame =
        InitialWaitFrames + RepeatingStartWaitFrames + RepeatingLifetimeFrames + 1;

    /// <summary>The cinematic phase's countdown seed when the initial fade completes.</summary>
    internal const int RepeatingCountdownSeed = 1;
    /// <summary>First delayed small-explosion actor.</summary>
    /// <remarks>Its animation displays six duration-3 small-explosion frames, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition<CeresAnimation> InitialActor =>
        new(CeresAnimation.SmallExplosion);

    /// <summary>Cyclic repeating small-explosion actor.</summary>
    /// <remarks>Its animation repeats six duration-3 frames and a duration-16 blank six times, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition<CeresAnimation> RepeatingActor =>
        new(CeresAnimation.RepeatingSmallExplosion);

    /// <summary>Final delayed large-explosion actor.</summary>
    /// <remarks>Its animation repeats four duration-5 frames and a duration-8 blank seven times, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition<CeresAnimation> FinalWaveActor =>
        new(CeresAnimation.LargeExplosion);

    /// <summary>Final station blast actor created as the station explodes.</summary>
    /// <remarks>Its animation displays six duration-5 station-blast frames, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition<CeresAnimation> StationBlastActor =>
        new(CeresAnimation.StationBlast);

    /// <summary>
    /// The first wave's five small blasts, in the order they appear: one at each corner of a square around the turning
    /// floor's centre, one after another, then one at the centre. Each offset is from the floor's centre on screen.
    /// </summary>
    internal static IReadOnlyList<CeresExplosionPlacement> InitialWave { get; } =
    [
        new(new(16, -16), 1),
        new(new(-16, 16), 16),
        new(new(16, 16), 32),
        new(new(-16, -16), 48),
        new(new(0, 0), 64),
    ];

    /// <summary>
    /// The repeating small blasts' eight places around the station, in the order they appear; after the eighth the next
    /// blast appears at the first again. Each appears on the update after it is spawned. The places stay on screen while
    /// the turning floor's picture shrinks beneath them.
    /// </summary>
    internal static IReadOnlyList<CeresExplosionPlacement> RepeatingBlasts { get; } =
    [
        new(new(14, -8), 1),
        new(new(8, 12), 1),
        new(new(-16, 12), 1),
        new(new(-8, -14), 1),
        new(new(0, 0), 1),
        new(new(16, 14), 1),
        new(new(-12, 4), 1),
        new(new(-8, -16), 1),
    ];

    /// <summary>
    /// The final wave's four large blasts, in the order they appear: two to the right of the floor's centre, then two to
    /// its left, each pair one above and one below.
    /// </summary>
    internal static IReadOnlyList<CeresExplosionPlacement> FinalWave { get; } =
    [
        new(new(8, -4), 1),
        new(new(12, 8), 4),
        new(new(-8, -10), 8),
        new(new(-12, 12), 16),
    ];
}
