using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Touching a dead Draygon runs its death reaction ($A5:9618), which releases Samus to a
    /// standing pose during EnemyMain, after alpha. Beta then runs standing movement, and with
    /// no floor below `$91:E8B6` reads the standing pose's type and makes her fall. In the 13%
    /// movie Samus sparks into Draygon and falls; the port judged the walk-off by her airborne
    /// pose from alpha and left her standing in mid-air.
    /// </summary>
    private static void VerifyReleasedSamusFallsOffDraygon()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xda60, cameraX: 256, cameraY: 288);
        DraygonEnemyState draygon = runtime.Enemies.Draygon!;
        // Stage the end of the intro rather than replaying the dance; the fight's hitboxes
        // carry Draygon's own touch routine.
        draygon.FunctionTimer = 0x100;
        runtime.StepFrame(0);
        draygon.FunctionTimer = DraygonIntroDanceDefinitions.DurationFrames;
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        for (int frame = 0; frame < draygon.SwoopPathEntryCount + 2; frame++)
            runtime.StepFrame(0);
        draygon.Body.Health = 0;

        // A Screw Attack spin harms the enemy without hurting Samus, as the movie's spark does.
        SamusState samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.ScrewAttack;
        samus.Pose = SamusPoseIds.ScrewAttackRightPose;
        // EnemyMain reads the contact-damage index the previous frame's spin movement set.
        samus.HorizontalSpeed.ContactDamageIndex = 3;
        // Inside the upper body's touch boxes, in thin water with no floor below.
        samus.XPosition = unchecked((ushort)(draygon.Body.XPosition - 8));
        samus.YPosition = unchecked((ushort)(draygon.Body.YPosition - 64));
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        // EnemyMain processes only on-screen enemies; frame the swooping body.
        runtime.Camera!.SetPosition(0, 0x0100);
        runtime.StepFrame(0);

        AssertEqual(DraygonAiFunction.Dying, draygon.Function, "touching the dead Draygon runs its death reaction");
        AssertEqual(SamusPoseIds.FallingRightPose, samus.Pose,
            "the released Samus walks off thin water and falls in the same frame");
        Console.WriteLine("  Released Samus falls: a mid-frame Draygon release still detects the missing floor.");
    }
}
