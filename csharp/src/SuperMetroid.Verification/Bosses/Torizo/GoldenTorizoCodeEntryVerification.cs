using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the Golden Torizo room-entry code and its queued beam artwork.</summary>
internal static class GoldenTorizoCodeEntryVerification
{
    /// <summary>Confirms the native item grant while the entering Wave artwork survives the next NMI.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyGoldenTorizoCodeEntry()
    {
        // Isolated SRAM; installed artwork is read-only and no player save is loaded.
        var installation = RepositoryInstallation.Installation;
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), new SuperMetroidGameOptions { Invincibility = true }, renderGameplayFrames: false);
        PrepareRomFreeBindings(installation)(game, false);
        game.InitializeDirectRoomVerification();
        var runtime = game.GameplayRuntimeForVerification;
        runtime.Player.IntroducedSamus.EquippedBeams = SamusBeamFlags.Wave;
        runtime.Player.IntroducedSamus.CollectedBeams = SamusBeamFlags.Wave;
        runtime.Controller1.Latch(GoldenTorizoCodeDefinitions.ControllerChord);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.GoldenTorizo);
        AssertEqual(GoldenTorizoCodeDefinitions.Beams, runtime.Player.IntroducedSamus.EquippedBeams, "GT code retains exact native beam grant");
        AssertEqual(GoldenTorizoCodeDefinitions.Items, runtime.Player.IntroducedSamus.EquippedItems, "GT code retains exact native equipment grant");
        AssertEqual(SamusBeamFlags.Wave, runtime.Artwork.LoadedBeams,
            "the room transition loaded the entering Wave beam before the code changed the beam word");
        for (int frame = 0; frame < 3; frame++) game.Step(0);
        AssertEqual(SamusBeamFlags.Wave, runtime.Artwork.LoadedBeams,
            "the projectile pages keep showing the entering Wave beam until the next beam update");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "GT room entry resumes gameplay after code");
        Console.WriteLine("Golden Torizo code: real room initialization and three continuation frames succeed with the native grant.");
    }

    /// <summary>Runs the focused room-entry and artwork-continuation check.</summary>
    [VerificationCommand("--golden-torizo-code-entry")]
    internal static int GoldenTorizoCodeEntryCommand()
    {
     VerifyGoldenTorizoCodeEntry(); return 0; 
    }
}
