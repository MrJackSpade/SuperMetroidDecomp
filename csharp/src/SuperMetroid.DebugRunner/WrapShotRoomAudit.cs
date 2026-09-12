using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Inspects actual room targets before choosing a local wrap-shot fixture.</summary>
internal static class WrapShotRoomAudit
{
    public static int Run(string rom)
    {
        foreach (ushort pointer in new[] { RoomHeaderPointers.LandingSite, RoomHeaderPointers.Crocomire, RoomHeaderPointers.GreenBrinstarMainShaft })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(pointer);
            var level = runtime.LevelData!;
            Console.WriteLine($"Room {pointer:X4}: {level.WidthInBlocks}x{level.HeightInBlocks}");
            for (int y = 0; y < level.HeightInBlocks; y++)
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                var block = level.GetCollisionBlock(x, y);
                if (block.CollisionType == RoomCollisionType.ShootableBlock && block.Bts.TryGetBlueDoorOrientation(out var orientation))
                    Console.WriteLine($"Blue door {orientation}: block={block.Index:X4} xy={x},{y} word={block.LevelWord:X4} bts={block.Bts.Value:X2}");
            }
            int target = pointer == RoomHeaderPointers.LandingSite ? 0x1561 : pointer == RoomHeaderPointers.Crocomire ? 0x0301 : 0x2981;
            bool left = pointer == RoomHeaderPointers.GreenBrinstarMainShaft;
            int sourceY = (target / level.WidthInBlocks - (left ? 64 : 1)) * 16;
            bool found = false;
            foreach (ushort beams in new ushort[] { 5, 9, 1 })
            {
                for (int edgeDistance = 16; edgeDistance <= 48 && !found; edgeDistance += 2)
                for (int y = Math.Max(32, sourceY - 48); y <= sourceY + 32 && !found; y += 2)
                {
                    var trial = new RoomLevelData(level.WidthInBlocks, level.HeightInBlocks, level.ForegroundEntries.Span,
                        level.BehaviorBytes.Span, level.BackgroundEntries.Span, level.BlockDefinitions.Span);
                    var samus = new SamusState { XPosition = (ushort)(left ? edgeDistance : level.WidthInBlocks * 16 - edgeDistance),
                        YPosition = (ushort)y, PoseId = left ? SamusPoseId.StandingAimDiagonalDownLeftPose : SamusPoseId.StandingAimDiagonalDownRightPose,
                        EquippedBeams = beams };
                    var shots = new SamusProjectileSystem();
                    var bombs = new SamusBombProjectileSystem();
                    var plms = new RoomPlmSystem();
                    plms.LoadRoomPopulation(bus, trial, trial.CreateBackgroundStreamer(0), new SnesVram(),
                        runtime.ActiveRoom!.State.PlmPointer, new Bank80SystemState(), runtime.ActiveRoom.AreaIndex,
                        () => samus, () => false);
                    for (int frame = 0; frame < 40; frame++)
                    {
                        ushort input = frame == 0 ? (ushort)0x40 : (ushort)0;
                        bombs.StepFrame(bus, trial, samus, input, input);
                        shots.StepFrame(bus, trial, samus, input, input,
                            (ushort)(left ? 0 : level.WidthInBlocks * 16 - 256), (ushort)Math.Max(0, y - 128), bombs, roomPlms: plms);
                        if (plms.PopulationSlots.Any(slot => slot.BlockIndex == target && slot.HeaderPointer == RoomPlmHeaders.BlueDoorFacingRight))
                        {
                            Console.WriteLine($"Candidate room={pointer:X4} beams={beams:X4} origin={samus.XPosition},{y} frame={frame} shot={shots.Slots[0].XPosition},{shots.Slots[0].YPosition} target={target:X4}");
                            found = true;
                            break;
                        }
                    }
                }
                if (found) break;
            }
            if (!found) Console.WriteLine($"No candidate for {pointer:X4} in this bounded input/setup search.");
        }
        return 0;
    }
}
