using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>
<<<<<<< ours
/// Where the attract demos are: the set the title plays, how far into it the next demonstration is, and how the scene last
/// begun ended. The scene's own loading, playback and hold countdowns are held by the demo step that counts them
/// (<see cref="AttractDemoGameSequence"/>).
||||||| base
/// Where the attract demos are: the set the title plays and how far into it the next demonstration is, the loading and
/// playback countdowns of the scene running, and how it ended.
=======
/// Which demonstrations the title plays (#2553): the set of <paramref name="playlist"/> it is at, which starts over after the
/// last, and how far into it the next demonstration is. Where a running scene stands is its step's own
/// (<see cref="AttractDemoStep"/>).
>>>>>>> theirs
/// </summary>
/// <param name="playlist">The sets the title cycles through, in order, the first set first.</param>
internal sealed class AttractDemoSchedule(IReadOnlyList<AttractDemoSet> playlist)
{
    /// <summary>The sets the title cycles through, in order.</summary>
    private readonly IReadOnlyList<AttractDemoSet> playlist = Ensure.NotNull(playlist);

    /// <summary>The place in <see cref="playlist"/> of the set the title plays.</summary>
    private int setPlace;

    /// <summary>The demo set the title plays.</summary>
    private AttractDemoSet Set => playlist[setPlace];

    /// <summary>
    /// How many of <see cref="Set"/>'s demonstrations have begun since the set last started from its first; the next
    /// one plays from this place in the set's list.
    /// </summary>
    internal int ScenesBegun { get; private set; }

<<<<<<< ours
    /// <summary>How the scene last begun ended: played to its end, or cancelled by a press (#2560).</summary>
    private AttractPlaybackEnd end;

    /// <summary>
    /// Where the attract mode goes after the scene (#2560): the set's next scene, the title at its title screen after a
    /// cancelled scene, or the title from its beginning once the set has played through.
    /// </summary>
    internal AttractDemoRoute RouteAfterScene { get; private set; }

||||||| base
    /// <summary>
    /// Where the scene stands (#2560): awaiting its runtime, loading, playing its scripted input or holding its final image,
    /// each with its own countdown.
    /// </summary>
    private SceneState Scene { get; set; } = new SceneState.Awaiting();

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

=======
>>>>>>> theirs
    /// <summary>A soft reset returns to the first scene of the first set.</summary>
    internal void Reset()
    {
        setPlace = 0;
        ScenesBegun = 0;
    }

    /// <summary>The title requests a demo: its set starts again from its first scene, whose load begins.</summary>
    internal AttractDemoStep.Loading.AwaitingRuntime RequestFromTitle()
    {
        ScenesBegun = 0;
<<<<<<< ours
    }

    /// <summary>The scene the next demo plays, or null once every demonstration of the set has begun.</summary>
    /// <exception cref="InvalidOperationException">More demonstrations began than the set holds.</exception>
    internal AttractDemoScene? NextScene =>
        ScenesBegun < Set.Scenes.Count ? Set.Scenes[ScenesBegun]
        : ScenesBegun == Set.Scenes.Count ? null
        : throw new InvalidOperationException($"{ScenesBegun} demonstrations began in {Set}, which holds {Set.Scenes.Count}.");

    /// <summary>The scene the next demo plays, which a demo awaiting its scene always has.</summary>
    /// <exception cref="InvalidDataException">Every demonstration of the set has begun.</exception>
    internal AttractDemoScene SceneToBegin => ScenesBegun < Set.Scenes.Count ? Set.Scenes[ScenesBegun]
        : throw new InvalidDataException($"The demo loader ran past the end of {Set}, which holds {Set.Scenes.Count} demonstrations.");

    /// <summary>Counts <paramref name="scene"/>, the next scene, as begun; it plays to its end unless a press cancels it.</summary>
    internal void Begin(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        ScenesBegun++;
        end = AttractPlaybackEnd.Completed;
||||||| base
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
=======
        return new(BeginNext());
>>>>>>> theirs
    }

    /// <summary>
    /// Where the attract mode goes after a scene that ended as <paramref name="end"/>: the title at its title screen after a
    /// cancelled scene; else the set's next scene, which begins; else, once the set has played through, the title from its
    /// beginning, the set that follows in the playlist playing from its first scene next.
    /// </summary>
    internal AttractDemoRoute RouteAfterScene(AttractPlaybackEnd end)
    {
        if (end == AttractPlaybackEnd.Cancelled)
            return new AttractDemoRoute.TitleAtItsScreen();
        if (ScenesBegun < Set.Scenes.Count)
            return new AttractDemoRoute.NextScene(BeginNext());
        setPlace = (setPlace + 1) % playlist.Count;
        ScenesBegun = 0;
<<<<<<< ours
        RouteAfterScene = AttractDemoRoute.TitleFromBeginning;
    }

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
||||||| base
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
=======
        return new AttractDemoRoute.TitleFromBeginning();
>>>>>>> theirs
    }
<<<<<<< ours
||||||| base

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
=======

    /// <summary>Begins the set's next scene and returns it.</summary>
    private AttractDemoScene BeginNext() => Set.Scenes[ScenesBegun++];
>>>>>>> theirs
}
