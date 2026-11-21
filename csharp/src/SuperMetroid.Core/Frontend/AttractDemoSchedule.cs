using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Where the attract demos are: the set the title plays, how far into it the next demonstration is, and how the scene last
/// begun ended. The scene's own loading, playback and hold countdowns are held by the demo step that counts them
/// (<see cref="AttractDemoGameSequence"/>).
/// </summary>
internal sealed class AttractDemoSchedule
{
    /// <summary>The demo set the title plays.</summary>
    private AttractDemoSet Set { get; set; } = StockAttractDemoScenes.FirstSet;

    /// <summary>
    /// How many of <see cref="Set"/>'s demonstrations have begun since the set last started from its first; the next
    /// one plays from this place in the set's list.
    /// </summary>
    internal int ScenesBegun { get; private set; }

    /// <summary>How the scene last begun ended: played to its end, or cancelled by a press (#2560).</summary>
    private AttractPlaybackEnd end;

    /// <summary>
    /// Where the attract mode goes after the scene (#2560): the set's next scene, the title at its title screen after a
    /// cancelled scene, or the title from its beginning once the set has played through.
    /// </summary>
    internal AttractDemoRoute RouteAfterScene { get; private set; }

    /// <summary>A soft reset returns to the first scene of the first set.</summary>
    internal void Reset()
    {
        Set = StockAttractDemoScenes.FirstSet;
        ScenesBegun = 0;
    }

    /// <summary>The title requests a demo: its set starts again from its first scene.</summary>
    internal void RequestFromTitle()
    {
        ScenesBegun = 0;
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
