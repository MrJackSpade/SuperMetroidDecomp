using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static RetailRuntimeFixtureVerification;

internal static class KraidArmSamusContactVerification
{
    /// <summary>
    /// Kraid's arm carries extra-property bit $0004, so $A0:9758 tests Samus against its
    /// extended-spritemap rectangles ($A0:9A5A), not its 48x48 header radius. The 13% movie's
    /// Samus touches the arm's frame $90FD there: $A7:9490 pushes her back, deals contact
    /// damage and makes physical slot four, the bottom lint, fire at once.
    /// </summary>
    internal static void VerifyKraidArmSamusContact()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Kraid);
        RoomEnemySlot arm = runtime.Enemies.Slots[1];
        AssertEqual(EnemyDefinitionId.KraidArm, arm.EnemyDefinitionPointer, "slot one is Kraid's arm");

        // Native update 35724: the arm and Samus as they stand when EnemyMain tests contact.
        arm.Position = arm.Position.WithWhole(new(0x00b0, 0x01df));
        arm.SpritemapPointer = 0x90fd;
        arm.Properties = 0x2800;
        SamusState samus = runtime.Samus!;
        samus.Position = samus.Position.WithWhole(new(0x007f, 0x019a));
        samus.Kinematics.Radius = new(5, 19);
        samus.InvincibilityTimer = 0;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(0, 0));
        // The header radius box (top $1AF) misses Samus's feet at $1AD; the arm frame does not.
        AssertTrue(!RadiusBoxesOverlapForArm(arm, samus), "the header radius alone does not reach Samus");

        // $A0:8EB6 builds this frame's interactive list from the native camera (0,$100).
        typeof(RoomEnemySystem).GetMethod("DetermineWhichEnemiesToProcess",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [new Position16<RoomPixels>((ushort)0, (ushort)0x100)]);
        runtime.Enemies.ResolveOrdinarySamusContact(samus, 0, runtime.LevelData, onlyNativeEnemyIndex: 0x40);

        AssertEqual((ushort)4, samus.Kinematics.ExtraDisplacement.X.WholePixels, "$A7:94A4 pushes Samus four pixels right");
        AssertEqual((short)-8, samus.Kinematics.ExtraDisplacement.Y.WholePixels, "$A7:94AA pushes Samus eight pixels up");
        AssertEqual((ushort)KraidAiFunction.LintFire, runtime.Enemies.Slots[4].VariableA,
            "the arm touch makes the bottom lint fire");
        AssertTrue(samus.InvincibilityTimer != 0, "the arm's contact damage starts invincibility");
        Console.WriteLine("  Kraid arm Samus contact: the extended arm frame pushes Samus back and fires the bottom lint.");
    }

    private static bool RadiusBoxesOverlapForArm(RoomEnemySlot arm, SamusState samus) =>
        Math.Abs(arm.Position.X.Whole - samus.Position.X.Whole) < arm.Radius.X + samus.Kinematics.Radius.X &&
        Math.Abs(arm.Position.Y.Whole - samus.Position.Y.Whole) < arm.Radius.Y + samus.Kinematics.Radius.Y;

    [VerificationCommand("--kraid-arm-samus-contact")]
    internal static int KraidArmSamusContactCommand()
    {
        Suite(nameof(VerifyKraidArmSamusContact), () => VerifyKraidArmSamusContact());
        return 0;
    }
}
