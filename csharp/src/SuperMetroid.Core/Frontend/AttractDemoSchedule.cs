using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Where the attract demos are: the set the title plays and how far into it the next demonstration is, the loading and
/// playback countdowns of the scene running, and how it ended.
/// </summary>
internal sealed class AttractDemoSchedule
{
    /// <summary>The demo set the title plays.</summary>
    internal AttractDemoSet Set { get; set; } = StockAttractDemoScenes.FirstSet;

    /// <summary>
    /// How many of <see cref="Set"/>'s demonstrations have begun since the set last started from its first; the next
    /// one plays from this place in the set's list.
    /// </summary>
    internal int ScenesBegun { get; set; }

    /// <summary>
    /// Where the scene stands (#2560): awaiting its runtime, loading, playing its scripted input or holding its final image,
    /// each with its own countdown.
    /// </summary>
    internal SceneState Scene { get; set; } = new SceneState.Awaiting();

    /// <summary>How the scene last begun ended: played to its end, or cancelled by a press (#2560).</summary>
    private AttractPlaybackEnd end;

    /// <summary>
    /// Where the attract mode goes after the scene (#2560): the set's next scene, the title at its title screen after a
    /// cancelled scene, or the title from its beginning once the set has played through.
    /// </summary>
    internal AttractDemoRoute RouteAfterScene { get; private set; }

    /// <summary>Whether the next scene's runtime has yet to be created.</summary>
    internal bool AwaitsScene => Scene is SceneState.Awaiting;

    /// <summary>Whether the scene's scripted input has ended and its final image is holding.</summary>
    internal bool Holding => Scene is SceneState.Holding { Hold.IsRunning: true };

    /// <summary>A soft reset returns to the first scene of the first set.</summary>
    internal void Reset()
    {
        Set = StockAttractDemoScenes.FirstSet;
        ScenesBegun = 0;
        Scene = new SceneState.Awaiting();
    }

    /// <summary>The title requests a demo: its set starts again from its first scene.</summary>
    internal void RequestFromTitle()
    {
        ScenesBegun = 0;
        Scene = new SceneState.Awaiting();
    }

    /// <summary>The scene the next demo plays, or null once every demonstration of the set has begun.</summary>
    /// <exception cref="InvalidOperationException">More demonstrations began than the set holds.</exception>
    internal AttractDemoScene? NextScene =>
        ScenesBegun < Set.Scenes.Count ? Set.Scenes[ScenesBegun]
        : ScenesBegun == Set.Scenes.Count ? null
        : throw new InvalidOperationException($"{ScenesBegun} demonstrations began in {Set}, which holds {Set.Scenes.Count}.");

    /// <summary>Starts <paramref name="scene"/>, the next scene, with its scripted input and enemy-setup waits ahead.</summary>
    internal void Begin(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        ScenesBegun++;
        end = AttractPlaybackEnd.Completed;
        Scene = new SceneState.Loading(UpdateCountdown.Lasting(GameLoadingDefinitions.EnemySetupWaits), scene.Duration);
    }

    /// <summary>Counts one enemy-setup wait; returns whether it was the last, after which the scene plays.</summary>
    /// <exception cref="InvalidOperationException">No scene is loading.</exception>
    internal bool CountLoadFrame()
    {
        SceneState.Loading loading = Scene as SceneState.Loading
            ?? throw new InvalidOperationException("No attract demo scene is loading.");
        if (!loading.Waits.Count())
            return false;
        Scene = new SceneState.Playing(UpdateCountdown.For(loading.Duration));
        return true;
    }

    /// <summary>Counts one held frame; returns whether the hold ended.</summary>
    /// <exception cref="InvalidOperationException">The scene's final image is not holding.</exception>
    internal bool CountHoldFrame() => (Scene as SceneState.Holding
        ?? throw new InvalidOperationException("No attract demo scene holds its final image.")).Hold.Count();

    /// <summary>Counts one scripted frame; once the script runs out, the final image starts holding.</summary>
    /// <exception cref="InvalidOperationException">No scene plays its scripted input.</exception>
    internal void CountPlaybackFrame()
    {
        SceneState.Playing playing = Scene as SceneState.Playing
            ?? throw new InvalidOperationException("No attract demo scene plays its scripted input.");
        if (playing.Playback.Step())
            Scene = new SceneState.Holding(UpdateCountdown.Lasting(AttractDemoDefinitions.FinalImageHoldFrames));
    }

    /// <summary>Records how the scene ended.</summary>
    internal void Finish(AttractPlaybackEnd ended) => end = ended;

    /// <summary>
    /// After a scene: an uncancelled demo continues with the set's next scene, and once the set is exhausted the set that
    /// follows it in <paramref name="playlist"/>, which starts over after its last, plays from its first scene.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="playlist"/> does not hold the set that played.</exception>
    internal void AdvanceAfterScene(IReadOnlyList<AttractDemoSet> playlist)
    {
        Ensure.NotNull(playlist);
        if (end == AttractPlaybackEnd.Cancelled)
        {
            RouteAfterScene = AttractDemoRoute.TitleAtItsScreen;
            return;
        }
        if (NextScene is not null)
        {
            RouteAfterScene = AttractDemoRoute.NextScene;
            return;
        }
        Set = SetAfter(playlist, Set);
        ScenesBegun = 0;
        RouteAfterScene = AttractDemoRoute.TitleFromBeginning;
    }

    /// <summary>The next scene of the set loads afresh.</summary>
    internal void LoadNextScene() => Scene = new SceneState.Awaiting();

    /// <summary>The set that follows <paramref name="played"/> in <paramref name="playlist"/>, which starts over after its last.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="playlist"/> does not hold <paramref name="played"/>.</exception>
    private static AttractDemoSet SetAfter(IReadOnlyList<AttractDemoSet> playlist, AttractDemoSet played)
    {
        for (int place = 0; place < playlist.Count; place++)
        {
            if (ReferenceEquals(playlist[place], played))
                return playlist[(place + 1) % playlist.Count];
        }
        throw new InvalidOperationException($"The attract playlist does not hold {played}.");
    }

    /// <summary>Where a scene stands (#2560), one value: each case carries only its own countdown.</summary>
    internal abstract record SceneState
    {
        /// <summary>The scene's runtime has yet to be created.</summary>
        internal sealed record Awaiting : SceneState;

        /// <summary>The scene's runtime exists and the screen stays black (#1627).</summary>
        /// <param name="Waits">The blank enemy-setup waits (<see cref="GameLoadingDefinitions.EnemySetupWaits"/>) left before the scene shows.</param>
        /// <param name="Duration">How long the scene's scripted input lasts once it plays.</param>
        internal sealed record Loading(UpdateCountdown Waits, int Duration) : SceneState;

        /// <summary>The scene plays its scripted input.</summary>
        /// <param name="Playback">The scripted input left, counted once each scripted update.</param>
        internal sealed record Playing(UpdateCountdown Playback) : SceneState;

        /// <summary>The scripted input has run out and the final image holds.</summary>
        /// <param name="Hold">The still-frame hold left.</param>
        internal sealed record Holding(UpdateCountdown Hold) : SceneState;
    }
}
