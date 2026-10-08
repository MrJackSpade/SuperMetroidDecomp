using System.Reflection;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: GradduallyAccelerateTowardsPoint ($A9:F46B) reverses with SEC; SBC #8; SBC q;
    // SBC q. From a zero velocity the first SBC borrows, so the second takes one extra:
    // 0 - 8 - (3 + 1) - 3 = $FFF1. In the 100% movie the Shitroid's first rise after
    // draining the Sidehopper (Y $9C toward $68, divisor $10) moves its Y subpixel by -$F00.
    private static void VerifyShitroidGradualAcceleration()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.BigBoy);
        RoomEnemySlot shitroid = runtime.Enemies.Slots.First(slot =>
            slot.EnemyDefinitionPointer == RoomEnemySystem.ShitroidDefinition);

        // The movie's state when the drain ends, entering RiseAfterFeeding with no velocity.
        shitroid.VariableA = (ushort)ShitroidAiFunction.RiseAfterFeeding;
        shitroid.VariableB = 0;
        shitroid.VariableC = 0;
        shitroid.XPosition = 0x02d6;
        shitroid.YPosition = 0x009c;
        shitroid.YSubposition = 0x6b00;
        var runMain = typeof(RoomEnemySystem).GetMethod("RunShitroidMain", BindingFlags.Instance | BindingFlags.NonPublic)!;
        runMain.Invoke(runtime.Enemies, [shitroid, runtime.Samus, (ushort)0x0200, (ushort)0x0000, null]);

        AssertEqual((ushort)0xfff1, shitroid.VariableC, "the reversal step borrows once, reaching Y velocity $FFF1");
        AssertEqual((ushort)0x009c, shitroid.YPosition, "the rise stays within the same pixel");
        AssertEqual((ushort)0x5c00, shitroid.YSubposition, "the Y subpixel moves by -$F00");
        Console.WriteLine("Shitroid gradual acceleration: the native SBC borrow chain sets the first rise's velocity.");
    }
}
