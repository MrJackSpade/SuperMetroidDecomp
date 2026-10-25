using SuperMetroid.Core.Frontend;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
namespace SuperMetroid.Verification.Frontend.Ending;

using SuperMetroid.Verification.Harness;

/// <summary>Reproduces main-loop RNG timing through the ending's initial NMI waits.</summary>
internal static class EndingSetupNmiWaitsVerification
{
    /// <summary>
    /// Confirms state $27's first dispatch calls GenerateRandomNumber ($82:894F), then
    /// nine resumed NMI waits ($8B:D484-$D48C) skip main-loop RNG before scene setup.
    /// Later dispatches resume one RNG call each, matching the 100% movie.
    /// </summary>
    internal static void VerifyEndingSetupNmiWaits()
    {
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        game.Session.BeginZebesEscapeFade();
        while (game.GameState == SuperMetroidGameState.SamusEscapesFromZebes)
            game.Step(0);
        AssertEqual(SuperMetroidGameState.EndingAndCredits, game.GameState, "the fade hands off to state $27");

        var ending = game.Session.ScreenAs<EndingCreditsState>();
        var expected = new Bank80SystemState();
        expected.SetRandomNumber(game.DispatcherRandomNumber);
        for (int call = 1; call <= 11; call++)
        {
            bool mainLoop = call is 1 or > (EndingSequenceDefinitions.SetupFrameWaits + 1);
            if (mainLoop)
                expected.NextRandom();
            game.Step(0);
            AssertEqual(expected.RandomNumber, game.DispatcherRandomNumber,
                $"ending call {call} makes {(mainLoop ? "one main-loop" : "no")} RNG call");
            bool setupDone = call > EndingSequenceDefinitions.SetupFrameWaits;
            AssertEqual(setupDone, (ending ?? throw new InvalidOperationException("Verification requires ending.")).Stage.Phase != EndingCreditsPhase.SetupEscapeFromZebes,
                $"the scene setup runs on call {EndingSequenceDefinitions.SetupFrameWaits + 1}, not call {call}");
        }
        Console.WriteLine("Ending setup NMI waits: nine resumed calls skip the main-loop RNG and precede the scene setup.");
    }

    /// <summary>Runs the focused ending setup and RNG cadence regression.</summary>
    [VerificationCommand("--ending-setup-nmi-waits")]
    internal static int EndingSetupNmiWaitsCommand()
    {
        VerifyEndingSetupNmiWaits();
        return 0;
    }
}
