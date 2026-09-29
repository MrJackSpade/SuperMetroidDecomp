using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Isolate the Golden Torizo room test without replaying the title and
    /// cinematic. The native reference owns the ROM; the installed instance
    /// has mutable RAM and installed artwork but no cartridge allocation.
    /// Both execute the same real-room wake sequence and a forced dodge turn.
    /// </summary>
    private static void VerifyFrontendRomFreeGoldenTorizo(string sourceRom,
        int frameCount, ushort heldInput = 0)
    {
        string testDirectory = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        string root = Path.Combine(testDirectory,
            "golden-rom-free-" + Guid.NewGuid().ToString("N"));
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, root);
            var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(sourceRom);
            var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            AssertEqual(false, installedMemory.GetType().GetProperty("Rom") is not null,
                "focused Golden Torizo fixture has no installed cartridge allocation");
            var guardedBus = new FrontendCartridgeReadGuard(installedMemory, nativeBus);
            var native = new SuperMetroidGame(nativeBus);
            var installed = new SuperMetroidGame(guardedBus);
            PrepareRomFreeBindings(installation)(installed, false);
            native.InitializeDirectRoomVerification();
            installed.InitializeDirectRoomVerification();
            VerifyFrontendRomFreeRoom(native, installed,
                RoomHeaderPointers.GoldenTorizo,
                $"focused Golden Torizo dodge and fall, input ${heldInput:X4}",
                frameCount: frameCount,
                setup: (nativeRoom, installedRoom) =>
                {
                    // Long neutral probes otherwise end at the frontend death
                    // screen, obscuring later enemy-list boundaries. Both
                    // references use the same player-facing host option.
                    var options = new SuperMetroidGameOptions { Invincibility = true };
                    nativeRoom.RuntimeForVerification!.ApplyHostOptions(options);
                    installedRoom.RuntimeForVerification!.ApplyHostOptions(options);
                    ForceGoldenTorizoLeftTurn(nativeRoom, installedRoom,
                        GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge);
                },
                forcedGoldenLeftTurnStart:
                    GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge,
                expectGoldenLeftFootOrb: frameCount >= 500,
                heldInput: heldInput);
        }
        finally
        {
            if (Path.GetDirectoryName(root) != testDirectory)
                throw new InvalidOperationException(
                    "Golden Torizo fixture cleanup target escaped test-temp.");
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
