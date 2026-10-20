using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Areas.Escape;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Frontend;

/// <summary>Checks the gunship takeoff fade while state $26 continues normal gameplay updates.</summary>
internal static class ZebesEscapeFadeVerification
{
    /// <summary>Confirms the $A2:AD0E takeoff keeps time running and hands off on the fifteenth fade call.</summary>
    /// <remarks>#1269: the earlier transition froze time and dimmed every second call, halting Samus's rise and RNG advancement in the 100% movie.</remarks>
    internal static void VerifyZebesEscapeFade()
    {
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var runtime = game.Session.GameplayRuntime;
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();

        game.Session.BeginZebesEscapeFade();
        AssertEqual(SuperMetroidGameState.SamusEscapesFromZebes, game.GameState, "the takeoff publishes state $26");
        AssertTrue(!runtime.Freeze.TimeIsFrozen, "state $26 keeps gameplay time running");

        int calls = 0;
        while (game.GameState == SuperMetroidGameState.SamusEscapesFromZebes)
        {
            game.Step(0);
            calls++;
            AssertTrue(calls <= 40, "the escape fade completes");
        }
        AssertEqual(15, calls, "HandleFadingOut with delay zero blanks on the fifteenth state-$26 call");
        Console.WriteLine("Zebes escape fade: state $26 runs unfrozen gameplay and hands off after 15 calls.");
    }

    /// <summary>Runs the focused gunship escape-fade check from the verification command line.</summary>
    [VerificationCommand("--zebes-escape-fade")]
    internal static int ZebesEscapeFadeCommand()
    {
        VerifyZebesEscapeFade();
        return 0;
    }
}
