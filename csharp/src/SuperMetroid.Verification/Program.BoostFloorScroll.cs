using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using static SuiteTimingVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using static RetailRuntimeFixtureVerification;

internal static class BoostFloorScrollVerification
{
    internal static void VerifyBoostFloorScroll()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.BrinstarRoom08, camera: new Position16<RoomPixels>(1024, 0));
        var level = runtime.LevelData!;
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
        samus.Position = samus.Position.WithWhole(new(1040, 171));
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        // Seed an already established boost at the reported floor, avoiding the
        // unrelated bomb-tunnel approach. All subsequent movement/contacts are live.
        samus.HorizontalSpeed.HasRunningMomentum = true;
        samus.HorizontalSpeed.BaseSpeed = 2;
        samus.HorizontalSpeed.ExtraRunSpeed = 7;
        samus.HorizontalSpeed.SpeedBoostCounter = SamusMovementRomData.HorizontalMotion.ActiveSpeedBoostStage | 1;
        for (int tick = 0; tick < 420; tick++)
        {
            runtime.StepFrame((ushort)((ushort)SnesButton.Right | runtime.ControllerBindings.Dash | runtime.ControllerBindings.Shoot));
            if (tick >= 40)
            {
                AssertEqual(RoomScrollState.Green, runtime.Camera!.Scrolls.ReadNativeState(11),
                    "lower trigger keeps shaft unlocked after torso leaves upper trigger");
                int screenY = samus.Position.Y.Whole - runtime.Camera.PixelPosition.Y;
                AssertTrue(screenY is >= 32 and < 224, "falling Samus remains within the viewport without byte wrapping");
            }
            if (tick % 100 == 0)
                Console.WriteLine($"frame={tick} Samus={samus.Position.X.Whole}/{samus.Position.Y.Whole} camera={runtime.Camera!.PixelPosition.X}/{runtime.Camera.PixelPosition.Y}");
        }
        runtime.RunNmi(0, true);
        var frame = GameplayDisplayCapture.TryCaptureFrame(runtime)!;

        AssertTrue(samus.Position.Y.Whole > 1000, "fixture breaks through the actual speed floor and falls down the shaft");
        AssertTrue(runtime.Camera!.PixelPosition.Y > 900,
            "#374: camera must follow the opened shaft instead of leaving falling Samus more than a screen below it");
        AssertEqual((ushort)1707, samus.Position.Y.Whole, "descent lands on the retail bottom floor");
        AssertEqual((ushort)1567, runtime.Camera.PixelPosition.Y, "camera reaches the bottom blue-scroll alignment");
        Console.WriteLine($"Dachora floor descent completed: SamusY={samus.Position.Y.Whole}, cameraY={runtime.Camera.PixelPosition.Y}.");
    }

    [VerificationCommand("--boost-floor-scroll")]
    internal static int BoostFloorScrollCommand()
    {
        Suite(nameof(VerifyBoostFloorScroll), () => VerifyBoostFloorScroll());
        return 0;
    }
}
