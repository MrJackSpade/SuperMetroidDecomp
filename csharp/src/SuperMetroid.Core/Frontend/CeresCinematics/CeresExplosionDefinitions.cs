using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>The Ceres destruction blasts' actors and where each wave places them.</summary>
internal static class CeresExplosionDefinitions
{
    /// <summary>
    /// The invisible actor whose animation (<see cref="CeresAnimations.ExplosionSpawner"/>) spawns the station's three
    /// waves of blasts (#2489).
    /// </summary>
    public static CinematicActorDefinition<CeresAnimation> SpawnerActor =>
        new(CeresAnimation.ExplosionSpawner);

    /// <summary>First delayed small-explosion actor.</summary>
    /// <remarks>Its animation burns through the small explosion's pictures, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition<CeresAnimation> InitialActor =>
        new(CeresAnimation.SmallExplosion);

    /// <summary>Cyclic repeating small-explosion actor.</summary>
    /// <remarks>Its animation repeats the small explosion's pictures and a blank six times, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition<CeresAnimation> RepeatingActor =>
        new(CeresAnimation.RepeatingSmallExplosion);

    /// <summary>Final delayed large-explosion actor.</summary>
    /// <remarks>Its animation repeats the large explosion's pictures and a blank seven times, then deletes. It runs as a typed cinematic sprite animation.</remarks>
    public static CinematicActorDefinition<CeresAnimation> FinalWaveActor =>
        new(CeresAnimation.LargeExplosion);

    /// <summary>Final station blast actor created as the station explodes.</summary>
    /// <remarks>Its animation burns through the station blast's pictures, then deletes. It runs as a typed cinematic sprite animation.</remarks>
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
    /// The six repeating small blasts' places around the station, each an offset from the turning floor's centre on screen,
    /// in the order the spawner's animation spawns them, one place each play of its repeating blast (#2489, #2541). The
    /// places stay on screen while the turning floor's picture shrinks beneath them.
    /// </summary>
    internal static IReadOnlyList<Offset16<ScreenPixels>> RepeatingBlastPlaces { get; } =
    [
        new(14, -8),
        new(8, 12),
        new(-16, 12),
        new(-8, -14),
        new(0, 0),
        new(16, 14),
    ];

    /// <summary>The updates before every repeating blast's first picture shows: it appears on the update after it is spawned.</summary>
    internal const int RepeatingBlastDelayFrames = 1;

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
