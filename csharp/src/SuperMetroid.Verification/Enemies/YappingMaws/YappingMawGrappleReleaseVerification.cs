using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Enemies.YappingMaws;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

/// <summary>Checks the reported Yapping Maw release timing through production grapple and enemy updates.</summary>
internal static class YappingMawGrappleReleaseVerification
{
    /// <summary>Stages the Maw's final hold frames and confirms the deferred grapple drop, standing pose, and restored input control.</summary>
    internal static void VerifyYappingMawGrappleRelease()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual((ushort)0xc8c5, bus.ReadCartridgeWord(0x90f15b), "pinned cartridge command three selects Dropped");
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.RedBrinstarFirefleas, camera: new Position16<RoomPixels>(768, 0));
        var samus = runtime.Player.IntroducedSamus;
        samus.PoseId = SamusPoseId.GrappleSwingLeftPose;
        samus.Position = samus.Position.WithWhole(new(936, 172));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus, samus.PoseFrameNumbered(24));
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, samus.PoseFrameNumbered(24), timer: 3);
        samus.Grapple.Phase = GrapplePhase.ConnectedSwinging;
        // She swings from a point in the room, hanging straight down from it, as a connected swing does.
        samus.Grapple.Anchor = new Position16<RoomPixels>(936, 120);
        samus.Grapple.Angle = RotationAngle.HalfTurn;
        samus.Grapple.MirroredAngle = RotationAngle.HalfTurn;
        samus.EquippedItems |= SamusEquipmentFlags.GrappleBeam;
        samus.SelectedHudItem = HudItem.Grapple;
        samus.Control.LockOutPlayer();
        // Stage the last two frames of the actual Maw's held delay. The report's
        // (936,172) is this population mouth (936,192) plus native held offset (0,-20).
        var maw = runtime.Enemies.YappingMawStates.Single(state => state?.Origin.X == 936);
        (maw ?? throw new InvalidOperationException("Verification requires maw.")).Action = YappingMawAction.RetractedDelay;
        maw.HasGrabbedSamus = true;
        maw.RetractedDelayTimer = UpdateCountdown.Lasting(2);
        maw.HeldSamusOffset = new Offset16<RoomPixels>(0, -20);
        // The grapple handler runs in Samus's alpha pass, before EnemyMain. The Maw's
        // command three therefore queues $9B:C8C5 after this frame's swing, and the drop
        // completes in the next frame's handler, before the Maw's delay underflows.
        runtime.StepFrame((ControllerButton.Left | ControllerButton.X | ControllerButton.R));
        AssertEqual(GrapplePhase.Dropped, samus.Grapple.Phase, "command three queues the C8C5 drop while held");
        AssertTrue(samus.Control.InputLocked && maw.HasGrabbedSamus, "Maw retains input until native delay underflows");
        runtime.StepFrame((ControllerButton.B | ControllerButton.Left | ControllerButton.R));
        SamusPoseId heldPose = samus.Pose;
        var heldMovement = runtime.LatestGameplayFrame?.GrappleMovement;
        Console.WriteLine($"Maw drop frame: pose=${(int)SamusPoseNumbers.Number(heldPose):X2}, grapple={samus.Grapple.Phase}, locked={samus.Control.InputLocked}.");
        AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "the next grapple handler completes the queued drop");
        AssertEqual(SamusPoseId.FacingLeftNormalPose, heldPose, "command three drops swing to native standing pose while held");
        // Only the `$9B:C8C5` drop completion publishes a deferred drop pose.
        AssertTrue(heldMovement is { Phase: GrapplePhase.Inactive, PendingDropPose: SamusPoseId.FacingLeftNormalPose }, "Maw uses C8C5 deferred drop, not C856 cancellation");
        AssertTrue(!samus.Control.InputLocked && !maw.HasGrabbedSamus, "native timer expiry releases player control");
        AssertTrue(SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.Grappling, "released Samus has an ordinary movement body");
        runtime.StepFrame((ControllerButton.Left | ControllerButton.X | ControllerButton.R));
        AssertTrue(runtime.LatestGameplayFrame?.GrappleMovement is { Fired: true }, "released control accepts a new shoot edge as a new grapple");
        AssertEqual((ushort)936, samus.Position.X.Whole, "Maw release retains native held X");
        Console.WriteLine($"  Yapping Maw grapple release: pose=${(int)SamusPoseNumbers.Number(samus.Pose):X2}, position={samus.Position.X.Whole}/{samus.Position.Y.Whole}, input unlocked.");
    }

    /// <summary>Runs the retail-room Maw grapple release reproduction.</summary>
    [VerificationCommand("--yapping-maw-grapple-release")]
    internal static int YappingMawGrappleReleaseCommand()
    {
     VerifyYappingMawGrappleRelease(); return 0; 
    }
}
