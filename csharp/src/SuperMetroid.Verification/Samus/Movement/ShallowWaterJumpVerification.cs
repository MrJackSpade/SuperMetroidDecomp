using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.TestSupport.ScenarioFixtures;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Compares the reported shallow-water jump with a frame-by-frame native trace.</summary>
internal static class ShallowWaterJumpVerification
{
    /// <summary>The checked-in native trace for issue #1258, one row per jump frame.</summary>
    private const string ShallowWaterJumpNativeTracePath = "csharp/test-fixtures/issue-1258-water-jump/native.csv";

    /// <summary>Checks water contact, ceiling apex, and every recorded pose and motion row for issue #1258.</summary>
    internal static void VerifyShallowWaterJump()
    {
        var scenario = new ShallowWaterJumpScenario(CreateRetailRuntimeFixture(playerInvincibilityEnabled: true));
        var samus = scenario.Samus;
        AssertEqual(447, samus.Kinematics.BottomPixel.Y, "standing feet occupy the pixel below the water surface");
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
