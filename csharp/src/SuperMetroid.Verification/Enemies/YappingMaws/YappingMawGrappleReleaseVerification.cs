using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
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
    /// <summary>
    /// Samus swings on her grapple above the room's Maw at origin X 936, which reaches for her and grabs her by its own AI;
    /// its hold drops her grapple through the deferred drop, and the room runs the Maw's retraction and held delay until it
    /// releases her, standing, with her input restored.
    /// </summary>
    internal static void VerifyYappingMawGrappleRelease()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual((ushort)0xc8c5, bus.ReadCartridgeWord(0x90f15b), "pinned cartridge command three selects Dropped");
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.RedBrinstarFirefleas, camera: new Position16<RoomPixels>(768, 0));
        var samus = runtime.Player.Samus;
        samus.PoseId = SamusPoseId.GrappleSwingLeftPose;
        samus.Position = samus.Position.WithWhole(new(936, 172));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.ChangePose(samus.Pose, samus.PoseFrameNumbered(24));
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, samus.PoseFrameNumbered(24), timer: 3);
        samus.Grapple.Phase = GrapplePhase.ConnectedSwinging;
        // She swings from a point in the room, an eighth of a turn out from hanging straight down, as a connected swing does:
        // swinging there she passes through the Maw's reach, outside the distance it holds back at.
        samus.Grapple.Anchor = new Position16<RoomPixels>(936, 120);
        samus.Grapple.Angle = RotationAngle.HalfTurn + RotationAngle.QuarterTurn.DividedBy(2);
        samus.Grapple.MirroredAngle = samus.Grapple.Angle;
        samus.EquippedItems |= SamusEquipmentFlags.GrappleBeam;
        samus.SelectedHudItem = HudItem.Grapple;
        // Samus keeps her grapple, swinging, in the reach of the Maw whose mouth is placed at (936,192); its own AI reaches
        // for her, and its touch grabs her, locking her out.
        while (!samus.Control.InputLocked)
            runtime.StepFrame(ControllerButton.Left | ControllerButton.X);
        AssertTrue(samus.Grapple.Phase is GrapplePhase.ConnectedSwinging or GrapplePhase.Dropped, "the Maw grabs Samus while she swings on her grapple");
        // The grapple handler runs in Samus's alpha pass, before EnemyMain. The Maw's hold therefore queues $9B:C8C5 after
        // a swing, and the drop completes in the next update's handler.
        while (samus.Grapple.Phase != GrapplePhase.Dropped)
            runtime.StepFrame(ControllerButton.Left | ControllerButton.X);
        AssertTrue(samus.Control.InputLocked, "the Maw's hold queues the C8C5 drop while it holds her");
        runtime.StepFrame((ControllerButton.B | ControllerButton.Left | ControllerButton.R));
        SamusPoseId heldPose = samus.Pose;
        var heldMovement = runtime.LatestGameplayFrame?.GrappleMovement;
        Console.WriteLine($"Maw drop frame: pose=${(int)SamusPoseNumbers.Number(heldPose):X2}, grapple={samus.Grapple.Phase}, locked={samus.Control.InputLocked}.");
        AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "the next grapple handler completes the queued drop");
        AssertEqual(SamusPoseId.FacingLeftNormalPose, heldPose, "the hold drops her swing to the native standing pose");
        // Only the `$9B:C8C5` drop completion publishes a deferred drop pose.
        AssertTrue(heldMovement is { Phase: GrapplePhase.Inactive, PendingDropPose: SamusPoseId.FacingLeftNormalPose }, "Maw uses C8C5 deferred drop, not C856 cancellation");
        // The Maw keeps her through its retraction and its whole held delay, then releases her.
        int heldUpdates = 0;
        for (; samus.Control.InputLocked; heldUpdates++)
            runtime.StepFrame((ControllerButton.B | ControllerButton.Left | ControllerButton.R));
        AssertTrue(heldUpdates >= YappingMawAttackDefinitions.RetractedHold, $"the Maw holds Samus through its held delay ({heldUpdates} updates) before the delay's end releases her");
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
