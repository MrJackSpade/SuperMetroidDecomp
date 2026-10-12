using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailGameFixtureVerification;
using static RetailRuntimeFixtureVerification;

/// <summary>Represents unpause elevator flags verification.</summary>
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
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.Room.ActiveRoom!.State.Pointer);

        runtime.Enemies.ElevatorBehavior.PublishElevatorDoorContact();
        AssertEqual((ushort)1, runtime.Enemies.ElevatorBehavior.ElevatorFlags, "the elevator is armed before the pause");
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.UnpausingB);
        game.Step(0);

        AssertEqual(SuperMetroidGameState.Unpausing, game.GameState, "state $11 hands off to the brightening state");
        AssertEqual((ushort)0, runtime.Enemies.ElevatorBehavior.ElevatorFlags, "ResumeGameplay clears $0E16");
        Console.WriteLine("  Unpause elevator flags: resuming gameplay disarms the elevator's pseudo-door.");
    }

    /// <summary>unpause elevator flags command.</summary>
    [VerificationCommand("--unpause-elevator-flags")]
    internal static int UnpauseElevatorFlagsCommand()
    {
        Suite(nameof(VerifyUnpauseElevatorFlags), () => VerifyUnpauseElevatorFlags());
        return 0;
    }
}
