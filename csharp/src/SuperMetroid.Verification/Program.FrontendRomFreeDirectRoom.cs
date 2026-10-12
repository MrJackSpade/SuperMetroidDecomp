using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static FrontendRomFreeVerification;
using static RepositoryInputsVerification;
using static RetailGameFixtureVerification;

/// <summary>Represents frontend rom free direct room verification.</summary>
internal static class FrontendRomFreeDirectRoomVerification
{
    /// <summary>
    /// Compare one retail room without first replaying the opening cinematic or
    /// deserializing a version-sensitive debugger graph. The installed instance
    /// starts with no cartridge allocation, so any uncompiled source read fails.
    /// </summary>
    internal static void VerifyFrontendRomFreeDirectRoom()
    {
        // Kraid's room, neutral and firing, then held Right+fire: the moving shot reaches the
        // arm's extended hitbox walker, which once read $A7:9127 from the cartridge.
        VerifyFrontendRomFreeDirectRoom(RoomHeaderPointers.Kraid, 900, 0);
        VerifyFrontendRomFreeDirectRoom(RoomHeaderPointers.Kraid, 900, (ushort)SnesButton.X);
        VerifyFrontendRomFreeDirectRoom(RoomHeaderPointers.Kraid, 1500, (ushort)(SnesButton.Right | SnesButton.X));
    }

    /// <summary>Verifies frontend rom free direct room.</summary>
    private static void VerifyFrontendRomFreeDirectRoom(ushort roomPointer, int frameCount, ushort heldInput)
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(RepositoryRomPath);
        var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        AssertEqual(false, installedMemory.GetType().GetProperty("Rom") is not null,
            "direct-room fixture has no installed cartridge allocation");
        var native = CreateRetailGameFixture(nativeBus);
        var installed = new SuperMetroidGame(
            NewFrontendCartridgeReadGuard(installedMemory));
        PrepareRomFreeBindings(installation)(installed, false);
        native.InitializeDirectRoomVerification();
        installed.InitializeDirectRoomVerification();
        var options = new SuperMetroidGameOptions { Invincibility = true };
        native.RuntimeForVerification!.ApplyHostOptions(options);
        installed.RuntimeForVerification!.ApplyHostOptions(options);
        VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = roomPointer, RoomName = $"direct retail room $8F:{roomPointer:X4}, input ${heldInput:X4}" }, frameCount, heldInput: heldInput);
    }

    /// <summary>Rom free direct room command.</summary>
    [VerificationCommand("--rom-free-direct-room")]
    internal static int RomFreeDirectRoomCommand()
    {
        Suite(nameof(VerifyFrontendRomFreeDirectRoom), () => VerifyFrontendRomFreeDirectRoom());
        return 0;
    }
}
