using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Bosses.Torizo.GoldenTorizoRoomSetupVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Bosses.Torizo;
using SuperMetroid.Verification.Harness;

/// <summary>Compares Golden Torizo's room sequence on native and cartridge-free frontends.</summary>
internal static class FrontendRomFreeGoldenTorizoVerification
{
    /// <summary>
    /// Isolate the Golden Torizo room test without replaying the title and
    /// cinematic. The native reference owns the ROM; the installed instance
    /// has mutable RAM and installed artwork but no cartridge allocation.
    /// Both execute the same real-room wake sequence and a forced dodge turn.
    /// </summary>
    internal static void VerifyFrontendRomFreeGoldenTorizo(string sourceRom,
        int frameCount, ControllerButton heldInput = ControllerButton.None)
    {
        string testDirectory = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        string root = Path.Combine(testDirectory,
            "golden-rom-free-" + Guid.NewGuid().ToString("N"));
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, root);
            var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
            var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            AssertEqual(false, MetadataShape.DeclaresProperty(installedMemory.GetType(), "Rom"),
                "focused Golden Torizo fixture has no installed cartridge allocation");
            var guardedBus = NewFrontendCartridgeReadGuard(installedMemory);
            var native = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
            var installed = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
            PrepareRomFreeBindings(installation)(installed, false);
            native.InitializeDirectRoomVerification();
            installed.InitializeDirectRoomVerification();
            VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { Room = RoomHeaderDefinitions.GoldenTorizo, RoomName = $"focused Golden Torizo dodge and fall, input ${heldInput:X}" }, frameCount: frameCount, setup: (nativeRoom, installedRoom) =>
                {
                    // Long neutral probes otherwise end at the frontend death
                    // screen, obscuring later enemy-list boundaries. Both
                    // references use the same player-facing host option.
                    var options = new SuperMetroidGameOptions { Invincibility = true };
                    nativeRoom.GameplayRuntimeForVerification.ApplyHostOptions(options.RuntimeOverrides(forAttractDemo: false));
                    installedRoom.GameplayRuntimeForVerification.ApplyHostOptions(options.RuntimeOverrides(forAttractDemo: false));
                    ForceGoldenTorizoLeftTurn(nativeRoom, installedRoom,
                        TorizoBodyProgram.GoldenDodgeTurningLeft);
                }, golden: new GoldenTorizoForcing { ForcedGoldenLeftTurnStart = TorizoBodyProgram.GoldenDodgeTurningLeft }, heldInput: heldInput);
        }
        finally
        {
            if (Path.GetDirectoryName(root) != testDirectory)
                throw new InvalidOperationException(
                    "Golden Torizo fixture cleanup target escaped test-temp.");
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Runs the focused 500-frame Golden Torizo dodge and fall comparison.</summary>
    [VerificationCommand("--golden-torizo-rom-free")]
    internal static int GoldenTorizoRomFreeCommand()
    {
        string goldenTorizoRom = RepositoryRomPath;
        Suite(nameof(VerifyFrontendRomFreeGoldenTorizo), () => VerifyFrontendRomFreeGoldenTorizo(goldenTorizoRom, frameCount: 500));
        return 0;
    }
}
