using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.Kraid;

using SuperMetroid.Verification.Harness;

/// <summary>Checks Kraid arm contact through extended-spritemap rectangles instead of its header radius.</summary>
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
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Kraid);
        RoomEnemySlot arm = runtime.Enemies.SlotTable.Slots[1];
        AssertEqual(EnemyDefinitionId.KraidArm, arm.DefinitionId, "slot one is Kraid's arm");

        // Native update 35724: the arm and Samus as they stand when EnemyMain tests contact.
        arm.Position = arm.Position.WithWhole(new(0x00b0, 0x01df));
        arm.SpritemapPointer = 0x90fd;
        arm.Properties = EnemyPropertyWord.FromWord(0x2800);
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.Position = samus.Position.WithWhole(new(0x007f, 0x019a));
        samus.Kinematics.Radius = new(5, 19);
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(0, 0));
        // The header radius box (top $1AF) misses Samus's feet at $1AD; the arm frame does not.
        AssertTrue(!RadiusBoxesOverlapForArm(arm, samus), "the header radius alone does not reach Samus");

        // $A0:8EB6 builds this frame's interactive list from the native camera (0,$100).
        runtime.Enemies.ProcessingWindow.DetermineWhichEnemiesToProcess(new Position16<RoomPixels>(0, 0x100));
        runtime.Enemies.Collision.ResolveOrdinarySamusContact(samus, 0, runtime.Room.LevelData, onlyEnemy: runtime.Enemies.SlotTable.Slots[1]);

        AssertEqual(4, samus.Kinematics.ExtraDisplacement.X.WholePixels, "$A7:94A4 pushes Samus four pixels right");
        AssertEqual((short)-8, samus.Kinematics.ExtraDisplacement.Y.WholePixels, "$A7:94AA pushes Samus eight pixels up");
        AssertEqual(KraidAiFunction.LintFire, (runtime.Enemies.KraidBehavior.Access.Kraid ?? throw new InvalidOperationException("The Kraid room has no Kraid state.")).Parts[4].Function,
            "the arm touch makes the bottom lint fire");
        AssertTrue(!samus.InvincibilityTimer.IsZero, "the arm's contact damage starts invincibility");
        Console.WriteLine("  Kraid arm Samus contact: the extended arm frame pushes Samus back and fires the bottom lint.");
    }

    /// <summary>Computes the smaller header-radius overlap used as a negative control for frame-$90FD contact.</summary>
    private static bool RadiusBoxesOverlapForArm(RoomEnemySlot arm, SamusState samus) =>
        Math.Abs(arm.Position.X.Whole - samus.Position.X.Whole) < arm.Radius.X + samus.Kinematics.Radius.X &&
        Math.Abs(arm.Position.Y.Whole - samus.Position.Y.Whole) < arm.Radius.Y + samus.Kinematics.Radius.Y;

    /// <summary>Runs the focused 13% movie Kraid arm collision and bottom-lint response check.</summary>
    [VerificationCommand("--kraid-arm-samus-contact")]
    internal static int KraidArmSamusContactCommand()
    {
        Suite(nameof(VerifyKraidArmSamusContact), () => VerifyKraidArmSamusContact());
        return 0;
    }
}
