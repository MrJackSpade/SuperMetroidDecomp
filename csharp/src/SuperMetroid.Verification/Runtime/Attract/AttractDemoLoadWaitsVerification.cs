using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime.Attract;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>
/// Checks the attract demo schedule (#1627, #2553): a title request loads the set's first scene; each completed scene routes
/// to the set's next, in order, until the set has played through, which returns the title from its beginning with the
/// playlist's next set to come; a cancelled scene returns the title at its title screen; a soft reset returns to the first
/// set.
/// </summary>
internal static class AttractDemoLoadWaitsVerification
{
    /// <summary>Routes the ordinary playlist's first set through, a cancellation and a reset.</summary>
    internal static void VerifyAttractDemoLoadWaits()
    {
        IReadOnlyList<AttractDemoSet> playlist = StockAttractDemoScenes.Playlist(completionSetUnlocked: false);
        var schedule = new AttractDemoSchedule(playlist);
        AttractDemoSet first = playlist[0];
        AssertEqual(first.Scenes[0], schedule.RequestFromTitle().Scene, "a title request loads the first set's first scene");
        for (int scene = 1; scene < first.Scenes.Count; scene++)
        {
            AttractDemoRoute route = schedule.RouteAfterScene(AttractPlaybackEnd.Completed);
            AssertTrue(route is AttractDemoRoute.NextScene next && next.Scene == first.Scenes[scene], $"a completed scene routes to the set's scene {scene}");
        }
        AssertTrue(schedule.RouteAfterScene(AttractPlaybackEnd.Completed) is AttractDemoRoute.TitleFromBeginning, "a set played through returns the title from its beginning");
        AssertEqual(playlist[1], schedule.Set, "the playlist's next set plays next");
        AssertEqual(playlist[1].Scenes[0], schedule.RequestFromTitle().Scene, "the next title request loads that set's first scene");
        AssertTrue(schedule.RouteAfterScene(AttractPlaybackEnd.Cancelled) is AttractDemoRoute.TitleAtItsScreen, "a cancelled scene returns the title at its title screen");
        schedule.Reset();
        AssertEqual(first.Scenes[0], schedule.RequestFromTitle().Scene, "a soft reset returns to the first set's first scene");
    }
}
