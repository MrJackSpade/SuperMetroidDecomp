using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Measures stationary launch windows without confusing gate crossing with switch activation.</summary>
internal static class GateGlitchRoomAudit
{
    public static int RunJump(string rom, int shootFrame, int aimFrame = 0, bool releaseJump = false)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.KronicBoost);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.PoseId = SamusPoseId.StandingAimDiagonalUpLeftPose;
        samus.XPosition = 140;
        samus.YPosition = 379;
        samus.EquippedItems = samus.EquippedBeams = 0;
        samus.SelectedHudItem = 1;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Camera!.SetPosition(0, 224);
        Console.WriteLine("frame,input,x,subx,y,pose,gateTimer,gateInstruction");
        for (int frame = -60; frame < 80; frame++)
        {
            ushort input = frame < 0 ? (ushort)0 : (ushort)(SnesButton.A | SnesButton.Left);
            if (aimFrame > 0 && frame == -1) input = (ushort)SnesButton.Left;
            if (frame >= aimFrame) input |= (ushort)SnesButton.R;
            if (releaseJump && frame >= aimFrame) input &= unchecked((ushort)~(ushort)SnesButton.A);
            if (frame == shootFrame) input |= (ushort)SnesButton.X;
            runtime.StepFrame(input);
            var gate = runtime.Plms.PopulationSlots.Single(s => s.HeaderPointer == RoomPlmHeaders.DownwardGate);
            Console.WriteLine($"{frame},{input:X4},{samus.XPosition},{samus.Kinematics.XSubposition},{samus.YPosition},{samus.Pose:X2},{gate.LoopTimer},{gate.InstructionPointer:X4}");
        }
        return 0;
    }

    public static int Run(string rom, byte? gateArgument = null, string? nativeTrace = null)
    {
        var actualRecords = new List<string>();
        ISnesAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        if (gateArgument.HasValue) bus = new GateVariantBus(bus, gateArgument.Value);
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
                        string record = $"{item},{x},{y},{frame},{shot.XPosition},{shot.YPosition}";
                        actualRecords.Add(record);
                        Console.WriteLine(record);
                        successes++;
                        break;
                    }
                    plms.Step(bus, level, streamer, 0, (ushort)Math.Max(0, gateY - 96), 0);
                }
            }
            Console.WriteLine($"Result item={item}: {successes}/2025 stationary origins activate the switch.");
        }
        if (nativeTrace is not null)
        {
            var expected = File.ReadLines(nativeTrace).Skip(1).ToArray();
            if (!expected.SequenceEqual(actualRecords))
                throw new InvalidDataException($"Gate variant {gateArgument}: native activation window differs.");
            Console.WriteLine($"PASS native gate variant {gateArgument}: all 6075 origins, {expected.Length} exact positive records.");
        }
        return 0;
    }

    /// <summary>Changes only the authored shot-block argument; all setup tables remain retail.</summary>
    private sealed class GateVariantBus : ISnesAddressSpace
    {
        private readonly ISnesAddressSpace _inner;
        private readonly int _argumentAddress;
        private readonly byte _argument;

        public GateVariantBus(ISnesAddressSpace inner, byte argument)
        {
            if (argument is not (0 or 2 or 8 or 10))
                throw new ArgumentOutOfRangeException(nameof(argument), "Use blue/green left/right table offsets 0,2,8,10.");
            _inner = inner;
            _argument = argument;
            // Kronic Boost's pinned room state: population word at state +20.
            int entry = 0x8f0000 | inner.ReadByte(0x8fae95) | inner.ReadByte(0x8fae96) << 8;
            while (true)
            {
                int header = inner.ReadByte(entry) | inner.ReadByte(entry + 1) << 8;
                if (header == 0) throw new InvalidDataException("Kronic shot-block actor missing.");
                if (header == RoomPlmHeaders.DownwardGateShotBlock) { _argumentAddress = entry + 4; break; }
                entry += 6;
            }
        }
        public byte ReadByte(int address) => address == _argumentAddress ? _argument : _inner.ReadByte(address);
        public void WriteByte(int address, byte value) => _inner.WriteByte(address, value);
    }
}
