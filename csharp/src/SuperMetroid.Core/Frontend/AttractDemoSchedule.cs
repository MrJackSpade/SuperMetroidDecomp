using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Which demonstrations the title plays (#2553): the set of <paramref name="playlist"/> it is at, which starts over after the
/// last, and how far into it the next demonstration is. Where a running scene stands is its step's own
/// (<see cref="AttractDemoStep"/>).
/// </summary>
/// <param name="playlist">The sets the title cycles through, in order, the first set first.</param>
internal sealed class AttractDemoSchedule(IReadOnlyList<AttractDemoSet> playlist)
{
    /// <summary>The sets the title cycles through, in order.</summary>
    private readonly IReadOnlyList<AttractDemoSet> playlist = Ensure.NotNull(playlist);

    /// <summary>The place in <see cref="playlist"/> of the set the title plays.</summary>
    private int setPlace;

    /// <summary>The demo set the title plays.</summary>
    internal AttractDemoSet Set => playlist[setPlace];

    /// <summary>
    /// How many of <see cref="Set"/>'s demonstrations have begun since the set last started from its first; the next
    /// one plays from this place in the set's list.
    /// </summary>
    internal int ScenesBegun { get; private set; }

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
        return new(BeginNext());
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
        return new AttractDemoRoute.TitleFromBeginning();
    }

    /// <summary>Begins the set's next scene and returns it.</summary>
    private AttractDemoScene BeginNext() => Set.Scenes[ScenesBegun++];
}
