using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;

namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that the pause menu reads physical buttons whatever the gameplay bindings are.</summary>
internal static class PauseRemappedShoulderVerification
{
    /// <summary>
    /// `$82:A50C` tests fixed bits of the delayed controller word: physical R opens the equipment page.
    /// The bindings rename only gameplay actions. The contributed lowc movie swaps aim up and down,
    /// and the port, which read the action-normalized latch, saw R as L and stayed on the map.
    /// </summary>
    internal static void VerifyPauseRemappedShoulder()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach ((SnesButton held, bool opensEquipment) in new[] { (SnesButton.R, true), (SnesButton.L, false) })
        {
            var runtime = CreateRetailRuntimeFixture(bus);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.Options.ControllerBindings = ControllerBindings.Default with { AimUp = SnesButton.L, AimDown = SnesButton.R };
            SamusState samus = runtime.Player.IntroducedSamus;
            runtime.NmiHandler.RunNmi(0, true);
            var pause = CreateRetailPauseFixture(bus, new PauseMenuEntry { Samus = samus, System = runtime.System, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), GameplayVram = runtime.Vram });
            var game = new SuperMetroidGame(bus) { GameState = SuperMetroidGameState.PausedB };
            game.Session.runtime = runtime;
            game.Session.pauseMenu = pause;
            long sequence = 0;
            for (int frame = 0; frame < 8 && !pause.IsPageTransitionActive; frame++)
                game.StepCaptured(held, ++sequence, 1);
            AssertEqual(opensEquipment, pause.IsPageTransitionActive,
                $"physical {held} with swapped aim bindings {(opensEquipment ? "opens" : "does not open")} the equipment page");
        }
        Console.WriteLine("  Pause remapped shoulder: physical R opens equipment, physical L does not, with aim bindings swapped.");
    }

    /// <summary>Runs the focused remapped-shoulder pause check from the verification command line.</summary>
    [VerificationCommand("--pause-remapped-shoulder")]
    internal static int PauseRemappedShoulderCommand()
    {
        Suite(nameof(VerifyPauseRemappedShoulder), () => VerifyPauseRemappedShoulder());
        return 0;
    }
}
