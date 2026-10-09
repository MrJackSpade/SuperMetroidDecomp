using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Unpause command $0C ($90:F2A2) restores Samus's normal handlers whenever command six's
    /// station lock holds them. In the 13% movie Samus touches a recharge station while the
    /// pause fades out; native frees her when the menu closes, and the port kept her locked.
    /// </summary>
    private static void VerifyUnpauseStationLockRelease()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.LockIntoRefillStation();
        runtime.RunNmi(0, true);
        var pause = CreateRetailPauseFixture(bus, samus, runtime.System, AreaId.Crateria, 0, 0,
            gameplayVram: runtime.Vram);
        EnterPauseEquipment(pause);
        var game = new SuperMetroidGame(bus);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("pauseMenu", flags)!.SetValue(game, pause);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.PausedB);
        long sequence = 0;

        for (int frame = 0; frame < 8 && game.GameState == SuperMetroidGameState.PausedB; frame++)
            game.StepCaptured((ushort)SnesButton.Start, ++sequence, 1);
        AssertEqual(SuperMetroidGameState.UnpausingA, game.GameState, "Start closes the menu");
        AssertTrue(samus.InputLocked && samus.RefillStationLocked, "the station lock holds through the fade");
        while (game.GameState != SuperMetroidGameState.Unpausing)
            game.StepCaptured(0, ++sequence, 1);

        AssertTrue(!samus.InputLocked, "gameplay resume restores Samus's normal handlers");
        AssertTrue(!samus.RefillStationLocked, "and clears the station lock");
        Console.WriteLine("  Unpause station lock: command $0C frees a station-locked Samus.");
    }
}
