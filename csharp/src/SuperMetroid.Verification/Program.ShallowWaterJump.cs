using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyShallowWaterJump()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var scenario = new ShallowWaterJumpScenario(CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true), bus);
        var samus = scenario.Samus;
        AssertEqual(447, samus.Kinematics.BottomPixel, "standing feet occupy the pixel below the water surface");
        AssertEqual(SamusLiquidPhysicsState.Water, samus.LiquidPhysics.LiquidPhysicsType, "standing water contact is preserved");
        var (rows, apex) = scenario.Jump();
        AssertEqual(0x01530000u, apex, "cartridge jump reaches the room ceiling at Y=339");
        string[] native = File.ReadAllLines(ShallowWaterJumpScenario.NativeTracePath);
        AssertSequenceEqual(native, rows, "reported shallow-water jump matches native pose, position, velocity, radius and medium on every frame");
    }
}
