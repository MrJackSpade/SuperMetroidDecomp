using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using static RetailRuntimeFixtureVerification;
using static VerificationHelpersVerification;

/// <summary>Compares the reported shallow-water jump with a frame-by-frame native trace.</summary>
internal static class ShallowWaterJumpVerification
{
    /// <summary>The checked-in native trace for issue #1258, one row per jump frame.</summary>
    private const string ShallowWaterJumpNativeTracePath = "csharp/test-fixtures/issue-1258-water-jump/native.csv";

    /// <summary>Checks water contact, ceiling apex, and every recorded pose and motion row for issue #1258.</summary>
    internal static void VerifyShallowWaterJump()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var scenario = new ShallowWaterJumpScenario(CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true), bus);
        var samus = scenario.Samus;
        AssertEqual(447, samus.Kinematics.BottomPixel, "standing feet occupy the pixel below the water surface");
        AssertEqual(SamusLiquidMedium.Water, samus.LiquidPhysics.LiquidPhysicsType, "standing water contact is preserved");
        var (rows, apex) = scenario.Jump();
        AssertEqual(0x01530000u, apex, "cartridge jump reaches the room ceiling at Y=339");
        string[] native = File.ReadAllLines(ShallowWaterJumpNativeTracePath);
        AssertSequenceEqual(native, rows, "reported shallow-water jump matches native pose, position, velocity, radius and medium on every frame");
    }

    /// <summary>Runs the focused shallow-water jump comparison.</summary>
    [VerificationCommand("--shallow-water-jump")]
    internal static int ShallowWaterJumpCommand()
    {
     VerifyShallowWaterJump(); return 0; 
    }
}
