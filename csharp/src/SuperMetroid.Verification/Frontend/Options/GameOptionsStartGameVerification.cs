using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Options;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks how the options menu's START GAME begins each kind of save.</summary>
internal static class GameOptionsStartGameVerification
{
    /// <summary>
    /// `$82:EDBD` starts a save whose loading state is the main game (5) at once, so the area map
    /// follows on the next update; a new game fades the menu out first. The contributed lowc movie
    /// resets and continues its save: native reached the map while the port was still fading.
    /// </summary>
    internal static void VerifyGameOptionsStartGame()
    {
        foreach ((SaveLoadingGameState route, GameOptionsPhase afterPress) in new[]
        {
            new VerifyGameOptionsStartGameCase(SaveLoadingGameState.MainGame, GameOptionsPhase.StartGame),
            new VerifyGameOptionsStartGameCase(SaveLoadingGameState.OpeningCinematic, GameOptionsPhase.FadeOutToIntro),
        })
        {
            var options = new GameOptionsMenuState(mapPresentation: RetailPresentationFixture(), spriteSheets: MenuSheetFixtures.For(RetailPresentationFixture())) { LoadingGameState = route };
            for (int frame = 0; frame < 80 && options.Phase != GameOptionsPhase.Main; frame++)
                options.Step(ControllerButton.None);
            AssertEqual(GameOptionsPhase.Main, options.Phase, $"{route} save reaches the primary options page");
            AssertEqual(0, options.SelectedItem, "the cursor starts on START GAME");
            options.Step(ControllerButton.A);
            AssertEqual(afterPress, options.Phase, $"START GAME with a {route} save");
            if (route == SaveLoadingGameState.MainGame)
            {
                options.Step(ControllerButton.A);
                AssertTrue(options.IntroRequested, "a main-game save is dispatched on the next update");
            }
        }
        Console.WriteLine("Game options START GAME: a main-game save starts at once; a new game fades out first.");
    }

    /// <summary>Runs the options START GAME check from the verification command line.</summary>
    [VerificationCommand("--game-options-start-game")]
    internal static int GameOptionsStartGameCommand()
    {
        Suite(nameof(VerifyGameOptionsStartGame), () => VerifyGameOptionsStartGame());
        return 0;
    }
}
