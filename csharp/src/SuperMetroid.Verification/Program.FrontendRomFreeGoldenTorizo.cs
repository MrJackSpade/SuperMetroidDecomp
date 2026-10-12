using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static Program;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static FrontendRomFreeVerification;
using static GoldenTorizoRoomSetupVerification;
using static RetailGameFixtureVerification;

/// <summary>Represents frontend rom free golden torizo verification.</summary>
internal static class FrontendRomFreeGoldenTorizoVerification
{
    /// <summary>
    /// Isolate the Golden Torizo room test without replaying the title and
    /// cinematic. The native reference owns the ROM; the installed instance
    /// has mutable RAM and installed artwork but no cartridge allocation.
    /// Both execute the same real-room wake sequence and a forced dodge turn.
    /// </summary>
    internal static void VerifyFrontendRomFreeGoldenTorizo(string sourceRom,
        int frameCount, ushort heldInput = 0)
    {
        string testDirectory = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        string root = Path.Combine(testDirectory,
            "golden-rom-free-" + Guid.NewGuid().ToString("N"));
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, root);
            var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
            var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            AssertEqual(false, installedMemory.GetType().GetProperty("Rom") is not null,
                "focused Golden Torizo fixture has no installed cartridge allocation");
            var guardedBus = NewFrontendCartridgeReadGuard(installedMemory);
            var native = CreateRetailGameFixture(nativeBus);
            var installed = new SuperMetroidGame(guardedBus);
            PrepareRomFreeBindings(installation)(installed, false);
            native.InitializeDirectRoomVerification();
            installed.InitializeDirectRoomVerification();
            VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GoldenTorizo, RoomName = $"focused Golden Torizo dodge and fall, input ${heldInput:X4}" }, frameCount: frameCount, setup: (nativeRoom, installedRoom) =>
                {
                    // Long neutral probes otherwise end at the frontend death
                    // screen, obscuring later enemy-list boundaries. Both
                    // references use the same player-facing host option.
                    var options = new SuperMetroidGameOptions { Invincibility = true };
                    nativeRoom.RuntimeForVerification!.ApplyHostOptions(options);
                    installedRoom.RuntimeForVerification!.ApplyHostOptions(options);
                    ForceGoldenTorizoLeftTurn(nativeRoom, installedRoom,
                        GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge);
                }, golden: new GoldenTorizoForcing { ForcedGoldenLeftTurnStart = GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge }, heldInput: heldInput);
        }
        finally
        {
            if (Path.GetDirectoryName(root) != testDirectory)
                throw new InvalidOperationException(
                    "Golden Torizo fixture cleanup target escaped test-temp.");
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Golden torizo rom free command.</summary>
    [VerificationCommand("--golden-torizo-rom-free")]
    internal static int GoldenTorizoRomFreeCommand()
    {
        string goldenTorizoRom = RepositoryRomPath;
        Suite(nameof(VerifyFrontendRomFreeGoldenTorizo), () => VerifyFrontendRomFreeGoldenTorizo(goldenTorizoRom, frameCount: 500));
        return 0;
    }
}
