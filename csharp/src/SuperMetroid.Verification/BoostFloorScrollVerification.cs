using static SuiteTimingVerification;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

/// <summary>Checks the camera after Samus breaks through the Dachora speed-boost floor.</summary>
internal static class BoostFloorScrollVerification
{
    /// <summary>Verifies the opened shaft stays scrollable, Samus remains in view, and both reach the retail bottom alignment.</summary>
    internal static void VerifyBoostFloorScroll()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.BrinstarRoom08, camera: new Position16<RoomPixels>(1024, 0));
        var level = runtime.Room.LevelData;
        var samus = runtime.Player.IntroducedSamus;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
        samus.Position = samus.Position.WithWhole(new(1040, 171));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        // Seed an already established boost at the reported floor, avoiding the
        // unrelated bomb-tunnel approach. All subsequent movement/contacts are live.
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.HorizontalSpeed.BaseSpeed = 2;
        samus.HorizontalSpeed.ExtraRunSpeed = 7;
        samus.HorizontalSpeed.SpeedBoostCounter = SamusMovementRomData.HorizontalMotion.ActiveSpeedBoostStage | 1;
        for (int tick = 0; tick < 420; tick++)
        {
            runtime.StepFrame((ushort)((ushort)SnesButton.Right | runtime.Options.ControllerBindings.Dash | runtime.Options.ControllerBindings.Shoot));
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
        runtime.NmiHandler.RunNmi(0, true);
        var frame = GameplayDisplayCapture.CaptureFrame(runtime);

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
