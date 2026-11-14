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
    internal int ScenesBegun { get; set; }

    /// <summary>The scripted input frames left in the scene playing.</summary>
    internal int FramesRemaining { get; set; }

    /// <summary>The still-frame hold left after the scene's scripted input.</summary>
    internal int HoldFramesRemaining { get; private set; }

    /// <summary>The blank enemy-loading waits left before the scene becomes visible; none before its runtime exists (#1627).</summary>
    private int? loadFramesRemaining;

    /// <summary>Whether live controller input cancelled the scene.</summary>
    internal bool Cancelled { get; private set; }

    /// <summary>Whether another scene follows before returning to the title.</summary>
    internal bool HasNextScene { get; private set; }

    /// <summary>Whether the next scene's runtime has yet to be created.</summary>
    internal bool AwaitsScene => loadFramesRemaining is null;

    /// <summary>Whether the scene's scripted input has ended and its final image is holding.</summary>
    internal bool Holding => HoldFramesRemaining != 0;

    /// <summary>A soft reset returns to the first scene of the first set.</summary>
    internal void Reset()
    {
        Set = StockAttractDemoScenes.FirstSet;
        ScenesBegun = 0;
        loadFramesRemaining = null;
    }

    /// <summary>The title requests a demo: its set starts again from its first scene.</summary>
    internal void RequestFromTitle()
    {
        ScenesBegun = 0;
        loadFramesRemaining = null;
    }

    /// <summary>The scene the next demo plays, or null once every demonstration of the set has begun.</summary>
    /// <exception cref="InvalidOperationException">More demonstrations began than the set holds.</exception>
    internal AttractDemoScene? NextScene =>
        ScenesBegun < Set.Scenes.Count ? Set.Scenes[ScenesBegun]
        : ScenesBegun == Set.Scenes.Count ? null
        : throw new InvalidOperationException($"{ScenesBegun} demonstrations began in {Set}, which holds {Set.Scenes.Count}.");

    /// <summary>Starts <paramref name="scene"/>, the next scene, with its scripted input and enemy-loading waits ahead.</summary>
    internal void Begin(AttractDemoScene scene)
    {
        Ensure.NotNull(scene);
        FramesRemaining = scene.Duration;
        ScenesBegun++;
        HoldFramesRemaining = 0;
        Cancelled = false;
        loadFramesRemaining = AttractDemoDefinitions.EnemyGraphicsLoadFrames;
    }

    /// <summary>Counts one enemy-loading wait; returns whether it was the last.</summary>
    /// <exception cref="InvalidOperationException">No scene has begun loading.</exception>
    internal bool CountLoadFrame()
    {
        int remaining = (loadFramesRemaining ?? throw new InvalidOperationException("No attract demo scene is loading.")) - 1;
        loadFramesRemaining = remaining;
        return remaining == 0;
    }

    /// <summary>Counts one held frame; returns whether the hold ended.</summary>
    internal bool CountHoldFrame() => --HoldFramesRemaining == 0;

    /// <summary>Counts one scripted frame; once the script runs out, the final image starts holding.</summary>
    internal void CountPlaybackFrame()
    {
        if (--FramesRemaining <= 0)
            HoldFramesRemaining = AttractDemoDefinitions.FinalImageHoldFrames;
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
    internal void LoadNextScene() => loadFramesRemaining = null;

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
