using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyBoostFloorScroll()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.BrinstarRoom08, cameraX: 1024);
        var level = runtime.LevelData!;
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.MovingRightNormalPose;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
        samus.XPosition = 1040;
        samus.YPosition = 171;
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
                int screenY = samus.YPosition - runtime.Camera.YPosition;
                AssertTrue(screenY is >= 32 and < 224, "falling Samus remains within the viewport without byte wrapping");
            }
            if (tick % 100 == 0)
                Console.WriteLine($"frame={tick} Samus={samus.XPosition}/{samus.YPosition} camera={runtime.Camera!.XPosition}/{runtime.Camera.YPosition}");
        }
        runtime.RunNmi(0, true);
        var frame = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        Directory.CreateDirectory("csharp/test-temp/issue-374-floor");
        PngWriter.WriteRgba("csharp/test-temp/issue-374-floor/entry.png", 256, 224,
            SoftwareLayeredSnapshotRenderer.Render(frame));
        AssertTrue(samus.YPosition > 1000, "fixture breaks through the actual speed floor and falls down the shaft");
        AssertTrue(runtime.Camera!.YPosition > 900,
            "#374: camera must follow the opened shaft instead of leaving falling Samus more than a screen below it");
        AssertEqual((ushort)1707, samus.YPosition, "descent lands on the retail bottom floor");
        AssertEqual((ushort)1567, runtime.Camera.YPosition, "camera reaches the bottom blue-scroll alignment");
        Console.WriteLine($"Dachora floor descent completed: SamusY={samus.YPosition}, cameraY={runtime.Camera.YPosition}.");
    }
}
