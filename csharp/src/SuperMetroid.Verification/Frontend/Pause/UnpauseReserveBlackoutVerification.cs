using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Frontend.Pause;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks the zero-health auto-reserve transition during unpause and the black screen retained for a later pause.</summary>
internal static class UnpauseReserveBlackoutVerification
{
    /// <summary>
    /// State $12 ($82:93A1) runs the whole state-eight routine, so a Samus unpaused at zero
    /// health with auto reserve enters the refill (state $1B) at once, leaving the screen black
    /// ($51 never finished fading in). A later pause darkens from that black register: state
    /// $0C ends on its first frame. The 13% movie does both after switching reserve to auto.
    /// </summary>
    internal static void VerifyUnpauseReserveBlackout()
    {
        (SuperMetroidGame Game, SuperMetroidRuntime Runtime) Create()
        {
            var random = new CartridgeRandom();
            SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(random: random);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
            runtime.Player.IntroducedSamus.Control.Unlock();
            var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false, random: random);
            game.AdoptRuntime(runtime);
            game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
            return (game, runtime);
        }

        var (unpausing, unpausingRuntime) = Create();
        SamusState samus = unpausingRuntime.Player.IntroducedSamus;
        samus.Health = 0;
        samus.MaxReserveEnergy = samus.ReserveEnergy = 100;
        samus.ReserveTankMode = ReserveTankMode.Auto;
        unpausing.Session.PauseBrightness = 0;
        unpausing.GameState = SuperMetroidGameState.Unpausing;
        unpausing.Step(0);
        AssertEqual(SuperMetroidGameState.ReserveTanksAuto, unpausing.GameState,
            "state $12's gameplay pass routes zero health into the auto reserve refill");
        AssertEqual(0, unpausing.Session.PauseBrightness, "the interrupted fade-in leaves $51 black");

        // Once gameplay resumes the screen is still black; a pause darkens from there.
        var (pausing, _) = Create();
        pausing.GameState = SuperMetroidGameState.MainGameplay;
        pausing.Session.PauseBrightness = 0;
        pausing.Step(0);
        pausing.Step(SnesButton.Start);
        AssertEqual(SuperMetroidGameState.PausingDarkening, pausing.GameState, "Start pauses");
        pausing.Step(0);
        AssertEqual(SuperMetroidGameState.Pausing, pausing.GameState,
            "darkening from a black screen ends on its first frame");
        Console.WriteLine("  Unpause reserve blackout: the refill starts in state $12 and the next pause darkens from black.");
    }

    /// <summary>Runs the reserve blackout reproduction through the verification command host.</summary>
    [VerificationCommand("--unpause-reserve-blackout")]
    internal static int UnpauseReserveBlackoutCommand()
    {
        Suite(nameof(VerifyUnpauseReserveBlackout), () => VerifyUnpauseReserveBlackout());
        return 0;
    }
}
