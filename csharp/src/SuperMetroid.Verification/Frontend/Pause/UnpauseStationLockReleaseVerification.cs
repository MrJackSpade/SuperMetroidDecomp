using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks Samus's refill-station lock release when the pause menu closes.</summary>
internal static class UnpauseStationLockReleaseVerification
{
    /// <summary>
    /// Unpause command $0C ($90:F2A2) restores Samus's normal handlers whenever command six's
    /// station lock holds them. In the 13% movie Samus touches a recharge station while the
    /// pause fades out; native frees her when the menu closes, and the port kept her locked.
    /// </summary>
    internal static void VerifyUnpauseStationLockRelease()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var random = new CartridgeRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.ScriptLocks.LockIntoRefillStation();
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = runtime.System, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), GameplayVram = runtime.Vram });
        EnterPauseEquipment(pause);
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, renderGameplayFrames: true, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random);
        game.AdoptRuntime(runtime);
        game.Session.pauseMenu = pause;
        game.GameState = SuperMetroidGameState.PausedB;
        long sequence = 0;

        for (int frame = 0; frame < 8 && game.GameState == SuperMetroidGameState.PausedB; frame++)
            game.StepCaptured(SnesButton.Start, ++sequence, 1);
        AssertEqual(SuperMetroidGameState.UnpausingA, game.GameState, "Start closes the menu");
        AssertTrue(samus.InputLocked && samus.ScriptLocks.RefillStationLocked, "the station lock holds through the fade");
        while (game.GameState != SuperMetroidGameState.Unpausing)
            game.StepCaptured(0, ++sequence, 1);

        AssertTrue(!samus.InputLocked, "gameplay resume restores Samus's normal handlers");
        AssertTrue(!samus.ScriptLocks.RefillStationLocked, "and clears the station lock");
        Console.WriteLine("  Unpause station lock: command $0C frees a station-locked Samus.");
    }

    /// <summary>Runs the focused unpause station-lock check from the verification command line.</summary>
    [VerificationCommand("--unpause-station-lock-release")]
    internal static int UnpauseStationLockReleaseCommand()
    {
        Suite(nameof(VerifyUnpauseStationLockRelease), () => VerifyUnpauseStationLockRelease());
        return 0;
    }
}
