using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyYappingMawGrappleRelease()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual((ushort)0xc8c5, (ushort)(bus.ReadCartridgeByte(0x90f15b) | bus.ReadCartridgeByte(0x90f15c) << 8), "pinned cartridge command three selects Dropped");
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xa293, cameraX: 768);
        var samus = runtime.Samus!;
        samus.PoseId = SamusPoseId.GrappleSwingLeftPose;
        samus.XPosition = 936;
        samus.YPosition = 172;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 24);
        samus.SetPoseAndAnimationFromScriptedController(bus, samus.Pose, 24, 3, refreshRadius: false);
        samus.Grapple.Phase = GrapplePhase.ConnectedSwinging;
        samus.Grapple.CancelFromConnectedPose = false;
        samus.EquippedItems |= (ushort)SamusEquipmentFlags.GrappleBeam;
        samus.SelectedHudItem = 4;
        samus.InputLocked = true;
        // Stage the last two frames of the actual Maw's held delay. The report's
        // (936,172) is this population mouth (936,192) plus native held offset (0,-20).
        var maw = runtime.Enemies.YappingMawStates.Single(state => state?.OriginX == 936)!;
        maw.Function = YappingMawAiFunction.RetractedDelay;
        maw.HasGrabbedSamus = true;
        maw.RetractedDelayTimer = 1;
        maw.HeldSamusXOffset = 0;
        maw.HeldSamusYOffset = unchecked((ushort)-20);
        runtime.StepFrame(0x0250);
        AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "drop completes grapple cleanup while held");
        byte heldPose = samus.Pose;
        var heldMovement = runtime.LastGrappleMovement;
        Console.WriteLine($"Maw held frame: pose=${heldPose:X2}, grapple={samus.Grapple.Phase}, locked={samus.InputLocked}.");
        AssertTrue(samus.InputLocked && maw.HasGrabbedSamus, "Maw retains input until native delay underflows");
        runtime.StepFrame(0x8210);
        AssertEqual(SamusPoseIds.FacingLeftNormalPose, heldPose, "command three drops swing to native standing pose while held");
        // Only the `$9B:C8C5` drop completion publishes a deferred drop pose.
        AssertTrue(heldMovement is { Phase: GrapplePhase.Inactive, PendingDropPose: SamusPoseIds.FacingLeftNormalPose }, "Maw uses C8C5 deferred drop, not C856 cancellation");
        AssertTrue(!samus.InputLocked && !maw.HasGrabbedSamus, "native timer expiry releases player control");
        AssertTrue(samus.ReadMovementType(bus) != SamusMovementType.Grappling, "released Samus has an ordinary movement body");
        AssertTrue(runtime.LastGrappleMovement is { Fired: true }, "released control accepts the retained shoot edge as a new grapple");
        AssertEqual((ushort)936, samus.XPosition, "Maw release retains native held X");
        Console.WriteLine($"  Yapping Maw grapple release: pose=${samus.Pose:X2}, position={samus.XPosition}/{samus.YPosition}, input unlocked.");
    }
}