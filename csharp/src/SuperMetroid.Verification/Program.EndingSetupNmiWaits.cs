using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: MainGameLoop calls GenerateRandomNumber ($82:894F) before every state dispatch.
    // State $27's first call enters CinematicFunction_Ending_Setup, which makes nine NMI
    // waits ($8B:D484-$D48C) before its scene setup, so calls two to ten resume inside it and
    // make no main-loop RNG call; every later call makes one. The port never advanced the RNG
    // in the ending and ran the setup at once, so in the 100% movie the RNG fell behind.
    private static void VerifyEndingSetupNmiWaits()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", flags)!.Invoke(game, [false]);
        var runtime = (SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime", flags)!.GetValue(game)!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        typeof(SuperMetroidGame).GetMethod("BeginZebesEscapeFade", flags)!.Invoke(game, null);
        while (game.GameState == SuperMetroidGameState.SamusEscapesFromZebes)
            game.Step(0);
        AssertEqual(SuperMetroidGameState.EndingAndCredits, game.GameState, "the fade hands off to state $27");

        var ending = (EndingCreditsState)typeof(SuperMetroidGame).GetField("endingCredits", flags)!.GetValue(game)!;
        var expected = new Bank80SystemState();
        expected.SetRandomNumber(game.DispatcherRandomNumber);
        for (int call = 1; call <= 11; call++)
        {
            bool mainLoop = call is 1 or > (EndingCreditsRomData.SetupNmiWaits + 1);
            if (mainLoop)
                expected.NextRandom();
            game.Step(0);
            AssertEqual(expected.RandomNumber, game.DispatcherRandomNumber,
                $"ending call {call} makes {(mainLoop ? "one main-loop" : "no")} RNG call");
            bool setupDone = call > EndingCreditsRomData.SetupNmiWaits;
            AssertEqual(setupDone, ending.Phase != EndingCreditsPhase.SetupEscapeFromZebes,
                $"the scene setup runs on call {EndingCreditsRomData.SetupNmiWaits + 1}, not call {call}");
        }
        Console.WriteLine("Ending setup NMI waits: nine resumed calls skip the main-loop RNG and precede the scene setup.");
    }
}
