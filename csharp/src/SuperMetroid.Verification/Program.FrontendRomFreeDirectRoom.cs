using SuperMetroid.Core.Input;
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
    private static void VerifyFrontendRomFreeDirectRoom()
    {
        // Kraid's room, neutral and firing, then held Right+fire: the moving shot reaches the
        // arm's extended hitbox walker, which once read $A7:9127 from the cartridge.
        VerifyFrontendRomFreeDirectRoom(RoomHeaderPointers.Kraid, 900, 0);
        VerifyFrontendRomFreeDirectRoom(RoomHeaderPointers.Kraid, 900, (ushort)SnesButton.X);
        VerifyFrontendRomFreeDirectRoom(RoomHeaderPointers.Kraid, 1500, (ushort)(SnesButton.Right | SnesButton.X));
    }

    private static void VerifyFrontendRomFreeDirectRoom(ushort roomPointer, int frameCount, ushort heldInput)
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(RepositoryRomPath);
        var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        AssertEqual(false, installedMemory.GetType().GetProperty("Rom") is not null,
            "direct-room fixture has no installed cartridge allocation");
        var native = CreateRetailGameFixture(nativeBus);
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
}
