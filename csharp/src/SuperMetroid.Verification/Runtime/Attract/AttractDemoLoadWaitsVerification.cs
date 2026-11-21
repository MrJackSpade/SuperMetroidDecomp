using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Saves;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>
/// Checks an attract demo's loading waits (#1627) as the game runs them: the demo's first update creates its scene's
/// runtime and loads the scene behind a black screen, which stays loading through each of its enemy-setup waits
/// (<see cref="GameLoadingDefinitions.EnemySetupWaits"/>) but the last, which starts the scene.
/// </summary>
internal static class AttractDemoLoadWaitsVerification
{
    /// <summary>Runs one demo scene's load from the title's request to its start.</summary>
    internal static void VerifyAttractDemoLoadWaits()
    {
        var game = CreateRetailGameFixture(new SaveSlotStore(InstalledFixtures.Maps));
        game.Session.Demo.RequestFromTitle();
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
    }
}
