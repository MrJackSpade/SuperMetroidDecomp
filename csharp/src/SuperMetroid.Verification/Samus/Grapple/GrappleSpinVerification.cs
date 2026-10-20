using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Grapple;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks Fire input consumed during the spin-to-grapple pose handoff.</summary>
internal static class GrappleSpinVerification
{
    /// <summary>Confirms both facings leave spin before the grapple fires, then held Fire extends while a tap requests cancellation.</summary>
    internal static void VerifyGrappleSpinInput()
    {
        foreach (bool left in new[] { false, true })
        foreach (bool held in new[] { false, true })
        {
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.WestOcean);
            var samus = runtime.Player.IntroducedSamus;
            samus.Control.Unlock();
            samus.Position = samus.Position.WithWhole(new(512, 256));
            samus.Pose = left ? SamusPoseId.SpinJumpLeftPose : SamusPoseId.SpinJumpRightPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.SelectedHudItem = HudItem.Grapple;
            samus.EquippedItems |= SamusEquipmentFlags.GrappleBeam;
            samus.Kinematics.YDirection = SamusVerticalDirection.Down;
            runtime.StepFrame(SnesButton.X);
            Console.WriteLine($"Spin fire first frame: pose={(int)samus.Pose:X2}, grapple={samus.Grapple.Phase}");
            AssertEqual(left ? SamusPoseId.NormalJumpGunExtendedLeftPose : SamusPoseId.NormalJumpGunExtendedRightPose,
                samus.Pose, "Fire exits the spin through the ROM pose table");
            AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "spin HUD handler does not fire before pose transition");
            runtime.StepFrame(held ? SnesButton.X : 0);
            AssertEqual(GrapplePhase.Firing, samus.Grapple.Phase, "preserved Fire edge starts grapple on the following frame");
            runtime.StepFrame(held ? SnesButton.X : 0);
            AssertEqual(held ? GrapplePhase.Firing : GrapplePhase.CancelPending, samus.Grapple.Phase,
                "held grapple extends; one-frame tap cancels on the next firing pass");
        }
    }

    /// <summary>Runs the focused grapple Fire-edge and spin-pose transition check.</summary>
    [VerificationCommand("--grapple-spin")]
    internal static int GrappleSpinCommand()
    {
        Suite(nameof(VerifyGrappleSpinInput), () => VerifyGrappleSpinInput());
        return 0;
    }
}
