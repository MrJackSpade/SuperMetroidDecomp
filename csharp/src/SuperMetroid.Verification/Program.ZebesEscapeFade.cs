using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    // #1269: gunship function $A2:AD0E only writes game state $26 and zeroes the fade delay
    // and counter. State $26 runs full gameplay before HandleFadingOut, which with delay zero
    // dims one step per call and blanks on the fifteenth, handing off on that same call. The
    // port froze time on the transition and dimmed every second call, so in the 100% movie
    // Samus stopped rising into the gunship and the RNG stopped advancing.
    private static void VerifyZebesEscapeFade()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", flags)!.Invoke(game, [false]);
        var runtime = (SuperMetroidRuntime)typeof(SuperMetroidGame).GetField("runtime", flags)!.GetValue(game)!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();

        typeof(SuperMetroidGame).GetMethod("BeginZebesEscapeFade", flags)!.Invoke(game, null);
        AssertEqual(SuperMetroidGameState.SamusEscapesFromZebes, game.GameState, "the takeoff publishes state $26");
        AssertTrue(!runtime.TimeIsFrozen, "state $26 keeps gameplay time running");

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
}
