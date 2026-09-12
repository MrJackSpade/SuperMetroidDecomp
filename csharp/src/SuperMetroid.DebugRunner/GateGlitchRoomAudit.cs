using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Measures stationary launch windows without confusing gate crossing with switch activation.</summary>
internal static class GateGlitchRoomAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
        var original = runtime.LevelData!;
        var gate = runtime.Plms.PopulationSlots.Single(s => s.HeaderPointer == RoomPlmHeaders.DownwardGate);
        int gateX = gate.BlockIndex % original.WidthInBlocks * 16;
        int gateY = gate.BlockIndex / original.WidthInBlocks * 16;
        Console.WriteLine($"Kronic Boost {original.WidthInBlocks}x{original.HeightInBlocks}; gate={gate.BlockIndex:X4} pixel={gateX},{gateY}");
        Console.WriteLine("item,x,y,hitFrame,shotX,shotY");
        for (ushort item = 0; item <= 2; item++)
        {
            int successes = 0;
            for (int x = gateX + 16; x <= gateX + 40; x++)
            for (int y = gateY - 16; y <= gateY + 64; y++)
            {
                var level = new RoomLevelData(original.WidthInBlocks, original.HeightInBlocks,
                    original.ForegroundEntries.Span, original.BehaviorBytes.Span,
                    original.BackgroundEntries.Span, original.BlockDefinitions.Span);
                var samus = new SamusState { XPosition = (ushort)x, YPosition = (ushort)y,
                    PoseId = SamusPoseId.NormalJumpAimDiagonalUpLeftPose, SelectedHudItem = item,
                    Missiles = 10, MaxMissiles = 10, SuperMissiles = 10, MaxSuperMissiles = 10 };
                var plms = new RoomPlmSystem();
                var streamer = level.CreateBackgroundStreamer(0);
                plms.LoadRoomPopulation(bus, level, streamer, new SnesVram(),
                    runtime.ActiveRoom!.State.PlmPointer, new Bank80SystemState(),
                    runtime.ActiveRoom.AreaIndex, () => samus, () => false);
                for (int warm = 0; warm < 2; warm++) plms.Step(bus, level, streamer, 0, 0, 0);
                plms.TakeDownwardGateProjectileRequests();
                var shots = new SamusProjectileSystem();
                var bombs = new SamusBombProjectileSystem();
                for (int frame = 0; frame < 24; frame++)
                {
                    ushort input = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
                    bombs.StepFrame(bus, level, samus, input, input);
                    shots.StepFrame(bus, level, samus, input, input, 0,
                        (ushort)Math.Max(0, gateY - 96), bombs, roomPlms: plms);
                    bool hit = plms.PopulationSlots.Any(s => s.NativeSlotIndex == gate.NativeSlotIndex && s.LoopTimer != 0);
                    if (hit)
                    {
                        var shot = shots.Slots[0];
                        Console.WriteLine($"{item},{x},{y},{frame},{shot.XPosition},{shot.YPosition}");
                        successes++;
                        break;
                    }
                    plms.Step(bus, level, streamer, 0, (ushort)Math.Max(0, gateY - 96), 0);
                }
            }
            Console.WriteLine($"Result item={item}: {successes}/2025 stationary origins activate the switch.");
        }
        return 0;
    }
}
