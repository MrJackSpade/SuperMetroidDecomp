using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Room-local ceiling-shot search with authored terrain and live PLM timers.</summary>
internal static class CeilingWrapRoomAudit
{
    public static int RunRuntime(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.FrogSpeedway);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
        samus.XPosition = 1237; samus.YPosition = 139;
        samus.EquippedBeams = 5;
        samus.EquippedItems = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(1109, 0);
        Console.WriteLine("frame,x,subx,y,pose,slots,camera");
        for (int frame = 0; frame < 900; frame++)
        {
            runtime.StepFrame((ushort)(SnesButton.X | SnesButton.B | SnesButton.Left | SnesButton.R));
            Console.WriteLine($"{frame},{samus.XPosition},{samus.Kinematics.XSubposition},{samus.YPosition},{samus.Pose:X2},{runtime.Plms.ActiveCount},{runtime.Camera.XPosition}");
            if (samus.XPosition < 800) break;
        }
        return 0;
    }

    public static int Run(string rom, ushort beams = 5)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.FrogSpeedway);
        var original = runtime.LevelData!;
        Console.WriteLine($"Frog Speedway {original.WidthInBlocks}x{original.HeightInBlocks}");
        for (int i = 0; i < original.ForegroundEntries.Length; i++)
        {
            var block = original.GetCollisionBlockByIndex(i);
            if (block.CollisionType == RoomCollisionType.SpecialBlock)
                Console.WriteLine($"Special {i:X4} xy={i % original.WidthInBlocks},{i / original.WidthInBlocks} word={block.LevelWord:X4} bts={block.Behavior:X2}");
        }
        for (int row = 0; row < original.HeightInBlocks; row++)
            Console.WriteLine($"Right column row={row} word={original.GetCollisionBlock(77, row).LevelWord:X4}");
        for (int x = 1237; x <= 1285; x++)
        for (int y = 139; y <= 139; y++)
        {
            var level = new RoomLevelData(original.WidthInBlocks, original.HeightInBlocks,
                original.ForegroundEntries.Span, original.BehaviorBytes.Span,
                original.BackgroundEntries.Span, original.BlockDefinitions.Span);
            var samus = new SamusState { XPosition = (ushort)x, YPosition = (ushort)y,
                PoseId = SamusPoseId.RunningAimDiagonalUpLeftPose, EquippedBeams = beams };
            samus.Kinematics.XRadius = 5;
            samus.Kinematics.YRadius = 21;
            var plms = new RoomPlmSystem();
            var streamer = level.CreateBackgroundStreamer(0);
            plms.LoadRoomPopulation(bus, level, streamer, new SnesVram(),
                runtime.ActiveRoom!.State.PlmPointer, new Bank80SystemState(),
                runtime.ActiveRoom.AreaIndex, () => samus, () => false);
            var shots = new SamusProjectileSystem();
            var bombs = new SamusBombProjectileSystem();
            int peak = plms.ActiveCount;
            for (int frame = 0; frame < 420; frame++)
            {
                ushort held = (ushort)(SnesButton.X | SnesButton.B | SnesButton.Left | SnesButton.R);
                ushort pressed = frame == 0 ? held : (ushort)0;
                SamusGroundedMovement.StepRunningLeft(bus, level, samus, (ushort)frame, held, plms);
                bombs.StepFrame(bus, level, samus, held, pressed);
                shots.StepFrame(bus, level, samus, held, pressed, (ushort)(x - 128), 0, bombs, roomPlms: plms);
                plms.Step(bus, level, streamer, (ushort)(x - 128), 0, 0);
                peak = Math.Max(peak, plms.ActiveCount);
                if (peak == 40)
                {
                    Console.WriteLine($"Full: origin={x},{y} frame={frame} slots={plms.ActiveCount}");
                    samus.Kinematics.XRadius = 5;
                    samus.Kinematics.YRadius = 21;
                    var contact = SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, -65536, plms: plms);
                    Console.WriteLine($"Full-pool left contact: collided={contact.Collided} x={samus.XPosition} slots={plms.ActiveCount}");
                    samus.PoseId = SamusPoseId.RunningAimDiagonalUpLeftPose;
                    ushort runningInput = (ushort)(SnesButton.X | SnesButton.B | SnesButton.Left | SnesButton.R);
                    for (int travel = 0; travel < 240; travel++)
                    {
                        var moved = SamusGroundedMovement.StepRunningLeft(bus, level, samus, (ushort)(frame + travel + 1), runningInput, plms);
                        bombs.StepFrame(bus, level, samus, runningInput, 0);
                        shots.StepFrame(bus, level, samus, runningInput, 0, (ushort)(samus.XPosition - 128), 0, bombs, roomPlms: plms);
                        plms.Step(bus, level, streamer, (ushort)(samus.XPosition - 128), 0, 0);
                        if (moved.Horizontal.Collided || samus.XPosition < 800)
                        {
                            Console.WriteLine($"Traversal: frame={frame + travel + 1} x={samus.XPosition} y={samus.YPosition} blocked={moved.Horizontal.Collided} slots={plms.ActiveCount}");
                            break;
                        }
                    }
                    return 0;
                }
            }
            if (peak > 10) Console.WriteLine($"Peak: origin={x},{y} slots={peak}");
        }
        Console.WriteLine("No full-pool candidate in bounded search.");
        return 0;
    }
}
