using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Samus's same-frame fall after a dead Draygon releases her during enemy AI.</summary>
internal static class ReleasedSamusFallsOffDraygonVerification
{
    /// <summary>
    /// Touching a dead Draygon runs its death reaction ($A5:9618), which releases Samus to a
    /// standing pose during EnemyMain, after alpha. Beta then runs standing movement, and with
    /// no floor below `$91:E8B6` reads the standing pose's type and makes her fall. In the 13%
    /// movie Samus sparks into Draygon and falls; the port judged the walk-off by her airborne
    /// pose from alpha and left her standing in mid-air.
    /// </summary>
    internal static void VerifyReleasedSamusFallsOffDraygon()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xda60, camera: new Position16<RoomPixels>(256, 288));
        DraygonEnemyState draygon = (runtime.Enemies.DraygonBehavior.Draygon ?? throw new InvalidOperationException("Verification requires runtime.Enemies.DraygonBehavior.Draygon."));
        // Stage the end of the intro rather than replaying the dance; the fight's hitboxes
        // carry Draygon's own touch routine.
        (draygon ?? throw new InvalidOperationException("Verification requires draygon.")).FunctionTimer = NativeCountdown.Start(0x100);
        runtime.StepFrame(0);
        draygon.FunctionTimer = NativeCountdown.Start(DraygonIntroDanceDefinitions.DurationFrames);
        runtime.StepFrame(0);
        runtime.StepFrame(0);
        for (int frame = 0; frame < draygon.SwoopPathEntryCount + 2; frame++)
            runtime.StepFrame(0);
        draygon.Body.Health = 0;

        // A Screw Attack spin harms the enemy without hurting Samus, as the movie's spark does.
        SamusState samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.ScrewAttack;
        samus.Pose = SamusPoseId.ScrewAttackRightPose;
        // EnemyMain reads the contact-damage index the previous frame's spin movement set.
        samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.ScrewAttack;
        // Inside the upper body's touch boxes, in thin water with no floor below.
        samus.Position = samus.Position.WithWhole(new(unchecked((ushort)(draygon.Body.Position.X.Whole - 8)), unchecked((ushort)(draygon.Body.Position.Y.Whole - 64))));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        // EnemyMain processes only on-screen enemies; frame the swooping body.
        runtime.Room.LoadedCamera.SetPosition(new Position16<RoomPixels>(0, 0x0100));
        runtime.StepFrame(0);

        AssertEqual(DraygonAiFunction.Dying, draygon.Function, "touching the dead Draygon runs its death reaction");
        AssertEqual(SamusPoseId.FallingRightPose, samus.Pose,
            "the released Samus walks off thin water and falls in the same frame");
        Console.WriteLine("  Released Samus falls: a mid-frame Draygon release still detects the missing floor.");
    }

    /// <summary>Runs the focused Draygon release and fall verification command.</summary>
    [VerificationCommand("--released-samus-falls-off-draygon")]
    internal static int ReleasedSamusFallsOffDraygonCommand()
    {
        Suite(nameof(VerifyReleasedSamusFallsOffDraygon), () => VerifyReleasedSamusFallsOffDraygon());
        return 0;
    }
}
