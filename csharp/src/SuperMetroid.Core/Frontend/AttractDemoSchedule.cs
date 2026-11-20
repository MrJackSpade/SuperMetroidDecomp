using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Where the attract demos are: the set the title plays and how far into it the next demonstration is, the loading and
/// playback countdowns of the scene running, and how it ended.
/// </summary>
internal sealed class AttractDemoSchedule
{
    /// <summary>The demo set the title plays.</summary>
    internal AttractDemoSet Set { get; private set; } = StockAttractDemoScenes.FirstSet;

    /// <summary>
    /// How many of <see cref="Set"/>'s demonstrations have begun since the set last started from its first; the next
    /// one plays from this place in the set's list.
    /// </summary>
    internal int ScenesBegun { get; private set; }

    /// <summary>The scene's scripted input, lasting the scene's duration and counted once each scripted update.</summary>
    internal UpdateCountdown Playback { get; private set; } = UpdateCountdown.Ended();

    /// <summary>The still-frame hold after the scene's scripted input, over until the script runs out.</summary>
    internal UpdateCountdown Hold { get; private set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The blank enemy-setup waits (<see cref="GameLoadingDefinitions.EnemySetupWaits"/>) before the scene becomes visible;
    /// over before its runtime exists (#1627).
    /// </summary>
    private UpdateCountdown load = UpdateCountdown.Ended();

    /// <summary>Whether live controller input cancelled the scene.</summary>
    internal bool Cancelled { get; private set; }

    /// <summary>Whether another scene follows before returning to the title.</summary>
    internal bool HasNextScene { get; private set; }

    /// <summary>Whether the next scene's runtime has yet to be created.</summary>
    internal bool AwaitsScene { get; private set; } = true;

    /// <summary>Whether the scene's scripted input has ended and its final image is holding.</summary>
    internal bool Holding => Hold.IsRunning;

    /// <summary>A soft reset returns to the first scene of the first set.</summary>
    internal void Reset()
    {
        Set = StockAttractDemoScenes.FirstSet;
        ScenesBegun = 0;
        AwaitsScene = true;
    }

    /// <summary>The title requests a demo: its set starts again from its first scene.</summary>
    internal void RequestFromTitle()
    {
        ScenesBegun = 0;
        AwaitsScene = true;
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

    /// <summary>Starts <paramref name="scene"/>, the next scene, with its scripted input and enemy-setup waits ahead.</summary>
    internal void Begin(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        Playback = UpdateCountdown.For(scene.Duration);
        ScenesBegun++;
        Hold = UpdateCountdown.Ended();
        Cancelled = false;
        load = UpdateCountdown.Lasting(GameLoadingDefinitions.EnemySetupWaits);
        AwaitsScene = false;
    }

    /// <summary>Counts one enemy-setup wait; returns whether it was the last.</summary>
    /// <exception cref="InvalidOperationException">No scene is loading: its waits are over.</exception>
    internal bool CountLoadFrame() => load.Count();

    /// <summary>Counts one held frame; returns whether the hold ended.</summary>
    internal bool CountHoldFrame() => Hold.Count();

    /// <summary>Counts one scripted frame; once the script runs out, the final image starts holding.</summary>
    internal void CountPlaybackFrame()
    {
        if (Playback.Step())
            Hold = UpdateCountdown.Lasting(AttractDemoDefinitions.FinalImageHoldFrames);
    }

    /// <summary>Records how the scene ended.</summary>
    internal void Finish(bool cancelled) => Cancelled = cancelled;

    /// <summary>
    /// After a scene: an uncancelled demo continues with the set's next scene, and once the set is exhausted the set that
    /// follows it in <paramref name="playlist"/>, which starts over after its last, plays from its first scene.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="playlist"/> does not hold the set that played.</exception>
    internal void AdvanceAfterScene(IReadOnlyList<AttractDemoSet> playlist)
    {
        Ensure.NotNull(playlist);
        HasNextScene = false;
        if (Cancelled)
            return;
        HasNextScene = NextScene is not null;
        if (HasNextScene)
            return;
        Set = SetAfter(playlist, Set);
        ScenesBegun = 0;
    }

    /// <summary>The next scene of the set loads afresh.</summary>
    internal void LoadNextScene() => AwaitsScene = true;

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
}
