using System.Globalization;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Compare one retail room without first replaying the opening cinematic or
    /// deserializing a version-sensitive debugger graph. The installed instance
    /// starts with no cartridge allocation, so any uncompiled source read fails.
    /// </summary>
    private static void VerifyFrontendRomFreeDirectRoom(string sourceRom,
        string roomPointerText, string frameCountText, string? heldInputText = null)
    {
        if (!ushort.TryParse(roomPointerText.TrimStart('$'), NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out ushort roomPointer) ||
            !RoomHeaderDefinitions.All.Any(room => room.Pointer == roomPointer))
            throw new ArgumentOutOfRangeException(nameof(roomPointerText),
                "Direct-room comparison requires a retail hexadecimal room pointer.");
        if (!int.TryParse(frameCountText, NumberStyles.None,
                CultureInfo.InvariantCulture, out int frameCount) ||
            frameCount is < 1 or > 5000)
            throw new ArgumentOutOfRangeException(nameof(frameCountText),
                "Direct-room comparison requires 1 through 5000 frames.");
        ushort heldInput = 0;
        if (heldInputText is not null &&
            !ushort.TryParse(heldInputText.TrimStart('$'), NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out heldInput))
            throw new ArgumentOutOfRangeException(nameof(heldInputText),
                "Held SNES input must be a hexadecimal 16-bit word.");

        string testDirectory = Path.GetFullPath(Path.Combine("csharp", "test-temp"));
        string installationRoot = Path.Combine(testDirectory,
            "rom-free-direct-" + Guid.NewGuid().ToString("N"));
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, installationRoot);
            var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(sourceRom);
            var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            AssertEqual(false, installedMemory.GetType().GetProperty("Rom") is not null,
                "direct-room fixture has no installed cartridge allocation");
            var native = new SuperMetroidGame(nativeBus);
            var installed = new SuperMetroidGame(
                new FrontendCartridgeReadGuard(installedMemory, nativeBus));
            PrepareRomFreeBindings(installation)(installed, false);
            native.InitializeDirectRoomVerification();
            installed.InitializeDirectRoomVerification();
            var options = new SuperMetroidGameOptions { Invincibility = true };
            native.RuntimeForVerification!.ApplyHostOptions(options);
            installed.RuntimeForVerification!.ApplyHostOptions(options);
            VerifyFrontendRomFreeRoom(native, installed, roomPointer,
                $"direct retail room $8F:{roomPointer:X4}, input ${heldInput:X4}",
                frameCount, heldInput: heldInput);
        }
        finally
        {
            if (Path.GetDirectoryName(installationRoot) != testDirectory)
                throw new InvalidOperationException(
                    "Direct-room fixture cleanup target escaped test-temp.");
            if (Directory.Exists(installationRoot))
                Directory.Delete(installationRoot, recursive: true);
        }
    }
}
