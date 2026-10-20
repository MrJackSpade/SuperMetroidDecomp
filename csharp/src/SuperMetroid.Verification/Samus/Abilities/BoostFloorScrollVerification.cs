using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the camera after Samus breaks through the Dachora speed-boost floor.</summary>
internal static class BoostFloorScrollVerification
{
    /// <summary>Verifies the opened shaft stays scrollable, Samus remains in view, and both reach the retail bottom alignment.</summary>
    internal static void VerifyBoostFloorScroll()
    {
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Dachora, camera: new Position16<RoomPixels>(1024, 0));
        var samus = runtime.Player.IntroducedSamus;
        samus.Control.Unlock();
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.EquippedItems = SamusEquipmentFlags.SpeedBooster;
        samus.Position = samus.Position.WithWhole(new(1040, 171));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // Seed an already established boost at the reported floor, avoiding the
        // unrelated bomb-tunnel approach. All subsequent movement/contacts are live.
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(2);
        samus.HorizontalSpeed.ExtraRunSpeed = samus.HorizontalSpeed.ExtraRunSpeed.WithWhole(7);
        samus.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(SamusSpeedBoostCounterFixtures.ActiveStageWord | 1);
        for (int tick = 0; tick < 420; tick++)
        {
            runtime.StepFrame((SnesButton.Right | runtime.Options.ControllerBindings.Dash | runtime.Options.ControllerBindings.Shoot));
            if (tick >= 40)
            {
                AssertEqual(RoomScrollState.Green, runtime.Room.LoadedCamera.Scrolls.ReadNativeState(11),
                    "lower trigger keeps shaft unlocked after torso leaves upper trigger");
                int screenY = samus.Position.Y.Whole - runtime.Room.LoadedCamera.PixelPosition.Y;
                AssertTrue(screenY is >= 32 and < 224, "falling Samus remains within the viewport without byte wrapping");
            }
            if (tick % 100 == 0)
                Console.WriteLine($"frame={tick} Samus={samus.Position.X.Whole}/{samus.Position.Y.Whole} camera={runtime.Room.LoadedCamera.PixelPosition.X}/{runtime.Room.LoadedCamera.PixelPosition.Y}");
        }
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        GameplayDisplayCapture.Capture(runtime.PublishDisplayFrame());

        AssertTrue(samus.Position.Y.Whole > 1000, "fixture breaks through the actual speed floor and falls down the shaft");
        AssertTrue(runtime.Room.LoadedCamera.PixelPosition.Y > 900,
            "#374: camera must follow the opened shaft instead of leaving falling Samus more than a screen below it");
        AssertEqual((ushort)1707, samus.Position.Y.Whole, "descent lands on the retail bottom floor");
        AssertEqual((ushort)1567, runtime.Room.LoadedCamera.PixelPosition.Y, "camera reaches the bottom blue-scroll alignment");
        Console.WriteLine($"Dachora floor descent completed: SamusY={samus.Position.Y.Whole}, cameraY={runtime.Room.LoadedCamera.PixelPosition.Y}.");
    }

    /// <summary>Runs the Dachora floor descent and camera scroll regression.</summary>
    [VerificationCommand("--boost-floor-scroll")]
    internal static int BoostFloorScrollCommand()
    {
        Suite(nameof(VerifyBoostFloorScroll), () => VerifyBoostFloorScroll());
        return 0;
    }
}
