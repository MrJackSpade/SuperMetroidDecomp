using SuperMetroid.Core.Enemies.Elevator;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that unpausing disarms elevator pseudo-door contact before gameplay resumes.</summary>
internal static class UnpauseElevatorFlagsVerification
{
    /// <summary>
    /// State $11's ResumeGameplay ($80:A149) reloads tiles through $82:E78C, which first clears
    /// ElevatorProperties ($0E16). In the 13% movie Samus pauses while riding an elevator down;
    /// with the word cleared, reaching the pseudo-door skips the 48-frame down-elevator wait.
    /// </summary>
    internal static void VerifyUnpauseElevatorFlags()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.LandingSite);
        var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
        game.Session.runtime = runtime;
        game.Session.lastAudioRoomState = runtime.Room.LoadedRoom.State;

        runtime.Enemies.ElevatorBehavior.PublishElevatorDoorContact();
        AssertEqual(ElevatorJourneyFlag.DoorContact, runtime.Enemies.ElevatorBehavior.ElevatorFlags, "the elevator is armed before the pause");
        game.GameState = SuperMetroidGameState.UnpausingB;
        game.Step(0);

        AssertEqual(SuperMetroidGameState.Unpausing, game.GameState, "state $11 hands off to the brightening state");
        AssertEqual(ElevatorJourneyFlag.None, runtime.Enemies.ElevatorBehavior.ElevatorFlags, "ResumeGameplay clears $0E16");
        Console.WriteLine("  Unpause elevator flags: resuming gameplay disarms the elevator's pseudo-door.");
    }

    /// <summary>Runs the focused unpause elevator flag verification command.</summary>
    [VerificationCommand("--unpause-elevator-flags")]
    internal static int UnpauseElevatorFlagsCommand()
    {
        Suite(nameof(VerifyUnpauseElevatorFlags), () => VerifyUnpauseElevatorFlags());
        return 0;
    }
}
