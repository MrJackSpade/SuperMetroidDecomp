using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime.Attract;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>
/// Checks an attract demo's loading waits (#1627) as the game runs them: the demo's first update creates its scene's
/// runtime and loads the scene behind a black screen, which stays loading through each of its enemy-setup waits
//// (<see cref="GameLoadingDefinitions.EnemySetupWaits"/>) but the last, which starts the scene. The schedule the title
/// plays from (#2553): a title request loads the set's first scene; each completed scene routes to the set's next, in
/// order, until the set has played through, which returns the title from its beginning with the playlist's next set to
/// come; a cancelled scene returns the title at its title screen; a soft reset returns to the first set.
/// </summary>
internal static class AttractDemoLoadWaitsVerification
{
    /// <summary>Runs one demo scene's load from the title's request to its start, then routes the ordinary playlist's first set through, a cancellation and a reset.</summary>
    internal static void VerifyAttractDemoLoadWaits()
    {
        var game = CreateRetailGameFixture(new SaveSlotStore(InstalledFixtures.Maps));
        game.Session.Enter(AttractDemoStep.Loading);
        AssertTrue(game.Session.Runtime is null, "the demo awaits its scene before any runtime exists");
        game.Step(ControllerButton.None);
        AssertTrue(game.Session.Runtime is not null && game.IsAt(AttractDemoStep.Loading),
            "the demo's first update creates its scene's runtime and keeps loading it");
        for (int wait = 1; wait < GameLoadingDefinitions.EnemySetupWaits; wait++)
        {
            game.Step(ControllerButton.None);
            AssertTrue(game.IsAt(AttractDemoStep.Loading), $"enemy-setup wait {wait} is not the last");
        }
        game.Step(ControllerButton.None);
        AssertTrue(game.IsAt(AttractDemoStep.Starting), "the last enemy-setup wait starts the scene");

        IReadOnlyList<AttractDemoSet> playlist = StockAttractDemoScenes.Playlist(completionSetUnlocked: false);
        var schedule = new AttractDemoSchedule(playlist);
        AttractDemoSet first = playlist[0];
        AssertEqual(first.Scenes[0], schedule.RequestFromTitle(), "a title request loads the first set's first scene");
        for (int scene = 1; scene < first.Scenes.Count; scene++)
        {
            AttractDemoRoute route = schedule.RouteAfterScene(AttractPlaybackEnd.Completed);
            AssertTrue(route is AttractDemoRoute.NextScene next && next.Scene == first.Scenes[scene], $"a completed scene routes to the set's scene {scene}");
        }
        AssertTrue(schedule.RouteAfterScene(AttractPlaybackEnd.Completed) is AttractDemoRoute.TitleFromBeginning, "a set played through returns the title from its beginning");
        AssertEqual(playlist[1].Scenes[0], schedule.RequestFromTitle(), "the playlist's next set plays next: the next title request loads its first scene");
        AssertTrue(schedule.RouteAfterScene(AttractPlaybackEnd.Cancelled) is AttractDemoRoute.TitleAtItsScreen, "a cancelled scene returns the title at its title screen");
        schedule.Reset();
        AssertEqual(first.Scenes[0], schedule.RequestFromTitle(), "a soft reset returns to the first set's first scene");
    }
}
