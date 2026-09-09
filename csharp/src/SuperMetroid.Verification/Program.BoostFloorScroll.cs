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
        runtime.LoadCartridgeRoomForDebug(0x9cb3, cameraX: 1024);
        var level = runtime.LevelData!;
        Console.WriteLine($"Room {runtime.ActiveRoom!.Identity}: {level.WidthInBlocks}x{level.HeightInBlocks} blocks");
        for (int y = 0; y < level.HeightInBlocks; y++)
        for (int x = 0; x < level.WidthInBlocks; x++)
        {
            var block = level.GetCollisionBlock(x, y);
            if (block.Behavior == RoomBlockBehaviorValues.ScrollTrigger.Value || block.CollisionType == RoomCollisionType.SpecialBlock)
                Console.WriteLine($"candidate block {x}/{y}: {block.CollisionType}/{block.Behavior}");
        }
        var samus = runtime.Samus!;
        foreach (var scroll in runtime.Plms.ScrollPlms)
            Console.WriteLine($"scroll owner={scroll.BlockIndex} data={scroll.DataPointer:X4} bytes={string.Join(',', Enumerable.Range(0, 15).Select(i => bus.ReadByte(0x8f0000 | (scroll.DataPointer + i)).ToString("X2")))}");
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
        for (int tick = 0; tick < 240; tick++)
        {
            runtime.StepFrame((ushort)((ushort)SnesButton.Right | runtime.ControllerBindings.Dash | runtime.ControllerBindings.Shoot));
            if (tick % 20 == 0 || tick is >= 12 and < 27)
                Console.WriteLine($"frame={tick} Samus={samus.XPosition}/{samus.YPosition} camera={runtime.Camera!.XPosition}/{runtime.Camera.YPosition} boost={samus.HorizontalSpeed.SpeedBoostCounter:X4} trigger={level.GetCollisionBlock(74,13).LevelWord:X4}/{level.GetCollisionBlock(74,13).Behavior:X2} live={string.Join(',', runtime.Plms.ScrollPlms.Select(p => p.Triggered))} scroll11={runtime.Camera.Scrolls.Storage[11]}");
        }
        runtime.RunNmi(0, true);
        var frame = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
        Directory.CreateDirectory("csharp/test-temp/issue-374-floor");
        PngWriter.WriteRgba("csharp/test-temp/issue-374-floor/entry.png", 256, 224,
            SoftwareLayeredSnapshotRenderer.Render(frame));
        AssertTrue(samus.YPosition > 1000, "fixture breaks through the actual speed floor and falls down the shaft");
        AssertTrue(runtime.Camera!.YPosition > 900,
            "#374: camera must follow the opened shaft instead of leaving falling Samus more than a screen below it");
    }
}
