using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Frontend.Pause;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

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
        foreach ((ControllerButton held, bool opensEquipment) in new[] { new VerifyPauseRemappedShoulderCase(ControllerButton.R, true), new VerifyPauseRemappedShoulderCase(ControllerButton.L, false) })
        {
            var random = new SharedRandom();
            var runtime = CreateRetailRuntimeFixture(random: random);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.Options.ControllerBindings = ControllerBindings.Default with { AimUp = ControllerButton.L, AimDown = ControllerButton.R };
            SamusState samus = runtime.Player.IntroducedSamus;
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = runtime.System, AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0), GameplayHud = runtime.AcceptedHud });
            var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), null, SuperMetroid.Core.Frontend.SuperMetroidGameHostServices.Default, random) { GameState = SuperMetroidGameState.PausedB };
            game.AdoptRuntime(runtime);
            game.Session.Show(pause);
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
