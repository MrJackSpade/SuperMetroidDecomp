using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Frontend;
using SuperMetroid.Verification.Harness;

/// <summary>Compares direct retail-room execution with an installed frontend that has no cartridge allocation.</summary>
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
        VerifyFrontendRomFreeDirectRoom(RoomHeaderDefinitions.Kraid, 900, 0);
        VerifyFrontendRomFreeDirectRoom(RoomHeaderDefinitions.Kraid, 900, ControllerButton.X);
        VerifyFrontendRomFreeDirectRoom(RoomHeaderDefinitions.Kraid, 1500, (ControllerButton.Right | ControllerButton.X));
    }

    /// <summary>Runs native and cartridge-free game instances with the same direct room and held input.</summary>
    private static void VerifyFrontendRomFreeDirectRoom(RoomHeaderDefinition room, int frameCount, ControllerButton heldInput)
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(RepositoryRomPath);
        var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        AssertEqual(false, MetadataShape.DeclaresProperty(installedMemory.GetType(), "Rom"),
            "direct-room fixture has no installed cartridge allocation");
        var native = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
        var installed = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        PrepareRomFreeBindings(installation)(installed, false);
        native.InitializeDirectRoomVerification();
        installed.InitializeDirectRoomVerification();
        var options = new SuperMetroidGameOptions { Invincibility = true };
        native.GameplayRuntimeForVerification.ApplyHostOptions(options.RuntimeOverrides(forAttractDemo: false));
        installed.GameplayRuntimeForVerification.ApplyHostOptions(options.RuntimeOverrides(forAttractDemo: false));
        VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { Room = room, RoomName = $"direct retail room {room.Name}, input ${heldInput:X}" }, frameCount, heldInput: heldInput);
    }

    /// <summary>Runs the focused cartridge-free direct-room verification command.</summary>
    [VerificationCommand("--rom-free-direct-room")]
    internal static int RomFreeDirectRoomCommand()
    {
        Suite(nameof(VerifyFrontendRomFreeDirectRoom), () => VerifyFrontendRomFreeDirectRoom());
        return 0;
    }
}
