using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Kraid's arm carries extra-property bit $0004, so $A0:9758 tests Samus against its
    /// extended-spritemap rectangles ($A0:9A5A), not its 48x48 header radius. The 13% movie's
    /// Samus touches the arm's frame $90FD there: $A7:9490 pushes her back, deals contact
    /// damage and makes physical slot four, the bottom lint, fire at once.
    /// </summary>
    private static void VerifyKraidArmSamusContact()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Kraid);
        RoomEnemySlot arm = runtime.Enemies.Slots[1];
        AssertEqual((ushort)0xe2ff, arm.EnemyDefinitionPointer, "slot one is Kraid's arm");

        // Native update 35724: the arm and Samus as they stand when EnemyMain tests contact.
        arm.XPosition = 0x00b0;
        arm.YPosition = 0x01df;
        arm.SpritemapPointer = 0x90fd;
        arm.Properties = 0x2800;
        SamusState samus = runtime.Samus!;
        samus.XPosition = 0x007f;
        samus.YPosition = 0x019a;
        samus.Kinematics.XRadius = 5;
        samus.Kinematics.YRadius = 19;
        samus.InvincibilityTimer = 0;
        samus.Kinematics.ExtraXDisplacement = 0;
        samus.Kinematics.ExtraYDisplacement = 0;
        // The header radius box (top $1AF) misses Samus's feet at $1AD; the arm frame does not.
        AssertTrue(!RadiusBoxesOverlapForArm(arm, samus), "the header radius alone does not reach Samus");

        // $A0:8EB6 builds this frame's interactive list from the native camera (0,$100).
        typeof(RoomEnemySystem).GetMethod("DetermineWhichEnemiesToProcess",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(runtime.Enemies, [(ushort)0, (ushort)0x100]);
        runtime.Enemies.ResolveOrdinarySamusContact(samus, 0, runtime.LevelData, onlyNativeEnemyIndex: 0x40);

        AssertEqual((ushort)4, samus.Kinematics.ExtraXDisplacement, "$A7:94A4 pushes Samus four pixels right");
        AssertEqual(unchecked((ushort)-8), samus.Kinematics.ExtraYDisplacement, "$A7:94AA pushes Samus eight pixels up");
        AssertEqual((ushort)KraidAiFunction.LintFire, runtime.Enemies.Slots[4].VariableA,
            "the arm touch makes the bottom lint fire");
        AssertTrue(samus.InvincibilityTimer != 0, "the arm's contact damage starts invincibility");
        Console.WriteLine("  Kraid arm Samus contact: the extended arm frame pushes Samus back and fires the bottom lint.");
    }

    /// <summary>
    /// Tests whether the arm's header-radius box strictly overlaps Samus's radius box on both axes.
    /// </summary>
    /// <param name="arm">Enemy slot providing the arm's center and header radii.</param>
    /// <param name="samus">Samus state providing her center and collision radii.</param>
    /// <returns><see langword="true"/> when both axis separations are less than their summed radii.</returns>
    private static bool RadiusBoxesOverlapForArm(RoomEnemySlot arm, SamusState samus) =>
        Math.Abs(arm.XPosition - samus.XPosition) < arm.XRadius + samus.Kinematics.XRadius &&
        Math.Abs(arm.YPosition - samus.YPosition) < arm.YRadius + samus.Kinematics.YRadius;
}
